using UnityEngine;
using UnityEngine.SceneManagement;

public class StatsResetter : MonoBehaviour
{
    public PlayerStats playerStats;
    public PlayerCardCollection playerCardCollection;

    private static StatsResetter _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Title Screen")
        {
            playerStats.ResetStats();
            playerCardCollection.ResetCards();
        }
    }
}