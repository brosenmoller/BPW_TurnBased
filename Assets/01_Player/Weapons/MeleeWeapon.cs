using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Weapon", menuName = "Weapons/Melee Weapon")]
public class MeleeWeapon : Weapon
{
    [SerializeField] private GameObject weaponPrefab;

    public override void StartUseWeapon(Vector3 origin, Vector3 target, IDamageAble damageAble, Action callback)
    {
        GameManager.Instance.StartCoroutine(UseWeapon(origin, target, damageAble, callback));
    }

    private IEnumerator UseWeapon(Vector3 origin, Vector3 target, IDamageAble damageAble, Action callback)
    {
        Vector3 direction = (target - origin).normalized;
        GameObject sword = Instantiate(weaponPrefab, origin + direction * 0.5f, Quaternion.identity);
        Animator animator = sword.GetComponentInChildren<Animator>();

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        sword.transform.rotation = rotation;

        animator.SetTrigger("AttackTrigger");

        yield return new WaitForSeconds(.1f);

        damageAble.GetHitEffects();

        yield return new WaitForSeconds(.5f);

        damageAble.ApplyDamge(damage);
        Destroy(sword);

        callback();
    }
}

