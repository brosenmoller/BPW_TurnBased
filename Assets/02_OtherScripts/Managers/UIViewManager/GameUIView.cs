using UnityEngine;
using UnityEngine.UI;

public class GameUIView : UIView
{
    [Header("Buttons")]
    [SerializeField] private Button movementModeButton;
    [SerializeField] private Button attackModeButton;

    private Outline movementModeButtonOutline;
    private Outline attackModeButtonOutline;

    public override void Initialize()
    {
        movementModeButton.onClick.AddListener(OnMovementButtonClick);
        attackModeButton.onClick.AddListener(OnAttackButtonClick);
        movementModeButtonOutline = movementModeButton.GetComponent<Outline>();
        attackModeButtonOutline = attackModeButton.GetComponent<Outline>();
    }

    private void OnMovementButtonClick()
    {
        movementModeButtonOutline.enabled = true;
        attackModeButtonOutline.enabled = false;
    }

    private void OnAttackButtonClick()
    {
        movementModeButtonOutline.enabled = false;
        attackModeButtonOutline.enabled = true;
    }
}

