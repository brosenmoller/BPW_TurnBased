using System;
using UnityEngine;
using System.Collections;

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
    [SerializeField] private AudioObject attackSound;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;

    public void StartUseWeapon(Vector3 origin, Vector3 target, IDamageAble damageAble, Action callback)
    {
        GameManager.Instance.StartCoroutine(UseWeapon(origin, target, damageAble, callback));
    }

    private IEnumerator UseWeapon(Vector3 origin, Vector3 target, IDamageAble damageAble, Action callback)
    {
        GameObject bullet = Instantiate(bulletPrefab, origin, Quaternion.identity);
        SpriteRenderer renderer = bullet.GetComponent<SpriteRenderer>();
        ParticleSystem destoryParticles = bullet.transform.GetChild(0).GetComponent<ParticleSystem>();
        ParticleSystem fireParticles = bullet.transform.GetChild(1).GetComponent<ParticleSystem>();

        Vector3 direction = (target - origin).normalized;
        float sqrDistance = (target - origin).sqrMagnitude;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        bullet.transform.rotation = rotation;

        fireParticles.Play();

        while ((bullet.transform.position - origin).sqrMagnitude < sqrDistance)
        {
            bullet.transform.position += bulletSpeed * Time.deltaTime * direction;
            yield return null;
        }

        renderer.enabled = false;
        destoryParticles.Play();
        damageAble.GetHitEffects();

        yield return new WaitForSeconds(.5f);

        damageAble.ApplyDamge(damage);
        Destroy(bullet);

        callback();
    }
}
