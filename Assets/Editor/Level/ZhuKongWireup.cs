using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public static class ZhuKongWireup
{
    public static void Build()
    {
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets", "Audio");

        AssetDatabase.Refresh();

        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Placeholder.wav");
        Sprite square = LoadSprite("Assets/ass/Square.png");
        Sprite circle = LoadSprite("Assets/ass/Circle.png");

        var demo = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Scene main = EditorSceneManager.OpenScene("Assets/Scenes/Main.unity", OpenSceneMode.Additive);

        GameObject playerSource = FindInScene(main, "player");
        GameObject blockSource = FindInScene(main, "GI 1");
        GameObject playerPrefab = null;
        GameObject blockPrefab = null;

        if (playerSource != null)
        {
            playerPrefab = PrefabUtility.SaveAsPrefabAsset(playerSource, "Assets/Prefabs/Player.prefab");

            GameObject playerContents = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Player.prefab");
            Player1 prefabPlayer = playerContents.GetComponentInChildren<Player1>(true);

            if (prefabPlayer != null)
            {
                prefabPlayer.gravityDirection = Vector2.down;
            }

            PrefabUtility.SaveAsPrefabAsset(playerContents, "Assets/Prefabs/Player.prefab");
            PrefabUtility.UnloadPrefabContents(playerContents);
        }

        if (blockSource != null)
        {
            blockPrefab = PrefabUtility.SaveAsPrefabAsset(blockSource, "Assets/Prefabs/GIBlock.prefab");
        }

        EditorSceneManager.CloseScene(main, true);

        GameObject bgTemp = new GameObject("BackgroundPrefab");
        SpriteRenderer bgRenderer = bgTemp.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = square;
        bgRenderer.color = new Color(0.10f, 0.13f, 0.19f, 1f);
        bgRenderer.sortingOrder = -1;
        GameObject bgPrefab = PrefabUtility.SaveAsPrefabAsset(bgTemp, "Assets/Prefabs/Background.prefab");
        Object.DestroyImmediate(bgTemp);

        LevelConfig config = AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/Level/Level_01.asset");

        if (config == null)
        {
            Debug.LogError("ZHUKONG: Level_01.asset 没找到");
            return;
        }

        config.displayName = "第一关（浅云）";
        config.playerSpawn = new Vector2(0f, -2.5f);
        config.mapMin = new Vector2(-8f, -5f);
        config.mapMax = new Vector2(8f, 5f);
        config.playerPrefab = playerPrefab;
        config.blockPrefab = blockPrefab;
        config.backgroundPrefab = bgPrefab;
        config.blocks = new LevelConfig.BlockSpec[]
        {
            MakeBlock(new Vector2(3f, -2.5f)),
            MakeBlock(new Vector2(-3f, -2.5f))
        };
        EditorUtility.SetDirty(config);

        GameObject camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        Camera cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.2f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.09f, 0.11f, 0.16f, 1f);
        camGO.transform.position = new Vector3(0f, 0f, -10f);
        camGO.AddComponent<AudioListener>();

        GameObject cursorGO = new GameObject("Cusor");
        SpriteRenderer cursorRenderer = cursorGO.AddComponent<SpriteRenderer>();
        cursorRenderer.sprite = circle;
        cursorRenderer.color = new Color(0.15f, 1f, 0f, 1f);
        cursorRenderer.sortingOrder = 10;
        cursorGO.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        cursorGO.AddComponent<GravityCursor>();

        GameObject geometry = new GameObject("Geometry");
        CreatePlatform(geometry.transform, "Ground", square, new Vector3(0f, -5.3f, 0f), new Vector3(18f, 0.6f, 1f));
        CreatePlatform(geometry.transform, "Left Wall", square, new Vector3(-8.8f, 0f, 0f), new Vector3(0.6f, 10f, 1f));
        CreatePlatform(geometry.transform, "Right Wall", square, new Vector3(8.8f, 0f, 0f), new Vector3(0.6f, 10f, 1f));
        CreatePlatform(geometry.transform, "Ledge", square, new Vector3(4f, -2f, 0f), new Vector3(3f, 0.4f, 1f));
        CreatePlatform(geometry.transform, "Ledge 2", square, new Vector3(-4f, 0f, 0f), new Vector3(3f, 0.4f, 1f));

        GameObject boot = new GameObject("Bootstrap");
        AudioSource audio = boot.AddComponent<AudioSource>();
        audio.playOnAwake = false;

        ChuShiHua init = boot.AddComponent<ChuShiHua>();
        init.GuanQia = 0;

        CreateLevel creator = boot.AddComponent<CreateLevel>();
        creator.levels = new List<LevelConfig> { config };
        creator.levelRootName = "[Level]";

        MusicPlayer music = boot.AddComponent<MusicPlayer>();
        music.Source = audio;
        music.Playlists = clip != null ? new AudioClip[] { clip } : new AudioClip[0];
        music.menuPlaylist = music.Playlists;

        MusicBootstrap musicBootstrap = boot.AddComponent<MusicBootstrap>();
        musicBootstrap.music = music;
        musicBootstrap.playlist = music.Playlists;

        EditorSceneManager.SaveScene(demo, "Assets/Scenes/ZhuKongDemo.unity");
        AddBuildScene("Assets/Scenes/ZhuKongDemo.unity");
        AssetDatabase.SaveAssets();

        Debug.Log("ZHUKONG WIREUP DONE");
    }

    static LevelConfig.BlockSpec MakeBlock(Vector2 position)
    {
        LevelConfig.BlockSpec spec = new LevelConfig.BlockSpec();
        spec.position = position;
        spec.gravityDirection = Vector2.down;
        spec.scale = 0.3f;
        return spec;
    }

    static GameObject FindInScene(Scene scene, string name)
    {
        GameObject[] roots = scene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] all = roots[i].GetComponentsInChildren<Transform>(true);

            for (int t = 0; t < all.Length; t++)
            {
                if (all[t].name == name)
                {
                    return all[t].gameObject;
                }
            }
        }

        return null;
    }

    static void CreatePlatform(Transform parent, string name, Sprite sprite, Vector3 position, Vector3 scale)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = position;
        go.transform.localScale = scale;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(0.13f, 0.25f, 0.26f, 1f);
        sr.sortingOrder = 1;
        go.AddComponent<BoxCollider2D>();
    }

    static Sprite LoadSprite(string path)
    {
        Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);

        for (int i = 0; i < all.Length; i++)
        {
            Sprite s = all[i] as Sprite;

            if (s != null)
            {
                return s;
            }
        }

        return null;
    }

    static void EnsureFolder(string parent, string name)
    {
        string path = parent + "/" + name;

        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
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
