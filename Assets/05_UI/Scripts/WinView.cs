using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinView : UIView
{
    [Header("UI References")]
    [SerializeField] private Button RePlayButton;
    [SerializeField] private Button MainMenuButton;

    public override void Initialize()
    {
        primarySelectable = RePlayButton;

        RePlayButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        MainMenuButton.onClick.AddListener(() => SceneManager.LoadScene(0));
    }
}
