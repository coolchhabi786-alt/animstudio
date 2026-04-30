"use client";

/**
 * AUDIO SYNC GUIDE
 *
 * Konva handles VIDEO RENDERING only. Audio is separate:
 *   1. Create a hidden <audio> element
 *   2. Sync it to <video> via currentTime
 *   3. When exporting: capture canvas frames + audio stream separately
 *   4. Mux them with ffmpeg or mp4-muxer library
 *
 * EXPORT PATTERN (simplified — for production use FFmpeg on backend):
 *   • Collect timeline data (video clips, audio tracks, text overlays)
 *   • POST to /api/render/export-video (backend job)
 *   • Backend calls ffmpeg with complex filtergraph:
 *     ffmpeg \
 *       -i scene_01_shot_01.mp4 -i scene_01_shot_02.mp4 \  (concat video clips)
 *       -i scene_01_mr_whiskers.mp3 \                      (dialogue)
 *       -i bg_music.mp3 \                                  (background music)
 *       -i storyboard_thumb.png \                          (optional overlay)
 *       -filter_complex "[0:v][1:v]concat=n=2:v=1[v]; ...text filters..." \
 *       -map "[v]" -map "[a]" output.mp4                   (composite + audio mix)
 */

import { useEffect, useRef, useState, useCallback } from "react";
import Konva from "konva";

// ── Shot catalog ──────────────────────────────────────────────────────────────

interface Shot {
  label:        string;
  videoUrl:     string;
  thumbnailUrl: string;
}

const SHOTS: Shot[] = [
  {
    label:        "S1 · Shot 1",
    videoUrl:     "/api/assets/animation/23MarAnimation/scene_01_shot_01.mp4",
    thumbnailUrl: "/api/assets/storyboard/29MarAnimationImages/scene_01_shot_01_6233dc.png",
  },
  {
    label:        "S1 · Shot 2",
    videoUrl:     "/api/assets/animation/23MarAnimation/scene_01_shot_02.mp4",
    thumbnailUrl: "/api/assets/storyboard/29MarAnimationImages/scene_01_shot_02_3b5d67.png",
  },
  {
    label:        "S2 · Shot 1",
    videoUrl:     "/api/assets/animation/23MarAnimation/scene_02_shot_01.mp4",
    thumbnailUrl: "/api/assets/storyboard/29MarAnimationImages/scene_02_shot_01_13117c.png",
  },
  {
    label:        "S2 · Shot 2",
    videoUrl:     "/api/assets/animation/23MarAnimation/scene_02_shot_02.mp4",
    thumbnailUrl: "/api/assets/storyboard/29MarAnimationImages/scene_02_shot_02_7b60f5.png",
  },
  {
    label:        "S2 · Shot 3",
    videoUrl:     "/api/assets/animation/23MarAnimation/scene_02_shot_03.mp4",
    thumbnailUrl: "/api/assets/storyboard/29MarAnimationImages/scene_02_shot_03_1f3279.png",
  },
  {
    label:        "S3 · Shot 1",
    videoUrl:     "/api/assets/animation/23MarAnimation/scene_03_shot_01.mp4",
    thumbnailUrl: "/api/assets/storyboard/29MarAnimationImages/scene_03_shot_01_b50a0d.png",
  },
  {
    label:        "S3 · Shot 2",
    videoUrl:     "/api/assets/animation/23MarAnimation/scene_03_shot_02.mp4",
    thumbnailUrl: "/api/assets/storyboard/29MarAnimationImages/scene_03_shot_02_480e61.png",
  },
  {
    label:        "S3 · Shot 3",
    videoUrl:     "/api/assets/animation/23MarAnimation/scene_03_shot_03.mp4",
    thumbnailUrl: "/api/assets/storyboard/29MarAnimationImages/scene_03_shot_03_96a061.png",
  },
];

// ── Audio tracks available (from your cartoon automation output) ────────────

interface AudioTrack {
  id:       string;
  label:    string;
  audioUrl: string;
  type:     "dialogue" | "music"; // for mixing (dialogue: -20dB, music: -30dB)
}

