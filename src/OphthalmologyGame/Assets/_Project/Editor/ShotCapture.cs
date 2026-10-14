using System;
using System.IO;
using System.Linq;
using Hindsight.Ophthalmology.SceneObjects;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Hindsight.Ophthalmology.EditorTools
{
    /// <summary>
    /// Renders every camera shot to PNGs in ./Captures so 3D layout can be reviewed without entering
    /// Play mode. UI is hidden: review UI with the autopilot build instead (edit-mode TMP renders are
    /// not representative). Scenes are opened fresh and never saved.
    /// </summary>
    public static class ShotCapture
    {
        const int Width = 1600;
        const int Height = 1000;

        [MenuItem("POC/Capture Shots")]
        public static void CaptureAll()
        {
            var outDir = Path.GetFullPath("Captures");
            Directory.CreateDirectory(outDir);
            ShaderUtil.allowAsyncCompilation = false; // otherwise first renders use placeholder shaders

            EditorSceneManager.OpenScene(PocBuilder.EyeDropsScenePath);
            var doctor = UnityEngine.Object.FindAnyObjectByType<DoctorActor>().transform;
            var anchors = GameObject.Find("Anchors").transform;
            var canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            var scope = GameObject.Find("Ophthalmoscope");
            var chairHighlight = GameObject.Find("ExamChair").transform.Find("Highlight").gameObject;
            var bottle = GameObject.Find("EyeDropBottle").transform;
            var hold = GameObject.Find("BottleHold").transform;
            var rest = GameObject.Find("BottleRest").transform;
            var drop = GameObject.Find("Drop").transform;
            var dropTarget = GameObject.Find("DropTarget").transform;
            var pupil = UnityEngine.Object.FindAnyObjectByType<EyeCloseupRig>().transform.GetComponentsInChildren<Transform>().First(t => t.name == "Pupil");

            PoseDoctor(doctor.gameObject);

            canvas.gameObject.SetActive(false);

            void Shot(string file, string shot, string doctorAnchor)
            {
                var a = anchors.Find(doctorAnchor);
                doctor.SetPositionAndRotation(a.position, a.rotation);
                scope.SetActive(shot == "ExamCloseFP");
                chairHighlight.SetActive(shot == "ChairApproach");
                var tilt = shot == "FirstPersonTiltBack";
                bottle.SetPositionAndRotation(tilt ? hold.position : rest.position, tilt ? hold.rotation : rest.rotation);
                drop.gameObject.SetActive(tilt);
                drop.position = Vector3.Lerp(bottle.Find("DropStart").position, dropTarget.position, 0.4f);
                var cam = GameObject.Find(shot).GetComponent<CinemachineCamera>();
                Render(cam.transform, cam.Lens.FieldOfView, Path.Combine(outDir, file + ".png"));
            }

            Shot("01_RoomWide", "RoomWide", "DoctorStandPoint");
            Shot("02_DoctorGreeting", "DoctorGreeting", "DoctorStandPoint");
            Shot("03_DoctorExplain", "DoctorExplain", "DoctorStandPoint");
            doctor.GetComponent<DoctorActor>().ApplyWave(1f, 0.5f);
            Shot("03b_DoctorWave", "DoctorGreeting", "DoctorStandPoint");
            PoseDoctor(doctor.gameObject);
            pupil.localScale = new Vector3(0.3f, 1f, 0.3f);
            Shot("04_EyeCloseup_Before", "EyeCloseup", "DoctorStandPoint");
            pupil.localScale = new Vector3(0.75f, 1f, 0.75f);
            Shot("05_EyeCloseup_After", "EyeCloseup", "DoctorStandPoint");
            Shot("06_ChairApproach", "ChairApproach", "DoctorStandPoint");
            Shot("07_FirstPerson", "FirstPerson", "DoctorExamPoint");
            Shot("08_TiltBack", "FirstPersonTiltBack", "DoctorDropPoint");
            Shot("09_ExamCloseFP", "ExamCloseFP", "DoctorExamPoint");
            Shot("10_Reward", "Reward", "DoctorStandPoint");

            EditorSceneManager.OpenScene(PocBuilder.MenuScenePath);
            UnityEngine.Object.FindAnyObjectByType<Canvas>().gameObject.SetActive(false);
            PoseDoctor(UnityEngine.Object.FindAnyObjectByType<DoctorActor>().gameObject);
            var mainCam = Camera.main;
            Render(mainCam.transform, mainCam.fieldOfView, Path.Combine(outDir, "00_Menu.png"));

            Debug.Log($"ShotCapture: wrote captures to {outDir}");
        }

        static void PoseDoctor(GameObject doctor)
        {
            var anim = doctor.GetComponentInChildren<Animation>();
            if (anim == null) return;
            foreach (AnimationState s in anim)
                if (s.name == "Idle" || s.name.EndsWith("|Idle")) s.clip.SampleAnimation(anim.gameObject, 0.4f);
        }






        static void Render(Transform pose, float fov, string file)
        {
            var go = new GameObject("CaptureCam");
            var cam = go.AddComponent<Camera>();
            go.transform.SetPositionAndRotation(pose.position, pose.rotation);
            cam.fieldOfView = fov;
            cam.nearClipPlane = 0.01f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.75f, 0.9f);
            var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;


            cam.Render();

            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            File.WriteAllBytes(file, tex.EncodeToPNG());

            cam.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
