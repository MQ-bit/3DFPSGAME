// 추격: 플레이어를 쫓아간다.
public class TraceState : ZombieStateBase
{
    public TraceState(Zombie zombie, CompositeState parent) : base(zombie, parent) { }

    public override void Update()
    {
        // Todo. Run 애니메이션 실행

        // 플레이어가 공격 거리 안이면 공격한다.
        if (Zombie.DistanceToPlayer() <= Zombie.AttackRange)
        {
            Zombie.ChangeState(ZombieState.Attack);
            return;
        }

        Zombie.MoveTowards(Zombie.Player.transform.position);
    }
}
