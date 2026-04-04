import "./globals.css";
import Providers from "./providers";
import { type ReactNode } from "react";

export const metadata = {
  title: "AnimStudio",
  description: "Create your animated projects effortlessly."
};

type Props = {
  children: ReactNode;
};

export default function RootLayout({ children }: Props) {
  return (
    <html lang="en">
      <body>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}