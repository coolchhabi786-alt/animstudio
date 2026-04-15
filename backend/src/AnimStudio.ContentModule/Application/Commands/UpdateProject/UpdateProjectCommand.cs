using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.UpdateProject;

public sealed record UpdateProjectCommand(Guid Id, Guid RequestingUserId, string Name, string Description, string? ThumbnailUrl) : IRequest<Result<ProjectDto>>;

public sealed class UpdateProjectValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public sealed class UpdateProjectHandler(IProjectRepository projects)
    : IRequestHandler<UpdateProjectCommand, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> Handle(UpdateProjectCommand cmd, CancellationToken ct)
    {
        var project = await projects.GetByIdAsync(cmd.Id, ct);
        if (project is null) return Result<ProjectDto>.Failure("Project not found", "NOT_FOUND");

        project.Update(cmd.Name, cmd.Description, cmd.ThumbnailUrl);
        await projects.UpdateAsync(project, ct);

        return Result<ProjectDto>.Success(project.ToDto());
    }
}
