using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hindsight.Ophthalmology.Core;
using Hindsight.Ophthalmology.SceneObjects;
using Hindsight.Ophthalmology.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hindsight.Ophthalmology.DevTools
{
    /// <summary>
    /// Smoke test: when the player is launched with "-autopilot &lt;dir&gt;" it plays like a child would:
    /// picks Save 1, plays the Eye Drops storyboard (taps Sam, the pupil and the things to discover,
    /// lets go once while the drop falls), collects the
    /// sticker, then buys a hat in Dress Up. It saves a screenshot at each new state, logs
    /// "AUTOPILOT ..." lines and keeps its saves in &lt;dir&gt;/data. Does nothing without the flag.
    /// </summary>
    public sealed class PocAutoPilot : MonoBehaviour
    {
        string outDir;
        int shotIndex;
        int errorCount;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args = System.Environment.GetCommandLineArgs();
            var i = System.Array.IndexOf(args, "-autopilot");
            if (i < 0) return;
            var go = new GameObject("PocAutoPilot");
            DontDestroyOnLoad(go);
            var pilot = go.AddComponent<PocAutoPilot>();
            pilot.outDir = i + 1 < args.Length ? args[i + 1] : "autopilot";
            Directory.CreateDirectory(pilot.outDir);
            GameServices.UseTestData(Path.Combine(pilot.outDir, "data")); // never touch real saves
        }

        void OnEnable() => Application.logMessageReceived += OnLog;
        void OnDisable() => Application.logMessageReceived -= OnLog;

        void OnLog(string message, string stack, LogType type)
        {
            if (type is LogType.Error or LogType.Exception or LogType.Assert) errorCount++;
        }

        IEnumerator Start()
        {
            yield return new WaitForSeconds(2f);
            yield return Shot("menu_title");
            yield return Burst("anim_title_doctor", 6, 0.2f);
            Click("PlayButton");
            yield return new WaitForSeconds(1f);
            yield return Shot("menu_saves");
            Click("SaveCard1");
            yield return new WaitForSeconds(1.2f);
            yield return Shot("menu_levels");
            var level = FindObjectsByType<Button>()
                .Where(b => b.isActiveAndEnabled && b.interactable && b.transform.parent != null && b.transform.parent.name == "LevelList")
                .OrderBy(b => b.transform.GetSiblingIndex())
                .FirstOrDefault();
            Log($"clicking level '{level?.GetComponentInChildren<TMPro.TMP_Text>().text}'");
            level?.onClick.Invoke();
            yield return new WaitForSeconds(2.5f);

            var deadline = Time.time + 400f;
            var lipSync = StartCoroutine(WatchMouths());
            yield return PlayLevel(deadline);
            StopCoroutine(lipSync);
            ReportMouths();
            yield return new WaitForSeconds(2f);
            yield return DressUp();

            var saved = GameServices.Saves.Peek(0);
            Log($"save 1: coins={saved?.coins} stickers={saved?.stickers.Count} owned={saved?.owned.Count} hat={saved?.look.hat}");
            if (saved == null || saved.stickers.Count == 0 || saved.owned.Count == 0)
            {
                Log("FAILED: expected a sticker and a bought item in Save 1");
                errorCount++;
            }

            Log(Time.time >= deadline ? "TIMED OUT" : $"DONE errors={errorCount}");
            yield return new WaitForSeconds(0.5f);
            Application.Quit(errorCount == 0 && Time.time < deadline ? 0 : 1);
        }

        IEnumerator PlayLevel(float deadline)
        {
            var released = false;
            var pressed = false;
            var lastBlinkShot = false;
            var tapped = new HashSet<string>();
            var lastCaption = "";
            var lastShot = "";
            var coinsShot = false;
            var director = FindAnyObjectByType<ShotDirector>();
            while (Time.time < deadline)
            {
                // Each new camera shot: a still, and frame bursts where something moves (walking, the chart lighting up).
                if (director != null && director.CurrentShot != lastShot)
                {
                    lastShot = director.CurrentShot;
                    if (lastShot == "ChartTalk") StartCoroutine(Burst("anim_chart", 8, 1.2f));
                    else if (lastShot == "DoorView") StartCoroutine(Burst("anim_sam_leaves", 6, 0.6f));
                    else if (lastShot == "ChairApproach") StartCoroutine(Burst("anim_chair_approach", 6, 0.5f));
                    else if (lastShot == "SamChair") StartCoroutine(Burst("anim_sam_chair", 6, 1f));
                    else StartCoroutine(Shot("shot_" + lastShot));
                }

                if (Active("RewardPanel"))
                {
                    if (!coinsShot && Active("CoinChip"))
                    {
                        coinsShot = true;
                        yield return Burst("anim_coins", 6, 0.15f);
                    }
                    var home = FindActive<Button>("HomeButton");
                    if (home != null && Active("EndButtons"))
                    {
                        yield return new WaitForSeconds(1f);
                        yield return Shot("end_card");
                        home.onClick.Invoke();
                        yield break;
                    }
                }

                // Things to discover glow during a panel; tap each once before moving on.
                var discover = FindObjectsByType<Interactable>().FirstOrDefault(t => t.Accepting && !tapped.Contains(t.Id));
                if (discover != null)
                {
                    tapped.Add(discover.Id);
                    yield return new WaitForSeconds(0.8f);
                    // Frame bursts so the animation can be checked from stills.
                    if (discover.Id == "ExamChair") yield return Burst("anim_kid_wave", 6, 0.15f);
                    yield return Shot("tap_" + discover.Id);
                    Pointer(discover.gameObject, ExecuteEvents.pointerClickHandler);
                    if (discover.Id == "Sam") yield return Burst("anim_sam_wave", 8, 0.15f);
                    yield return new WaitForSeconds(0.6f);
                    continue;
                }

                var hold = FindActive<HoldButton>("HoldButton");
                if (hold != null)
                {
                    var ring = FindActive<Image>("Ring");
                    if (!pressed)
                    {
                        yield return new WaitForSeconds(0.8f);
                        yield return Shot("hold_prompt");
                        Pointer(hold.gameObject, ExecuteEvents.pointerDownHandler);
                        pressed = true;
                    }
                    else if (!released && ring != null && ring.fillAmount > 0.3f)
                    {
                        released = true;
                        Log("letting go mid-drop to test the pause");
                        Pointer(hold.gameObject, ExecuteEvents.pointerUpHandler);
                        yield return new WaitForSeconds(0.6f);
                        yield return Shot("eyes_open_reminder");
                        yield return new WaitForSeconds(0.8f);
                        Pointer(hold.gameObject, ExecuteEvents.pointerDownHandler);
                    }
                    yield return null;
                    continue;
                }
                if (pressed && hold == null)
                {
                    pressed = false;
                    var buttonGo = GameObject.Find("ActionButtons");
                    var holdAny = buttonGo != null ? buttonGo.GetComponentInChildren<HoldButton>(true) : null;
                    if (holdAny != null) Pointer(holdAny.gameObject, ExecuteEvents.pointerUpHandler);
                    yield return new WaitForSeconds(0.4f);
                    yield return Shot("drop_landed");
                }

                var blink = FindActive<Button>("BlinkButton");
                if (blink != null)
                {
                    if (!lastBlinkShot) yield return Shot("blink");
                    lastBlinkShot = true;
                    blink.onClick.Invoke();
                    yield return new WaitForSeconds(0.45f);
                    continue;
                }

                var next = FindActive<Button>("NextButton");
                if (next != null)
                {
                    var panel = GameObject.Find("CaptionPanel");
                    var caption = panel != null ? panel.transform.Find("Fill/Caption")?.GetComponent<TMPro.TMP_Text>() : null;
                    var text = caption != null ? caption.text : "";
                    if (text != lastCaption)
                    {
                        lastCaption = text;
                        yield return new WaitForSeconds(0.4f);
                        yield return Shot("panel");
                    }
                    next.onClick.Invoke();
                    yield return new WaitForSeconds(0.3f);
                    continue;
                }

                var target = FindObjectsByType<Interactable>().FirstOrDefault(t => t.Accepting);
                if (target != null)
                {
                    yield return new WaitForSeconds(0.8f);
                    yield return Shot("tap_" + target.Id);
                    Pointer(target.gameObject, ExecuteEvents.pointerClickHandler);
                    yield return new WaitForSeconds(0.4f);
                    continue;
                }

                yield return null;
            }
        }

        // Lip sync check: while a character talks, how often each mouth shape shows. A mouth that
        // follows the voice uses all of them; one stuck shut or wide open would not.
        readonly Dictionary<string, int[]> mouthFrames = new();

        IEnumerator WatchMouths()
        {
            while (true)
            {
                foreach (var rig in FindObjectsByType<BeanRig>())
                {
                    if (!rig.IsTalking || rig.transform.position.x < -30f) continue; // the room's characters, not the portrait copies
                    if (!mouthFrames.TryGetValue(rig.name, out var counts)) mouthFrames[rig.name] = counts = new int[FaceArt.MouthFrames];
                    counts[rig.MouthFrame]++;
                }
                yield return null;
            }
        }

        void ReportMouths()
        {
            foreach (var (who, counts) in mouthFrames)
            {
                var total = Mathf.Max(1, counts.Sum());
                Log($"lip sync {who}: " + string.Join(" ", counts.Select((c, i) => $"shape{i}={100f * c / total:0}%")));
                if (counts.Count(c => c > 0) < 3)
                {
                    Log($"FAILED: {who}'s mouth used fewer than 3 shapes while talking");
                    errorCount++;
                }
            }
            if (mouthFrames.Count == 0)
            {
                Log("FAILED: nobody talked");
                errorCount++;
            }
        }

        IEnumerator DressUp()
        {
            yield return Shot("menu_levels_after");
            Click("DressUpButton");
            yield return new WaitForSeconds(1.2f);
            yield return Shot("dressup");
            // Spin the kid round by dragging over them.
            var spin = GameObject.Find("SpinArea");
            if (spin != null)
            {
                for (var i = 0; i < 6; i++)
                {
                    ExecuteEvents.Execute(spin, new PointerEventData(EventSystem.current) { delta = new Vector2(-60f, 0f) }, ExecuteEvents.dragHandler);
                    yield return null;
                }
                ExecuteEvents.Execute(spin, new PointerEventData(EventSystem.current), ExecuteEvents.endDragHandler);
                yield return new WaitForSeconds(0.8f);
                yield return Shot("dressup_spun");
            }
            else
            {
                Log("no SpinArea in Dress up");
            }
            Click("Tab_Fits");
            yield return new WaitForSeconds(0.5f);
            yield return Shot("dressup_fits");
            foreach (var tab in new[] { "Eyes", "Brows", "Mouth" })
            {
                Click("Tab_" + tab);
                yield return new WaitForSeconds(0.5f);
                var second = FindObjectsByType<Button>().Where(b => b.isActiveAndEnabled && b.name.StartsWith("Item_"))
                    .OrderBy(b => b.transform.GetSiblingIndex()).Skip(1).FirstOrDefault();
                second?.onClick.Invoke();
                yield return new WaitForSeconds(0.5f);
                yield return Shot("dressup_" + tab.ToLowerInvariant());
            }
            Click("Tab_Hats");
            yield return new WaitForSeconds(0.5f);
            var coins = GameServices.Progress.coins;
            var item = FindObjectsByType<Button>()
                .Where(b => b.isActiveAndEnabled && b.name.StartsWith("Item_") && b.transform.Find("Price").gameObject.activeSelf)
                .Select(b => (b, price: int.Parse(b.transform.Find("Price").GetComponentInChildren<TMPro.TMP_Text>().text)))
                .Where(x => x.price <= coins)
                .OrderBy(x => x.price)
                .Select(x => x.b)
                .FirstOrDefault();
            if (item == null)
            {
                Log($"no affordable hat with {coins} coins");
                yield break;
            }
            Log($"trying on {item.name}");
            item.onClick.Invoke();
            yield return new WaitForSeconds(0.6f);
            yield return Shot("dressup_try_on");
            Click("BuyButton");
            yield return new WaitForSeconds(0.6f);
            yield return Shot("dressup_bought");
            Click("DoneButton");
            yield return new WaitForSeconds(1f);
            yield return Shot("menu_after_dressup");
        }

        IEnumerator Burst(string label, int count, float interval)
        {
            for (var i = 0; i < count; i++)
            {
                yield return Shot($"{label}_{i}");
                yield return new WaitForSeconds(interval);
            }
        }

        IEnumerator Shot(string label)
        {
            yield return new WaitForEndOfFrame();
            var file = Path.Combine(outDir, $"{++shotIndex:000}_{label}.png");
            ScreenCapture.CaptureScreenshot(file);
            // The capture happens at the end of a later frame; don't change the screen before it does.
            yield return null;
            yield return new WaitForEndOfFrame();
            Log($"shot {file}");
        }

        static void Log(string message) => Debug.Log("AUTOPILOT " + message);

        static bool Active(string name) => GameObject.Find(name) != null;

        static T FindActive<T>(string name) where T : Component
        {
            var go = GameObject.Find(name);
            return go != null && go.activeInHierarchy ? go.GetComponent<T>() : null;
        }

        static void Click(string name)
        {
            var button = FindActive<Button>(name);
            if (button == null) Log($"button '{name}' not found");
            button?.onClick.Invoke();
        }

        static void Pointer<T>(GameObject go, ExecuteEvents.EventFunction<T> handler) where T : IEventSystemHandler =>
            ExecuteEvents.Execute(go, new PointerEventData(EventSystem.current), handler);
    }
}
