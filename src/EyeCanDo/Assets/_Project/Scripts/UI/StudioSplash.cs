using System.Collections;
using Hindsight.EyeCanDo.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Hindsight.EyeCanDo.UI
{
    /// <summary>
    /// The "Hindsight" studio splash shown before the menu. The logo, a lamp's lens holding an H,
    /// sits unlit in the dark. A switch clicks, the lens lights up and throws a beam to the right;
    /// the view shifts over as the beam reaches along the rest of the name (INDSIGHT), which was
    /// standing there in shadow and shows as dark letters against the light. Then the menu loads.
    /// A tap skips it.
    ///
    /// The animation is a pure function of time (<see cref="Pose"/>), so it stays in step with its
    /// sound: one clip that starts with it. The cue times here are repeated in
    /// Tools/SplashGen/make_splash_audio.py, which makes the clip; change them together.
    /// With "less motion" on, the finished logo fades in and out instead.
    /// </summary>
    public sealed class StudioSplash : MonoBehaviour, IPointerDownHandler
    {
        const float MarkExtent = 0.8f;   // half size of the logo's quad, in lens diameters: room for the glow
        const float MarkSize = 2.4f;     // the lens's diameter in cap heights (its H is then 1.1 capitals tall)
        const float MarkGap = 0.3f;      // between the lens and the next letter's own side bearing, in cap heights
        const float CapBoost = 1.03f;    // bold text is drawn a little past its metrics

        // The timeline, in seconds.
        public const float Length = 3.3f;
        const float SwitchAt = 0.75f;    // the click: the lens lights and the beam starts
        const float Revealed = 1.50f;    // the whole name is in the light
        const float FadeOut = 0.4f, SkipFade = 0.25f, StillLength = 2.4f;

        static readonly Color Inner = Hex("#0E1528"), Outer = Hex("#03050A"), Glow = Hex("#8FE4FF"),
            Light = Hex("#BDEEFF"), Shadow = Hex("#04070E"), Curtain = Hex("#020308"),
            UnlitLine = Hex("#7387AD"), UnlitLens = Hex("#121A2B");

        [Tooltip("Stay in this scene when it ends instead of loading the menu (the recorder).")]
        [SerializeField] bool stay;
        [SerializeField] RectTransform lockup;
        [SerializeField] RawImage backdrop;
        [SerializeField] RawImage mark;
        [SerializeField] TextMeshProUGUI word;
        [SerializeField] Image fader;
        [SerializeField] AudioSource sting;

        /// <summary>True once the splash has faded out.</summary>
        public bool Finished { get; private set; }

        Material markMaterial, backdropMaterial;
        RectTransform markRect, wordRect, backdropRect;
        float capHeight, lens, markX, wordRight; // lens: its diameter in canvas units
        float[] letterX = new float[0];
        bool playing, still, laidOut;
        float clock, skipAt = -1f;

        // ---- the pose: what the timeline sets each frame
        float zoom, centreOnMark, lit, beamFront, curtain;

        static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        void Awake()
        {
            markRect = (RectTransform)mark.transform;
            wordRect = (RectTransform)word.transform;
            backdropRect = (RectTransform)backdrop.transform;
            markMaterial = new Material(mark.material);
            backdropMaterial = new Material(backdrop.material);
            mark.material = markMaterial;
            backdrop.material = backdropMaterial;
            fader.color = Curtain;
        }

        IEnumerator Start()
        {
            // The first frames after a scene load are long; start on a steady one so sound and picture agree.
            yield return null;
            yield return null;
            if (!playing && !Finished) Play();
        }

        void OnDestroy()
        {
            Destroy(markMaterial);
            Destroy(backdropMaterial);
        }

        /// <summary>Plays the splash and stays in this scene afterwards (the recorder).</summary>
        public void Preview()
        {
            stay = true;
            Play();
        }

        void Play()
        {
            still = GameServices.Settings.reduceMotion;
            if (!laidOut) Layout();
            clock = 0f;
            skipAt = -1f;
            Finished = false;
            playing = true;

            sting.Stop();
            sting.volume = GameServices.Settings.soundVolume;
            // Less motion: the logo fades up already lit, so there is no switch to hear.
            if (sting.clip != null && !still) sting.Play();
            Pose(0f);
            Apply();
        }

        void Update()
        {
            if (!playing) return;
            clock += Time.deltaTime;
            Pose(clock);
            var end = still ? StillLength : Length;
            if (skipAt >= 0f)
            {
                var k = Mathf.Clamp01((clock - skipAt) / SkipFade);
                curtain = Mathf.Max(curtain, k);
                sting.volume = GameServices.Settings.soundVolume * (1f - k);
                end = Mathf.Min(end, skipAt + SkipFade);
            }
            Apply();
            if (clock < end) return;

            playing = false;
            Finished = true;
            if (!stay) SplashCurtain.CoverAndLoad(Curtain, sting, GameFlow.MenuScene);
        }

        /// <summary>A tap anywhere moves on.</summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (stay || !playing || skipAt >= 0f || clock < 0.3f) return;
            if (clock < (still ? StillLength : Length) - FadeOut) skipAt = clock;
        }

        /// <summary>
        /// Sizes the logo from the text and centres it on the line of capitals, with the logo plus
        /// INDSIGHT centred as one word.
        /// </summary>
        void Layout()
        {
            word.ForceMeshUpdate();
            var info = word.textInfo;
            var count = info.characterCount;
            if (count == 0) return;
            var first = info.characterInfo[0];
            var last = info.characterInfo[count - 1];
            var face = word.font.faceInfo;
            var cap = face.capLine / face.pointSize * word.fontSize;
            capHeight = cap * CapBoost;
            lens = capHeight * MarkSize;
            var gap = MarkGap * capHeight;
            // Pen positions (origin to advance), not the quads: those include the glyph padding.
            var left = -(lens + gap + last.xAdvance - first.origin) * 0.5f;
            markX = left + lens * 0.5f;
            wordRect.anchoredPosition = new Vector2(left + lens + gap - first.origin, -(first.baseLine + cap * 0.5f));
            wordRight = wordRect.anchoredPosition.x + last.xAdvance;
            markRect.anchoredPosition = new Vector2(markX, 0f);
            markRect.sizeDelta = Vector2.one * (2f * MarkExtent * lens);
            letterX = new float[count];
            for (var i = 0; i < count; i++)
                letterX[i] = wordRect.anchoredPosition.x + (info.characterInfo[i].origin + info.characterInfo[i].xAdvance) * 0.5f;
            laidOut = true;
        }

        // ------------------------------------------------------------------ timeline

        /// <summary>
        /// 0-0.75 the unlit logo waits in the dark · 0.75 click: the lens flickers on and the beam starts ·
        /// 0.85-1.55 the view shifts across and pulls back while the beam's front runs along the name, each
        /// letter showing as a shadow once the light is behind it · 1.50 the whole name is lit · hold · 2.9 fade out.
        /// </summary>
        void Pose(float t)
        {
            if (still)
            {
                // The finished, lit logo, fading in and out.
                curtain = Mathf.Max(1f - Smooth(Seg(t, 0f, 0.5f)), Smooth(Seg(t, StillLength - FadeOut, StillLength)));
                t = Length - FadeOut;
            }
            else
            {
                curtain = Mathf.Max(1f - Smooth(Seg(t, 0f, 0.3f)), Smooth(Seg(t, Length - FadeOut, Length)));
            }

            // Like a lamp catching: on, a brief dip, then steady.
            var s = t - SwitchAt;
            lit = s < 0f ? 0f : s < 0.03f ? s / 0.03f : s < 0.07f ? Mathf.Lerp(1f, 0.45f, (s - 0.03f) / 0.04f) : Mathf.Lerp(0.45f, 1f, Seg(s, 0.07f, 0.13f));

            // Only once the light is on does the view move.
            var pull = InOutCubic(Seg(t, SwitchAt + 0.1f, Revealed + 0.05f));
            zoom = Mathf.Lerp(1.5f, 1f, pull) * (1f + 0.03f * Seg(t, Revealed, Length));
            centreOnMark = 1f - pull;

            // The beam's front, in the lockup's own space: along the name, then away off screen.
            var away = Seg(t, Revealed - 0.1f, Revealed + 0.5f);
            beamFront = Mathf.Lerp(markX, wordRight + capHeight * 0.7f, Smooth(Seg(t, SwitchAt, Revealed))) + 3000f * away * away;
        }

        static float Seg(float t, float from, float to) => Mathf.Clamp01((t - from) / (to - from));
        static float Smooth(float x) => x * x * (3f - 2f * x);
        static float InOutCubic(float x) => x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) * 0.5f;

        // ------------------------------------------------------------------ applying a pose

        static readonly int GlowColorId = Shader.PropertyToID("_GlowColor"), LensId = Shader.PropertyToID("_Lens"), InkId = Shader.PropertyToID("_Ink"),
            OffId = Shader.PropertyToID("_Off"), OffFillId = Shader.PropertyToID("_OffFill"),
            ExtentId = Shader.PropertyToID("_Extent"), LitId = Shader.PropertyToID("_Lit"),
            InnerId = Shader.PropertyToID("_Inner"), OuterId = Shader.PropertyToID("_Outer"), SizeId = Shader.PropertyToID("_Size"),
            SpotId = Shader.PropertyToID("_Spot"), SpotColorId = Shader.PropertyToID("_SpotColor"),
            BeamId = Shader.PropertyToID("_Beam"), BeamShapeId = Shader.PropertyToID("_BeamShape"), BeamColorId = Shader.PropertyToID("_BeamColor");

        void Apply()
        {
            if (!laidOut) return;

            // The "camera": the whole lockup zooms, centred first on the logo and then on the full name.
            lockup.localScale = Vector3.one * zoom;
            lockup.anchoredPosition = new Vector2(-markX * zoom * centreOnMark, 0f);

            markMaterial.SetColor(GlowColorId, Glow);
            markMaterial.SetColor(LensId, Light);
            markMaterial.SetColor(InkId, Shadow);
            markMaterial.SetColor(OffId, UnlitLine);
            markMaterial.SetColor(OffFillId, UnlitLens);
            markMaterial.SetFloat(ExtentId, MarkExtent);
            markMaterial.SetFloat(LitId, lit);

            // The lens's place on screen, for the light it throws on the backdrop.
            var lamp = lockup.anchoredPosition + new Vector2(markX, 0f) * zoom;
            backdropMaterial.SetColor(InnerId, Inner);
            backdropMaterial.SetColor(OuterId, Outer);
            backdropMaterial.SetVector(SizeId, backdropRect.rect.size);
            backdropMaterial.SetColor(SpotColorId, Glow);
            backdropMaterial.SetVector(SpotId, new Vector4(lamp.x, lamp.y, lens * 0.85f * zoom, 0.3f * lit));
            backdropMaterial.SetColor(BeamColorId, Light);
            // The beam comes out from behind the lens, so its start is as round as the lens:
            // half height, spread, soft front, length of the round nose.
            backdropMaterial.SetVector(BeamShapeId, new Vector4(capHeight * 0.82f * zoom, 0.09f, capHeight * 1.6f * zoom, lens * 0.5f * zoom));
            backdropMaterial.SetVector(BeamId, new Vector4(lamp.x, lamp.y, (beamFront - markX) * zoom, 0.8f * lit));

            // Each letter is a shadow: it shows once the light has got behind it.
            word.ForceMeshUpdate();
            var info = word.textInfo;
            for (var i = 0; i < info.characterCount && i < letterX.Length; i++)
            {
                var ch = info.characterInfo[i];
                if (!ch.isVisible) continue;
                var tint = Shadow;
                tint.a = Smooth(Seg(beamFront - letterX[i], capHeight * 0.1f, capHeight * 0.9f)) * lit;
                var colors = info.meshInfo[ch.materialReferenceIndex].colors32;
                for (var corner = 0; corner < 4; corner++) colors[ch.vertexIndex + corner] = tint;
            }
            word.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            fader.color = new Color(Curtain.r, Curtain.g, Curtain.b, curtain);
        }

