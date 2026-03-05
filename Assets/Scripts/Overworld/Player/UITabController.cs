using UnityEngine;

public class UITabController : MonoBehaviour
{
    [SerializeField] private GameObject[] tabs;

    private int currentTab = 0;

    void Start()
    {
        ShowTab(0);
    }

    public void NextTab()
    {
        currentTab++;

        if (currentTab >= tabs.Length)
            currentTab = 0;

        ShowTab(currentTab);
    }

    public void PreviousTab()
    {
        currentTab--;

        if (currentTab < 0)
            currentTab = tabs.Length - 1;

        ShowTab(currentTab);
    }

    void ShowTab(int index)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetActive(i == index);
        }
    }

    public void ResetTabs()
    {
        currentTab = 0;
        ShowTab(0);
    }
}