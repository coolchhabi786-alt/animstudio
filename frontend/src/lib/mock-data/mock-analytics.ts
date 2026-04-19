/** Mock analytics data for Phase 12 UI testing. 5 episode dashboards + 30-day admin metrics. */

export interface DashboardAnalytics {
  episodeId: string
  episodeName: string
  /** Total all-time view count */
  viewCount: number
  uniqueViewers: number
  renderCount: number
  shareCount: number
  embedCount: number
  avgWatchTimeSeconds: number
  commentCount: number
  /** Views per day for the last 7 days (index 0 = 7 days ago) */
  viewsLast7Days: number[]
  /** Views per hour for the last 24 hours (index 0 = 24h ago) */
  viewsLast24Hours: number[]
  publishedAt: string
}

export interface SubscriptionTierBreakdown {
  free: number
  pro: number
  studio: number
}

/** Platform-wide admin metrics spanning the last 30 days */
export interface AdminMetrics {
  /** Daily active users */
  dau: number
  /** Monthly active users */
  mau: number
  subscriptionTiers: SubscriptionTierBreakdown
  /** Average time from job submission to render complete (seconds) */
  avgProcessingTimeSeconds: number
  /** Average render cost per episode in USD */
  costPerEpisodeUsd: number
  /** Percentage of failed jobs (0.0–1.0) */
  errorRate: number
  /** Total episodes created in last 30 days */
  episodesCreated: number
  /** Total renders completed in last 30 days */
  rendersCompleted: number
  /** Total AI generation cost in USD (last 30 days) */
  totalAiCostUsd: number
  /** MRR in USD */
  monthlyRecurringRevenueUsd: number
  /** Pending jobs in the render queue right now */
  queueDepth: number
}

export const mockAnalytics: { dashboards: DashboardAnalytics[]; admin: AdminMetrics } = {
  dashboards: [
    {
      episodeId: 'ep-0011-2222-3333-4444-555566667777',
      episodeName: 'Neon City — Episode 1: The Signal',
      viewCount: 2847,
      uniqueViewers: 1923,
      renderCount: 4,
      shareCount: 312,
      embedCount: 47,
      avgWatchTimeSeconds: 68,
      commentCount: 23,
      viewsLast7Days: [210, 345, 189, 412, 388, 276, 503],
      viewsLast24Hours: [12, 8, 5, 3, 2, 4, 9, 15, 22, 31, 48, 56, 62, 58, 44, 39, 52, 67, 72, 58, 43, 34, 28, 19],
      publishedAt: '2026-04-15T12:00:00.000Z',
    },
    {
      episodeId: 'ep-0022-3333-4444-5555-666677778888',
      episodeName: 'Neon City — Episode 2: Dark Frequency',
      viewCount: 1245,
      uniqueViewers: 980,
      renderCount: 2,
      shareCount: 89,
      embedCount: 12,
      avgWatchTimeSeconds: 72,
      commentCount: 8,
      viewsLast7Days: [0, 0, 145, 312, 287, 298, 203],
      viewsLast24Hours: [5, 3, 2, 1, 1, 2, 4, 8, 12, 18, 26, 34, 41, 38, 29, 25, 33, 44, 48, 39, 28, 22, 16, 11],
      publishedAt: '2026-04-17T15:30:00.000Z',
    },
    {
      episodeId: 'ep-0033-4444-5555-6666-777788889999',
      episodeName: 'Galaxy Riders — Pilot',
      viewCount: 4512,
      uniqueViewers: 3102,
      renderCount: 6,
      shareCount: 678,
      embedCount: 134,
      avgWatchTimeSeconds: 91,
      commentCount: 51,
      viewsLast7Days: [523, 612, 489, 701, 654, 588, 945],
      viewsLast24Hours: [28, 19, 14, 11, 9, 12, 18, 27, 38, 52, 71, 89, 94, 87, 75, 68, 82, 101, 112, 98, 78, 61, 47, 34],
      publishedAt: '2026-04-10T10:00:00.000Z',
    },
    {
      episodeId: 'ep-0044-5555-6666-7777-88889999aaaa',
      episodeName: 'Timeless — Chapter 1',
      viewCount: 892,
      uniqueViewers: 712,
      renderCount: 3,
      shareCount: 54,
      embedCount: 8,
      avgWatchTimeSeconds: 55,
      commentCount: 4,
      viewsLast7Days: [78, 112, 98, 134, 145, 167, 158],
      viewsLast24Hours: [4, 3, 2, 1, 1, 2, 3, 5, 7, 11, 15, 19, 22, 20, 17, 14, 18, 23, 27, 22, 17, 13, 9, 6],
      publishedAt: '2026-04-12T08:00:00.000Z',
    },
    {
      episodeId: 'ep-0055-6666-7777-8888-9999aaaabbbb',
      episodeName: 'Deep Blue — Short Film',
      viewCount: 5103,
      uniqueViewers: 4201,
      renderCount: 1,
      shareCount: 1023,
      embedCount: 289,
      avgWatchTimeSeconds: 112,
      commentCount: 87,
      viewsLast7Days: [612, 734, 645, 812, 924, 786, 590],
      viewsLast24Hours: [34, 24, 18, 14, 11, 13, 21, 35, 52, 71, 98, 124, 138, 129, 111, 98, 119, 142, 156, 133, 108, 89, 68, 49],
      publishedAt: '2026-04-05T09:00:00.000Z',
    },
  ],
  admin: {
    dau: 847,
    mau: 5203,
    subscriptionTiers: {
      free: 3842,
      pro: 1089,
      studio: 272,
    },
    avgProcessingTimeSeconds: 487,
    costPerEpisodeUsd: 0.672,
    errorRate: 0.023,
    episodesCreated: 312,
    rendersCompleted: 289,
    totalAiCostUsd: 209.66,
    monthlyRecurringRevenueUsd: 18450,
    queueDepth: 7,
  },
}
