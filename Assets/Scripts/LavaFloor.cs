using UnityEngine;

public class LavaFloor : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 5;           
    [SerializeField] private float damageCool = 0.5f; 

    private float lastDamageTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log("용암 구역 진입!");
    }

    // 구역 안에 있는 동안 지속 데미지
    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - lastDamageTime < damageCool) return;

        if (other.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(damage);
            lastDamageTime = Time.time;
            Debug.Log($"용암 데미지! -{damage}");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log("용암 구역 탈출!");
    }

}
