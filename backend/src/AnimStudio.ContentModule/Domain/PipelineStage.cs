namespace AnimStudio.ContentModule.Domain;

/// <summary>
/// Represents the production pipeline steps an episode moves through.
/// </summary>
public enum PipelineStage
{
    Idle = 0,
    ScriptGeneration = 1,
    CharacterDesign = 2,
    LoraTraining = 3,
    StoryboardGeneration = 4,
    VoiceGeneration = 5,
    AnimationRendering = 6,
    FinalComposition = 7,
    QualityReview = 8,
    Completed = 9,
    Failed = -1,
}
