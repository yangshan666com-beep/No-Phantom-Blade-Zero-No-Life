using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicPlayer : MonoBehaviour
{
  
    [Header("引用音乐模块")]
    public AudioSource Source;

    [Header("分区歌单")]
    public AudioClip[] menuPlaylist;                 // 主菜单音乐
    public AudioClip[] Playlists ;                   // 歌单
    public AudioClip[] easterEggPlaylist;            // 彩蛋歌单
    
    [Header("全局总音量")]
    [Range(0f, 1f)] public float bgmVolume = 0.6f;
    
    public AudioClip[] currentPlaylist;
    public static MusicPlayer Instance { get; private set; }

    bool paused = true;
    int currentIndex = 0;

     // ===== 关键：不依赖任何场景，启动后自动创建 =====
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
      static void Boot()
    {
        if (Instance != null) return;                 // 只创建一次
        GameObject go = new GameObject("[MusicPlayer]");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<MusicPlayer>();               // 其余全交给 Awake
    }
     void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);                      // 已有实例 → 销毁这个
            return;
        }
        Instance = this;

        if (Source == null) Source = GetComponent<AudioSource>();
        if (Source == null) Source = gameObject.AddComponent<AudioSource>();

        Source.playOnAwake = false;
        Source.loop = false;
        Source.spatialBlend = 0f;
        Source.volume = bgmVolume;

        // Inspector 没配歌单 → 从 Resources 兜底
        if (menuPlaylist == null || menuPlaylist.Length == 0)
            menuPlaylist = Resources.LoadAll<AudioClip>("Music/Menu");
        if (Playlists == null || Playlists.Length == 0)
            Playlists = Resources.LoadAll<AudioClip>("Music/Game");

        // 订阅两个切歌时机
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameSignals.LevelReady += OnLevelReady;

        // Boot 在 AfterSceneLoad 跑，第一个场景已经加载完 → 手动补一次
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    public void SwitchTo(AudioClip[] clips)           // 切换歌单
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning("[MusicPlayer] 歌单是空的，没有音乐可播");
            return;
        }
        if (clips == currentPlaylist && Source.isPlaying) return;   // 同一套就别重播

        currentPlaylist = clips;
        currentIndex = 0;
        paused = false;
        Play();
    }

    // ===== 两个切歌时机（新增）=====

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "UIstart")                  // 进主菜单 → 菜单歌单
        {
            SwitchTo(menuPlaylist);
        }
    }

    void OnLevelReady(int index)
    {
        SwitchTo(Playlists);                          // 关卡生成完 → 游戏歌单
    }

    void OnDestroy()                                  // 新增
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameSignals.LevelReady -= OnLevelReady;
        if (Instance == this) Instance = null;
    }
    void Update()                               //判断暂停或者下一首
    {
        if (paused || currentPlaylist == null || currentPlaylist.Length == 0) return;
        if (!Source.isPlaying) 
        {
            currentIndex = (currentIndex + 1) % currentPlaylist.Length;
            Play();
        }
    }

    void Play()
    {
        Source.Stop();
        Source.clip = currentPlaylist[currentIndex];
        Source.Play(); 
    }
    public void Pause()                        //暂停
    {
        if (paused) return;
        paused = true;
        Source.Pause();
    }

    public void Resume()                       //继续播放
    { 
        if (!paused) return;
        paused = false;
        Source.UnPause();
    }
}
