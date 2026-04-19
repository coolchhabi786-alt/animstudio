/** Mock review link and comment data for Phase 11 UI testing. 3 links with 7 comments. */

export interface MockReviewLink {
  id: string
  episodeId: string
  /** URL-safe token used in the public review route: /review/[token] */
  token: string
  createdAt: string
  /** ISO timestamp or null if link never expires */
  expiresAt: string | null
  isRevoked: boolean
  /** Hashed password or null if no password set */
  password: string | null
  viewCount: number
  createdByName: string
}

export interface MockReviewComment {
  id: string
  reviewLinkId: string
  authorName: string
  text: string
  /** Playback position in seconds when comment was made */
  timestampSeconds: number
  isResolved: boolean
  createdAt: string
}

const links: MockReviewLink[] = [
  {
    id: 'rl-0001-aaaa-bbbb-cccc-ddddeeee1111',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    token: 'tkn_abc123def456ghi789jkl',
    createdAt: '2026-04-15T10:00:00.000Z',
    expiresAt: '2026-04-29T10:00:00.000Z',
    isRevoked: false,
    password: null,
    viewCount: 14,
    createdByName: 'Vaibhav Gupta',
  },
  {
    id: 'rl-0002-aaaa-bbbb-cccc-ddddeeee2222',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    token: 'tkn_xyz987uvw654rst321opq',
    createdAt: '2026-04-17T14:30:00.000Z',
    expiresAt: null,
    isRevoked: false,
    password: '$2b$10$hashedpasswordplaceholder',
    viewCount: 3,
    createdByName: 'Vaibhav Gupta',
  },
  {
    id: 'rl-0003-aaaa-bbbb-cccc-ddddeeee3333',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    token: 'tkn_exp000000000000000000',
    createdAt: '2026-04-01T09:00:00.000Z',
    expiresAt: '2026-04-08T09:00:00.000Z',
    isRevoked: false,
    password: null,
    viewCount: 28,
    createdByName: 'Vaibhav Gupta',
  },
]

const comments: MockReviewComment[] = [
  {
    id: 'rc-0001-aaaa-1111-2222-3333-44445555',
    reviewLinkId: 'rl-0001-aaaa-bbbb-cccc-ddddeeee1111',
    authorName: 'Priya Sharma',
    text: 'Love the city establishing shot! The neon color grading really sells the sci-fi vibe.',
    timestampSeconds: 3,
    isResolved: false,
    createdAt: '2026-04-15T16:22:00.000Z',
  },
  {
    id: 'rc-0002-aaaa-1111-2222-3333-44445555',
    reviewLinkId: 'rl-0001-aaaa-bbbb-cccc-ddddeeee1111',
    authorName: 'Jordan Lee',
    text: 'The transition into Scene 2 feels a bit abrupt. Could we use a dissolve here instead of a cut?',
    timestampSeconds: 26,
    isResolved: true,
    createdAt: '2026-04-16T09:05:00.000Z',
  },
  {
    id: 'rc-0003-aaaa-1111-2222-3333-44445555',
    reviewLinkId: 'rl-0001-aaaa-bbbb-cccc-ddddeeee1111',
    authorName: 'Jordan Lee',
    text: 'Dr. Chen\'s voice is perfect — really menacing without being over the top.',
    timestampSeconds: 34,
    isResolved: false,
    createdAt: '2026-04-16T09:10:00.000Z',
  },
  {
    id: 'rc-0004-aaaa-1111-2222-3333-44445555',
    reviewLinkId: 'rl-0001-aaaa-bbbb-cccc-ddddeeee1111',
    authorName: 'Priya Sharma',
    text: 'Music is slightly too loud in this section — the dialogue is getting buried.',
    timestampSeconds: 48,
    isResolved: false,
    createdAt: '2026-04-16T11:30:00.000Z',
  },
  {
    id: 'rc-0005-aaaa-1111-2222-3333-44445555',
    reviewLinkId: 'rl-0001-aaaa-bbbb-cccc-ddddeeee1111',
    authorName: 'Alex Kim',
    text: 'Scene 3 chase sequence is great! The pacing really picks up here.',
    timestampSeconds: 61,
    isResolved: false,
    createdAt: '2026-04-17T08:45:00.000Z',
  },
  {
    id: 'rc-0006-aaaa-2222-3333-4444-55556666',
    reviewLinkId: 'rl-0002-aaaa-bbbb-cccc-ddddeeee2222',
    authorName: 'Sam Rivera',
    text: 'The ending is ambiguous in a great way — really looking forward to episode 2.',
    timestampSeconds: 72,
    isResolved: false,
    createdAt: '2026-04-17T17:00:00.000Z',
  },
  {
    id: 'rc-0007-aaaa-2222-3333-4444-55556666',
    reviewLinkId: 'rl-0002-aaaa-bbbb-cccc-ddddeeee2222',
    authorName: 'Sam Rivera',
    text: 'Title card font feels slightly too bold. Maybe try a lighter weight?',
    timestampSeconds: 1,
    isResolved: true,
    createdAt: '2026-04-17T17:05:00.000Z',
  },
]

export const mockReviewLinks = { links, comments }
