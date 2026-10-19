using GameSaveCenter.Playnite.Controls;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class MediaComparisonViewportStateTests
    {
        [Fact]
        public void SynchronizedZoomIsClampedAndAppliedToBothPanes()
        {
            var state = new MediaComparisonViewportState();

            state.SetZoom(MediaComparisonPane.First, 99d);

            Assert.Equal(MediaComparisonViewportState.MaximumZoom, state.GetZoom(MediaComparisonPane.First, 0.5d));
            Assert.Equal(MediaComparisonViewportState.MaximumZoom, state.GetZoom(MediaComparisonPane.Second, 0.4d));
            state.SetZoom(MediaComparisonPane.Second, 0.01d);
            Assert.Equal(MediaComparisonViewportState.MinimumZoom, state.GetZoom(MediaComparisonPane.First, 0.5d));
            Assert.Equal(MediaComparisonViewportState.MinimumZoom, state.GetZoom(MediaComparisonPane.Second, 0.4d));
        }

        [Fact]
        public void IndependentZoomKeepsPaneScalesSeparateUntilSynchronizationIsEnabled()
        {
            var state = new MediaComparisonViewportState();
            state.SetSynchronized(false);
            state.SetZoom(MediaComparisonPane.First, 1.5d);
            state.SetZoom(MediaComparisonPane.Second, 0.75d);

            Assert.Equal(1.5d, state.GetZoom(MediaComparisonPane.First, 0.4d));
            Assert.Equal(0.75d, state.GetZoom(MediaComparisonPane.Second, 0.4d));

            state.SetSynchronized(true);

            Assert.Equal(1.5d, state.GetZoom(MediaComparisonPane.First, 0.4d));
            Assert.Equal(1.5d, state.GetZoom(MediaComparisonPane.Second, 0.4d));
        }

        [Fact]
        public void FitAndOneToOneResetBothPanesAndCenters()
        {
            var state = new MediaComparisonViewportState();
            state.SetZoom(MediaComparisonPane.First, 2d);
            state.SetCenter(MediaComparisonPane.First, 0.8d, 0.2d);

            state.FitBoth();
            Assert.True(state.IsFit(MediaComparisonPane.First));
            Assert.True(state.IsFit(MediaComparisonPane.Second));
            Assert.Equal(0.5d, state.GetCenterX(MediaComparisonPane.Second));
            Assert.Equal(0.5d, state.GetCenterY(MediaComparisonPane.Second));

            state.SetOneToOne();
            Assert.False(state.IsFit(MediaComparisonPane.First));
            Assert.False(state.IsFit(MediaComparisonPane.Second));
            Assert.Equal(1d, state.GetZoom(MediaComparisonPane.First, 0.25d));
            Assert.Equal(1d, state.GetZoom(MediaComparisonPane.Second, 0.35d));
        }

        [Fact]
        public void NormalizedCenterMapsAcrossDifferentImageAndViewportDimensions()
        {
            var firstCenter = MediaComparisonViewportState.GetNormalizedCenter(300d, 400d, 1400d);
            var secondOffset = MediaComparisonViewportState.GetOffset(firstCenter, 250d, 900d);
            var secondCenter = MediaComparisonViewportState.GetNormalizedCenter(secondOffset, 250d, 900d);

            Assert.Equal(0.357142857d, firstCenter, 6);
            Assert.Equal(firstCenter, secondCenter, 6);
            Assert.Equal(0d, MediaComparisonViewportState.GetOffset(0d, 400d, 900d));
            Assert.Equal(500d, MediaComparisonViewportState.GetOffset(1d, 400d, 900d));
        }
    }
}
