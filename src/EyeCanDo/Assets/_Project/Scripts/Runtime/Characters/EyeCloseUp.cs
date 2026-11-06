using System.Collections;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.Core.Animation;
using EyeCanDo.UI;
using UnityEngine;
using UnityEngine.UI;

namespace EyeCanDo.Characters
{
    /// <summary>
    /// A big round close-up of one eye, shown beside the characters (never on top of them) for the
    /// moments that are too small to see on the face: the pupil growing, the drop landing, blinking
    /// and the drip that the tissue wipes away. Coordinates inside are the 600x600 art canvas.
    /// </summary>
    public sealed class EyeCloseUp : MonoBehaviour
    {
        private const float Canvas = 600f;
        private const float LidTop = 190f;
        private const float LidClosedHeight = 260f;

        private RectTransform root;
        private RectTransform art;
        private Image skin;
        private Image lid;
        private Image iris;
        private RectTransform pupil;
        private Image outline;
        private Image closedLine;
        private Image drip;
        private float lidAmount;

        public RectTransform Root => root;

        /// <summary>Tap target over the eye, for blinking.</summary>
        public RectTransform EyeArea { get; private set; }

        /// <summary>Where a drip sits on the cheek, for the tissue.</summary>
        public RectTransform DripPoint { get; private set; }

        public bool HasDrip => drip.gameObject.activeSelf;

        public static EyeCloseUp Create(Transform parent, float diameter)
        {
            var theme = UiTheme.Instance;
            var frame = Ui.Disc(parent, "EyeCloseUp", theme.paper);
            var rootRect = (RectTransform)frame.parent;
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(diameter, diameter);
            var eye = rootRect.gameObject.AddComponent<EyeCloseUp>();
            eye.root = rootRect;
            eye.Build(frame, diameter);
            return eye;
        }

        private void Build(RectTransform frame, float diameter)
        {
            // A circular mask keeps the lid and drip inside the frame.
            var maskImage = Ui.Image(frame, "Mask", Shapes.Circle, Color.white);
            Ui.Stretch(maskImage.rectTransform, 6f);
            maskImage.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            skin = maskImage;

            art = Ui.Rect(maskImage.rectTransform, "Art");
            art.anchorMin = art.anchorMax = new Vector2(0.5f, 0.5f);
            art.sizeDelta = Vector2.one * diameter * 1.08f;

            Layer("White", "Eye/closeup_white");
            iris = Layer("Iris", "Eye/closeup_iris");
            pupil = Layer("Pupil", "Eye/closeup_pupil").rectTransform;
            Layer("Shine", "Eye/closeup_shine");

            lid = Ui.Image(art, "Lid", ArtLibrary.Get("Eye/closeup_lid"), Color.white);
            lid.preserveAspect = false;
            lid.rectTransform.anchorMin = new Vector2(0f, 1f);
            lid.rectTransform.anchorMax = new Vector2(1f, 1f);
            lid.rectTransform.pivot = new Vector2(0.5f, 1f);

            outline = Layer("Outline", "Eye/closeup_outline");
            closedLine = Layer("Closed", "Eye/closeup_closed");

            drip = Ui.Image(art, "Drip", ArtLibrary.Get("Props/drop"), Color.white);
            drip.preserveAspect = true;
            drip.rectTransform.anchorMin = drip.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            drip.rectTransform.sizeDelta = new Vector2(70f, 90f) * Unit;
            drip.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            drip.gameObject.SetActive(false);

            EyeArea = Ui.Rect(art, "EyeArea");
            EyeArea.anchorMin = EyeArea.anchorMax = new Vector2(0.5f, 0.5f);
            EyeArea.sizeDelta = new Vector2(480f, 260f) * Unit;

            DripPoint = Ui.Rect(art, "DripPoint");
            DripPoint.anchorMin = DripPoint.anchorMax = new Vector2(0.5f, 0.5f);
            DripPoint.anchoredPosition = new Vector2(-170f, -150f) * Unit;
            DripPoint.sizeDelta = Vector2.one * 80f * Unit;

            SetLid(0f);
            SetPupil(1f);
        }

        private float Unit => art.sizeDelta.x / Canvas;

        private Image Layer(string name, string sprite)
        {
            var image = Ui.Image(art, name, ArtLibrary.Get(sprite), Color.white);
            Ui.Stretch(image.rectTransform);
            return image;
        }

        /// <summary>Colours the close-up for one character: skin and iris.</summary>
        public void SetLook(Core.Avatar.AvatarLook look)
        {
            SetColors(AvatarHead.Hex(look.skin), AvatarHead.Hex(look.eyeColor));
        }

