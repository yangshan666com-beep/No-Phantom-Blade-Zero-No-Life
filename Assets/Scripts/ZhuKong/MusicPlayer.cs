using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float fadeDuration = 0.8f;

    public AudioClip[] currentPlaylist;
    bool paused = true;
    int currentIndex = 0;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);          // 将播放器挂到不被删除场景
        Source.loop = false;                    // 非循环模式
        Source.playOnAwake = false;
        Source.spatialBlend = 0f;               // 音量大小不受远近影响
        Source.volume = bgmVolume;              // 设置音量
        ToPlay(menuPlaylist);                   // 默认播放菜单歌单
    }

    public void ToPlay(AudioClip[] change)      //播放
    {
        currentPlaylist = change;
        if (currentPlaylist == null || currentPlaylist.Length == 0)
        {
            Debug.LogWarning("[MusicPlayer] clips 是空的，没有音乐可播");
            return;
        }
        paused = false;
        currentIndex = 0;
        Play();
    }
    void Update()                               //判断暂停或者下一首
    {
        if (paused) return;
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
