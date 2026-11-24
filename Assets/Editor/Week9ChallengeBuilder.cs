// Week 9 Animation Challenge helper (Editor only).
// 菜单: Tools > Week 9 Challenge
//   1. Build Keyframed Animations  -> 生成 5 个关键帧动画 + 各自的 Animator Controller 并挂到场景物体上
//   2. Build Karter Story Controller -> 生成 KarterStory.controller (Idle/Walk/Cheer/Dance) 并挂到 Karter 上
// 生成后可以在 Animation 窗口 (Ctrl+6) 里打开每个 clip 继续修改关键帧。
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Week9ChallengeBuilder
{
    const string Dir = "Assets/CreativeCore_Animation/Animations/Challenge";
    const string ImportedDir = "Assets/CreativeCore_Animation/Animations/Imported Animations";

    // ---------------------------------------------------------------- keyframed
    [MenuItem("Tools/Week 9 Challenge/1. Build Keyframed Animations")]
    static void BuildKeyframes()
    {
        EnsureDir();
        var log = new List<string>();

        // 1) 路灯亮起：给路灯加一个点光源，用关键帧让灯光闪几下再亮起来
        var lamp = Find("StreetLamp", log);
        if (lamp != null)
        {
            var light = lamp.GetComponentInChildren<Light>();
            if (light == null)
            {
                var go = new GameObject("LampLight");
                Undo.RegisterCreatedObjectUndo(go, "Create LampLight");
                go.transform.SetParent(lamp.transform, false);
                var b = GetBounds(lamp);
                go.transform.position = new Vector3(b.center.x, b.max.y - 0.3f, b.center.z);
                light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 8f;
                light.color = new Color(1f, 0.85f, 0.55f);
                light.intensity = 0f;
            }
            string path = AnimationUtility.CalculateTransformPath(light.transform, lamp.transform);
            var clip = new AnimationClip { name = "Lamp_TurnOn" };
            SetCurve(clip, path, typeof(Light), "m_Intensity",
                0f, 0f, 0.2f, 4f, 0.35f, 0.3f, 0.6f, 5f, 0.8f, 0.5f, 1.2f, 6f, 2f, 6f);
            Attach(lamp, clip, "StreetLamp_Controller", false, log);
        }

        // 2) 苹果上下跳
        var apple = Find("Apple", log);
        if (apple != null)
        {
            var p = apple.transform.localPosition;
            var clip = new AnimationClip { name = "Apple_Bounce" };
            SetConst(clip, "m_LocalPosition.x", p.x, 1f);
            SetCurve(clip, "", typeof(Transform), "m_LocalPosition.y", 0f, p.y, 0.4f, p.y + 0.6f, 0.8f, p.y);
            SetConst(clip, "m_LocalPosition.z", p.z, 1f);
            Attach(apple, clip, "Apple_Controller", true, log);
        }

        // 3) 香蕉原地旋转
        var banana = Find("Banana", log);
        if (banana != null)
        {
            var e = banana.transform.localEulerAngles;
            var clip = new AnimationClip { name = "Banana_Spin" };
            SetConst(clip, "localEulerAnglesRaw.x", e.x, 2f);
            var spin = AnimationCurve.Linear(0f, e.y, 2f, e.y + 360f);
            clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.y", spin);
            SetConst(clip, "localEulerAnglesRaw.z", e.z, 2f);
            Attach(banana, clip, "Banana_Controller", true, log);
        }

        // 4) 柠檬跳起 + 压扁拉长
        var lemon = Find("Lemon", log);
        if (lemon != null)
        {
            var p = lemon.transform.localPosition;
            var s = lemon.transform.localScale;
            var clip = new AnimationClip { name = "Lemon_Jump" };
            SetConst(clip, "m_LocalPosition.x", p.x, 1f);
            SetCurve(clip, "", typeof(Transform), "m_LocalPosition.y", 0f, p.y, 0.15f, p.y, 0.5f, p.y + 0.8f, 0.85f, p.y, 1f, p.y);
            SetConst(clip, "m_LocalPosition.z", p.z, 1f);
            SetCurve(clip, "", typeof(Transform), "m_LocalScale.x", 0f, s.x, 0.15f, s.x * 1.25f, 0.5f, s.x * 0.85f, 0.85f, s.x * 1.2f, 1f, s.x);
            SetCurve(clip, "", typeof(Transform), "m_LocalScale.y", 0f, s.y, 0.15f, s.y * 0.75f, 0.5f, s.y * 1.25f, 0.85f, s.y * 0.8f, 1f, s.y);
            SetCurve(clip, "", typeof(Transform), "m_LocalScale.z", 0f, s.z, 0.15f, s.z * 1.25f, 0.5f, s.z * 0.85f, 0.85f, s.z * 1.2f, 1f, s.z);
            Attach(lemon, clip, "Lemon_Controller", true, log);
        }

        // 5) 摄像机慢慢往后拉远
        var cam = Find("Camera", log);
        if (cam == null && Camera.main != null) cam = Camera.main.gameObject;
        if (cam != null)
        {
            var t = cam.transform;
            var p0 = t.localPosition;
            var p1 = p0 - (t.parent ? t.parent.InverseTransformDirection(t.forward) : t.forward) * 2f; // 慢慢往后拉，露出整个派对
            var clip = new AnimationClip { name = "Camera_PullBack" };
            SetCurve(clip, "", typeof(Transform), "m_LocalPosition.x", 0f, p0.x, 2f, p0.x, 10f, p1.x);
            SetCurve(clip, "", typeof(Transform), "m_LocalPosition.y", 0f, p0.y, 2f, p0.y, 10f, p1.y);
            SetCurve(clip, "", typeof(Transform), "m_LocalPosition.z", 0f, p0.z, 2f, p0.z, 10f, p1.z);
            Attach(cam, clip, "Camera_Controller", false, log);
        }

        SaveScene();
        Report("关键帧动画", log);
    }

    // ---------------------------------------------------------------- layout
    // 按摄像机的视角把苹果和路灯摆进画面（放在 Karter 脚下那个高度的地面上），然后重新生成关键帧动画
    [MenuItem("Tools/Week 9 Challenge/0. Move Apple & Lamp Into View")]
    static void PlaceInView()
    {
        var log = new List<string>();
        var camGo = Find("Camera", log);
        var cam = camGo ? camGo.GetComponent<Camera>() : Camera.main;
        var karter = Find("Karter", log);
        if (cam == null || karter == null) { Report("摆放位置", log); return; }

        float groundY = karter.transform.position.y;
        var ground = new Plane(Vector3.up, new Vector3(0, groundY, 0));

        PlaceAt(Find("Apple", log), cam, ground, groundY, new Vector2(0.62f, 0.32f), log);
        PlaceLampBesideStage(Find("StreetLamp", log), Find("Environment", log), karter, cam, log);

        Debug.Log("[Week9] 摆放位置\n" + string.Join("\n", log));
        BuildKeyframes();   // 用新位置重新生成 5 个动画（会自动保存场景）
    }

    // 把路灯放在 Karter 所站台子的左边（镜头的左侧）那块更低的地面上
    static void PlaceLampBesideStage(GameObject lamp, GameObject env, GameObject karter, Camera cam, List<string> log)
    {
        if (lamp == null || env == null) return;
        var rs = env.GetComponentsInChildren<Renderer>();
        float stageY = karter.transform.position.y;
        System.Func<Vector3, float> topAt = q =>
        {
            float top = float.NegativeInfinity;
            foreach (var r in rs)
            {
                var b = r.bounds;
                if (q.x >= b.min.x && q.x <= b.max.x && q.z >= b.min.z && q.z <= b.max.z)
                    top = Mathf.Max(top, b.max.y);
            }
            return top;
        };
        var left = Vector3.ProjectOnPlane(-cam.transform.right, Vector3.up).normalized;
        var toCam = Vector3.ProjectOnPlane(cam.transform.position - karter.transform.position, Vector3.up).normalized;
        Vector3 basePos = Vector3.zero; bool found = false;
        for (float d = 1f; d <= 15f && !found; d += 0.25f)
        {
            var q = karter.transform.position + left * d + toCam * 0.5f;
            float y = topAt(q);
            if (!float.IsNegativeInfinity(y) && y < stageY - 0.3f)
            {
                q += left * 0.8f;                       // 离台子边缘再远一点
                float y2 = topAt(q);
                if (!float.IsNegativeInfinity(y2) && y2 < stageY - 0.3f) y = y2;
                basePos = new Vector3(q.x, y, q.z); found = true;
            }
        }
        if (!found) { log.Add("StreetLamp: 没找到台子旁边的地面，没有移动"); return; }

        Undo.RecordObject(lamp.transform, "Place lamp");
        var src = PrefabUtility.GetCorrespondingObjectFromSource(lamp);
        if (src != null) lamp.transform.localScale = src.transform.localScale;
        lamp.transform.position = new Vector3(basePos.x, lamp.transform.position.y, basePos.z);
        var bb = GetBounds(lamp);
        lamp.transform.position += Vector3.up * (basePos.y - bb.min.y);
        log.Add($"StreetLamp: 放在台子左边的地面上 {lamp.transform.position}");
    }

    static void PlaceAt(GameObject go, Camera cam, Plane ground, float groundY, Vector2 viewport, List<string> log, bool onRealGround = false)
    {
        if (go == null) return;
        var ray = cam.ViewportPointToRay(new Vector3(viewport.x, viewport.y, 0));
        if (!ground.Raycast(ray, out float d)) { log.Add(go.name + ": 摄像机看不到地面，没有移动"); return; }
        var hit = ray.GetPoint(d);
        Undo.RecordObject(go.transform, "Move into view");
        if (onRealGround)
        {
            // 用射线打到场景里真正的地面（台子下面的地面），而不是 Karter 站的台子
            var src = PrefabUtility.GetCorrespondingObjectFromSource(go);
            if (src != null) go.transform.localScale = src.transform.localScale;
            Physics.SyncTransforms();
            var hits = Physics.RaycastAll(ray, 500f).OrderBy(h => h.distance);
            foreach (var h in hits)
            {
                if (h.transform.IsChildOf(go.transform)) continue;
                if (h.normal.y < 0.7f) continue;            // 只要朝上的平面
                if (h.point.y > groundY - 0.3f) continue;   // 要比台面低，才是真正的地面
                hit = h.point; groundY = h.point.y; break;
            }
        }
        var p = go.transform.position;
        go.transform.position = new Vector3(hit.x, p.y, hit.z);
        // 让物体底部正好落在地面上
        var b = GetBounds(go);
        go.transform.position += Vector3.up * (groundY - b.min.y);
        // 路灯太高时按比例缩小，直到灯头能完整出现在画面里
        if (go.name == "StreetLamp")
        {
            Undo.RecordObject(go.transform, "Fit lamp");
            for (int i = 0; i < 30; i++)
            {
                b = GetBounds(go);
                float top = cam.WorldToViewportPoint(new Vector3(b.center.x, b.max.y, b.center.z)).y;
                if (top <= 0.88f) break;
                go.transform.localScale *= 0.92f;
                b = GetBounds(go);
                go.transform.position += Vector3.up * (groundY - b.min.y);
            }
        }
        log.Add($"{go.name}: 移到 {go.transform.position}");
    }

    // ---------------------------------------------------------------- Karter
    [MenuItem("Tools/Week 9 Challenge/2. Build Karter Story Controller")]
    static void BuildKarter()
    {
        EnsureDir();
        var log = new List<string>();

        var idle = LoadFirstClip("Assets/Animations/HumanoidIdle.fbx");
        var walk = FindImported(new[] { "walk" }, ImportedDir + "/HumanoidWalk.fbx", true, log, "Walk");
        var cheer = FindImported(new[] { "cheer", "victory", "clap", "celebrat" }, ImportedDir + "/HumanoidIdleWin.fbx", false, log, "Cheer");
        var dance = FindImported(new[] { "danc", "hip hop", "hiphop", "samba", "salsa" }, ImportedDir + "/HumanoidJumpAndFall.fbx", true, log, "Dance");

        string path = Dir + "/KarterStory.controller";
        AssetDatabase.DeleteAsset(path);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        foreach (var n in new[] { "Walk", "Cheer", "Dance" })
            ctrl.AddParameter(n, AnimatorControllerParameterType.Trigger);

        var sm = ctrl.layers[0].stateMachine;
        var sIdle = sm.AddState("Idle", new Vector3(250, 0));
        var sWalk = sm.AddState("Walk", new Vector3(250, 80));
        var sCheer = sm.AddState("Cheer", new Vector3(250, 160));
        var sDance = sm.AddState("Dance", new Vector3(250, 240));
        sIdle.motion = idle; sWalk.motion = walk; sCheer.motion = cheer; sDance.motion = dance;
        sm.defaultState = sIdle;
        Link(sIdle, sWalk, "Walk");
        Link(sWalk, sCheer, "Cheer");
        Link(sCheer, sDance, "Dance");

        var karter = Find("Karter", log);
        if (karter != null)
        {
            var anim = karter.GetComponent<Animator>();
            if (anim == null) anim = Undo.AddComponent<Animator>(karter);
            Undo.RecordObject(anim, "Assign KarterStory");
            anim.runtimeAnimatorController = ctrl;
            anim.applyRootMotion = false; // 原地播放，避免 Karter 走出镜头
            var sd = karter.GetComponent<StoryDirector>();
            if (sd == null) sd = Undo.AddComponent<StoryDirector>(karter);
            Undo.RecordObject(sd, "Story timing");
            // 走路只走 1.5 秒，避免 Karter 走到镜头前面挡住画面
            sd.walkAt = 3f; sd.cheerAt = 4.5f; sd.danceAt = 8f;
            log.Add("Karter: Animator Controller = KarterStory, 已添加 StoryDirector");
        }

        AssetDatabase.SaveAssets();
        SaveScene();
        Report("Karter 导入动画", log);
    }

    // ---------------------------------------------------------------- helpers
    static void EnsureDir()
    {
        if (!AssetDatabase.IsValidFolder(Dir))
            AssetDatabase.CreateFolder("Assets/CreativeCore_Animation/Animations", "Challenge");
    }

    static GameObject Find(string name, List<string> log)
    {
        var scene = EditorSceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == name) return root;
            var t = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == name);
            if (t != null) return t.gameObject;
        }
        log.Add("找不到物体: " + name + "（请先打开 ChallengeScene_Animation）");
        return null;
    }

    static Bounds GetBounds(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return new Bounds(go.transform.position + Vector3.up * 3f, Vector3.one);
        var b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }

    // times/values interleaved: t0, v0, t1, v1, ...
    static void SetCurve(AnimationClip clip, string path, System.Type type, string prop, params float[] tv)
    {
        var keys = new Keyframe[tv.Length / 2];
        for (int i = 0; i < keys.Length; i++) keys[i] = new Keyframe(tv[i * 2], tv[i * 2 + 1]);
        var curve = new AnimationCurve(keys);
        for (int i = 0; i < keys.Length; i++) AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
        for (int i = 0; i < keys.Length; i++) AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
        clip.SetCurve(path, type, prop, curve);
    }

    static void SetConst(AnimationClip clip, string prop, float v, float len)
    {
        clip.SetCurve("", typeof(Transform), prop, AnimationCurve.Constant(0f, len, v));
    }

    static void Attach(GameObject go, AnimationClip clip, string ctrlName, bool loop, List<string> log)
    {
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        string clipPath = $"{Dir}/{clip.name}.anim";
        AssetDatabase.DeleteAsset(clipPath);
        AssetDatabase.CreateAsset(clip, clipPath);

        string ctrlPath = $"{Dir}/{ctrlName}.controller";
        AssetDatabase.DeleteAsset(ctrlPath);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPathWithClip(ctrlPath, clip);

        var anim = go.GetComponent<Animator>();
        if (anim == null) anim = Undo.AddComponent<Animator>(go);
        Undo.RecordObject(anim, "Assign controller");
        anim.runtimeAnimatorController = ctrl;
        anim.applyRootMotion = false;
        log.Add($"{go.name}: {clip.name}.anim + {ctrlName}");
    }

    static void Link(AnimatorState from, AnimatorState to, string trigger)
    {
        var tr = from.AddTransition(to);
        tr.hasExitTime = false;
        tr.duration = 0.25f;
        tr.AddCondition(AnimatorConditionMode.If, 0, trigger);
    }

    static AnimationClip LoadFirstClip(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
            .FirstOrDefault(c => !c.name.StartsWith("__preview__"));
    }

    // 优先使用你自己下载的动画（Assets 里文件名含关键字、且不在 CreativeCore 包里的 FBX），
    // 找不到再用包里的备用动画。
    static AnimationClip FindImported(string[] keywords, string fallback, bool loop, List<string> log, string label)
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets" }))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (p.StartsWith("Assets/CreativeCore_")) continue;
            string n = Path.GetFileNameWithoutExtension(p).ToLowerInvariant();
            if (!keywords.Any(k => n.Contains(k))) continue;

            var imp = AssetImporter.GetAtPath(p) as ModelImporter;
            if (imp != null)
            {
                bool dirty = false;
                if (imp.animationType != ModelImporterAnimationType.Human)
                {
                    imp.animationType = ModelImporterAnimationType.Human;
                    imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    dirty = true;
                }
                var clips = imp.clipAnimations.Length > 0 ? imp.clipAnimations : imp.defaultClipAnimations;
                foreach (var c in clips) if (c.loopTime != loop) { c.loopTime = loop; dirty = true; }
                if (dirty) { imp.clipAnimations = clips; imp.SaveAndReimport(); }
            }
            var clip = LoadFirstClip(p);
            if (clip != null) { log.Add($"{label}: {p}（你导入的动画）"); return clip; }
        }
        var fb = LoadFirstClip(fallback);
        log.Add($"{label}: 没找到你下载的动画，暂时用包里的 {Path.GetFileName(fallback)}");
        return fb;
    }

    static void SaveScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    static void Report(string title, List<string> log)
    {
        string msg = string.Join("\n", log);
        Debug.Log($"[Week9] {title}\n{msg}");
        EditorUtility.DisplayDialog("Week 9: " + title, msg, "OK");
    }
}
