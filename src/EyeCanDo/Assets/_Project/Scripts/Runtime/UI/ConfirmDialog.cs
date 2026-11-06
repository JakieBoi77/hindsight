using System;
using System.Collections;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.Core.Animation;
using TMPro;
using UnityEngine;

namespace EyeCanDo.UI
{
    /// <summary>
    /// A question that must be answered before anything is deleted: a picture, a title, a plain
    /// message saying exactly what will be lost, and two buttons. The safe choice is on the left
    /// and nothing happens unless the red button is pressed. It sits above every screen and
    /// blocks taps on what is behind it.
    /// </summary>
    public sealed class ConfirmDialog : MonoBehaviour
    {
        public const int SortingOrder = 700;

        private RectTransform panel;
        private Action confirmed;
        private Action cancelled;
        private bool answered;

        /// <summary>The dialog on screen, if any (also the PlayMode test hook).</summary>
        public static ConfirmDialog Current { get; private set; }

        public string Title { get; private set; }

        public string Message { get; private set; }

        public ChunkyButton ConfirmButton { get; private set; }

        public ChunkyButton CancelButton { get; private set; }

        public static ConfirmDialog Show(string title, string message, string icon, string confirmLabel, string cancelLabel, Action confirmed, Action cancelled = null)
        {
            if (Current != null)
            {
                Destroy(Current.gameObject);
            }

            var theme = UiTheme.Instance;
            Ui.EnsureEventSystem();
            var canvas = Ui.CreateCanvas("ConfirmDialog", SortingOrder);
            var dialog = canvas.gameObject.AddComponent<ConfirmDialog>();
            dialog.Title = title;
            dialog.Message = message;
            dialog.confirmed = confirmed;
            dialog.cancelled = cancelled;

            var dim = Ui.Image(canvas.transform, "Dim", null, new Color(theme.ink.r, theme.ink.g, theme.ink.b, 0.55f), true);
            Ui.Stretch(dim.rectTransform);

            var face = Ui.Panel(canvas.transform, "Panel", theme.paper, 48f);
            dialog.panel = (RectTransform)face.parent;
            Ui.Place(dialog.panel, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1180f, 680f));

            var picture = Ui.Art(face, "Picture", icon, new Vector2(140f, 140f));
            Ui.Place(picture.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -96f), new Vector2(140f, 140f));

            var heading = Ui.Text(face, title, 64f, theme.ink);
            Ui.Place(heading.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -226f), new Vector2(1060f, 90f));

            var body = Ui.Text(face, message, 40f, theme.ink, TextAlignmentOptions.Center, false);
            Ui.Place(body.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -360f), new Vector2(1020f, 170f));
            body.fontSizeMin = 30f;

            var size = new Vector2(470f, 130f);
            dialog.CancelButton = ChunkyButton.Create(face, "Cancel", cancelLabel, null, theme.paper, size, 50f);
            Ui.Place((RectTransform)dialog.CancelButton.transform, new Vector2(0.5f, 0f), new Vector2(-265f, 100f), size);
            dialog.CancelButton.Clicked += dialog.Cancel;

            dialog.ConfirmButton = ChunkyButton.Create(face, "Confirm", confirmLabel, ArtLibrary.Get("Icons/trash"), theme.danger, size, 50f);
            Ui.Place((RectTransform)dialog.ConfirmButton.transform, new Vector2(0.5f, 0f), new Vector2(265f, 100f), size);
            dialog.ConfirmButton.Clicked += dialog.Confirm;
            foreach (var label in new[] { dialog.CancelButton.Label, dialog.ConfirmButton.Label })
            {
                label.enableAutoSizing = true;
                label.fontSizeMin = 30f;
                label.fontSizeMax = theme.buttonSize;
            }

            Current = dialog;
            Sfx.Play(Sound.Whoosh);
            dialog.StartCoroutine(Tween.Pop(dialog.panel, 0.3f));
            return dialog;
        }

        /// <summary>Presses the red button: closes the dialog, then does what was asked.</summary>
        public void Confirm()
        {
            Answer(confirmed);
        }

        /// <summary>Presses the safe button: closes the dialog and changes nothing.</summary>
        public void Cancel()
        {
            Answer(cancelled);
        }

        private void Answer(Action then)
        {
            if (answered)
            {
                return;
            }

            answered = true;
            if (Current == this)
            {
                Current = null;
            }

            StartCoroutine(Close());
            then?.Invoke();
        }

        private IEnumerator Close()
        {
            GetComponent<UnityEngine.UI.GraphicRaycaster>().enabled = false;
            yield return Tween.Scale(panel, Vector3.zero, 0.15f, Ease.InCubic);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }
    }
}
