using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public void QuitGame()
    {
#if UNITY_EDITOR
        // 如果在 Unity 编辑器中测试，停止 Play 模式
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 在打包编译后的游戏中真正退出
        Application.Quit();
#endif
    }
}
