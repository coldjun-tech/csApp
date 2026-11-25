using System;
using System.Collections.Generic;

namespace SampleChartData
{
    public enum ChartPattern
    {
        Flat,       // 최고점 이후 일정하게 유지
        LinearDrop, // 최고점 이후 지속적으로 감소
        StepDrop    // 최고점 이후 계단식으로 감소
    }

    public sealed class ChartSample
    {
        public double X { get; set; }
        public double First { get; set; }  // 1차: 초록색
        public double Second { get; set; } // 2차: 빨간색
    }

    public static class ChartDataGenerator
    {
        // 같은 seed를 지정하면 같은 데이터를 재현합니다.
        // peak: 원하는 최고값. 단위는 사용하는 차트의 단위를 따릅니다.
        public static List<ChartSample> Generate(
            double peak = 0.7,
            ChartPattern pattern = ChartPattern.StepDrop,
            int? seed = null)
        {
            if (double.IsNaN(peak) || double.IsInfinity(peak) || peak <= 0)
                throw new ArgumentOutOfRangeException("peak");
            if (!Enum.IsDefined(typeof(ChartPattern), pattern))
                throw new ArgumentOutOfRangeException("pattern");

            var random = new Random(seed ?? Guid.NewGuid().GetHashCode());
            var result = new List<ChartSample>(121);

            int firstStart = random.Next(12, 19);
            int secondStart = random.Next(12, 19);
            int peakX = random.Next(27, 32);
            int settleX = peakX + random.Next(1, 5);

            double firstPeak = peak * Between(random, 0.97, 1.03);
            double secondPeak = peak * Between(random, 0.97, 1.03);
            double secondHold = secondPeak * Between(random, 0.90, 0.96);
            double firstHold = firstPeak * (pattern == ChartPattern.Flat
                ? Between(random, 0.90, 0.96)
                : Between(random, 0.55, 0.85));
            double firstEnd = pattern == ChartPattern.Flat
                ? firstHold
                : firstHold * (pattern == ChartPattern.LinearDrop
                    ? Between(random, 0.05, 0.80)
                    : Between(random, 0.65, 0.92));

            double firstCurve = Between(random, 0.85, 1.25);
            double secondCurve = Between(random, 0.85, 1.25);
            int stepCount = random.Next(5, 9);

            for (int x = 0; x <= 120; x++)
            {
                double first;
                if (x <= peakX)
                    first = Rise(x, firstStart, peakX, firstPeak, firstCurve);
                else if (x <= settleX)
                    first = Mix(firstPeak, firstHold,
                        (double)(x - peakX) / (settleX - peakX));
                else
                {
                    double progress = (double)(x - settleX) / (120 - settleX);
                    if (pattern == ChartPattern.StepDrop)
                        progress = Math.Floor(progress * stepCount) / stepCount;
                    first = Mix(firstHold, firstEnd, progress);
                }

                double second = x <= peakX
                    ? Rise(x, secondStart, peakX, secondPeak, secondCurve)
                    : secondHold;

                result.Add(new ChartSample
                {
                    X = x,
                    First = first,
                    Second = second
                });
            }

            return result;
        }

        private static double Rise(int x, int start, int end, double peak, double curve)
        {
            if (x <= start) return 0;
            double progress = (double)(x - start) / (end - start);
            return peak * Math.Pow(progress, curve);
        }

        private static double Mix(double from, double to, double progress)
        {
            return from + (to - from) * progress;
        }

        private static double Between(Random random, double min, double max)
        {
            return min + random.NextDouble() * (max - min);
        }
    }
}
