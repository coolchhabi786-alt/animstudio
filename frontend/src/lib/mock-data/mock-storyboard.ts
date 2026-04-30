/**
 * Mock storyboard using real AI-generated images from the cartoon automation pipeline.
 * Images served from: /api/assets/storyboard/29MarAnimationImages/
 * Episode: "The Superpowered Shenanigans of Mr. Whiskers"
 *   Scene 1 — Mr. Whiskers meets Dave (2 shots + 2 extras)
 *   Scene 2 — The Prank Plan (3 shots + 1 extra)
 *   Scene 3 — Superpowered Shenanigans (3 shots + 2 extras)
 */

export interface StoryboardShot {
  id:                string;
  sceneNumber:       number;
  shotIndex:         number;
  imageUrl:          string;
  description:       string;
  styleOverride?:    string;
  regenerationCount: number;
  updatedAt:         string;
}

export interface StoryboardScene {
  id:     string;
  number: number;
  shots:  StoryboardShot[];
}

export interface MockStoryboard {
  id:        string;
  episodeId: string;
  scenes:    StoryboardScene[];
  createdAt: string;
  updatedAt: string;
}

function img(file: string) {
  return `/api/assets/storyboard/29MarAnimationImages/${file}`;
}

export const mockStoryboard: MockStoryboard = {
  id:        "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  episodeId: "ep-0011-2222-3333-4444-555566667777",
  createdAt: "2026-03-29T08:00:00.000Z",
  updatedAt: "2026-04-27T10:00:00.000Z",
  scenes: [
    {
      id: "scene-0001-1111-2222-3333-444455556666",
      number: 1,
      shots: [
        {
          id: "shot-s1-01-aaaa-bbbb-cccc-ddddeeee0001",
          sceneNumber: 1, shotIndex: 1,
          imageUrl: img("scene_01_shot_01_6233dc.png"),
          description:
            "Wide establishing shot of Mr. Whiskers lounging on a sun-soaked windowsill as Dave the Owner walks in carrying groceries.",
          regenerationCount: 1,
          updatedAt: "2026-03-29T08:05:00.000Z",
        },
        {
          id: "shot-s1-01b-aaaa-bbbb-cccc-ddddeeee0001",
          sceneNumber: 1, shotIndex: 2,
          imageUrl: img("scene_01_shot_01_fa1fd5.png"),
          description:
            "Alternate take: tighter framing on Mr. Whiskers giving Dave a suspicious side-eye.",
          regenerationCount: 0,
          updatedAt: "2026-03-29T08:06:00.000Z",
        },
        {
          id: "shot-s1-02-aaaa-bbbb-cccc-ddddeeee0002",
          sceneNumber: 1, shotIndex: 3,
          imageUrl: img("scene_01_shot_02_3b5d67.png"),
          description:
            "Medium shot of Dave setting down bags, noticing Mr. Whiskers has knocked over a plant — again.",
          regenerationCount: 0,
          updatedAt: "2026-03-29T08:10:00.000Z",
        },
        {
          id: "shot-s1-02b-aaaa-bbbb-cccc-ddddeeee0002",
          sceneNumber: 1, shotIndex: 4,
          imageUrl: img("scene_01_shot_02_5422b4.png"),
          description:
            "Close-up on shattered pot and guilty paw — Mr. Whiskers pretends to be asleep.",
          styleOverride: "comic-panel",
          regenerationCount: 1,
          updatedAt: "2026-03-29T08:12:00.000Z",
        },
      ],
    },
    {
      id: "scene-0002-1111-2222-3333-444455556666",
      number: 2,
      shots: [
        {
          id: "shot-s2-01-aaaa-bbbb-cccc-ddddeeee0005",
          sceneNumber: 2, shotIndex: 1,
          imageUrl: img("scene_02_shot_01_13117c.png"),
          description:
            "Professor Paws (neighbour cat) sneaks through the cat flap with a gadget strapped to his back.",
          regenerationCount: 0,
          updatedAt: "2026-03-29T09:00:00.000Z",
        },
        {
          id: "shot-s2-01b-aaaa-bbbb-cccc-ddddeeee0005",
          sceneNumber: 2, shotIndex: 2,
          imageUrl: img("scene_02_shot_01_313eb6.png"),
          description:
            "Alternate angle: Professor Paws presenting a holographic blueprint to Mr. Whiskers.",
          regenerationCount: 0,
          updatedAt: "2026-03-29T09:02:00.000Z",
        },
        {
          id: "shot-s2-02-aaaa-bbbb-cccc-ddddeeee0006",
          sceneNumber: 2, shotIndex: 3,
          imageUrl: img("scene_02_shot_02_7b60f5.png"),
          description:
            "Two-shot of both cats studying the contraption — Mr. Whiskers looks intrigued, Professor Paws narrates.",
          styleOverride: "pixar3d",
          regenerationCount: 1,
          updatedAt: "2026-03-29T09:15:00.000Z",
        },
        {
          id: "shot-s2-03-aaaa-bbbb-cccc-ddddeeee0007",
          sceneNumber: 2, shotIndex: 4,
          imageUrl: img("scene_02_shot_03_1f3279.png"),
          description:
            "The gadget sparks to life, bathing the room in purple light as Mr. Whiskers gets zapped.",
          regenerationCount: 0,
          updatedAt: "2026-03-29T09:20:00.000Z",
        },
      ],
    },
    {
      id: "scene-0003-1111-2222-3333-444455556666",
      number: 3,
      shots: [
        {
          id: "shot-s3-01-aaaa-bbbb-cccc-ddddeeee0009",
          sceneNumber: 3, shotIndex: 1,
          imageUrl: img("scene_03_shot_01_b50a0d.png"),
          description:
            "Mr. Whiskers levitates off the couch — eyes glowing purple, fur on end — discovering his new superpower.",
          regenerationCount: 0,
          updatedAt: "2026-03-29T10:00:00.000Z",
        },
        {
          id: "shot-s3-01b-aaaa-bbbb-cccc-ddddeeee0009",
          sceneNumber: 3, shotIndex: 2,
          imageUrl: img("scene_03_shot_01_bb3785.png"),
          description:
            "Dave drops his coffee mug in shock as Mr. Whiskers floats past at eye level.",
          regenerationCount: 0,
          updatedAt: "2026-03-29T10:01:00.000Z",
        },
        {
          id: "shot-s3-02-aaaa-bbbb-cccc-ddddeeee0010",
          sceneNumber: 3, shotIndex: 3,
          imageUrl: img("scene_03_shot_02_480e61.png"),
          description:
            "Chaos: Mr. Whiskers zooms around the apartment at super-speed, knocking over everything.",
          regenerationCount: 1,
          updatedAt: "2026-03-29T10:10:00.000Z",
        },
        {
          id: "shot-s3-03-aaaa-bbbb-cccc-ddddeeee0011",
          sceneNumber: 3, shotIndex: 4,
          imageUrl: img("scene_03_shot_03_96a061.png"),
          description:
            "Professor Paws furiously scribbles notes while Dave tries to catch Mr. Whiskers with a laundry basket.",
          styleOverride: "comicbook",
          regenerationCount: 0,
          updatedAt: "2026-03-29T10:20:00.000Z",
        },
      ],
    },
  ],
};
