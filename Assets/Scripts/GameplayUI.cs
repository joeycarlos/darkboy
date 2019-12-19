using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    private static GameplayUI _instance;

    public static GameplayUI Instance {
        get {
            if (_instance == null) {
                GameObject go = new GameObject("GameplayUI");
                go.AddComponent<GameplayUI>();
            }

            return _instance;
        }
    }

    // HEALTH
    [Header("Health")]
    [SerializeField] private Text healthLabel;
    [SerializeField] private Image healthIcon;
    private List<Image> healthIcons;

    // SPIRIT
    /*
    [Header("Spirit")]
    [SerializeField] private Text spiritLevelValue;
    [SerializeField] private Image spiritBarImage;
    [HideInInspector] public int spiritBarMin;
    [HideInInspector] public int spiritBarMax;
    private int mCurrentValue;
    private float mCurrentPercent;
    */

    void Awake() {
        _instance = this;
        healthIcons = new List<Image>();
    }

    // HEALTH UI HELPER FUNCTIONS

    public void GenerateHealthUI(int health) {
        for (int i = 0; i < health; i++) {
            Image iHealthIcon = Instantiate(healthIcon, transform);
            iHealthIcon.rectTransform.anchoredPosition = new Vector2(healthLabel.rectTransform.anchoredPosition.x + 20.0f + (i * 30.0f), healthLabel.rectTransform.anchoredPosition.y);
            healthIcons.Add(iHealthIcon);
        }
    }

    public void RemoveHealthIcon(int value) {
        for (int i = 0; i < value; i++) {
            Image iHealthIcon = healthIcons[healthIcons.Count - 1];
            healthIcons.RemoveAt(healthIcons.Count - 1);
            Destroy(iHealthIcon.gameObject);
        }
    }

    public void AddHealthIcon(int value) {
        int originalHealth = healthIcons.Count;

        for (int i = 0 ; i <  value; i++) {
            Image iHealthIcon = Instantiate(healthIcon, transform);
            iHealthIcon.rectTransform.anchoredPosition = new Vector2(healthLabel.rectTransform.anchoredPosition.x + 20.0f + ((originalHealth + i) * 30.0f), healthLabel.rectTransform.anchoredPosition.y);
            healthIcons.Add(iHealthIcon);
        }
    }

    // SPIRIT UI HELPER FUNCTIONS
    /*
    public void UpdateSpiritLevelValue(int newLevel) {
        spiritLevelValue.text = newLevel.ToString();
    }

    public void SetSpirit(int spirit) {
        if (spiritBarMax - spiritBarMin == 0) {
            mCurrentValue = 0;
            mCurrentPercent = 0;
        }
        else {
            mCurrentValue = spirit;
            mCurrentPercent = (float)mCurrentValue / (float)(spiritBarMax - spiritBarMin);
        }

        spiritBarImage.fillAmount = mCurrentPercent;
    }
    */
}
