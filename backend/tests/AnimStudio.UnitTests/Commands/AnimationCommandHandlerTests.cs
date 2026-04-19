using Xunit;
using Moq;
using FluentAssertions;
using AnimStudio.ContentModule.Application.Commands.ApproveAnimation;
using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Application.Queries.GetAnimationClipSignedUrl;
using AnimStudio.ContentModule.Application.Queries.GetAnimationClips;
using AnimStudio.ContentModule.Application.Queries.GetAnimationEstimate;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;

namespace AnimStudio.UnitTests.Commands;

public class ApproveAnimationCommandHandlerTests
{
    private readonly Mock<IEpisodeRepository> _episodes = new();
    private readonly Mock<IAnimationJobRepository> _animationJobs = new();
    private readonly Mock<IAnimationClipRepository> _animationClips = new();
    private readonly Mock<IAnimationEstimateService> _estimator = new();
    private readonly Mock<IJobRepository> _jobs = new();
    private readonly ApproveAnimationHandler _handler;

    public ApproveAnimationCommandHandlerTests()
    {
        _handler = new ApproveAnimationHandler(
            _episodes.Object,
            _animationJobs.Object,
            _animationClips.Object,
            _estimator.Object,
            _jobs.Object);
    }

    private static ApproveAnimationCommand MakeCmd(Guid? episodeId = null) =>
        new(episodeId ?? Guid.NewGuid(), AnimationBackend.Kling, Guid.NewGuid());

