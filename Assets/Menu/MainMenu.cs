using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] 
    private GameObject settingsPanel;

    public void PlayGame()
    {
        SceneManager.LoadScene("Level_1"); 
    }

    public void OpenSettings()
    {
        Debug.Log("Open Settings");
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        Debug.Log("Close Settings");
        settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game exited");
    }
}
