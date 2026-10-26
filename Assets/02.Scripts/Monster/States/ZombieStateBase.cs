// 좀비 상태의 공통 부모. 상태마다 클래스로 나누고, Zombie(Context)가 전환을 관리한다.
public abstract class ZombieStateBase
{
    protected Zombie Zombie { get; }

    // 계층형 FSM에서 이 상태를 감싸는 부모 상태. 최상위 상태는 null.
    public CompositeState Parent { get; }

    protected ZombieStateBase(Zombie zombie, CompositeState parent = null)
    {
        Zombie = zombie;
        Parent = parent;
    }

    public virtual void Enter() { }

    public virtual void Update() { }

    public virtual void Exit() { }
}
