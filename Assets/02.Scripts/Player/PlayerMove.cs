using UnityEngine;

// 키보드(WASD)로 이동하고, 중력을 받으며, Space로 점프한다.
[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 7f; // 이동 속도 (m/s)
    [SerializeField] private float _dashSpeed = 12f;
    [SerializeField] private float _jumpPower = 7f;     // 점프할 때 위로 주는 속도 (m/s)
    [SerializeField] private CharacterController _controller;
    [Tooltip("모든 시점에서 이동 방향의 기준이 되는 카메라 Transform")]
    [SerializeField] private Transform _viewTransform;
    private CameraRotate _cameraRotate;

    // 코요테 타임: 발판을 벗어난 직후에도 점프를 허용해 늦은 입력을 보완한다.
    [Min(0f)] public float coyoteTime = 0.12f;

    // 점프 버퍼: 조금 일찍 누른 입력을 기억해 착지 직전 입력도 자연스럽게 이어 준다.
    [Min(0f)] public float jumpBufferTime = 0.15f;

    // 가변 점프 높이: 상승 중 버튼을 놓으면 중력을 강화해 짧게 누를수록 낮게 뛴다.
    [Min(1f)] public float jumpReleaseGravityMultiplier = 2.5f;

    private const float Gravity = -20f;    // 중력 가속도 (m/s²)
    private const int MaxJumpCount = 2;
    private int _jumpCount = 0;
    private float _yVelocity = 0f;  // 위아래 속도

    // 코요테 타임/점프 버퍼: 시작 시 가짜 착지나 점프 입력이 생기지 않도록 초기화한다.
    private float _lastGroundedTime = float.NegativeInfinity;
    private float _lastJumpPressedTime = float.NegativeInfinity;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        // 인스펙터 지정이 없으면 플레이어의 자식 카메라, 메인 카메라 순서로 찾는다.
        if (_viewTransform == null)
        {
            Camera viewCamera = GetComponentInChildren<Camera>();
            if (viewCamera == null)
            {
                viewCamera = Camera.main;
            }

            _viewTransform = viewCamera != null ? viewCamera.transform : transform;
        }

        _cameraRotate = _viewTransform.GetComponent<CameraRotate>();
    }

    private void Update()
    {
        // 1. 키보드 입력을 받는다.
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 카메라의 좌우 회전을 기준으로 이동해 W를 누르면 보는 방향으로 전진한다.
        // 상하 회전은 제외해 위아래를 보더라도 수평 이동 속도가 일정하게 유지된다.
        // LateUpdate의 카메라 표시를 기다리지 않고 이번 프레임의 시점 방향을 사용한다.
        // 탑다운처럼 수직으로 보는 시점도 좌우 각도로 계산해 이동 방향이 사라지지 않는다.
        Quaternion viewYaw = _cameraRotate != null && _cameraRotate.isActiveAndEnabled
            ? _cameraRotate.MovementRotation
            : Quaternion.Euler(0f, _viewTransform.eulerAngles.y, 0f);
        Vector3 direction = viewYaw * new Vector3(h, 0f, v).normalized;

        // 코요테 타임: 상승 중에는 이전 Move의 접지 결과로 점프 횟수가 초기화되지 않게 한다.
        bool isGrounded = _controller.isGrounded && _yVelocity <= 0f;
        if (isGrounded)
        {
            _yVelocity = -2f; // 살짝 아래로 눌러 두어야 isGrounded가 계속 true로 유지된다.
            _jumpCount = 0;
            _lastGroundedTime = Time.time;
        }

        // 점프 버퍼: 누른 순간을 저장해 아직 점프할 수 없어도 잠깐 입력을 유지한다.
        if (Input.GetButtonDown("Jump"))
        {
            _lastJumpPressedTime = Time.time;
        }

        // 코요테 타임: 유예 시간 안의 점프는 첫 점프로 처리해 공중 점프를 남겨 둔다.
        bool canGroundJump = _jumpCount == 0
            && (isGrounded || Time.time - _lastGroundedTime <= coyoteTime);

        // 코요테 타임: 유예 시간이 끝난 뒤에는 첫 점프를 소모한 것으로 처리한다.
        // 발판에서 걸어 떨어져도 공중 점프는 한 번만 가능해 2단 점프 규칙이 일관된다.
        if (!isGrounded && !canGroundJump && _jumpCount == 0)
        {
            _jumpCount = 1;
        }

        // 점프 버퍼: 착지하거나 공중 점프가 가능해지면 기억한 입력을 한 번만 소비한다.
        bool hasBufferedJump = Time.time - _lastJumpPressedTime <= jumpBufferTime;
        if (hasBufferedJump && (canGroundJump || _jumpCount < MaxJumpCount))
        {
            _yVelocity = _jumpPower;
            _jumpCount++;
            _lastJumpPressedTime = float.NegativeInfinity;
            _lastGroundedTime = float.NegativeInfinity;
        }

        // 가변 점프 높이: 누르고 있으면 원래 높이로, 놓으면 빠르게 상승을 멈춘다.
        // 현재 버튼 상태를 확인하므로 착지 전에 버튼을 놓은 버퍼 점프에도 적용된다.
        float gravityMultiplier = _yVelocity > 0f && !Input.GetButton("Jump")
            ? jumpReleaseGravityMultiplier
            : 1f;
        _yVelocity += Gravity * gravityMultiplier * Time.deltaTime;

        // 6. 수평 속도와 수직 속도를 합쳐서 이동한다.
        float speed = Input.GetKey(KeyCode.LeftShift) ? _dashSpeed : _moveSpeed;
        Vector3 velocity = direction * speed;
        velocity.y = _yVelocity;
        _controller.Move(velocity * Time.deltaTime);
    }
}
