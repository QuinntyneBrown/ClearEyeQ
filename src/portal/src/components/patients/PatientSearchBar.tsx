"use client";

import { Search } from "lucide-react";
import { cn } from "@/lib/utils";

interface PatientSearchBarProps {
  search: string;
  onSearchChange: (value: string) => void;
  activeFilter: string;
  onFilterChange: (value: string) => void;
}

const filters = ["All", "Flagged", "Stable", "Urgent", "Active"];

export function PatientSearchBar({
  search,
  onSearchChange,
  activeFilter,
  onFilterChange,
}: PatientSearchBarProps) {
  return (
    <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      {/* Search Input */}
      <div className="relative w-full max-w-sm">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-textTertiary" />
        <input
          type="text"
          placeholder="Search patients by name or email..."
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          className="h-10 w-full rounded-md border border-border bg-white pl-9 pr-3 text-sm text-textPrimary placeholder:text-textTertiary focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-1"
        />
      </div>

      {/* Filter Chips */}
      <div className="flex gap-2 flex-wrap">
        {filters.map((filter) => (
          <button
            key={filter}
            onClick={() => onFilterChange(filter)}
            className={cn(
              "rounded-full px-3 py-1.5 text-xs font-medium transition-colors",
              activeFilter === filter
                ? "bg-primary text-white"
                : "bg-bgSurface text-textSecondary hover:bg-border/50"
            )}
          >
            {filter}
          </button>
        ))}
      </div>
    </div>
  );
}
