using EyeCanDo.Art;
using EyeCanDo.Core.Avatar;
using EyeCanDo.UI;
using UnityEngine;
using UnityEngine.UI;

namespace EyeCanDo.Characters
{
    /// <summary>
    /// A kid's head built from avatar parts (Assets/_Project/Art/Avatar). Skin, hair and iris
    /// parts are recoloured from the <see cref="AvatarLook"/>. Used by the character rig, the
    /// caption bar portraits and the dress-up tiles, so a kid looks the same everywhere.
    /// </summary>
    public sealed class AvatarHead : MonoBehaviour
    {
        /// <summary>Head canvas size and the point (pixels from top-left) that joins the neck.</summary>
        public static readonly Vector2 Canvas = new Vector2(340f, 360f);
        public static readonly Vector2 NeckPoint = new Vector2(170f, 316f);
        public static readonly Vector2[] EyeCentres = { new Vector2(140f, 230f), new Vector2(200f, 230f) };

        private const string Folder = "Avatar/";

        // Head tipped back (seen from the front): the hair over the forehead squashes up toward the
        // top of the head, the hat lifts with it, and glasses fade away, so the eyes stay in view.
        private const float HairPivotY = 120f;
        private const float HairSquash = 0.42f;
        private const float HatLift = 54f;

        private static Material recolor;

        private Image hairBack;
        private Image ears;
        private Image head;
        private Image nose;
        private Image mouth;
        private Image eyesWhite;
        private Image eyesIris;
        private Image eyesPupil;
        private Image hairFront;
        private Image brows;
        private Image glasses;
        private Image hat;

        private float tilt;
        private Eyes eyes = Eyes.Open;
        private Mouth mouthState = Mouth.Smile;

        public AvatarLook Look { get; private set; }

        /// <summary>The features that move together when the head tips back.</summary>
        public RectTransform Face { get; private set; }

        public RectTransform[] EyePoints { get; } = new RectTransform[2];

        /// <summary>The shared recolour material: white becomes the Image colour, black the ink.</summary>
        public static Material Recolor
        {
            get
            {
                if (recolor == null)
                {
                    recolor = Resources.Load<Material>("Recolor");
                }

                return recolor;
            }
        }

        /// <summary>Builds a head filling <paramref name="rect"/> (which should have the head canvas aspect).</summary>
        public static AvatarHead Build(RectTransform rect, AvatarLook look)
        {
            var avatar = rect.gameObject.AddComponent<AvatarHead>();
            avatar.hairBack = avatar.Layer(rect, "HairBack", true);
            avatar.ears = avatar.Layer(rect, "Ears", true);
            avatar.head = avatar.Layer(rect, "Head", true);
            // The fringe belongs to the head, not the face, so it stays on when the face lifts.
            avatar.hairFront = avatar.Layer(rect, "HairFront", true);
            avatar.hairFront.rectTransform.pivot = new Vector2(0.5f, 1f - HairPivotY / Canvas.y);
            avatar.Face = Ui.Rect(rect, "Face");
            Ui.Stretch(avatar.Face);
            var cheeks = avatar.Layer(avatar.Face, "Cheeks", false);
            cheeks.sprite = ArtLibrary.Get(Folder + "cheeks");
            avatar.nose = avatar.Layer(avatar.Face, "Nose", false);
            avatar.mouth = avatar.Layer(avatar.Face, "Mouth", false);
            avatar.eyesWhite = avatar.Layer(avatar.Face, "EyesWhite", false);
            avatar.eyesIris = avatar.Layer(avatar.Face, "EyesIris", false);
            avatar.eyesPupil = avatar.Layer(avatar.Face, "EyesPupil", false);
            avatar.brows = avatar.Layer(avatar.Face, "Brows", false);
            avatar.glasses = avatar.Layer(avatar.Face, "Glasses", false);
            avatar.hat = avatar.Layer(rect, "Hat", false);

            for (var i = 0; i < 2; i++)
            {
                var point = Ui.Rect(avatar.Face, "EyePoint" + i);
                point.anchorMin = point.anchorMax = new Vector2(EyeCentres[i].x / Canvas.x, 1f - EyeCentres[i].y / Canvas.y);
                point.sizeDelta = new Vector2(40f, 40f);
                avatar.EyePoints[i] = point;
            }

            avatar.SetLook(look);
            return avatar;
        }

