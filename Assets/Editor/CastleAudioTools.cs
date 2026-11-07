using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class CastleAudioImporter : AssetPostprocessor
{
    void OnPreprocessAudio()
    {
        if (!assetPath.StartsWith("Assets/Resources/CastleAudio/")) return;
        AudioImporter importer = (AudioImporter)assetImporter;
        AudioImporterSampleSettings settings = importer.defaultSampleSettings;
        bool music = assetPath.Contains("/BGM/");
        settings.loadType = music ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat = music ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.PCM;
        settings.quality = music ? 0.85f : 1f;
        settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
        settings.preloadAudioData = !music;
        importer.defaultSampleSettings = settings;
        importer.forceToMono = !music;
    }
}

[InitializeOnLoad]
public static class CastleAudioTools
{
    const string SettingsPath = "Assets/Resources/CastleAudio/Settings.asset";
    static CastleAudioTools()
    {
        EditorApplication.delayCall += ValidateWhenRequested;
        EditorApplication.update += ValidateWhenRequested;
        EditorApplication.update += RecordRuntimeWhenRequested;
    }

    static void RecordRuntimeWhenRequested()
    {
        const string request = "Library/CastleAudioRuntimeValidation.request";
        if (!File.Exists(request) || !EditorApplication.isPlaying || Time.timeSinceLevelLoad < 1f) return;
        File.Delete(request);
        RecordRuntimeStatus();
    }

    [MenuItem("Tools/城堡音频/检查运行音频状态")]
    public static void RecordRuntimeStatus()
    {
        if (!EditorApplication.isPlaying) { Debug.Log("请先进入 Play 模式。"); return; }
        CastleAudioRuntime runtime = CastleAudioRuntime.Instance;
        Player1[] players = Object.FindObjectsOfType<Player1>();
        bool attached = players.Length > 0;
        foreach (Player1 player in players) attached &= player.GetComponent<CastlePlayerAudio>() != null;
        bool music = false;
        foreach (AudioSource source in Object.FindObjectsOfType<AudioSource>())
            if (source.clip != null && source.clip.name == "MysteriousCastle_PianoStrings_Loop")
                music |= source.isPlaying && source.loop;
        bool listener = false;
        foreach (AudioListener item in Object.FindObjectsOfType<AudioListener>()) listener |= item.isActiveAndEnabled;
        string report = "{\"runtime_ready\":" + (runtime != null ? "true" : "false")
            + ",\"players_have_audio\":" + (attached ? "true" : "false")
            + ",\"bgm_playing_and_looping\":" + (music ? "true" : "false")
            + ",\"listener_active\":" + (listener ? "true" : "false")
            + ",\"listener_paused\":" + (AudioListener.pause ? "true" : "false") + "}";
        File.WriteAllText("Library/CastleAudioRuntimeValidation.json", report);
        Debug.Log("[CastleAudio] Runtime: " + report);
    }

    [MenuItem("Tools/城堡音频/音量与步幅设置")]
    static void SelectSettings() { Selection.activeObject = AssetDatabase.LoadAssetAtPath<CastleAudioSettings>(SettingsPath); }

    [MenuItem("Tools/城堡音频/检查音频资源")]
    public static void Validate()
    {
        int count = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Resources/CastleAudio" }))
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));
            if (clip == null || clip.length <= 0f) throw new System.Exception("Invalid audio: " + guid);
            count++;
        }
        if (count != 22) throw new System.Exception("Expected 22 audio clips, got " + count);
        if (AssetDatabase.LoadAssetAtPath<CastleAudioSettings>(SettingsPath) == null) throw new System.Exception("Missing settings");
        AudioClip bgm = Resources.Load<AudioClip>("CastleAudio/BGM/MysteriousCastle_PianoStrings_Loop");
        if (bgm == null || Mathf.Abs(bgm.length - 64f) > .01f) throw new System.Exception("Invalid BGM length");
        File.WriteAllText("Library/CastleAudioValidation.json", "{\"status\":\"passed\",\"audio_clips\":" + count + ",\"music_seconds\":64,\"style\":\"A\"}");
        Debug.Log("[CastleAudio] Validation passed: " + count + " audio clips, realistic Foley + mechanical motion + 64-second piano/strings loop.");
    }

    static void ValidateWhenRequested()
    {
        const string request = "Library/CastleAudioValidation.request";
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        File.Delete(request);
        try { Validate(); }
        catch (System.Exception e)
        {
            File.WriteAllText("Library/CastleAudioValidation.json", JsonUtility.ToJson(new ValidationError { error = e.Message }));
            Debug.LogException(e);
        }
    }

    [System.Serializable] class ValidationError { public string error; }
}
