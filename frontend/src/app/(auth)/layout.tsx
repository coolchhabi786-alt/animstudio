import { ReactNode } from "react";

export const metadata = {
  title: "Auth | AnimStudio",
};

type Props = {
  children: ReactNode;
};

export default function AuthLayout({ children }: Props) {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      {children}
    </div>
  );
}