using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;


// 마우스 왼쪽 버튼을 누르면 바라보는 방향으로 총알을 발사하고 싶다.
public class PlayerGunFire : MonoBehaviour
{
    private const int HitEffectPoolSize = 16;

    [SerializeField] private ParticleSystem _hitEffectPrefab;

    [Header("스탯")][SerializeField] private int _maxBulletCount = 30;

    [SerializeField] private float _reloadingTime = 3f;
    [SerializeField] private float _range = 100f; // 사거리
    [SerializeField] private float _damage = 10f; // 공격력
    [SerializeField] private float _fireInterval = 0.1f; // 발사 속도
    private int _bulletCount;

    [Header("레이저")]
    [SerializeField] private Transform _muzzle;           // 레이저가 시작되는 위치. 비워두면 카메라 기준 오프셋을 쓴다.
    [SerializeField] private Vector3 _laserStartOffset = new Vector3(0.3f, -0.25f, 0.6f); // 카메라 기준 시작 위치
    [SerializeField] private Material _laserMaterial;
    [SerializeField] private float _laserWidth = 0.02f;
    [SerializeField] private Color _laserIdleColor = new Color(0.2f, 1f, 0.4f, 1f);  // 평소 색
    [SerializeField] private Color _laserFireColor = new Color(1f, 0.15f, 0.1f, 1f); // 발사할 때 색
    [SerializeField] private float _laserFireFlashDuration = 0.1f;                   // 발사 후 이 시간 동안 발사 색을 유지한다.

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private LineRenderer _laserLine;
    private MaterialPropertyBlock _laserBlock;   // 머티리얼을 복제하지 않고 색만 바꾼다.
    private float _fireFlashTimer;
    private bool _isLaserFireColor;

    private float _fireTimer;
    private Coroutine _reloadCoroutine;


    private void Awake()
    {
        _laserLine = GetComponent<LineRenderer>();
        if (_laserLine == null) _laserLine = gameObject.AddComponent<LineRenderer>();

        _laserLine.positionCount = 2;
        _laserLine.useWorldSpace = true;
        _laserLine.startWidth = _laserWidth;
        _laserLine.endWidth = _laserWidth;
        _laserLine.sharedMaterial = _laserMaterial;
        _laserLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _laserLine.receiveShadows = false;
        _laserLine.enabled = false;

        _laserBlock = new MaterialPropertyBlock();
        SetLaserColor(_laserIdleColor);
    }

    private void Start()
    {
        _bulletCount = _maxBulletCount;
    }


    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;
        // 0. 발사 타이머를 지난 시간만큼 줄인다.
        _fireTimer -= Time.deltaTime;

        // 1. 버튼을 누르고 있고, 마우스가 UI 위에 있지 않고, 쏠 때가 되었고, 총알이 남아 있으면 쏜다.
        bool isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
        if (Input.GetButton("Fire1") && !isPointerOverUI && _fireTimer <= 0f && _bulletCount > 0)
        {

            // 2. 마우스 왼쪽 버튼 입력 받기
            if (Input.GetButton("Fire1") && _fireTimer <= 0f && _bulletCount > 0)
            {
                _bulletCount--;
                _fireTimer = _fireInterval;
                Shoot();

                // 이 방식은: 이 모노비헤이비어가 등록한 모든 코루틴을 실행 취소
                // StopAllCoroutines();

                if (_reloadCoroutine != null)
                {
                    StopCoroutine(_reloadCoroutine);
                    Debug.Log("재장전 취소!");
                    _reloadCoroutine = null;
                }
            }
        }

