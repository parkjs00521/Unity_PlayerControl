using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance { get; private set; }

    [Header("탄창 설정")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int MagSize = 20;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializePool();
    }

    // 풀 초기화 - 미리 총알 생성
    void InitializePool()
    {
        for (int i = 0; i < MagSize; i++)
        {
            GameObject bullet = CreateNewBullet();
            pool.Enqueue(bullet);
        }
    }

    // 총알 생성 (비활성 상태로)
    GameObject CreateNewBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, transform);
        bullet.SetActive(false);
        return bullet;
    }

    // 풀에서 총알 꺼내기
    public GameObject GetBullet()
    {
        // 풀이 비었으면 새로 생성 (자동 확장)
        if (pool.Count == 0)
        {
            Debug.Log("풀 확장 - 새 총알 생성");
            return CreateNewBullet();
        }

        GameObject bullet = pool.Dequeue();
        bullet.SetActive(true);
        return bullet;
    }

    // 총알 반환 (비활성화 후 풀에 다시 넣기)
    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bullet.transform.SetParent(transform);
        pool.Enqueue(bullet);
    }
}