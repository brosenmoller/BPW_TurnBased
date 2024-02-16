using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GameUIView : UIView
{
    [Header("Buttons")]
    [SerializeField] private Button movementModeButton;
    [SerializeField] private Button attackModeButton;
    [SerializeField] private Button turnEndButton;

    [Header("Test")]
    [SerializeField] private TextMeshProUGUI levelIndicatorText;

    private Outline movementModeButtonOutline;
    private Outline attackModeButtonOutline;

    [HideInInspector] public UnityEvent OnSwitchedToMovement;
    [HideInInspector] public UnityEvent OnSwitchedToAttack;
    [HideInInspector] public UnityEvent OnTurnEnd;

    [Header("Weapons")]
    [SerializeField] private GameObject weaponSlotPrefab;
    [SerializeField] private RectTransform weaponSlotParent;
    
    private readonly List<WeaponSlot> weaponSlots = new();

    private WeaponSlot SelectedWeaponSlot => weaponSlots[selectedWeaponSlotIndex];
    private int selectedWeaponSlotIndex = 0;
    
    public override void Initialize()
    {
        movementModeButton.onClick.AddListener(OnMovementButtonClick);
        attackModeButton.onClick.AddListener(OnAttackButtonClick);
        turnEndButton.onClick.AddListener(OnTurnEndButtonClick);

        movementModeButtonOutline = movementModeButton.GetComponent<Outline>();
        attackModeButtonOutline = attackModeButton.GetComponent<Outline>();

        GameManager.InputManager.controls.Default.TurnEnd.performed += _ => OnTurnEnd?.Invoke();
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

    private void OnTurnEndButtonClick()
    {
        OnTurnEnd?.Invoke();
    }

    public void SetLevelIndicatorText(string text)
    {
        levelIndicatorText.text = text;
    }

    public void HighligtAttackMode()
    {
        movementModeButtonOutline.enabled = false;
        attackModeButtonOutline.enabled = true;
    }

    public void UpdateWeaponDisplay(Weapon[] weapons, Weapon selectedWeapon, int maxWeapons)
    {
        foreach (RectTransform child in weaponSlotParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < maxWeapons; i++)
        {
            GameObject newWeaponSlot = Instantiate(weaponSlotPrefab, weaponSlotParent);

            if (i < weapons.Length)
            {
                newWeaponSlot.GetComponent<Outline>().enabled = weapons[i] == selectedWeapon;
                newWeaponSlot.transform.GetChild(0).GetComponent<Image>().sprite = weapons[i].icon;
            }
        }
    }

    public void EquipWeapon(Weapon weapon)
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (weaponSlots[i] == null)
            {
                weaponSlots[i].SetWeapon(weapon);
                weaponSlots[selectedWeaponSlotIndex].ToggleOutlineEnabled();
                selectedWeaponSlotIndex = i;
                weaponSlots[selectedWeaponSlotIndex].ToggleOutlineEnabled();
                return;
            }
        }

        // TODO: Drop Weapon in slot

        SelectedWeaponSlot.SetWeapon(weapon);
    }

    public Weapon CycleWeaponSelection()
    {
        int nextIndex = selectedWeaponSlotIndex;
        do
        {
            nextIndex++;
            if (nextIndex >= weaponSlots.Count) { nextIndex = 0; }
        }
        while (weaponSlots[nextIndex].Weapon == null);

        weaponSlots[selectedWeaponSlotIndex].ToggleOutlineEnabled();
        selectedWeaponSlotIndex = nextIndex;
        weaponSlots[selectedWeaponSlotIndex].ToggleOutlineEnabled();

        return weaponSlots[selectedWeaponSlotIndex].Weapon;
    }

    [System.Serializable]
    public class WeaponSlot
    {
        public Weapon Weapon;
        public Image Image;
        public Outline Outline;

        public WeaponSlot(Image image, Outline outline)
        {
            Weapon = null;
            Image = image;
            Outline = outline;
        }

        public void SetWeapon(Weapon weapon)
        {
            Weapon = weapon;
            Image.sprite = weapon.icon;
        }

        public void ToggleOutlineEnabled()
        {
            Outline.enabled = !Outline.enabled;
        }
    }
}
