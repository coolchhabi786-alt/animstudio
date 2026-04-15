export interface UserDto {
  id: string;
  externalId: string;
  email: string;
  displayName: string;
  avatarUrl?: string;
  createdAt: string;
}

export interface TeamDto {
  id: string;
  name: string;
}

export interface TeamMemberDto {
  userId: string;
  email: string;
  displayName: string;
  avatarUrl?: string;
  role: string;
  isAccepted: boolean;
  joinedAt: string;
}

export interface PlanDto {
  id: string;
  name: string;
  price: number;
  stripePriceId: string;
  episodesPerMonth: number;
  maxCharacters: number;
  maxTeamMembers: number;
  isActive: boolean;
  isDefault: boolean;
}

export interface SubscriptionDto {
  id: string;
  planName: string;
  status: string;
  episodesUsedThisMonth: number;
  episodesPerMonth: number;
  currentPeriodEnd?: string;
  trialEndsAt?: string;
  cancelAtPeriodEnd: boolean;
  stripeCustomerId: string;
}

/** Generic paginated list envelope returned by GET /api/v1/{resource} endpoints. */
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  /** 1-based page number (backend field name: `page`) */
  page: number;
  pageSize: number;
  totalPages?: number;
  hasPreviousPage?: boolean;
  hasNextPage?: boolean;
}

export interface ProjectDto {
  id: string;
  teamId: string;
  name: string;
  description?: string;
  thumbnailUrl?: string;
  createdAt: string;
  updatedAt: string;
}

export interface EpisodeDto {
  id: string;
  projectId: string;
  name: string;
  idea?: string;
  style?: string;
  status: string;
  templateId?: string;
  characterIds?: string[];
  directorNotes?: string;
  createdAt: string;
  updatedAt: string;
  renderedAt?: string;
}

export interface SagaStateDto {
  id: string;
  episodeId: string;
  currentStage: string;
  retryCount: number;
  lastError?: string;
  startedAt: string;
  updatedAt: string;
  isCompensating: boolean;
}

export interface JobDto {
  id: string;
  episodeId: string;
  type: string;
  status: string;
  payload?: string;
  result?: string;
  errorMessage?: string;
  queuedAt: string;
  startedAt?: string;
  completedAt?: string;
  attemptNumber: number;
}

// ── Phase 3: Template & Style Library ────────────────────────────────────────

export type Genre =
  | "Kids"
  | "Comedy"
  | "Drama"
  | "Horror"
  | "Romance"
  | "SciFi"
  | "Marketing"
  | "Fantasy";

export type Style =
  | "Pixar3D"
  | "Anime"
  | "WatercolorIllustration"
  | "ComicBook"
  | "Realistic"
  | "PhotoStorybook"
  | "RetroCartoon"
  | "Cyberpunk";

export interface PlotAct {
  name: string;
  description: string;
  beats: number;
}

export interface PlotStructure {
  acts: PlotAct[];
}

export interface TemplateDto {
  id: string;
  title: string;
  genre: Genre;
  description: string;
  plotStructure: PlotStructure;
  defaultStyle: Style;
  previewVideoUrl?: string;
  thumbnailUrl?: string;
  isActive: boolean;
  sortOrder: number;
}

export interface StylePresetDto {
  id: string;
  style: Style;
  displayName: string;
  description: string;
  sampleImageUrl?: string;
  fluxStylePromptSuffix: string;
  isActive: boolean;
}

// ── Phase 4: Character Studio ─────────────────────────────────────────────────

export type TrainingStatus =
  | "Draft"
  | "PoseGeneration"
  | "TrainingQueued"
  | "Training"
  | "Ready"
  | "Failed";

/** Maps the numeric C# enum value returned by the API to the string label. */
export const TRAINING_STATUS_MAP: Record<number, TrainingStatus> = {
  0: "Draft",
  1: "PoseGeneration",
  2: "TrainingQueued",
  3: "Training",
  4: "Ready",
  5: "Failed",
};

/** Normalises trainingStatus from number (API) → string (UI). */
export function normaliseTrainingStatus(raw: number | TrainingStatus): TrainingStatus {
  if (typeof raw === "string") return raw;
  return TRAINING_STATUS_MAP[raw] ?? "Draft";
}

export interface CharacterDto {
  id: string;
  teamId: string;
  name: string;
  description?: string;
  styleDna?: string;
  imageUrl?: string;
  loraWeightsUrl?: string;
  triggerWord?: string;
  /** Backend returns a numeric enum; normalised to string by useCharacters hook. */
  trainingStatus: TrainingStatus;
  trainingProgressPercent: number;
  creditsCost: number;
  createdAt: string;
  updatedAt: string;
}

/** @deprecated Use PagedResult<CharacterDto> instead. */
export type PagedCharactersResponse = PagedResult<CharacterDto>;

export interface CharacterJobAcceptedDto {
  jobId: string;
  characterId: string;
  message: string;
  estimatedCreditsCost: number;
}

/** SignalR message payload for CharacterTrainingUpdate */
export interface CharacterTrainingUpdatePayload {
  characterId: string;
  status: TrainingStatus;
  progressPercent: number;
  stage: string;
}

// ── Phase 5: Script Workshop ───────────────────────────────────────────────────

export interface DialogueLineDto {
  character: string;
  text: string;
  startTime: number;
  endTime: number;
}

export interface SceneDto {
  sceneNumber: number;
  visualDescription: string;
  emotionalTone: string;
  dialogue: DialogueLineDto[];
}

export interface ScreenplayDto {
  title: string;
  scenes: SceneDto[];
}

export interface ScriptDto {
  id: string;
  episodeId: string;
  title: string;
  screenplay: ScreenplayDto;
  isManuallyEdited: boolean;
  directorNotes?: string;
  createdAt: string;
  updatedAt: string;
}

// ── Phase 6: Storyboard Studio ────────────────────────────────────────────────

export interface StoryboardShotDto {
  id: string;
  storyboardId: string;
  sceneNumber: number;
  shotIndex: number;
  imageUrl?: string;
  description: string;
  styleOverride?: string;
  regenerationCount: number;
  updatedAt: string;
}

export interface StoryboardDto {
  id: string;
  episodeId: string;
  screenplayTitle: string;
  directorNotes?: string;
  shots: StoryboardShotDto[];
  createdAt: string;
  updatedAt: string;
}

/** SignalR message payload for ShotUpdated (team group). */
export interface ShotUpdatedPayload {
  shotId: string;
  storyboardId: string;
  episodeId: string;
  imageUrl?: string;
  regenerationCount: number;
}