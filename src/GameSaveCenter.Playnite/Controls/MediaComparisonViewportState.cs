using System;

namespace GameSaveCenter.Playnite.Controls
{
    internal enum MediaComparisonPane
    {
        First,
        Second
    }

    /// <summary>
    /// Stores comparison zoom and normalized image-center state independently from WPF
    /// ScrollViewer offsets, so screenshots with different dimensions stay aligned.
    /// </summary>
    internal sealed class MediaComparisonViewportState
    {
        internal const double MinimumZoom = 0.125d;
        internal const double MaximumZoom = 4d;
        private const double ZoomStep = 1.25d;

        private bool firstFit = true;
        private bool secondFit = true;
        private double firstZoom = 1d;
        private double secondZoom = 1d;
        private double firstCenterX = 0.5d;
        private double firstCenterY = 0.5d;
        private double secondCenterX = 0.5d;
        private double secondCenterY = 0.5d;

        public bool IsSynchronized { get; private set; } = true;

        public bool IsFit(MediaComparisonPane pane)
            => pane == MediaComparisonPane.First ? firstFit : secondFit;

        public double GetZoom(MediaComparisonPane pane, double fitZoom)
            => IsFit(pane) ? fitZoom : pane == MediaComparisonPane.First ? firstZoom : secondZoom;

        public double GetCenterX(MediaComparisonPane pane)
            => pane == MediaComparisonPane.First ? firstCenterX : secondCenterX;

        public double GetCenterY(MediaComparisonPane pane)
            => pane == MediaComparisonPane.First ? firstCenterY : secondCenterY;

        public void SetSynchronized(bool synchronized)
        {
            if (IsSynchronized == synchronized) return;
            IsSynchronized = synchronized;
            if (!synchronized) return;

            secondFit = firstFit;
            secondZoom = firstZoom;
            secondCenterX = firstCenterX;
            secondCenterY = firstCenterY;
        }

        public void FitBoth()
        {
            firstFit = true;
            secondFit = true;
            firstCenterX = secondCenterX = 0.5d;
            firstCenterY = secondCenterY = 0.5d;
        }

        public void SetOneToOne()
        {
            firstFit = false;
            secondFit = false;
            firstZoom = secondZoom = 1d;
            firstCenterX = secondCenterX = 0.5d;
            firstCenterY = secondCenterY = 0.5d;
        }

        public void AdjustZoom(MediaComparisonPane pane, int direction, double fitZoom)
        {
            if (direction == 0) return;
            var current = GetZoom(pane, fitZoom);
            var factor = Math.Pow(ZoomStep, Math.Sign(direction));
            SetZoom(pane, ClampZoom(current * factor));
        }

        public void SetZoom(MediaComparisonPane pane, double zoom)
        {
            var bounded = ClampZoom(zoom);
            if (pane == MediaComparisonPane.First)
            {
                firstFit = false;
                firstZoom = bounded;
                if (IsSynchronized)
                {
                    secondFit = false;
                    secondZoom = bounded;
                }
            }
            else
            {
                secondFit = false;
                secondZoom = bounded;
                if (IsSynchronized)
                {
                    firstFit = false;
                    firstZoom = bounded;
                }
            }
        }

        public bool SetCenter(MediaComparisonPane pane, double centerX, double centerY)
        {
            var boundedX = ClampUnit(centerX);
            var boundedY = ClampUnit(centerY);
            var oldX = GetCenterX(pane);
            var oldY = GetCenterY(pane);
            if (Math.Abs(oldX - boundedX) < 0.0001d && Math.Abs(oldY - boundedY) < 0.0001d)
                return false;

            if (pane == MediaComparisonPane.First)
            {
                firstCenterX = boundedX;
                firstCenterY = boundedY;
                if (IsSynchronized)
                {
                    secondCenterX = boundedX;
                    secondCenterY = boundedY;
                }
            }
            else
            {
                secondCenterX = boundedX;
                secondCenterY = boundedY;
                if (IsSynchronized)
                {
                    firstCenterX = boundedX;
                    firstCenterY = boundedY;
                }
            }

            return true;
        }

        public static double GetNormalizedCenter(double offset, double viewport, double extent)
        {
            if (extent <= viewport || extent <= 0 || viewport <= 0) return 0.5d;
            return ClampUnit((offset + viewport / 2d) / extent);
        }

        public static double GetOffset(double normalizedCenter, double viewport, double extent)
        {
            var maximum = Math.Max(0d, extent - viewport);
            return Math.Max(0d, Math.Min(maximum, ClampUnit(normalizedCenter) * extent - viewport / 2d));
        }

        private static double ClampZoom(double zoom)
            => Math.Max(MinimumZoom, Math.Min(MaximumZoom, double.IsNaN(zoom) || double.IsInfinity(zoom) ? 1d : zoom));

        private static double ClampUnit(double value)
            => Math.Max(0d, Math.Min(1d, double.IsNaN(value) || double.IsInfinity(value) ? 0.5d : value));
    }
}
