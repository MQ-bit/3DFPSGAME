using UnityEngine;
using UnityEngine.UI;

public class ZombieHealthBar : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    private Zombie _zombie;


    private void Awake()
    {
        _zombie = GetComponent<Zombie>();
        _zombie.OnDamaged += RefreshHealth;
    }

    private void Start()
    {
        RefreshHealth();
    }

    private void OnDestroy()
    {
        _zombie.OnDamaged -= RefreshHealth;
    }

    private void LateUpdate()
    {
        // 카메라는 매 프레임 움직이므로 빌보드 회전만 폴링한다.
        _slider.transform.forward = Camera.main.transform.forward;
    }

    private void RefreshHealth()
    {
        //0~100%
        _slider.value = _zombie.Health / _zombie.MaxHealth;
    }
}
