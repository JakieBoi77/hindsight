using System;
using System.Collections;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.Content;
using EyeCanDo.Core.Animation;
using EyeCanDo.Core.Content;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EyeCanDo.UI
{
    /// <summary>
    /// Captions standing in for narration. They always appear in the same strip across the top of
    /// the screen, between the Home and Skip buttons, so they never cover the characters or the
    /// play area. The speaker's face is on the left and a pictogram on the right (snowflake for
    /// cold, clock for "a few seconds"), so the meaning does not depend on reading. A line types
    /// itself out; a tap shows all of it at once, and the next tap continues. Recorded narration
    /// in <see cref="CaptionLine.narration"/> plays automatically when present. Text size, typing
    /// speed and auto-continue follow the settings, and change at once when they do.
    /// </summary>
    public sealed class CaptionBar : MonoBehaviour, IPointerClickHandler
    {
        public const float Width = 1280f;
        public const float Height = 160f;

        /// <summary>The bar's height with big text: still clear of the play area.</summary>
        public const float BigHeight = 186f;

        private const float RevealCharsPerSecond = 55f;

        /// <summary>With auto-continue on, how long a finished line stays before moving on.</summary>
        public const float AutoContinueSeconds = 1.6f;

        private RectTransform root;
        private CanvasGroup group;
        private RectTransform portrait;
        private RectTransform portraitHeadRoot;
        private Image portraitHead;
        private Image portraitEyes;
        private Image portraitMouth;
        private Image portraitGlasses;
        private Characters.AvatarHead kidPortrait;
        private Speaker portraitSpeaker = Speaker.None;
        private TextMeshProUGUI label;
        private Image pictogram;
        private RectTransform next;
        private RectTransform tapCatcher;
        private AudioSource voice;
        private bool waitingForTap;
        private bool reading;
        private bool tapped;
        private bool visible;
        private Coroutine nextBounce;
        private Coroutine reveal;

        /// <summary>Raised when a line starts: who says it, and for roughly how long.</summary>
        public event Action<Speaker, float> Spoke;

        /// <summary>Raised when a tap cuts a line short, so the speaker can stop talking too.</summary>
        public event Action Hurried;

        /// <summary>The kid shown for <see cref="Speaker.You"/>.</summary>
        public Core.Avatar.AvatarLook YouLook { get; set; } = Core.Avatar.AvatarLook.Default;

        public bool IsWaitingForTap => waitingForTap;

        /// <summary>A line is still appearing; a tap now shows it all.</summary>
        public bool IsReading => reading;

        /// <summary>While set (the skip button is fast-forwarding), lines continue without a tap.</summary>
        public bool AutoContinue { get; set; }

        public bool IsVisible => visible;

        public string CurrentText => label != null ? label.text : null;

        public static CaptionBar Create(RectTransform overlay)
        {
            var theme = UiTheme.Instance;

            // Taps anywhere continue a line, but only while one is waiting.
            var catcher = Ui.Rect(overlay, "TapToContinue");
            Ui.Stretch(catcher);
            var catcherImage = catcher.gameObject.AddComponent<Image>();
            catcherImage.color = Color.clear;
            catcher.gameObject.SetActive(false);

            var rect = Ui.Rect(overlay, "CaptionBar");
            Ui.Place(rect, new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(Width, Height), new Vector2(0.5f, 1f));
            var bar = rect.gameObject.AddComponent<CaptionBar>();
            bar.root = rect;
            bar.tapCatcher = catcher;
            catcher.gameObject.AddComponent<CaptionTapRelay>().Owner = bar;
            bar.group = rect.gameObject.AddComponent<CanvasGroup>();
            bar.group.alpha = 0f;
            bar.group.blocksRaycasts = false;

            var face = Ui.Panel(rect, "Panel", theme.paper);
            Ui.Stretch((RectTransform)face.parent);

            bar.portrait = Ui.Disc(face, "Portrait", theme.soft);
            var portraitRoot = (RectTransform)bar.portrait.parent;
            Ui.Place(portraitRoot, new Vector2(0f, 0.5f), new Vector2(18f + 66f, 0f), new Vector2(132f, 132f));
            var mask = Ui.Image(bar.portrait, "Mask", Shapes.Circle, Color.white);
            Ui.Stretch(mask.rectTransform);
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            var head = Ui.Rect(mask.rectTransform, "Head");
            Ui.Place(head, new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(150f, 150f));
            bar.portraitHeadRoot = head;
            bar.portraitHead = Layer(head, "HeadArt");
            bar.portraitEyes = Layer(head, "Eyes");
            bar.portraitMouth = Layer(head, "Mouth");
            bar.portraitGlasses = Layer(head, "Glasses");

            bar.label = Ui.Text(face, "", theme.captionSize, theme.ink, TextAlignmentOptions.MidlineLeft);
            bar.label.rectTransform.anchorMin = Vector2.zero;
            bar.label.rectTransform.anchorMax = Vector2.one;
            bar.label.rectTransform.offsetMin = new Vector2(176f, 12f);
            bar.label.rectTransform.offsetMax = new Vector2(-200f, -12f);
            bar.label.fontSizeMin = theme.captionSize * 0.7f;

            bar.pictogram = Ui.Image(face, "Pictogram", null, Color.white);
            bar.pictogram.preserveAspect = true;
            Ui.Place(bar.pictogram.rectTransform, new Vector2(1f, 0.5f), new Vector2(-122f, 0f), new Vector2(100f, 100f));

            var nextFace = Ui.Disc(face, "Next", theme.accent);
            bar.next = (RectTransform)nextFace.parent;
            Ui.Place(bar.next, new Vector2(1f, 0f), new Vector2(-12f, 12f), new Vector2(76f, 76f), new Vector2(1f, 0f));
            var arrow = Ui.Image(nextFace, "Arrow", ArtLibrary.Get("Icons/arrow_right"), Color.white);
            Ui.Stretch(arrow.rectTransform, 16f);
            arrow.preserveAspect = true;
            bar.next.gameObject.SetActive(false);

            bar.voice = rect.gameObject.AddComponent<AudioSource>();
            bar.voice.playOnAwake = false;
            bar.ApplyTextSize();
            GameServices.SettingsChanged += bar.ApplyTextSize;
            return bar;
        }

        private void OnDestroy()
        {
            GameServices.SettingsChanged -= ApplyTextSize;
        }

        /// <summary>Sizes the text and the bar for the Text size setting.</summary>
        private void ApplyTextSize()
        {
            if (label == null)
            {
                return;
            }

            var theme = UiTheme.Instance;
            var scale = GameServices.Settings.TextScale;
            root.sizeDelta = new Vector2(Width, scale > 1f ? BigHeight : Height);
            label.fontSize = theme.captionSize * scale;
            label.fontSizeMax = theme.captionSize * scale;
            label.fontSizeMin = theme.captionSize * 0.7f * scale;
        }

        private static Image Layer(RectTransform parent, string name)
        {
            var image = Ui.Image(parent, name, null, Color.white);
            Ui.Stretch(image.rectTransform);
            return image;
        }

        /// <summary>Shows a line and waits: for the reading time, then (if asked) for a tap.</summary>
        public IEnumerator Say(CaptionLine line, bool waitForTap = true)
        {
            if (line == null || string.IsNullOrEmpty(line.text))
            {
                yield break;
            }

            Show(line);
            var minimum = ContentRules.MinimumSeconds(line.text);
            if (line.narration != null && Sfx.Enabled)
            {
                voice.clip = line.narration;
                voice.Play();
                minimum = Mathf.Max(minimum * 0.5f, line.narration.length);
            }

            if (!waitForTap)
            {
                yield return Tween.Wait(minimum);
                yield break;
            }

            yield return ReadingTime(minimum);
            yield return WaitForTap();
        }

        /// <summary>Shows an instruction that one tap continues straight away, with no reading wait.</summary>
        public IEnumerator Instruct(CaptionLine line)
        {
            if (line == null || string.IsNullOrEmpty(line.text))
            {
                yield break;
            }

            Show(line);
            yield return WaitForTap();
            FinishReveal();
        }

        /// <summary>Waits while the line is read, unless a tap asks for all of it at once.</summary>
        private IEnumerator ReadingTime(float seconds)
        {
            tapped = false;
            reading = true;
            tapCatcher.gameObject.SetActive(true);
            tapCatcher.SetAsFirstSibling();
            for (var time = 0f; time < seconds && !tapped && !AutoContinue; time += Time.deltaTime)
            {
                yield return null;
            }

            reading = false;
            tapCatcher.gameObject.SetActive(false);
            if (tapped)
            {
                FinishReveal();
                Hurried?.Invoke();
                yield return null;
            }
        }

        private void FinishReveal()
        {
            if (reveal != null)
            {
                StopCoroutine(reveal);
                reveal = null;
            }

            label.maxVisibleCharacters = 99999;
            SetPortraitMouth(false);
        }

        /// <summary>Shows a line and returns at once, for prompts that stay up during an action.</summary>
        public void Show(CaptionLine line)
        {
            if (line == null || string.IsNullOrEmpty(line.text))
            {
                return;
            }

            SetPortrait(line.speaker);
            label.text = line.text;
            label.maxVisibleCharacters = 0;
            var picture = Ui.PictogramFor(line.icon);
            pictogram.sprite = picture;
            pictogram.gameObject.SetActive(picture != null);
            label.rectTransform.offsetMax = new Vector2(picture != null ? -200f : -100f, -12f);
            StopAllCoroutines();
            nextBounce = null;
            reading = false;
            next.gameObject.SetActive(false);
            reveal = StartCoroutine(Reveal());
            if (!visible)
            {
                visible = true;
                group.blocksRaycasts = true;
                StartCoroutine(SlideIn());
            }
            else
            {
                StartCoroutine(Tween.Bump(root, 0.03f, 0.2f));
            }

            Spoke?.Invoke(line.speaker, Mathf.Clamp(line.text.Length / 16f, 0.6f, 2.6f));
        }

        public void Hide()
        {
            if (!visible)
            {
                return;
            }

            visible = false;
            waitingForTap = false;
            reading = false;
            tapCatcher.gameObject.SetActive(false);
            voice.Stop();
            StopAllCoroutines();
            group.blocksRaycasts = false;
            StartCoroutine(Tween.Run(0.2f, Ease.Linear, t => group.alpha = 1f - t));
        }

        public IEnumerator WaitForTap()
        {
            tapped = false;
            waitingForTap = true;
            tapCatcher.gameObject.SetActive(true);
            tapCatcher.SetAsFirstSibling();
            next.gameObject.SetActive(true);
            nextBounce = StartCoroutine(BounceNext());
            var waited = 0f;
            while (!tapped && !AutoContinue && !(GameServices.Settings.autoContinue && waited >= AutoContinueSeconds))
            {
                waited += Time.deltaTime;
                yield return null;
            }

            waitingForTap = false;
            tapCatcher.gameObject.SetActive(false);
            next.gameObject.SetActive(false);
            if (nextBounce != null)
            {
                StopCoroutine(nextBounce);
                nextBounce = null;
            }

            Sfx.Play(Sound.Tap);
        }

        /// <summary>Test hook: acts as a tap on the screen while a line appears or waits.</summary>
        public void SimulateTap()
        {
            if (waitingForTap || reading)
            {
                tapped = true;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SimulateTap();
        }

        private void SetPortrait(Speaker speaker)
        {
            portrait.parent.gameObject.SetActive(speaker != Speaker.None);
            var doctor = speaker == Speaker.Doctor;
            portraitHead.sprite = doctor ? ArtLibrary.Get("Characters/doctor_head") : null;
            portraitEyes.sprite = doctor ? ArtLibrary.Get("Characters/doctor_eyes_open") : null;
            portraitMouth.sprite = doctor ? ArtLibrary.Get("Characters/doctor_mouth_smile") : null;
            portraitGlasses.sprite = doctor ? ArtLibrary.Get("Characters/doctor_glasses") : null;
            foreach (var image in new[] { portraitHead, portraitEyes, portraitMouth, portraitGlasses })
            {
                image.enabled = doctor;
            }

            if (doctor || speaker == Speaker.None)
            {
                if (kidPortrait != null)
                {
                    kidPortrait.gameObject.SetActive(false);
                }

                portraitSpeaker = speaker;
                return;
            }

            var look = speaker == Speaker.Mia ? Core.Avatar.AvatarLook.Mia : YouLook;
            if (kidPortrait == null)
            {
                kidPortrait = Characters.AvatarHead.Portrait(portraitHeadRoot.parent, look, 200f);
                kidPortrait.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 20f);
            }
            else
            {
                kidPortrait.SetLook(look);
            }

            kidPortrait.gameObject.SetActive(true);
            portraitSpeaker = speaker;
        }

        private void SetPortraitMouth(bool talking)
        {
            if (portraitSpeaker == Speaker.Doctor)
            {
                portraitMouth.sprite = ArtLibrary.Get(talking ? "Characters/doctor_mouth_talk" : "Characters/doctor_mouth_smile");
            }
            else if (kidPortrait != null && kidPortrait.gameObject.activeSelf)
            {
                kidPortrait.SetMouth(talking ? Characters.Mouth.Talk : Characters.Mouth.Smile);
            }
        }

        private IEnumerator Reveal()
        {
            label.ForceMeshUpdate();
            var total = label.textInfo.characterCount;
            var shown = 0f;
            while (shown < total)
            {
                shown += Time.deltaTime * RevealCharsPerSecond * GameServices.Settings.RevealSpeed;
                label.maxVisibleCharacters = Mathf.Min(total, Mathf.CeilToInt(shown));
                SetPortraitMouth(Mathf.FloorToInt(shown / 7f) % 2 == 0);
                yield return null;
            }

            label.maxVisibleCharacters = 99999;
            SetPortraitMouth(false);
            reveal = null;
        }

        private IEnumerator SlideIn()
        {
            var end = new Vector2(0f, -22f);
            var start = end + new Vector2(0f, 60f);
            yield return Tween.Run(0.25f, Ease.OutCubic, t =>
            {
                group.alpha = t;
                root.anchoredPosition = Vector2.LerpUnclamped(start, end, t);
            });
        }

        private IEnumerator BounceNext()
        {
            var time = 0f;
            while (true)
            {
                time += Time.deltaTime;
                next.localScale = Vector3.one * (1f + Mathf.Abs(Mathf.Sin(time * 3f)) * 0.12f);
                yield return null;
            }
        }
    }
}
