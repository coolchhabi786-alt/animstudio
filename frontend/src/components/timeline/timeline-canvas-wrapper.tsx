"use client";

import { useEffect, useRef, useState } from "react";
import dynamic from "next/dynamic";

// Konva uses browser APIs — load only on the client side
const TimelineContainer = dynamic(
  () => import("./timeline-container").then((m) => m.TimelineContainer),
  { ssr: false }
);

/**
 * TimelineCanvasWrapper
 *
 * Measures the available container width via ResizeObserver and passes it
 * to the Konva Stage. Handles horizontal scrolling when the timeline
 * content is wider than the viewport.
 */
export function TimelineCanvasWrapper() {
  const wrapperRef                          = useRef<HTMLDivElement>(null);
  const [containerWidth, setContainerWidth] = useState(0);
  const [mounted, setMounted]               = useState(false);

  useEffect(() => {
    setMounted(true);
    const el = wrapperRef.current;
    if (!el) return;

    setContainerWidth(el.clientWidth);

    const ro = new ResizeObserver((entries) => {
      setContainerWidth(entries[0].contentRect.width);
    });
    ro.observe(el);
    return () => ro.disconnect();
  }, []);

  return (
    <div
      ref={wrapperRef}
      className="w-full overflow-x-auto overflow-y-hidden bg-[#0a0f1a] select-none"
      // Prevent text selection while dragging clips
    >
      {mounted && containerWidth > 0 && (
        <TimelineContainer containerWidth={containerWidth} />
      )}
    </div>
  );
}
