"use client";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { BUILT_IN_VOICES } from "@/types";

interface Props {
  value: string;
  onValueChange: (value: string) => void;
  disabled?: boolean;
}

export function VoicePicker({ value, onValueChange, disabled }: Props) {
  return (
    <Select value={value} onValueChange={onValueChange} disabled={disabled}>
      <SelectTrigger className="w-[180px]" aria-label="Select voice">
        <SelectValue placeholder="Select voice" />
      </SelectTrigger>
      <SelectContent>
        {BUILT_IN_VOICES.map((v) => (
          <SelectItem key={v.value} value={v.value}>
            <span className="flex items-center gap-2">
              <span>{v.label}</span>
              <span className="text-xs text-muted-foreground">({v.gender})</span>
            </span>
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
