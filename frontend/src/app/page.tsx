import { Button } from "@/components/ui/button";

export const metadata = {
  title: "Home | AnimStudio",
  description: "Create stunning animations with AnimStudio."
};

export default function HomePage() {
  return (
    <main className="flex flex-col items-center justify-center min-h-screen bg-gray-100">
      <section className="text-center">
        <h1 className="text-4xl font-bold mb-4">Welcome to AnimStudio</h1>
        <p className="text-lg text-gray-600 mb-6">
          Start your animation journey with our intuitive platform.
        </p>
        <Button asChild variant="default">
          <a href="/signup">Get Started</a>
        </Button>
      </section>
    </main>
  );
}