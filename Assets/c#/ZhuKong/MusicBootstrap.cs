using UnityEngine;

public class MusicBootstrap : MonoBehaviour
{
    public MusicPlayer music;
    public AudioClip[] playlist;

    bool started;

    void OnEnable()
    {
        GameSignals.LevelReady += OnLevelReady;
    }

    void OnDisable()
    {
        GameSignals.LevelReady -= OnLevelReady;
    }

    void OnLevelReady(int index)
    {
        if (started || music == null)
        {
            return;
        }

        started = true;
        music.Playlists = playlist;
        music.ToPlay(playlist);
        Debug.Log("MUSIC STARTED: clips=" + (playlist != null ? playlist.Length : 0));
    }
}
