using UnityEngine;

// 순찰: 처음 자리 주변의 랜덤 지점으로 걸어간다.
public class PatrolState : ZombieStateBase
{
    private const float ArriveDistance = 1f;

    private Vector3 _destination;
    private float _timer;

    public PatrolState(Zombie zombie, CompositeState parent) : base(zombie, parent) { }

    public override void Enter()
    {
        // Todo. Walk 애니메이션 실행

        Vector2 offset = Random.insideUnitCircle * Zombie.PatrolRadius;
        Vector3 center = Zombie.StartPosition;
        _destination = new Vector3(center.x + offset.x, Zombie.transform.position.y, center.z + offset.y);
        _timer = 0f;
    }

    public override void Update()
    {
        _timer += Time.deltaTime;

        // 벽에 막혀 도착하지 못해도 일정 시간 뒤에는 쉰다.
        Vector3 toDestination = _destination - Zombie.transform.position;
        toDestination.y = 0f;
        if (toDestination.magnitude < ArriveDistance || _timer >= Zombie.PatrolTimeout)
        {
            Zombie.ChangeState(ZombieState.Idle);
            return;
        }

        Zombie.MoveTowards(_destination);
    }
}
