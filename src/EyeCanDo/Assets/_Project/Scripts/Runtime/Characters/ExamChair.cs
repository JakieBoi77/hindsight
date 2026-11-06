using System.Collections;
using EyeCanDo.Art;
using EyeCanDo.UI;
using UnityEngine;
using UnityEngine.UI;

namespace EyeCanDo.Characters
{
    /// <summary>
    /// The exam chair. A seated kid is parented to its seat so they move together. The pivot is
    /// at the bottom of the base, on the floor.
    /// </summary>
    public sealed class ExamChair : MonoBehaviour
    {
        private const float CanvasWidth = 600f;
        private const float CanvasHeight = 700f;

        /// <summary>Height of the seated kid's feet above the chair's base, in chair canvas pixels.</summary>
        private const float SeatedFeet = 110f;

        public RectTransform Root { get; private set; }

        public RectTransform Seat { get; private set; }

        public Image Art { get; private set; }

        public float Scale { get; private set; }

        public CharacterRig Occupant { get; private set; }

        public static ExamChair Create(Transform parent, Vector2 floorPosition, float scale)
        {
            var rect = Ui.Rect(parent, "ExamChair");
            Ui.Place(rect, new Vector2(0.5f, 0.5f), floorPosition, new Vector2(CanvasWidth, CanvasHeight) * scale, new Vector2(0.5f, 0f));
            var chair = rect.gameObject.AddComponent<ExamChair>();
            chair.Root = rect;
            chair.Scale = scale;
            chair.Art = Ui.Image(rect, "Art", ArtLibrary.Get("Props/exam_chair"), Color.white);
            chair.Art.preserveAspect = false;
            Ui.Stretch(chair.Art.rectTransform);
            chair.Seat = Ui.Rect(rect, "Seat");
            Ui.Place(chair.Seat, new Vector2(0.5f, 0f), new Vector2(0f, SeatedFeet * scale), Vector2.zero);
            return chair;
        }

        /// <summary>Anchored position (in the chair's parent) where a seated kid's feet go.</summary>
        public Vector2 SeatPosition => Root.anchoredPosition + new Vector2(0f, SeatedFeet * Scale);

        /// <summary>Puts a kid in the chair, switching them to the seated pose.</summary>
        public void SeatKid(CharacterRig kid)
        {
            Occupant = kid;
            kid.SetPose("seated");
            kid.Root.SetParent(Seat, false);
            kid.Root.anchorMin = kid.Root.anchorMax = new Vector2(0.5f, 0.5f);
            kid.Root.anchoredPosition = Vector2.zero;
        }

        /// <summary>Takes the kid out of the chair and back onto a parent layer.</summary>
        public void Release(Transform layer, Vector2 position)
        {
            if (Occupant == null)
            {
                return;
            }

            Occupant.Root.SetParent(layer, false);
            Occupant.Root.anchoredPosition = position;
            Occupant.SetTilt(0f);
            Occupant.SetPose("stand");
            Occupant = null;
        }

        /// <summary>The kid hops from where they stand up into the seat.</summary>
        public IEnumerator HopIn(CharacterRig kid)
        {
            yield return kid.HopTo(SeatPosition, 180f, 0.55f);
            SeatKid(kid);
            yield return Tween.Bump(Root, 0.04f, 0.25f);
        }
    }
}
