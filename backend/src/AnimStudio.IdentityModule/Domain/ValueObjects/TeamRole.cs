namespace AnimStudio.IdentityModule.Domain.ValueObjects
{
    /// <summary>
    /// Represents the role a user can have within a team.
    /// </summary>
    public enum TeamRole
    {
        /// <summary>
        /// Indicates the user is the owner of the team.
        /// </summary>
        Owner,

        /// <summary>
        /// Indicates the user is an administrator of the team.
        /// </summary>
        Admin,

        /// <summary>
        /// Indicates the user is a regular member of the team.
        /// </summary>
        Member
    }
}