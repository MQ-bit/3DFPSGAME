using UnityEngine;
using UnityEngine.Pool;

// 마우스 올느쪽 버튼을 누르면 시선이 바라보는 방향으로 수류탄을 던지고 싶다.
public class PlayerBombFire : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Bomb _bombPrefab;
    [SerializeField] private float _throwPower = 15f;

    [Header("풀")]
    [SerializeField] private int _poolMaxSize = 20;   // 보관할 수 있는 폭탄 수. 넘치면 반납할 때 삭제한다.

    private ObjectPool<Bomb> _pool;

    private void Awake()
    {
        _pool = new ObjectPool<Bomb>(
            createFunc: CreateBomb,
            actionOnGet: bomb => bomb.gameObject.SetActive(true),
            actionOnRelease: bomb => bomb.gameObject.SetActive(false),
            actionOnDestroy: bomb => Destroy(bomb.gameObject),
            defaultCapacity: _poolMaxSize / 2,
            maxSize: _poolMaxSize);
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;
        // 1. 마우스 오른쪽 버튼 입력 받기
        if (Input.GetButtonDown("Fire2")) Shoot();
    }

    private void Shoot()
    {
        // 2. 발사 위치에서 수류탄 꺼내기
        var bomb = _pool.Get();
        bomb.OnExploded += Release;
        bomb.Init(_firePoint.position);

        // 3. 꺼낸 수류탄을 시선 방향으로 던지기
        bomb.Throw(Camera.main.transform.forward * _throwPower);
    }

    private Bomb CreateBomb()
    {
        return Instantiate(_bombPrefab);
    }

    private void Release(Bomb bomb)
    {
        bomb.OnExploded -= Release;
        _pool.Release(bomb);
    }
}
