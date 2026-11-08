#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Luna.Gameplay;
using GameFramework;
using GameFramework.Setting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityGameFramework.Runtime;

public sealed partial class LunaNativeValidation
{
    private string PlayerOutputDirectory => Path.GetDirectoryName(reportPath);
    [Serializable] private sealed class PersistenceFixture
    {
        public string nonce, unit, stage;
        public int gold, stone, unitLevel, cards, itemId, items;
        public bool sound;
        public string[] deck, skills;
        public int skin;
    }
    private readonly List<string> playerDiagnosticFailures = new List<string>();
    private bool simulatedHeld;
    private bool simulatedLeft, simulatedRight;
    private UIPlayer simulatedUI;

    private void LateUpdate()
    {
        if (simulatedHeld && simulatedUI)
            ApplyKeyboard(simulatedUI, simulatedLeft, simulatedRight);
    }
    private static void ApplyKeyboard(UIPlayer ui, bool left, bool right)
    {
        Require(ui != null, "The normal keyboard movement form is unavailable.");
        ui.ApplyKeyboardMovement(left, right);
    }
    private IEnumerator PlayerProbe(string mode)
    {
        yield return Boot();
        if (mode == "seed")
        {
            BeginCase("persistence", "write-first-process");
            PlayerData player = LunaDataTable.Instance.playerData;
            var fixture = new PersistenceFixture
            {
                nonce = Guid.NewGuid().ToString("N"), gold = 246813, stone = 1357,
                deck = new[] { "Alice", "Lene", "Dren", "Tira", "PorongArcher", "PorongSword" },
                skills = new[] { "Thunder", "Healing", "FireBall" }, skin = 1,
                stage = "Stage01-02", unit = "Alice", unitLevel = 3, cards = 7,
                itemId = LunaDataTable.Instance.itemTable.Keys.OrderBy(i => i).First(), items = 5, sound = false
            };
            player.Gold = fixture.gold; player.StarStone = fixture.stone;
            player.UnitNamesInDeck = fixture.deck.ToList(); player.equippedSkill = fixture.skills.ToList();
            player.SkinType = (PlayerData.eLunaSkin)fixture.skin; player.SaveStageName = fixture.stage;
            LunaDataTable.Instance.UnitTable[fixture.unit].Level = fixture.unitLevel;
            LunaDataTable.Instance.UnitTable[fixture.unit].cardCount = fixture.cards;
            LunaDataTable.Instance.itemTable[fixture.itemId].itemCount = fixture.items;
            LunaDataTable.Instance.StageTable["Stage01-01"].isClear = true;
            var settings = GameFrameworkEntry.GetModule<ISettingManager>();
            settings.SetBool("Luna.Sound.Enabled", fixture.sound);
            settings.SetString("Luna.Validation.Nonce", fixture.nonce);
            Require(LunaRuntimeData.SaveNow(), "Initial process failed to save its fixture.");
            File.WriteAllText(Path.Combine(PlayerOutputDirectory, "persistence-expected.json"), JsonUtility.ToJson(fixture, true));
            EndCase("Distinctive progress and sound saved through the real framework setting helper; second process must recover the random nonce and all fields.");
            yield break;
        }
        Require(mode == "verify" || mode == "diagnostics", "Unknown player validation mode: " + mode);
        if (mode == "verify")
        {
            BeginCase("persistence", "read-second-process");
            string path = Path.Combine(PlayerOutputDirectory, "persistence-expected.json");
            Require(File.Exists(path), "First-process expected fixture is missing.");
            PersistenceFixture expected = JsonUtility.FromJson<PersistenceFixture>(File.ReadAllText(path));
            PlayerData player = LunaDataTable.Instance.playerData;
            var settings = GameFrameworkEntry.GetModule<ISettingManager>();
            Require(settings.GetString("Luna.Validation.Nonce", "") == expected.nonce, "Second process did not load the first process's save.");
            Require(player.Gold == expected.gold && player.StarStone == expected.stone && player.SkinType == (PlayerData.eLunaSkin)expected.skin &&
                player.SaveStageName == expected.stage && player.UnitNamesInDeck.SequenceEqual(expected.deck) && player.equippedSkill.SequenceEqual(expected.skills), "Cross-process player progress mismatch.");
            Require(LunaDataTable.Instance.UnitTable[expected.unit].Level == expected.unitLevel && LunaDataTable.Instance.UnitTable[expected.unit].cardCount == expected.cards &&
                LunaDataTable.Instance.itemTable[expected.itemId].itemCount == expected.items && LunaDataTable.Instance.StageTable["Stage01-01"].isClear, "Cross-process unit/item/stage mismatch.");
            Require(settings.GetBool("Luna.Sound.Enabled", true) == expected.sound && AudioListener.volume == 0f, "Cross-process saved mute did not apply.");
            EndCase("A separate executable process restored the nonce, gold/stones, deck, skills, skin, stage clear, unit growth, inventory and muted listener.");
        }
        string requestedSection = Environment.GetEnvironmentVariable("LUNA_PLAYER_SECTION") ?? "all";
        Require(new[] { "all", "persistence", "audio", "input", "render", "soak" }.Contains(requestedSection), "Unknown Windows probe section: " + requestedSection);
        Require(requestedSection != "persistence" || mode == "verify", "Persistence verification requires the separate seed/verify process pair.");
        // Native round setup plays battle music only in Game/Dungeon; Lobby is intentionally silent.
        if (requestedSection == "all" || requestedSection == "audio" || requestedSection == "input" || requestedSection == "render" || requestedSection == "soak")
            yield return LoadStage("Stage01-01");
        try
        {
        if (requestedSection == "all" || requestedSection == "audio") yield return Independent(AudioOutput());
        if (requestedSection == "all" || requestedSection == "input") yield return Independent(KeyboardMovement());
        if (requestedSection == "all" || requestedSection == "render") yield return Independent(ResolutionCaptures());
        if (requestedSection == "all" || requestedSection == "soak") yield return Independent(Soak());
        }
        finally { report.errors.AddRange(playerDiagnosticFailures); playerDiagnosticFailures.Clear(); }
    }
    // Measurement failures are recorded per case and reconciled at the end of this probe.
    // Unexpected Unity errors still reach the main driver's fail-fast logger immediately.
    private IEnumerator Independent(IEnumerator routine)
    {
        var stack = new Stack<IEnumerator>(); stack.Push(routine);
        try
        {
            while (stack.Count > 0)
            {
                object yielded = null; Exception failure = null;
                try
                {
                    var next = stack.Peek();
                    if (!next.MoveNext())
                    {
                        stack.Pop(); (next as IDisposable)?.Dispose();
                        continue;
                    }
                    else yielded = next.Current;
                }
                catch (Exception error) { failure = error; }
                if (failure != null)
                {
                    playerDiagnosticFailures.Add((current == null ? "diagnostic" : current.group + "/" + current.name) + ": " + failure);
                    if (current != null)
                    {
                        current.finishedUtc = DateTime.UtcNow.ToString("O"); current.seconds = Time.realtimeSinceStartup - caseStarted;
                        current.detail = "Failed or unavailable; later independent Windows probes may continue."; current.success = false;
                        current.observations.Add(failure.Message);
                    }
                    SaveReport(); current = null; yield break;
                }
                if (yielded is IEnumerator nested) stack.Push(nested);
                else if (stack.Count > 0) yield return yielded;
            }
        }
        finally
        {
            while (stack.Count > 0)
                try { (stack.Pop() as IDisposable)?.Dispose(); }
                catch (Exception error) { playerDiagnosticFailures.Add("Windows fixture cleanup: " + error); }
        }
    }

