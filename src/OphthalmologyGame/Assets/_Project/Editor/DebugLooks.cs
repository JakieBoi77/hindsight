using System.IO;
using Hindsight.Ophthalmology.Core.Avatar;
using Hindsight.Ophthalmology.SceneObjects;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Linq;

namespace Hindsight.Ophthalmology.EditorTools
{
    /// <summary>
    /// Renders the kid in every outfit, hairstyle, hat and cape, and frames of the idle clips and the
    /// wave, into ./Captures/looks, on an empty stage. Nothing is saved.
    /// </summary>
    public static class DebugLooks
    {
        [MenuItem("POC/Debug Looks")]
        public static void Run()
        {
            var outDir = Path.GetFullPath("Captures/looks");
            Directory.CreateDirectory(outDir);
            ShaderUtil.allowAsyncCompilation = false;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(40, 150, 0);
            RenderSettings.ambientLight = new Color(0.6f, 0.6f, 0.65f);

            var kid = ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBuilder.KidPath))).GetComponent<KidActor>();
            // Every outfit, front and back.
            var shirts = new[] { "#FF6B6B", "#4A90E2", "#5CBF73", "#FFC845", "#9B6BE0", "#FF9BC2", "#2BB3A3" };
            var outfits = AvatarCatalog.For(Slot.Outfit).ToList();
            for (var i = 0; i < outfits.Count; i++)
            {
                var look = new AvatarLook { outfit = outfits[i].Id, shirt = shirts[i % shirts.Length] };
                kid.Apply(look);
                Sample(kid.gameObject, "Idle", 0.3f);
                Shot(kid.transform, 1.0f, 0.5f, Path.Combine(outDir, $"outfit_{outfits[i].Id}.png"));
                Shot(kid.transform, 1.0f, 0.5f, Path.Combine(outDir, $"outfit_{outfits[i].Id}_back.png"), back: true);
            }
            kid.Apply(AvatarLook.Sam);
            Sample(kid.gameObject, "Idle", 0.3f);
            Shot(kid.transform, 1.0f, 0.5f, Path.Combine(outDir, "outfit_sam.png"));

            // Every hat on every hairstyle (front and side), so none pokes through the hair.
            foreach (var hair in AvatarCatalog.For(Slot.HairStyle))
            foreach (var hat in AvatarCatalog.For(Slot.Hat))
            {
                kid.Apply(new AvatarLook { hairStyle = hair.Id, hat = hat.Id, hair = "#D8692E" });
                Sample(kid.gameObject, "Idle", 0.3f);
                Shot(kid.transform, 0.55f, 0.66f, Path.Combine(outDir, $"hat_{hair.Id}_{hat.Id}.png"));
                Shot(kid.transform, 0.55f, 0.66f, Path.Combine(outDir, $"hat_{hair.Id}_{hat.Id}_side.png"), side: true);
            }

            kid.Apply(new AvatarLook());
            foreach (var clip in new[] { "Idle", "Walking_A", "Cheer" })
            foreach (var t in new[] { 0f, 0.4f, 0.8f })
            {
                Sample(kid.gameObject, clip, t);
                Shot(kid.transform, 1.0f, 0.5f, Path.Combine(outDir, $"kid_{clip}_{t:0.0}.png"));
            }

            Object.DestroyImmediate(kid.gameObject);
            var doctor = ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBuilder.DoctorPath))).GetComponent<DoctorActor>();
            foreach (var clip in new[] { "Idle", "Unarmed_Idle" })
            foreach (var t in new[] { 0f, 0.7f })
            {
                Sample(doctor.gameObject, clip, t);
                Shot(doctor.transform, 1.5f, 0.75f, Path.Combine(outDir, $"doctor_{clip}_{t:0.0}.png"));
            }
            foreach (var swing in new[] { -1f, 1f })
            {
                Sample(doctor.gameObject, "Idle", 0.3f);
                doctor.ApplyWave(1f, swing);
                Shot(doctor.transform, 1.5f, 0.8f, Path.Combine(outDir, $"doctor_wave_{swing:0}.png"));
            }
            Debug.Log("DEBUG looks written to " + outDir);
        }

        static string PathOf(Transform t) => t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name;

        static void Sample(GameObject actor, string clip, float time)
        {
            var anim = actor.GetComponentInChildren<Animation>();
            foreach (AnimationState s in anim)
                if (s.name == clip || s.name.EndsWith("|" + clip)) s.clip.SampleAnimation(anim.gameObject, time);
        }

        static void Shot(Transform target, float height, float lookHeight, string file, bool back = false, bool side = false)
        {
            var dir = side ? target.right : back ? -target.forward : target.forward;
            var from = target.position + dir * height * 2.1f + Vector3.up * (lookHeight + height * 0.25f) + (side ? -target.forward * height * 0.3f : target.right * height * 0.35f);
            var to = target.position + Vector3.up * lookHeight;
            const int w = 600, h = 700;
            var go = new GameObject("Cam");
            go.transform.SetPositionAndRotation(from, Quaternion.LookRotation(to - from));
            var cam = go.AddComponent<Camera>();
            cam.fieldOfView = 35f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.85f, 0.92f, 1f);
            var rt = new RenderTexture(w, h, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            RenderTexture.active = null;
            File.WriteAllBytes(file, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(go);
        }
    }
}
