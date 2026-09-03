using System.Windows.Controls;

namespace GameSaveCenter.Playnite.Infrastructure;

/// <summary>
/// Shared restore path for filter ComboBoxes whose ObservableCollection is rebuilt
/// asynchronously. WPF clears SelectedItem on a collection reset; this helper puts the
/// logical default (usually 全部) back on screen without overwriting a real user choice.
/// </summary>
public static class UiFilterSelection
{
    /// <summary>
    /// Aligns a bound ComboBox with the authoritative ViewModel value after WPF has
    /// regenerated its items. Unlike RestoreDefault, this also repairs a valid but stale
    /// selection left behind by another compatible view bound to the same state.
    /// </summary>
    public static void Synchronize(ComboBox combo, string selectedText)
    {
        if (combo == null || combo.Items.Count == 0)
            return;

        var index = string.IsNullOrEmpty(selectedText) ? 0 : combo.Items.IndexOf(selectedText);
        if (index < 0)
            index = 0;
        if (index >= 0 && index < combo.Items.Count && combo.SelectedIndex != index)
            combo.SelectedIndex = index;
    }

    public static void RestoreDefault(ComboBox combo, string defaultText)
    {
        if (combo == null || combo.Items.Count == 0)
            return;

        // A ComboBox bound to an ObservableCollection can retain an old selected
        // object while the collection is being reset. Treat that state as empty so
        // the logical default can be restored after the item container is generated.
        if (combo.SelectedIndex >= 0 && combo.SelectedItem != null && combo.Items.Contains(combo.SelectedItem))
            return;

        var index = string.IsNullOrEmpty(defaultText) ? 0 : combo.Items.IndexOf(defaultText);
        if (index < 0)
            index = 0;
        if (index >= 0 && index < combo.Items.Count)
            combo.SelectedIndex = index;
    }
}
