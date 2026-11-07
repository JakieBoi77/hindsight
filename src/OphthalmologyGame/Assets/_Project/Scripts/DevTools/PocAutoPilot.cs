using System.Collections;
using System.IO;
using System.Linq;
using Hindsight.Ophthalmology.SceneObjects;
using Hindsight.Ophthalmology.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hindsight.Ophthalmology.DevTools
{
    /// <summary>
    /// Smoke test: when the player is launched with "-autopilot &lt;dir&gt;" it plays through the menu and the
    /// Eye Drops level like a child would (including one deliberate blink), saving a screenshot at each
    /// new state and logging "AUTOPILOT ..." lines. Does nothing without the flag.
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
            Click("PlayButton");
            yield return new WaitForSeconds(1f);
            yield return Shot("menu_levels");
            var level = FindObjectsByType<Button>(FindObjectsSortMode.None)
                .Where(b => b.isActiveAndEnabled && b.interactable && b.transform.parent != null && b.transform.parent.name == "LevelList")
                .OrderBy(b => b.transform.GetSiblingIndex())
                .FirstOrDefault();
            Log($"clicking level '{level?.GetComponentInChildren<TMPro.TMP_Text>().text}'");
            level?.onClick.Invoke();
            yield return new WaitForSeconds(2.5f);

            var blinkedOnPurpose = false;
            var firstLine = true;
            var lastBlinkShot = false;
            var deadline = Time.time + 300f;
            while (Time.time < deadline)
            {
                if (Active("RewardPanel"))
                {
                    yield return new WaitForSeconds(1.5f);
                    yield return Shot("reward");
                    break;
                }

                var next = FindActive<Button>("NextButton");
                if (next != null)
                {
                    // Let the typewriter and camera blend finish (shorter for the first line, to catch the greeting wave).
                    yield return new WaitForSeconds(firstLine ? 0.8f : 1.6f);
                    firstLine = false;
                    yield return Shot("dialogue");
                    next.onClick.Invoke();
                    yield return new WaitForSeconds(0.3f);
                    continue;
                }

                if (Active("QuizPanel"))
                {
                    var option = FindObjectsByType<Button>(FindObjectsSortMode.None)
                        .Where(b => b.isActiveAndEnabled && b.interactable && b.name.StartsWith("Option"))
                        .OrderBy(b => b.name).FirstOrDefault();
                    if (option != null)
                    {
                        yield return Shot("quiz");
                        option.onClick.Invoke();
                        yield return new WaitForSeconds(0.6f);
                        yield return Shot("quiz_answered");
                    }
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }

                var hold = FindActive<HoldButton>("HoldButton");
                if (hold != null)
                {
                    yield return new WaitForSeconds(1f);
                    yield return Shot("hold_prompt");
                    Pointer(hold.gameObject, ExecuteEvents.pointerDownHandler);
                    if (!blinkedOnPurpose)
                    {
                        blinkedOnPurpose = true;
                        yield return new WaitForSeconds(0.5f);
                        yield return Shot("drop_falling");
                        Log("releasing early to test the blink/miss path");
                        Pointer(hold.gameObject, ExecuteEvents.pointerUpHandler);
                    }
                    else
                    {
                        while (hold.isActiveAndEnabled) yield return null;
                        Pointer(hold.gameObject, ExecuteEvents.pointerUpHandler);
                        yield return new WaitForSeconds(0.4f);
                        yield return Shot("drop_landed");
                    }
                    continue;
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

                var target = FindObjectsByType<Interactable>(FindObjectsSortMode.None).FirstOrDefault(t => t.Accepting);
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

            Log(Time.time >= deadline ? "TIMED OUT" : $"DONE errors={errorCount}");
            yield return new WaitForSeconds(0.5f);
            Application.Quit(errorCount == 0 && Time.time < deadline ? 0 : 1);
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

        static void Click(string name) => FindActive<Button>(name)?.onClick.Invoke();

        static void Pointer<T>(GameObject go, ExecuteEvents.EventFunction<T> handler) where T : IEventSystemHandler =>
            ExecuteEvents.Execute(go, new PointerEventData(EventSystem.current), handler);
    }
}
