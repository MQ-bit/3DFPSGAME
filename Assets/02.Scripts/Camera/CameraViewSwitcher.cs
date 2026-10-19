using System;
using UnityEngine;

// 시점을 선택하고 위치·방향 설정을 전달한다. 실제 이동과 회전은 각 담당 컴포넌트가 처리한다.
[DisallowMultipleComponent]
[RequireComponent(typeof(CameraFollow), typeof(CameraRotate))]
public class CameraViewSwitcher : MonoBehaviour
{
    public enum CameraView
    {
        FirstPerson,
        ThirdPerson,
        Shoulder,
        Quarter,
        TopDown,
        Side
    }

    [Serializable]
    private class ViewSettings
    {
        [SerializeField, Tooltip("CameraPosition을 기준으로 한 위치 오프셋")]
        private Vector3 _positionOffset;

        [SerializeField, Tooltip("카메라가 바라보는 방향의 오일러 각도(X: 상하, Y: 좌우, Z: 기울기)")]
        private Vector3 _rotationEuler;

        [SerializeField, Tooltip("켜면 위치와 회전이 Player의 Y축 방향을 따르고, 끄면 월드 축을 기준으로 합니다.")]
        private bool _followPlayerYaw;

        public Vector3 PositionOffset => _positionOffset;
        public Vector3 RotationEuler => _rotationEuler;
        public bool FollowPlayerYaw => _followPlayerYaw;

        public ViewSettings(Vector3 positionOffset, Vector3 rotationEuler, bool followPlayerYaw)
        {
            _positionOffset = positionOffset;
            _rotationEuler = rotationEuler;
            _followPlayerYaw = followPlayerYaw;
        }
    }

    [Header("추적 대상")]
    [SerializeField, Tooltip("Player의 자식 CameraPosition. 비어 있으면 CameraFollow.Target을 사용합니다.")]
    private Transform _target;

    [SerializeField, Tooltip("몸 방향을 제공하는 Player. 비어 있으면 CameraPosition의 부모를 사용합니다.")]
    private Transform _player;

    [Header("시점 전환")]
    [SerializeField] private KeyCode _switchKey = KeyCode.V;
    [SerializeField] private CameraView _currentView = CameraView.FirstPerson;

    [Header("시점별 위치와 방향")]
    // 1인칭: 눈 위치에 배치하고 몸의 좌우 회전과 마우스의 상하 회전을 반영한다.
    [SerializeField]
    private ViewSettings _firstPerson = new ViewSettings(
        Vector3.zero, Vector3.zero, true);

    // 3인칭: 눈 위치보다 위쪽, 뒤쪽으로 이동하고 아래를 바라본다.
    [SerializeField]
    private ViewSettings _thirdPerson = new ViewSettings(
        new Vector3(0f, 1f, -4f), new Vector3(15f, 0f, 0f), true);

    // 숄더뷰: 오른쪽 어깨 뒤로 이동하고 몸이 향하는 방향을 바라본다.
    [SerializeField]
    private ViewSettings _shoulder = new ViewSettings(
        new Vector3(0.7f, 0.2f, -2f), new Vector3(5f, 0f, 0f), true);

    // 쿼터뷰: 대각선 위로 이동하고 월드 기준 대각선 아래 방향을 바라본다.
    [SerializeField]
    private ViewSettings _quarter = new ViewSettings(
        new Vector3(-6f, 6f, -6f), new Vector3(35.264f, 45f, 0f), false);

    // 탑다운: 머리 위로 이동하고 수직 아래를 바라본다.
    [SerializeField]
    private ViewSettings _topDown = new ViewSettings(
        new Vector3(0f, 10f, 0f), new Vector3(90f, 0f, 0f), false);

    // 사이드뷰: 월드 X축 옆으로 이동하고 플레이어 쪽을 수평으로 바라본다.
    [SerializeField]
    private ViewSettings _side = new ViewSettings(
        new Vector3(8f, 0f, 0f), new Vector3(0f, -90f, 0f), false);

    private CameraRotate _cameraRotate;
    private CameraFollow _cameraFollow;

    public CameraView CurrentView => _currentView;

    private void Awake()
    {
        _cameraRotate = GetComponent<CameraRotate>();
        _cameraFollow = GetComponent<CameraFollow>();

        if (_target == null && _cameraFollow != null)
        {
            _target = _cameraFollow.Target;
        }

    }

    private void OnEnable()
    {
        ApplyView(_currentView);
    }

    private void Update()
    {
        if (Input.GetKeyDown(_switchKey))
        {
            _currentView = (CameraView)(((int)_currentView + 1) % 6);
            ApplyView(_currentView);
        }
    }

    private void ApplyView(CameraView view)
    {
        if (_cameraFollow == null || _cameraRotate == null)
        {
            return;
        }

        if (_target != null)
        {
            _cameraFollow.Target = _target;
        }

        Transform target = _cameraFollow.Target;
        Transform player = _player != null ? _player : target != null ? target.parent : null;
        Transform yawSource = player != null ? player : target;
        ViewSettings settings = GetViewSettings(view);

        // 시점이 바뀔 때 설정만 전달한다. Transform은 여기서 변경하지 않는다.
        _cameraFollow.SetViewSettings(settings.PositionOffset, settings.FollowPlayerYaw, yawSource);
        _cameraRotate.SetViewSettings(view == CameraView.FirstPerson,
            settings.RotationEuler, settings.FollowPlayerYaw, yawSource);
    }

    private ViewSettings GetViewSettings(CameraView view)
    {
        switch (view)
        {
            case CameraView.ThirdPerson: return _thirdPerson;
            case CameraView.Shoulder: return _shoulder;
            case CameraView.Quarter: return _quarter;
            case CameraView.TopDown: return _topDown;
            case CameraView.Side: return _side;
            default: return _firstPerson;
        }
    }

    private void OnDisable()
    {
        // 전환 기능을 끄면 기존 두 컴포넌트가 기본 1인칭 동작을 이어간다.
        ApplyView(CameraView.FirstPerson);
    }
}
