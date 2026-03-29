import type { Metadata } from "next";
import { Providers } from "./providers";
import { Sidebar } from "@/components/layout/Sidebar";
import { TopBar } from "@/components/layout/TopBar";
import "./globals.css";

export const metadata: Metadata = {
  title: "ClearEyeQ Clinical Portal",
  description: "Clinician-facing portal for ClearEyeQ eye health management",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" className="font-sans">
      <body className="min-h-screen bg-bgPage antialiased">
        <Providers>
          <div className="flex min-h-screen">
            <Sidebar />
            <div className="flex-1 pl-[240px]">
              <TopBar />
              <main className="p-6">{children}</main>
            </div>
          </div>
        </Providers>
      </body>
    </html>
  );
}
