using UnityEngine;

// 공격: 일정 시간마다 플레이어를 때린다.
public class AttackState : ZombieStateBase
{
    private float _attackTimer;

    public AttackState(Zombie zombie, CompositeState parent) : base(zombie, parent) { }

    public override void Enter()
    {
        _attackTimer = 0f;
    }

    public override void Update()
    {
        // 플레이어가 공격 사정 거리 밖으로 나가면 쫓아!
        if (Zombie.DistanceToPlayer() > Zombie.AttackRange)
        {
            Zombie.ChangeState(ZombieState.Trace);
            return;
        }

        _attackTimer += Time.deltaTime;
        if (_attackTimer < Zombie.AttackInterval) return;

        // Todo. 공격 애니메이션 실행
        Vector3 playerPosition = Zombie.Player.transform.position;
        DamageInfo info = new DamageInfo()
        {
            Value = Zombie.AttackDamage,
            HitPoint = playerPosition,
            HitNormal = (Zombie.transform.position - playerPosition).normalized,
        };

        Zombie.Player.GetComponent<IDamageable>().TakeDamage(info);

        _attackTimer = 0f;
    }
}
