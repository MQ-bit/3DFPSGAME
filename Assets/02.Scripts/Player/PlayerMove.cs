using UnityEngine;

// 키보드(WASD)로 이동하고, 중력을 받으며, Space로 점프한다.
[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public float _moveSpeed = 7f;    // 이동 속도 (m/s)
    public float JumpPower = 7f;    // 점프할 때 위로 주는 속도 (m/s)
    private const float Gravity = -20f;    // 중력 가속도 (m/s²)

    private CharacterController _controller;
    private float _yVelocity = 0f;  // 위아래 속도

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 1. 키보드 입력을 받는다.
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 2. 입력으로 방향을 만들고, 플레이어가 보는 방향 기준으로 바꾼다.
        Vector3 direction = new Vector3(h, 0f, v);
        direction.Normalize();
        direction = transform.TransformDirection(direction);

        // 3. 바닥에 서 있으면 떨어지는 속도를 초기화한다.
        if (_controller.isGrounded && _yVelocity < 0f)
        {
            _yVelocity = -2f; // 살짝 아래로 눌러 두어야 isGrounded가 계속 true로 유지된다.
        }

        // 4. 바닥에서 점프 키를 누르면 위로 속도를 준다.
        if (Input.GetButtonDown("Jump") && _controller.isGrounded)
        {
            _yVelocity = JumpPower;
        }

        // 5. 중력을 누적한다. (v = v0 + a·t)
        _yVelocity += Gravity * Time.deltaTime;

        // 6. 수평 속도와 수직 속도를 합쳐서 이동한다.
        Vector3 velocity = direction * _moveSpeed;
        velocity.y = _yVelocity;
        _controller.Move(velocity * Time.deltaTime);
    }
}