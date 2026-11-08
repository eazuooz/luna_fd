#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EverydayDevup.Framework;
using GameFramework;
using GameFramework.Setting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Profiling;

public sealed partial class LunaRuntimeValidation
{
    [Serializable] private sealed class PersistenceFixture
    {
        public string nonce, unit, stage;
        public int gold, stone, unitLevel, cards, itemId, items;
        public bool sound;
        public string[] deck, skills;
        public int skin;
    }
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
        MethodInfo method = typeof(UIPlayer).GetMethod("ApplyKeyboardMovement");
        Require(method != null, "The normal keyboard movement handler is unavailable.");
        method.Invoke(ui, new object[] { left, right });
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
            File.WriteAllText(Path.Combine(outputDirectory, "persistence-expected.json"), JsonUtility.ToJson(fixture, true));
            EndCase("Distinctive progress and sound saved through the real framework setting helper; second process must recover the random nonce and all fields.");
            yield break;
        }
        Require(mode == "verify" || mode == "diagnostics", "Unknown player validation mode: " + mode);
        if (mode == "verify")
        {
            BeginCase("persistence", "read-second-process");
            string path = Path.Combine(outputDirectory, "persistence-expected.json");
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
        // The authored sound manager plays battle music only in Game/Dungeon; Lobby is intentionally silent.
        if (requestedSection == "all" || requestedSection == "audio" || requestedSection == "input" || requestedSection == "render" || requestedSection == "soak")
            yield return LoadStage("Stage01-01");
        if (requestedSection == "all" || requestedSection == "audio") yield return Independent(AudioOutput());
        if (requestedSection == "all" || requestedSection == "input") yield return Independent(KeyboardMovement());
        if (requestedSection == "all" || requestedSection == "render") yield return Independent(ResolutionCaptures());
        if (requestedSection == "all" || requestedSection == "soak") yield return Independent(Soak());
    }
    private IEnumerator Independent(IEnumerator routine)
    {
        var stack = new Stack<IEnumerator>(); stack.Push(routine);
        while (stack.Count > 0)
        {
            object yielded = null; Exception failure = null;
            try { if (!stack.Peek().MoveNext()) stack.Pop(); else yielded = stack.Peek().Current; }
            catch (Exception ex) { failure = ex; }
            if (failure != null)
            {
                report.errors.Add((current == null ? "diagnostic" : current.group + "/" + current.name) + ": " + failure);
                if (current != null) { current.finishedUtc = DateTime.UtcNow.ToString("O"); current.detail = "Failed or unavailable; independent diagnostics continue."; current.success = false; }
                SaveReport(); current = null; yield break;
            }
            if (yielded is IEnumerator nested) stack.Push(nested);
            else if (stack.Count > 0) yield return yielded;
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
        SetSound(true); yield return new WaitForSecondsRealtime(1f);
        int playing = FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude).Count(a => a.isPlaying && a.clip);
        float normal = 0; double clock = 0;
        yield return MeasureRms((r, d) => { normal = r; clock = d; });
        current.observations.Add("Live game audio: playing clip sources=" + playing + ", listener RMS=" + normal.ToString("R") + ", DSP elapsed=" + clock.ToString("R"));
        Require(playing > 0, "No game music/effect AudioSource was playing.");
        Require(clock > 0.5, "Audio DSP clock did not advance in this process.");
        // A quiet, known PCM fixture separates output/mute behavior from a silent interval in the music.
        int sampleRate = AudioSettings.outputSampleRate;
        Require(sampleRate > 0, "No audio output sample rate.");
        AudioClip tone = AudioClip.Create("Validation PCM fixture", sampleRate, 1, sampleRate, false);
        float[] samples = new float[sampleRate];
        for (int i = 0; i < samples.Length; i++) samples[i] = 0.02f * Mathf.Sin(2f * Mathf.PI * 440f * i / sampleRate);
        tone.SetData(samples, 0);
        var source = gameObject.AddComponent<AudioSource>(); source.clip = tone; source.loop = true; source.spatialBlend = 0; source.Play();
        yield return new WaitForSecondsRealtime(0.3f);
        float on = 0, off = 0, resumed = 0;
        yield return MeasureRms((r, d) => on = r);
        SetSound(false); yield return new WaitForSecondsRealtime(0.2f);
        yield return MeasureRms((r, d) => off = r);
        SetSound(true); yield return new WaitForSecondsRealtime(0.2f);
        yield return MeasureRms((r, d) => resumed = r);
        source.Stop(); Destroy(source); Destroy(tone); SetSound(saved);
        current.observations.Add("PCM fixture + mix RMS on=" + on.ToString("R") + ", muted=" + off.ToString("R") + ", restored=" + resumed.ToString("R") + ", sampleRate=" + sampleRate);
        Require(on > 0.00001f && resumed > 0.00001f, "Listener PCM output remained silent despite a playing known tone; audio output is unverified in this environment.");
        Require(off <= Math.Max(on, resumed) * 0.02f + 0.000001f, "Mute did not suppress measured listener PCM output.");
        EndCase("Actual sound button and persisted setting exercised; DSP clock and listener output RMS measured. This is signal measurement, not a claim that speakers were heard.");
    }
    private IEnumerator KeyboardMovement()
    {
        BeginCase("input", "normal-handler-held-and-release");
        UIPlayer ui = Active<UIPlayer>(); GameObject player = Game.Instance.dataManager.LunaPlayer;
        LunaStateMachine controller = player.GetComponent<LunaStateMachine>();
        ApplyKeyboard(ui, true, false); Require(controller.moving == -1, "Left handler did not request left movement.");
        ApplyKeyboard(ui, true, true); Require(controller.moving == 0, "Opposed keys were not neutral.");
        ApplyKeyboard(ui, false, true); Require(controller.moving == 1, "Releasing one opposed key did not continue right.");
        ApplyKeyboard(ui, false, false); Require(controller.moving == 0, "Releasing both keys did not stop keyboard movement.");
        Game.Instance.dataManager.LunaPlayer = null;
        try { ApplyKeyboard(ui, true, false); ApplyKeyboard(ui, false, false); }
        finally { Game.Instance.dataManager.LunaPlayer = player; }
        float startX = player.transform.position.x;
        simulatedUI = ui; simulatedLeft = false; simulatedRight = true; simulatedHeld = true;
        yield return new WaitForSecondsRealtime(1f);
        simulatedHeld = false; ApplyKeyboard(ui, false, false);
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
        int[][] sizes = { new[] { 1280, 720 }, new[] { 1920, 1080 }, new[] { 1024, 768 } };
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
            Require(capture && capture.width == size[0] && capture.height == size[1], "Render capture size mismatch.");
            Color32[] pixels = capture.GetPixels32(); double sum = 0, squares = 0; int visible = 0;
            foreach (Color32 pixel in pixels)
            {
                double luma = (pixel.r + pixel.g + pixel.b) / (3.0 * 255);
                sum += luma; squares += luma * luma; if (luma > 0.02) visible++;
            }
            double variance = squares / pixels.Length - Math.Pow(sum / pixels.Length, 2);
            Require(visible > pixels.Length / 20 && variance > 0.0001, "Captured render is blank or nearly uniform.");
            string path = Path.Combine(outputDirectory, "render-" + renderMode + "-" + size[0] + "x" + size[1] + ".png");
            File.WriteAllBytes(path, capture.EncodeToPNG()); Destroy(capture);
            Require(renderBoundsFailures.Count == 0, "Critical HUD rectangles exceed render bounds: " + string.Join("; ", renderBoundsFailures));
            foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
                Require(canvas.pixelRect.width > 0 && canvas.pixelRect.height > 0, "Active UI canvas has zero render area.");
            current.observations.Add("PNG=" + path + "; nonblack=" + visible + "/" + pixels.Length + "; luminance variance=" + variance.ToString("R"));
            EndCase(explicitOffscreen
                ? "Explicit offscreen pipeline render and critical HUD rectangle bounds measured at requested target dimensions; overlay canvases temporarily camera-space. This does not validate the OS window/backbuffer or unchanged overlay layout. PNG inspection remains separate."
                : "Graphics-enabled hidden player backbuffer captured; dimensions, nonblank pixels, critical HUD rectangle bounds and active canvas areas verified. PNG inspection is separate from pixel assertions.");
        }
    }
    private IEnumerator Soak()
    {
        int seconds = 300;
        string requested = Environment.GetEnvironmentVariable("LUNA_VALIDATION_SOAK_SECONDS");
        if (!string.IsNullOrEmpty(requested)) seconds = Math.Max(10, Math.Min(7200, int.Parse(requested, System.Globalization.CultureInfo.InvariantCulture)));
        BeginCase("soak", seconds + "-seconds-scene-combat-cycles");
        float start = Time.realtimeSinceStartup; int cycles = 0; long peak = 0, initial = Profiler.GetTotalAllocatedMemoryLong();
        int lobbyEntities = -1;
        while (Time.realtimeSinceStartup - start < seconds)
        {
            if (SceneManager.GetActiveScene().name != "Game" || !Active<UIPlayer>()) yield return LoadStage("Stage01-01");
            float battle = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - battle < 20f && Time.realtimeSinceStartup - start < seconds)
            {
                var result = Active<UIResult>(); if (result) { result.ReStage(); yield return new WaitForSecondsRealtime(8f); }
                peak = Math.Max(peak, Profiler.GetTotalAllocatedMemoryLong());
                yield return new WaitForSecondsRealtime(0.5f);
            }
            Game.Instance.lunaSceneManager.LoadScene("Lobby");
            yield return Until(() => SceneManager.GetActiveScene().name == "Lobby" && Active<UILobby>(), "Soak lobby return");
            yield return new WaitForSecondsRealtime(2f);
            CheckOwnership();
            if (lobbyEntities < 0) lobbyEntities = LunaEntities.ActiveCount;
            Require(LunaEntities.ActiveCount <= lobbyEntities + 2, "Soak accumulated active entities after lobby return.");
            Require(!Game.Instance.dataManager.ObjectSounds.Any(s => !s), "Soak retained destroyed AudioSources in the gameplay registry.");
            cycles++;
        }
        current.observations.Add("cycles=" + cycles + "; actualSeconds=" + (Time.realtimeSinceStartup - start).ToString("R") +
            "; initialAllocatedBytes=" + initial + "; peakAllocatedBytes=" + peak + "; finalAllocatedBytes=" + Profiler.GetTotalAllocatedMemoryLong() + "; finalNativeEntities=" + LunaEntities.ActiveCount);
        EndCase("Timed real scene/combat loops completed with no logged errors, stable lobby entity counts and no destroyed sound-registry entries. Memory sampled, not a proof of unlimited-session leak freedom.");
    }
}
#endif
