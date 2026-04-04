import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { apiFetch } from "@/lib/api-client";

const inviteSchema = z.object({
  email: z.string().email(),
  role: z.enum(["Member", "Admin"]),
});

type InviteFormValues = z.infer<typeof inviteSchema>;

type Props = {
  teamId?: string;
};

export default function InviteMemberForm({ teamId }: Props) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<InviteFormValues>({
    resolver: zodResolver(inviteSchema),
  });

  const onSubmit = async (data: InviteFormValues) => {
    try {
      await apiFetch(`/api/v1/teams/${teamId}/invites`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
      });
      alert("Invitation sent successfully!");
    } catch {
      alert("Failed to send invitation.");
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div>
        <Label>Email</Label>
        <Input type="email" {...register("email")} aria-invalid={!!errors.email} />
        {errors.email && <span className="text-red-500">{errors.email.message}</span>}
      </div>

      <div>
        <Label>Role</Label>
        <select
          {...register("role")}
          className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
        >
          <option value="Member">Member</option>
          <option value="Admin">Admin</option>
        </select>
        {errors.role && <span className="text-red-500">{errors.role.message}</span>}
      </div>

      <Button type="submit">Send Invite</Button>
    </form>
  );
}