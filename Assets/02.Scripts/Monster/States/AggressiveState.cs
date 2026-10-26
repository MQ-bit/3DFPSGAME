using UnityEngine;

// 플레이어와 교전하는 상태 묶음: Trace, Attack
public class AggressiveState : CompositeState
{
    public AggressiveState(Zombie zombie, CompositeState parent = null) : base(zombie, parent) { }

    protected override bool CheckTransition()
    {
        // 처음 자리에서 너무 멀어지면 돌아간다.
        if (Vector3.Distance(Zombie.transform.position, Zombie.StartPosition) <= Zombie.ComebackDistance) return false;

        Zombie.ChangeState(ZombieState.Comeback);
        return true;
    }
}
