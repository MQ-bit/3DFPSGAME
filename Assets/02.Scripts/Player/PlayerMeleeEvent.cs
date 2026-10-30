using UnityEngine;

// Slash 동작의 애니메이션 이벤트를 받아 부모의 PlayerMeleeAttack에게 전해 준다.
// 애니메이션 이벤트는 Animator가 붙은 오브젝트(Model)의 스크립트만 부를 수 있다.
public class PlayerMeleeEvent : MonoBehaviour
{
    private PlayerMeleeAttack _meleeAttack;

    private void Start()
    {
        _meleeAttack = GetComponentInParent<PlayerMeleeAttack>();
    }

    public void ActivateCollider()
    {
        _meleeAttack.ActivateCollider();
    }

    public void DeactivateCollider()
    {
        _meleeAttack.DeactivateCollider();
    }

    public void FanAttack()
    {
        _meleeAttack.FanAttack();
    }
}
