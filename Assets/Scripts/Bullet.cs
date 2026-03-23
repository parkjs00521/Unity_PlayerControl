using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 20f;        // 총알 속도
    [SerializeField] private float lifetime = 3f;      // 자동 회수 시간
    [SerializeField] private int damage = 10;          // 데미지

    private float timer;

    void OnEnable()
    {
        // 활성화될 때마다 타이머 초기화
        timer = lifetime;
    }

    void Update()
    {
        // 앞 방향으로 이동
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            ReturnToPool();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var target))
        {
            Debug.Log("총알맞음");
            target.TakeDamage(damage);
        }
    }

    void ReturnToPool()
    {
        BulletManager.Instance.ReturnBullet(gameObject);
    }
}

public interface IDamageable
{
    void TakeDamage(int damage);
}