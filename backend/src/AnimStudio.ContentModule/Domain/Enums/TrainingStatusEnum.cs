namespace AnimStudio.ContentModule.Domain.Enums;

/// <summary>
/// LoRA training lifecycle states for a Character.
/// </summary>
public enum TrainingStatus
{
    /// <summary>Character created but training has not started.</summary>
    Draft,

    /// <summary>AI pipeline is generating reference pose images.</summary>
    PoseGeneration,

    /// <summary>Training job has been enqueued in Azure Service Bus — waiting for a GPU worker.</summary>
    TrainingQueued,

    /// <summary>GPU LoRA training is actively running.</summary>
    Training,

    /// <summary>LoRA weights are available; character is usable in Episodes.</summary>
    Ready,

    /// <summary>Training failed — the character can be retrained after investigation.</summary>
    Failed,
}
