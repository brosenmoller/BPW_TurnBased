using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameUIView : UIView
{
    [Header("Buttons")]
    [SerializeField] private Button movementModeButton;
    [SerializeField] private Button attackModeButton;

    private Outline movementModeButtonOutline;
    private Outline attackModeButtonOutline;

    [HideInInspector] public UnityEvent OnSwitchedToMovement;
    [HideInInspector] public UnityEvent OnSwitchedToAttack;

    public override void Initialize()
    {
        movementModeButton.onClick.AddListener(OnMovementButtonClick);
        attackModeButton.onClick.AddListener(OnAttackButtonClick);
        movementModeButtonOutline = movementModeButton.GetComponent<Outline>();
        attackModeButtonOutline = attackModeButton.GetComponent<Outline>();
    }

    private void OnMovementButtonClick()
    {
        OnSwitchedToMovement?.Invoke();
    }

    public void HighligtMovementMode()
    {
        movementModeButtonOutline.enabled = true;
        attackModeButtonOutline.enabled = false;
    }

    private void OnAttackButtonClick()
    {
        OnSwitchedToAttack?.Invoke();
    }

    public void HighligtAttackMode()
    {
        movementModeButtonOutline.enabled = false;
        attackModeButtonOutline.enabled = true;
    }
}

