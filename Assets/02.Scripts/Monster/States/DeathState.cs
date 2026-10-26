using UnityEngine;

// 죽음: 쓰러지고 잠시 뒤 사라진다.
public class DeathState : ZombieStateBase
{
    private const float DisappearDelay = 0.5f;

    private float _timer;

    public DeathState(Zombie zombie, CompositeState parent = null) : base(zombie, parent) { }

    public override void Enter()
    {
        Zombie.Controller.enabled = false;
        _timer = 0f;
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < DisappearDelay) return;

        Zombie.FinishDeath();
    }
}
