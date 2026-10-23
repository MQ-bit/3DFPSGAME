using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [Header("스탯")][SerializeField] private float _maxHealth = 100f;

    [SerializeField] private UI_HitEffect _uiHitEffect;

    public float MaxHealth => _maxHealth;
    public float Health { get; private set; }

    private void Start()
    {
        Health = _maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (Health <= 0f) return;

        Health -= damageInfo.Value;

        _uiHitEffect.Fade(0.7f);

        Debug.Log($"플레이어가 맞았다. 대미지 {damageInfo.Value}, 남은 체력 {Health}");
        if (Health <= 0f)
        {
            Debug.Log("플레이어가 쓰러졌다!");
            // Todo. 게임 오버
        }
    }
}