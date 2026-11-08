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

            // Hidden at runtime by their Awake: the floating exam light and the "tap me" diamonds.
            Object.FindAnyObjectByType<OphthalmoscopeRig>().Show(false);
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
                if (t.name == "Highlight") t.gameObject.SetActive(false);

            // ---- Sam seated (and with his head back), from the story camera and from the side.
            var sam = GameObject.Find("Sam").GetComponent<KidActor>();
            sam.Apply(Hindsight.Ophthalmology.Core.Avatar.AvatarLook.Sam);
            var anchors = GameObject.Find("Anchors").transform;
            var seat = anchors.Find("SamSeat");
            if (seat != null) sam.SitInSeat(seat);
            Render("SamChair", outDir, "sam_story"); // the first renders of a fresh editor draw some materials wrongly: render twice
            Render("SamChair", outDir, "sam_story");
            RenderAt(head + new Vector3(2.2f, 0.2f, 0.1f), head + new Vector3(0, -0.3f, 0.1f), 50f, Path.Combine(outDir, "sam_side.png"));
            sam.Rig.SetTilt(1f);
            Advance(sam.Rig, 1f);
            Render("SamChair", outDir, "sam_story_tilt");
            RenderAt(head + new Vector3(2.2f, 0.2f, 0.1f), head + new Vector3(0, -0.3f, 0.1f), 50f, Path.Combine(outDir, "sam_side_tilt.png"));
            Debug.Log($"DEBUG Sam eyes {sam.EyePosition} chair eyes {anchors.Find("ChairEyes").position}");

            // ---- Dr. Iris looks into Sam's eye (leaning in, light at her eye), then holds the drops over it.
            var doc = GameObject.Find("Doctor").GetComponent<DoctorActor>();
            doc.Rig.ApplyLook(Hindsight.Ophthalmology.Core.Avatar.AvatarLook.Doctor);
            doc.SetProp(DoctorProp.None);
            doc.PlaceAt(anchors.Find("DoctorSamPoint"));
            doc.Examine(sam.EyePosition, 0.72f);
            Advance(doc.Rig, 2f);
            doc.PoseLight(10f);
            Render("SamChair", outDir, "exam_sam");
            RenderAt(head + new Vector3(0.3f, 0.35f, 1.6f), head + new Vector3(0.3f, -0.1f, 0f), 50f, Path.Combine(outDir, "exam_sam_front.png"));
            var dropRig = Object.FindAnyObjectByType<EyeDropRig>();
            doc.FinishTask();
            Advance(doc.Rig, 2f);
            var towardDoc = doc.transform.position - sam.EyePosition;
            towardDoc.y = 0f;
            dropRig.Bottle.SetPositionAndRotation(sam.EyePosition + Vector3.up * 0.14f + towardDoc.normalized * 0.05f, Quaternion.identity);
            doc.HoldBottle(dropRig.Bottle, sam.EyePosition);
            doc.CompleteTurn();
            Advance(doc.Rig, 2f);
            Debug.Log($"DEBUG reach to bottle over Sam: hand {Reach(doc, dropRig.Bottle):0.000} m from the bottle");
            Render("SamChair", outDir, "drop_sam");

            // ---- First person in the chair: the exam (standing back, then leaning in) and the drop.
            sam.gameObject.SetActive(false);
            GameObject.Find("PlayerKid").SetActive(false);
            var chairEyes = anchors.Find("ChairEyes").position;
            doc.FinishTask();
            doc.PlaceAt(anchors.Find("DoctorExamPoint"));
            Advance(doc.Rig, 0.1f);
            Render("ExamCloseFP", outDir, "fp_exam_start");
            doc.Examine(chairEyes, 0.42f);
            Advance(doc.Rig, 2.5f);
            doc.PoseLight(10f);
            Render("ExamCloseFP", outDir, "fp_exam_leaning");
            RenderAt(chairEyes + new Vector3(2.2f, 0.4f, 0.8f), chairEyes + new Vector3(0f, -0.2f, 0.7f), 50f, Path.Combine(outDir, "fp_exam_side.png"));
            doc.FinishTask();
            doc.PlaceAt(anchors.Find("DoctorDropPoint"));
            dropRig.ShowHolding();
            doc.HoldBottle(dropRig.Bottle, dropRig.HeldOverEye);
            doc.CompleteTurn();
            Advance(doc.Rig, 2.5f);
            Debug.Log($"DEBUG reach to bottle over the player: hand {Reach(doc, dropRig.Bottle):0.000} m from the bottle");
            Render("FirstPersonTiltBack", outDir, "fp_drop");
            Render("FirstPerson", outDir, "fp_drop_front");
            RenderAt(chairEyes + new Vector3(0.5f, 0.7f, 1.7f), chairEyes + new Vector3(0.25f, 0.05f, 0f), 50f, Path.Combine(outDir, "fp_drop_side.png"));
            dropRig.ShowAtRest();
            doc.FinishTask();
            sam.gameObject.SetActive(true);

            // ---- Waves, in the story's shots.
            var doctor = GameObject.Find("Doctor").GetComponent<DoctorActor>();
            doctor.Rig.ApplyLook(Hindsight.Ophthalmology.Core.Avatar.AvatarLook.Doctor);
            doctor.transform.SetPositionAndRotation(anchors.Find("DoctorStandPoint").position, anchors.Find("DoctorStandPoint").rotation);
            doctor.Rig.ResetLayers();
            doctor.Rig.Wave(10f);
            Advance(doctor.Rig, 0.6f);
            Render("DoctorGreeting", outDir, "doctor_wave");
            var dPos = doctor.transform.position;
            RenderAt(dPos + doctor.transform.forward * 1.6f + doctor.transform.right * 1.2f + Vector3.up * 1.2f, dPos + Vector3.up * 1.1f, 50f,
                Path.Combine(outDir, "doctor_wave_side.png"));

            var stand = anchors.Find("SamWaitPoint");
            sam.PlaceAt(stand);
            sam.Rig.Wave(10f);
            Advance(sam.Rig, 0.6f);
            Render("SamWaiting", outDir, "sam_wave");
            var kidGo = Resources.FindObjectsOfTypeAll<KidActor>().First(k => k.name == "PlayerKid" && k.gameObject.scene.IsValid());
            kidGo.gameObject.SetActive(true);
            var kid = kidGo;
            kid.Apply(Hindsight.Ophthalmology.Core.Avatar.AvatarLook.Default);
            kid.PlaceAt(anchors.Find("KidStandPoint"));
            Render("ChairApproach", outDir, "kid_by_chair");
            Render("FirstPersonTiltBack", outDir, "ceiling_sun");
            foreach (var n in new[] { "DoctorPortrait", "SamPortrait" })
            {
                var go = GameObject.Find(n);
                var hc = go.GetComponentsInChildren<Transform>(true).First(t => t.name == "HeadCenter");
                Debug.Log($"DEBUG {n} root={go.transform.position} headCenter={hc.position} parent={hc.parent.name} bounds={EditorWorldBounds(go)} cam={GameObject.Find(n + "Camera").transform.position}");
            }
            GameObject.Find("DoctorPortrait").GetComponent<BeanRig>().ApplyLook(Hindsight.Ophthalmology.Core.Avatar.AvatarLook.Doctor);
            var samPortrait = GameObject.Find("SamPortrait").GetComponent<BeanRig>();
            samPortrait.ApplyLook(Hindsight.Ophthalmology.Core.Avatar.AvatarLook.Sam);
            samPortrait.SetTalking(true);
            samPortrait.Voice = () => 0.6f;
            Advance(samPortrait, 0.3f);
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

        /// <summary>How far the doctor's nearer hand is from the bottle.</summary>
        static float Reach(DoctorActor doc, Transform bottle) =>
            Mathf.Min(Vector3.Distance(doc.Rig.HandL.position, bottle.position), Vector3.Distance(doc.Rig.HandR.position, bottle.position));

        static void Advance(BeanRig rig, float seconds)
        {
            for (var t = 0f; t < seconds; t += 1f / 60f) rig.Evaluate(1f / 60f);
        }

        static void Render(string shot, string dir, string file)
        {
            var cam = GameObject.Find(shot).transform;
            RenderAt(cam.position, cam.position + cam.forward, 50f, Path.Combine(dir, file + ".png"));
        }

        static void RenderAt(Vector3 from, Vector3 to, float fov, string file)
        {
            const int w = 1600, h = 1000; // the game's 16:10
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
