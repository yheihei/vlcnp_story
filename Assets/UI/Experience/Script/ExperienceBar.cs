using UnityEngine;
using UnityEngine.UI;

public class ExperienceBar : MonoBehaviour {
    private Slider slider;
    private PlayerLevel playerLevel;
    private int beforeLevel;

    void Start()
    {
        slider = GetComponent<Slider>();
        slider.value = 1;
        playerLevel = GameObject.FindWithTag("Player").GetComponent<PlayerLevel>();
        beforeLevel = playerLevel.Level;
    }

    // void Update()
    // {
    //     float max = playerLevel.LevelToRequiredExperience[playerLevel.Level] - playerLevel.LevelToRequiredExperience[playerLevel.Level - 1];
    //     float current = playerLevel.Experience - playerLevel.LevelToRequiredExperience[playerLevel.Level - 1];
    //     current = current == max ? 0 : current;
    //     slider.value = (float) current / (float) max;
    //     // 最大経験値になった場合はスライダーを1で固定
    //     if (playerLevel.Experience >= playerLevel.LevelToRequiredExperience[playerLevel.MaxLevel]) {
    //         slider.value = 1;
    //     }
    //     // Debug.Log($"max: {max}, required: {playerLevel.LevelToRequiredExperience[playerLevel.Level]}, current: {current}, value: {slider.value}");
    // }

    private void FixedUpdate() {
        float max = playerLevel.LevelToRequiredExperience[playerLevel.Level] - playerLevel.LevelToRequiredExperience[playerLevel.Level - 1];
        float current = playerLevel.Experience - playerLevel.LevelToRequiredExperience[playerLevel.Level - 1];
        current = current == max ? 0 : current;
        slider.value = (float) current / (float) max;
        // 最大経験値になった場合はスライダーを1で固定
        if (playerLevel.Experience >= playerLevel.LevelToRequiredExperience[playerLevel.MaxLevel]) {
            slider.value = 1;
        }
        // Debug.Log($"max: {max}, required: {playerLevel.LevelToRequiredExperience[playerLevel.Level]}, current: {current}, value: {slider.value}");
    }

}