using UnityEngine;

// 대기: 제자리에서 잠시 서 있다가 순찰을 나간다.
public class IdleState : ZombieStateBase
{
    private float _timer;
    private float _duration;

    public IdleState(Zombie zombie, CompositeState parent) : base(zombie, parent) { }

    public override void Enter()
    {
        // Todo. Idle 애니메이션 실행

        // 좀비들이 동시에 움직이지 않도록 대기 시간을 흩뜨린다.
        _duration = Random.Range(Zombie.IdleTime * 0.5f, Zombie.IdleTime * 1.5f);
        _timer = 0f;
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _duration) Zombie.ChangeState(ZombieState.Patrol);
    }
}
