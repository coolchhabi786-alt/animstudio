# Video Composition & Export Guide

## Quick Answer: Two Approaches

### 🎯 **Recommended: Server-side (Backend + FFmpeg)**
- **Simpler, more reliable, professional quality**
- Timeline JSON → POST to backend → FFmpeg processes → MP4 output
- This is what production uses

### 🚀 **Advanced: Browser-based (Canvas + WebCodecs)**
- **Client-side, no backend needed**
- Complex, browser limitations, good for learning
- Use when you need instant preview without upload

---

## 1. KONVA COMPOSER: What It Does (and Doesn't)

### ✅ What Konva Does
- **Render 2D shapes** to HTML5 Canvas (text overlays, image layers, animations)
- **Draw video frames** via `Konva.Image` wrapping an HTML `<video>` element
- **Export snapshots** via `stage.toDataURL()` → PNG of current frame

### ❌ What Konva Doesn't Do
- **Audio playback/mixing** (not its job — audio is separate)
- **Video encoding** (creating .mp4 files)
- **Clip transitions** (fade, dissolve require frame-by-frame blending)
- **Real-time video export** (too slow in-browser)

### 🔗 The Sync Pattern: Audio + Video in Browser

```typescript
// This is what the Konva compositor does:

// 1. Create hidden video & audio elements
const video = document.createElement("video");
const audio = document.createElement("audio");

// 2. Load clips
video.src = "/api/assets/animation/23MarAnimation/scene_01_shot_01.mp4";
audio.src = "/api/assets/audio/scene_01_mr._whiskers.mp3";

// 3. Wrap video in Konva.Image (so canvas reads frames)
const konvaImage = new Konva.Image({
  image: video,    // ← any HTMLImageElement or HTMLVideoElement!
  width: 960,
  height: 540,
});

// 4. Sync audio to video in animation loop
const animation = new Konva.Animation(() => {
  // Keep currentTime in sync
  const audioTime = (video.currentTime * 1000 - audioStartOffsetMs) / 1000;
  if (Math.abs(audio.currentTime - audioTime) > 0.1) {
    audio.currentTime = audioTime;
  }
}, layer);

// 5. Play both together
video.play();
audio.play();
animation.start();
```

---

## 2. TIMELINE EDITOR: What It's For

The timeline UI exists to **preview edits**, not to render videos. It shows:

| Section | Purpose |
|---------|---------|
| **Video Track** | Arrange clip sequence, set transition types |
| **Audio Track** | Manage dialogue/VO timing, volume, fade-in/out |
| **Music Track** | Background music with auto-duck (reduce volume during dialogue) |
| **Text Track** | Text overlays with position, animation, timing |

You **edit here**, then **export JSON**, then **backend renders**.

---

## 3. RECOMMENDED WORKFLOW: Backend FFmpeg Rendering

### Step 1: Edit in Timeline / Composer
- Click timeline clips to arrange
- Set audio start times
- Add text overlays
- Preview with audio sync

### Step 2: Export Timeline as JSON
Click **"Export Timeline JSON"** → downloads:
```json
{
  "videoClips": [
    {
      "sceneNumber": 1,
      "shotIndex": 1,
      "clipUrl": "/api/assets/animation/23MarAnimation/scene_01_shot_01.mp4",
      "startMs": 0,
      "durationMs": 5640,
      "transitionIn": "fade"
    }
  ],
  "audioTracks": [
    {
      "type": "dialogue",
      "audioUrl": "/api/assets/audio/scene_01_mr._whiskers.mp3",
      "startMs": 500,    // ← YOU SET THIS VIA SLIDER
      "volumePercent": 90,
      "fadeInMs": 0,
      "fadeOutMs": 1000
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

### Step 3: Send to Backend
```typescript
const response = await fetch('/api/render/export-video', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(timelineData)
});

const job = await response.json();
// {
//   jobId: "job-xyz-123",
//   status: "queued",
//   estimatedTimeMs: 45000
// }
```

### Step 4: Backend FFmpeg Command
Your C# / .NET backend runs (simplified):
```bash
ffmpeg \
  -i scene_01_shot_01.mp4 -i scene_01_shot_02.mp4 \
  -i scene_01_mr_whiskers.mp3 \
  -filter_complex "
    [0:v] scale=960:540 [v0];
    [1:v] scale=960:540 [v1];
    [v0][v1] concat=n=2:v=1:a=0 [v];
    [v] drawtext=text='Mr. Whiskers':x=50:y=450:fontsize=36:fontcolor=white [out]
  " \
  -map "[out]" -map 1:a -c:v libx264 -c:a aac \
  output.mp4
```

This produces a **real, playable .mp4** file.

### Step 5: Poll for completion
```typescript
// Poll every 2 seconds
const interval = setInterval(async () => {
  const status = await fetch(`/api/render/jobs/${jobId}`).then(r => r.json());
  if (status.status === 'completed') {
    window.location.href = status.outputUrl;  // download the mp4!
    clearInterval(interval);
  }
}, 2000);
```

---

## 4. BACKEND IMPLEMENTATION SKETCH (C# / .NET)

```csharp
[ApiController]
[Route("api/render")]
public class RenderController : ControllerBase
{
    [HttpPost("export-video")]
    public async Task<IActionResult> ExportVideo([FromBody] TimelineData timeline)
    {
        var jobId = Guid.NewGuid().ToString();
        
        // Queue the job
        _backgroundJobs.Enqueue(() => RenderVideoWithFFmpeg(jobId, timeline));
        
        return Ok(new {
            jobId,
            status = "queued",
            estimatedTimeMs = 60000
        });
    }
    