        /// <summary>A head on its own, sized to fit a box, for portraits and tiles.</summary>
        public static AvatarHead Portrait(Transform parent, AvatarLook look, float height)
        {
            var rect = Ui.Rect(parent, "AvatarHead");
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(Canvas.x / Canvas.y * height, height);
            return Build(rect, look);
        }

        private Image Layer(Transform parent, string name, bool recolored)
        {
            var image = Ui.Image(parent, name, null, Color.white);
            image.preserveAspect = false;
            Ui.Stretch(image.rectTransform);
            if (recolored)
            {
                image.material = Recolor;
            }

            return image;
        }

        public void SetLook(AvatarLook look)
        {
            Look = look.Copy();
            var skin = Hex(look.skin);
            var hair = Hex(look.hairColor);
            Set(hairBack, Optional($"hair_{look.hairStyle}_back"), hair);
            Set(ears, Folder + "ears_" + look.ears, skin);
            Set(head, Folder + "head", skin);
            Set(hairFront, look.hairStyle == AvatarCatalog.None ? null : Folder + $"hair_{look.hairStyle}_front", hair);
            Set(nose, Folder + "nose_" + look.nose, Color.white);
            Set(glasses, look.glasses == AvatarCatalog.None ? null : Folder + "glasses_" + look.glasses, Color.white);
            Set(hat, look.hat == AvatarCatalog.None ? null : Folder + "hat_" + look.hat, Color.white);
            eyesIris.color = Hex(look.eyeColor);
            SetEyes(eyes);
            SetMouth(mouthState);
            Tilt(tilt);
        }

        /// <summary>Tips the hair, hat and glasses back with the head (0 upright, 1 fully back).</summary>
        public void Tilt(float amount)
        {
            var t = tilt = Mathf.Clamp01(amount);
            var pixel = ((RectTransform)transform).rect.height / Canvas.y;
            hairFront.rectTransform.localScale = new Vector3(1f, 1f - HairSquash * t, 1f);
            hat.rectTransform.anchoredPosition = new Vector2(0f, HatLift * pixel * t);
            var color = glasses.color;
            color.a = 1f - Mathf.SmoothStep(0f, 1f, t / 0.35f);
            glasses.color = color;
        }

        public void SetEyes(Eyes value)
        {
            eyes = value;
            var style = Look.eyeStyle;
            switch (value)
            {
                case Eyes.Closed:
                case Eyes.Happy:
                case Eyes.Squeeze:
                    Set(eyesWhite, null, Color.white);
                    Set(eyesIris, null, eyesIris.color);
                    Set(eyesPupil, Folder + "eyes_" + value.ToString().ToLowerInvariant(), Color.white);
                    break;
                default:
                    var state = value == Eyes.Up ? "up" : "open";
                    var hasWhites = style != "dots";
                    Set(eyesWhite, hasWhites ? Folder + $"eyes_{style}_{state}_white" : null, Color.white);
                    Set(eyesIris, hasWhites ? Folder + $"eyes_{style}_{state}_iris" : null, eyesIris.color);
                    Set(eyesPupil, Folder + $"eyes_{style}_{state}_pupil", Color.white);
                    break;
            }

            var browState = value == Eyes.Up || value == Eyes.Happy ? "up" : value == Eyes.Worried || value == Eyes.Squeeze ? "worried" : "calm";
            Set(brows, Look.brows == AvatarCatalog.None ? null : Folder + $"brows_{Look.brows}_{browState}", Color.white);
        }

        public void SetMouth(Mouth value)
        {
            mouthState = value;
            var name = value == Mouth.Smile ? "mouth_" + Look.mouth : "mouth_" + value.ToString().ToLowerInvariant();
            Set(mouth, Folder + name, Color.white);
        }

        private string Optional(string name)
        {
            return ArtLibrary.Instance.Has(Folder + name) ? Folder + name : null;
        }

        private static void Set(Image image, string sprite, Color color)
        {
            image.sprite = sprite != null ? ArtLibrary.Get(sprite) : null;
            image.enabled = image.sprite != null;
            image.color = color;
        }

        public static Color Hex(string hex)
        {
            return UiTheme.Hex(hex);
        }
    }
}
