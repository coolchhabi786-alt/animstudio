import Link from "next/link";

export const metadata = {
  title: "AnimStudio - Create Stunning Animations",
  description: "Simplify your animation workflow with AnimStudio. Create projects, manage episodes, and bring your ideas to life.",
};

export default async function HomePage() {
  return (
    <main className="flex flex-col items-center justify-center min-h-screen bg-gradient-to-b from-blue-50 to-white">
      <section className="text-center py-10">
        <h1 className="text-4xl font-bold text-blue-800">
          Welcome to AnimStudio
        </h1>
        <p className="mt-4 text-lg text-gray-600">
          Streamline your animation workflow and create stunning episodes.
        </p>
        <div className="mt-6">
          <Link
            href="/signup"
            className="px-6 py-3 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring focus:ring-blue-300"
            aria-label="Sign up for AnimStudio"
          >
            Get Started
          </Link>
        </div>
      </section>
    </main>
  );
}