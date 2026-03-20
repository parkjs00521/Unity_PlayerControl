using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHp = 100f;
    private float currentHp;
    private bool isDead = false;

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public bool IsDead => isDead;

    void Awake()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0f);
        Debug.Log($"플레이어 HP: {currentHp}");

        if (currentHp <= 0) Die();
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("플레이어 사망");
    }
}