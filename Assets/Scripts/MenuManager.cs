using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsPanel;
    public GameObject titlePanel; // 新增：把 TitlePanel 拖进来

    void Update()
    {
        // 监听玩家按下的 Esc 键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettings();
        }
    }

    /// <summary>
    /// 主菜单 PLAY 按钮调用：恢复时间流速
    /// </summary>
    public void StartGame()
    {
        Time.timeScale = 1f; // 恢复时间流速，游戏正常运行
    }

    /// <summary>
    /// 打开/关闭设置界面的切换方法 (Esc键)
    /// </summary>
    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            bool isShown = !settingsPanel.activeSelf;
            settingsPanel.SetActive(isShown);

            // 弹出设置时暂停游戏，关闭设置时恢复游戏
            Time.timeScale = isShown ? 0f : 1f;
        }
    }

    /// <summary>
    /// 设置界面右上角 X 按钮调用：回到主菜单并暂停游戏
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (titlePanel != null)
        {
            titlePanel.SetActive(true); // 唤出主菜单
        }

        Time.timeScale = 0f; // 处于主菜单时，游戏画面保持冻结
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}