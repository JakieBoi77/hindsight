using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hindsight.EyeCanDo.Core.Avatar;
using Hindsight.EyeCanDo.SceneObjects;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hindsight.EyeCanDo.EditorTools
{
    /// <summary>
    /// Contact sheets of the bean characters into ./Captures/looks, on an empty stage: every outfit
    /// (front and back), every hat (front and side), every face, nose and colour, and filmstrips of
    /// each animation (the rig's clock is stepped by hand). Nothing is saved.
    /// </summary>
    public static class DebugLooks
    {
        const float Step = 1f / 60f;

        [MenuItem("POC/Debug Looks")]
        public static void Run()
        {
            var outDir = Path.GetFullPath("Captures/looks");
            if (Directory.Exists(outDir)) Directory.Delete(outDir, true);
            Directory.CreateDirectory(outDir);
            ShaderUtil.allowAsyncCompilation = false;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.1f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(45, 160, 0);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.62f, 0.64f, 0.7f);
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.transform.localScale = Vector3.one * 5f;
            floor.GetComponent<Renderer>().sharedMaterial = BuilderUtil.Lit("PortraitFloor", new Color(0.8f, 0.9f, 1f));

            string File(string name) => Path.Combine(outDir, name + ".png");
            var skins = AvatarCatalog.For(Slot.Skin).Select(i => i.Id).ToArray();
            string Skin(int i) => skins[i % skins.Length];

            // The first renders of a fresh headless editor draw some materials wrongly while shaders
            // load, so render one throwaway sheet first.
            Row(AvatarCatalog.For(Slot.Outfit).Select(o => new AvatarLook { outfit = o.Id }).Take(6).ToList(), File("_warmup"));
            System.IO.File.Delete(File("_warmup"));

            // ---- Fits, front and back; hats front and side.
            Pages(AvatarCatalog.For(Slot.Outfit).ToList(), 6, (items, page) =>
            {
                var looks = items.Select((o, i) => new AvatarLook { outfit = o.Id, skin = Skin(page * 6 + i) }).ToList();
                Row(looks, File($"fits_{page + 1}"));
                Row(looks, File($"fits_{page + 1}_back"), back: true);
            });
            Pages(AvatarCatalog.For(Slot.Hat).ToList(), 6, (items, page) =>
            {
                var looks = items.Select((h, i) => new AvatarLook { hat = h.Id, skin = Skin(page * 6 + i + 2) }).ToList();
                Row(looks, File($"hats_{page + 1}"), close: true);
                Row(looks, File($"hats_{page + 1}_side"), close: true, yaw: 60f);
            });

            // ---- Faces: every eye style open and shut, every mouth at each opening, every brow in every mood.
            Pages(FaceArt.EyeStyles.ToList(), 6, (items, page) =>
            {
                var looks = items.Select((e, i) => new AvatarLook { eyes = e, brows = AvatarCatalog.None, skin = Skin(page * 6 + i) }).ToList();
                Row(looks, File($"eyes_{page + 1}"), close: true);
                Row(looks, File($"eyes_{page + 1}_shut"), close: true, pose: (rig, _) => rig.SetEyesShut(true));
            });
            Pages(FaceArt.MouthStyles.ToList(), 5, (items, page) =>
            {
                for (var f = 0; f < FaceArt.MouthFrames; f++)
                {
                    var frame = f;
                    Row(items.Select((m, i) => new AvatarLook { mouth = m, skin = Skin(page * 5 + i + 1) }).ToList(), File($"mouths_{page + 1}_shape{f}"), close: true,
                        pose: (rig, _) =>
                        {
                            // Talk with a voice held at the level for this mouth shape.
                            rig.SetTalking(frame > 0);
                            rig.Voice = () => frame == 1 ? 0.3f : frame == 2 ? 0.6f : 1f;
                            Advance(rig, 0.4f);
                        });
                }
            });
            // Brows: every style, a sheet per mood.
            for (var m = 0; m < FaceArt.MoodCount; m++)
            {
                var mood = (Mood)m;
                Pages(FaceArt.BrowStyles.ToList(), 6, (items, page) =>
                    Row(items.Select((b, i) => new AvatarLook { brows = b, skin = Skin(page * 6 + i + 1) }).ToList(),
                        File($"brows_{page + 1}_{mood.ToString().ToLowerInvariant()}"), close: true, pose: (rig, _) => rig.SetMood(mood)));
            }
            Row(skins.Take(5).Select(c => new AvatarLook { skin = c }).ToList(), File("colours_1"));
            Row(skins.Skip(5).Select(c => new AvatarLook { skin = c }).ToList(), File("colours_2"));

            // ---- Animation filmstrips: the same look at successive moments.
            var sam = AvatarLook.Sam;
            Filmstrip(sam, File("anim_walk"), 0.08f, rig => { rig.SetMoveSpeed(0.75f); rig.SetMode(BeanMode.Walk); }, yaw: 70f);
            Filmstrip(sam, File("anim_wave"), 0.1f, rig => rig.Wave(10f));
            Filmstrip(sam, File("anim_cheer"), 0.22f, rig => rig.Cheer());
            Filmstrip(sam, File("anim_dance"), 0.12f, rig => rig.Dance(10f));
            Filmstrip(sam, File("anim_talk"), 0.07f, rig => rig.SetTalking(true));
            Filmstrip(sam, File("anim_sit_tilt"), 0.25f, rig => { rig.SetMode(BeanMode.Sit); rig.SetTilt(1f); }, yaw: 80f);
            Filmstrip(sam, File("anim_shiver_blink"), 0.05f, rig => { rig.Shiver(10f); rig.SetEyesShut(true); rig.SetSquash(0.8f); });

            // ---- The doctor: standing, waving, reaching with the light and walking.
            var doctors = new List<BeanRig>();
            var poses = new Action<BeanRig>[]
            {
                _ => { },
                r => r.Wave(10f),
                r => r.GetComponent<DoctorActor>().Examine(r.transform.position + r.transform.forward * 0.9f + Vector3.up * 0.9f, 0.35f),
                r => { r.GetComponent<DoctorActor>().SetProp(DoctorProp.EyeDrops); r.Reach(10f, left: true); },
                r => r.SetTalking(true),
                r => { r.SetMoveSpeed(1.1f); r.SetMode(BeanMode.Walk); },
            };
            for (var i = 0; i < poses.Length; i++)
            {
                var d = Spawn(PrefabBuilder.DoctorPath, new Vector3(i * 1.1f, 0f, 0f), i == 5 ? 60f : 0f);
                d.ApplyLook(AvatarLook.Doctor);
                d.GetComponent<DoctorActor>().SetProp(DoctorProp.None);
                poses[i](d);
                Advance(d, 1.5f);
                doctors.Add(d);
            }
            // Beside a kid for scale.
            var kid = Spawn(PrefabBuilder.KidPath, new Vector3(poses.Length * 1.1f - 0.2f, 0f, 0f), 0f);
            kid.ApplyLook(AvatarLook.Default);
            Advance(kid, 0.6f);
            var width = poses.Length * 1.1f;
            Shot(new Vector3(width / 2f - 0.5f, 1.2f, 7.5f), new Vector3(width / 2f - 0.5f, 0.8f, 0f), 30f, 2000, 700, File("doctor"));
            Shot(new Vector3(0f, 1.42f, 2.2f), new Vector3(0f, 1.3f, 0f), 30f, 900, 700, File("doctor_face"));
            foreach (var d in doctors) Object.DestroyImmediate(d.gameObject);
            Object.DestroyImmediate(kid.gameObject);

            Debug.Log("DEBUG looks written to " + outDir);
        }

        /// <summary>Up to six kids in a row with different looks; <paramref name="close"/> frames the faces.</summary>
        static void Row(List<AvatarLook> looks, string file, bool back = false, bool close = false, float yaw = 0f, Action<BeanRig, int> pose = null)
        {
            var spacing = close ? 0.62f : 0.8f;
            var rigs = new List<BeanRig>();
            for (var i = 0; i < looks.Count; i++)
            {
                var rig = Spawn(PrefabBuilder.KidPath, new Vector3(-i * spacing, 0f, 0f), (back ? 180f : 0f) + yaw);
                rig.ApplyLook(looks[i]);
                Advance(rig, 0.3f);
                pose?.Invoke(rig, i);
                Advance(rig, 0.05f);
                rigs.Add(rig);
            }
            var centreX = -(looks.Count - 1) * spacing / 2f;
            if (close) Shot(new Vector3(centreX, 0.88f, 3.3f), new Vector3(centreX, 0.76f, 0f), 15f, 2000, 480, file);
            else Shot(new Vector3(centreX, 0.9f, 5.6f), new Vector3(centreX, 0.5f, 0f), 22f, 2000, 640, file);
            foreach (var r in rigs) Object.DestroyImmediate(r.gameObject);
        }

        /// <summary>Six copies of one look, each stepped <paramref name="interval"/> seconds further into the same move.</summary>
        static void Filmstrip(AvatarLook look, string file, float interval, Action<BeanRig> start, float yaw = 0f)
        {
            const float spacing = 0.85f;
            var rigs = new List<BeanRig>();
            for (var i = 0; i < 6; i++)
            {
                var rig = Spawn(PrefabBuilder.KidPath, new Vector3(-i * spacing, 0f, 0f), yaw);
                rig.ApplyLook(look);
                start(rig);
                Advance(rig, 0.3f + i * interval);
                rigs.Add(rig);
            }
            var centreX = -5 * spacing / 2f;
            Shot(new Vector3(centreX, 1.0f, 5.8f), new Vector3(centreX, 0.55f, 0f), 22f, 2000, 640, file);
            foreach (var r in rigs) Object.DestroyImmediate(r.gameObject);
        }

        /// <summary>Calls <paramref name="sheet"/> with each page of up to <paramref name="perPage"/> items.</summary>
        static void Pages<T>(List<T> all, int perPage, Action<List<T>, int> sheet)
        {
            for (var page = 0; page * perPage < all.Count; page++) sheet(all.Skip(page * perPage).Take(perPage).ToList(), page);
        }

        static BeanRig Spawn(string prefab, Vector3 pos, float yaw)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefab));
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
            var highlight = go.transform.Find("Highlight");
            if (highlight != null) highlight.gameObject.SetActive(false);
            return go.GetComponent<BeanRig>();
        }

        static void Advance(BeanRig rig, float seconds)
        {
            for (var t = 0f; t < seconds; t += Step) rig.Evaluate(Step);
        }

        static void Shot(Vector3 from, Vector3 to, float fov, int w, int h, string file)
        {
            var go = new GameObject("Cam");
            go.transform.SetPositionAndRotation(from, Quaternion.LookRotation(to - from));
            var cam = go.AddComponent<Camera>();
            cam.fieldOfView = fov;
            cam.nearClipPlane = 0.05f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.85f, 0.92f, 1f);
            var rt = new RenderTexture(w, h, 24) { antiAliasing = 4 };
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            RenderTexture.active = null;
            System.IO.File.WriteAllBytes(file, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(go);
        }
    }
}
