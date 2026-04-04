"use client";

import { Button } from "@/components/ui/button";
import { signIn } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

export default function LoginPage() {
  const router = useRouter();

  useEffect(() => {
    // Redirect if authenticated (example logic, replace w/ real auth check)
    if (false) { // Replace "false" with actual auth state
      router.push("/");
    }
  }, [router]);

  return (
    <main className="flex flex-col items-center justify-center min-h-screen bg-gray-50">
      <div className="p-6 bg-white rounded shadow">
        <h1 className="text-xl font-medium mb-6">Login to AnimStudio</h1>
        <Button onClick={() => signIn("azure-ad")}>Sign in with Azure AD</Button>
      </div>
    </main>
  );
}