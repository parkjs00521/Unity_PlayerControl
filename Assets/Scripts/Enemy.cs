using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Enemy : MonoBehaviour, IDamageable
{
    private float currentHp = 100f;
    private bool isDead = false;
    private Rigidbody rb;

    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private float attackCooldown = 10f;
    private float lastAttackTime = -999f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHp -= damage;
        Debug.Log($"{gameObject.name} HP: {currentHp}");

        if (currentHp <= 0) Die();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;

        if (collision.gameObject.TryGetComponent<IDamageable>(out var player))
        {
            player.TakeDamage((int)attackDamage);
            Debug.Log("플레이어에게 데미지!");
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} 사망");
        Destroy(gameObject);
    }
}