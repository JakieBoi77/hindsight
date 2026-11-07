using System;
using System.Collections;
using System.Collections.Generic;
using Hindsight.Ophthalmology.Core.Avatar;
using UnityEngine;

namespace Hindsight.Ophthalmology.SceneObjects
{
    public enum KidPose
    {
        Idle,
        Wave,
        Cheer,
        Sit,
        Walk,
    }

    /// <summary>Which recolourable part an atlas cell belongs to.</summary>
    public enum KidRegion
    {
        None,
        Skin,
        Hair,
        Shirt,
        Shoes,
        Jeans, // fixed colours: trousers and trims of the everyday outfits
        White,
    }

    /// <summary>
    /// A child character (Sam, or the player's own kid) on the KayKit skeleton. Looks come from an
    /// <see cref="AvatarLook"/>. Outfits, hairstyles, hats and capes are child objects named
    /// "&lt;Slot&gt;_&lt;id&gt;" or "&lt;Slot&gt;_&lt;id&gt;_&lt;part&gt;" ("Outfit_knight_Body", "HairStyle_long") that are
    /// switched on and off; the outfits and heads come from the other KayKit characters, rebound to
    /// this skeleton. Colours are painted into a per-kid copy of each character's gradient atlas (the
    /// builder turns the recolourable swatches grey so a tint keeps their shading). Poses map to the pack's legacy clips; waving, talking, tilting back,
    /// shivering and blinking are procedural layers, as on the doctor.
    /// </summary>
    public sealed class KidActor : MonoBehaviour
    {
        [Serializable]
        public struct Cell
        {
            public int col;
            public int row;
            public KidRegion region;
        }

        /// <summary>
        /// Where a hat sits for one hairstyle (bone-local). Hats are made for one head; bigger or
        /// taller hair gets the hat raised and scaled so the hair stays underneath.
        /// </summary>
        [Serializable]
        public struct HatFit
        {
            public string hat;
            public string hairStyle;
            public Vector3 localPosition;
            public Vector3 localScale;
        }

        /// <summary>One source character's atlas: its material, a readable grey-shade copy, and which cells take which colour.</summary>
        [Serializable]
        public sealed class Paintable
        {
            public Material material;
            public Texture2D shade;
            public Cell[] cells = new Cell[0];
            [NonSerialized] public Texture2D painted;
        }

        [Header("Clips (matched by exact name, or by suffix after '|')")]
        [SerializeField] string idleClip = "Idle";
        [SerializeField] string walkClip = "Walking_A";
        [SerializeField] string cheerClip = "Cheer";
        [SerializeField] string sitClip = "Sit_Chair_Idle";
        [SerializeField] float crossFade = 0.25f;

        [Header("Bones and parts")]
        [SerializeField] Transform headBone;
        [SerializeField] Transform chestBone;
        [SerializeField] Transform upperArmR;
        [SerializeField] Transform lowerArmR;
        [SerializeField] Transform mouth;
        [SerializeField] Transform eyes;
        [SerializeField] Transform hips;
        [SerializeField] Transform spine;
        [SerializeField] Transform lowerLegL;
        [SerializeField] Transform lowerLegR;

        [Header("Seated in the exam chair")]
        [Tooltip("Upper body leans back this far against the reclined backrest.")]
        [SerializeField] float seatRecline = 38f;
        [Tooltip("Shins swing forward this far to lie along the leg rest.")]
        [SerializeField] float kneeExtend = 62f;

        [Header("Looks")]
        [SerializeField] int gridColumns = 8;
        [SerializeField] int gridRows = 4;
        [SerializeField] Paintable[] paintables = new Paintable[0];
        [Tooltip("Switchable parts named <Slot>_<id>[_<part>].")]
        [SerializeField] GameObject[] accessories = new GameObject[0];
        [SerializeField] HatFit[] hatFits = new HatFit[0];

        static readonly Dictionary<Texture2D, Color32[]> BasePixels = new();

        Animation anim;
        WaveIK wave;
        bool talking;
        float talkWeight;
        float waveWeight;
        float waveUntil;
        float tilt;
        float tiltTarget;
        float shiverUntil;
        float squash;
        Vector3 mouthClosedScale;
        Vector3 headScale = Vector3.one;
        AvatarLook appliedLook;
        bool seated;
        float danceUntil;

        [Header("Moving")]
        [SerializeField] float walkSpeed = 0.75f;
        [SerializeField] float turnSpeed = 360f;

