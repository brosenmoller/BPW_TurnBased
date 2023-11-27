using System;
using UnityEngine;

public abstract class Weapon : ScriptableObject
{
    [Header("Base Data")]
    public string weaponName;
    public string weaponDescription;
    public Sprite icon;

    [Header("Stats")]
    public int damage;
    public float force;
    public int attackRange;

    [Header("Effects")]
    [SerializeField] protected AudioObject attackSound;

    public abstract void StartUseWeapon(Vector3 origin, Vector3 target, IDamageAble damageAble, Action callback);
}
