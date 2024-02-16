using UnityEngine;

public class TileWeaponPickup : TileContent
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public Weapon Weapon { get; private set; }

    public override void OnAwake()
    {
        ContentType = TileContentType.WeaponPickup;
    }

    public override void Interact(TileEntity tileEntity)
    {
        tileEntity.SetWeapon(Weapon);
        combatRoomController.RemoveTileContent(this);
        Debug.Log("Pickup");
    }

    public void SetWeapon(Weapon weapon)
    {
        Weapon = weapon;
        spriteRenderer.sprite = Weapon.icon;
    }
}
