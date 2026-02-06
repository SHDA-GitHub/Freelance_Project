using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] private List<GameObject> pages = new List<GameObject>();

    private int currentPage = 0;

    void Start()
    {
        if (pages.Count == 0)
        {
            Debug.LogError("No tutorial pages assigned!");
            return;
        }
        SetPage(0);
    }

    public void flipPageForward()
    {
        if (currentPage >= pages.Count - 1)
            return;
        currentPage++;
        SetPage(currentPage);
    }

    public void flipPageBackward()
    {
        if (currentPage <= 0)
            return;

        currentPage--;
        SetPage(currentPage);
    }

    private void SetPage(int pageIndex)
    {
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].SetActive(i == pageIndex);
        }
    }
}
