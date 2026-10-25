using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Automation;

namespace GameSaveCenter.Playnite.Tests
{
    internal static class TestWindowAutomation
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent,
                new RoutedEventHandler(NameWindow));
        }

        private static void NameWindow(object sender, RoutedEventArgs args)
        {
            if (sender is Window window && string.IsNullOrEmpty(AutomationProperties.GetName(window)))
            {
                // Retain a managed name after Close destroys the HWND. Delayed layout
                // automation must not fall back to GetWindowText on that old handle.
                AutomationProperties.SetName(window, string.IsNullOrEmpty(window.Title)
                    ? "GameSaveCenter WPF test window"
                    : window.Title);
            }
        }
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class ModuleInitializerAttribute : Attribute
    {
    }
}
