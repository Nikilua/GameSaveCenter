using System.Windows.Controls;

namespace GameSaveCenter.Playnite.Infrastructure;

/// <summary>
/// Shared restore path for filter ComboBoxes whose ObservableCollection is rebuilt
/// asynchronously. WPF clears SelectedItem on a collection reset; this helper puts the
/// logical default (usually 全部) back on screen without overwriting a real user choice.
/// </summary>
public static class UiFilterSelection
{
    public static void RestoreDefault(ComboBox combo, string defaultText)
    {
        if (combo == null || combo.Items.Count == 0 || combo.SelectedItem != null)
            return;

        var index = string.IsNullOrEmpty(defaultText) ? 0 : combo.Items.IndexOf(defaultText);
        if (index < 0)
            index = 0;
        if (index >= 0 && index < combo.Items.Count)
            combo.SelectedIndex = index;
    }
}