        // 3. R키 누르면 총알 재장전 시작
        if (Input.GetKeyDown(KeyCode.R) && _reloadCoroutine == null)
            _reloadCoroutine = StartCoroutine(Reloading_Coroutine());
    }

    private IEnumerator Reloading_Coroutine()
    {
        Debug.Log("재장전 시작!");

        yield return new WaitForSeconds(_reloadingTime);

        _bulletCount = _maxBulletCount;

        Debug.Log("재장전 완료!");
        _reloadCoroutine = null;
    }


    // 카메라가 Update 에서 회전하고 LateUpdate 에서 따라오므로, 레이저도 LateUpdate 에서 그린다.
    private void LateUpdate()
    {
        // 게임 중에는 항상 보이고, 준비/게임오버에는 숨긴다.
        bool isPlaying = GameManager.Instance.State == GameState.Playing;
        _laserLine.enabled = isPlaying;
        if (!isPlaying) return;

        UpdateLaser();
        UpdateLaserColor();
    }

    // 조준하는 곳까지 선을 이어준다. 맞는 물체가 있으면 거기서 끝나고, 허공이면 사거리 끝까지 간다.
    private void UpdateLaser()
    {
        var cameraRotation = Camera.main.transform.rotation;
        var origin = transform.position;   // 카메라는 플레이어 위치를 그대로 따라간다.
        var ray = new Ray(origin, cameraRotation * Vector3.forward);

        var end = Physics.Raycast(ray, out var hitInfo, _range) ? hitInfo.point : ray.GetPoint(_range);

        // 총구가 지정되지 않았다면 카메라 오른쪽 아래에서 시작한다.
        // (시선 한가운데에서 시작하면 선이 끝에서 본 점처럼 보여 안 보인다.)
        var start = _muzzle != null ? _muzzle.position : origin + cameraRotation * _laserStartOffset;

        _laserLine.SetPosition(0, start);
        _laserLine.SetPosition(1, end);
    }

    // 발사한 직후에는 발사 색, 그 외에는 평소 색으로 바꾼다. 색이 바뀔 때만 적용한다.
    private void UpdateLaserColor()
    {
        _fireFlashTimer -= Time.deltaTime;

        bool isFireColor = _fireFlashTimer > 0f;
        if (isFireColor == _isLaserFireColor) return;

        _isLaserFireColor = isFireColor;
        SetLaserColor(isFireColor ? _laserFireColor : _laserIdleColor);
    }

    private void SetLaserColor(Color color)
    {
        _laserLine.GetPropertyBlock(_laserBlock);
        _laserBlock.SetColor(BaseColorId, color);
        _laserLine.SetPropertyBlock(_laserBlock);
    }

    private void Shoot()
    {
        // Ray: 레이저(시작위치, 날아갈 방향)
        // Physics.RayCast: 레이저를 발사
        // RayCastHit: 레이저가 물체와 부딛히면 그 정보를 저장하는 구조체

        // 2. 레이(광선)를 생성하고 발사될 위치와 진행 방향 설정하기
        var cameraTransform = Camera.main.transform;
        var ray = new Ray(cameraTransform.position, cameraTransform.forward);

        // 3. 레이가 부딛힌 대상의 정보를 저장할 변수 생성하기
        var hitInfo = new RaycastHit();

        // 4. 레이를 발사하고, 만일 부딛힌 물체가 있다면 이펙트 표시하기
        var isHit = Physics.Raycast(ray, out hitInfo, _range);

        // 쏘는 순간 레이저 색을 발사 색으로 바꾼다.
        _fireFlashTimer = _laserFireFlashDuration;

        if (isHit)
        {
            // Debug.Log($"{hitInfo.collider.name}이 총알을 맞았다. 대미지: {_damage}");

            // 맞은 자리에 총알 자국 이펙트 만들기
            // 맞은 자리에서 수직 방향으로 회전해 재생한다.
            EffectManager.Instance.Play(_hitEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal), 1f, HitEffectPoolSize);

            // 대미지 구조체 만들기
            var damageInfo = new DamageInfo();
            damageInfo.Value = _damage;
            damageInfo.HitPoint = hitInfo.point;
            damageInfo.HitNormal = hitInfo.normal;

            hitInfo.collider.GetComponent<IDamageable>()?.TakeDamage(damageInfo);
            /*
            hitInfo.collider.GetComponent<Zombie>()?.TakeDamage(_damage);
            hitInfo.collider.GetComponent<Drum>()?.TakeDamage(_damage);
            hitInfo.collider.GetComponent<Box>()?.TakeDamage(_damage);
            */
        }
    }
}