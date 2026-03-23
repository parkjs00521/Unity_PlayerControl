using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Slider HpSlider;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Player player;
    [SerializeField] private BulletManager bulletManager;
    [SerializeField] private Text magText;

    void Update()
    {
        if (playerController != null && staminaSlider != null)
        {
            staminaSlider.value = playerController.CurrentStamina; 
        }
        if (player != null && HpSlider != null)
        {
            HpSlider.value = player.CurrentHp; 
        }
        if (bulletManager != null && magText !=null)
        {
            magText.text = $"{bulletManager.CurrentMag} / {bulletManager.MagSize}";
        }
    }
}