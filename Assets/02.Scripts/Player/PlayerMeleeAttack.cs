using UnityEngine;



// 근접 모드에서 마우스 왼쪽 버튼을 누르면 오른손의 카타나를 휘둘러, 칼날이나 앞쪽 부채꼴 안의 대상에 대미지를 준다.
public class PlayerMeleeAttack : MonoBehaviour
{
    [SerializeField] private float _damage = 25f;  // 대미지
    [SerializeField] private GameObject _katana;   // 오른손 뼈를 따라가는 Katana
    [SerializeField] private GameObject _rifle;    // 오른손 뼈 아래의 소총 (근접 모드에서는 숨긴다)

    [SerializeField] private MeleeHitMode _hitMode = MeleeHitMode.Collider;  // 맞힐 대상을 고르는 방법
    [SerializeField] private float _range = 2f;                              // 부채꼴의 반지름 (m)
    [SerializeField, Range(0f, 360f)] private float _angle = 90f;            // 부채꼴 각도 (도)
    [SerializeField] private LayerMask _targetMask;                          // 때릴 대상 (Drum, Zombie)
    [SerializeField] private LayerMask _obstacleMask;                        // 가로막는 것 (Default)

    private Player _player;
    private Animator _animator;
    private Collider _katanaCollider;              // 카타나의 칼날 (트리거)
    private int _meleeLayerIndex;                  // Melee Layer의 번호

    private void Awake()
    {
        _player = GetComponent<Player>();
        _animator = GetComponentInChildren<Animator>();
        _meleeLayerIndex = _animator.GetLayerIndex("Melee Layer");
        _katanaCollider = _katana.GetComponentInChildren<Collider>(true);  // 칼날의 콜라이더 (꺼져 있어도 찾는다)
    }

    private void OnEnable()
    {
        // 구독: 무기 모드가 바뀌면 불러 달라고 등록한다.
        _player.OnWeaponModeChanged += RefreshKatana;
    }

    private void OnDisable()
    {
        // 구독 취소
        _player.OnWeaponModeChanged -= RefreshKatana;
    }

    private void Start()
    {
        RefreshKatana(_player.WeaponMode);  // 처음 모드에 맞춰 카타나를 보이거나 숨긴다.
        DeactivateCollider();               // 처음에는 칼날을 꺼 둔다.
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        // 근접 모드에서 마우스 왼쪽 버튼을 누르고 있으면, 쉬고 있을 때마다 이어서 휘두른다. (커서가 보이는 동안은 제외)
        if (!Cursor.visible && Input.GetButton("Fire1") && _player.WeaponMode == WeaponMode.Melee && IsResting())
        {
            DeactivateCollider();  // 지난번에 켜진 채 남은 칼날이 있으면 끄고 시작한다.
            _animator.SetTrigger("Slash");
        }
    }

    // Melee Layer가 Empty에 있고 다른 상태로 넘어가는 중도 아니면 쉬고 있는 것이다.
    private bool IsResting()
    {
        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(_meleeLayerIndex);
        return state.IsName("Empty") && !_animator.IsInTransition(_meleeLayerIndex);
    }

    // 무기 모드가 바뀌었다는 알림이 오면 불리는 함수(콜백). 카타나는 근접 모드에서만, 소총은 그 외 모드에서만 보인다.
    private void RefreshKatana(WeaponMode mode)
    {
        var isMelee = mode == WeaponMode.Melee;
        _katana.SetActive(isMelee);
        _rifle.SetActive(!isMelee);
    }

    // 칼날을 켠다. Slash 동작의 ActivateCollider 이벤트가 불러 준다.
    public void ActivateCollider()
    {
        // 부채꼴 방법일 때는 칼날을 켜지 않는다.
        if (_hitMode != MeleeHitMode.Collider) return;

        _katanaCollider.enabled = true;
    }

