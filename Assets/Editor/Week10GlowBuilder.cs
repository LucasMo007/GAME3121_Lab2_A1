// Week 10 - Emissive material + Post Processing (Bloom) for "Pincers in Paradise"
// Built-in Render Pipeline + Post Processing Stack v2 (com.unity.postprocessing).
// Menu: Tools > Week 10 Glow > Build Glow + Bloom (SampleScene)
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public static class Week10GlowBuilder
{
    const string ScenePath = "Assets/Scenes/SampleScene.unity";
    const string MatDir = "Assets/Materials/Theme";
    const string PPDir = "Assets/Settings/PostProcessing";
    const string PPLayerName = "PostProcessing";

    [MenuItem("Tools/Week 10 Glow/Build Glow + Bloom (SampleScene)")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Directory.CreateDirectory(MatDir);
        Directory.CreateDirectory(PPDir);
        var log = new List<string>();

        // ---------- 1. Theme materials (tropical beach) ----------
        // Environment is muted so the player, enemy and pickups pop.
        var pickupGlow = MakeMat("Pickup_Glow", new Color(1f, 0.78f, 0.15f), 0.6f, 0.2f,
                                 new Color(1f, 0.7f, 0.1f), 4f);            // HDR emission -> blooms
        var player     = MakeMat("Player_Crab", new Color(1f, 0.33f, 0.08f), 0.65f, 0f,
                                 new Color(1f, 0.25f, 0.05f), 0.35f);       // bright, slight self-light
        var enemyCube  = MakeMat("Enemy_Glow", new Color(0.35f, 0.05f, 0.45f), 0.5f, 0f,
                                 new Color(0.9f, 0.05f, 0.6f), 1.8f);       // menacing magenta glow
        var wall       = MakeMat("Wall_DriftwoodTeal", new Color(0.13f, 0.36f, 0.40f), 0.15f, 0f, Color.black, 0f);
        var obstacle   = MakeMat("Obstacle_CoralRock", new Color(0.62f, 0.42f, 0.36f), 0.1f, 0f, Color.black, 0f);
        var ground     = MakeMat("Ground_Sand", new Color(0.90f, 0.80f, 0.60f), 0.05f, 0f, Color.black, 0f);

        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string n = r.gameObject.name;
            if (!(r is MeshRenderer)) continue;
            Material m = null;
            if (n.StartsWith("Pickup")) m = pickupGlow;
            else if (n == "Player") m = player;
            else if (n == "AI") m = enemyCube;
            else if (n.StartsWith("Wall")) m = wall;
            else if (n.StartsWith("Obstacle")) m = obstacle;
            else if (n == "Plane") m = ground;
            if (m == null) continue;
            Undo.RecordObject(r, "Week10 material");
            var arr = r.sharedMaterials;
            for (int i = 0; i < arr.Length; i++) arr[i] = m;
            r.sharedMaterials = arr;
            log.Add(n + " -> " + m.name);
        }

        // Enemy character model (Karter under "AI"): keep its textures, add a red/magenta glow tint.
        var ai = GameObject.Find("AI");
        if (ai != null)
        {
            foreach (var smr in ai.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var mats = smr.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null) continue;
                    string baseName = mats[i].name.Replace("Enemy_", "");
                    string path = MatDir + "/Enemy_" + baseName + ".mat";
                    var copy = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (copy == null)
                    {
                        copy = new Material(mats[i]) { name = "Enemy_" + baseName };
                        if (copy.shader.name == "Standard")
                        {
                            if (copy.HasProperty("_Color"))
                                copy.SetColor("_Color", copy.GetColor("_Color") * new Color(1f, 0.6f, 0.8f));
                            copy.EnableKeyword("_EMISSION");
                            copy.SetColor("_EmissionColor", new Color(0.9f, 0.05f, 0.5f) * 0.8f);
                            copy.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                        }
                        AssetDatabase.CreateAsset(copy, path);
                    }
                    mats[i] = copy;
                }
                Undo.RecordObject(smr, "Week10 enemy tint");
                smr.sharedMaterials = mats;
                log.Add("Enemy model " + smr.name + " -> glow tint");
            }
        }

        // ---------- 2. Post-processing layer ----------
        int ppLayer = EnsureLayer(PPLayerName);

        // ---------- 3. Profile with Bloom (+ light vignette) ----------
        string profilePath = PPDir + "/Week10_PostProcessProfile.asset";
        var profile = AssetDatabase.LoadAssetAtPath<PostProcessProfile>(profilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<PostProcessProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
        }
        var bloom = GetOrAdd<Bloom>(profile);
        bloom.enabled.Override(true);
        bloom.intensity.Override(5f);
        bloom.threshold.Override(1f);   // only HDR (emissive) pixels bloom
        bloom.softKnee.Override(0.5f);
        bloom.diffusion.Override(7f);
        bloom.color.Override(Color.white);

        var vig = GetOrAdd<Vignette>(profile);
        vig.enabled.Override(true);
        vig.intensity.Override(0.3f);
        vig.smoothness.Override(0.4f);
        EditorUtility.SetDirty(profile);

        // ---------- 4. Global Post-process Volume ----------
        var volGo = GameObject.Find("PostProcess Volume");
        if (volGo == null) volGo = new GameObject("PostProcess Volume");
        volGo.layer = ppLayer;
        var vol = volGo.GetComponent<PostProcessVolume>();
        if (vol == null) vol = volGo.AddComponent<PostProcessVolume>();
        vol.isGlobal = true;
        vol.priority = 1;
        vol.weight = 1;
        vol.sharedProfile = profile;

        // ---------- 5. Post-process Layer on the camera ----------
        Camera cam = Camera.main;
        if (cam == null) { var g = GameObject.Find("Main Camera"); if (g) cam = g.GetComponent<Camera>(); }
        if (cam != null)
        {
            cam.allowHDR = true;
            var ppl = cam.GetComponent<PostProcessLayer>();
            if (ppl == null) ppl = cam.gameObject.AddComponent<PostProcessLayer>();
            var res = AssetDatabase.LoadAssetAtPath<PostProcessResources>(
                "Packages/com.unity.postprocessing/PostProcessing/PostProcessResources.asset");
            if (res == null)
            {
                var guids = AssetDatabase.FindAssets("t:PostProcessResources");
                if (guids.Length > 0) res = AssetDatabase.LoadAssetAtPath<PostProcessResources>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            if (res != null) ppl.Init(res);
            ppl.volumeTrigger = cam.transform;
            ppl.volumeLayer = 1 << ppLayer;
            ppl.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
            EditorUtility.SetDirty(ppl);
            log.Add("Camera: HDR on, Post-process Layer (layer " + PPLayerName + ")");
        }
        else log.Add("WARNING: no Main Camera found");

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Week10 Glow] Done:\n" + string.Join("\n", log));
        EditorUtility.DisplayDialog("Week 10 Glow", "Done! Press Play to see the glowing pickups.\n\n" + string.Join("\n", log), "OK");
    }

    static Material MakeMat(string name, Color albedo, float smooth, float metal, Color emission, float intensity)
    {
        string path = MatDir + "/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) { m = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(m, path); }
        m.SetColor("_Color", albedo);
        m.SetFloat("_Glossiness", smooth);
        m.SetFloat("_Metallic", metal);
        if (intensity > 0f)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission * intensity);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            m.DisableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", Color.black);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }
        EditorUtility.SetDirty(m);
        return m;
    }

    static T GetOrAdd<T>(PostProcessProfile p) where T : PostProcessEffectSettings
    {
        if (p.TryGetSettings<T>(out var s)) return s;
        s = p.AddSettings<T>();
        s.name = typeof(T).Name;
        s.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
        AssetDatabase.AddObjectToAsset(s, p);
        return s;
    }

    static int EnsureLayer(string layerName)
    {
        int existing = LayerMask.NameToLayer(layerName);
        if (existing >= 0) return existing;
        var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = tm.FindProperty("layers");
        for (int i = 8; i < layers.arraySize; i++)
        {
            var sp = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(sp.stringValue))
            {
                sp.stringValue = layerName;
                tm.ApplyModifiedProperties();
                return i;
            }
        }
        Debug.LogWarning("No free layer slot; using Default layer for the volume.");
        return 0;
    }
}
#endif
