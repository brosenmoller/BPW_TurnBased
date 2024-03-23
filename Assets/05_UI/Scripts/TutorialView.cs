using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialView : UIView
{
    [SerializeField] private Button nextButton;
    [SerializeField] private RectTransform screenParent;

    private List<Transform> tutorialScreens;
    private int tutorialScreenIndex = -1;

    private void Awake()
    {
        nextButton.onClick.AddListener(Next);

        tutorialScreens = new List<Transform>();
        for (int i = 0; i < screenParent.childCount; i++) 
        {
            Transform child = screenParent.GetChild(i);
            child.gameObject.SetActive(false);
            tutorialScreens.Add(child);
        }
    }

    private void Start()
    {
        Next();
    }

    private void Next()
    {
        tutorialScreenIndex++;

        if (tutorialScreenIndex >= tutorialScreens.Count)
        {
            GameManager.UIViewManager.Show(typeof(TutorialEndView));
            return;
        }

        for (int i = 0;i < tutorialScreens.Count; i++)
        {
            if (i == tutorialScreenIndex)
            {
                tutorialScreens[i].gameObject.SetActive(true);
            }
            else
            {
                tutorialScreens[i].gameObject.SetActive(false);
            }
        }
    }
}
