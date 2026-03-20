using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Slider HpSlider;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Player player;


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
    }
}