using UnityEngine;

// 타겟(CamerPosition)을 따라다닌다. 근접 모드(WeaponMode.Melee)에서는 3인칭으로 뒤로 물러난다.
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;

    [Header("3인칭 (근접 모드)")]
    [SerializeField] private float _distance = 3f;           // 시선 방향 뒤로 물러나는 거리 (m)
    [SerializeField] private float _height = 0.4f;           // 위로 올리는 높이 (m)
    [SerializeField] private float _shoulderOffset = 0.5f;   // 오른쪽 어깨 쪽으로 비키는 거리 (m)
    [SerializeField] private float _blendTime = 0.25f;       // 1인칭 <-> 3인칭 전환에 걸리는 시간 (초)
    [SerializeField] private float _collisionRadius = 0.2f;  // 벽 검사에 쓰는 구의 반지름 (m)
    [SerializeField] private LayerMask _obstacleMask;        // 카메라를 가로막는 것 (Default)

    private Player _player;
    private float _blend;  // 0: 1인칭, 1: 3인칭

    private void Awake()
    {
        if (_target == null)
        {
            Debug.LogWarning("[CameraFollow] 카메라 타겟이 없습니다!!");
            return;
        }

        _player = _target.GetComponentInParent<Player>();  // 타겟은 Player의 자식이다.
    }

    private void LateUpdate()
    {
        // 근접 모드이면 3인칭 쪽으로, 아니면 1인칭 쪽으로 서서히 옮긴다.
        var isThirdPerson = _player != null && _player.WeaponMode == WeaponMode.Melee;
        _blend = Mathf.MoveTowards(_blend, isThirdPerson ? 1f : 0f, Time.deltaTime / _blendTime);

        var origin = _target.position;
        var offset = (-transform.forward * _distance + Vector3.up * _height + transform.right * _shoulderOffset) * Mathf.SmoothStep(0f, 1f, _blend);

        // 카메라가 벽을 뚫지 않도록, 벽에 막히면 그 앞까지만 물러난다.
        if (offset.sqrMagnitude > 0.0001f
            && Physics.SphereCast(origin, _collisionRadius, offset.normalized, out RaycastHit hit, offset.magnitude, _obstacleMask, QueryTriggerInteraction.Ignore))
        {
            offset = offset.normalized * hit.distance;
        }

        transform.position = origin + offset;
    }
}
