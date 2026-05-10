using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private void Update()
    {
        timerText.text =
            Mathf.Ceil(GameManager.Instance.currentTime).ToString();
    }
}