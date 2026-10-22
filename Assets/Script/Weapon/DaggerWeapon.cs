using System.Collections;
using UnityEngine;

public class DaggerWeapon : WeaponBase
{
    [Header("단검 설정")]
    [SerializeField] private float activeTime = 0.15f;

    [SerializeField]
    private DaggerHitBox daggerHitBox;

    private void Awake()
    {
        if (daggerHitBox != null)
        {
            daggerHitBox.SetDamage(damage);
            daggerHitBox.gameObject.SetActive(false);
        }
    }

    protected override void Attack()
    {
        StartCoroutine(DaggerAttack());
    }

    private IEnumerator DaggerAttack()
    {
        if (daggerHitBox == null)
            yield break;

        daggerHitBox.SetDamage(damage);

        daggerHitBox.gameObject.SetActive(true);

        yield return new WaitForSeconds(activeTime);

        daggerHitBox.gameObject.SetActive(false);
    }
}