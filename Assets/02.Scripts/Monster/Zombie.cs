using System;
using System.Collections.Generic;
using UnityEngine;

// FSM(유한 상태 머신) : 유한개의 상태가 있고, 상태마다 행동을 다르게 함으로써 AI를 구현하는 유명한 방법
// - "상태": 객체가 가질 수 있는 어떤 조건이나 상황
// - "전이": 조건에 따라 현재 상태에서 다른 상태로 넘어가는 것
// - 그리고 위 방식에 따라 규칙을 세워둔 것을 "머신"
//
// HFSM(계층형 FSM) : 비슷한 상태를 부모 상태로 묶어 공통 전이를 한 번만 쓴다.
// Alive
//  ├─ Passive      Idle ⇄ Patrol
//  ├─ Aggressive   Trace ⇄ Attack
//  ├─ Comeback
//  └─ Hit
// Death


// 좀비가 가질 수 있는 상태 (실제로 행동하는 리프 상태)
public enum ZombieState
{
    Idle,       // 대기: 제자리에서 잠시 서 있는다.
    Patrol,     // 순찰: 처음 자리 주변을 돌아다닌다.
    Trace,      // 추격: 플레이어를 쫓아간다.
    Comeback,    // 복귀: 처음 자리로 돌아간다.
    Attack,     // 공격: 일정 시간마다 플레이어를 때린다.
    Hit,        // 피격: 공격(총알, 폭탄)을 받아 잠깐 멈칫한다.
    Death,      // 죽음: 쓰러지고 잠시 뒤 사라진다.
}

public class Zombie : MonoBehaviour, IDamageable
{
    [Header("스탯")]
    [SerializeField] private float _detectDistance = 8;    // 플레이어를 발견하는 거리
    [SerializeField] private float _combackDistance = 15;   // 처음 자리에서 이만큼 멀어지면 돌아가는 거리
    [SerializeField] private float _attackDamage = 10f;     // 공격 대미지
    [SerializeField] private float _attackRange = 1.5f;     // 공격 범위
    [SerializeField] private float _attackInterval = 1.2f;  // 공격 간격
    [SerializeField] private float _moveSpeed = 5;          // 이동속도
    [SerializeField] private float _maxHealth = 30;
    [SerializeField] private float _health;

    [Header("순찰")]
    [SerializeField] private float _idleTime = 3f;          // 순찰 사이에 대기하는 평균 시간
    [SerializeField] private float _patrolRadius = 6f;      // 처음 자리에서 이 반경 안에서 순찰한다.
    [SerializeField] private float _patrolTimeout = 5f;     // 이 시간 안에 도착하지 못하면 포기하고 쉰다.

    [Header("이펙트")]
    [SerializeField] private GameObject _bloodSprayPrefab;

    private CharacterController _controller;
    private Vector3 _startPosition;
    private GameObject _player;

    public float MaxHealth => _maxHealth;
    public float Health => _health;

    // 상태 객체들이 읽는 값
    public CharacterController Controller => _controller;
    public GameObject Player => _player;
    public Vector3 StartPosition => _startPosition;
    public float DetectDistance => _detectDistance;
    public float ComebackDistance => _combackDistance;
    public float AttackDamage => _attackDamage;
    public float AttackRange => _attackRange;
    public float AttackInterval => _attackInterval;
    public float IdleTime => _idleTime;
    public float PatrolRadius => _patrolRadius;
    public float PatrolTimeout => _patrolTimeout;

    // 피격 시 체력 UI 등이 구독한다.
    public event Action OnDamaged;

    // 죽음 연출이 끝났을 때 스포너(풀)가 구독한다. 구독자가 없으면 스스로 Destroy 한다.
    public event Action<Zombie> OnDeath;

    private ZombieState _state = ZombieState.Idle;
    private Dictionary<ZombieState, ZombieStateBase> _states;
    private ZombieStateBase _currentState;
    private ZombieStateBase _activeTopState;   // 현재 상태를 품은 최상위 상태. Update의 진입점
    private readonly List<GameObject> _bloodSprays = new List<GameObject>();

    private void Awake()
    {
        // 풀에서 꺼낸 직후 Init이 Start보다 먼저 불리므로 참조는 Awake에서 잡는다.
        _controller = GetComponent<CharacterController>();
        _player = GameObject.FindGameObjectWithTag("Player");

        // 체력 UI가 Start에서 값을 읽으므로 Awake에서 초기화한다.
        _health = _maxHealth;
        _startPosition = transform.position;

        CreateStates();
        ChangeState(ZombieState.Idle);
    }

