using UnityEngine;

// 회전 시점에서는 시선을, 고정 시점에서는 이동 방향을 향해 몸을 돌린다.
[DefaultExecutionOrder(-100)]
public class PlayerRotate : MonoBehaviour
{
    public float RotationSpeed = 200f; // 카메라가 없을 때 사용할 마우스 감도

    private float _accumulationX = 0f;
    [SerializeField] private CameraRotate _cameraRotate;

    private void Awake()
    {
        _accumulationX = transform.eulerAngles.y;
        if (_cameraRotate == null)
        {
            Camera viewCamera = GetComponentInChildren<Camera>();
            if (viewCamera == null)
            {
                viewCamera = Camera.main;
            }

            if (viewCamera != null)
            {
                _cameraRotate = viewCamera.GetComponent<CameraRotate>();
            }
        }
    }

    private void Update()
    {
        if (_cameraRotate != null && _cameraRotate.isActiveAndEnabled)
        {
            if (_cameraRotate.UsesMouseLook)
            {
                // 1인칭·3인칭·숄더뷰: 카메라가 계산한 방향을 공유해 몸도 함께 회전한다.
                transform.rotation = _cameraRotate.MovementRotation;
            }
            else
            {
                // 쿼터·탑다운·사이드뷰: 카메라는 고정하고 몸만 화면 기준 이동 방향을 향한다.
                Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f,
                    Input.GetAxisRaw("Vertical"));
                if (input.sqrMagnitude > 0f)
                {
                    Vector3 direction = _cameraRotate.MovementRotation * input;
                    transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                }
            }

            _accumulationX = transform.eulerAngles.y;
            return;
        }

        // 카메라 회전 컴포넌트가 없는 경우에도 마우스로 몸을 돌릴 수 있다.
        float mouseX = Input.GetAxis("Mouse X");
        _accumulationX += mouseX * RotationSpeed * Time.deltaTime;

        transform.eulerAngles = new Vector3(0f, _accumulationX, 0f);
    }
}
