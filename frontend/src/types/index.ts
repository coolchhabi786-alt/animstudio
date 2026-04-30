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

// ── Phase 7: Voice Studio ─────────────────────────────────────────────────────

/** Built-in OpenAI TTS voice names. */
export type BuiltInVoice = "Alloy" | "Echo" | "Fable" | "Onyx" | "Nova" | "Shimmer";

export const BUILT_IN_VOICES: { value: BuiltInVoice; label: string; gender: string }[] = [
  { value: "Alloy", label: "Alloy", gender: "Neutral" },
  { value: "Echo", label: "Echo", gender: "Male" },
  { value: "Fable", label: "Fable", gender: "Male" },
  { value: "Onyx", label: "Onyx", gender: "Male" },
  { value: "Nova", label: "Nova", gender: "Female" },
  { value: "Shimmer", label: "Shimmer", gender: "Female" },
];

export interface VoiceAssignmentDto {
  id: string;
  episodeId: string;
  characterId: string;
  characterName: string;
  voiceName: string;
  language: string;
  voiceCloneUrl?: string;
  updatedAt: string;
}

export interface VoiceAssignmentRequest {
  characterId: string;
  voiceName: string;
  language: string;
  voiceCloneUrl?: string;
}

export interface BatchUpdateVoicesRequest {
  assignments: VoiceAssignmentRequest[];
}

export interface VoicePreviewRequest {
  text: string;
  voiceName: string;
  language?: string;
}

export interface VoicePreviewResponse {
  audioUrl: string;
  expiresAt: string;
}

export interface VoiceCloneRequest {
  characterId: string;
  audioSampleUrl?: string;
}

export interface VoiceCloneResponse {
  voiceCloneUrl?: string;
  status: string;
}

// ────────────────────────────────────────────────────────────────────────────
// Phase 8 — Animation Studio
// ────────────────────────────────────────────────────────────────────────────

export type AnimationBackend = "Kling" | "Local";

export type AnimationStatus =
  | "PendingApproval"
  | "Approved"
  | "Running"
  | "Completed"
  | "Failed"
  | "Cancelled";

export type ClipStatus = "Pending" | "Rendering" | "Ready" | "Failed";

export interface AnimationBackendOption {
  value: AnimationBackend;
  label: string;
  description: string;
  perClipUsd: number;
}

export const ANIMATION_BACKENDS: AnimationBackendOption[] = [
  {
    value: "Kling",
    label: "Kling (Cloud)",
    description: "High-fidelity cloud rendering — ~$0.056 per shot",
    perClipUsd: 0.056,
  },
  {
    value: "Local",
    label: "Local (On-prem)",
    description: "Local GPU rendering — no per-shot spend",
    perClipUsd: 0,
  },
];

export interface AnimationEstimateLineItem {
  sceneNumber: number;
  shotIndex: number;
  storyboardShotId?: string;
  unitCostUsd: number;
}

export interface AnimationEstimateDto {
  episodeId: string;
  backend: AnimationBackend;
  shotCount: number;
  unitCostUsd: number;
  totalCostUsd: number;
  breakdown: AnimationEstimateLineItem[];
}

export interface ApproveAnimationRequest {
  backend: AnimationBackend;
}

export interface AnimationJobDto {
  id: string;
  episodeId: string;
  backend: AnimationBackend;
  estimatedCostUsd: number;
  actualCostUsd?: number | null;
  approvedByUserId?: string | null;
  approvedAt?: string | null;
  status: AnimationStatus;
  createdAt: string;
}

export interface AnimationClipDto {
  id: string;
  episodeId: string;
  sceneNumber: number;
  shotIndex: number;
  storyboardShotId?: string | null;
  clipUrl?: string | null;
  durationSeconds?: number | null;
  status: ClipStatus;
  createdAt: string;
}

export interface SignedClipUrlDto {
  clipId: string;
  url: string;
  expiresAt: string;
}

/** SignalR payload broadcast by the backend when a single clip is ready. */
export interface ClipReadyEvent {
  episodeId: string;
  clipId: string;
  sceneNumber: number;
  shotIndex: number;
  clipUrl: string;
}

// ── Phase 9 — Render & Delivery ──────────────────────────────────────────────

/** Maps to backend RenderAspectRatio enum (string serialized). */
export type RenderAspectRatio = "SixteenNine" | "NineSixteen" | "OneOne";

/** Maps to backend RenderStatus enum (string serialized). */
export type RenderStatus = "Pending" | "Rendering" | "Complete" | "Failed";

/** Display strings for aspect ratio values used by the picker. */
export const ASPECT_RATIO_DISPLAY: Record<RenderAspectRatio, string> = {
  SixteenNine: "16:9",
  NineSixteen: "9:16",
  OneOne: "1:1",
};

export interface RenderDto {
  id: string;
  episodeId: string;
  aspectRatio: RenderAspectRatio;
  status: RenderStatus;
  finalVideoUrl: string | null;
  cdnUrl: string | null;
  srtUrl: string | null;
  durationSeconds: number;
  errorMessage: string | null;
  createdAt: string;
  completedAt: string | null;
}

export interface StartRenderRequest {
  aspectRatio: RenderAspectRatio;
}

/** SignalR payload broadcast when render progress updates. */
export interface RenderProgressEvent {
  renderId: string;
  episodeId: string;
  percent: number;
  stage: string;
}

/** SignalR payload broadcast when a render completes. */
export interface RenderCompleteEvent {
  renderId: string;
  episodeId: string;
  cdnUrl: string | null;
  srtUrl: string | null;
  durationSeconds: number;
}

/** SignalR payload broadcast when a render fails. */
export interface RenderFailedEvent {
  renderId: string;
  episodeId: string;
  errorMessage: string;
}