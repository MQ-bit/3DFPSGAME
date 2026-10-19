using UnityEngine;

// 목표(Target)의 위치를 따라다니는 카메라
public class CameraFollow : MonoBehaviour
{
    public Transform Target; // 따라갈 대상 (Player의 CameraPosition)

    private Vector3 _positionOffset;
    private bool _followPlayerYaw;
    private Transform _player;

    public void SetViewSettings(Vector3 positionOffset, bool followPlayerYaw, Transform player)
    {
        _positionOffset = positionOffset;
        _followPlayerYaw = followPlayerYaw;
        _player = player;
    }

    private void LateUpdate()
    {
        if (Target == null)
        {
            return;
        }

        // 1인칭의 기본 오프셋은 0이므로 기존처럼 눈 위치를 그대로 따라간다.
        // 다른 시점에서는 눈 위치에 시점별 오프셋을 더한다.
        Transform yawSource = _player != null ? _player : Target;
        Quaternion referenceRotation = _followPlayerYaw
            ? Quaternion.Euler(0f, yawSource.eulerAngles.y, 0f)
            : Quaternion.identity;

        transform.position = Target.position + referenceRotation * _positionOffset;
    }
}
