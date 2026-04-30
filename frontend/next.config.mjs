/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,

  webpack(config, { isServer }) {
    // konva/react-konva require the 'canvas' npm package for server-side rendering.
    // We don't use SSR for the timeline canvas (it's client-only via dynamic(ssr:false)).
    // Marking it external tells webpack to skip bundling it — the browser's native
    // canvas API is used at runtime instead.
    if (isServer) {
      config.externals = [...(config.externals ?? []), { canvas: "canvas" }];
    }
    return config;
  },

  // Redirect next-auth v5 beta internal pages that can appear transiently
  // (e.g. /auth/new-user after first Credentials sign-in, /auth/signup, etc.)
  async redirects() {
    return [
      { source: "/auth/signup",   destination: "/signup",    permanent: false },
      { source: "/auth/new-user", destination: "/dashboard", permanent: false },
      { source: "/auth/signin",   destination: "/login",     permanent: false },
      { source: "/auth/error",    destination: "/login",     permanent: false },
    ];
  },

  async headers() {
    return [
      {
        source: "/(.*)",
        headers: [
          {
            key: "Cache-Control",
            value: "public, max-age=0, must-revalidate, stale-while-revalidate",
          },
        ],
      },
    ];
  },
  env: {
    NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY: process.env.NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY,
    NEXT_PUBLIC_API_BASE_URL: process.env.NEXT_PUBLIC_API_BASE_URL ?? process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5001",
  },
};

export default nextConfig;