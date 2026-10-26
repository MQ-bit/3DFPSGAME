using System;
using UnityEngine;

// 무언가에 부딛히면 폭발 이펙트를 남기고 사라지는 폭탄
[RequireComponent(typeof(Rigidbody))]
public class Bomb : MonoBehaviour
{
    private const int ExplosionEffectPoolSize = 6;

    [SerializeField] private ParticleSystem _explosionEffectPrefab;
    [SerializeField] private float _explosionRange = 4.5f;
    [SerializeField] private float _damage = 100;

    private Rigidbody _rigidbody;
    private bool _exploded;

    // 폭발이 끝났을 때 풀(PlayerBombFire)이 구독한다. 구독자가 없으면 스스로 Destroy 한다.
    public event Action<Bomb> OnExploded;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // 풀에서 꺼낼 때 처음 만들어진 상태로 되돌린다.
    public void Init(Vector3 position)
    {
        _exploded = false;

        transform.SetPositionAndRotation(position, Quaternion.identity);

        // 이전에 날아가던 속도가 남지 않도록 지운다.
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    public void Throw(Vector3 force)
    {
        _rigidbody.AddForce(force, ForceMode.Impulse);
    }

    // 다른 콜라이더와 부딛힌 순간 실행되는 메서드
    private void OnCollisionEnter(Collision collision)
    {
        // 한 물리 스텝에 여러 번 불려도 한 번만 폭발한다. (이중 Release 방지)
        if (_exploded) return;
        _exploded = true;

        EffectManager.Instance.Play(_explosionEffectPrefab, transform.position, Quaternion.identity, 1f, ExplosionEffectPoolSize);

        ApplyAreaDamage();

        // 풀에 반납한다. 구독자가 없으면 삭제한다.
        if (OnExploded == null)
        {
            Destroy(gameObject);
            return;
        }

        OnExploded.Invoke(this);
    }

    private void ApplyAreaDamage()
    {
        var colliders = Physics.OverlapSphere(transform.position, _explosionRange, LayerMask.GetMask("Drum", "Zombie"));

        foreach (var col in colliders)
        {
            DamageInfo damageInfo = new()
            {
                Value = _damage,
                HitPoint = col.ClosestPoint(transform.position), // 폭발 중심에서 가장 가까운 콜라이더 표면 위치
                HitNormal = (transform.position - col.ClosestPoint(transform.position)).normalized
            };


            col.GetComponent<IDamageable>()?.TakeDamage(damageInfo);
        }
    }
}
