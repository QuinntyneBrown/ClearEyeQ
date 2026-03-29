"use client";

import { useState } from "react";
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { useAuth } from "@/hooks/useAuth";

interface NotificationPreference {
  id: string;
  label: string;
  description: string;
  enabled: boolean;
}

export default function SettingsPage() {
  const { user } = useAuth();

  const [name, setName] = useState(user?.name || "Dr. Thompson");
  const [email, setEmail] = useState(user?.email || "thompson@cleareyeq.com");

  const [notifications, setNotifications] = useState<NotificationPreference[]>([
    {
      id: "new-referral",
      label: "New Referrals",
      description: "Get notified when a new patient referral arrives",
      enabled: true,
    },
    {
      id: "treatment-review",
      label: "Treatment Reviews",
      description: "Get notified when a treatment plan needs your review",
      enabled: true,
    },
    {
      id: "scan-results",
      label: "Scan Results",
      description: "Get notified when new scan results are available for your patients",
      enabled: true,
    },
    {
      id: "patient-alerts",
      label: "Patient Alerts",
      description: "Get notified about urgent patient status changes",
      enabled: true,
    },
    {
      id: "weekly-digest",
      label: "Weekly Digest",
      description: "Receive a weekly summary of patient activity",
      enabled: false,
    },
    {
      id: "system-updates",
      label: "System Updates",
      description: "Get notified about platform updates and maintenance",
      enabled: false,
    },
  ]);

  function toggleNotification(id: string) {
    setNotifications((prev) =>
      prev.map((n) => (n.id === id ? { ...n, enabled: !n.enabled } : n))
    );
  }

  return (
    <div className="max-w-2xl space-y-6">
      {/* Profile */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Clinician Profile</CardTitle>
          <CardDescription>
            Manage your account information
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <Input
            label="Full Name"
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
          <Input
            label="Email Address"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
          <Input
            label="Role"
            value="Clinician"
            disabled
          />
          <div className="flex justify-end pt-2">
            <Button>Save Changes</Button>
          </div>
        </CardContent>
      </Card>

      {/* Notification Preferences */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Notification Preferences</CardTitle>
          <CardDescription>
            Choose which notifications you want to receive
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-0">
            {notifications.map((pref, index) => (
              <div key={pref.id}>
                {index > 0 && <Separator className="my-4" />}
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium text-textPrimary">
                      {pref.label}
                    </p>
                    <p className="text-xs text-textSecondary">
                      {pref.description}
                    </p>
                  </div>
                  <button
                    onClick={() => toggleNotification(pref.id)}
                    className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                      pref.enabled ? "bg-primary" : "bg-border"
                    }`}
                    role="switch"
                    aria-checked={pref.enabled}
                  >
                    <span
                      className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                        pref.enabled ? "translate-x-6" : "translate-x-1"
                      }`}
                    />
                  </button>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
