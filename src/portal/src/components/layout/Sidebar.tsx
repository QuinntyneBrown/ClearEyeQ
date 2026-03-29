"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  Home,
  Users,
  ClipboardCheck,
  Inbox,
  Settings,
  Eye,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Badge } from "@/components/ui/badge";

interface NavItem {
  label: string;
  href: string;
  icon: React.ComponentType<{ className?: string }>;
}

const navItems: NavItem[] = [
  { label: "Dashboard", href: "/", icon: Home },
  { label: "Patients", href: "/patients", icon: Users },
  { label: "Treatment Reviews", href: "/treatment-reviews", icon: ClipboardCheck },
  { label: "Referrals", href: "/referrals", icon: Inbox },
  { label: "Settings", href: "/settings", icon: Settings },
];

export function Sidebar() {
  const pathname = usePathname();

  function isActive(href: string): boolean {
    if (href === "/") return pathname === "/";
    return pathname.startsWith(href);
  }

  return (
    <aside className="fixed left-0 top-0 z-40 flex h-screen w-[240px] flex-col border-r border-border bg-white">
      {/* Logo */}
      <div className="flex items-center gap-2 px-6 py-5">
        <div className="flex h-8 w-8 items-center justify-center rounded-md bg-primary">
          <Eye className="h-5 w-5 text-white" />
        </div>
        <div className="flex items-center gap-2">
          <span className="text-lg font-bold text-textPrimary">ClearEyeQ</span>
          <Badge variant="default" className="text-[10px] px-1.5 py-0">
            Clinical
          </Badge>
        </div>
      </div>

      {/* Navigation */}
      <nav className="flex-1 space-y-1 px-3 py-4">
        {navItems.map((item) => {
          const active = isActive(item.href);
          const Icon = item.icon;

          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex items-center gap-3 rounded-md px-3 py-2.5 text-sm font-medium transition-colors",
                active
                  ? "bg-primary-light text-primary"
                  : "text-textSecondary hover:bg-bgSurface hover:text-textPrimary"
              )}
            >
              <Icon className={cn("h-5 w-5", active ? "text-primary" : "text-textSecondary")} />
              {item.label}
            </Link>
          );
        })}
      </nav>

      {/* Footer */}
      <div className="border-t border-border px-6 py-4">
        <p className="text-xs text-textTertiary">ClearEyeQ Clinical Portal</p>
        <p className="text-xs text-textTertiary">v0.1.0</p>
      </div>
    </aside>
  );
}
