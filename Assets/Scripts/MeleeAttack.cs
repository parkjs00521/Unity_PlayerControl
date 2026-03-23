using UnityEngine;
using System.Collections;

public class MeleeAttack : MonoBehaviour
{
    [Header("Hitbox Settings")]
    [SerializeField] private Vector3 hitboxOffset = new Vector3(0f, 0f, 1f);
    [SerializeField] private Vector3 hitboxSize = new Vector3(1f, 1f, 1f);
    [SerializeField] private LayerMask enemyLayer;

    [Header("Combo Damage")]
    [SerializeField] private int firstDamage  = 25;
    [SerializeField] private int secondDamage = 45;
    [SerializeField] private int thirdDamage  = 70;

    [Header("Timing Settings")]
    [SerializeField] private float attackCooldown = 0.3f;
    [SerializeField] private float comboWindow    = 3.0f;

    [Header("Cant Move Time")]
    [SerializeField] private float firstLockTime  = 0.4f;  // 1타 이동 잠금 시간
    [SerializeField] private float secondLockTime = 0.4f;  // 2타 이동 잠금 시간
    [SerializeField] private float thirdLockTime  = 0.6f;  // 3타 이동 잠금 시간

    [Header("Animation")]
    [SerializeField] private Animator anim;
    [SerializeField] private string firstAttackTrigger  = "DoPunch1";
    [SerializeField] private string secondAttackTrigger = "DoPunch2";
    [SerializeField] private string thirdAttackTrigger  = "DoPunch3";

    private PlayerController playerController;
    private int comboStep = 0;
    private float lastAttackTime = -999f;

    private bool isAttacking = false;
    private float gizmosTimer = 0f;
    [SerializeField] private float gizmosDisplayTime = 0.2f;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (!playerController.isArmed && Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }

        if (comboStep > 0 && Time.time - lastAttackTime > comboWindow)
        {
            ResetCombo();
        }

        if (isAttacking)
        {
            gizmosTimer -= Time.deltaTime;
            if (gizmosTimer <= 0f) isAttacking = false;
        }
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown || playerController.CurrentStamina <= 0) return;

        switch (comboStep)
        {
            case 0:
                PerformAttack(firstDamage, firstAttackTrigger, firstLockTime);
                playerController.ConsumeStamina(20);
                comboStep = 1;
                break;
            case 1:
                PerformAttack(secondDamage, secondAttackTrigger, secondLockTime);
                playerController.ConsumeStamina(30);
                comboStep = 2;
                break;
            case 2:
                PerformAttack(thirdDamage, thirdAttackTrigger, thirdLockTime);
                playerController.ConsumeStamina(50);
                comboStep = 0;
                break;
        }

        lastAttackTime = Time.time;
    }

    private void PerformAttack(int damage, string triggerName, float lockTime)
    {
        if (anim != null) anim.SetTrigger(triggerName);

        isAttacking = true;
        gizmosTimer = gizmosDisplayTime;

        StartCoroutine(LockMovement(lockTime));

        Vector3 hitboxCenter = transform.TransformPoint(hitboxOffset);
        Collider[] hits = Physics.OverlapBox(
            hitboxCenter,
            hitboxSize / 2,
            transform.rotation,
            enemyLayer
        );

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var enemy))
            {
                enemy.TakeDamage(damage);
                Debug.Log($"{triggerName} 적중! 데미지: {damage}");
            }
        }
    }

    private IEnumerator LockMovement(float duration)
    {
        playerController.canMove = false;
        yield return new WaitForSeconds(duration);
        playerController.canMove = true;
    }

    private void ResetCombo()
    {
        comboStep = 0;
        Debug.Log("콤보 초기화");
    }

    void OnDrawGizmos()
    {
        Vector3 hitboxCenter = transform.TransformPoint(hitboxOffset);

        if (isAttacking)
        {
            Gizmos.color = comboStep switch
            {
                1 => new Color(1f, 0f, 0f, 0.5f),
                2 => new Color(1f, 0.5f, 0f, 0.5f),
                0 => new Color(1f, 0f, 1f, 0.5f),
                _ => new Color(1f, 0f, 0f, 0.5f)
            };
            Gizmos.matrix = Matrix4x4.TRS(hitboxCenter, transform.rotation, Vector3.one);
            Gizmos.DrawCube(Vector3.zero, hitboxSize);
            Gizmos.DrawWireCube(Vector3.zero, hitboxSize);
        }
        else
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.matrix = Matrix4x4.TRS(hitboxCenter, transform.rotation, Vector3.one);
            Gizmos.DrawCube(Vector3.zero, hitboxSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, hitboxSize);
        }

        Gizmos.matrix = Matrix4x4.identity;
    }
}