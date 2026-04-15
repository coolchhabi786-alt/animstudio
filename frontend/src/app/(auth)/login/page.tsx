"use client";

import { signIn, useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";

export default function LoginPage() {
  const { data: session, status } = useSession();
  const router = useRouter();

  if (status === "loading") {
    return <Skeleton className="h-10 w-36" />;
  }

  if (session) {
    router.push("/dashboard");
    return null;
  }

  const isDev = process.env.NODE_ENV === "development";

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50">
      <div className="w-full max-w-sm space-y-6 rounded-lg bg-white p-8 shadow">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-900">AnimStudio</h1>
          <p className="mt-2 text-sm text-gray-600">Sign in to your account</p>
        </div>

        {isDev && (
          <div className="rounded-md border border-amber-200 bg-amber-50 p-3 text-sm text-amber-800">
            <p className="font-medium">Development Mode</p>
            <p className="mt-1">Click &ldquo;Dev Login&rdquo; to sign in as the local dev user without Azure AD.</p>
          </div>
        )}

        <div className="space-y-3">
          {isDev && (
            <Button
              className="w-full bg-amber-500 hover:bg-amber-600 text-white"
              onClick={() => signIn("dev", { callbackUrl: "/dashboard" })}
            >
              Dev Login (local only)
            </Button>
          )}

          {process.env.AUTH_MICROSOFT_ENTRA_ID_ID && (
            <Button
              className="w-full"
              onClick={() => signIn("microsoft-entra-id", { callbackUrl: "/dashboard" })}
            >
              Sign in with Microsoft
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}