namespace AnimStudio.IdentityModule.Domain.Exceptions;

/// <summary>Thrown when adding a member would exceed the plan's MaxTeamMembers limit.</summary>
public sealed class TeamMemberLimitExceededException : DomainException
{
    public int Limit { get; }

    public TeamMemberLimitExceededException(int limit)
        : base($"Team member limit of {limit} has been reached. Upgrade your plan to add more members.",
               "TEAM_MEMBER_LIMIT_EXCEEDED")
    {
        Limit = limit;
    }
}
