using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Queries.GetCharacters;

/// <summary>
/// Returns a paginated list of all characters owned by the specified team.
/// </summary>
/// <param name="TeamId">Team whose library to fetch — extracted from JWT by the controller.</param>
/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">Number of items per page (max 100).</param>
public sealed record GetCharactersQuery(Guid TeamId, int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedCharactersResponse>>;

/// <summary>Validates <see cref="GetCharactersQuery"/>.</summary>
public sealed class GetCharactersValidator : AbstractValidator<GetCharactersQuery>
{
    public GetCharactersValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

/// <summary>Handles <see cref="GetCharactersQuery"/>.</summary>
public sealed class GetCharactersQueryHandler(
    ICharacterRepository characters) : IRequestHandler<GetCharactersQuery, Result<PagedCharactersResponse>>
{
    public async Task<Result<PagedCharactersResponse>> Handle(GetCharactersQuery query, CancellationToken ct)
    {
        var (items, totalCount) = await characters.GetByTeamIdAsync(query.TeamId, query.Page, query.PageSize, ct);

        var dtos = items.Select(c => c.ToDto()).ToList();
        return Result<PagedCharactersResponse>.Success(
            new PagedCharactersResponse(dtos, totalCount, query.Page, query.PageSize));
    }
}
