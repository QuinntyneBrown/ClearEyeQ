import React from 'react';
import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { Tabs } from 'expo-router';
import { Home, ScanEye, ChartNoAxesColumn, Pill, Settings } from 'lucide-react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';

interface TabIconProps {
  icon: React.ReactNode;
  label: string;
  focused: boolean;
}

function TabIcon({ icon, label, focused }: TabIconProps) {
  return (
    <View style={[styles.tabItem, focused && styles.tabItemActive]}>
      {icon}
      <Text style={[styles.tabLabel, focused && styles.tabLabelActive]}>{label}</Text>
    </View>
  );
}

export default function TabLayout() {
  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarStyle: styles.tabBar,
        tabBarShowLabel: false,
      }}
    >
      <Tabs.Screen
        name="index"
        options={{
          title: 'Home',
          tabBarIcon: ({ focused }) => (
            <TabIcon
              icon={<Home size={20} color={focused ? colors.textOnAccent : colors.textTertiary} />}
              label="Home"
              focused={focused}
            />
          ),
        }}
      />
      <Tabs.Screen
        name="scan"
        options={{
          title: 'Scan',
          tabBarIcon: ({ focused }) => (
            <TabIcon
              icon={<ScanEye size={20} color={focused ? colors.textOnAccent : colors.textTertiary} />}
              label="Scan"
              focused={focused}
            />
          ),
        }}
      />
      <Tabs.Screen
        name="timeline"
        options={{
          title: 'Trends',
          tabBarIcon: ({ focused }) => (
            <TabIcon
              icon={<ChartNoAxesColumn size={20} color={focused ? colors.textOnAccent : colors.textTertiary} />}
              label="Trends"
              focused={focused}
            />
          ),
        }}
      />
      <Tabs.Screen
        name="treatment"
        options={{
          title: 'Treat',
          tabBarIcon: ({ focused }) => (
            <TabIcon
              icon={<Pill size={20} color={focused ? colors.textOnAccent : colors.textTertiary} />}
              label="Treat"
              focused={focused}
            />
          ),
        }}
      />
      <Tabs.Screen
        name="settings"
        options={{
          title: 'More',
          tabBarIcon: ({ focused }) => (
            <TabIcon
              icon={<Settings size={20} color={focused ? colors.textOnAccent : colors.textTertiary} />}
              label="More"
              focused={focused}
            />
          ),
        }}
      />
    </Tabs>
  );
}

const styles = StyleSheet.create({
  tabBar: {
    position: 'absolute',
    bottom: spacing.lg,
    left: spacing.md,
    right: spacing.md,
    height: 70,
    borderRadius: radius.pill,
    backgroundColor: colors.bgPage,
    borderWidth: 1,
    borderColor: colors.border,
    shadowColor: colors.black,
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.08,
    shadowRadius: 12,
    elevation: 8,
    paddingBottom: 0,
    paddingTop: 0,
    flexDirection: 'row',
    alignItems: 'center',
  },
  tabItem: {
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: spacing.sm + 4,
    paddingVertical: spacing.sm,
    borderRadius: radius.pill,
    minWidth: 56,
  },
  tabItemActive: {
    backgroundColor: colors.primary,
  },
  tabLabel: {
    ...typography.labelSmall,
    color: colors.textTertiary,
    marginTop: 2,
    fontSize: 10,
  },
  tabLabelActive: {
    color: colors.textOnAccent,
  },
});
