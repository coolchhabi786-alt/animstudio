# ✅ What's Ready to Test — Start Here

## 1. Run the Dev Server
```bash
cd frontend
npm run dev
# Server at http://localhost:3000
```

## 2. Navigate to Compositor Page
```
http://localhost:3000/studio/1/composer
```
*Look for "Compositor (Demo)" in the sidebar under Dev Studio*

---

## 3. What You Can Do Right Now

### A. Load a Shot (Video)
- **Left panel:** Click any shot thumbnail
- **Canvas:** Shows the video playing
- **Real assets:** From your `C:\Users\Vaibhav\cartoon_automation\output\animation\23MarAnimation\`

### B. Add Audio Track
- **Below shots:** "Audio Tracks" section
- **Click:** Select "Scene 1 — Mr. Whiskers" or any dialogue
- **Audio syncs:** Plays alongside the video

### C. Set Audio Start Time (THE KEY FEATURE)
- **Slider:** "Audio Start" appears below the canvas
- **Drag left/right:** Position when audio begins relative to video
- **Display:** Shows offset in seconds (e.g., "+0.50s")
- **Example:**
  - Slider at 0s → audio starts with video
  - Slider at +0.5s → audio begins 500ms into video
  - Slider at -1.0s → audio starts 1 second BEFORE video (pre-roll)

### D. Add Text on Canvas
1. Type text in the input field (e.g., "Mr. Whiskers")
2. Click "+ Add Text"
3. Text appears on the video
4. **Drag** to move it
5. **Double-click** to edit inline
6. **Click** to select (shows resize handles on edges)

### E. Play Everything Together
- Click **"▶ Play"** button
- Video plays in canvas
- Audio plays from the audio element (synced via slider offset)
- Text overlays sit on top
- See everything in real-time

### F. Export
- **"Export Frame"** → PNG snapshot of what's on canvas RIGHT NOW
- **"Export Timeline JSON"** → Downloads a JSON file with all your settings

---

## 4. What the JSON Looks Like

When you click "Export Timeline JSON", it downloads something like:
```json
{
  "episode": "S1 · Shot 1",
  "videoClip": "/api/assets/animation/23MarAnimation/scene_01_shot_01.mp4",
  "audioTracks": [
    {
      "id": "dialogue-s1",
      "url": "/api/assets/audio/scene_01_mr._whiskers.mp3",
      "startMs": 500,
      "type": "dialogue",
      "volume": 90
    }
  ],
  "textOverlays": [
    {
      "text": "Mr. Whiskers",
      "position": "bottom-left",
      "fontSize": 36,
      "startMs": 0,
      "durationMs": 5640
    }
  ]
}
```

**This JSON is the blueprint.** Your backend will take it and use FFmpeg to create the actual .mp4 file.

---

## 5. Timeline Editor (Also Ready)

Visit: `http://localhost:3000/studio/1/timeline`

Shows:
- **Video Track:** 8 real shot clips arranged sequentially
- **Audio Track:** Dialogue tracks
- **Music Track:** Background audio with auto-duck
- **Text Track:** Text overlays with timing

This is for **editing the full episode sequence** while Compositor is for **fine-tuning a single shot**.

---

## 6. Where Your Assets Are Served From

All files are served via: `/api/assets/...`

Examples:
- Video: `/api/assets/animation/23MarAnimation/scene_01_shot_01.mp4`
- Audio: `/api/assets/audio/scene_01_mr._whiskers.mp3`
- Images: `/api/assets/storyboard/29MarAnimationImages/scene_01_shot_01_6233dc.png`

The API endpoint supports **HTTP range requests** (so you can seek in videos).
Server code: `src/app/api/assets/[...path]/route.ts`

---

## 7. Understanding Audio Sync (Critical!)

### The Problem
- Video plays via `<video>` element in Konva
- Audio plays via separate `<audio>` element
- Both are HTML elements, not controlled by Konva
- **They can desync** if not synced properly

### The Solution (What We Implemented)
```typescript
// In Konva Animation loop (runs every frame):
const audioTime = (video.currentTime * 1000 - audioStartOffsetMs) / 1000;
if (Math.abs(audio.currentTime - audioTime) > 0.1) {
  audio.currentTime = audioTime;  // ← Snap audio to match video
}
```

**In plain English:**
1. Video plays
2. User sets offset via slider (e.g., +500ms)
3. Audio's `currentTime` is calculated: `(video.currentTime - offset)`
4. If audio drifts, snap it back into sync
5. Result: Video + audio stay together, offset by user's amount

