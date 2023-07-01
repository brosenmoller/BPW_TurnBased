using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    [Header("Base Data")]
    public string weaponName;
    public string weaponDescription;

    [Header("Stats")]
    public int damage;
    public float force;
    public int attackRange;

    [Header("Effects")]
    public AudioObject attackSound;
}
