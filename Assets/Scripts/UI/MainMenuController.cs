using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {

        SceneManager.LoadScene("Level_01");
    }
    public void InstructionGame()
    {

        SceneManager.LoadScene("Instruction");
    }
    public void QuitGame()
    {
        Debug.Log("Đã bấm thoát game!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}