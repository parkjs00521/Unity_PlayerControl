using UnityEngine;

public enum ItemType
{
    Consumable,
    Ammo
}
[System.Serializable]
public struct ItemData
{
    public string itemName;
    public ItemType itemType;
    public int value;
}

public class WorldItems : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private ItemData itemData;

    private float rotateSpeed = 90f;
    private float floatSpeed  = 1f;
    private float floatHeight = 0.3f;
    private Vector3 startPos;
    private bool isPickedUp = false;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (isPickedUp) return;

        // 회전 + 부유 연출
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isPickedUp) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        if (collision.gameObject.TryGetComponent<IItemCollector>(out var collector))
        {
            collector.CollectItem(itemData);
            PickUp();
        }
    }

    private void PickUp()
    {
        isPickedUp = true;
        Debug.Log($"{itemData.itemName} 획득!");
        Destroy(gameObject);
    }
}

public interface IItemCollector
{
    void CollectItem(ItemData item);
}