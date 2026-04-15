using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.DeleteProject;

public sealed record DeleteProjectCommand(Guid Id, Guid RequestingUserId) : IRequest<Result<bool>>;

public sealed class DeleteProjectValidator : AbstractValidator<DeleteProjectCommand>
{
    public DeleteProjectValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RequestingUserId).NotEmpty();
    }
}

public sealed class DeleteProjectHandler(IProjectRepository projects)
    : IRequestHandler<DeleteProjectCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteProjectCommand cmd, CancellationToken ct)
    {
        var project = await projects.GetByIdAsync(cmd.Id, ct);
        if (project is null) return Result<bool>.Failure("Project not found", "NOT_FOUND");

        project.SoftDelete(cmd.RequestingUserId);
        await projects.UpdateAsync(project, ct);

        return Result<bool>.Success(true);
    }
}
