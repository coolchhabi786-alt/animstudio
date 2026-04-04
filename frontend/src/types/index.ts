export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  externalId: string;
  createdAt: string;
  lastLoginAt: string;
}

export interface TeamDto {
  id: string;
  name: string;
  ownerId: string;
  createdAt: string;
}

export interface TeamMemberDto {
  userId: string;
  email: string;
  displayName: string;
  avatarUrl: string | null;
  role: string;
  isAccepted: boolean;
  joinedAt: string;
}

export interface PlanDto {
  id: string;
  name: string;
  stripePriceId: string;
  episodesPerMonth: number;
  maxCharacters: number;
  maxTeamMembers: number;
  price: number;
  isDefault: boolean;
}

export interface SubscriptionDto {
  id: string;
  /** Human-readable plan name (e.g. "Starter", "Pro"). */
  planName: string;
  status: string;
  episodesUsedThisMonth: number;
  episodesPerMonth: number;
  currentPeriodEnd: string | null;
  trialEndsAt: string | null;
  cancelAtPeriodEnd: boolean;
  stripeCustomerId: string;
}