#if UNITY_EDITOR
        public void EditorInit(RectTransform lockupRoot, RawImage backdropImage, RawImage markImage, TextMeshProUGUI wordText,
            Image faderImage, AudioSource stingSource)
        {
            lockup = lockupRoot;
            backdrop = backdropImage;
            mark = markImage;
            word = wordText;
            fader = faderImage;
            sting = stingSource;
        }
#endif
    }

    /// <summary>
    /// Covers the screen in the splash's last colour while the next scene loads, then fades away, so
    /// the menu never pops in. It also carries the sting, so its tail rings out over the menu.
    /// </summary>
    public sealed class SplashCurtain : MonoBehaviour
    {
        Image cover;
        AudioSource tail;

        public static void CoverAndLoad(Color color, AudioSource sting, string scene)
        {
            var go = new GameObject("SplashCurtain");
            DontDestroyOnLoad(go);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            var curtain = go.AddComponent<SplashCurtain>();
            curtain.cover = new GameObject("Cover", typeof(RectTransform)).AddComponent<Image>();
            var rect = curtain.cover.rectTransform;
            rect.SetParent(go.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            curtain.cover.color = color;
            curtain.cover.raycastTarget = false;
            if (sting != null)
            {
                sting.transform.SetParent(go.transform, false);
                curtain.tail = sting;
            }
            SceneManager.LoadScene(scene);
        }

        IEnumerator Start()
        {
            yield return null; // the new scene is up after this frame
            yield return null;
            var color = cover.color;
            for (var k = 0f; k < 1f; k += Mathf.Min(Time.unscaledDeltaTime, 1f / 30f) / 0.45f)
            {
                cover.color = new Color(color.r, color.g, color.b, 1f - k * k * (3f - 2f * k));
                yield return null;
            }
            cover.enabled = false;
            while (tail != null && tail.isPlaying) yield return null;
            Destroy(gameObject);
        }
    }
}
