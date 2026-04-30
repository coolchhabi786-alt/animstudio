# 🎬 AnimStudio Video Production Quick Start

## What You Have Now

### 1. **Real Assets Loaded** ✅
- 8 actual shot videos (5.64s each) from your `23MarAnimation` folder
- Real dialogue audio files (Mr. Whiskers, Professor Paws, etc.)
- Real storyboard images (29Mar batch)

### 2. **Asset Server Running** ✅
- API: `/api/assets/animation/23MarAnimation/scene_01_shot_01.mp4`
- Supports range requests (you can seek in videos)
- Serves audio, video, images locally

### 3. **Konva Compositor (Educational)** ✅
- `http://localhost:3000/studio/1/composer`
- **View a shot video** → click shot thumbnail
- **Add audio** → select from "Audio Tracks" panel
- **Sync audio to video** → drag the "Audio Start" slider
- **Add text overlays** → type text, click "Add Text", drag to position
- **Export PNG frame** → click "Export Frame" (snapshot of current canvas)
- **Export Timeline JSON** → click "Export Timeline JSON" (send to backend)

### 4. **Timeline Editor** ✅
- `http://localhost:3000/studio/1/timeline`
- Arrange video clips, manage tracks
- Real shot videos in sequence

---

## How to Use them Together: Simple Workflow

### Step 1: Go to Komva Compositor
```
http://localhost:3000/studio/1/composer
```

### Step 2: Select a Shot
Click on a shot thumbnail in the left panel. Video appears in canvas.

### Step 3: Pick Audio Track
Select from "Audio Tracks" section. Audio syncs with video.

### Step 4: Adjust Audio Timing
Use the **"Audio Start"** slider to position when the audio begins:
- Drag right = delay audio (it starts later)
- Drag left = pre-roll audio (it starts before video)
- Watch the timecode (e.g., "+0.50s" = starts 500ms after video)

### Step 5: Add Text Overlay
1. Type text in the input field
2. Click "+ Add Text"
3. Text appears on canvas (drag to move, double-click to edit)

### Step 6: Export What You've Built
**Option A: Just Preview PNG**
- Click "Export Frame" → saves PNG of what you see RIGHT NOW

**Option B: Export for Backend Rendering** (RECOMMENDED)
- Click "Export Timeline JSON"
- This downloads a JSON file with:
  ```json
  {
    "videoClips": [...],
    "audioTracks": [
      { "startMs": 500, "audioUrl": "...", "volumePercent": 90 }
    ],
    "textOverlays": [...]
  }
  ```

---

## Then: Backend Rendering (Next Phase)

Once you export the JSON, your **C# backend** takes over:

```csharp
// 1. Receive JSON POST
POST /api/render/export-video { ...timelineData... }

// 2. Backend builds FFmpeg command
ffmpeg -i scene_01_shot_01.mp4 \
       -i scene_01_mr_whiskers.mp3 \
       -filter_complex "concat video, drawtext overlays, amix audio" \
       output.mp4

// 3. Output real .mp4 file
// Returns download link
```

---

## Key Concepts Explained

### Konva's Role
- **Draws to canvas**: video frames + text shapes
- **Not for export**: doesn't create .mp4 files
- **For preview**: audio syncs in real-time so you hear + see together

### Audio Offset (The Slider)
```
Video timeline:  |--shot1 (5.64s)--|
Audio:                    |--dialogue starts at +500ms--|

You set this offset via the slider.
Backend's FFmpeg uses it to delay the audio file.
```

### Why Export JSON Not MP4
- **Browser can't encode video fast enough** (would take hours)
- **Backend FFmpeg does it in seconds**
- **JSON is just instructions** → backend interprets them
- **Separation of concerns**: frontend UI ≠ encoding engine

---

## What's Next (Implementation Checklist)

- [ ] Go to Compositor, load a shot
- [ ] Select audio, adjust offset slider
- [ ] Add text overlay
- [ ] Click "Export Timeline JSON"
- [ ] Check JSON file in downloads folder
- [ ] Share with backend team to implement FFmpeg render endpoint
- [ ] Test backend rendering

---

## Common Questions

**Q: Can I export a full episode?**  
A: Yes! Timeline JSON supports multiple clips. Just set their start times in ms.

**Q: Does Konva play audio?**  
A: No, but we added a hidden `<audio>` element that syncs via `currentTime`.

**Q: Why can't I download an MP4 from Konva?**  
A: Browser video encoding is **1000x slower** than FFmpeg. Use backend.

**Q: What if I don't have a backend?**  
A: Check the guide at `src/lib/guides/video-composition-guide.md` for browser-based WebCodecs approach (educational only, slow).

**Q: Can I use this for other shows?**  
A: Yes! Just change the `SHOTS` array URLs to your assets folder.

---

## File Locations (For Reference)

```
src/
  app/
    (dashboard)/studio/[id]/
      composer/page.tsx           ← The page you visit
      timeline/page.tsx           ← Timeline editor
    api/assets/[...path]/
      route.ts                    ← Serves your local files
  components/composer/
    konva-compositor.tsx          ← The Konva canvas code
  lib/
    mock-data/
      mock-timeline.ts            ← Real video URLs
      mock-storyboard.ts          ← Real image URLs
    guides/
      video-composition-guide.md  ← Full technical guide
  types/
    timeline.ts                   ← Data shapes
```

---

## Next Boost: Real Video Export

When backend team is ready, they implement:

```csharp
// AnimStudio.API/Controllers/RenderController.cs

[HttpPost("export-video")]
public async Task<IActionResult> ExportVideo([FromBody] TimelineData timeline)
{
    // 1. Validate timeline
    // 2. Build FFmpeg filter_complex
    // 3. Spawn ffmpeg process
    // 4. Stream output to /uploads/{jobId}.mp4
    // 5. Return download URL
    
    return Ok(new { downloadUrl = "..." });
}
```

The **frontend** (what you just built):
- User clicks "Export Timeline JSON"
- POST to `/api/render/export-video`
- Backend handles the heavy lifting
- Download link appears when done

---

## Final Tip

**Konva is the Preview Canvas.**  
**Your Backend is the Production Engine.**

Konva shows you what it will look like.  
Backend actually makes it.

Both are needed. Both matter. 👍