        /// <summary>Where the kid's eyes are (the drop's target).</summary>
        public Vector3 EyePosition => eyes != null ? eyes.position : transform.position + Vector3.up;

        void Awake()
        {
            anim = GetComponentInChildren<Animation>();
            if (anim != null)
            {
                foreach (AnimationState state in anim)
                    state.wrapMode = Matches(state.name, idleClip) || Matches(state.name, walkClip) || Matches(state.name, sitClip)
                        ? WrapMode.Loop : WrapMode.Once;
                anim.playAutomatically = false;
                Play(KidPose.Idle);
            }
            if (mouth != null) mouthClosedScale = mouth.localScale;
            if (headBone != null) headScale = headBone.localScale;
        }

        void OnDestroy()
        {
            foreach (var p in paintables)
            {
                if (p.painted != null) Destroy(p.painted);
            }
        }

        // ------------------------------------------------------------------ looks

        /// <summary>Dresses the kid. Safe to call every frame: nothing is repainted unless a colour changed.</summary>
        public void Apply(AvatarLook look)
        {
            if (look == null) return;
            foreach (var item in accessories)
            {
                if (item == null) continue;
                var parts = item.name.Split('_');
                if (parts.Length < 2 || !Enum.TryParse<Slot>(parts[0], out var slot)) continue;
                var on = look.Get(slot) == parts[1];
                item.SetActive(on);
                if (on && slot == Slot.Hat) FitHat(item.transform, parts[1], look.hairStyle);
            }

            if (appliedLook != null && SameColours(appliedLook, look)) return;
            appliedLook = look.Copy();
            Paint(look);
        }

        void FitHat(Transform hat, string hatId, string hairStyle)
        {
            foreach (var fit in hatFits)
            {
                if (fit.hat != hatId || fit.hairStyle != hairStyle) continue;
                hat.localPosition = fit.localPosition;
                hat.localScale = fit.localScale;
                return;
            }
        }

        static bool SameColours(AvatarLook a, AvatarLook b) =>
            a.skin == b.skin && a.hair == b.hair && a.shirt == b.shirt && a.shoes == b.shoes;

        void Paint(AvatarLook look)
        {
            foreach (var p in paintables)
                if (p.shade != null && p.material != null) Paint(p, look);
        }

        void Paint(Paintable p, AvatarLook look)
        {
            if (!BasePixels.TryGetValue(p.shade, out var source))
            {
                source = p.shade.GetPixels32();
                BasePixels[p.shade] = source;
            }

            int w = p.shade.width, h = p.shade.height;
            if (p.painted == null)
            {
                p.painted = new Texture2D(w, h, TextureFormat.RGBA32, true) { name = name + "_" + p.material.name, wrapMode = TextureWrapMode.Clamp };
                // The renderers keep the shared material and get this kid's painted atlas through a
                // property block. (Per-kid material copies that differed only in their texture were
                // drawn with each other's texture.)
                var block = new MaterialPropertyBlock();
                block.SetTexture("_BaseMap", p.painted);
                foreach (var r in GetComponentsInChildren<Renderer>(true))
                    if (System.Array.IndexOf(r.sharedMaterials, p.material) >= 0) r.SetPropertyBlock(block);
            }

            var pixels = (Color32[])source.Clone();
            int cw = w / gridColumns, ch = h / gridRows;
            foreach (var cell in p.cells)
            {
                var tint = (Color32)ColourOf(look, cell.region);
                var y0 = h - (cell.row + 1) * ch; // rows count from the top of the PNG
                for (var y = y0; y < y0 + ch; y++)
                for (var x = cell.col * cw; x < (cell.col + 1) * cw; x++)
                {
                    var i = y * w + x;
                    var c = pixels[i];
                    pixels[i] = new Color32((byte)(c.r * tint.r / 255), (byte)(c.g * tint.g / 255), (byte)(c.b * tint.b / 255), 255);
                }
            }
            p.painted.SetPixels32(pixels);
            p.painted.Apply(true);
        }

        static Color ColourOf(AvatarLook look, KidRegion region)
        {
            var hex = region switch
            {
                KidRegion.Skin => look.skin,
                KidRegion.Hair => look.hair,
                KidRegion.Shirt => look.shirt,
                KidRegion.Shoes => look.shoes,
                KidRegion.Jeans => "#4A6A9E",
                KidRegion.White => "#F4F4F0",
                _ => "#FFFFFF",
            };
            return ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.white;
        }

