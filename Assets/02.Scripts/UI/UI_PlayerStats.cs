using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStats : MonoBehaviour
{
    // Todo. ZombieHealthBar를 이용해서 체력과 스태미나 UI 구현
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _staminaSlider;

    [Header("체력바 잔상")]
    [SerializeField] private Slider _healthTrailSlider;  // 체력바 뒤에서 늦게 줄어드는 잔상 바
    [SerializeField] private float _trailDelay = 0.4f;   // 맞은 뒤 잔상이 줄어들기 시작할 때까지 기다리는 시간 (초)
    [SerializeField] private float _trailSpeed = 0.5f;   // 잔상이 줄어드는 속도 (초당 체력 비율)

    [SerializeField] private Player _player;
    [SerializeField] private PlayerMove _playerMove;

    private float _trailTimer;

    private void Start()
    {
        // 플레이어 공격 이벤트 구독!
        _player.OnDamaged += RefreshHealth;


        // 맞기 전에도 현재 체력으로 체력바를 채워 둔다.
        RefreshHealth();
        _healthTrailSlider.value = _healthSlider.value;  // 시작할 때는 잔상이 없다.
    }

    // 업데이트가 초당 60회 이상은 실행인된다.
    private void Update()
    {
        // RefreshHealth(); // 폴링

        _staminaSlider.value = _playerMove.Stamina / PlayerMove.MaxStamina;
        UpdateHealthTrail();
    }

    // 잔상 바가 체력바보다 높으면, 잠깐 기다렸다가 체력바 값까지 천천히 내려온다.
    private void UpdateHealthTrail()
    {
        if (_healthTrailSlider.value <= _healthSlider.value) return;

        if (_trailTimer > 0f)
        {
            _trailTimer -= Time.deltaTime;
            return;
        }

        _healthTrailSlider.value = Mathf.MoveTowards(_healthTrailSlider.value, _healthSlider.value, _trailSpeed * Time.deltaTime);
    }

    private void RefreshHealth()
    {
        // 문제: 체력값이 변하지 않았음에도 초당 60회 이상 캔버스를 불필요하게 다시 그린다. -> 성능 낭비가 일어난다.
        // 요고 하나 정도는 괜찮아 보이지만.. 이런 불필요한 연산이 다른 UI에서 누적될수록 느려진다.
        // 해결: 값이 바뀌었을때만 캔버스 갱신을한다.

        // 이때쓰는게: 옵저버 패턴
        // 옵저버 패턴은 한 객체(주체)의 상태가 바뀔 때 이를 구독 중인 여러 객체(관찰자)에게 자동으로 알려주는 패턴으로, 서로 직접 참조하지 않아 결합도가 낮아진다.
        // 유튜브 구독 패턴은 유튜버가 영상을 올릴 때 이를 구독 중인 여러 구독자에 알려주는 패턴이다.
        // C#에서는 이것을 Action으로 구현할 수 있다.

        float healthRatio = _player.Health / _player.MaxHealth;

        if (healthRatio >= _healthTrailSlider.value)
        {
            _healthTrailSlider.value = healthRatio;  // 체력이 늘었을 때는 잔상도 바로 따라간다.
        }
        else
        {
            _trailTimer = _trailDelay;  // 체력이 줄었으면 잠깐 기다린 뒤 잔상이 따라 내려온다.
        }

        _healthSlider.value = healthRatio;
    }


}