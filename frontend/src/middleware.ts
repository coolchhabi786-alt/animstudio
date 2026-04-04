import { auth } from "@/lib/auth";
import { NextResponse } from "next/server";

const PUBLIC_PATHS = ["/login", "/signup", "/accept-invite", "/api/auth"];

export default auth(function middleware(req) {
  const { pathname } = req.nextUrl;

  // Allow public paths through
  if (PUBLIC_PATHS.some((p) => pathname.startsWith(p))) {
    return NextResponse.next();
  }

  // next-auth v5 exposes auth on the request object in the auth() callback
  const session = req.auth;

  if (!session) {
    const loginUrl = new URL("/login", req.url);
    loginUrl.searchParams.set("callbackUrl", pathname);
    return NextResponse.redirect(loginUrl);
  }

  const response = NextResponse.next();

  // Forward team context header if present (e.g. set by client)
  const teamId = req.headers.get("x-team-id");
  if (teamId) {
    response.headers.set("x-team-id", teamId);
  }

  return response;
});

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico).*)"],
};
