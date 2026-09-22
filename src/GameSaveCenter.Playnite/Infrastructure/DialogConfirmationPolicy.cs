using System.Windows.Controls;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Keeps keyboard defaults explicit for embedded confirmation dialogs. A dangerous
    /// confirmation must never promote its action to the window-wide Enter default.
    /// </summary>
    internal static class DialogConfirmationPolicy
    {
        internal static void ApplyConfirmationButtons(Button confirmButton, Button cancelButton, bool isDangerous)
        {
            confirmButton.IsDefault = !isDangerous;
            confirmButton.IsCancel = false;
            cancelButton.IsDefault = false;
            cancelButton.IsCancel = true;
        }

        internal static void ApplyChoiceButtons(Button primaryButton, Button cancelButton)
        {
            primaryButton.IsDefault = true;
            primaryButton.IsCancel = false;
            cancelButton.IsDefault = false;
            cancelButton.IsCancel = true;
        }

        internal static void ApplyResultButton(Button resultButton, Button cancelButton)
        {
            resultButton.IsDefault = true;
            resultButton.IsCancel = false;
            cancelButton.IsDefault = false;
            cancelButton.IsCancel = false;
        }
    }
}
