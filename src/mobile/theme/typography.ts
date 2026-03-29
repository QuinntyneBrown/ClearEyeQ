import { TextStyle } from 'react-native';

export const fontFamily = {
  regular: 'Inter_400Regular',
  medium: 'Inter_500Medium',
  semiBold: 'Inter_600SemiBold',
  bold: 'Inter_700Bold',
} as const;

export const fontWeights = {
  regular: '400' as TextStyle['fontWeight'],
  medium: '500' as TextStyle['fontWeight'],
  semiBold: '600' as TextStyle['fontWeight'],
  bold: '700' as TextStyle['fontWeight'],
};

export const typography = {
  heading1: {
    fontSize: 28,
    lineHeight: 34,
    fontWeight: fontWeights.bold,
  } as TextStyle,
  heading2: {
    fontSize: 22,
    lineHeight: 28,
    fontWeight: fontWeights.semiBold,
  } as TextStyle,
  heading3: {
    fontSize: 18,
    lineHeight: 24,
    fontWeight: fontWeights.semiBold,
  } as TextStyle,
  body: {
    fontSize: 16,
    lineHeight: 24,
    fontWeight: fontWeights.regular,
  } as TextStyle,
  bodyMedium: {
    fontSize: 16,
    lineHeight: 24,
    fontWeight: fontWeights.medium,
  } as TextStyle,
  caption: {
    fontSize: 13,
    lineHeight: 18,
    fontWeight: fontWeights.regular,
  } as TextStyle,
  label: {
    fontSize: 14,
    lineHeight: 20,
    fontWeight: fontWeights.medium,
  } as TextStyle,
  labelSmall: {
    fontSize: 12,
    lineHeight: 16,
    fontWeight: fontWeights.medium,
  } as TextStyle,
} as const;

export type TypographyVariant = keyof typeof typography;
