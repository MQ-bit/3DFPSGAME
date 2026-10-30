using UnityEngine;

// 칼날 끝에만 보이는 이펙트: 칼끝 글로우(검 메시에 겹쳐 그림)와 칼끝 잔상(트레일).
// 칼날 판정이 켜져 있는 동안(ActivateCollider ~ DeactivateCollider) 가장 강하게 빛난다.
[RequireComponent(typeof(BoxCollider))]
public class MeleeBladeEffect : MonoBehaviour
{
    [Header("재질")]
    [SerializeField] private Material _glowMaterial;   // Custom/BladeTipGlow
    [SerializeField] private Material _trailMaterial;  // Custom/SwordTrail

    [Header("칼끝")]
    [SerializeField] private Vector3 _tipLocalPosition = new Vector3(0f, 0.98f, 0f);  // 칼끝 위치 (검 로컬)

    [Header("글로우 세기")]
    [SerializeField] private float _idleIntensity = 0.25f;    // 판정이 없을 때
    [SerializeField] private float _activeIntensity = 2.2f;   // 판정이 있을 때
    [SerializeField] private float _intensitySpeed = 14f;     // 세기가 바뀌는 속도

    [Header("잔상")]
    [SerializeField] private float _trailTime = 0.22f;        // 잔상이 남아 있는 시간 (초)
    [SerializeField] private float _trailWidth = 0.2f;        // 칼끝에서의 폭 (m)
    [SerializeField] private float _trailExtraTime = 0.06f;   // 판정이 꺼진 뒤에도 잔상을 더 그리는 시간 (초)

    private BoxCollider _collider;
    private Material _glow;
    private TrailRenderer _trail;
    private float _intensity;
    private float _trailTimer;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        CreateGlow();
        CreateTrail();
    }

    private void OnDestroy()
    {
        if (_glow != null) Destroy(_glow);
    }

    // 검 메시를 한 번 더 그리는 자식을 만들어 칼끝 글로우 재질을 입힌다.
    private void CreateGlow()
    {
        var glowObject = new GameObject("BladeGlow");
        glowObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        glowObject.transform.SetParent(transform, false);

        glowObject.AddComponent<MeshFilter>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
        var meshRenderer = glowObject.AddComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        _glow = new Material(_glowMaterial);  // 세기를 따로 바꾸므로 복사본을 쓴다.
        meshRenderer.sharedMaterial = _glow;
    }

    // 칼끝 위치에 트레일을 단다.
    private void CreateTrail()
    {
        var trailObject = new GameObject("TipTrail");
        trailObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        trailObject.transform.SetParent(transform, false);
        trailObject.transform.localPosition = _tipLocalPosition;

        _trail = trailObject.AddComponent<TrailRenderer>();
        _trail.sharedMaterial = _trailMaterial;
        _trail.time = _trailTime;
        _trail.widthMultiplier = _trailWidth;
        _trail.widthCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));  // 칼끝은 굵고 꼬리는 가늘게
        _trail.minVertexDistance = 0.02f;
        _trail.alignment = LineAlignment.View;
        _trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _trail.receiveShadows = false;
        _trail.emitting = false;

        // 꼬리로 갈수록 알파가 줄어든다. (셰이더가 정점 색의 알파를 쓴다)
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.95f, 0.7f), 0f), new GradientColorKey(new Color(1f, 0.45f, 0.1f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        _trail.colorGradient = gradient;
    }

    private void OnEnable()
    {
        // 검이 다시 나타날 때 예전 위치에서 길게 이어진 잔상이 남지 않도록 비운다.
        if (_trail != null) _trail.Clear();
        _trailTimer = 0f;
    }

    private void Update()
    {
        var isActive = _collider.enabled;

        // 글로우: 판정이 있는 동안은 세게, 아니면 은은하게 서서히 바꾼다.
        var target = isActive ? _activeIntensity : _idleIntensity;
        _intensity = Mathf.MoveTowards(_intensity, target, _intensitySpeed * Time.deltaTime);
        _glow.SetFloat("_Intensity", _intensity);

        // 잔상: 판정이 있는 동안과 그 직후 잠깐만 그린다.
        _trailTimer = isActive ? _trailExtraTime : _trailTimer - Time.deltaTime;
        _trail.emitting = _trailTimer > 0f;
    }
}
