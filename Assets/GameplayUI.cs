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

    public Text healthLabel;
    public Image healthIcon;

    private List<Image> healthIcons;

    void Awake() {
        _instance = this;
        healthIcons = new List<Image>();
    }

    void Start(){
        GenerateHealthUI(5);
    }

    void GenerateHealthUI(int health) {
        for (int i = 0; i < health; i++) {
            Image iHealthIcon = Instantiate(healthIcon, transform);
            iHealthIcon.rectTransform.anchoredPosition = new Vector2(healthLabel.rectTransform.anchoredPosition.x + 20.0f + (i * 30.0f), healthLabel.rectTransform.anchoredPosition.y + 5.0f);
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
            iHealthIcon.rectTransform.anchoredPosition = new Vector2(healthLabel.rectTransform.anchoredPosition.x + 20.0f + ((originalHealth + i) * 30.0f), healthLabel.rectTransform.anchoredPosition.y + 5.0f);
            healthIcons.Add(iHealthIcon);
        }
    }
}
