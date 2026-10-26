using UnityEngine;

// 피격: 공격(총알, 폭탄)을 받아 잠깐 멈칫한다.
public class HitState : ZombieStateBase
{
    private const float StunDuration = 0.3f;

    private float _timer;

    public HitState(Zombie zombie, CompositeState parent) : base(zombie, parent) { }

    public override void Enter()
    {
        _timer = 0f;
    }

    public override void Update()
    {
        _timer += Time.deltaTime;

        // 맞았으니 쫒아온다.
        if (_timer >= StunDuration) Zombie.ChangeState(ZombieState.Trace);
    }
}