---

## 8. Quick Test: Steps to Verify Everything Works

### Test #1: Load Video
1. Go to `/studio/1/composer`
2. Click "S1 · Shot 1" thumbnail
3. ✅ Video appears in canvas
4. ✅ Says "Loaded: S1 · Shot 1 (5.6s)"

### Test #2: Load Audio
1. Click "Scene 1 — Mr. Whiskers" in Audio Tracks
2. ✅ Audio slider appears
3. ✅ Status shows "Audio: Scene 1 — Mr. Whiskers @ +0ms"

### Test #3: Play Both
1. Click "▶ Play"
2. ✅ Click canvas if autoplay blocked
3. ✅ Video plays in canvas
4. ✅ Audio plays in background
5. ✅ They stay in sync

### Test #4: Adjust Offset
1. Drag "Audio Start" slider to +1s
2. Click "▶ Play"
3. ✅ Audio starts 1 second into the video
4. ✅ Status shows "Audio Start: +1.00s"

### Test #5: Add Text
1. Type "Amazing!" in the text input
2. Click "+ Add Text"
3. ✅ Text appears on canvas (bottom-left)
4. ✅ Can drag it around
5. ✅ Double-click to edit

### Test #6: Export
1. Click "Export Timeline JSON"
2. ✅ Downloads file `timeline_S1_·_Shot_1.json`
3. Open it, verify JSON structure

---

## 9. Common Issues & Fixes

### Video doesn't load?
- Check browser console (F12) for errors
- Verify API server running: `http://localhost:3000/api/assets/animation/23MarAnimation/scene_01_shot_01.mp4`
- Try in incognito mode (cache issue?)

### Audio doesn't play?
- Might be autoplay blocked → click canvas first
- Check audio URL: `/api/assets/audio/scene_01_mr._whiskers.mp3` exists?
- Audio slider offset might be way off? Reset it

### Text not showing?
- Click the text overlay to see if it's there (might be off-screen)
- Type new text slowly, wait for UI update

### Sync falls apart?
- Try pausing and pressing play again
- Refresh the page
- Check browser console for JS errors

---

## 10. Next: Actual Video Export (Backend Part)

Once you've tested the compositor:

**Frontend job: DONE** ✅  
- You can preview everything
- You can export timeline JSON

**Backend job: TODO**  
- Build `/api/render/export-video` endpoint
- Use FFmpeg to composite video + audio + text
- Output real .mp4 file

For guidance, see: `src/lib/guides/video-composition-guide.md`

---

## 11. The Big Picture

```
┌─────────────────────────────────────────────────────────────────┐
│  You Are Here: Frontend Preview & Timeline Definition            │
├─────────────────────────────────────────────────────────────────┤
│  Konva Compositor                                               │
│  ├─ Load shot video                                           │
│  ├─ Load audio, set offset                                   │
│  ├─ Add text overlays                                        │
│  ├─ Preview in real-time (audio synced!)                    │
│  └─ Export Timeline JSON                                     │
├─────────────────────────────────────────────────────────────────┤
│  Backend (Next Phase)                                           │
│  ├─ Receive Timeline JSON                                    │
│  ├─ Run FFmpeg with complex filters                         │
│  ├─ Composite: video clips + transitions + audio mix + text │
│  └─ Output: Real .mp4 file                                   │
├─────────────────────────────────────────────────────────────────┤
│  User Gets: Download link to finished episode                    │
└─────────────────────────────────────────────────────────────────┘
```

**Your job (frontend):** DONE  
**Backend team's job:** Use the JSON you export + FFmpeg

---

## 12. Key Files to Know

```
src/components/composer/konva-compositor.tsx
  ↑ The Konva canvas with audio sync (what you interact with)

src/app/api/assets/[...path]/route.ts
  ↑ Serves your local cartoon automation output files

src/lib/mock-data/mock-timeline.ts
  ↑ Real video/audio URLs for testing

src/lib/guides/video-composition-guide.md
  ↑ Full technical deep-dive (read when curious)

README-VIDEO-PRODUCTION.md  (this file's parent)
  ↑ Product overview
```

---

## 13. You're Ready! 🚀

Everything is in place to:
1. ✅ Load your cartoon assets
2. ✅ Preview shots with audio
3. ✅ Set audio timing (the crucial part you asked about)
4. ✅ Add text overlays
5. ✅ Export a blueprint (JSON) for backend rendering

**Start here:** Go to `/studio/1/composer` and click a shot! 🎬
