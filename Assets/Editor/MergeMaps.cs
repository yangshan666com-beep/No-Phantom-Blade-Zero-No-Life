using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public static class MergeMaps
{
    public static void Build()
    {
        string source = "Assets/Scenes/Main.unity";
        string target = "Assets/Scenes/MergedMap.unity";

        AssetDatabase.DeleteAsset(target);
        AssetDatabase.CopyAsset(source, target);

        Scene scene = EditorSceneManager.OpenScene(target, OpenSceneMode.Single);

        GameObject oldPlayer = GameObject.Find("player");

        if (oldPlayer != null)
        {
            oldPlayer.SetActive(false);
        }

        LevelConfig config = AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/Level/Level_Merged.asset");

        if (config == null)
        {
            config = ScriptableObject.CreateInstance<LevelConfig>();
            AssetDatabase.CreateAsset(config, "Assets/Level/Level_Merged.asset");
        }

        config.displayName = "杨杉地图 + 浅云系统";
        config.playerSpawn = new Vector2(24.99f, -2.5f);
        config.mapMin = new Vector2(-11f, -6f);
        config.mapMax = new Vector2(126f, 16f);
        config.playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        config.blockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GIBlock.prefab");
        config.backgroundPrefab = null;
        config.blocks = new LevelConfig.BlockSpec[0];
        EditorUtility.SetDirty(config);

        GameObject boot = new GameObject("浅云系统 Bootstrap");

        AudioSource audio = boot.AddComponent<AudioSource>();
        audio.playOnAwake = false;

        ChuShiHua init = boot.AddComponent<ChuShiHua>();
        init.GuanQia = 0;

        CreateLevel creator = boot.AddComponent<CreateLevel>();
        creator.levels = new List<LevelConfig> { config };
        creator.levelRootName = "[Level]";

        MusicPlayer music = boot.AddComponent<MusicPlayer>();
        music.Source = audio;
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Placeholder.wav");
        music.Playlists = clip != null ? new AudioClip[] { clip } : new AudioClip[0];
        music.menuPlaylist = music.Playlists;

        MusicBootstrap musicBootstrap = boot.AddComponent<MusicBootstrap>();
        musicBootstrap.music = music;
        musicBootstrap.playlist = music.Playlists;

        ZhuKongUiDemo ui = boot.AddComponent<ZhuKongUiDemo>();
        ui.config = config;

        ZhuKongCameraBinder binder = boot.AddComponent<ZhuKongCameraBinder>();
        binder.virtualCamera = FindVirtualCamera(scene);

        EditorSceneManager.SaveScene(scene);
        AddBuildScene(target);
        AssetDatabase.SaveAssets();

        Debug.Log("MERGED MAP DONE");
    }

    static MonoBehaviour FindVirtualCamera(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            MonoBehaviour[] all = roots[i].GetComponentsInChildren<MonoBehaviour>(true);

            for (int m = 0; m < all.Length; m++)
            {
                if (all[m] != null && all[m].GetType().Name == "CinemachineVirtualCamera")
                {
                    return all[m];
                }
            }
        }

        return null;
    }

    static void AddBuildScene(string path)
    {
        List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].path == path)
            {
                return;
            }
        }

        list.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = list.ToArray();
    }
}
