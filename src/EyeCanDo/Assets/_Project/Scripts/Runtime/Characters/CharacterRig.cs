using System.Collections;
using System.Collections.Generic;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.Core.Animation;
using EyeCanDo.Core.Avatar;
using EyeCanDo.UI;
using UnityEngine;
using UnityEngine.UI;

namespace EyeCanDo.Characters
{
    public enum Eyes
    {
        Open,
        Up,
        Worried,
        Closed,
        Happy,
        Squeeze,
    }

    public enum Mouth
    {
        Smile,
        Talk,
        Grin,
        Oh,
        Wobble,
    }

    /// <summary>Part sizes and anchor points, matching Tools/ArtGen (pixels from the top-left of each canvas).</summary>
    public static class Looks
    {
        public static readonly Vector2 DoctorHead = new Vector2(300f, 300f);
        public static readonly Vector2 DoctorHeadAnchor = new Vector2(150f, 262f);
        public static readonly Vector2 DoctorBody = new Vector2(600f, 840f);
        public static readonly Vector2 DoctorNeck = new Vector2(300f, 200f);
        public static readonly Vector2[] DoctorEyes = { new Vector2(112f, 164f), new Vector2(188f, 164f) };

        public static Vector2 KidBody(string pose)
        {
            return pose == "seated" ? new Vector2(340f, 430f) : new Vector2(340f, 620f);
        }

        public static Vector2 KidNeck(string pose)
        {
            return pose == "seated" ? new Vector2(170f, 36f) : new Vector2(170f, 96f);
        }

        /// <summary>The kid's hand on the screen-right side, which holds items.</summary>
        public static Vector2 KidHand(string pose)
        {
            switch (pose)
            {
                case "seated": return new Vector2(270f, 226f);
                case "wave": return new Vector2(300f, 40f);
                default: return new Vector2(270f, 322f);
            }
        }

        public static Vector2 KidChest(string pose)
        {
            return pose == "seated" ? new Vector2(170f, 160f) : new Vector2(170f, 220f);
        }

        /// <summary>Doctor hand centres per pose (0 = the hand on screen left).</summary>
        public static Vector2 DoctorHand(string pose, int hand)
        {
            switch (pose)
            {
                case "wave": return hand == 0 ? new Vector2(196f, 460f) : new Vector2(486f, 80f);
                case "point": return hand == 0 ? new Vector2(196f, 460f) : new Vector2(548f, 180f);
                case "cheer": return hand == 0 ? new Vector2(112f, 80f) : new Vector2(488f, 80f);
                case "hold": return hand == 0 ? new Vector2(52f, 140f) : new Vector2(404f, 460f);
                default: return hand == 0 ? new Vector2(196f, 460f) : new Vector2(404f, 460f);
            }
        }
    }

    /// <summary>
    /// A character assembled from SVG parts. Dr. Iris uses drawn poses; kids are built from avatar
    /// parts and an <see cref="AvatarLook"/>, so the player's own character appears everywhere.
    /// The root's pivot is at the feet. The rig blinks and breathes on its own, talks while its
    /// lines are shown, and can tip its head back, shiver, hop, walk and wave. All motion uses
    /// scaled time, so pausing the game pauses it.
    /// </summary>
    public sealed class CharacterRig : MonoBehaviour
    {
        private const float TiltFaceShift = 46f;
        private static readonly string[] KidParts = { "skin", "shorts", "shoes", "shirt", "hands" };

        private RectTransform root;
        private RectTransform bodyRoot;
        private readonly List<Image> bodyLayers = new List<Image>();
        private Image doctorBody;
        private Image doctorEyes;
        private Image doctorMouth;
        private Image emblem;
        private Image held;
        private RectTransform headPivot;
        private RectTransform face;
        private AvatarHead avatarHead;
        private readonly RectTransform[] eyePoints = new RectTransform[2];

        private Eyes eyes = Eyes.Open;
        private Mouth mouth = Mouth.Smile;
        private float blinkTimer;
        private bool blinking;
        private bool shivering;
        private float talkTime;
        private float breath;
        private float tilt;

        public bool IsDoctor { get; private set; }

        public AvatarLook Look => avatarHead != null ? avatarHead.Look : null;

        public string Pose { get; private set; }

        public float Scale { get; private set; }

        public RectTransform Root => root;

        public RectTransform Head => headPivot;

        public float Tilt => tilt;

        public bool IsTalking => talkTime > 0f;

        public static CharacterRig CreateDoctor(Transform parent, string pose, float scale)
        {
            var rig = Create(parent, "Character_doctor", scale);
            rig.IsDoctor = true;
            rig.BuildDoctor(pose);
            return rig;
        }

