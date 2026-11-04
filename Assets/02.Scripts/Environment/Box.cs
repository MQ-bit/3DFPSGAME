using UnityEngine;

// Todo. 공격 받으면 산산조각 나듯 부셔진다. 
public class Box : MonoBehaviour, IDamageable
{
    public bool IsDead => false;  // 아직 체력이 없다.

    public void TakeDamage(DamageInfo damageInfo)
    {
        // Todo. 뭔가 해야함
    }
}