    private void RenderVideoWithFFmpeg(string jobId, TimelineData timeline)
    {
        var ffmpegArgs = BuildFFmpegCommand(timeline);  // Your logic here
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = ffmpegArgs,
                UseShellExecute = false,
                RedirectStandardOutput = true
            }
        };
        
        process.Start();
        process.WaitForExit();
        
        // Save output to /outputs/{jobId}.mp4
        // Update job status to "completed"
    }
}
```

---

## 5. BROWSER-BASED EXPORT (Advanced, Educational)

If you want **client-side rendering** for learning (not production):

```typescript
import { Muxer } from 'mp4-muxer';

// Capture canvas frames + encode with WebCodecs
const muxer = new Muxer({
  target: new FileSystemWritableFileStream(handle),  // IndexedDB
  video: {
    codec: 'avc',
    width: 960,
    height: 540,
  },
  audio: {
    codec: 'aac',
    numberOfChannels: 2,
    sampleRate: 44100,
  }
});

const videoEncoder = new VideoEncoder({
  output: (chunk) => muxer.addVideoChunk(chunk),
  error: (e) => console.error(e),
});

videoEncoder.configure({
  codec: 'avc1.42E01E',
  width: 960,
  height: 540,
  bitrate: 2_500_000,
});

// For each frame:
// 1. Draw to canvas
stage.draw();

// 2. Get canvas as VideoFrame
const frame = new VideoFrame(stage.canvas, { timestamp: frameIndex * (1000 / 24) });
videoEncoder.encode(frame);
```

**Problems:**
- 🔴 Very slow (hours for 1-minute video)
- 🔴 Browser tabs limited to ~30min recordings
- 🟡 Only modern browsers support WebCodecs
- ✅ Good for learning the pipeline

**Use FFmpeg on backend instead** — 100x faster.

---

## 6. YOUR AUDIO TIMELINE WORKFLOW

### Setting up dialogue timing per shot:

```
Timeline:
S1·Shot1 (5.64s) → S2·Shot1 (5.64s) → S3·Shot1 (5.64s)
0ms ─────────────────  5640ms ────────────────── 11280ms

Audio track:
[Mr. Whiskers dialogue] ← starts at +500ms
  (so: after first 500ms of video, dialogue begins)
  (audio time 0 ↔ video time 0.5s)
```

**In the Konva Compositor slider:**
- Drag the slider to position the audio relative to the video
- The slider shows **offset in milliseconds**
- Positive = audio starts later (delay from video start)
- Negative = audio starts before video (pre-roll)

**Export includes this timing:**
```json
{
  "startMs": 500,  // ← This audio track starts 500ms into the video
  "audioUrl": "..."
}
```

**Backend FFmpeg uses it:**
```bash
ffmpeg \
  -i video.mp4 \
  -i audio.mp3 \
  -itsoffset 0.5 -i audio.mp3 \  # ← Delays audio by 500ms
  -filter_complex "[0:v]...[v]; [1:a][2:a]amix=inputs=2[a]" \
  -map "[v]" -map "[a]" output.mp4
```

---

## 7. QUICK REFERENCE: What Each Tool Does

| Tool | Use For | Output |
|------|---------|--------|
| **Konva Compositor** | Preview + text positioning | PNG frame snapshots |
| **Timeline Editor** | Clip sequencing + VO timing | Timeline JSON |
| **Backend FFmpeg** | ACTUAL VIDEO CREATION | .mp4 file (production) |

---

## 8. Next Steps

1. **Edit** your shots in the Compositor
2. **Adjust audio offset** with the slider (hear it in real-time)
3. **Click "Export Timeline JSON"**
4. **Implement backend API** (use FFmpeg or HandBrake)
5. **POSTthe JSON** to your backend
6. **Wait for render** → download .mp4

---

## Resources

- **FFmpeg Docs**: https://ffmpeg.org/ffmpeg-filters.html (concat, drawtext, volume)
- **mp4-muxer**: https://github.com/Vanilagy/mp4-muxer (JS muxing, educational)
- **WebCodecs**: https://developer.mozilla.org/en-US/docs/Web/API/WebCodecs_API (browser video encoding)
- **Konva Docs**: https://konvajs.org/docs/index.html (canvas rendering)

---

## TL;DR

**Use Backend + FFmpeg. It's 100x simpler and 1000x faster than browser export.**

Backend flow:
```
Timeline JSON → Backend FFmpeg → Real .mp4 file
```

Browser-only:
❌ Complex, slow, unreliable  
✅ Only use if you have no backend

The Konva Compositor is for **previewing** + **setting audio timing**.  
The export is for **sending to backend**.
