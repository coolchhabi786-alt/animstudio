using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Queries.GetCharacter;

/// <summary>Returns a single character by ID, enforcing team ownership.</summary>
/// <param name="TeamId">Caller's team — used for BOLA check.</param>
/// <param name="CharacterId">Character to fetch.</param>
public sealed record GetCharacterQuery(Guid TeamId, Guid CharacterId) : IRequest<Result<CharacterDto>>;

/// <summary>Validates <see cref="GetCharacterQuery"/>.</summary>
public sealed class GetCharacterValidator : AbstractValidator<GetCharacterQuery>
{
    public GetCharacterValidator()
    {
        RuleFor(x => x.CharacterId).NotEmpty();
    }
}

/// <summary>Handles <see cref="GetCharacterQuery"/>.</summary>
public sealed class GetCharacterQueryHandler(
    ICharacterRepository characters) : IRequestHandler<GetCharacterQuery, Result<CharacterDto>>
{
    public async Task<Result<CharacterDto>> Handle(GetCharacterQuery query, CancellationToken ct)
    {
        var character = await characters.GetByIdAsync(query.CharacterId, ct);
        if (character is null)
            return Result<CharacterDto>.Failure("Character not found.", "NOT_FOUND");

        // BOLA — ensure the character belongs to the caller's team
        if (character.TeamId != query.TeamId)
            return Result<CharacterDto>.Failure("Character not found.", "NOT_FOUND");

        return Result<CharacterDto>.Success(character.ToDto());
    }
}
