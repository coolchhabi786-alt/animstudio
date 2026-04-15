import { auth } from "@/lib/auth";

// next-auth v5: export auth as the middleware function directly.
// The callback receives the augmented NextRequest with req.auth set.
export default auth((req) => {
  // ── Development: allow all routes without authentication ──────────────────
  // This lets developers navigate freely without needing Azure AD or a sign-in.
  if (process.env.NODE_ENV === "development") {
    return; // undefined = continue to the requested route
  }

  const { pathname } = req.nextUrl;
  const isAuthenticated = !!req.auth;

  // Public routes — never redirect
  const isPublicRoute =
    pathname === "/" ||
    pathname.startsWith("/login") ||
    pathname.startsWith("/signup") ||
    pathname.startsWith("/accept-invite") ||
    pathname.startsWith("/api/auth"); // next-auth internal endpoints

  if (!isAuthenticated && !isPublicRoute) {
    const loginUrl = new URL("/login", req.url);
    loginUrl.searchParams.set("callbackUrl", pathname);
    return Response.redirect(loginUrl);
  }
});

export const config = {
  // Skip Next.js internals, static files, and edge runtimes
  matcher: ["/((?!_next/static|_next/image|favicon.ico|.*\\.(?:svg|png|jpg|jpeg|gif|webp)$).*)"],
};