    // 칼날을 끈다. Slash 동작의 DeactivateCollider 이벤트가 불러 준다.
    public void DeactivateCollider()
    {
        _katanaCollider.enabled = false;
    }

    // 칼날에 무언가가 닿으면 MeleeWeapon이 불러 준다.
    public void Hit(Collider other)
    {
        // 1. 내 몸(Player의 Character Controller)에 닿은 것은 무시한다.
        if (other.transform == transform) return;

        // 2. 대미지를 받을 수 없으면 무시한다.
        if (!other.TryGetComponent(out IDamageable damageable)) return;

        // 3. 대상에서 플레이어에 가장 가까운 점을 맞은 자리로 삼는다.
        //    칼날이 지나가며 닿은 대상은 모두 맞는다. (OnTriggerEnter는 대상에 새로 들어갈 때 한 번만 불리므로 같은 대상이 여러 번 맞지는 않는다.)
        Vector3 hitPoint = other.ClosestPoint(transform.position);
        GiveDamage(damageable, hitPoint);
    }

    // 대미지 정보를 만들어 넘긴다. 맞은 자리의 방향은 플레이어 쪽이다.
    private void GiveDamage(IDamageable damageable, Vector3 hitPoint)
    {
        DamageInfo damageInfo = new DamageInfo();
        damageInfo.Value = _damage;
        damageInfo.HitPoint = hitPoint;
        damageInfo.HitNormal = (transform.position - hitPoint).normalized;
        damageable.TakeDamage(damageInfo);
    }

    // 앞쪽 부채꼴 안의 대상을 한 번에 때린다. Slash 동작의 FanAttack 이벤트가 불러 준다.
    public void FanAttack()
    {
        // 칼날 콜라이더 방법일 때는 하지 않는다.
        if (_hitMode != MeleeHitMode.Fan) return;

        Vector3 origin = transform.position;

        // 1. 사거리 안의 대상을 모두 찾는다.
        Collider[] colliders = Physics.OverlapSphere(origin, _range, _targetMask);
        foreach (Collider col in colliders)
        {
            // 2. 대상에서 플레이어에 가장 가까운 점을 맞을 자리로 삼는다.
            Vector3 hitPoint = col.ClosestPoint(origin);

            // 3. 높이를 뺀 방향이 앞쪽에서 Angle의 절반 안에 있는지 본다.
            Vector3 direction = hitPoint - origin;
            direction.y = 0f;
            if (Vector3.Angle(transform.forward, direction) > _angle / 2f) continue;

            // 4. 가는 길에 벽이 있으면 때리지 않는다.
            if (Physics.Linecast(origin, hitPoint, _obstacleMask)) continue;

            // 5. 대미지를 받을 수 있으면 대미지를 준다.
            if (col.TryGetComponent(out IDamageable damageable))
            {
                GiveDamage(damageable, hitPoint);
            }
        }
    }

    // Player를 고르면 Scene 창에 부채꼴 방법의 공격 범위를 그린다.
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;
        Gizmos.color = Color.yellow;

        // 앞쪽을 y축으로 -Angle/2, +Angle/2만큼 돌린 두 방향이 부채꼴의 양 끝이다.
        Vector3 left = Quaternion.Euler(0f, -_angle / 2f, 0f) * transform.forward;
        Vector3 right = Quaternion.Euler(0f, _angle / 2f, 0f) * transform.forward;
        Gizmos.DrawLine(origin, origin + left * _range);
        Gizmos.DrawLine(origin, origin + right * _range);

        // 호: 10도마다 짧은 선을 이어 그린다.
        int count = Mathf.Max(1, Mathf.CeilToInt(_angle / 10f));
        Vector3 previous = origin + left * _range;
        for (int i = 1; i <= count; i++)
        {
            float angle = -_angle / 2f + _angle * i / count;
            Vector3 next = origin + Quaternion.Euler(0f, angle, 0f) * transform.forward * _range;
            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }
}
