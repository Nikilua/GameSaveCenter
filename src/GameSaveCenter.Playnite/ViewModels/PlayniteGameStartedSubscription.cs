using System;

namespace GameSaveCenter.Playnite.ViewModels
{
    /// <summary>Owns one idempotent subscription to the Playnite game-started event.</summary>
    internal sealed class PlayniteGameStartedSubscription
    {
        private readonly Action<Action<Guid>> subscribe;
        private readonly Action<Action<Guid>> unsubscribe;
        private readonly Action<Guid> callback;
        private bool isSubscribed;

        public PlayniteGameStartedSubscription(
            Action<Action<Guid>> subscribe,
            Action<Action<Guid>> unsubscribe,
            Action<Guid> callback)
        {
            this.subscribe = subscribe ?? throw new ArgumentNullException(nameof(subscribe));
            this.unsubscribe = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public bool IsSubscribed => isSubscribed;

        public void Start()
        {
            if (isSubscribed) return;
            subscribe(callback);
            isSubscribed = true;
        }

        public void Stop()
        {
            if (!isSubscribed) return;
            unsubscribe(callback);
            isSubscribed = false;
        }
    }
}
