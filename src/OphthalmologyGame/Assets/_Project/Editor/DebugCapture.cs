using System.IO;
using System.Linq;
using System.Text;
using Hindsight.Ophthalmology.SceneObjects;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Hindsight.Ophthalmology.EditorTools
{
    /// <summary>
    /// Layout checks without Play mode: logs the exam chair's top profile and renders Sam seated and
    /// the characters waving into ./Captures/debug. Scenes are opened fresh and never saved.
    /// </summary>
    public static class DebugCapture
    {
        [MenuItem("POC/Debug Captures")]
        public static void Run()
        {
            var outDir = Path.GetFullPath("Captures/debug");
            Directory.CreateDirectory(outDir);
            ShaderUtil.allowAsyncCompilation = false;
            EditorSceneManager.OpenScene(PocBuilder.EyeDropsScenePath);
            Object.FindAnyObjectByType<Canvas>().gameObject.SetActive(false);

            // ---- Chair profile along its centre line (headrest at -Z).
            var chair = GameObject.Find("ExamChair");
            var probes = chair.GetComponentsInChildren<MeshFilter>().Where(m => m.sharedMesh != null).Select(m =>
            {
                var go = new GameObject("Probe");
                go.transform.SetPositionAndRotation(m.transform.position, m.transform.rotation);
                go.transform.localScale = m.transform.lossyScale;
                go.AddComponent<MeshCollider>().sharedMesh = m.sharedMesh;
                return go.GetComponent<MeshCollider>();
            }).ToList();
            Physics.SyncTransforms();
            var sb = new StringBuilder("DEBUG chair profile (z: top y)\n");
            var cx = chair.transform.position.x;
            for (var z = -1.0f; z <= 1.3f; z += 0.05f)
            {
                var best = float.MinValue;
                foreach (var p in probes)
                    if (p.Raycast(new Ray(new Vector3(cx, 3f, z), Vector3.down), out var hit, 4f)) best = Mathf.Max(best, hit.point.y);
                sb.AppendLine($"  z={z:0.00} y={(best > float.MinValue ? best.ToString("0.000") : "-")}");
            }
            var head = chair.transform.Find("Head").position;
            sb.AppendLine($"  Head anchor {head}");
            Debug.Log(sb.ToString());
            foreach (var p in probes) Object.DestroyImmediate(p.gameObject);

            // ---- Sam seated, from the story camera and from the side.
            var sam = GameObject.Find("Sam").GetComponent<KidActor>();
            var anchors = GameObject.Find("Anchors").transform;
            var seat = anchors.Find("SamSeat");
            if (seat != null) sam.SitInSeat(seat);
            Render("SamChair", outDir, "sam_story");
            RenderAt(head + new Vector3(2.2f, 0.2f, 0.1f), head + new Vector3(0, -0.3f, 0.1f), 50f, Path.Combine(outDir, "sam_side.png"));

            // ---- Waves (the IK is applied once on the sampled idle pose).
            var doctor = GameObject.Find("Doctor").GetComponent<DoctorActor>();
            doctor.transform.SetPositionAndRotation(anchors.Find("DoctorStandPoint").position, anchors.Find("DoctorStandPoint").rotation);
            Idle(doctor.gameObject);
            doctor.ApplyWave(1f, 0.8f);
            Render("DoctorGreeting", outDir, "doctor_wave");
            var dPos = doctor.transform.position;
            RenderAt(dPos + doctor.transform.forward * 1.6f + doctor.transform.right * 1.2f + Vector3.up * 1.2f, dPos + Vector3.up * 1.1f, 50f,
                Path.Combine(outDir, "doctor_wave_side.png"));

            GameObject.Find("PlayerKid").SetActive(false);
            var kid = ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBuilder.KidPath))).GetComponent<KidActor>();
            var stand = anchors.Find("SamWaitPoint");
            kid.transform.SetPositionAndRotation(stand.position, stand.rotation);
            sam.gameObject.SetActive(false);
            Idle(kid.gameObject);
            var ik = kid.GetComponent<WaveIK>();
            var so = new SerializedObject(ik);
            Debug.Log($"DEBUG kid ik upper={so.FindProperty("upperArm").objectReferenceValue} lower={so.FindProperty("lowerArm").objectReferenceValue} hand={so.FindProperty("hand").objectReferenceValue} centre={so.FindProperty("headCenter").objectReferenceValue} r={so.FindProperty("headRadius").floatValue}");
            var kanim = kid.GetComponentInChildren<Animation>();
            Debug.Log("DEBUG kid clips: " + string.Join(",", kanim.Cast<AnimationState>().Select(st => st.name).Take(8)) + " count=" + kanim.GetClipCount());
            ik.Apply(1f, 0.8f);
            Render("SamWaiting", outDir, "kid_wave");
            var kPos = kid.transform.position;
            RenderAt(kPos + kid.transform.forward * 1.2f + kid.transform.right * 0.9f + Vector3.up * 0.8f, kPos + Vector3.up * 0.7f, 50f,
                Path.Combine(outDir, "kid_wave_side.png"));
            Render("FirstPersonTiltBack", outDir, "ceiling_sun");
            foreach (var n in new[] { "DoctorPortrait", "SamPortrait" })
            {
                var go = GameObject.Find(n);
                var hc = go.GetComponentsInChildren<Transform>(true).First(t => t.name == "HeadCenter");
                Debug.Log($"DEBUG {n} root={go.transform.position} headCenter={hc.position} parent={hc.parent.name} bounds={EditorWorldBounds(go)} cam={GameObject.Find(n + "Camera").transform.position}");
            }
            foreach (var name in new[] { "DoctorPortraitCamera", "SamPortraitCamera" })
            {
                var c = GameObject.Find(name).GetComponent<Camera>();
                RenderAt(c.transform.position, c.transform.position + c.transform.forward, c.fieldOfView, Path.Combine(outDir, name + ".png"));
            }
            Debug.Log("DEBUG captures written to " + outDir);
        }

        static Bounds EditorWorldBounds(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>();
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        static void Idle(GameObject actor)
        {
            var anim = actor.GetComponentInChildren<Animation>();
            foreach (AnimationState s in anim)
                {
                if (s.name != "Idle" && !s.name.EndsWith("|Idle")) continue;
                s.clip.SampleAnimation(anim.gameObject, 0.4f);
                Debug.Log($"DEBUG sampled {s.name} on {actor.name}");
            }
        }

        static void Render(string shot, string dir, string file)
        {
            var cam = GameObject.Find(shot).transform;
            RenderAt(cam.position, cam.position + cam.forward, 50f, Path.Combine(dir, file + ".png"));
        }

        static void RenderAt(Vector3 from, Vector3 to, float fov, string file)
        {
            const int w = 1200, h = 800;
            var go = new GameObject("CaptureCam");
            go.transform.SetPositionAndRotation(from, Quaternion.LookRotation(to - from, Vector3.up));
            var cam = go.AddComponent<Camera>();
            cam.fieldOfView = fov;
            cam.nearClipPlane = 0.01f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.75f, 0.9f);
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            cam.Render();
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            File.WriteAllBytes(file, tex.EncodeToPNG());
            cam.targetTexture = null;
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(go);
        }
    }
}
