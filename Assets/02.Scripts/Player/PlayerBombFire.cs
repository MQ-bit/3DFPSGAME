using UnityEngine;

// 마우스 오른쪽 버튼을 누르고 있다 떼면 시선이 바라보는 방향으로 수류탄을 던지고 싶다.
// - 오래 누를수록 더 멀리 날아간다.
// - 던지는 동작 한 번에 수류탄은 하나만 나간다. (동작 중에는 다시 던질 수 없다)
public class PlayerBombFire : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Bomb _bombPrefab;

    [Header("던지는 힘")]
    [SerializeField] private float _minThrowPower = 8f;     // 바로 뗐을 때의 힘
    [SerializeField] private float _maxThrowPower = 25f;    // 끝까지 모았을 때의 힘
    [SerializeField] private float _maxChargeTime = 1.5f;   // 힘이 최대가 되기까지 누르는 시간 (초)

    [Header("던지는 동작")]
    [SerializeField] private float _releaseTime = 0.37f;    // 동작이 이만큼 진행되면 수류탄이 손을 떠난다 (0 ~ 1)
    [SerializeField] private float _throwFadeTime = 0.3f;   // 던진 뒤 팔이 총으로 돌아오는 시간 (초)

    private Animator _animator;
    private int _throwLayer;     // Throw Layer의 번호
    private float _throwWeight;  // Throw Layer의 지금 무게

    private bool _isCharging;     // 오른쪽 버튼을 누르고 모으는 중인가
    private float _chargeTime;    // 지금까지 모은 시간 (초)

    private bool _isThrowing;     // 던지는 동작 중인가 (트리거를 건 순간부터 동작이 끝날 때까지)
    private bool _enteredThrow;   // Throw 상태에 들어왔는가
    private bool _hasReleased;    // 이번 동작에서 수류탄을 이미 놓았는가
    private float _throwPower;    // 이번에 던질 힘

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _throwLayer = _animator.GetLayerIndex("Throw Layer");  // 이름으로 레이어 번호를 찾는다.
    }

    private void Update()
    {
        UpdateThrow();  // 게임 상태와 상관없이 던지는 동작은 끝까지 진행하고 팔은 돌려놓는다.

        if (GameManager.Instance.State != GameState.Playing)
        {
            _isCharging = false;
            return;
        }

        // 던지는 동작 중에는 새로 모으거나 던질 수 없다.
        if (_isThrowing) return;

        // 1. 마우스 오른쪽 버튼을 누르면 모으기 시작한다.
        if (Input.GetButtonDown("Fire2"))
        {
            _isCharging = true;
            _chargeTime = 0f;
        }

        if (!_isCharging) return;

        // 2. 누르고 있는 동안 힘을 모은다.
        _chargeTime = Mathf.Min(_chargeTime + Time.deltaTime, _maxChargeTime);

        // 3. 버튼을 떼면 모은 만큼의 힘으로 던지는 동작을 시작한다.
        if (Input.GetButtonUp("Fire2"))
        {
            _isCharging = false;
            StartThrow(Mathf.Lerp(_minThrowPower, _maxThrowPower, _chargeTime / _maxChargeTime));
        }
    }

    // 던지는 동작을 시작한다. 수류탄은 동작 중 손을 떠나는 순간(_releaseTime)에 나간다.
    private void StartThrow(float power)
    {
        _throwPower = power;
        _isThrowing = true;
        _enteredThrow = false;
        _hasReleased = false;

        _throwWeight = 1f;
        _animator.SetTrigger("Throw");
    }

    // 던지는 동작을 진행하고, 손을 떠나는 순간 수류탄을 던지고, 동작이 끝나면 팔을 돌려놓는다.
    private void UpdateThrow()
    {
        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(_throwLayer);
        bool inThrow = state.IsName("Throw");

        if (_isThrowing)
        {
            if (inThrow)
            {
                _enteredThrow = true;

                if (!_hasReleased && state.normalizedTime >= _releaseTime)
                {
                    _hasReleased = true;
                    ThrowBomb();
                }
            }
            else if (_enteredThrow)
            {
                // Throw 상태가 끝나고 Empty로 돌아왔다. 다시 던질 수 있다.
                _isThrowing = false;
                _enteredThrow = false;
            }
        }

        // 던지는 동작이 반쯤 지나면 Throw Layer의 무게를 천천히 0으로 줄인다.
        if (inThrow && state.normalizedTime >= 0.5f)
        {
            _throwWeight = Mathf.MoveTowards(_throwWeight, 0f, Time.deltaTime / _throwFadeTime);
        }
        _animator.SetLayerWeight(_throwLayer, _throwWeight);
    }

    private void ThrowBomb()
    {
        // 발사 위치에서 수류탄 생성하기
        Bomb bomb = Instantiate(_bombPrefab, _firePoint.position, Quaternion.identity);

        // 생성된 수류탄을 시선 방향으로 던지기
        Rigidbody rigidbody = bomb.GetComponent<Rigidbody>();
        rigidbody.AddForce(Camera.main.transform.forward * _throwPower, ForceMode.Impulse);
    }
}