        /// <summary>The demonstration eye Dr. Iris shows before anyone gets drops.</summary>
        public void SetDemo()
        {
            SetColors(UiTheme.Hex("#F2C69B"), UiTheme.Hex("#4A90E2"));
        }

        private void SetColors(Color skinColor, Color irisColor)
        {
            skin.color = skinColor;
            lid.color = skinColor;
            iris.color = irisColor;
        }

        /// <summary>0 = open, 1 = closed.</summary>
        public void SetLid(float amount)
        {
            lidAmount = Mathf.Clamp01(amount);
            lid.rectTransform.anchoredPosition = new Vector2(0f, -LidTop * Unit + 40f * Unit);
            lid.rectTransform.sizeDelta = new Vector2(0f, Mathf.Lerp(40f, LidClosedHeight + 40f, lidAmount) * Unit);
            lid.gameObject.SetActive(lidAmount > 0.02f);
            var closed = lidAmount > 0.85f;
            outline.gameObject.SetActive(!closed);
            closedLine.gameObject.SetActive(closed);
        }

        /// <summary>Pupil size: 1 is normal; the drops make it about twice as big.</summary>
        public void SetPupil(float scale)
        {
            pupil.localScale = Vector3.one * scale;
        }

        public void Show(bool visible)
        {
            root.gameObject.SetActive(visible);
        }

        public IEnumerator Appear()
        {
            root.gameObject.SetActive(true);
            Sfx.Play(Sound.Pop);
            yield return Tween.Pop(root, 0.4f);
        }

        public IEnumerator Disappear()
        {
            yield return Tween.Scale(root, Vector3.zero, 0.25f, Ease.InCubic);
            root.gameObject.SetActive(false);
            root.localScale = Vector3.one;
        }

        public IEnumerator Dilate(float seconds = 1.6f)
        {
            Sfx.Play(Sound.Sparkle);
            yield return Tween.Run(seconds, Ease.InOutSine, t => SetPupil(Mathf.Lerp(1f, 1.9f, t)));
        }

        public IEnumerator Blink(int times = 1)
        {
            for (var i = 0; i < times; i++)
            {
                Sfx.Play(Sound.Blink, Random.Range(0.95f, 1.1f));
                yield return Tween.Run(0.1f, Ease.Linear, SetLid);
                yield return Tween.Wait(0.06f);
                yield return Tween.Run(0.14f, Ease.Linear, t => SetLid(1f - t));
                yield return Tween.Wait(0.08f);
            }
        }

        /// <summary>A drop falls from the top of the frame into the eye and splashes.</summary>
        public IEnumerator DropIn()
        {
            var drop = Ui.Image(art, "FallingDrop", ArtLibrary.Get("Props/drop"), Color.white);
            drop.preserveAspect = true;
            var rect = drop.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(90f, 120f) * Unit;
            var from = new Vector2(20f, 330f) * Unit;
            var to = new Vector2(20f, 10f) * Unit;
            Sfx.Play(Sound.DropFall);
            yield return Tween.Run(0.55f, Ease.InCubic, t => rect.anchoredPosition = Vector2.Lerp(from, to, t));
            Destroy(drop.gameObject);
            Sfx.Play(Sound.Catch);
            var splash = Ui.Image(art, "Splash", ArtLibrary.Get("Props/splash"), Color.white);
            splash.preserveAspect = true;
            splash.rectTransform.anchorMin = splash.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            splash.rectTransform.anchoredPosition = new Vector2(20f, 40f) * Unit;
            splash.rectTransform.sizeDelta = new Vector2(260f, 156f) * Unit;
            yield return Tween.Run(0.45f, Ease.OutCubic, t =>
            {
                splash.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.2f, t);
                splash.color = new Color(1f, 1f, 1f, 1f - t);
            });
            Destroy(splash.gameObject);
        }

        /// <summary>A drip runs from the corner of the eye down onto the cheek.</summary>
        public IEnumerator ShowDrip()
        {
            drip.gameObject.SetActive(true);
            drip.color = Color.white;
            var from = new Vector2(-200f, -40f) * Unit;
            var to = DripPoint.anchoredPosition;
            yield return Tween.Run(0.7f, Ease.OutCubic, t => drip.rectTransform.anchoredPosition = Vector2.Lerp(from, to, t));
        }

        public IEnumerator WipeDrip()
        {
            Sfx.Play(Sound.Wipe);
            yield return Tween.Run(0.3f, Ease.Linear, t => drip.color = new Color(1f, 1f, 1f, 1f - t));
            drip.gameObject.SetActive(false);
        }

        public void ResetEye()
        {
            SetLid(0f);
            SetPupil(1f);
            drip.gameObject.SetActive(false);
        }
    }
}
