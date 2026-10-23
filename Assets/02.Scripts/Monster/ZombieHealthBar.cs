using UnityEngine;
using UnityEngine.UI;

public class ZombieHealthBar : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    private Zombie _zombie;


    private void Awake()
    {
        _zombie = GetComponent<Zombie>();
    }

    private void LateUpdate()
    {
        //0~100%
        _slider.value = _zombie.Health / _zombie.MaxHealth;

        transform.forward = Camera.main.transform.forward;
    }
}