    [Fact]
    public async Task Handle_EpisodeNotFound_ReturnsNotFound()
    {
        var cmd = MakeCmd();
        _episodes.Setup(r => r.GetByIdAsync(cmd.EpisodeId, default))
            .ReturnsAsync((Episode?)null);

        var result = await _handler.Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ActiveJobExists_ReturnsConflict()
    {
        var cmd = MakeCmd();
        _episodes.Setup(r => r.GetByIdAsync(cmd.EpisodeId, default))
            .ReturnsAsync(Episode.Create(Guid.NewGuid(), "Ep", "idea", "Pixar3D"));
        _animationJobs.Setup(r => r.HasActiveJobAsync(cmd.EpisodeId, default))
            .ReturnsAsync(true);

        var result = await _handler.Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("ANIMATION_ALREADY_ACTIVE");
    }

    [Fact]
    public async Task Handle_NoStoryboard_ReturnsStoryboardNotReady()
    {
        var cmd = MakeCmd();
        _episodes.Setup(r => r.GetByIdAsync(cmd.EpisodeId, default))
            .ReturnsAsync(Episode.Create(Guid.NewGuid(), "Ep", "idea", "Pixar3D"));
        _animationJobs.Setup(r => r.HasActiveJobAsync(cmd.EpisodeId, default))
            .ReturnsAsync(false);
        _estimator.Setup(s => s.EstimateAsync(cmd.EpisodeId, cmd.Backend, default))
            .ReturnsAsync((AnimationEstimateDto?)null);

        var result = await _handler.Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("STORYBOARD_NOT_READY");
    }

    [Fact]
    public async Task Handle_EmptyStoryboard_ReturnsStoryboardEmpty()
    {
        var cmd = MakeCmd();
        _episodes.Setup(r => r.GetByIdAsync(cmd.EpisodeId, default))
            .ReturnsAsync(Episode.Create(Guid.NewGuid(), "Ep", "idea", "Pixar3D"));
        _animationJobs.Setup(r => r.HasActiveJobAsync(cmd.EpisodeId, default))
            .ReturnsAsync(false);
        _estimator.Setup(s => s.EstimateAsync(cmd.EpisodeId, cmd.Backend, default))
            .ReturnsAsync(new AnimationEstimateDto(
                cmd.EpisodeId, cmd.Backend, 0, 0.056m, 0m,
                new List<AnimationEstimateLineItem>()));

        var result = await _handler.Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("STORYBOARD_EMPTY");
    }

    [Fact]
    public async Task Handle_Success_PersistsJobSeedsClipsAndEnqueuesWorkerJob()
    {
        var cmd = MakeCmd();
        var episode = Episode.Create(Guid.NewGuid(), "Ep", "idea", "Pixar3D");

        _episodes.Setup(r => r.GetByIdAsync(cmd.EpisodeId, default)).ReturnsAsync(episode);
        _animationJobs.Setup(r => r.HasActiveJobAsync(cmd.EpisodeId, default)).ReturnsAsync(false);

        var breakdown = new List<AnimationEstimateLineItem>
        {
            new(1, 0, Guid.NewGuid(), 0.056m),
            new(1, 1, Guid.NewGuid(), 0.056m),
            new(2, 0, Guid.NewGuid(), 0.056m),
        };
        _estimator.Setup(s => s.EstimateAsync(cmd.EpisodeId, cmd.Backend, default))
            .ReturnsAsync(new AnimationEstimateDto(
                cmd.EpisodeId, cmd.Backend, 3, 0.056m, 0.168m, breakdown));

        _jobs.Setup(r => r.GetByEpisodeIdAsync(cmd.EpisodeId, default))
            .ReturnsAsync(new List<Job>());

        var result = await _handler.Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Backend.Should().Be(AnimationBackend.Kling);
        result.Value!.EstimatedCostUsd.Should().Be(0.168m);
        result.Value!.Status.Should().Be(AnimationStatus.Approved);

        _animationJobs.Verify(r => r.AddAsync(
            It.Is<AnimationJob>(j =>
                j.EpisodeId == cmd.EpisodeId &&
                j.EstimatedCostUsd == 0.168m &&
                j.Status == AnimationStatus.Approved),
            default), Times.Once);

        _animationClips.Verify(r => r.AddRangeAsync(
            It.Is<IEnumerable<AnimationClip>>(cs => cs.Count() == 3),
            default), Times.Once);

        _jobs.Verify(r => r.AddAsync(
            It.Is<Job>(j => j.Type == JobType.Animation && j.EpisodeId == cmd.EpisodeId),
            default), Times.Once);

        episode.Status.Should().Be(EpisodeStatus.Animation);
        _episodes.Verify(r => r.UpdateAsync(episode, default), Times.Once);
    }

    [Fact]
    public async Task Handle_LocalBackend_UsesZeroCost()
    {
        var cmd = new ApproveAnimationCommand(Guid.NewGuid(), AnimationBackend.Local, Guid.NewGuid());
        var episode = Episode.Create(Guid.NewGuid(), "Ep", "idea", "Pixar3D");

        _episodes.Setup(r => r.GetByIdAsync(cmd.EpisodeId, default)).ReturnsAsync(episode);
        _animationJobs.Setup(r => r.HasActiveJobAsync(cmd.EpisodeId, default)).ReturnsAsync(false);
        _estimator.Setup(s => s.EstimateAsync(cmd.EpisodeId, AnimationBackend.Local, default))
            .ReturnsAsync(new AnimationEstimateDto(
                cmd.EpisodeId, AnimationBackend.Local, 2, 0m, 0m,
                new List<AnimationEstimateLineItem>
                {
                    new(1, 0, Guid.NewGuid(), 0m),
                    new(1, 1, Guid.NewGuid(), 0m),
                }));
        _jobs.Setup(r => r.GetByEpisodeIdAsync(cmd.EpisodeId, default))
            .ReturnsAsync(new List<Job>());

        var result = await _handler.Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.EstimatedCostUsd.Should().Be(0m);
        result.Value!.Backend.Should().Be(AnimationBackend.Local);
    }
}

public class GetAnimationEstimateQueryHandlerTests
{
    private readonly Mock<IEpisodeRepository> _episodes = new();
    private readonly Mock<IAnimationEstimateService> _estimator = new();
    private readonly GetAnimationEstimateHandler _handler;

    public GetAnimationEstimateQueryHandlerTests()
    {
        _handler = new GetAnimationEstimateHandler(_episodes.Object, _estimator.Object);
    }

    [Fact]
    public async Task Handle_EpisodeNotFound_ReturnsNotFound()
    {
        var episodeId = Guid.NewGuid();
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default)).ReturnsAsync((Episode?)null);

