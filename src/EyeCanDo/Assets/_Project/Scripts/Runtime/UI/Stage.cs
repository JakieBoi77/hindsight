using EyeCanDo.Art;
using UnityEngine;
using UnityEngine.UI;

namespace EyeCanDo.UI
{
    /// <summary>
    /// The layout every screen shares, so the same things are always in the same place on phones
    /// and tablets (NFR_Layout):
    /// <list type="bullet">
    /// <item>Backdrop fills the whole screen (walls, floor, sky), whatever its shape.</item>
    /// <item>World is the 1920x1080 play area, always fully visible. Characters and props live here,
    /// below <see cref="PlayTop"/>, so captions can never cover them.</item>
    /// <item>Overlay follows the safe area (notches): the caption bar across the top, Home and
    /// Restart top-left, Skip top-right, and the ghost hand.</item>
    /// </list>
    /// </summary>
    public sealed class Stage : MonoBehaviour
    {
        /// <summary>World y (from the centre) above which only the caption bar and buttons go.</summary>
        public const float PlayTop = 330f;

        /// <summary>World y of the floor line in rooms.</summary>
        public const float FloorY = -330f;

        public Canvas Canvas { get; private set; }

        public RectTransform Backdrop { get; private set; }

        public RectTransform World { get; private set; }

        public RectTransform Overlay { get; private set; }

        public CaptionBar Captions { get; private set; }

        public GhostHand Ghost { get; private set; }

        public LevelChrome Chrome { get; private set; }

        /// <summary>World units per canvas unit, for offsets given in world positions.</summary>
        public float WorldUnit => World.lossyScale.x;

        public static Stage Create(string name, bool withChrome, int sortingOrder = 0)
        {
            Ui.EnsureEventSystem();
            var canvas = Ui.CreateCanvas(name, sortingOrder);
            var stage = canvas.gameObject.AddComponent<Stage>();
            stage.Canvas = canvas;

            stage.Backdrop = Ui.Rect(canvas.transform, "Backdrop");
            Ui.Stretch(stage.Backdrop);

            stage.World = Ui.Rect(canvas.transform, "World");
            Ui.Place(stage.World, new Vector2(0.5f, 0.5f), Vector2.zero, Ui.ReferenceResolution);

            stage.Overlay = Ui.Rect(canvas.transform, "Overlay");
            Ui.Stretch(stage.Overlay);
            stage.Overlay.gameObject.AddComponent<SafeAreaFitter>();

            stage.Captions = CaptionBar.Create(stage.Overlay);
            if (withChrome)
            {
                stage.Chrome = LevelChrome.Create(stage.Overlay);
            }

            stage.Ghost = GhostHand.Create(stage.Overlay);
            return stage;
        }

        /// <summary>A flat colour filling the backdrop, optionally only below a world y.</summary>
        public Image Fill(Color color, float? belowWorldY = null)
        {
            var image = Ui.Image(Backdrop, "Fill", null, color);
            Ui.Stretch(image.rectTransform);
            if (belowWorldY.HasValue)
            {
                image.rectTransform.anchorMax = new Vector2(1f, 0.5f);
                image.rectTransform.offsetMax = new Vector2(0f, belowWorldY.Value);
            }

            return image;
        }

        /// <summary>An exam room: wall, floor, skirting, window, eye chart, cabinet, plant and lamp.</summary>
        public RectTransform BuildClinicRoom(bool decor = true)
        {
            var theme = UiTheme.Instance;
            Fill(theme.wall);
            var stripe = Fill(UiTheme.Darker(theme.wall, 0.04f), FloorY + 170f);
            stripe.name = "WallStripe";
            var floor = Fill(theme.floor, FloorY);
            floor.name = "Floor";
            var skirting = Ui.Image(Backdrop, "Skirting", null, UiTheme.Darker(theme.floor, 0.15f));
            skirting.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            skirting.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            skirting.rectTransform.offsetMin = new Vector2(0f, FloorY - 4f);
            skirting.rectTransform.offsetMax = new Vector2(0f, FloorY + 14f);

            var decorRoot = Ui.Rect(World, "Decor");
            Ui.Stretch(decorRoot);
            if (decor)
            {
                // Kept clear of where the characters stand and of the eye close-up.
                Prop(decorRoot, "Room/window", new Vector2(190f, 150f), new Vector2(300f, 244f));
                Prop(decorRoot, "Room/eye_chart", new Vector2(-860f, 140f), new Vector2(160f, 222f));
                Prop(decorRoot, "Room/plant", new Vector2(860f, -360f), new Vector2(116f, 180f));
            }

            return decorRoot;
        }

        /// <summary>Outdoors: sky, rolling grass and slowly drifting clouds.</summary>
        public RectTransform BuildOutdoors(float grassY)
        {
            Fill(UiTheme.Hex("#BFE6FF"));
            var far = Fill(UiTheme.Hex("#A8E09A"), grassY + 40f);
            far.name = "GrassFar";
            var near = Fill(UiTheme.Hex("#8CD47E"), grassY);
            near.name = "Grass";

            var decor = Ui.Rect(World, "Decor");
            Ui.Stretch(decor);
            for (var i = 0; i < 4; i++)
            {
                var cloud = Prop(decor, "Map/cloud", new Vector2(-900f + i * 560f, 300f - (i % 2) * 70f), new Vector2(260f, 130f) * (1f - (i % 2) * 0.25f));
                cloud.gameObject.AddComponent<Drift>().Speed = 14f + i * 4f;
            }

            return decor;
        }

        public static Image Prop(Transform parent, string art, Vector2 position, Vector2 size)
        {
            var image = Ui.Image(parent, art, ArtLibrary.Get(art), Color.white);
            image.preserveAspect = true;
            Ui.Place(image.rectTransform, new Vector2(0.5f, 0.5f), position, size);
            return image;
        }

        /// <summary>A layer in the world, added after earlier layers so it draws on top of them.</summary>
        public RectTransform Layer(string name)
        {
            var layer = Ui.Rect(World, name);
            Ui.Stretch(layer);
            return layer;
        }
    }
}
