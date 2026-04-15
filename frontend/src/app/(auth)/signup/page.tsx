"use client";

import Link from "next/link";
import { Button } from "@/components/ui/button";

/**
 * Signup page.
 *
 * In DEVELOPMENT — Azure AD and Stripe are not required. Use the Dev Login
 * button on the login page to sign in instantly.
 *
 * In PRODUCTION — this page will initiate a Stripe checkout flow. The
 * /api/billing/checkout Next.js route handler needs to be implemented when
 * Stripe keys are configured.
 */
export default function SignupPage() {
  const isDev = process.env.NODE_ENV === "development";

  if (isDev) {
    return (
      <div className="space-y-6 text-center">
        <h1 className="text-2xl font-bold">Create your account</h1>
        <p className="text-gray-600 text-sm">
          You&apos;re running in <strong>development mode</strong>. No Azure AD
          or Stripe setup is needed.
        </p>
        <p className="text-gray-600 text-sm">
          Click <strong>Dev Login</strong> on the login page to sign in
          instantly as the seeded dev user.
        </p>
        <Button asChild className="w-full bg-blue-600 hover:bg-blue-700 text-white">
          <Link href="/login">Go to Dev Login →</Link>
        </Button>
      </div>
    );
  }

  // ── Production: Stripe checkout (implement when Stripe is configured) ──────
  return (
    <div className="space-y-6 text-center">
      <h1 className="text-2xl font-bold">Create your account</h1>
      <p className="text-gray-600 text-sm">
        Choose a plan to get started. You&apos;ll be redirected to our secure
        checkout.
      </p>
      <Button
        className="w-full bg-blue-600 hover:bg-blue-700 text-white"
        onClick={() =>
          (window.location.href =
            "mailto:hello@animstudio.io?subject=Sign+Up+Request")
        }
      >
        Contact Us to Sign Up
      </Button>
      <p className="text-xs text-gray-400">
        Already have an account?{" "}
        <Link href="/login" className="text-blue-500 hover:underline">
          Sign in
        </Link>
      </p>
    </div>
  );
}
