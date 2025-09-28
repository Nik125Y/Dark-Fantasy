using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); 
    }

    public void OpenSettings()
    {
        Debug.Log("Open Settings");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game exited");
    }
}
