using AnimStudio.API.Hosted;
using AnimStudio.ContentModule.Application.Commands.AttachCharacter;
using AnimStudio.ContentModule.Application.Commands.CreateCharacter;
using AnimStudio.ContentModule.Application.Commands.DeleteCharacter;
using AnimStudio.ContentModule.Application.Commands.DetachCharacter;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Application.Queries.GetCharacter;
using AnimStudio.ContentModule.Application.Queries.GetCharacters;
using AnimStudio.ContentModule.Application.Queries.GetEpisodeCharacters;
using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.IdentityModule.Application.Interfaces;
using Asp.Versioning;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimStudio.API.Controllers;

/// <summary>
/// REST endpoints for the Character Studio — LoRA character management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "RequireTeamMember")]
public sealed class CharactersController(
    ISender mediator,
    ICurrentUserService currentUser,
    IBackgroundJobClient backgroundJobs,
    ICharacterRepository characters) : ControllerBase
{
    // ── POST /api/v1/characters ── Create + enqueue training ─────────────────

    /// <summary>Creates a new character and enqueues LoRA training.</summary>
    [HttpPost("api/v{version:apiVersion}/characters")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCharacterRequest req,
        CancellationToken ct)
    {
        var teamId = currentUser.GetCurrentTeamId();
        var result = await mediator.Send(
            new CreateCharacterCommand(teamId, req.Name, req.Description, req.StyleDna), ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "INSUFFICIENT_CREDITS")
                return Problem(result.Error, statusCode: StatusCodes.Status402PaymentRequired,
                    title: "Insufficient Credits");

            return BadRequest(new { error = result.Error, code = result.ErrorCode });
        }

        // Enqueue CharacterDesign dispatch to the Python worker via Service Bus
        backgroundJobs.Enqueue<CharacterTrainingHangfireProcessor>(
            x => x.DispatchCharacterDesignAsync(result.Value!.CharacterId, CancellationToken.None));

        return StatusCode(StatusCodes.Status202Accepted, result.Value);
    }

    // ── GET /api/v1/characters ── Team character library (paginated) ──────────

    /// <summary>Returns the team's character library.</summary>
    [HttpGet("api/v{version:apiVersion}/characters")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var teamId = currentUser.GetCurrentTeamId();
        var result = await mediator.Send(new GetCharactersQuery(teamId, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    // ── GET /api/v1/characters/{id} ── Single character + training status ─────

    /// <summary>Returns a single character including current training status.</summary>
    [HttpGet("api/v{version:apiVersion}/characters/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var teamId = currentUser.GetCurrentTeamId();
        var result = await mediator.Send(new GetCharacterQuery(teamId, id), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    // ── DELETE /api/v1/characters/{id} ── Soft-delete ─────────────────────────

    /// <summary>Soft-deletes a character. Blocked if used in any active episode.</summary>
    [HttpDelete("api/v{version:apiVersion}/characters/{id:guid}")]
    [Authorize(Policy = "RequireTeamEditor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var teamId = currentUser.GetCurrentTeamId();
        var userId = currentUser.GetCurrentUserId();
        var result = await mediator.Send(new DeleteCharacterCommand(teamId, userId, id), ct);

        return result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(new { error = result.Error }),
            "CHARACTER_IN_USE" => Conflict(new { error = result.Error, code = result.ErrorCode }),
            null when result.IsSuccess => NoContent(),
            _ => BadRequest(new { error = result.Error }),
        };
    }

    // ── POST /api/v1/episodes/{id}/characters ── Attach character ─────────────

    /// <summary>Attaches a Ready character to an episode.</summary>
    [HttpPost("api/v{version:apiVersion}/episodes/{episodeId:guid}/characters")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AttachToEpisode(
        Guid episodeId,
        [FromBody] AttachCharacterRequest req,
        CancellationToken ct)
    {
        var teamId = currentUser.GetCurrentTeamId();
        var result = await mediator.Send(new AttachCharacterCommand(teamId, episodeId, req.CharacterId), ct);

        return result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(new { error = result.Error }),
            null when result.IsSuccess => StatusCode(StatusCodes.Status201Created),
            _ => BadRequest(new { error = result.Error, code = result.ErrorCode }),
        };
    }

    // ── POST /api/v1/episodes/{id}/characters/approve-training ── Approve Draft chars ──

    /// <summary>
    /// Advances all Draft characters in an episode to TrainingQueued and dispatches
    /// design jobs. Called when the user approves script-identified characters.
    /// </summary>
    [HttpPost("api/v{version:apiVersion}/episodes/{episodeId:guid}/characters/approve-training")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveForTraining(Guid episodeId, CancellationToken ct)
    {
        var episodeChars = await characters.GetByEpisodeIdAsync(episodeId, ct);
        var draftChars = episodeChars
            .Where(c => c.TrainingStatus == TrainingStatus.Draft)
            .ToList();

        foreach (var character in draftChars)
        {
            character.AdvanceTraining(TrainingStatus.TrainingQueued, progressPercent: 0);
            await characters.UpdateAsync(character, ct);
            backgroundJobs.Enqueue<CharacterTrainingHangfireProcessor>(
                x => x.DispatchCharacterDesignAsync(character.Id, CancellationToken.None));
        }

        return Ok(new { approved = draftChars.Count });
    }

    // ── GET /api/v1/episodes/{id}/characters ── Episode character roster ──────

    /// <summary>Returns all characters attached to an episode.</summary>
    [HttpGet("api/v{version:apiVersion}/episodes/{episodeId:guid}/characters")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListEpisodeCharacters(Guid episodeId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetEpisodeCharactersQuery(episodeId), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    // ── DELETE /api/v1/episodes/{episodeId}/characters/{charId} ── Detach ─────

    /// <summary>Detaches a character from an episode.</summary>
    [HttpDelete("api/v{version:apiVersion}/episodes/{episodeId:guid}/characters/{charId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DetachFromEpisode(
        Guid episodeId,
        Guid charId,
        CancellationToken ct)
    {
        var teamId = currentUser.GetCurrentTeamId();
        var result = await mediator.Send(new DetachCharacterCommand(teamId, episodeId, charId), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return NoContent();
    }

    // ── POST /api/v1/characters/{id}/retry-training ── Resume failed training ──

    /// <summary>
    /// Retries character training. If the character has no dataset yet, re-runs the full
    /// CharacterDesign flow. If a dataset exists (imageUrl set) but LoRA failed, dispatches
    /// LoRA training only.
    /// </summary>
    [HttpPost("api/v{version:apiVersion}/characters/{id:guid}/retry-training")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RetryTraining(Guid id, CancellationToken ct)
    {
        var teamId = currentUser.GetCurrentTeamId();
        var character = await characters.GetByIdAsync(id, ct);
        if (character is null || character.TeamId != teamId)
            return NotFound(new { error = "Character not found." });

        if (character.TrainingStatus == TrainingStatus.Ready)
            return Conflict(new { error = "Character is already trained.", code = "ALREADY_READY" });

        if (character.TrainingStatus is TrainingStatus.PoseGeneration or TrainingStatus.Training or TrainingStatus.TrainingQueued)
            return Conflict(new { error = "Training is already in progress.", code = "TRAINING_IN_PROGRESS" });

        // If the dataset already exists (imageUrl set), skip dataset generation and go straight to LoRA.
        // If no dataset yet, run the full CharacterDesign flow to generate it first.
        bool hasDataset = character.ImageUrl is not null;

        if (hasDataset)
        {
            character.AdvanceTraining(TrainingStatus.TrainingQueued, progressPercent: 0);
            await characters.UpdateAsync(character, ct);
            backgroundJobs.Enqueue<CharacterTrainingHangfireProcessor>(
                x => x.DispatchLoraTrainingAsync(character.Id, CancellationToken.None));
        }
        else
        {
            character.AdvanceTraining(TrainingStatus.PoseGeneration, progressPercent: 0);
            await characters.UpdateAsync(character, ct);
            backgroundJobs.Enqueue<CharacterTrainingHangfireProcessor>(
                x => x.DispatchCharacterDesignAsync(character.Id, CancellationToken.None));
        }

        return Accepted(new { retried = true, stage = hasDataset ? "LoraTraining" : "CharacterDesign" });
    }

    // ── POST /api/v1/characters/{id}/regenerate-dataset ── Re-run pose generation ──

    /// <summary>
    /// Resets all training artefacts and re-dispatches the CharacterDesign (dataset pose
    /// generation) job. This costs credits. Blocked while training is actively running.
    /// </summary>
    [HttpPost("api/v{version:apiVersion}/characters/{id:guid}/regenerate-dataset")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegenerateDataset(Guid id, CancellationToken ct)
    {
        var teamId = currentUser.GetCurrentTeamId();
        var character = await characters.GetByIdAsync(id, ct);
        if (character is null || character.TeamId != teamId)
            return NotFound(new { error = "Character not found." });

        if (character.TrainingStatus is TrainingStatus.PoseGeneration or TrainingStatus.Training)
            return Conflict(new { error = "Training is already in progress.", code = "TRAINING_IN_PROGRESS" });

        character.ResetForDatasetRegeneration();
        await characters.UpdateAsync(character, ct);
        backgroundJobs.Enqueue<CharacterTrainingHangfireProcessor>(
            x => x.DispatchCharacterDesignAsync(character.Id, CancellationToken.None));

        return Accepted(new { regenerating = true, creditCost = character.CreditsCost });
    }

    // ── Request records ───────────────────────────────────────────────────────

    /// <summary>Request body for creating a character.</summary>
    public sealed record CreateCharacterRequest(string Name, string? Description, string? StyleDna);

    /// <summary>Request body for attaching a character to an episode.</summary>
    public sealed record AttachCharacterRequest(Guid CharacterId);
}
