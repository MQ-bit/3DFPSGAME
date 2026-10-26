using UnityEngine;

// 복귀: 처음 자리로 돌아간다.
public class ComebackState : ZombieStateBase
{
    private const float ArriveDistance = 1f;

    public ComebackState(Zombie zombie, CompositeState parent) : base(zombie, parent) { }

    public override void Update()
    {
        // 처음 자리에 도착하면 Idle
        if (Vector3.Distance(Zombie.transform.position, Zombie.StartPosition) < ArriveDistance)
        {
            Zombie.ChangeState(ZombieState.Idle);
            return;
        }

        Zombie.MoveTowards(Zombie.StartPosition);
    }
}
