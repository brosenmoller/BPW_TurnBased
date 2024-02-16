using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class MainMenuView : UIView
{
    [Header("UI References")]
    [SerializeField] private Button PlayButton;
    [SerializeField] private Button CloseButton;

    public override void Initialize()
    {
        primarySelectable = PlayButton;

        PlayButton.onClick.AddListener(() => SceneManager.LoadScene(1));
        CloseButton.onClick.AddListener(() => Application.Quit());
    }
}