const AUDIO_TRACKS: AudioTrack[] = [
  {
    id:       "dialogue-s1",
    label:    "Scene 1 — Mr. Whiskers",
    audioUrl: "/api/assets/audio/scene_01_mr._whiskers.mp3",
    type:     "dialogue",
  },
  {
    id:       "dialogue-s2",
    label:    "Scene 2 — Professor Paws",
    audioUrl: "/api/assets/audio/scene_02_professor_paws.mp3",
    type:     "dialogue",
  },
  {
    id:       "dialogue-s3",
    label:    "Scene 3 — Mr. Whiskers",
    audioUrl: "/api/assets/audio/scene_03_mr._whiskers.mp3",
    type:     "dialogue",
  },
  {
    id:       "ambience-s1",
    label:    "Scene 1 — Ambience (Dave)",
    audioUrl: "/api/assets/audio/scene_01_dave_the_owner.mp3",
    type:     "music",
  },
];

// Canvas output dimensions (16:9)
const CANVAS_W = 960;
const CANVAS_H = 540;

// ── Component ─────────────────────────────────────────────────────────────────

export default function KonvaCompositor() {
  const containerRef   = useRef<HTMLDivElement>(null);
  const stageRef       = useRef<Konva.Stage | null>(null);
  const videoLayerRef  = useRef<Konva.Layer | null>(null);
  const overlayLayerRef = useRef<Konva.Layer | null>(null);
  const videoNodeRef   = useRef<Konva.Image | null>(null);
  const animRef        = useRef<Konva.Animation | null>(null);
  const videoElRef     = useRef<HTMLVideoElement | null>(null);
  const audioElRef     = useRef<HTMLAudioElement | null>(null);
  const transformerRef = useRef<Konva.Transformer | null>(null);

  const [selectedShot,        setSelectedShot]        = useState<Shot>(SHOTS[0]);
  const [selectedAudioTrack,  setSelectedAudioTrack]  = useState<AudioTrack | null>(null);
  const [audioStartOffsetMs,  setAudioStartOffsetMs]  = useState(0);
  const [isPlaying,           setIsPlaying]           = useState(false);
  const [overlayText,         setOverlayText]         = useState("Mr. Whiskers");
  const [status,              setStatus]              = useState("Load a shot + pick audio start point");
  const [videoDurationMs,     setVideoDurationMs]     = useState(0);

  // ── Bootstrap: create Stage once ──────────────────────────────────────────

  useEffect(() => {
    if (!containerRef.current) return;

    const stage = new Konva.Stage({
      container: containerRef.current,
      width:     CANVAS_W,
      height:    CANVAS_H,
    });
    stageRef.current = stage;

    const videoLayer = new Konva.Layer();
    stage.add(videoLayer);
    videoLayerRef.current = videoLayer;

    const overlayLayer = new Konva.Layer();
    stage.add(overlayLayer);
    overlayLayerRef.current = overlayLayer;

    const transformer = new Konva.Transformer({
      enabledAnchors: ["middle-left", "middle-right"],
      boundBoxFunc: (oldBox, newBox) => (newBox.width < 40 ? oldBox : newBox),
    });
    overlayLayer.add(transformer);
    transformerRef.current = transformer;

    stage.on("click tap", (e) => {
      if (e.target === stage || e.target instanceof Konva.Image) {
        transformer.nodes([]);
        overlayLayer.batchDraw();
      }
    });

    // Video element
    const video       = document.createElement("video");
    video.muted       = true;
    video.loop        = true;
    video.crossOrigin = "anonymous";
    video.playsInline = true;
    videoElRef.current = video;

    const videoNode = new Konva.Image({
      x:      0,
      y:      0,
      width:  CANVAS_W,
      height: CANVAS_H,
      image:  video,
    });
    videoLayer.add(videoNode);
    videoNodeRef.current = videoNode;

    // Audio element (for sync preview)
    const audio = document.createElement("audio");
    audio.crossOrigin = "anonymous";
    audioElRef.current = audio;

    const anim = new Konva.Animation(() => {
      // Sync audio to video
      if (videoElRef.current && audioElRef.current && selectedAudioTrack) {
        const audioCurrentTime = Math.max(
          0,
          (videoElRef.current.currentTime * 1000 - audioStartOffsetMs) / 1000
        );
        if (Math.abs(audioElRef.current.currentTime - audioCurrentTime) > 0.1) {
          audioElRef.current.currentTime = audioCurrentTime;
        }
      }
    }, videoLayer);
    animRef.current = anim;

    return () => {
      anim.stop();
      stage.destroy();
    };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // ── Load shot ──────────────────────────────────────────────────────────────

  useEffect(() => {
    const video = videoElRef.current;
    if (!video) return;

    animRef.current?.stop();
    video.pause();
    video.src = selectedShot.videoUrl;
    video.load();

    video.onloadedmetadata = () => {
      setVideoDurationMs(video.duration * 1000);
      setStatus(`Loaded: ${selectedShot.label} (${(video.duration).toFixed(1)}s)`);
    };

    setIsPlaying(false);
  }, [selectedShot]);

  // ── Load audio track ───────────────────────────────────────────────────────

  useEffect(() => {
    const audio = audioElRef.current;
    if (!audio || !selectedAudioTrack) return;

    audio.src = selectedAudioTrack.audioUrl;
    setStatus(`Audio: ${selectedAudioTrack.label} @ +${audioStartOffsetMs}ms`);
  }, [selectedAudioTrack, audioStartOffsetMs]);

  // ── Play / Pause ───────────────────────────────────────────────────────────

  const togglePlay = useCallback(() => {
    const video = videoElRef.current;
    const audio = audioElRef.current;
    const anim  = animRef.current;
    if (!video || !anim) return;

    if (isPlaying) {
      video.pause();
      audio?.pause();
      anim.stop();
      setIsPlaying(false);
      setStatus("Paused");
    } else {
      video.play().then(() => {
        audio?.play().catch(() => {
          console.warn("Audio autoplay blocked");
        });
        anim.start();
        setIsPlaying(true);
        setStatus(`Playing: ${selectedShot.label} + ${selectedAudioTrack?.label}`);
      }).catch(() => {
        setStatus("Playback blocked — click the canvas first");
      });
    }
  }, [isPlaying, selectedAudioTrack, selectedShot]);

  // ── Add text overlay ───────────────────────────────────────────────────────

  const addTextOverlay = useCallback(() => {
    const layer       = overlayLayerRef.current;
    const transformer = transformerRef.current;
    if (!layer || !transformer) return;

    const text = new Konva.Text({
      x:          60,
      y:          CANVAS_H - 80,
      text:       overlayText || "New overlay",
      fontSize:   36,
      fontFamily: "Arial",
      fontStyle:  "bold",
      fill:       "#ffffff",
      shadowColor:   "rgba(0,0,0,0.8)",
      shadowBlur:    6,
      shadowOffsetX: 1,
      shadowOffsetY: 1,
      draggable: true,
    });

    text.on("click tap", () => {
      transformer.nodes([text]);
      layer.batchDraw();
    });

    text.on("dblclick dbltap", () => {
      const stage     = stageRef.current!;
      const textPos   = text.getAbsolutePosition();
      const stageBox  = stage.container().getBoundingClientRect();

      text.hide();
      transformer.hide();
      layer.batchDraw();

      const textarea        = document.createElement("textarea");
      document.body.appendChild(textarea);
      textarea.value        = text.text();
      textarea.style.cssText = `
        position: absolute;
        top:      ${stageBox.top  + textPos.y}px;
        left:     ${stageBox.left + textPos.x}px;
        width:    ${text.width()}px;
        font-size:  ${text.fontSize()}px;
        font-family: Arial, sans-serif;
        font-weight: bold;
        color:    #ffffff;
        background: rgba(0,0,0,0.7);
        border:   1px solid #7c3aed;
        outline:  none;
        padding:  2px 4px;
        z-index:  9999;
        resize:   none;
      `;
      textarea.focus();

      function commitEdit() {
        text.text(textarea.value);
        text.show();
        transformer!.show();
        transformer!.forceUpdate();
        layer!.batchDraw();
        textarea.remove();
      }
      textarea.addEventListener("keydown", (e) => {
        if (e.key === "Escape" || (e.key === "Enter" && !e.shiftKey)) commitEdit();
      });
      textarea.addEventListener("blur", commitEdit);
    });

    layer.add(text);
    transformer.nodes([text]);
    layer.batchDraw();
  }, [overlayText]);

  // ── Export: Timeline JSON (send to backend for ffmpeg render) ──────────────

  const exportTimelineJson = useCallback(() => {
    const timelineData = {
      episode: selectedShot.label,
      videoClip: selectedShot.videoUrl,
      audioTracks: selectedAudioTrack ? [
        {
          id:       selectedAudioTrack.id,
          url:      selectedAudioTrack.audioUrl,
          startMs:  audioStartOffsetMs,
          type:     selectedAudioTrack.type,
          volume:   selectedAudioTrack.type === "dialogue" ? 90 : 30,
        },
      ] : [],
      textOverlays: [
        {
          text:     overlayText,
          position: "bottom-left",
          fontSize: 36,
          startMs:  0,
          durationMs: videoDurationMs,
        },
      ],
    };

    const json = JSON.stringify(timelineData, null, 2);
    const a    = document.createElement("a");
    a.href     = "data:application/json," + encodeURIComponent(json);
    a.download = `timeline_${selectedShot.label.replace(/\s·\s/g, "_")}.json`;
    a.click();

    setStatus("✓ Timeline JSON exported");
  }, [selectedShot, selectedAudioTrack, audioStartOffsetMs, overlayText, videoDurationMs]);

  // ── Export: Frame snapshot ─────────────────────────────────────────────────

  const exportFrame = useCallback(() => {
    const stage = stageRef.current;
    if (!stage) return;

    const dataUrl = stage.toDataURL({ pixelRatio: 1 });
    const a       = document.createElement("a");
    a.href        = dataUrl;
    a.download    = `frame_${selectedShot.label.replace(/\s·\s/g, "_")}.png`;
    a.click();

    setStatus("✓ Frame exported as PNG");
  }, [selectedShot]);

  // ── Render ────────────────────────────────────────────────────────────────

  return (
    <div className="flex h-full overflow-hidden">

      {/* ── Left panel: shots + audio ────────────────────────────────────── */}
      <div className="w-56 shrink-0 flex flex-col gap-0 overflow-y-auto bg-[#0f1623] border-r border-slate-800">

        {/* Shots section */}
        <p className="text-[10px] font-semibold text-slate-500 uppercase tracking-widest px-3 pt-3 pb-2">
          Video Clips
        </p>
        {SHOTS.map((shot) => (
          <button
            key={shot.videoUrl}
            onClick={() => setSelectedShot(shot)}
            className={`group relative w-full text-left border-b border-slate-800 transition-colors ${
              selectedShot.videoUrl === shot.videoUrl
                ? "bg-purple-900/40 border-l-2 border-l-purple-500"
                : "hover:bg-slate-800/50"
            }`}
          >
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img
              src={shot.thumbnailUrl}
              alt={shot.label}
              className="w-full aspect-video object-cover opacity-90 group-hover:opacity-100 transition-opacity"
            />
            <span className="block px-2 py-1 text-[10px] text-slate-400 truncate">
              {shot.label}
            </span>
          </button>
        ))}

        {/* Audio section */}
        <div className="border-t border-slate-800 mt-2 pt-2">
          <p className="text-[10px] font-semibold text-slate-500 uppercase tracking-widest px-3 pb-2">
            Audio Tracks
          </p>
          {AUDIO_TRACKS.map((track) => (
            <button
              key={track.id}
              onClick={() => setSelectedAudioTrack(track)}
              className={`w-full text-left px-3 py-2 text-[11px] border-b border-slate-800 transition-colors ${
                selectedAudioTrack?.id === track.id
                  ? "bg-emerald-900/40 border-l-2 border-l-emerald-500 text-emerald-300"
                  : "hover:bg-slate-800/50 text-slate-400"
              }`}
            >
              <span className="block truncate font-medium">{track.label}</span>
              <span className="block text-[9px] text-slate-600">{track.type}</span>
            </button>
          ))}
        </div>
      </div>

      {/* ── Centre: canvas + controls ────────────────────────────────────── */}
      <div className="flex-1 flex flex-col items-center justify-center gap-3 bg-[#07090f] overflow-hidden p-4">

        {/* Status bar */}
        <div className="flex items-center gap-4 self-stretch px-1">
          <span className="text-xs text-slate-500">{status}</span>
          <div className="flex-1" />
          <span className="text-[10px] text-slate-600 font-mono">{CANVAS_W}×{CANVAS_H}</span>
        </div>

        {/* Konva canvas */}
        <div
          ref={containerRef}
          className="rounded-lg overflow-hidden shadow-2xl border border-slate-700/60"
          style={{
            width:     CANVAS_W,
            height:    CANVAS_H,
            maxWidth:  "100%",
            maxHeight: "calc(100vh - 300px)",
          }}
        />

        {/* Audio offset slider ────────────────────────────────────────────── */}
        {selectedAudioTrack && (
          <div className="self-stretch flex items-center gap-3 px-2 bg-slate-800/30 rounded py-2">
            <span className="text-[10px] font-semibold text-slate-400 whitespace-nowrap">
              Audio Start:
            </span>
            <input
              type="range"
              min={-5000}
              max={videoDurationMs}
              step={100}
              value={audioStartOffsetMs}
              onChange={(e) => setAudioStartOffsetMs(Number(e.target.value))}
              className="flex-1"
            />
            <span className="text-[11px] font-mono text-slate-300 w-16 text-right">
              {audioStartOffsetMs > 0 ? "+" : ""}{(audioStartOffsetMs / 1000).toFixed(2)}s
            </span>
          </div>
        )}

        {/* Controls bar ──────────────────────────────────────────────────── */}
        <div className="flex items-center gap-3 self-stretch flex-wrap">
          {/* Play / Pause */}
          <button
            onClick={togglePlay}
            className={`px-4 py-1.5 rounded text-sm font-medium transition-colors ${
              isPlaying
                ? "bg-slate-700 hover:bg-slate-600 text-white"
                : "bg-purple-600 hover:bg-purple-500 text-white"
            }`}
          >
            {isPlaying ? "⏸ Pause" : "▶ Play"}
          </button>

          {/* Text overlay input + add */}
          <input
            type="text"
            value={overlayText}
            onChange={(e) => setOverlayText(e.target.value)}
            placeholder="Overlay text…"
            className="flex-1 min-w-0 px-3 py-1.5 bg-slate-800 border border-slate-700 rounded text-sm text-white placeholder-slate-500 focus:outline-none focus:border-purple-500"
          />
          <button
            onClick={addTextOverlay}
            className="px-4 py-1.5 bg-slate-700 hover:bg-slate-600 text-white rounded text-sm font-medium transition-colors"
          >
            + Add Text
          </button>

          <div className="w-px h-5 bg-slate-700" />

          {/* Export buttons */}
          <button
            onClick={exportFrame}
            className="px-4 py-1.5 bg-emerald-700 hover:bg-emerald-600 text-white rounded text-sm font-medium transition-colors"
          >
            Export Frame
          </button>

          <button
            onClick={exportTimelineJson}
            className="px-4 py-1.5 bg-cyan-700 hover:bg-cyan-600 text-white rounded text-sm font-medium transition-colors"
          >
            Export Timeline JSON
          </button>
        </div>

        {/* Tips */}
        <div className="self-stretch text-[10px] text-slate-600 leading-relaxed">
          <span className="text-slate-500 font-medium">Audio sync: </span>
          Use the slider to set when audio starts relative to video ·
          <span className="text-slate-500 font-medium ml-3">Export: </span>
          Timeline JSON goes to backend for FFmpeg rendering
        </div>
      </div>
    </div>
  );
}
