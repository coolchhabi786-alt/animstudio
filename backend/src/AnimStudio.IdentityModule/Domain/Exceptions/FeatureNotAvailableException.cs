namespace AnimStudio.IdentityModule.Domain.Exceptions;

/// <summary>
/// Thrown when a team on a lower tier attempts to use a feature restricted to a higher tier.
/// </summary>
public sealed class FeatureNotAvailableException : DomainException
{
    public string CurrentTier { get; }
    public string RequiredTier { get; }
    public string Feature { get; }

    public FeatureNotAvailableException(string feature, string currentTier, string requiredTier)
        : base($"Feature '{feature}' is not available on the '{currentTier}' plan. Upgrade to '{requiredTier}' or higher.",
               "FEATURE_NOT_AVAILABLE")
    {
        Feature = feature;
        CurrentTier = currentTier;
        RequiredTier = requiredTier;
    }
}
