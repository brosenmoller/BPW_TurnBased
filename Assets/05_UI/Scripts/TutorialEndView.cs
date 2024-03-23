using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialEndView : UIView
{
    [SerializeField] private Button startButton;

    private void Awake()
    {
        startButton.onClick.AddListener(OnStart);
    }

    private void OnStart()
    {
        SceneManager.LoadScene(2);
    }
}
