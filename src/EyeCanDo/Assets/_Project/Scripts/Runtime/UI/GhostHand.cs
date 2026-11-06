using System.Collections;
using EyeCanDo.Art;
using UnityEngine;
using UnityEngine.UI;

namespace EyeCanDo.UI
{
    /// <summary>
    /// An animated hand that shows the child what to do: tap, double tap, hold or drag. This is
    /// the main way instructions reach children who cannot read (FR_SpokenInstructions intent,
    /// FR_HintOnMiss). It never blocks touches.
    /// </summary>
    public sealed class GhostHand : MonoBehaviour
    {
        private RectTransform hand;
        private RectTransform ripple;
        private Image rippleImage;
        private CanvasGroup group;
        private Coroutine running;

        public bool IsShowing => running != null;

        public static GhostHand Create(Transform overlay)
        {
            var theme = UiTheme.Instance;
            var host = Ui.Rect(overlay, "GhostHand");
            Ui.Stretch(host);
            var ghost = host.gameObject.AddComponent<GhostHand>();
            ghost.group = host.gameObject.AddComponent<CanvasGroup>();
            ghost.group.blocksRaycasts = false;
            ghost.group.interactable = false;

            ghost.ripple = Ui.Rect(host, "Ripple");
            ghost.ripple.sizeDelta = new Vector2(170f, 170f);
            ghost.rippleImage = Ui.Image(ghost.ripple, "Ring", Shapes.Ring, theme.paper);
            Ui.Stretch(ghost.rippleImage.rectTransform);

            // A pointing hand; its pivot is the fingertip, so the fingertip lands on the target.
            ghost.hand = Ui.Rect(host, "Hand");
            ghost.hand.sizeDelta = new Vector2(150f, 150f);
            ghost.hand.pivot = new Vector2(120f / 256f, 1f - 26f / 256f);
            var shadow = Ui.Image(ghost.hand, "Shadow", ArtLibrary.Get("Icons/hand"), new Color(0f, 0f, 0f, 0.22f));
            Ui.Stretch(shadow.rectTransform);
            shadow.rectTransform.anchoredPosition = new Vector2(8f, -10f);
            var art = Ui.Image(ghost.hand, "Art", ArtLibrary.Get("Icons/hand"), Color.white);
            Ui.Stretch(art.rectTransform);
            host.gameObject.SetActive(false);
            return ghost;
        }

        public void ShowTap(RectTransform target)
        {
            Run(TapLoop(target, 1));
        }

        public void ShowDoubleTap(RectTransform target)
        {
            Run(TapLoop(target, 2));
        }

        public void ShowHold(RectTransform target, float seconds = 1.5f)
        {
            Run(HoldLoop(target, seconds));
        }

        public void ShowDrag(RectTransform from, Vector3 worldTo)
        {
            Run(DragLoop(from, worldTo));
        }

        public void Stop()
        {
            if (running != null)
            {
                StopCoroutine(running);
                running = null;
            }

            gameObject.SetActive(false);
        }

        private void Run(IEnumerator loop)
        {
            Stop();
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            running = StartCoroutine(loop);
        }

        private IEnumerator TapLoop(RectTransform target, int taps)
        {
            while (true)
            {
                hand.position = target.position + new Vector3(60f, -60f, 0f) * Scale;
                ripple.position = target.position;
                yield return Tween.Run(0.35f, t => t, t => { group.alpha = t; hand.position = Vector3.Lerp(target.position + new Vector3(60f, -60f, 0f) * Scale, target.position, t); });
                for (var i = 0; i < taps; i++)
                {
                    yield return Press(0.12f);
                    yield return Tween.Wait(0.12f);
                }

                yield return Tween.Wait(0.5f);
                yield return Tween.Run(0.25f, t => t, t => group.alpha = 1f - t);
                yield return Tween.Wait(0.6f);
            }
        }

        private IEnumerator HoldLoop(RectTransform target, float seconds)
        {
            while (true)
            {
                hand.position = target.position;
                ripple.position = target.position;
                yield return Tween.Run(0.3f, t => t, t => group.alpha = t);
                hand.localScale = Vector3.one * 0.9f;
                yield return Tween.Run(seconds, t => t, t =>
                {
                    ripple.localScale = Vector3.one * (0.6f + 0.4f * Mathf.PingPong(t * 3f, 1f));
                    rippleImage.color = RippleColor(0.9f);
                });
                hand.localScale = Vector3.one;
                rippleImage.color = Color.clear;
                yield return Tween.Run(0.25f, t => t, t => group.alpha = 1f - t);
                yield return Tween.Wait(0.6f);
            }
        }

        private IEnumerator DragLoop(RectTransform from, Vector3 worldTo)
        {
            while (true)
            {
                var start = from.position;
                hand.position = start;
                ripple.position = start;
                rippleImage.color = Color.clear;
                yield return Tween.Run(0.3f, t => t, t => group.alpha = t);
                hand.localScale = Vector3.one * 0.9f;
                yield return Tween.Run(1.1f, Core.Animation.Ease.InOutSine, t => hand.position = Vector3.Lerp(start, worldTo, t));
                hand.localScale = Vector3.one;
                yield return Tween.Wait(0.3f);
                yield return Tween.Run(0.25f, t => t, t => group.alpha = 1f - t);
                yield return Tween.Wait(0.5f);
            }
        }

        private IEnumerator Press(float duration)
        {
            hand.localScale = Vector3.one * 0.88f;
            rippleImage.color = RippleColor(0.9f);
            yield return Tween.Run(duration * 2f, t => t, t =>
            {
                ripple.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.1f, t);
                rippleImage.color = RippleColor(0.9f * (1f - t));
            });
            hand.localScale = Vector3.one;
        }

        private static Color RippleColor(float alpha)
        {
            var accent = UiTheme.Instance.accent;
            return new Color(accent.r, accent.g, accent.b, alpha);
        }

        private float Scale
        {
            get
            {
                var canvas = GetComponentInParent<Canvas>();
                return canvas != null ? canvas.rootCanvas.transform.lossyScale.x : 1f;
            }
        }
    }
}
