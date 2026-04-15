using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.ContentModule.Domain.Entities
{
    /// <summary>
    /// Represents a project.
    /// </summary>
    [Table("Projects", Schema = "content")]
    public class Project
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Team")]
        public Guid TeamId { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(1024)]
        public string Description { get; set; }

        [Url]
        public string ThumbnailUrl { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedByUserId { get; set; }
    }

    /// <summary>
    /// Represents an episode attached to a project.
    /// </summary>
    [Table("Episodes", Schema = "content")]
    public class Episode
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Project")]
        public Guid ProjectId { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(1024)]
        public string Idea { get; set; }

        [MaxLength(255)]
        public string Style { get; set; }

        [Required]
        public EpisodeStatus Status { get; set; }

        public Guid? TemplateId { get; set; }

        public List<Guid> CharacterIds { get; set; } = new List<Guid>();

        public string DirectorNotes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        public DateTime? RenderedAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedByUserId { get; set; }
    }

    /// <summary>
    /// Represents a job within an episode execution.
    /// </summary>
    [Table("Jobs", Schema = "content")]
    public class Job
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Episode")]
        public Guid EpisodeId { get; set; }

        [Required]
        public JobType Type { get; set; }

        [Required]
        public JobStatus Status { get; set; }

        public string Payload { get; set; }

        public string Result { get; set; }

        public string ErrorMessage { get; set; }

        public DateTime? QueuedAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public int AttemptNumber { get; set; }
    }

    /// <summary>
    /// Represents the saga state of an episode's pipeline progress.
    /// </summary>
    [Table("SagaStates", Schema = "shared")]
    public class EpisodeSagaState
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Episode")]
        public Guid EpisodeId { get; set; }

        [Required]
        public PipelineStage CurrentStage { get; set; }

        [Required]
        public int RetryCount { get; set; }

        public string LastError { get; set; }

        [Required]
        public DateTime StartedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsCompensating { get; set; }
    }

    public enum EpisodeStatus
    {
        Idle,
        CharacterDesign,
        LoraTraining,
        Script,
        Storyboard,
        Voice,
        Animation,
        PostProduction,
        Done,
        Failed
    }

    public enum JobType
    {
        CharacterDesign,
        LoraTraining,
        Script,
        StoryboardPlan,
        StoryboardGen,
        Voice,
        Animation,
        PostProd
    }

    public enum JobStatus
    {
        Pending,
        Running,
        Completed,
        Failed
    }

    public enum PipelineStage
    {
        Idle,
        CharacterDesign,
        LoraTraining,
        Script,
        Storyboard,
        Voice,
        Animation,
        PostProduction,
        Done,
        Failed
    }
}