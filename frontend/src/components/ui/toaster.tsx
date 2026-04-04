"use client";

import * as ToastPrimitives from "@radix-ui/react-toast";
import { cn } from "@/lib/utils";

/**
 * Minimal Toaster.  Renders the Radix Toast viewport so the app compiles.
 * Wire individual toasts via the useToast hook when notification flows are built.
 */
export function Toaster() {
  return (
    <ToastPrimitives.Provider swipeDirection="right">
      <ToastPrimitives.Viewport
        className={cn(
          "fixed bottom-0 right-0 z-[100] flex max-h-screen w-full flex-col-reverse gap-2 p-4 sm:max-w-[420px]"
        )}
      />
    </ToastPrimitives.Provider>
  );
}
