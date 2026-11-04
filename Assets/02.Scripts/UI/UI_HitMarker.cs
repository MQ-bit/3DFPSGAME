using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 총알이 대미지를 받는 대상에 맞으면 크로스헤어 가운데에 X 표시를 번쩍이고 서서히 지운다.
// 맞히기만 했으면 흰색, 이 총알로 쓰러뜨렸으면 빨간색으로 보여 준다.
[RequireComponent(typeof(CanvasGroup))]
public class UI_HitMarker : MonoBehaviour
{
    [SerializeField] private PlayerGunFire _gunFire;
    [SerializeField] private float _fadeTime = 0.15f;   // 표시가 사라지는 데 걸리는 시간 (초)
    [SerializeField] private float _startScale = 1.4f;  // 맞은 순간의 크기. 점점 1배로 줄어든다.
    [SerializeField] private Color _hitColor = Color.white;  // 맞혔을 때 색
    [SerializeField] private Color _killColor = Color.red;   // 쓰러뜨렸을 때 색

    private CanvasGroup _canvasGroup;
    private Image[] _ticks;
    private Coroutine _showCoroutine;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _ticks = GetComponentsInChildren<Image>();
    }

    private void Start()
    {
        _gunFire.OnHitConfirmed += Show;
    }

    private void OnDestroy()
    {
        if (_gunFire != null)
        {
            _gunFire.OnHitConfirmed -= Show;
        }
    }

    private void Show(bool isKill)
    {
        SetColor(isKill ? _killColor : _hitColor);

        // 연속으로 맞으면 처음부터 다시 번쩍인다.
        if (_showCoroutine != null)
        {
            StopCoroutine(_showCoroutine);
        }

        _showCoroutine = StartCoroutine(Show_Coroutine());
    }

    private void SetColor(Color color)
    {
        foreach (Image tick in _ticks)
        {
            tick.color = color;
        }
    }

    private IEnumerator Show_Coroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < _fadeTime)
        {
            float t = elapsedTime / _fadeTime;
            _canvasGroup.alpha = 1f - t;
            transform.localScale = Vector3.one * Mathf.Lerp(_startScale, 1f, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        transform.localScale = Vector3.one;
        _showCoroutine = null;
    }
}
