using UnityEngine;
using UnityEngine.Pool;

public class ZombieSpawner : MonoBehaviour
{
    [Header("스폰")]
    [SerializeField] private Zombie _zombiePrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _spawnInterval = 3f;   // 스폰 간격
    [SerializeField] private int _maxAliveCount = 10;     // 동시에 살아있을 수 있는 최대 좀비 수

    private ObjectPool<Zombie> _pool;
    private float _spawnTimer = 0f;

    private void Awake()
    {
        _pool = new ObjectPool<Zombie>(
            createFunc: CreateZombie,
            actionOnGet: zombie => zombie.gameObject.SetActive(true),
            actionOnRelease: zombie => zombie.gameObject.SetActive(false),
            actionOnDestroy: zombie => Destroy(zombie.gameObject),
            defaultCapacity: _maxAliveCount,
            maxSize: _maxAliveCount);
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        _spawnTimer += Time.deltaTime;
        if (_spawnTimer < _spawnInterval) return;

        _spawnTimer = 0f;
        Spawn();
    }

    private void Spawn()
    {
        if (_spawnPoints.Length == 0) return;
        if (_pool.CountActive >= _maxAliveCount) return;

        Transform point = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

        Zombie zombie = _pool.Get();
        zombie.OnDeath += Release;
        zombie.Init(point.position, point.rotation);
    }

    private Zombie CreateZombie()
    {
        return Instantiate(_zombiePrefab, transform);
    }

    private void Release(Zombie zombie)
    {
        zombie.OnDeath -= Release;
        _pool.Release(zombie);
    }
}
