"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { apiFetch } from "@/lib/api-client";
import { useState } from "react";

const profileSchema = z.object({
  displayName: z.string().min(1, "Name is required"),
});

type ProfileFormValues = z.infer<typeof profileSchema>;

export default function SettingsPage() {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ProfileFormValues>({
    resolver: zodResolver(profileSchema),
  });

  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const onSubmit = async (data: ProfileFormValues) => {
    try {
      await apiFetch("/api/v1/users/profile", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
      });
      setSuccessMessage("Profile updated successfully.");
    } catch (error) {
      setSuccessMessage(null);
      console.error("Failed to update profile.");
    }
  };

  return (
    <main className="p-6 bg-gray-100 min-h-screen">
      <form onSubmit={handleSubmit(onSubmit)} className="p-6 bg-white rounded shadow w-96">
        <h1 className="text-xl font-medium mb-6">Update Profile</h1>
        <Label>Name</Label>
        <Input type="text" {...register("displayName")} aria-invalid={!!errors.displayName} />
        {errors.displayName && <span className='text-red-500'>{errors.displayName.message}</span>}

        <Button type="submit" className="mt-6">Save</Button>

        {successMessage && (
          <div className="mt-4 text-green-500">{successMessage}</div>
        )}
      </form>
    </main>
  );
}