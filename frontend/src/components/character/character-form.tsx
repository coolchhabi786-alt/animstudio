"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { useCreateCharacter } from "@/hooks/use-characters";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";

// ── Form schema ───────────────────────────────────────────────────────────────

const schema = z.object({
  name: z
    .string()
    .min(1, "Character name is required.")
    .max(200, "Name must be 200 characters or fewer."),
  description: z
    .string()
    .max(2000, "Description must be 2000 characters or fewer.")
    .optional(),
  styleDna: z
    .string()
    .max(4000, "Style guidance must be 4000 characters or fewer.")
    .optional(),
});

type FormValues = z.infer<typeof schema>;

// ── Prompt tips ───────────────────────────────────────────────────────────────
const STYLE_TIPS = [
  'Art style: "anime", "Pixar 3D", "watercolor illustration"',
  'Palette: "pastel", "vibrant", "monochrome"',
  'Features: "big expressive eyes", "simple shapes", "detailed texture"',
];

// ── Component ─────────────────────────────────────────────────────────────────

interface CharacterFormProps {
  onSuccess?: () => void;
}

const TRAINING_COST = 50; // credits — matches the backend constant

/**
 * CharacterForm — react-hook-form + zod form for creating a new character.
 * Shows a cost estimate before submission and disables submit while pending.
 */
export function CharacterForm({ onSuccess }: CharacterFormProps) {
  const [showTips, setShowTips] = useState(false);
  const createMutation = useCreateCharacter();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  const onSubmit = async (values: FormValues) => {
    try {
      await createMutation.mutateAsync({
        name: values.name,
        description: values.description || undefined,
        styleDna: values.styleDna || undefined,
      });
      toast.success(`"${values.name}" is queued for training.`);
      reset();
      onSuccess?.();
    } catch {
      // apiFetch already shows a toast on error
    }
  };

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className="flex flex-col gap-4"
      aria-label="Create character form"
      noValidate
    >
      <div className="space-y-2">
        <Label htmlFor="char-name">
          Name <span className="text-destructive">*</span>
        </Label>
        <Input
          id="char-name"
          type="text"
          placeholder="e.g. Professor Whiskerbolt"
          {...register("name")}
          aria-invalid={!!errors.name}
          aria-describedby={errors.name ? "char-name-error" : undefined}
        />
        {errors.name && (
          <p id="char-name-error" className="text-xs text-destructive" role="alert">
            {errors.name.message}
          </p>
        )}
      </div>

      <div className="space-y-2">
        <Label htmlFor="char-description">Description</Label>
        <Textarea
          id="char-description"
          rows={3}
          placeholder="A wise, eccentric orange tabby with round glasses…"
          {...register("description")}
        />
        {errors.description && (
          <p className="text-xs text-destructive">{errors.description.message}</p>
        )}
      </div>

      <div className="space-y-2">
        <div className="flex items-center justify-between">
          <Label htmlFor="char-style">Style Guidance</Label>
          <Button
            type="button"
            variant="link"
            size="sm"
            onClick={() => setShowTips((p) => !p)}
            aria-expanded={showTips}
            aria-controls="style-tips"
            className="h-auto p-0 text-xs"
          >
            Prompt tips
          </Button>
        </div>

        {showTips && (
          <ul
            id="style-tips"
            className="rounded-md border border-purple-100 bg-purple-50 p-3 text-xs text-purple-800"
            aria-label="Style guidance tips"
          >
            {STYLE_TIPS.map((tip) => (
              <li key={tip} className="flex items-start gap-1">
                <span aria-hidden="true">•</span>
                <span>{tip}</span>
              </li>
            ))}
          </ul>
        )}

        <Textarea
          id="char-style"
          rows={3}
          placeholder="anime, pastel palette, big expressive eyes, 2D flat design"
          {...register("styleDna")}
        />
        {errors.styleDna && (
          <p className="text-xs text-destructive">{errors.styleDna.message}</p>
        )}
      </div>

      {/* Cost estimate */}
      <p className="rounded-md bg-amber-50 px-3 py-2 text-xs text-amber-700">
        Training this character costs{" "}
        <strong className="font-semibold">{TRAINING_COST} credits</strong>. Credits
        are charged immediately when you submit.
      </p>

      <Button
        type="submit"
        disabled={isSubmitting || createMutation.isPending}
        className="w-full"
      >
        {isSubmitting || createMutation.isPending
          ? "Queueing training…"
          : "Create Character"}
      </Button>
    </form>
  );
}
