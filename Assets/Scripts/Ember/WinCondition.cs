using UnityEngine;
using UnityEngine.SceneManagement;

public class WinCondition : MonoBehaviour
{
    public GameObject winText;

    private void Start()
    {
        if (winText != null) winText.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (winText != null) winText.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}