using TMPro;
using UnityEngine;

public class UI_ZombieCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private ZombieSpawner _spawner;

    private void Start()
    {
        // 좀비 수 변화 이벤트 구독!
        _spawner.OnCountChanged += Refresh;

        Refresh();
    }

    private void OnDestroy()
    {
        _spawner.OnCountChanged -= Refresh;
    }

    private void Refresh()
    {
        _countText.text = $"남은 좀비 {_spawner.AliveCount}";
    }
}
