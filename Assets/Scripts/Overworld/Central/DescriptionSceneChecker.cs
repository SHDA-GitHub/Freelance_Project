using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "DescriptionSceneChecker", menuName = "Systems/DescriptionSceneChecker")]
public class DescriptionSceneChecker : ScriptableObject
{
    public bool isSceneOverworld;
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
        if (scene.name == "Overworld")
        {
            isSceneOverworld = true;
        }
        else if (scene.name == "Battle Scene")
        {
            isSceneOverworld = false;
        }
    }
}