//Author: Nguyễn Tín Nghĩa
//Date: 11/06/2026
//Description: Quản lý menu chính, bao gồm các chức năng bắt đầu trò chơi, hướng dẫn và thoát game.

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