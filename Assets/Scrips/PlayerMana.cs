using UnityEngine;
using UnityEngine.UI;

public class PlayerMana : MonoBehaviour
{
    [Header("Mana Settings")]
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float fillDuration = 30f;

    [Header("UI")]
    [SerializeField] private Slider manaSlider;

    private float currentMana;
    private bool isFull;

    public bool IsFull => isFull;
    public float CurrentMana => currentMana;

    private void Start()
    {
        currentMana = 0f;

        if (manaSlider != null)
        {
            manaSlider.minValue = 0f;
            manaSlider.maxValue = maxMana;
            manaSlider.value = currentMana;
        }

        UpdateManaUI();
    }

    private void Update()
    {
        if (isFull)
            return;

        currentMana += (maxMana / fillDuration) * Time.deltaTime;

        if (currentMana >= maxMana)
        {
            currentMana = maxMana;
            isFull = true;
        }

        UpdateManaUI();
    }

    public bool TryUseSpecialAttack()
    {
        if (!isFull)
            return false;

        currentMana = 0f;
        isFull = false;

        UpdateManaUI();

        return true;
    }

    private void UpdateManaUI()
    {
        if (manaSlider != null)
            manaSlider.value = currentMana;
    }
}