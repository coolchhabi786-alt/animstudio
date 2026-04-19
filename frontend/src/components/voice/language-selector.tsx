"use client";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

const LANGUAGES = [
  { value: "en-US", label: "English (US)", flag: "🇺🇸" },
  { value: "en-GB", label: "English (UK)", flag: "🇬🇧" },
  { value: "es-ES", label: "Spanish", flag: "🇪🇸" },
  { value: "fr-FR", label: "French", flag: "🇫🇷" },
  { value: "de-DE", label: "German", flag: "🇩🇪" },
  { value: "it-IT", label: "Italian", flag: "🇮🇹" },
  { value: "pt-BR", label: "Portuguese (BR)", flag: "🇧🇷" },
  { value: "ja-JP", label: "Japanese", flag: "🇯🇵" },
  { value: "ko-KR", label: "Korean", flag: "🇰🇷" },
  { value: "zh-CN", label: "Chinese (Simplified)", flag: "🇨🇳" },
];

interface Props {
  value: string;
  onValueChange: (value: string) => void;
  disabled?: boolean;
}

export function LanguageSelector({ value, onValueChange, disabled }: Props) {
  return (
    <Select value={value} onValueChange={onValueChange} disabled={disabled}>
      <SelectTrigger className="w-[200px]" aria-label="Select language">
        <SelectValue placeholder="Select language" />
      </SelectTrigger>
      <SelectContent>
        {LANGUAGES.map((lang) => (
          <SelectItem key={lang.value} value={lang.value}>
            <span className="flex items-center gap-2">
              <span>{lang.flag}</span>
              <span>{lang.label}</span>
            </span>
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
