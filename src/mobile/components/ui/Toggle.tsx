import React, { useEffect, useRef } from 'react';
import { TouchableOpacity, Animated, StyleSheet, View, Text } from 'react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing } from '@/theme/spacing';

interface ToggleProps {
  value: boolean;
  onValueChange: (value: boolean) => void;
  label?: string;
  description?: string;
  disabled?: boolean;
}

const TRACK_WIDTH = 50;
const TRACK_HEIGHT = 30;
const THUMB_SIZE = 26;
const THUMB_OFFSET = 2;

export function Toggle({
  value,
  onValueChange,
  label,
  description,
  disabled = false,
}: ToggleProps) {
  const translateX = useRef(new Animated.Value(value ? TRACK_WIDTH - THUMB_SIZE - THUMB_OFFSET : THUMB_OFFSET)).current;

  useEffect(() => {
    Animated.spring(translateX, {
      toValue: value ? TRACK_WIDTH - THUMB_SIZE - THUMB_OFFSET : THUMB_OFFSET,
      useNativeDriver: true,
      tension: 100,
      friction: 10,
    }).start();
  }, [value, translateX]);

  return (
    <TouchableOpacity
      style={styles.container}
      activeOpacity={0.7}
      disabled={disabled}
      onPress={() => onValueChange(!value)}
    >
      {(label || description) && (
        <View style={styles.textContainer}>
          {label && <Text style={styles.label}>{label}</Text>}
          {description && <Text style={styles.description}>{description}</Text>}
        </View>
      )}
      <View
        style={[
          styles.track,
          { backgroundColor: value ? colors.primary : colors.border },
          disabled && styles.disabled,
        ]}
      >
        <Animated.View
          style={[
            styles.thumb,
            { transform: [{ translateX }] },
          ]}
        />
      </View>
    </TouchableOpacity>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: spacing.sm,
  },
  textContainer: {
    flex: 1,
    marginRight: spacing.md,
  },
  label: {
    ...typography.body,
    color: colors.textPrimary,
  },
  description: {
    ...typography.caption,
    color: colors.textSecondary,
    marginTop: 2,
  },
  track: {
    width: TRACK_WIDTH,
    height: TRACK_HEIGHT,
    borderRadius: TRACK_HEIGHT / 2,
    justifyContent: 'center',
  },
  thumb: {
    width: THUMB_SIZE,
    height: THUMB_SIZE,
    borderRadius: THUMB_SIZE / 2,
    backgroundColor: colors.white,
    shadowColor: colors.black,
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.2,
    shadowRadius: 2,
    elevation: 2,
  },
  disabled: {
    opacity: 0.5,
  },
});
