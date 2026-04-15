import NextAuth from "next-auth";
import Credentials from "next-auth/providers/credentials";
import MicrosoftEntraID from "next-auth/providers/microsoft-entra-id";

const isDev = process.env.NODE_ENV === "development";

// Dev fixed identity — must match backend DevAuthHandler GUIDs exactly
const DEV_USER = {
  id: "00000000-0000-0000-0000-000000000001",
  name: "Dev User",
  email: "dev@animstudio.local",
  teamId: "C0000001-0000-0000-0000-000000000001", // matches DevAuthHandler.DevTeamId
};

export const { handlers, auth, signIn, signOut } = NextAuth({
  secret: process.env.AUTH_SECRET,
  providers: [
    // ── Dev provider (only active in development, no Azure AD creds required) ──
    ...(isDev
      ? [
          Credentials({
            id: "dev",
            name: "Dev Login",
            credentials: {
              // No real credentials needed — accept any submission in dev
            },
            authorize() {
              return DEV_USER;
            },
          }),
        ]
      : []),

    // ── Microsoft Entra ID (production / when credentials are configured) ──
    ...(process.env.AUTH_MICROSOFT_ENTRA_ID_ID
      ? [
          MicrosoftEntraID({
            clientId: process.env.AUTH_MICROSOFT_ENTRA_ID_ID,
            clientSecret: process.env.AUTH_MICROSOFT_ENTRA_ID_SECRET!,
            tenantId: process.env.AUTH_MICROSOFT_ENTRA_ID_TENANT_ID ?? "common",
          }),
        ]
      : []),
  ],
  callbacks: {
    jwt({ token, user }) {
      if (user) {
        token.id = (user as typeof DEV_USER).id;
        token.teamId = (user as typeof DEV_USER).teamId;
      }
      return token;
    },
    session({ session, token }) {
      if (token && session.user) {
        (session.user as any).id = token.id as string;
        (session.user as any).teamId = token.teamId as string;
      }
      return session;
    },
  },
  pages: {
    signIn: "/login",
    // Redirect new users to the dashboard, not to next-auth's default /auth/new-user
    newUser: "/dashboard",
  },
});