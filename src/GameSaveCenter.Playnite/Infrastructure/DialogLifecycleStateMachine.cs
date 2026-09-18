namespace GameSaveCenter.Playnite.Infrastructure
{
    internal enum DialogLifecycleState
    {
        Closed,
        Opening,
        Open,
        Closing
    }

    /// <summary>
    /// Keeps the embedded dialog's visual lifetime separate from its request completion.
    /// A closing overlay remains active until its exit transition settles, so a second
    /// command cannot race the first completion or land focus behind the mask.
    /// </summary>
    internal sealed class DialogLifecycleStateMachine
    {
        private DialogLifecycleState state = DialogLifecycleState.Closed;
        private bool completionClaimed;

        internal DialogLifecycleState State => state;

        internal bool IsActive => state != DialogLifecycleState.Closed;

        internal bool IsClosing => state == DialogLifecycleState.Closing;

        internal bool TryBeginOpening()
        {
            if (state != DialogLifecycleState.Closed)
                return false;

            completionClaimed = false;
            state = DialogLifecycleState.Opening;
            return true;
        }

        internal bool TryMarkOpen()
        {
            if (state != DialogLifecycleState.Opening)
                return false;

            state = DialogLifecycleState.Open;
            return true;
        }

        internal bool TryClaimCompletion()
        {
            if (completionClaimed || state == DialogLifecycleState.Closed || state == DialogLifecycleState.Closing)
                return false;

            completionClaimed = true;
            return true;
        }

        internal bool TryBeginClosing()
        {
            if (state == DialogLifecycleState.Closed || state == DialogLifecycleState.Closing)
                return false;

            state = DialogLifecycleState.Closing;
            return true;
        }

        internal bool TryFinishClosing()
        {
            if (state != DialogLifecycleState.Closing)
                return false;

            state = DialogLifecycleState.Closed;
            completionClaimed = true;
            return true;
        }

        internal void ForceClosed()
        {
            state = DialogLifecycleState.Closed;
            completionClaimed = true;
        }
    }
}
