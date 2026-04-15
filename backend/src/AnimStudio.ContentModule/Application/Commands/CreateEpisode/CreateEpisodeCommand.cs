using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.CreateEpisode;

public sealed record CreateEpisodeCommand(
    Guid ProjectId,
    string Name,
    string Idea,
    string Style,
    Guid? TemplateId = null) : IRequest<Result<EpisodeDto>>;

public sealed class CreateEpisodeValidator : AbstractValidator<CreateEpisodeCommand>
{
    public CreateEpisodeValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Idea).MaximumLength(5000);
        RuleFor(x => x.Style).MaximumLength(500);
    }
}

public sealed class CreateEpisodeHandler(IEpisodeRepository episodes, IProjectRepository projects)
    : IRequestHandler<CreateEpisodeCommand, Result<EpisodeDto>>
{
    public async Task<Result<EpisodeDto>> Handle(CreateEpisodeCommand cmd, CancellationToken ct)
    {
        var project = await projects.GetByIdAsync(cmd.ProjectId, ct);
        if (project is null) return Result<EpisodeDto>.Failure("Project not found", "NOT_FOUND");

        var episode = Episode.Create(cmd.ProjectId, cmd.Name, cmd.Idea, cmd.Style, cmd.TemplateId);
        await episodes.AddAsync(episode, ct);

        return Result<EpisodeDto>.Success(episode.ToDto());
    }
}
