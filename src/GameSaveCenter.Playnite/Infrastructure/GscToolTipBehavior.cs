using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Keeps production tooltips dismissible with Escape without moving keyboard focus into a
    /// transient popup. It also mirrors the scoped theme resources into detached WPF surfaces and
    /// closes them when their plugin surface becomes hidden or unloaded. The scope is attached to
    /// the plugin surface so other Playnite extensions do not receive this behavior.
    /// </summary>
    public static class GscToolTipBehavior
    {
        public static readonly DependencyProperty EnableEscapeCloseProperty =
            DependencyProperty.RegisterAttached(
                "EnableEscapeClose",
                typeof(bool),
                typeof(GscToolTipBehavior),
                new PropertyMetadata(false, OnEnableEscapeCloseChanged));

        public static bool GetEnableEscapeClose(DependencyObject element)
            => (bool)element.GetValue(EnableEscapeCloseProperty);

        public static void SetEnableEscapeClose(DependencyObject element, bool value)
            => element.SetValue(EnableEscapeCloseProperty, value);

        private static void OnEnableEscapeCloseChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            if (!(dependencyObject is FrameworkElement element))
                return;

            if ((bool)args.NewValue)
            {
                element.PreviewKeyDown += OnPreviewKeyDown;
                element.IsVisibleChanged += OnIsVisibleChanged;
                element.Unloaded += OnUnloaded;
                element.Loaded += OnLoaded;
                element.AddHandler(ToolTipService.ToolTipOpeningEvent, new ToolTipEventHandler(OnToolTipOpening), true);
            }
            else
            {
                element.PreviewKeyDown -= OnPreviewKeyDown;
                element.IsVisibleChanged -= OnIsVisibleChanged;
                element.Unloaded -= OnUnloaded;
                element.Loaded -= OnLoaded;
                element.RemoveHandler(ToolTipService.ToolTipOpeningEvent, new ToolTipEventHandler(OnToolTipOpening));
                UnregisterToolTipHandlers(element);
            }
        }

        private static void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (sender is DependencyObject scope)
            {
                RegisterToolTipHandlers(scope);
                RefreshOpenTransientSurfaces(scope);
            }
        }

        private static void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue is bool isVisible && !isVisible && sender is DependencyObject scope)
                CloseOpenTransientSurfaces(scope);
        }

        private static void OnUnloaded(object sender, RoutedEventArgs args)
        {
            if (sender is DependencyObject scope)
            {
                CloseOpenTransientSurfaces(scope);
                UnregisterToolTipHandlers(scope);
            }
        }

        private static void OnToolTipOpening(object sender, ToolTipEventArgs args)
        {
            if (!(sender is DependencyObject scope))
                return;

            // ToolTipService creates a detached ToolTip after this routed event. Refresh once
            // now for an already materialized instance and once after the popup is opened for
            // string/object ToolTips created by the service.
            RefreshOpenTransientSurfaces(scope);
            if (scope.Dispatcher != null)
            {
                scope.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    if (scope is FrameworkElement element && !element.IsLoaded)
                        return;
                    RefreshOpenTransientSurfaces(scope);
                }));
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key != Key.Escape || args.Handled || !(sender is DependencyObject scope))
                return;

            if (!CloseOpenTooltips(scope))
                return;

            args.Handled = true;
        }

        private static bool CloseOpenTooltips(DependencyObject scope)
        {
            var openTooltips = new List<ToolTip>();
            foreach (PresentationSource source in PresentationSource.CurrentSources)
            {
                if (source.RootVisual is DependencyObject root)
                    CollectOpenTooltips(root, openTooltips);
            }

            var closed = false;
            foreach (var tooltip in openTooltips)
            {
                if (tooltip.PlacementTarget is UIElement target && IsWithinScope(target, scope))
                {
                    tooltip.IsOpen = false;
                    closed = true;
                }
            }

            return closed;
        }

        private static void CloseOpenTransientSurfaces(DependencyObject scope)
        {
            CloseOpenTooltips(scope);

            var openPopups = new List<Popup>();
            foreach (PresentationSource source in PresentationSource.CurrentSources)
            {
                if (source.RootVisual is DependencyObject root)
                    CollectOpenPopups(root, openPopups);
            }

            foreach (var popup in openPopups)
            {
                var placementTarget = popup.PlacementTarget;
                if ((placementTarget != null && IsWithinScope(placementTarget, scope))
                    || IsWithinScope(popup, scope))
                {
                    popup.IsOpen = false;
                }
            }
        }

        internal static void RefreshOpenTransientSurfaces(DependencyObject scope)
        {
            var openTooltips = new List<ToolTip>();
            var openPopups = new List<Popup>();
            foreach (PresentationSource source in PresentationSource.CurrentSources)
            {
                if (source.RootVisual is DependencyObject root)
                {
                    CollectOpenTooltips(root, openTooltips);
                    CollectOpenPopups(root, openPopups);
                }
            }

            foreach (var tooltip in openTooltips)
            {
                if (tooltip.PlacementTarget is DependencyObject target && IsWithinScope(target, scope))
                    ApplyScopedThemeResources(tooltip, scope);
            }

            foreach (var popup in openPopups)
            {
                var placementTarget = popup.PlacementTarget;
                if ((placementTarget != null && IsWithinScope(placementTarget, scope))
                    || IsWithinScope(popup, scope))
                {
                    ApplyScopedThemeResources(popup, scope);
                }
            }
        }

        private static void RegisterToolTipHandlers(DependencyObject root)
        {
            if (root is FrameworkElement element
                && ToolTipService.GetToolTip(element) is ToolTip tooltip)
            {
                tooltip.Opened -= OnToolTipOpened;
                tooltip.Opened += OnToolTipOpened;
            }

            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
                RegisterToolTipHandlers(VisualTreeHelper.GetChild(root, index));
        }

        private static void UnregisterToolTipHandlers(DependencyObject root)
        {
            if (root is FrameworkElement element
                && ToolTipService.GetToolTip(element) is ToolTip tooltip)
            {
                tooltip.Opened -= OnToolTipOpened;
            }

            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
                UnregisterToolTipHandlers(VisualTreeHelper.GetChild(root, index));
        }

        private static void OnToolTipOpened(object sender, RoutedEventArgs args)
        {
            if (!(sender is ToolTip tooltip)
                || !(tooltip.PlacementTarget is DependencyObject target))
                return;

            var scope = FindEnabledScope(target);
            if (scope != null)
                ApplyScopedThemeResources(tooltip, scope);
        }

        private static DependencyObject? FindEnabledScope(DependencyObject element)
        {
            var visited = new HashSet<DependencyObject>();
            var current = element;
            while (current != null && visited.Add(current))
            {
                if (current is FrameworkElement frameworkElement
                    && GetEnableEscapeClose(frameworkElement))
                    return frameworkElement;

                var visualParent = current is Visual || current is Visual3D
                    ? VisualTreeHelper.GetParent(current)
                    : null;
                current = visualParent ?? LogicalTreeHelper.GetParent(current);
            }

            return null;
        }

        private static void ApplyScopedThemeResources(FrameworkElement surface, DependencyObject scope)
        {
            if (!(scope is FrameworkElement frameworkScope))
                return;

            foreach (var key in ScopedThemeResourceKeys)
            {
                var value = frameworkScope.TryFindResource(key);
                if (value != null)
                    surface.Resources[key] = value;
            }
        }

        private static readonly string[] ScopedThemeResourceKeys =
        {
            "FloatingFillBrush",
            "FloatingStrokeBrush",
            "TextPrimaryBrush",
            "ShadowColor",
            "GscPrimaryTextBrush",
            "GscSecondaryTextBrush",
            "GscMutedTextBrush",
            "GscPopupBrush",
            "GscControlFillBrush",
            "GscControlStrokeBrush",
            "GscDividerBrush"
        };

        private static void CollectOpenPopups(DependencyObject root, ICollection<Popup> results)
        {
            if (root is Popup popup && popup.IsOpen)
                results.Add(popup);

            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
                CollectOpenPopups(VisualTreeHelper.GetChild(root, index), results);
        }

        private static void CollectOpenTooltips(DependencyObject root, ICollection<ToolTip> results)
        {
            if (root is ToolTip tooltip && tooltip.IsOpen)
                results.Add(tooltip);

            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
                CollectOpenTooltips(VisualTreeHelper.GetChild(root, index), results);
        }

        private static bool IsWithinScope(DependencyObject element, DependencyObject scope)
        {
            var visited = new HashSet<DependencyObject>();
            var current = element;
            while (current != null && visited.Add(current))
            {
                if (ReferenceEquals(current, scope))
                    return true;

                var visualParent = current is Visual || current is Visual3D
                    ? VisualTreeHelper.GetParent(current)
                    : null;
                current = visualParent ?? LogicalTreeHelper.GetParent(current);
            }

            return false;
        }
    }
}