        public static CharacterRig CreateKid(Transform parent, AvatarLook look, string pose, float scale)
        {
            var rig = Create(parent, "Character_kid", scale);
            rig.BuildKid(look, pose);
            return rig;
        }

        private static CharacterRig Create(Transform parent, string name, float scale)
        {
            var rect = Ui.Rect(parent, name);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0f);
            var rig = rect.gameObject.AddComponent<CharacterRig>();
            rig.root = rect;
            rig.Scale = scale;
            rig.blinkTimer = Random.Range(1.5f, 4f);
            return rig;
        }

        private void BuildDoctor(string pose)
        {
            bodyRoot = Ui.Rect(root, "Body");
            Ui.Stretch(bodyRoot);
            doctorBody = BodyLayer("Coat", false);
            headPivot = HeadPivot(Looks.DoctorHead, Looks.DoctorHeadAnchor);
            var head = Ui.Image(headPivot, "HeadArt", ArtLibrary.Get("Characters/doctor_head"), Color.white);
            Ui.Stretch(head.rectTransform);
            face = Ui.Rect(headPivot, "Face");
            Ui.Stretch(face);
            doctorEyes = Ui.Image(face, "Eyes", null, Color.white);
            Ui.Stretch(doctorEyes.rectTransform);
            doctorMouth = Ui.Image(face, "Mouth", null, Color.white);
            Ui.Stretch(doctorMouth.rectTransform);
            var glasses = Ui.Image(face, "Glasses", ArtLibrary.Get("Characters/doctor_glasses"), Color.white);
            Ui.Stretch(glasses.rectTransform);
            for (var i = 0; i < 2; i++)
            {
                var point = Ui.Rect(face, "EyePoint" + i);
                var p = Looks.DoctorEyes[i];
                point.anchorMin = point.anchorMax = new Vector2(p.x / Looks.DoctorHead.x, 1f - p.y / Looks.DoctorHead.y);
                eyePoints[i] = point;
            }

            SetPose(pose);
            SetEyes(Eyes.Open);
            SetMouth(Mouth.Smile);
        }

        private void BuildKid(AvatarLook look, string pose)
        {
            bodyRoot = Ui.Rect(root, "Body");
            Ui.Stretch(bodyRoot);
            for (var i = 0; i < 4; i++)
            {
                bodyLayers.Add(BodyLayer(KidParts[i], true));
            }

            emblem = Ui.Image(bodyRoot, "Emblem", null, Color.white);
            emblem.preserveAspect = true;
            emblem.rectTransform.anchorMin = emblem.rectTransform.anchorMax = new Vector2(0f, 1f);
            held = Ui.Image(bodyRoot, "Held", null, Color.white);
            held.preserveAspect = true;
            held.rectTransform.anchorMin = held.rectTransform.anchorMax = new Vector2(0f, 1f);
            // The hand grips the item at (100, 250) on its 200x300 canvas.
            held.rectTransform.pivot = new Vector2(0.5f, 50f / 300f);
            bodyLayers.Add(BodyLayer(KidParts[4], true));

            headPivot = HeadPivot(AvatarHead.Canvas, AvatarHead.NeckPoint);
            avatarHead = AvatarHead.Build(headPivot, look);
            face = avatarHead.Face;
            eyePoints[0] = avatarHead.EyePoints[0];
            eyePoints[1] = avatarHead.EyePoints[1];
            SetPose(pose);
            SetLook(look);
        }

        private Image BodyLayer(string name, bool recolored)
        {
            var image = Ui.Image(bodyRoot, name, null, Color.white);
            image.preserveAspect = false;
            Ui.Stretch(image.rectTransform);
            if (recolored)
            {
                image.material = AvatarHead.Recolor;
            }

            return image;
        }

        private RectTransform HeadPivot(Vector2 canvas, Vector2 anchor)
        {
            var pivot = Ui.Rect(root, "Head");
            pivot.anchorMin = pivot.anchorMax = new Vector2(0f, 1f);
            pivot.pivot = new Vector2(anchor.x / canvas.x, 1f - anchor.y / canvas.y);
            pivot.sizeDelta = canvas * Scale;
            return pivot;
        }

        /// <summary>Changes a kid's whole look: skin, face, hair, clothes and items.</summary>
        public void SetLook(AvatarLook look)
        {
            if (IsDoctor || look == null)
            {
                return;
            }

            avatarHead.SetLook(look);
            var skin = AvatarHead.Hex(look.skin);
            bodyLayers[0].color = skin;
            bodyLayers[1].color = AvatarHead.Hex(look.shortsColor);
            bodyLayers[2].color = AvatarHead.Hex(look.shoesColor);
            bodyLayers[3].color = AvatarHead.Hex(look.shirtColor);
            bodyLayers[4].color = skin;
            emblem.sprite = look.emblem == AvatarCatalog.None ? null : ArtLibrary.Get("Avatar/emblem_" + look.emblem);
            emblem.enabled = emblem.sprite != null;
            held.sprite = look.held == AvatarCatalog.None ? null : ArtLibrary.Get("Avatar/held_" + look.held);
            held.enabled = held.sprite != null;
            SetEyes(eyes);
            SetMouth(mouth);
        }

