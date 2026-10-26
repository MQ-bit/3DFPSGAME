// 자식 상태를 묶는 부모 상태. 공통 전이 조건은 여기서 한 번만 검사한다.
public class CompositeState : ZombieStateBase
{
    // 지금 활성화된 자식. Zombie.ChangeState가 진입 경로를 따라 채운다.
    public ZombieStateBase Current { get; set; }

    public CompositeState(Zombie zombie, CompositeState parent = null) : base(zombie, parent) { }

    public override void Update()
    {
        // 부모의 전이 조건이 먼저다. 전이가 일어났다면 자식은 더 이상 업데이트하지 않는다.
        if (CheckTransition()) return;

        Current?.Update();
    }

    // 전이가 일어났으면 true
    protected virtual bool CheckTransition()
    {
        return false;
    }
}
