using UnityEngine;

// 마우스를 움직이면 카메라를 그 방향으로 회전시킨다.
public class CameraRotate : MonoBehaviour
{
    public float RotationSpeed = 200f; // 마우스 감도 (1초에 도는 각도)

    // 회전값을 직접 누적할 변수 (eulerAngles는 0~360으로 바뀌기 때문)
    private float _accumulationX; // 좌우 회전(Y축)
    private float _accumulationY; // 위아래 회전(X축)
    private bool _followPlayerYaw;

    private bool _isFirstPerson = true;
    private Transform _player;
    private Vector3 _rotationEuler;

    private void Update()
    {
        // 1. 마우스 입력을 받는다.
        var mouseX = Input.GetAxis("Mouse X");
        var mouseY = Input.GetAxis("Mouse Y");

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

        // 3인칭·숄더뷰는 몸 방향을 따르고, 고정 시점은 월드 방향을 사용한다.
        var referenceRotation = _followPlayerYaw && _player != null
            ? Quaternion.Euler(0f, _player.eulerAngles.y, 0f)
            : Quaternion.identity;

        transform.rotation = referenceRotation * Quaternion.Euler(_rotationEuler);
    }

    private void OnEnable()
    {
        if (_isFirstPerson)
        {
            _accumulationX = transform.eulerAngles.y;
            _accumulationY = Mathf.DeltaAngle(0f, transform.eulerAngles.x);
        }
    }

    public void SetViewSettings(bool isFirstPerson, Vector3 rotationEuler,
        bool followPlayerYaw, Transform player)
    {
        // 다른 시점에서 돌아오면 몸의 현재 방향과 마지막 1인칭 상하 각도로 복귀한다.
        if (isFirstPerson && !_isFirstPerson && player != null) _accumulationX = player.eulerAngles.y;

        _isFirstPerson = isFirstPerson;
        _rotationEuler = rotationEuler;
        _followPlayerYaw = followPlayerYaw;
        _player = player;
    }
}