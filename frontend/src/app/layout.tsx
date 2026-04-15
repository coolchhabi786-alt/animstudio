import "./globals.css";
import Providers from "./providers";

export const metadata = {
  title: "AnimStudio",
  description: "AI-powered animation studio platform",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <head>
        <meta name="viewport" content="width=device-width, initial-scale=1" />
      </head>
      <body className="bg-gray-100 font-sans">
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}