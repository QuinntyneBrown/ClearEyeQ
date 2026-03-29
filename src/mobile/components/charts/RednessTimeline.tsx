import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';

interface DataPoint {
  date: string;
  value: number;
}

interface RednessTimelineProps {
  data: DataPoint[];
  timeRange: '7D' | '30D' | '90D' | '1Y';
}

export function RednessTimeline({ data, timeRange }: RednessTimelineProps) {
  const maxValue = Math.max(...data.map((d) => d.value), 100);
  const yLabels = [0, 25, 50, 75, 100];

  return (
    <View style={styles.container}>
      <View style={styles.chartArea}>
        <View style={styles.yAxis}>
          {yLabels.reverse().map((label) => (
            <Text key={label} style={styles.yLabel}>
              {label}
            </Text>
          ))}
        </View>

        <View style={styles.plotArea}>
          {yLabels.map((label) => (
            <View key={`grid-${label}`} style={styles.gridLine} />
          ))}

          <View style={styles.barsContainer}>
            {data.map((point, index) => {
              const barHeight = (point.value / maxValue) * 100;
              let barColor = colors.scoreGreen;
              if (point.value >= 70) barColor = colors.scoreRed;
              else if (point.value >= 40) barColor = colors.scoreYellow;

              return (
                <View key={index} style={styles.barWrapper}>
                  <View
                    style={[
                      styles.bar,
                      {
                        height: `${barHeight}%`,
                        backgroundColor: barColor,
                      },
                    ]}
                  />
                </View>
              );
            })}
          </View>
        </View>
      </View>

      <View style={styles.xAxis}>
        {data.map((point, index) => (
          <Text key={index} style={styles.xLabel} numberOfLines={1}>
            {point.date}
          </Text>
        ))}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: colors.bgPage,
    borderRadius: radius.md,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.md,
  },
  chartArea: {
    flexDirection: 'row',
    height: 180,
  },
  yAxis: {
    width: 30,
    justifyContent: 'space-between',
    paddingRight: spacing.xs,
  },
  yLabel: {
    ...typography.labelSmall,
    color: colors.textTertiary,
    fontSize: 10,
    textAlign: 'right',
  },
  plotArea: {
    flex: 1,
    position: 'relative',
    justifyContent: 'space-between',
  },
  gridLine: {
    height: 1,
    backgroundColor: colors.border,
    opacity: 0.5,
  },
  barsContainer: {
    ...StyleSheet.absoluteFillObject,
    flexDirection: 'row',
    alignItems: 'flex-end',
    justifyContent: 'space-evenly',
    paddingHorizontal: spacing.xs,
  },
  barWrapper: {
    flex: 1,
    alignItems: 'center',
    height: '100%',
    justifyContent: 'flex-end',
    paddingHorizontal: 2,
  },
  bar: {
    width: '60%',
    borderRadius: 3,
    minHeight: 4,
  },
  xAxis: {
    flexDirection: 'row',
    justifyContent: 'space-evenly',
    marginTop: spacing.sm,
    paddingLeft: 30,
  },
  xLabel: {
    ...typography.labelSmall,
    color: colors.textTertiary,
    fontSize: 10,
    flex: 1,
    textAlign: 'center',
  },
});