    private IEnumerator MeasureRms(Action<float, double> result, float duration = 1.5f)
    {
        var data = new float[1024]; double sum = 0; long samples = 0;
        double dsp = AudioSettings.dspTime; float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - start < duration)
        {
            AudioListener.GetOutputData(data, 0);
            foreach (float sample in data) { sum += sample * sample; samples++; }
            yield return new WaitForSecondsRealtime(0.05f);
        }
        result((float)Math.Sqrt(sum / Math.Max(1, samples)), AudioSettings.dspTime - dsp);
    }
    private static void SetSound(bool on)
    {
        var option = Active<UIGameOption>(); Require(option, "Sound option component missing.");
        bool currentState = GameFrameworkEntry.GetModule<ISettingManager>().GetBool("Luna.Sound.Enabled", true);
        if (currentState != on) option.OnClickSoundOnOff();
        Require(AudioListener.volume == (on ? 1f : 0f), "Sound UI action did not apply listener volume.");
    }
    private IEnumerator AudioOutput()
    {
        BeginCase("audio", "listener-pcm-rms-on-off-on");
        bool saved = GameFrameworkEntry.GetModule<ISettingManager>().GetBool("Luna.Sound.Enabled", true);
        AudioSource source = null; AudioClip tone = null;
        try
        {
            SetSound(true);
            yield return Until(() => FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude).Any(a => a.isPlaying && a.clip), "Live native battle music or effects", 30f);
            int playing = FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude).Count(a => a.isPlaying && a.clip);
            float normal = 0; double clock = 0;
            yield return MeasureRms((r, d) => { normal = r; clock = d; });
            current.observations.Add("Live game audio: playing clip sources=" + playing + ", listener RMS=" + normal.ToString("R") + ", DSP elapsed=" + clock.ToString("R"));
            Require(clock > 0.5, "Audio DSP clock did not advance in this process.");
            int sampleRate = AudioSettings.outputSampleRate;
            Require(sampleRate > 0, "No audio output sample rate.");
            // Known PCM separates actual mixer/mute behavior from a quiet part of the authored music.
            tone = AudioClip.Create("Validation PCM fixture", sampleRate, 1, sampleRate, false);
            float[] samples = new float[sampleRate];
            for (int i = 0; i < samples.Length; i++) samples[i] = 0.02f * Mathf.Sin(2f * Mathf.PI * 440f * i / sampleRate);
            tone.SetData(samples, 0);
            source = gameObject.AddComponent<AudioSource>(); source.clip = tone; source.loop = true; source.spatialBlend = 0; source.Play();
            yield return new WaitForSecondsRealtime(0.3f);
            float on = 0, off = 0, resumed = 0;
            yield return MeasureRms((r, d) => on = r);
            SetSound(false); yield return new WaitForSecondsRealtime(0.2f);
            yield return MeasureRms((r, d) => off = r);
            SetSound(true); yield return new WaitForSecondsRealtime(0.2f);
            yield return MeasureRms((r, d) => resumed = r);
            current.observations.Add("PCM fixture + mix RMS on=" + on.ToString("R") + ", muted=" + off.ToString("R") + ", restored=" + resumed.ToString("R") + ", sampleRate=" + sampleRate);
            Require(on > 0.00001f && resumed > 0.00001f, "Listener PCM output remained silent despite a playing known tone; audio output is unverified in this environment.");
            Require(off <= Math.Max(on, resumed) * 0.02f + 0.000001f, "Mute did not suppress measured listener PCM output.");
            EndCase("Actual sound button and persisted setting exercised; DSP clock and listener output RMS measured. This is signal measurement, not a claim that speakers were heard.");
        }
        finally
        {
            if (source != null) { source.Stop(); Destroy(source); }
            if (tone != null) Destroy(tone);
            SetSound(saved);
        }
    }
    private IEnumerator KeyboardMovement()
    {
        BeginCase("input", "normal-handler-held-and-release");
        UIPlayer ui = Active<UIPlayer>(); GameObject player = LunaSession.Current.LunaPlayer;
        PlayerLogic controller = UnitLogic.From(player) as PlayerLogic;
        Require(controller != null, "Native PlayerLogic is missing.");
        ApplyKeyboard(ui, true, false); Require(controller.Moving == -1, "Left handler did not request left movement.");
        ApplyKeyboard(ui, true, true); Require(controller.Moving == 0, "Opposed keys were not neutral.");
        ApplyKeyboard(ui, false, true); Require(controller.Moving == 1, "Releasing one opposed key did not continue right.");
        ApplyKeyboard(ui, false, false); Require(controller.Moving == 0, "Releasing both keys did not stop keyboard movement.");
        LunaSession.Current.LunaPlayer = null;
        try { ApplyKeyboard(ui, true, false); ApplyKeyboard(ui, false, false); }
        finally { LunaSession.Current.LunaPlayer = player; }
        float startX = player.transform.position.x;
        simulatedUI = ui; simulatedLeft = false; simulatedRight = true; simulatedHeld = true;
        try { yield return new WaitForSecondsRealtime(1f); }
        finally { simulatedHeld = false; if (ui != null) ApplyKeyboard(ui, false, false); simulatedUI = null; }
        float displacement = player.transform.position.x - startX;
        current.observations.Add("One-second repeated normal handler requested right; measured world displacement=" + displacement.ToString("R"));
        Require(displacement > 0.02f, "Repeated held handler did not move the real player.");
        EndCase("Normal input handler left/right/opposed/release/null-player cases and one-second movement measured programmatically; no OS key hold or physical input is claimed.");
    }
    private IEnumerator ResolutionCaptures()
    {
        string renderMode = Environment.GetEnvironmentVariable("LUNA_RENDER_MODE") ?? "backbuffer";
        Require(renderMode == "backbuffer" || renderMode == "offscreen", "Unknown render measurement mode: " + renderMode);
        bool explicitOffscreen = renderMode == "offscreen";
        Require(SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null, "Rendering requires a real graphics device, not -nographics.");
        int oldWidth = Screen.width, oldHeight = Screen.height; var oldMode = Screen.fullScreenMode;
        int[][] sizes = { new[] { 1280, 720 }, new[] { 1920, 1080 }, new[] { 1024, 768 } };
        try
        {
        foreach (int[] size in sizes)
        {
            BeginCase("render", renderMode + "-" + size[0] + "x" + size[1]);
            Screen.SetResolution(size[0], size[1], false);
            yield return new WaitForSecondsRealtime(2f);
            current.observations.Add("Requested=" + size[0] + "x" + size[1] + "; reported Screen=" + Screen.width + "x" + Screen.height + "; mode=" + renderMode);
            if (!explicitOffscreen)
                Require(Screen.width == size[0] && Screen.height == size[1], "Requested backbuffer resolution was not applied.");
            if (!explicitOffscreen) { Canvas.ForceUpdateCanvases(); MeasureCriticalUiBounds(size[0], size[1]); }
            Texture2D capture = explicitOffscreen ? CaptureOffscreen(size[0], size[1]) : ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
            Require(capture && capture.width == size[0] && capture.height == size[1], "Render capture size mismatch.");
            Color32[] pixels = capture.GetPixels32(); double sum = 0, squares = 0; int visible = 0;
            foreach (Color32 pixel in pixels)
            {
                double luma = (pixel.r + pixel.g + pixel.b) / (3.0 * 255);
                sum += luma; squares += luma * luma; if (luma > 0.02) visible++;
            }
            double variance = squares / pixels.Length - Math.Pow(sum / pixels.Length, 2);
            string path = Path.Combine(PlayerOutputDirectory, "render-" + renderMode + "-" + size[0] + "x" + size[1] + ".png");
            File.WriteAllBytes(path, capture.EncodeToPNG());
            current.observations.Add("Captured PNG=" + path);
            Require(visible > pixels.Length / 20 && variance > 0.0001, "Captured render is blank or nearly uniform.");
            Require(renderBoundsFailures.Count == 0, "Critical HUD rectangles exceed render bounds: " + string.Join("; ", renderBoundsFailures));
            foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
                Require(canvas.pixelRect.width > 0 && canvas.pixelRect.height > 0, "Active UI canvas has zero render area.");
            current.observations.Add("PNG=" + path + "; nonblack=" + visible + "/" + pixels.Length + "; luminance variance=" + variance.ToString("R"));
            EndCase(explicitOffscreen
                ? "Explicit offscreen pipeline render and critical HUD rectangle bounds measured at requested target dimensions; overlay canvases temporarily camera-space. This does not validate the OS window/backbuffer or unchanged overlay layout. PNG inspection remains separate."
                : "Graphics-enabled hidden player backbuffer captured; dimensions, nonblank pixels, critical HUD rectangle bounds and active canvas areas verified. PNG inspection is separate from pixel assertions.");
            }
            finally { if (capture != null) Destroy(capture); }
        }
        }
        finally { Screen.SetResolution(oldWidth, oldHeight, oldMode); }
    }
    private IEnumerator Soak()
    {
        int seconds = 300;
        string requested = Environment.GetEnvironmentVariable("LUNA_VALIDATION_SOAK_SECONDS");
        if (!string.IsNullOrEmpty(requested)) seconds = Math.Max(10, Math.Min(7200, int.Parse(requested, System.Globalization.CultureInfo.InvariantCulture)));
        BeginCase("soak", seconds + "-seconds-scene-combat-cycles");
        float start = Time.realtimeSinceStartup; int cycles = 0; long peak = 0, initial = Profiler.GetTotalAllocatedMemoryLong();
        int lobbyEntities = -1, lobbyFsms = -1, lobbyForms = -1;
        while (Time.realtimeSinceStartup - start < seconds)
        {
            if (LunaScenes.CurrentScene != "Game" || !Active<UIPlayer>()) yield return LoadStage("Stage01-01");
            float battle = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - battle < 20f && Time.realtimeSinceStartup - start < seconds)
            {
                var result = Active<UIResult>(); if (result) { result.ReStage(); yield return new WaitForSecondsRealtime(8f); }
                peak = Math.Max(peak, Profiler.GetTotalAllocatedMemoryLong());
                yield return new WaitForSecondsRealtime(0.5f);
            }
            int retiredRound = LunaScenes.CurrentGame != null ? LunaScenes.CurrentGame.RoundId : 0;
            LunaScenes.Load("Lobby");
            yield return Until(() => LunaScenes.CurrentScene == "Lobby" && !LunaScenes.IsTransitioning &&
                Active<UILobby>() != null && Active<UILobby>().ProfileReady, "Soak lobby presentation ready");
            yield return Until(() => LunaSpawn.PendingCount == 0 && !Entities().Any(e => e.SpawnData != null && e.SpawnData.RoundId == retiredRound), "Soak retired round and pending loads");
            yield return new WaitForSecondsRealtime(2f);
            yield return Until(() => !LunaScenes.IsTransitioning && LunaSpawn.PendingCount == 0, "Soak settled native ownership");
            CheckOwnership();
            AssertEntityDataOwnershipBalance("Soak lobby cycle " + (cycles + 1));
            int fsms = GameEntry.GetComponent<FsmComponent>().Count;
            int forms = GameEntry.GetComponent<UIComponent>().GetAllLoadedUIForms().Length;
            if (lobbyEntities < 0) { lobbyEntities = Entities().Length; lobbyFsms = fsms; lobbyForms = forms; }
            Require(Entities().Length <= lobbyEntities && fsms <= lobbyFsms && forms <= lobbyForms, "Soak accumulated native entities, FSMs or UI forms after lobby return.");
            Require(!LunaSession.Current.ObjectSounds.Any(s => !s), "Soak retained destroyed AudioSources in the gameplay registry.");
            cycles++;
        }
        current.observations.Add("cycles=" + cycles + "; actualSeconds=" + (Time.realtimeSinceStartup - start).ToString("R") +
            "; initialAllocatedBytes=" + initial + "; peakAllocatedBytes=" + peak + "; finalAllocatedBytes=" + Profiler.GetTotalAllocatedMemoryLong() + "; finalNativeEntities=" + Entities().Length);
        EndCase("Timed real scene/combat loops completed with no logged errors, stable lobby native entity/FSM/UI counts, exact EntityData reference usage matching live native owners, and no destroyed sound-registry entries. Memory sampled, not a proof of unlimited-session leak freedom.");
    }
}
#endif
