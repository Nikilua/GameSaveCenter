using System;
using System.Threading;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Threading;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class GamePickerFilterBehaviorTests
    {
        [Fact]
        public void ProgrammaticSelectionDoesNotCommitWhileTheDropdownIsClosed()
        {
            string? committed = null;
            RunOnSta(() =>
            {
                var combo = CreateFilterComboBox();
                combo.DropDownClosed += (_, _) => committed = combo.SelectedItem as string;
                combo.SelectedIndex = 1;
                Assert.Null(committed);
            });
        }

        [Fact]
        public void SelectionCommittedWhenTheDropdownClosesAfterUserFlow()
        {
            string? committed = null;
            RunOnSta(() =>
            {
                var combo = CreateFilterComboBox();
                using var window = ShowHost(combo);
                combo.DropDownClosed += (_, _) => committed = combo.SelectedItem as string;
                combo.IsDropDownOpen = true;
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                combo.SelectedIndex = 1;
                combo.IsDropDownOpen = false;
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                Assert.Equal("已安装", committed);
            });
        }

        private static ComboBox CreateFilterComboBox()
        {
            var combo = new ComboBox();
            combo.Items.Add("全部");
            combo.Items.Add("已安装");
            combo.SelectedIndex = 0;
            return combo;
        }

        private static WindowHost ShowHost(ComboBox combo)
        {
            var window = new Window
            {
                Content = combo,
                Width = 160,
                Height = 80,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.ToolWindow,
                ResizeMode = ResizeMode.NoResize
            };
            window.Show();
            window.UpdateLayout();
            return new WindowHost(window);
        }

        private sealed class WindowHost : IDisposable
        {
            private readonly Window window;

            public WindowHost(Window window) { this.window = window; }

            public void Dispose()
            {
                if (window.IsVisible) window.Close();
            }
        }

        private static void RunOnSta(Action action)
        {
            Exception? failure = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                    Dispatcher.CurrentDispatcher.InvokeShutdown();
                }
                catch (Exception ex)
                {
                    failure = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (failure != null) throw new Xunit.Sdk.XunitException(failure.ToString());
        }
    }
}
