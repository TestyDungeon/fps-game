using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject grapple;
    [SerializeField] private GameObject[] itemsPos;
    [SerializeField] private GameObject[] UI;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
    }

    public void GameOver()
    {
        Time.timeScale = 0;
        gameOverUI.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void EnableGrapple()
    {
        grapple.SetActive(true);
    }

    public void EnableItems()
    {
        foreach(GameObject item in itemsPos)
        {
            item.SetActive(true);  
        }
    }

    public void EnableUI()
    {
        foreach(GameObject item in UI)
        {
            item.SetActive(true);  
        }
    }
}