        /// <summary>Swaps the body (kids: stand, wave, seated; doctor: idle, wave, point, cheer, hold).</summary>
        public void SetPose(string pose)
        {
            Pose = pose;
            Vector2 size;
            Vector2 neck;
            if (IsDoctor)
            {
                doctorBody.sprite = ArtLibrary.Get($"Characters/doctor_{pose}");
                size = Looks.DoctorBody;
                neck = Looks.DoctorNeck;
            }
            else
            {
                for (var i = 0; i < KidParts.Length; i++)
                {
                    bodyLayers[i].sprite = ArtLibrary.Get($"Avatar/{KidParts[i]}_{pose}");
                }

                size = Looks.KidBody(pose);
                neck = Looks.KidNeck(pose);
                var chest = Looks.KidChest(pose);
                emblem.rectTransform.anchoredPosition = new Vector2(chest.x, -chest.y) * Scale;
                emblem.rectTransform.sizeDelta = Vector2.one * 92f * Scale;
                var hand = Looks.KidHand(pose);
                held.rectTransform.anchoredPosition = new Vector2(hand.x, -hand.y) * Scale;
                held.rectTransform.sizeDelta = new Vector2(200f, 300f) * Scale * 0.8f;
            }

            root.sizeDelta = size * Scale;
            headPivot.anchoredPosition = new Vector2(neck.x, -neck.y) * Scale;
        }

        public void SetEyes(Eyes value)
        {
            eyes = value;
            if (!blinking)
            {
                ShowEyes(value);
            }
        }

        public void SetMouth(Mouth value)
        {
            mouth = value;
            if (talkTime <= 0f)
            {
                ShowMouth(value);
            }
        }

        /// <summary>Moves the mouth for a while, as if saying a line.</summary>
        public void Talk(float seconds)
        {
            talkTime = Mathf.Max(talkTime, seconds);
        }

        public void StopTalking()
        {
            talkTime = 0f;
            ShowMouth(mouth);
        }

        /// <summary>
        /// Tips the head back: 0 is upright, 1 is head back with the chin up and eyes looking up.
        /// Seen from the front this lifts and squashes the face.
        /// </summary>
        public void SetTilt(float amount)
        {
            tilt = Mathf.Clamp01(amount);
            headPivot.localScale = new Vector3(1f, 1f - 0.16f * tilt, 1f);
            face.anchoredPosition = new Vector2(0f, TiltFaceShift * Scale * tilt);
            if (avatarHead != null)
            {
                avatarHead.Tilt(tilt);
            }
        }

        /// <summary>World position of an eye (0 = the eye on screen left).</summary>
        public Vector3 EyeWorld(int index)
        {
            return eyePoints[Mathf.Clamp(index, 0, 1)].position;
        }

        public Vector3 EyesCentreWorld => (EyeWorld(0) + EyeWorld(1)) * 0.5f;

        public RectTransform EyePoint(int index)
        {
            return eyePoints[Mathf.Clamp(index, 0, 1)];
        }

        /// <summary>World position of a point given in body canvas pixels (from top-left).</summary>
        public Vector3 BodyPointWorld(Vector2 canvasPixels)
        {
            var size = IsDoctor ? Looks.DoctorBody : Looks.KidBody(Pose);
            var local = new Vector3((canvasPixels.x - size.x / 2f) * Scale, (size.y - canvasPixels.y) * Scale, 0f);
            return root.TransformPoint(local);
        }

        public Vector3 HandWorld(int hand)
        {
            return BodyPointWorld(IsDoctor ? Looks.DoctorHand(Pose, hand) : Looks.KidHand(Pose));
        }

        public IEnumerator Blink(int times = 1)
        {
            for (var i = 0; i < times; i++)
            {
                blinking = true;
                ShowEyes(Eyes.Closed);
                Sfx.Play(Sound.Blink, Random.Range(0.95f, 1.1f));
                yield return Tween.Wait(0.14f);
                blinking = false;
                ShowEyes(eyes);
                yield return Tween.Wait(0.16f);
            }
        }

