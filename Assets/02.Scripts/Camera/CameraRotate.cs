using UnityEngine;

// 마우스를 움직이면 카메라를 그 방향으로 회전시킨다.
[DefaultExecutionOrder(-200)]
public class CameraRotate : MonoBehaviour
{
    public float RotationSpeed = 200f; // 마우스 감도 (1초에 도는 각도)

    // 회전값을 직접 누적할 변수 (eulerAngles는 0~360으로 바뀌기 때문)
    private float _accumulationX = 0f; // 좌우 회전(Y축)
    private float _accumulationY = 0f; // 위아래 회전(X축)

    private bool _isFirstPerson = true;
    private Vector3 _rotationEuler;
    private bool _followPlayerYaw;
    private Transform _player;

    // 이동과 몸 회전도 같은 방향을 사용해 시점별 조작이 어긋나지 않게 한다.
    public bool UsesMouseLook => _isFirstPerson || _followPlayerYaw;
    public Quaternion MovementRotation => Quaternion.Euler(0f,
        UsesMouseLook ? _accumulationX + _rotationEuler.y : _rotationEuler.y, 0f);

    private void OnEnable()
    {
        if (_isFirstPerson)
        {
            _accumulationX = _player != null
                ? _player.eulerAngles.y - _rotationEuler.y
                : transform.eulerAngles.y - _rotationEuler.y;
            _accumulationY = Mathf.DeltaAngle(0f, transform.eulerAngles.x);
        }
    }

    public void SetViewSettings(bool isFirstPerson, Vector3 rotationEuler,
        bool followPlayerYaw, Transform player)
    {
        // 회전 시점으로 전환하면 현재 몸 방향에서 이어서 돌아 급격한 방향 전환을 막는다.
        if ((isFirstPerson || followPlayerYaw) && player != null)
        {
            _accumulationX = player.eulerAngles.y - rotationEuler.y;
        }

        _isFirstPerson = isFirstPerson;
        _rotationEuler = rotationEuler;
        _followPlayerYaw = followPlayerYaw;
        _player = player;
    }

    private void Update()
    {
        // 고정 시점에서는 마우스로 회전하지 않고 화면 방향을 유지한다.
        if (!UsesMouseLook)
        {
            return;
        }

        // 1. 마우스 입력을 받는다.
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // 2. 입력만큼 회전값을 누적한다. 마우스를 올리면 위를 보도록 Y는 부호를 뒤집는다.
        _accumulationX += mouseX * RotationSpeed * Time.deltaTime;
        _accumulationY += -mouseY * RotationSpeed * Time.deltaTime;

        // 3. 카메라에 넣기 전에, 내 변수에서 먼저 -90 ~ 90도로 제한한다.
        _accumulationY = Mathf.Clamp(_accumulationY, -90f, 90f);

    }

    private void LateUpdate()
    {
        if (_isFirstPerson)
        {
            // 1인칭은 기존 마우스 회전값을 사용하고 최종 상하 각도를 제한한다.
            transform.eulerAngles = new Vector3(
                Mathf.Clamp(_accumulationY + _rotationEuler.x, -90f, 90f),
                _accumulationX + _rotationEuler.y,
                _rotationEuler.z);
            return;
        }

        // 3인칭·숄더뷰도 몸과 같은 좌우 각도를 사용하고, 고정 시점은 월드 방향을 유지한다.
        transform.rotation = MovementRotation * Quaternion.Euler(_rotationEuler.x, 0f, _rotationEuler.z);
    }
}
