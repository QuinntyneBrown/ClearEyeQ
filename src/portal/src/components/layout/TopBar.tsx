"use client";

import { usePathname } from "next/navigation";
import { Search, Bell, User, LogOut, Settings } from "lucide-react";
import { Input } from "@/components/ui/input";
import {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuLabel,
} from "@/components/ui/dropdown-menu";
import { useAuth } from "@/hooks/useAuth";

const pageTitles: Record<string, string> = {
  "/": "Dashboard",
  "/patients": "Patients",
  "/referrals": "Referrals",
  "/treatment-reviews": "Treatment Reviews",
  "/settings": "Settings",
};

function getPageTitle(pathname: string): string {
  if (pageTitles[pathname]) return pageTitles[pathname];
  if (pathname.startsWith("/patients/")) return "Patient Detail";
  return "Dashboard";
}

export function TopBar() {
  const pathname = usePathname();
  const { user, logout } = useAuth();
  const title = getPageTitle(pathname);

  return (
    <header className="sticky top-0 z-30 flex h-16 items-center justify-between border-b border-border bg-white px-6">
      {/* Page Title */}
      <h1 className="text-xl font-semibold text-textPrimary">{title}</h1>

      {/* Right Section */}
      <div className="flex items-center gap-4">
        {/* Search */}
        <div className="relative w-64">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-textTertiary" />
          <input
            type="text"
            placeholder="Search patients..."
            className="h-9 w-full rounded-md border border-border bg-bgSurface pl-9 pr-3 text-sm text-textPrimary placeholder:text-textTertiary focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-1"
          />
        </div>

        {/* Notifications */}
        <button className="relative rounded-md p-2 text-textSecondary hover:bg-bgSurface hover:text-textPrimary transition-colors">
          <Bell className="h-5 w-5" />
          <span className="absolute -right-0.5 -top-0.5 flex h-4 w-4 items-center justify-center rounded-full bg-error text-[10px] font-bold text-white">
            3
          </span>
        </button>

        {/* User Menu */}
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <button className="flex items-center gap-2 rounded-md p-1.5 hover:bg-bgSurface transition-colors">
              <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary text-white text-sm font-medium">
                {user?.name?.charAt(0)?.toUpperCase() || "C"}
              </div>
              <span className="text-sm font-medium text-textPrimary hidden lg:inline">
                {user?.name || "Clinician"}
              </span>
            </button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-56">
            <DropdownMenuLabel>
              <div className="flex flex-col">
                <span className="text-sm font-medium">{user?.name || "Clinician"}</span>
                <span className="text-xs text-textSecondary">{user?.email || "clinician@cleareyeq.com"}</span>
              </div>
            </DropdownMenuLabel>
            <DropdownMenuSeparator />
            <DropdownMenuItem>
              <User className="mr-2 h-4 w-4" />
              Profile
            </DropdownMenuItem>
            <DropdownMenuItem>
              <Settings className="mr-2 h-4 w-4" />
              Settings
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem onClick={logout} className="text-error">
              <LogOut className="mr-2 h-4 w-4" />
              Logout
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </header>
  );
}
