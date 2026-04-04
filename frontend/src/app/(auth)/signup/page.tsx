"use client";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";

const signupSchema = z.object({
  email: z.string().email(),
  name: z.string().min(1, "Name is required"),
});

type SignupFormValues = z.infer<typeof signupSchema>;

export default function SignupPage() {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<SignupFormValues>({
    resolver: zodResolver(signupSchema),
  });

  const router = useRouter();

  const onSubmit = async (data: SignupFormValues) => {
    try {
      const response = await fetch("/api/v1/billing/checkout", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
      });
      const result = await response.json();
      router.push(result.redirectUrl);
    } catch {
      console.error("Signup failed");
    }
  };

  return (
    <main className="flex flex-col items-center justify-center min-h-screen bg-gray-50">
      <form onSubmit={handleSubmit(onSubmit)} className="p-6 bg-white rounded shadow w-96">
        <h1 className="text-xl font-medium mb-6">Sign up for AnimStudio</h1>
        <Label>Email</Label>
        <Input type="email" {...register("email")} aria-invalid={!!errors.email} />
        {errors.email && <span className='text-red-500'>{errors.email.message}</span>}

        <Label className="mt-4">Name</Label>
        <Input type="text" {...register("name")} aria-invalid={!!errors.name} />
        {errors.name && <span className='text-red-500'>{errors.name.message}</span>}

        <Button type="submit" className="mt-6">Start Free Trial</Button>
      </form>
    </main>
  );
}