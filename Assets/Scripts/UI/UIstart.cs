using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 开屏菜单：挂在菜单场景里的空物体（MenuManager）上。
/// 按钮的 OnClick 分别绑 StartGame / QuitGame。
/// </summary>
public class UIstart : MonoBehaviour
{
    [Tooltip("点击开始后要加载的游戏场景名，必须出现在 Build Settings 里")]
    [SerializeField] private string gameSceneName = "Main";

    // 绑到“开始游戏”按钮的 OnClick
    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // 绑到“退出”按钮的 OnClick
    public void QuitGame()
    {
        Debug.Log("[UIstart] QuitGame 被调用了");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();   // 编辑器：退出 Play 模式
#else
        Application.Quit();                             // 打包后：真正退出程序
#endif
    }

    // 诊断用：Play 模式下按 Esc 等同于点退出按钮
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            QuitGame();
    }
}
