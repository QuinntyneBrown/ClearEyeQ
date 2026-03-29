"use client";

import { useState } from "react";
import { Send } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import type { ClinicalNote } from "@/hooks/usePatients";

interface ClinicalNotesProps {
  notes: ClinicalNote[];
  onAddNote?: (content: string) => void;
}

export function ClinicalNotes({ notes, onAddNote }: ClinicalNotesProps) {
  const [newNote, setNewNote] = useState("");

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!newNote.trim()) return;
    onAddNote?.(newNote.trim());
    setNewNote("");
  }

  return (
    <div className="space-y-6">
      {/* Compose Form */}
      <form onSubmit={handleSubmit} className="space-y-3">
        <Textarea
          placeholder="Write a clinical note..."
          value={newNote}
          onChange={(e) => setNewNote(e.target.value)}
          className="min-h-[100px]"
        />
        <div className="flex justify-end">
          <Button type="submit" size="sm" disabled={!newNote.trim()}>
            <Send className="mr-2 h-4 w-4" />
            Add Note
          </Button>
        </div>
      </form>

      {/* Notes List */}
      <div className="space-y-4">
        {notes.length === 0 && (
          <p className="text-sm text-textTertiary">No clinical notes</p>
        )}
        {notes.map((note) => (
          <div
            key={note.id}
            className="rounded-lg border border-border p-4"
          >
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-medium text-textPrimary">
                {note.authorName}
              </span>
              <time className="text-xs text-textTertiary">
                {new Date(note.createdAt).toLocaleDateString("en-US", {
                  month: "short",
                  day: "numeric",
                  year: "numeric",
                  hour: "numeric",
                  minute: "2-digit",
                })}
              </time>
            </div>
            <p className="text-sm text-textSecondary whitespace-pre-wrap">
              {note.content}
            </p>
          </div>
        ))}
      </div>
    </div>
  );
}
