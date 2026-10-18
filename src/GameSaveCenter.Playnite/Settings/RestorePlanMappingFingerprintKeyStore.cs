using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using GameSaveCenter.Contracts;

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

        public static RestorePlanMappingFingerprintKeyMaterial? TryLoad(GameSaveCenterSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            return TryLoad(settings, out var key, out var keyId)
                ? new RestorePlanMappingFingerprintKeyMaterial(key, keyId)
                : null;
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

        public string ComputeFileIdentityFingerprint(string? contentFingerprint)
        {
            var normalized = (contentFingerprint ?? string.Empty).Trim().ToUpperInvariant();
            if (normalized.Length != 64) return string.Empty;
            foreach (var character in normalized)
                if (!Uri.IsHexDigit(character)) return string.Empty;

            return ComputeDomainSeparatedFingerprint("GameSaveCenter.RestorePlan.FileIdentity.v1", normalized);
        }

        public string ComputeReadinessFingerprint(RestoreReadinessDto? readiness)
        {
            if (readiness?.CheckedUtc is not DateTime checkedUtc || string.IsNullOrWhiteSpace(readiness.BackupVersionId))
                return string.Empty;

            var canonical = string.Join("\n", new[]
            {
                readiness.BackupVersionId.Trim().ToUpperInvariant(),
                readiness.Status.ToString(),
                checkedUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                readiness.ArchiveReadable ? "1" : "0",
                readiness.ExtractSucceeded ? "1" : "0",
                readiness.ExpectedFileCount.ToString(CultureInfo.InvariantCulture),
                readiness.ActualFileCount.ToString(CultureInfo.InvariantCulture),
                readiness.ExpectedTotalSize.ToString(CultureInfo.InvariantCulture),
                readiness.ActualTotalSize.ToString(CultureInfo.InvariantCulture),
                readiness.HashValidation ?? string.Empty,
                readiness.HashCoveredFileCount.ToString(CultureInfo.InvariantCulture),
                readiness.HashEligibleFileCount.ToString(CultureInfo.InvariantCulture),
                readiness.WarningCount.ToString(CultureInfo.InvariantCulture),
                readiness.ErrorCount.ToString(CultureInfo.InvariantCulture),
                readiness.StagingCleanupStatus ?? string.Empty
            });
            return ComputeDomainSeparatedFingerprint("GameSaveCenter.RestorePlan.Readiness.v1", canonical);
        }

        private string ComputeDomainSeparatedFingerprint(string domain, string canonical)
        {
            using var hmac = new HMACSHA256(key);
            var input = Encoding.UTF8.GetBytes(domain + "\n" + canonical);
            return BitConverter.ToString(hmac.ComputeHash(input)).Replace("-", string.Empty);
        }

        public RestorePlanMappingFingerprintKeyMaterial(byte[] key, string keyId)
        {
            this.key = key ?? throw new ArgumentNullException(nameof(key));
            KeyId = keyId ?? string.Empty;
        }

        public void Dispose() => Array.Clear(key, 0, key.Length);
    }
}