        // ------------------------------------------------------------------ poses and moves

        public void Play(KidPose pose)
        {
            // A kid's arms are shorter than their head is wide, so a procedural wave folds the hand onto
            // the head. Kids greet with the pack's hand-animated Cheer clip (arms up, a happy bounce).
            if (pose == KidPose.Wave) pose = KidPose.Cheer;
            if (anim == null) return;
            var clip = FindClip(pose);
            if (clip == null) return;
            anim.CrossFade(clip, crossFade);
            if (pose == KidPose.Cheer)
            {
                var idle = FindClip(KidPose.Idle);
                if (idle != null) anim.CrossFadeQueued(idle, crossFade);
            }
        }

        public void SetTalking(bool value) => talking = value;

        public void Show(bool visible) => gameObject.SetActive(visible);

        /// <summary>Stands the kid at an anchor, facing its forward.</summary>
        public void PlaceAt(Transform target)
        {
            if (target == null) return;
            Show(true);
            seated = false;
            tiltTarget = tilt = 0f;
            transform.SetPositionAndRotation(target.position, target.rotation);
            Play(KidPose.Idle);
            if (anim != null) anim.Sample();
        }

        /// <summary>Walks to <paramref name="target"/> and turns to face its forward.</summary>
        public IEnumerator WalkTo(Transform target)
        {
            if (target == null) yield break;
            Show(true);
            seated = false;
            tiltTarget = tilt = 0f;
            var flat = new Vector3(target.position.x, transform.position.y, target.position.z);
            if (Vector3.Distance(transform.position, flat) > 0.05f)
            {
                Play(KidPose.Walk);
                while (Vector3.Distance(transform.position, flat) > 0.02f)
                {
                    var dir = flat - transform.position;
                    if (dir.sqrMagnitude > 0.0001f)
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);
                    transform.position = Vector3.MoveTowards(transform.position, flat, walkSpeed * Time.deltaTime);
                    yield return null;
                }
                Play(KidPose.Idle);
            }
            while (Quaternion.Angle(transform.rotation, target.rotation) > 1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target.rotation, turnSpeed * Time.deltaTime);
                yield return null;
            }
            transform.rotation = target.rotation;
        }

        /// <summary>A little happy dance: the cheer clip on repeat with a bounce and a wiggle, then idle.</summary>
        public IEnumerator Dance(float seconds)
        {
            danceUntil = Time.time + seconds;
            var clip = FindClip(KidPose.Cheer);
            if (anim != null && clip != null)
            {
                anim[clip].wrapMode = WrapMode.Loop;
                anim.CrossFade(clip, crossFade);
            }
            while (Time.time < danceUntil) yield return null;
            if (anim != null && clip != null) anim[clip].wrapMode = WrapMode.Once;
            Play(KidPose.Idle);
        }

        /// <summary>
        /// Sits the kid in the chair: the seat anchor's rotation is the body's (facing the footrest,
        /// leaning back with the backrest) and its position is where the hips go.
        /// </summary>
        public void SitInSeat(Transform seat)
        {
            if (seat == null) return;
            Show(true);
            tiltTarget = tilt = 0f;
            transform.rotation = seat.rotation;
            var a = anim != null ? anim : GetComponentInChildren<Animation>();
            if (a != null)
            {
                foreach (AnimationState state in a)
                {
                    if (!Matches(state.name, sitClip)) continue;
                    state.clip.SampleAnimation(a.gameObject, 0f);
                    if (Application.isPlaying) a.Play(state.name);
                    break;
                }
            }
            seated = true;
            ApplySeated(); // also poses it outside Play mode
            if (hips != null) transform.position += seat.position - hips.position;
        }

        /// <summary>Lean the upper body back against the backrest and lay the shins along the leg rest.</summary>
        void ApplySeated()
        {
            var right = transform.right;
            if (spine != null) spine.rotation = Quaternion.AngleAxis(-seatRecline, right) * spine.rotation;
            if (headBone != null) headBone.rotation = Quaternion.AngleAxis(seatRecline * 0.45f, right) * headBone.rotation;
            if (lowerLegL != null) lowerLegL.rotation = Quaternion.AngleAxis(-kneeExtend, right) * lowerLegL.rotation;
            if (lowerLegR != null) lowerLegR.rotation = Quaternion.AngleAxis(-kneeExtend, right) * lowerLegR.rotation;
        }

        /// <summary>Head back (1) or upright (0), eased.</summary>
        public void SetTiltBack(float amount) => tiltTarget = Mathf.Clamp01(amount);

        public void Shiver(float seconds = 0.8f) => shiverUntil = Time.time + seconds;

        /// <summary>Two quick squints and a happy wiggle.</summary>
        public IEnumerator Blink()
        {
            for (var i = 0; i < 2; i++)
            {
                for (var t = 0f; t < 0.12f; t += Time.deltaTime)
                {
                    squash = Mathf.Sin(Mathf.PI * t / 0.12f);
                    yield return null;
                }
                squash = 0f;
                for (var t = 0f; t < 0.15f; t += Time.deltaTime) yield return null;
            }
        }

        // Procedural layers run after the Animation component has posed the skeleton this frame.
        void LateUpdate()
        {
            var dt = Time.deltaTime;
            var t = Time.time;
            talkWeight = Mathf.MoveTowards(talkWeight, talking ? 1f : 0f, dt * 6f);
            waveWeight = Mathf.MoveTowards(waveWeight, t < waveUntil ? 1f : 0f, dt * 4f);
            tilt = Mathf.MoveTowards(tilt, tiltTarget, dt * 2f);
            if (seated) ApplySeated();
            if (t < danceUntil)
            {
                // Bounce on the beat and wiggle the shoulders, on top of the cheer clip.
                if (hips != null) hips.position += transform.up * (Mathf.Abs(Mathf.Sin(t * 7f)) * 0.035f);
                if (spine != null) spine.rotation = Quaternion.AngleAxis(Mathf.Sin(t * 3.5f) * 14f, transform.up) * spine.rotation;
            }

            if (headBone != null)
            {
                var right = transform.right;
                var nod = talkWeight * (Mathf.Sin(t * 7.3f) * 5f + Mathf.Sin(t * 2.9f) * 3f);
                var shiver = t < shiverUntil ? Mathf.Sin(t * 60f) * 4f : 0f;
                headBone.rotation = Quaternion.AngleAxis(nod - 32f * Mathf.SmoothStep(0f, 1f, tilt), right)
                                    * Quaternion.AngleAxis(shiver, transform.forward) * headBone.rotation;
                headBone.localScale = Vector3.Scale(headScale, new Vector3(1f + 0.06f * squash, 1f - 0.12f * squash, 1f));
            }
            if (chestBone != null && t < shiverUntil)
                chestBone.rotation = Quaternion.AngleAxis(Mathf.Sin(t * 55f) * 3f, transform.up) * chestBone.rotation;

            if (mouth != null)
            {
                var open = talkWeight * Mathf.Clamp01(0.5f + 0.5f * Mathf.Sin(t * 17f) * Mathf.Sin(t * 5.3f + 1f));
                mouth.localScale = new Vector3(mouthClosedScale.x, mouthClosedScale.y * (1f + 1.1f * open), mouthClosedScale.z);
            }

            if (waveWeight > 0f)
            {
                if (wave == null) wave = GetComponent<WaveIK>();
                if (wave != null) wave.Apply(waveWeight, Mathf.Sin(t * 9f));
            }
        }

        string FindClip(KidPose pose)
        {
            var wanted = pose switch
            {
                KidPose.Idle => idleClip,
                KidPose.Walk => walkClip,
                KidPose.Cheer => cheerClip,
                KidPose.Sit => sitClip,
                _ => null,
            };
            if (wanted == null || anim == null) return null;
            foreach (AnimationState state in anim)
                if (Matches(state.name, wanted)) return state.name;
            return null;
        }

        static bool Matches(string clipName, string wanted) =>
            clipName == wanted || clipName.EndsWith("|" + wanted, StringComparison.OrdinalIgnoreCase);

#if UNITY_EDITOR
        public void EditorInit(Transform head, Transform chest, Transform upperArm, Transform lowerArm, Transform mouthT, Transform eyesT,
            Transform hipsT, Transform spineT, Transform shinL, Transform shinR, Paintable[] atlases, GameObject[] items, HatFit[] fits)
        {
            headBone = head;
            chestBone = chest;
            upperArmR = upperArm;
            lowerArmR = lowerArm;
            mouth = mouthT;
            eyes = eyesT;
            hips = hipsT;
            spine = spineT;
            lowerLegL = shinL;
            lowerLegR = shinR;
            paintables = atlases;
            accessories = items;
            hatFits = fits;
        }
#endif
    }
}
