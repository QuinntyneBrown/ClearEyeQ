#!/usr/bin/env python3
"""Render all .puml files in docs/detailed-design/ to .png using PlantUML server."""
import os
import sys
import plantuml

SERVER = "http://www.plantuml.com/plantuml/png/"

def render_all(root_dir=None):
    if root_dir is None:
        root_dir = os.path.dirname(os.path.abspath(__file__))

    p = plantuml.PlantUML(url=SERVER)
    rendered = 0
    errors = 0

    for dirpath, _, filenames in os.walk(root_dir):
        for fname in sorted(filenames):
            if not fname.endswith(".puml"):
                continue
            puml_path = os.path.join(dirpath, fname)
            png_path = puml_path.replace(".puml", ".png")

            # Skip if png is newer than puml
            if os.path.exists(png_path):
                if os.path.getmtime(png_path) >= os.path.getmtime(puml_path):
                    print(f"  SKIP (up-to-date): {os.path.relpath(puml_path, root_dir)}")
                    continue

            print(f"  Rendering: {os.path.relpath(puml_path, root_dir)} ... ", end="", flush=True)
            try:
                p.processes_file(puml_path, outfile=png_path)
                if os.path.exists(png_path) and os.path.getsize(png_path) > 0:
                    print(f"OK ({os.path.getsize(png_path)} bytes)")
                    rendered += 1
                else:
                    print("FAILED (empty output)")
                    errors += 1
            except Exception as e:
                print(f"ERROR: {e}")
                errors += 1

    print(f"\nDone: {rendered} rendered, {errors} errors")
    return errors

if __name__ == "__main__":
    root = sys.argv[1] if len(sys.argv) > 1 else None
    sys.exit(render_all(root))
