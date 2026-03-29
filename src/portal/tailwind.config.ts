import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./src/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: "#2563EB",
          light: "#EFF6FF",
        },
        bgPage: "#FFFFFF",
        bgSurface: "#F9FAFB",
        textPrimary: "#18181B",
        textSecondary: "#71717A",
        textTertiary: "#A1A1AA",
        border: "#E4E4E7",
        success: "#16A34A",
        warning: "#F59E0B",
        error: "#DC2626",
      },
      spacing: {
        xs: "4px",
        sm: "8px",
        md: "16px",
        lg: "24px",
        xl: "32px",
      },
      borderRadius: {
        sm: "6px",
        md: "12px",
        lg: "16px",
      },
      fontFamily: {
        sans: ["Inter", "system-ui", "sans-serif"],
      },
    },
  },
  plugins: [],
};

export default config;