        /// <summary>A quick cold shiver: squeeze the eyes, wobble the mouth and shake.</summary>
        public IEnumerator Shiver()
        {
            if (shivering)
            {
                yield break;
            }

            shivering = true;
            var previousEyes = eyes;
            var previousMouth = mouth;
            SetEyes(Eyes.Squeeze);
            SetMouth(Mouth.Wobble);
            Sfx.Play(Sound.Shiver);
            var start = root.anchoredPosition;
            yield return Tween.Run(0.6f, Ease.Linear, t => root.anchoredPosition = start + new Vector2(Mathf.Sin(t * Mathf.PI * 10f) * 7f * (1f - t), 0f));
            root.anchoredPosition = start;
            SetEyes(previousEyes);
            SetMouth(previousMouth);
            shivering = false;
        }

        /// <summary>A happy hop on the spot.</summary>
        public IEnumerator Bounce(float height = 40f, bool sound = true)
        {
            var start = root.anchoredPosition;
            if (sound)
            {
                Sfx.Play(Sound.Hop);
            }

            yield return Tween.Run(0.4f, Ease.Linear, t => root.anchoredPosition = start + Vector2.up * (Mathf.Sin(t * Mathf.PI) * height));
            root.anchoredPosition = start;
        }

        /// <summary>Jumps in an arc to a new anchored position.</summary>
        public IEnumerator HopTo(Vector2 to, float height = 160f, float seconds = 0.55f)
        {
            var from = root.anchoredPosition;
            Sfx.Play(Sound.Hop);
            yield return Tween.Run(seconds, Ease.InOutSine, t =>
                root.anchoredPosition = Vector2.Lerp(from, to, t) + Vector2.up * (Mathf.Sin(t * Mathf.PI) * height));
            root.anchoredPosition = to;
        }

        /// <summary>Walks to an x position with a little step bounce.</summary>
        public IEnumerator WalkTo(float x, float speed = 520f)
        {
            var from = root.anchoredPosition;
            var to = new Vector2(x, from.y);
            var seconds = Mathf.Max(0.3f, Mathf.Abs(x - from.x) / speed);
            var steps = Mathf.Max(2, Mathf.RoundToInt(seconds * 3.5f));
            var lastStep = -1;
            yield return Tween.Run(seconds, Ease.Linear, t =>
            {
                var step = Mathf.FloorToInt(t * steps);
                if (step != lastStep)
                {
                    lastStep = step;
                    Sfx.Play(Sound.Step, Random.Range(0.9f, 1.1f));
                }

                root.anchoredPosition = Vector2.Lerp(from, to, t) + Vector2.up * Mathf.Abs(Mathf.Sin(t * steps * Mathf.PI)) * 14f;
            });
            root.anchoredPosition = to;
        }

        /// <summary>Waves for a moment, then goes back to the previous pose. Seated kids stay seated.</summary>
        public IEnumerator Wave(float seconds = 1.4f)
        {
            var previous = Pose;
            if (previous != "seated")
            {
                SetPose("wave");
            }

            yield return Tween.Wait(seconds);
            SetPose(previous);
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            if (dt <= 0f)
            {
                return;
            }

            // Breathing: a slow, small stretch of the body.
            breath += dt;
            bodyRoot.localScale = new Vector3(1f, 1f + Mathf.Sin(breath * 2.2f) * 0.008f, 1f);

            if (talkTime > 0f)
            {
                talkTime -= dt;
                ShowMouth(talkTime <= 0f ? mouth : (Mathf.FloorToInt(talkTime / 0.13f) % 2 == 0 ? Mouth.Talk : (mouth == Mouth.Talk ? Mouth.Smile : mouth)));
            }

            if (eyes == Eyes.Open || eyes == Eyes.Up || eyes == Eyes.Worried)
            {
                blinkTimer -= dt;
                if (blinkTimer <= 0f && !blinking)
                {
                    blinkTimer = Random.Range(2.5f, 5f);
                    StartCoroutine(IdleBlink());
                }
            }
        }

        private IEnumerator IdleBlink()
        {
            blinking = true;
            ShowEyes(Eyes.Closed);
            yield return Tween.Wait(0.12f);
            blinking = false;
            ShowEyes(eyes);
        }

        private void ShowEyes(Eyes value)
        {
            if (IsDoctor)
            {
                doctorEyes.sprite = ArtLibrary.Get($"Characters/doctor_eyes_{value.ToString().ToLowerInvariant()}");
            }
            else
            {
                avatarHead.SetEyes(value);
            }
        }

        private void ShowMouth(Mouth value)
        {
            if (IsDoctor)
            {
                doctorMouth.sprite = ArtLibrary.Get($"Characters/doctor_mouth_{value.ToString().ToLowerInvariant()}");
            }
            else
            {
                avatarHead.SetMouth(value);
            }
        }
    }
}
