namespace AnimStudio.ContentModule.Domain;

public enum EpisodeStatus
{
    Idle            = 0,
    CharacterDesign = 1,
    LoraTraining    = 2,
    Script          = 3,
    Storyboard      = 4,
    Voice           = 5,
    Animation       = 6,
    PostProduction  = 7,
    Done            = 8,
    Failed          = -1,
}
