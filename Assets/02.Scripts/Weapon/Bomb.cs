using UnityEngine;

// 무언가에 부딪히면 폭발 이펙트를 남기고 사라지는 폭탄
public class Bomb : MonoBehaviour
{
    [SerializeField] private ParticleSystem _explosionEffectPrefab; // 폭발 이펙트 프리팹

    // 다른 콜라이더와 부딪힌 순간 한 번 불린다.
    private void OnCollisionEnter(Collision collision)
    {
        // 1. 폭발 이펙트를 만들어 폭탄 위치로 옮기고 재생한다.
        ParticleSystem effect = Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);
        effect.Play();

        // Todo. 오브젝트 풀링 적용
        // 폭탄을 삭제한다.
        Destroy(gameObject);
    }
}