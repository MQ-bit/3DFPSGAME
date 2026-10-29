using System.Collections;
using UnityEngine;
using System;

// 마우스 왼쪽 버튼을 누르면 바라보는 방향으로 총알을 발사하고 싶다.
public class PlayerGunFire : MonoBehaviour
{
    [SerializeField] private ParticleSystem _hitEffectPrefab;

    [Header("스탯")][SerializeField] private int _maxBulletCount = 30;

    [SerializeField] private float _reloadingTime = 3f;
    [SerializeField] private float _range = 100f; // 사거리
    [SerializeField] private float _damage = 10f; // 공격력
    [SerializeField] private float _fireInterval = 0.1f; // 발사 속도

    [Header("총알 궤적")]
    [SerializeField] private Material _tracerMaterial;
    [SerializeField] private Color _tracerColor = new Color(1f, 0.85f, 0.4f);
    [SerializeField] private float _tracerWidth = 0.03f;
    [SerializeField] private float _tracerDuration = 0.08f; // 궤적이 남아 있는 시간 (초)
    [SerializeField] private Vector3 _muzzleOffset = new Vector3(0.15f, -0.28f, 1f); // 카메라 기준 총구 위치 (오른쪽, 위, 앞)

    public event Action<int, int> OnBulletChanged;
    public int BulletCount => _bulletCount;
    public int MaxBulletCount => _maxBulletCount;

    private Animator _animator; // 군인 모델의 애니메이터
    private int _bulletCount;

    private float _fireTimer;
    private Coroutine _reloadCoroutine;
    private int _fireLayer; // Fire Layer의 번호

    private const float RecoilBlendTime = 0.03f; // 반동 모션을 처음부터 다시 재생할 때 섞는 시간 (초)

    private void Awake()
    {
        _bulletCount = _maxBulletCount;
    }

    private void Start()
    {

        _animator = GetComponentInChildren<Animator>();
        _fireLayer = _animator.GetLayerIndex("Fire Layer");
    }


    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        // 0. 발사 타이머를 지난 시간만큼 줄인다.
        _fireTimer -= Time.deltaTime;

        // 1. 마우스 왼쪽 버튼 입력 받기
        var isMouseVisible = Cursor.visible;
        var isFiring = !isMouseVisible && Input.GetButton("Fire1") && _bulletCount > 0;

        // 버튼을 누르고 있는 동안은 발사 간격 사이에도 Fire를 유지해야 사격 모션이 떨리지 않는다.
        _animator.SetBool("Fire", isFiring);

        if (isFiring && _fireTimer <= 0f)
        {
            _bulletCount--;
            OnBulletChanged?.Invoke(_bulletCount, _maxBulletCount);  // 한 발 줄었다고 알린다.
            _fireTimer = _fireInterval;
            Shoot();

            // 한 발마다 반동 모션을 처음부터 다시 재생한다. (클립 반동이 시작 직후에만 있다)
            _animator.CrossFadeInFixedTime("Fire", RecoilBlendTime, _fireLayer, 0f);

            // 이 방식은: 이 모노비헤이비어가 등록한 모든 코루틴을 실행 취소
            // StopAllCoroutines();

            if (_reloadCoroutine != null)
            {
                StopCoroutine(_reloadCoroutine);
                Debug.Log("재장전 취소!");
                _reloadCoroutine = null;
            }
        }

        // 2. R키 누르면 총알 재장전 시작
        if (Input.GetKeyDown(KeyCode.R) && _reloadCoroutine == null)
            _reloadCoroutine = StartCoroutine(Reloading_Coroutine());
    }

    private IEnumerator Reloading_Coroutine()
    {
        Debug.Log("재장전 시작!");

        yield return new WaitForSeconds(_reloadingTime);

        _bulletCount = _maxBulletCount;
        OnBulletChanged?.Invoke(_bulletCount, _maxBulletCount);  // 탄창을 채웠다고 알린다.
        Debug.Log("재장전 완료!");
        _reloadCoroutine = null;
    }


    // 총구에서 도착 지점까지 라인렌더러로 선을 그리고, 잠깐 뒤에 지운다.
    private void ShowTracer(Vector3 endPoint)
    {
        var cameraTransform = Camera.main.transform;
        var muzzle = cameraTransform.position
                     + cameraTransform.right * _muzzleOffset.x
                     + cameraTransform.up * _muzzleOffset.y
                     + cameraTransform.forward * _muzzleOffset.z;

        var tracer = new GameObject("BulletTracer");
        var line = tracer.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, muzzle);
        line.SetPosition(1, endPoint);
        line.startWidth = _tracerWidth;
        line.endWidth = _tracerWidth;
        line.startColor = _tracerColor;
        line.endColor = _tracerColor;
        line.sharedMaterial = _tracerMaterial;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;

        Destroy(tracer, _tracerDuration);
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

        // 맞았으면 맞은 자리까지, 허공이면 사거리 끝까지 총알 궤적을 그린다.
        var tracerEnd = isHit ? hitInfo.point : ray.origin + ray.direction * _range;
        ShowTracer(tracerEnd);

        if (isHit)
        {
            // Debug.Log($"{hitInfo.collider.name}이 총알을 맞았다. 대미지: {_damage}");

            // 맞은 자리에 총알 자국 이펙트 만들기
            var effect = Instantiate(_hitEffectPrefab);
            effect.transform.position = hitInfo.point; // 맞은 자리
            effect.transform.forward = hitInfo.normal; // 수직 위치로 회전
            effect.Play();

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

            // 황유찬 :         Player
            // 김민쉬:     Zombie, Boss
            // 박현서:  Drum, Box, Bird
        }
    }
}