    // 상태 객체는 좀비마다 한 번만 만든다. (풀링으로 재사용해도 할당이 늘지 않는다.)
    private void CreateStates()
    {
        CompositeState alive = new CompositeState(this);
        PassiveState passive = new PassiveState(this, alive);
        AggressiveState aggressive = new AggressiveState(this, alive);

        _states = new Dictionary<ZombieState, ZombieStateBase>
        {
            { ZombieState.Idle, new IdleState(this, passive) },
            { ZombieState.Patrol, new PatrolState(this, passive) },
            { ZombieState.Trace, new TraceState(this, aggressive) },
            { ZombieState.Attack, new AttackState(this, aggressive) },
            { ZombieState.Comeback, new ComebackState(this, alive) },
            { ZombieState.Hit, new HitState(this, alive) },
            { ZombieState.Death, new DeathState(this) },
        };
    }

    // 풀에서 꺼낼 때 위치와 상태를 처음 스폰된 상태로 되돌린다.
    public void Init(Vector3 position, Quaternion rotation)
    {
        // CharacterController가 켜져 있으면 transform 이동을 덮어쓰므로 끄고 옮긴다.
        _controller.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        _controller.enabled = true;

        _startPosition = position;
        _health = _maxHealth;
        ChangeState(ZombieState.Idle);

        foreach (GameObject bloodSpray in _bloodSprays)
        {
            if (bloodSpray != null) Destroy(bloodSpray);
        }
        _bloodSprays.Clear();

        // 체력바를 가득 찬 상태로 갱신한다.
        OnDamaged?.Invoke();
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        _activeTopState.Update();
    }

    // 상태를 전환한다. 현재 상태와 목표 상태의 공통 부모까지 Exit하고, 거기서 목표까지 Enter한다.
    // 예) Idle -> Trace : Idle.Exit, Passive.Exit, Aggressive.Enter, Trace.Enter
    public void ChangeState(ZombieState next)
    {
        ZombieStateBase target = _states[next];

        CompositeState commonParent = FindCommonParent(_currentState, target);

        ZombieStateBase exiting = _currentState;
        while (exiting != null && exiting != commonParent)
        {
            exiting.Exit();
            exiting = exiting.Parent;
        }

        GameLogger.Log($"{_state} -> {next}");

        _state = next;
        _currentState = target;
        EnterFromParent(target, commonParent);

        _activeTopState = target;
        while (_activeTopState.Parent != null)
        {
            _activeTopState = _activeTopState.Parent;
        }
    }

    // 두 상태를 모두 품고 있는 가장 가까운 부모. 없으면 null
    private static CompositeState FindCommonParent(ZombieStateBase from, ZombieStateBase to)
    {
        if (from == null) return null;

        for (CompositeState parent = from.Parent; parent != null; parent = parent.Parent)
        {
            for (CompositeState ancestor = to.Parent; ancestor != null; ancestor = ancestor.Parent)
            {
                if (ancestor == parent) return parent;
            }
        }

        return null;
    }

    // 공통 부모 바로 아래부터 목표까지 위에서 아래로 Enter한다.
    private static void EnterFromParent(ZombieStateBase state, CompositeState commonParent)
    {
        if (state.Parent != commonParent) EnterFromParent(state.Parent, commonParent);

        if (state.Parent != null) state.Parent.Current = state;
        state.Enter();
    }

    // 대미지를 받는다.
    public void TakeDamage(DamageInfo damageInfo)
    {
        if (_state == ZombieState.Death)
        {
            return;
        }

        _health -= damageInfo.Value;

        OnDamaged?.Invoke();

        if (_health > 0f)
        {
            // 좀비의 자식으로 블러드 스프레이를 생성! -> 위치 수정!
            GameObject bloodSpray = Instantiate(_bloodSprayPrefab, transform);
            _bloodSprays.Add(bloodSpray);
            //bloodSpray.transform.SetParent(transform);
            bloodSpray.transform.position = damageInfo.HitPoint;
            bloodSpray.transform.forward = damageInfo.HitNormal;

            // 잠깐 멈칫 하고 싶다!
            ChangeState(ZombieState.Hit);
        }
        else
        {
            ChangeState(ZombieState.Death);
        }
    }

    // 죽음 연출이 끝났을 때 DeathState가 부른다.
    public void FinishDeath()
    {
        if (OnDeath == null)
        {
            Destroy(gameObject);
            return;
        }

        OnDeath.Invoke(this);
    }

    public float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, _player.transform.position);
    }

    // 목표 지점을 향해 바라보며 걸어간다. (높이는 무시)
    public void MoveTowards(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        direction.Normalize();

        transform.forward = direction;
        _controller.SimpleMove(direction * _moveSpeed);
    }
}
