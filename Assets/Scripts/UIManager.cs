using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private PlayerController player;

    void Update()
    {
        if (player != null && staminaSlider != null)
        {
            staminaSlider.value = player.CurrentStamina; 
        }
    }
}