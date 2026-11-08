#if UNITY_EDITOR || DEBUG
using System;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

public sealed partial class LunaNativeValidation
{
    private void CheckSaveReplacementLocks()
    {
        BeginCase("save", "atomic-replacement-locks");
        Require(Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer,
            "Atomic replacement lock regression requires Windows; this platform is not silently skipped.");
        var helper = Active<LunaSaveSettingsHelper>();
        Require(helper != null, "Native settings helper is missing.");
        var player = LunaDataTable.Instance.playerData;
        int originalGold = player.Gold;
        string primary = Path.Combine(LunaRuntimeData.SaveDirectory, "LunaSave.dat"), backup = primary + ".bak";
        int warningStart = report.warnings.Count;
        try
        {
            player.Gold = 704001;
            Require(LunaRuntimeData.SaveNow() && LunaRuntimeData.SaveNow(), "Could not seed a valid primary and backup for locking.");
            byte[] seed = File.ReadAllBytes(primary);
            player.Gold = 704002;
            var transient = new FileStream(backup, FileMode.Open, FileAccess.Read, FileShare.Read);
            Exception releaseFailure = null;
            long attemptSerialBefore = helper.SaveAttemptSerial, releaseSerial = 0;
            using var watcherReady = new ManualResetEventSlim(false);
            var release = new Thread(() =>
            {
                try
                {
                    watcherReady.Set();
                    var wait = System.Diagnostics.Stopwatch.StartNew();
                    // The seed itself may have retried. A monotonic serial names
                    // attempts of this save without mistaking that previous count.
                    while (helper.SaveAttemptSerial < attemptSerialBefore + 2 && wait.ElapsedMilliseconds < 2000) Thread.Sleep(1);
                    releaseSerial = helper.SaveAttemptSerial;
                    if (releaseSerial < attemptSerialBefore + 2) throw new TimeoutException("This save did not reach its second actual replacement attempt.");
                }
                catch (Exception error) { releaseFailure = error; }
                finally { transient.Dispose(); }
            }) { IsBackground = true, Name = "Luna isolated save-lock release" };
            bool transientSaved;
            release.Start();
            try
            {
                Require(watcherReady.Wait(2000), "Background lock watcher did not start within its bound.");
                transientSaved = LunaRuntimeData.SaveNow();
            }
            finally
            {
                bool joined = release.Join(2000);
                transient.Dispose();
                Require(joined, "Background lock release did not finish within its bound.");
            }
            string transientEvidence = "saved=" + transientSaved + "; attempts=" + helper.LastSaveAttemptCount +
                "; serial before/release/after=" + attemptSerialBefore + "/" + releaseSerial + "/" + helper.SaveAttemptSerial +
                "; releaseFailure=" + (releaseFailure == null ? "none" : releaseFailure.ToString());
            current.observations.Add(transientEvidence);
            Require(releaseFailure == null && transientSaved && releaseSerial >= attemptSerialBefore + 2 && helper.LastSaveAttemptCount >= 2 && helper.LastSaveAttemptCount <= LunaSaveSettingsHelper.MaxSaveAttempts,
                "Transient replacement lock was not actually retried to a successful atomic save: " + transientEvidence);
            Require(report.warnings.Count == warningStart, "A recovered transient lock emitted a failure warning.");
            Require(!File.ReadAllBytes(primary).SequenceEqual(seed) && File.ReadAllBytes(backup).SequenceEqual(seed), "Successful retry did not preserve the previous valid generation in backup.");
            player.Gold = 0;
            Require(LunaRuntimeData.Reload() && player.Gold == 704002, "Transient retry did not persist the new generation.");
            current.observations.Add("Transient backup delete lock released only after observing the second actual save attempt; actual save attempts=" + helper.LastSaveAttemptCount + "; new generation704002 reloaded.");

            // Locking the backup must fail before the live primary is changed.
            byte[] beforePrimary = File.ReadAllBytes(primary), beforeBackup = File.ReadAllBytes(backup);
            player.Gold = 704003;
            using (var locked = new FileStream(backup, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Require(!LunaRuntimeData.SaveNow() && helper.LastSaveAttemptCount == LunaSaveSettingsHelper.MaxSaveAttempts,
                    "Persistent backup lock did not report failure after the bounded retries.");
                Require(File.ReadAllBytes(primary).SequenceEqual(beforePrimary) && File.ReadAllBytes(backup).SequenceEqual(beforeBackup),
                    "Failed backup preparation modified either committed generation.");
            }
            player.Gold = 0;
            Require(LunaRuntimeData.Reload() && player.Gold == 704002, "Backup-lock failure lost the prior committed save.");

            // A primary lock may atomically rotate the backup to the current valid
            // primary, but may never partially publish the failed new generation.
            player.Gold = 704004;
            using (var locked = new FileStream(primary, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Require(!LunaRuntimeData.SaveNow() && helper.LastSaveAttemptCount == LunaSaveSettingsHelper.MaxSaveAttempts,
                    "Persistent primary lock did not report failure after the bounded retries.");
                Require(File.ReadAllBytes(primary).SequenceEqual(beforePrimary) && File.ReadAllBytes(backup).SequenceEqual(beforePrimary),
                    "Failed primary commit lost or partially replaced the last valid saved generation.");
            }
            player.Gold = 0;
            Require(LunaRuntimeData.Reload() && player.Gold == 704002, "Primary-lock failure published failed progress or lost its valid save.");
            player.Gold = 704005;
            Require(LunaRuntimeData.SaveNow(), "An unlocked save did not recover after persistent replacement failures.");
            player.Gold = 0;
            Require(LunaRuntimeData.Reload() && player.Gold == 704005, "Post-lock recovery did not reload the new committed generation.");
            string[] failures = report.warnings.Skip(warningStart).ToArray();
            Require(failures.Length == 2 && failures.All(warning => warning.StartsWith("Could not save Luna progress:", StringComparison.Ordinal)),
                "Persistent lock probes emitted missing, extra, or unrelated warnings.");
            // These exact occurrences are allowed only after the real failure,
            // old-generation preservation, and subsequent recovery assertions pass.
            report.expectedWarnings.AddRange(failures);
            current.observations.AddRange(failures.Select(warning => "Verified intentional persistent file-lock diagnostic: " + warning));
            current.observations.Add("Backup and primary locks both reached the bounded retry limit; failed new progress never replaced primary, and generation704005 saved/reloaded after unlock.");
        }
        finally
        {
            player.Gold = originalGold;
            Require(LunaRuntimeData.SaveNow(), "Restoring the isolated save after lock regression failed.");
        }
        EndCase("Actual Windows sharing locks exercised transient retry, persistent backup/primary failure, atomic generation preservation and recovery; exact intentional diagnostics were reconciled.");
    }
}
#endif
