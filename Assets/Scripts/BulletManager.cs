using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance { get; private set; }

    [Header("Mag Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int magSize = 20;
    [SerializeField] private float reloadTime = 2f;
    [Header("AudioClip Settings")]
    [SerializeField] private AudioClip reloadClip;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private int currentMag;
    private bool isReloading = false;

    public int CurrentMag   => currentMag;
    public int MagSize      => magSize;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();


        currentMag = magSize;   
        InitializePool();
    }

    void Update()
    {
        // R키 재장전
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentMag < magSize)
        {
            StartCoroutine(Reload());
        }
    }

    public GameObject GetBullet()
    {
        if (isReloading)
        {
            return null;
        }

        if (currentMag <= 0)
        {
            Debug.Log("탄약 없음! 재장전중...");
            StartCoroutine(Reload());   // 탄약 소진 시 자동 재장전
            return null;
        }

        currentMag--;
        Debug.Log($"탄약: {currentMag} / {magSize}");

        if (pool.Count == 0) return CreateNewBullet();

        GameObject bullet = pool.Dequeue();
        bullet.SetActive(true);
        return bullet;
    }


    private IEnumerator Reload()
    {
        isReloading = true;
        audioSource.PlayOneShot(reloadClip);

        yield return new WaitForSeconds(reloadTime);

        currentMag = magSize;
        isReloading = false;
        Debug.Log("재장전 완료!");
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bullet.transform.SetParent(transform);
        pool.Enqueue(bullet);
    }

    void InitializePool()
    {
        for (int i = 0; i < magSize; i++)
            pool.Enqueue(CreateNewBullet());
    }

    GameObject CreateNewBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, transform);
        bullet.SetActive(false);
        return bullet;
    }
}