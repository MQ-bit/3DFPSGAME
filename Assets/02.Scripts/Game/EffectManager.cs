using System.Collections.Generic;
using UnityEngine;

// 이펙트 프리팹별로 링 버퍼 풀을 하나씩 두고 공유한다.
// (드럼통처럼 여러 개인 오브젝트가 각자 풀을 갖지 않도록 한 곳에서 관리한다.)
public class EffectManager : MonoBehaviour
{
    private const int DefaultPoolSize = 8;

    public static EffectManager Instance { get; private set; }

    private readonly Dictionary<ParticleSystem, RingBufferPool<ParticleSystem>> _pools = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // 이펙트를 꺼내 재생한다. poolSize 는 그 프리팹의 풀을 처음 만들 때만 쓰인다.
    public void Play(ParticleSystem prefab, Vector3 position, Quaternion rotation, float scale = 1f, int poolSize = DefaultPoolSize)
    {
        if (!_pools.TryGetValue(prefab, out var pool))
        {
            pool = new RingBufferPool<ParticleSystem>(prefab, poolSize, transform, DisableAutoDestroy);
            _pools.Add(prefab, pool);
        }

        var effect = pool.Get();

        // 재생 중이던 것이 돌아왔을 수 있으니 끊고 시작한다.
        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        effect.transform.SetPositionAndRotation(position, rotation);
        effect.transform.localScale = Vector3.one * scale;
        effect.Play();
    }

    // 프리팹의 Stop Action 이 Destroy 라면 풀에 있는 이펙트가 끝나면서 스스로 사라진다.
    // 서드파티 프리팹을 고치지 않고 생성 직후 코드에서 끈다.
    private static void DisableAutoDestroy(ParticleSystem effect)
    {
        foreach (var particle in effect.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = particle.main;
            main.stopAction = ParticleSystemStopAction.None;
        }
    }
}
