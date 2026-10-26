// 플레이어와 교전하지 않는 상태 묶음: Idle, Patrol
public class PassiveState : CompositeState
{
    public PassiveState(Zombie zombie, CompositeState parent = null) : base(zombie, parent) { }

    protected override bool CheckTransition()
    {
        // 플레이어가 가까워지면 Trace 상태로 전이한다.
        if (Zombie.DistanceToPlayer() > Zombie.DetectDistance) return false;

        Zombie.ChangeState(ZombieState.Trace);
        return true;
    }
}
