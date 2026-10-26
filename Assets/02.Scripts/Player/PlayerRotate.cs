using UnityEngine;

// 마우스를 좌우로 움직이면 플레이어 몸을 좌우로 회전시킨다.
public class PlayerRotate : MonoBehaviour
{
    public float RotationSpeed = 200f; // CameraRotate와 같은 값으로 맞춘다.

    private float _accumulationX;

    private void Update()
    {
        // 커서가 풀려 있으면(UI를 누르는 중이면) 시선을 돌리지 않는다.
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        if (GameManager.Instance.State != GameState.Playing) return;
        var mouseX = Input.GetAxis("Mouse X");
        _accumulationX += mouseX * RotationSpeed * Time.deltaTime;

        transform.eulerAngles = new Vector3(0f, _accumulationX, 0f);
    }
}