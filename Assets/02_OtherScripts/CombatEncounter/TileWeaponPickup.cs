using UnityEngine;

public class TileWeaponPickup : TileContent
{
    [SerializeField] private Weapon weapon;

    public override void OnAwake()
    {
        GetComponentInChildren<SpriteRenderer>().sprite = weapon.icon;
    }

    public void PickupWeapon(TileEntity tileEntity)
    {
        tileEntity.selectedWeapon = weapon;
        combatRoomController.RemoveTileContent(this);
    }

    public void SetWeapon(Weapon weapon)
    {
        this.weapon = weapon;
    }
}
