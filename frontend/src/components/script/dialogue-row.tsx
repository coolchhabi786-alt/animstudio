"use client";

import type { DialogueLineDto, CharacterDto } from "@/types";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

interface DialogueRowProps {
  line: DialogueLineDto;
  index: number;
  isEditMode: boolean;
  characters: CharacterDto[];
  onChange: (updated: DialogueLineDto) => void;
}

export function DialogueRow({ line, isEditMode, characters, onChange }: DialogueRowProps) {
  // Find the matching character for the avatar
  const character = characters.find(
    (c) => c.name.toLowerCase() === line.character.toLowerCase()
  );

  function handleChange(field: keyof DialogueLineDto, value: string | number) {
    onChange({ ...line, [field]: value });
  }

  return (
    <tr className="align-top hover:bg-gray-50/60 transition-colors">
      {/* Character column */}
      <td className="py-2 pr-3 align-middle">
        {isEditMode ? (
          <Select
            value={line.character}
            onValueChange={(val) => handleChange("character", val)}
          >
            <SelectTrigger className="w-full" aria-label="Select character">
              <SelectValue placeholder="Select character" />
            </SelectTrigger>
            <SelectContent>
              {characters.map((c) => (
                <SelectItem key={c.id} value={c.name}>
                  {c.name}
                </SelectItem>
              ))}
              {/* Keep current value even if not in roster */}
              {!characters.some((c) => c.name === line.character) && (
                <SelectItem value={line.character}>{line.character} (unknown)</SelectItem>
              )}
            </SelectContent>
          </Select>
        ) : (
          <div className="flex items-center gap-2">
            {character?.imageUrl ? (
              // eslint-disable-next-line @next/next/no-img-element
              <img
                src={character.imageUrl}
                alt={character.name}
                className="h-6 w-6 rounded-full object-cover shrink-0"
              />
            ) : (
              <span
                className="h-6 w-6 rounded-full bg-indigo-100 text-indigo-700 flex items-center justify-center text-xs font-semibold shrink-0"
                aria-hidden="true"
              >
                {line.character.charAt(0).toUpperCase()}
              </span>
            )}
            <span className="text-sm font-medium text-gray-900 truncate max-w-[90px]">
              {line.character}
            </span>
          </div>
        )}
      </td>

      {/* Dialogue text column */}
      <td className="py-2 pr-3">
        {isEditMode ? (
          <Textarea
            value={line.text}
            onChange={(e) => handleChange("text", e.target.value)}
            aria-label="Dialogue text"
            rows={2}
            className="min-h-0"
          />
        ) : (
          <span className="text-sm text-gray-800 leading-relaxed">{line.text}</span>
        )}
      </td>

      {/* Start time column */}
      <td className="py-2 pr-3 text-right align-middle">
        {isEditMode ? (
          <Input
            type="number"
            min={0}
            step={0.1}
            value={line.startTime}
            onChange={(e) => handleChange("startTime", parseFloat(e.target.value) || 0)}
            aria-label="Start time in seconds"
            className="w-16 text-right"
          />
        ) : (
          <span className="text-sm text-gray-600 tabular-nums">{line.startTime.toFixed(1)}s</span>
        )}
      </td>

      {/* End time column */}
      <td className="py-2 text-right align-middle">
        {isEditMode ? (
          <Input
            type="number"
            min={0}
            step={0.1}
            value={line.endTime}
            onChange={(e) => handleChange("endTime", parseFloat(e.target.value) || 0)}
            aria-label="End time in seconds"
            className="w-16 text-right"
          />
        ) : (
          <span className="text-sm text-gray-600 tabular-nums">{line.endTime.toFixed(1)}s</span>
        )}
      </td>
    </tr>
  );
}
