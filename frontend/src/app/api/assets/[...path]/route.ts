import { type NextRequest, NextResponse } from "next/server";
import { createReadStream, statSync, existsSync } from "fs";
import path from "path";
import { Readable } from "stream";

// Root folder containing all cartoon automation output assets.
// Only files under this path can be served — anything else gets a 403.
const ASSETS_ROOT = path.normalize("C:/Users/Vaibhav/cartoon_automation/output");

const CONTENT_TYPES: Record<string, string> = {
  ".mp4":  "video/mp4",
  ".mp3":  "audio/mpeg",
  ".png":  "image/png",
  ".jpg":  "image/jpeg",
  ".jpeg": "image/jpeg",
};

export async function GET(
  request: NextRequest,
  { params }: { params: { path: string[] } }
) {
  const subPath  = params.path.join("/");
  const filePath = path.normalize(path.join(ASSETS_ROOT, subPath));

  // Prevent path-traversal attacks
  if (!filePath.startsWith(ASSETS_ROOT)) {
    return new NextResponse("Forbidden", { status: 403 });
  }

  if (!existsSync(filePath)) {
    return new NextResponse("Not Found", { status: 404 });
  }

  const ext         = path.extname(filePath).toLowerCase();
  const contentType = CONTENT_TYPES[ext] ?? "application/octet-stream";
  const stat        = statSync(filePath);
  const fileSize    = stat.size;
  const rangeHeader = request.headers.get("range");

  // Support HTTP range requests so the browser can seek in video/audio
  if (rangeHeader) {
    const [rawStart, rawEnd] = rangeHeader.replace(/bytes=/, "").split("-");
    const start    = parseInt(rawStart, 10);
    const end      = rawEnd ? parseInt(rawEnd, 10) : Math.min(start + 2 * 1024 * 1024 - 1, fileSize - 1);
    const chunkLen = end - start + 1;

    const nodeStream = createReadStream(filePath, { start, end });
    const webStream  = Readable.toWeb(nodeStream) as ReadableStream;

    return new NextResponse(webStream, {
      status: 206,
      headers: {
        "Content-Range":  `bytes ${start}-${end}/${fileSize}`,
        "Accept-Ranges":  "bytes",
        "Content-Length": String(chunkLen),
        "Content-Type":   contentType,
        "Cache-Control":  "public, max-age=3600",
      },
    });
  }

  // Full file (used for images, short audio clips)
  const nodeStream = createReadStream(filePath);
  const webStream  = Readable.toWeb(nodeStream) as ReadableStream;

  return new NextResponse(webStream, {
    headers: {
      "Content-Length": String(fileSize),
      "Accept-Ranges":  "bytes",
      "Content-Type":   contentType,
      "Cache-Control":  "public, max-age=3600",
    },
  });
}
