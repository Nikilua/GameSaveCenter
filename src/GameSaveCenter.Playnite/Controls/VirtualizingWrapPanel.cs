using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace GameSaveCenter.Playnite.Controls
{
    /// <summary>
    /// Fixed-size, vertically scrolling wrap panel for large selector lists.
    /// Unlike a plain WrapPanel this panel only generates containers intersecting
    /// the current viewport, while leaving the owning ListBox's production
    /// ScrollViewer and ScrollBar template in charge of visual scrolling.
    /// </summary>
    public sealed class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
    {
        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
            nameof(ItemWidth), typeof(double), typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(164d, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
            nameof(ItemHeight), typeof(double), typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(142d, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty HorizontalSpacingProperty = DependencyProperty.Register(
            nameof(HorizontalSpacing), typeof(double), typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(12d, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty VerticalSpacingProperty = DependencyProperty.Register(
            nameof(VerticalSpacing), typeof(double), typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(12d, FrameworkPropertyMetadataOptions.AffectsMeasure));

        private Size extent;
        private Size viewport;
        private double verticalOffset;
        private int columnCount = 1;
        private ScrollViewer? scrollOwner;
        private bool recoveryQueued;

        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public double HorizontalSpacing
        {
            get => (double)GetValue(HorizontalSpacingProperty);
            set => SetValue(HorizontalSpacingProperty, value);
        }

        public double VerticalSpacing
        {
            get => (double)GetValue(VerticalSpacingProperty);
            set => SetValue(VerticalSpacingProperty, value);
        }

        public bool CanHorizontallyScroll { get; set; }

        public bool CanVerticallyScroll { get; set; } = true;

        public double ExtentWidth => extent.Width;

        public double ExtentHeight => extent.Height;

        public double ViewportWidth => viewport.Width;

        public double ViewportHeight => viewport.Height;

        public double HorizontalOffset => 0;

        public double VerticalOffset => verticalOffset;

        public ScrollViewer? ScrollOwner
        {
            get => scrollOwner;
            set => scrollOwner = value;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var width = ResolveViewportWidth(availableSize.Width);
            // A ListBox can make its panel with an infinite height during the first
            // pass (especially after switching back to the media tab). Returning a
            // zero viewport here realizes only a partial range and can leave the
            // recycled visual tree empty after a scroll-back. Prefer the owner's
            // finite viewport, then the last measured/arranged size, and finally one
            // real card row so the first pass is always recoverable.
            var height = ResolveViewportHeight(availableSize.Height);
            var itemCount = ItemsControl.GetItemsOwner(this)?.Items.Count ?? 0;
            var itemWidth = Math.Max(1, ItemWidth);
            var itemHeight = Math.Max(1, ItemHeight);
            var horizontalSpacing = Math.Max(0, HorizontalSpacing);
            var verticalSpacing = Math.Max(0, VerticalSpacing);

            columnCount = Math.Max(1, (int)Math.Floor((width + horizontalSpacing) / (itemWidth + horizontalSpacing)));
            var rowCount = itemCount == 0 ? 0 : (itemCount + columnCount - 1) / columnCount;
            var contentWidth = columnCount * itemWidth + Math.Max(0, columnCount - 1) * horizontalSpacing;
            var contentHeight = rowCount == 0 ? 0 : rowCount * itemHeight + Math.Max(0, rowCount - 1) * verticalSpacing;

            viewport = new Size(width, height);
            extent = new Size(Math.Max(width, contentWidth), contentHeight);
            verticalOffset = ClampVerticalOffset(verticalOffset);

            if (itemCount == 0)
            {
                RemoveAllGeneratedChildren();
            }
            else
            {
                var firstIndex = GetFirstVisibleIndex(itemCount, itemHeight + verticalSpacing);
                var lastIndex = GetLastVisibleIndex(itemCount, height, itemHeight + verticalSpacing);
                RealizeRange(firstIndex, lastIndex);
            }

            foreach (UIElement child in InternalChildren)
                child.Measure(new Size(itemWidth, itemHeight));

            scrollOwner?.InvalidateScrollInfo();
            return new Size(width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var itemWidth = Math.Max(1, ItemWidth);
            var itemHeight = Math.Max(1, ItemHeight);
            var horizontalSpacing = Math.Max(0, HorizontalSpacing);
            var verticalSpacing = Math.Max(0, VerticalSpacing);
            var rowStep = itemHeight + verticalSpacing;
            var columnStep = itemWidth + horizontalSpacing;

            for (var childIndex = 0; childIndex < InternalChildren.Count; childIndex++)
            {
                var child = InternalChildren[childIndex];
                var itemIndex = IndexFromContainer(child);
                if (itemIndex < 0) continue;

                var row = itemIndex / columnCount;
                var column = itemIndex % columnCount;
                child.Arrange(new Rect(
                    column * columnStep,
                    row * rowStep - verticalOffset,
                    itemWidth,
                    itemHeight));
            }

            return finalSize;
        }

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                RemoveAllGeneratedChildren();
                QueueGenerationRecovery();
            }

            InvalidateMeasure();
            InvalidateArrange();
        }

        protected override void BringIndexIntoView(int index)
        {
            if (index < 0) return;
            var rowStep = Math.Max(1, ItemHeight + VerticalSpacing);
            SetVerticalOffset((index / Math.Max(1, columnCount)) * rowStep);
        }

        public void LineDown() => SetVerticalOffset(verticalOffset + Math.Max(1, ItemHeight + VerticalSpacing));

        public void LineLeft() { }

        public void LineRight() { }

        public void LineUp() => SetVerticalOffset(verticalOffset - Math.Max(1, ItemHeight + VerticalSpacing));

        public void MouseWheelDown() => SetVerticalOffset(verticalOffset + Math.Max(1, (ItemHeight + VerticalSpacing) * 0.75));

        public void MouseWheelLeft() { }

        public void MouseWheelRight() { }

        public void MouseWheelUp() => SetVerticalOffset(verticalOffset - Math.Max(1, (ItemHeight + VerticalSpacing) * 0.75));

        public void PageDown() => SetVerticalOffset(verticalOffset + Math.Max(1, viewport.Height - ItemHeight));

        public void PageLeft() { }

        public void PageRight() { }

        public void PageUp() => SetVerticalOffset(verticalOffset - Math.Max(1, viewport.Height - ItemHeight));

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            var container = visual as DependencyObject;
            while (container != null && IndexFromContainer(container) < 0)
                container = VisualTreeHelper.GetParent(container);

            if (container == null) return rectangle;
            var index = IndexFromContainer(container);
            if (index < 0) return rectangle;

            var rowStep = Math.Max(1, ItemHeight + VerticalSpacing);
            var top = (index / Math.Max(1, columnCount)) * rowStep;
            var bottom = top + ItemHeight;
            if (top < verticalOffset)
                SetVerticalOffset(top);
            else if (bottom > verticalOffset + viewport.Height)
                SetVerticalOffset(bottom - viewport.Height);
            return rectangle;
        }

        public void SetHorizontalOffset(double offset)
        {
            // Horizontal scrolling is deliberately disabled for the media cards.
            scrollOwner?.InvalidateScrollInfo();
        }

        public void SetVerticalOffset(double offset)
        {
            var clamped = ClampVerticalOffset(offset);
            if (Math.Abs(clamped - verticalOffset) < 0.1) return;
            verticalOffset = clamped;
            InvalidateMeasure();
            InvalidateArrange();
            scrollOwner?.InvalidateScrollInfo();
        }

        private double ResolveViewportWidth(double availableWidth)
        {
            if (!double.IsInfinity(availableWidth) && availableWidth > 1)
                return availableWidth;
            if (ActualWidth > 1)
                return ActualWidth;
            return Math.Max(1, ItemWidth);
        }

        private double ResolveViewportHeight(double availableHeight)
        {
            if (!double.IsInfinity(availableHeight) && availableHeight > 1)
                return availableHeight;
            if (scrollOwner != null && !double.IsInfinity(scrollOwner.ViewportHeight) && scrollOwner.ViewportHeight > 1)
                return scrollOwner.ViewportHeight;
            if (viewport.Height > 1 && !double.IsInfinity(viewport.Height))
                return viewport.Height;
            if (ActualHeight > 1 && !double.IsInfinity(ActualHeight))
                return ActualHeight;
            return Math.Max(1, ItemHeight + VerticalSpacing);
        }

        private int GetFirstVisibleIndex(int itemCount, double rowStep)
        {
            if (itemCount == 0) return 0;
            var row = Math.Max(0, (int)Math.Floor(verticalOffset / Math.Max(1, rowStep)) - 1);
            return Math.Min(itemCount - 1, row * Math.Max(1, columnCount));
        }

        private int GetLastVisibleIndex(int itemCount, double height, double rowStep)
        {
            if (itemCount == 0) return -1;
            var lastRow = Math.Max(0, (int)Math.Ceiling((verticalOffset + Math.Max(1, height)) / Math.Max(1, rowStep)));
            return Math.Min(itemCount - 1, (lastRow + 1) * Math.Max(1, columnCount) - 1);
        }

        private double ClampVerticalOffset(double offset)
        {
            var max = Math.Max(0, extent.Height - viewport.Height);
            if (double.IsNaN(offset) || double.IsInfinity(offset)) return 0;
            return Math.Max(0, Math.Min(offset, max));
        }

        private void RealizeRange(int firstIndex, int lastIndex)
        {
            var generator = ItemContainerGenerator;
            if (generator == null) return;
            if (lastIndex < firstIndex)
            {
                RemoveAllGeneratedChildren();
                return;
            }

            for (var childIndex = InternalChildren.Count - 1; childIndex >= 0; childIndex--)
            {
                var child = InternalChildren[childIndex];
                var itemIndex = IndexFromContainer(child);
                if (itemIndex >= firstIndex && itemIndex <= lastIndex) continue;
                ReleaseGeneratedChild(generator, childIndex);
                RemoveInternalChildRange(childIndex, 1);
            }

            var startPosition = generator.GeneratorPositionFromIndex(firstIndex);
            // When the first realized item is not currently represented by a
            // visual child, WPF returns GeneratorPosition(-1, 0). Passing -1
            // through to VisualCollection.Insert causes Playnite itself to
            // terminate with ArgumentOutOfRangeException during a resize or
            // page switch. Clamp the generator position to the current visual
            // collection; the generator still owns the item-index mapping.
            var insertionIndex = startPosition.Offset == 0
                ? startPosition.Index
                : startPosition.Index + 1;
            insertionIndex = Math.Max(0, Math.Min(insertionIndex, InternalChildren.Count));

            try
            {
                using (generator.StartAt(startPosition, GeneratorDirection.Forward, true))
                {
                    for (var index = firstIndex; index <= lastIndex; index++)
                    {
                        bool newlyRealized;
                        var child = generator.GenerateNext(out newlyRealized) as UIElement;
                        if (child == null) continue;
                        if (newlyRealized)
                        {
                            insertionIndex = Math.Max(0, Math.Min(insertionIndex, InternalChildren.Count));
                            InsertInternalChild(insertionIndex, child);
                            generator.PrepareItemContainer(child);
                        }
                        insertionIndex++;
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // A collection refresh can invalidate the generator between
                // MeasureOverride and GenerateNext. Clear the stale visual tree and
                // explicitly request a later generator pass. Without the deferred
                // pass WPF can keep the panel measured but empty after scrolling
                // back to the first row.
                RemoveAllGeneratedChildren();
                QueueGenerationRecovery();
            }
        }

        private void QueueGenerationRecovery()
        {
            if (recoveryQueued || !IsInitialized)
                return;

            recoveryQueued = true;
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                recoveryQueued = false;
                InvalidateMeasure();
                InvalidateArrange();
                scrollOwner?.InvalidateScrollInfo();
            }));
        }

        private void RemoveAllGeneratedChildren()
        {
            var generator = ItemContainerGenerator;
            if (generator == null)
            {
                RemoveInternalChildRange(0, InternalChildren.Count);
                return;
            }
            for (var childIndex = InternalChildren.Count - 1; childIndex >= 0; childIndex--)
            {
                ReleaseGeneratedChild(generator, childIndex);
                RemoveInternalChildRange(childIndex, 1);
            }
        }

        private static void ReleaseGeneratedChild(IItemContainerGenerator generator, int childIndex)
        {
            var position = new GeneratorPosition(childIndex, 0);
            try
            {
                if (generator is IRecyclingItemContainerGenerator recycling)
                    recycling.Recycle(position, 1);
                else
                    generator.Remove(position, 1);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentOutOfRangeException)
            {
                // A collection reset can update the generator before the visual
                // child cleanup reaches this panel; the visual tree is still safe
                // to clear in that case.
            }
        }

        private int IndexFromContainer(DependencyObject container)
        {
            var owner = ItemsControl.GetItemsOwner(this);
            return owner?.ItemContainerGenerator.IndexFromContainer(container) ?? -1;
        }
    }
}