        var result = await _handler.Handle(
            new GetAnimationEstimateQuery(episodeId, AnimationBackend.Kling), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_NoStoryboard_ReturnsStoryboardNotFound()
    {
        var episodeId = Guid.NewGuid();
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default))
            .ReturnsAsync(Episode.Create(Guid.NewGuid(), "Ep", "idea", "Pixar3D"));
        _estimator.Setup(s => s.EstimateAsync(episodeId, AnimationBackend.Kling, default))
            .ReturnsAsync((AnimationEstimateDto?)null);

        var result = await _handler.Handle(
            new GetAnimationEstimateQuery(episodeId, AnimationBackend.Kling), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("STORYBOARD_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ReturnsEstimate()
    {
        var episodeId = Guid.NewGuid();
        var dto = new AnimationEstimateDto(
            episodeId, AnimationBackend.Kling, 2, 0.056m, 0.112m,
            new List<AnimationEstimateLineItem>
            {
                new(1, 0, Guid.NewGuid(), 0.056m),
                new(1, 1, Guid.NewGuid(), 0.056m),
            });

        _episodes.Setup(r => r.GetByIdAsync(episodeId, default))
            .ReturnsAsync(Episode.Create(Guid.NewGuid(), "Ep", "idea", "Pixar3D"));
        _estimator.Setup(s => s.EstimateAsync(episodeId, AnimationBackend.Kling, default))
            .ReturnsAsync(dto);

        var result = await _handler.Handle(
            new GetAnimationEstimateQuery(episodeId, AnimationBackend.Kling), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ShotCount.Should().Be(2);
        result.Value!.TotalCostUsd.Should().Be(0.112m);
    }
}

public class GetAnimationClipsQueryHandlerTests
{
    private readonly Mock<IEpisodeRepository> _episodes = new();
    private readonly Mock<IAnimationClipRepository> _clips = new();
    private readonly GetAnimationClipsHandler _handler;

    public GetAnimationClipsQueryHandlerTests()
    {
        _handler = new GetAnimationClipsHandler(_episodes.Object, _clips.Object);
    }

    [Fact]
    public async Task Handle_EpisodeNotFound_ReturnsNotFound()
    {
        var episodeId = Guid.NewGuid();
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default)).ReturnsAsync((Episode?)null);

        var result = await _handler.Handle(new GetAnimationClipsQuery(episodeId), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ReturnsClipsOrderedBySceneAndShot()
    {
        var episodeId = Guid.NewGuid();
        var clips = new List<AnimationClip>
        {
            AnimationClip.CreatePending(episodeId, 2, 0, null),
            AnimationClip.CreatePending(episodeId, 1, 1, null),
            AnimationClip.CreatePending(episodeId, 1, 0, null),
        };
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default))
            .ReturnsAsync(Episode.Create(Guid.NewGuid(), "Ep", "idea", "Pixar3D"));
        _clips.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
            .ReturnsAsync(clips);

        var result = await _handler.Handle(new GetAnimationClipsQuery(episodeId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(3);
        result.Value![0].SceneNumber.Should().Be(1);
        result.Value![0].ShotIndex.Should().Be(0);
        result.Value![2].SceneNumber.Should().Be(2);
    }
}

public class GetAnimationClipSignedUrlQueryHandlerTests
{
    private readonly Mock<IAnimationClipRepository> _clips = new();
    private readonly Mock<IClipUrlSigner> _signer = new();
    private readonly GetAnimationClipSignedUrlHandler _handler;

    public GetAnimationClipSignedUrlQueryHandlerTests()
    {
        _handler = new GetAnimationClipSignedUrlHandler(_clips.Object, _signer.Object);
    }

    [Fact]
    public async Task Handle_ClipMissing_ReturnsNotFound()
    {
        var episodeId = Guid.NewGuid();
        var clipId = Guid.NewGuid();
        _clips.Setup(r => r.GetByIdAsync(clipId, default))
            .ReturnsAsync((AnimationClip?)null);

        var result = await _handler.Handle(
            new GetAnimationClipSignedUrlQuery(episodeId, clipId), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ClipFromDifferentEpisode_ReturnsNotFound()
    {
        var episodeId = Guid.NewGuid();
        var clip = AnimationClip.CreatePending(Guid.NewGuid(), 1, 0, null);
        _clips.Setup(r => r.GetByIdAsync(clip.Id, default)).ReturnsAsync(clip);

        var result = await _handler.Handle(
            new GetAnimationClipSignedUrlQuery(episodeId, clip.Id), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ClipNotReady_ReturnsClipNotReady()
    {
        var episodeId = Guid.NewGuid();
        var clip = AnimationClip.CreatePending(episodeId, 1, 0, null);
        _clips.Setup(r => r.GetByIdAsync(clip.Id, default)).ReturnsAsync(clip);

        var result = await _handler.Handle(
            new GetAnimationClipSignedUrlQuery(episodeId, clip.Id), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CLIP_NOT_READY");
    }

    [Fact]
    public async Task Handle_ReadyClip_ReturnsSignedUrl()
    {
        var episodeId = Guid.NewGuid();
        var clip = AnimationClip.CreatePending(episodeId, 1, 0, null);
        clip.MarkRendering();
        clip.MarkReady("clips/ep/clip.mp4", 4.5);

        var expires = DateTimeOffset.UtcNow.AddSeconds(60);
        _clips.Setup(r => r.GetByIdAsync(clip.Id, default)).ReturnsAsync(clip);
        _signer.Setup(s => s.Sign(clip.ClipUrl!))
            .Returns(("https://cdn.local/clips/ep/clip.mp4?se=abc", expires));

        var result = await _handler.Handle(
            new GetAnimationClipSignedUrlQuery(episodeId, clip.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ClipId.Should().Be(clip.Id);
        result.Value!.Url.Should().StartWith("https://cdn.local/");
        result.Value!.ExpiresAt.Should().Be(expires);
    }
}

public class AnimationDomainTests
{
    [Fact]
    public void AnimationJob_Approve_InitialisesApprovedState()
    {
        var episodeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var job = AnimationJob.Approve(episodeId, AnimationBackend.Kling, 1.12m, userId);

        job.Status.Should().Be(AnimationStatus.Approved);
        job.EstimatedCostUsd.Should().Be(1.12m);
        job.ApprovedByUserId.Should().Be(userId);
        job.ApprovedAt.Should().NotBeNull();
        job.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "AnimationJobApprovedEvent");
    }

    [Fact]
    public void AnimationJob_Approve_ZeroCost_IsAllowedForLocalBackend()
    {
        var job = AnimationJob.Approve(Guid.NewGuid(), AnimationBackend.Local, 0m, Guid.NewGuid());
        job.Status.Should().Be(AnimationStatus.Approved);
        job.EstimatedCostUsd.Should().Be(0m);
    }

    [Fact]
    public void AnimationJob_Approve_NegativeCost_Throws()
    {
        var act = () => AnimationJob.Approve(Guid.NewGuid(), AnimationBackend.Kling, -1m, Guid.NewGuid());
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AnimationJob_MarkCompleted_FromApproved_Succeeds()
    {
        var job = AnimationJob.Approve(Guid.NewGuid(), AnimationBackend.Kling, 1m, Guid.NewGuid());
        job.MarkRunning();
        job.MarkCompleted(0.95m);

        job.Status.Should().Be(AnimationStatus.Completed);
        job.ActualCostUsd.Should().Be(0.95m);
    }

    [Fact]
    public void AnimationJob_MarkRunning_FromCompleted_Throws()
    {
        var job = AnimationJob.Approve(Guid.NewGuid(), AnimationBackend.Kling, 1m, Guid.NewGuid());
        job.MarkRunning();
        job.MarkCompleted(1m);

        var act = () => job.MarkRunning();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AnimationClip_MarkReady_PublishesClipReadyEvent()
    {
        var clip = AnimationClip.CreatePending(Guid.NewGuid(), 1, 0, null);
        clip.MarkRendering();
        clip.MarkReady("clips/ep/clip.mp4", 3.5);

        clip.Status.Should().Be(ClipStatus.Ready);
        clip.ClipUrl.Should().Be("clips/ep/clip.mp4");
        clip.DurationSeconds.Should().Be(3.5);
        clip.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "AnimationClipReadyEvent");
    }

    [Fact]
    public void AnimationClip_CreatePending_InvalidSceneNumber_Throws()
    {
        var act = () => AnimationClip.CreatePending(Guid.NewGuid(), 0, 0, null);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
