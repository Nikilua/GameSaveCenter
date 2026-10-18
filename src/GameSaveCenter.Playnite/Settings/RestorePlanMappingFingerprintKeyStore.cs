using System;
using System.Security.Cryptography;
using System.Text;

namespace GameSaveCenter.Playnite.Settings
{
    /// <summary>Loads or creates the per-Windows-user key used to HMAC redacted mapping evidence.</summary>
    internal static class RestorePlanMappingFingerprintKeyStore
    {
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("GameSaveCenter.RestoreMappingFingerprint.v1");

        public static RestorePlanMappingFingerprintKeyMaterial GetOrCreate(
            GameSaveCenterSettings settings,
            Action persistSettings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (persistSettings == null) throw new ArgumentNullException(nameof(persistSettings));

            if (TryLoad(settings, out var existingKey, out var existingKeyId))
                return new RestorePlanMappingFingerprintKeyMaterial(existingKey, existingKeyId);

            var key = new byte[32];
            using (var random = RandomNumberGenerator.Create()) random.GetBytes(key);
            var protectedKey = Convert.ToBase64String(ProtectedData.Protect(key, Entropy, DataProtectionScope.CurrentUser));
            var keyId = Guid.NewGuid().ToString("N");
            var previousProtectedKey = settings.RestorePlanMappingFingerprintKeyProtected;
            var previousKeyId = settings.RestorePlanMappingFingerprintKeyId;

            settings.RestorePlanMappingFingerprintKeyProtected = protectedKey;
            settings.RestorePlanMappingFingerprintKeyId = keyId;
            try
            {
                persistSettings();
            }
            catch
            {
                settings.RestorePlanMappingFingerprintKeyProtected = previousProtectedKey ?? string.Empty;
                settings.RestorePlanMappingFingerprintKeyId = previousKeyId ?? string.Empty;
                Array.Clear(key, 0, key.Length);
                throw;
            }

            return new RestorePlanMappingFingerprintKeyMaterial(key, keyId);
        }

        private static bool TryLoad(GameSaveCenterSettings settings, out byte[] key, out string keyId)
        {
            key = Array.Empty<byte>();
            keyId = string.Empty;
            if (string.IsNullOrWhiteSpace(settings.RestorePlanMappingFingerprintKeyProtected)
                || !Guid.TryParseExact(settings.RestorePlanMappingFingerprintKeyId, "N", out _))
                return false;

            try
            {
                var protectedBytes = Convert.FromBase64String(settings.RestorePlanMappingFingerprintKeyProtected);
                var unprotected = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
                Array.Clear(protectedBytes, 0, protectedBytes.Length);
                if (unprotected.Length != 32)
                {
                    Array.Clear(unprotected, 0, unprotected.Length);
                    return false;
                }

                key = unprotected;
                keyId = settings.RestorePlanMappingFingerprintKeyId;
                return true;
            }
            catch (Exception exception) when (exception is FormatException || exception is CryptographicException || exception is ArgumentException)
            {
                return false;
            }
        }
    }

    internal sealed class RestorePlanMappingFingerprintKeyMaterial : IDisposable
    {
        private readonly byte[] key;

        public string KeyId { get; }
        public string KeyBase64 => Convert.ToBase64String(key);

        public RestorePlanMappingFingerprintKeyMaterial(byte[] key, string keyId)
        {
            this.key = key ?? throw new ArgumentNullException(nameof(key));
            KeyId = keyId ?? string.Empty;
        }

        public void Dispose() => Array.Clear(key, 0, key.Length);
    }
}
