using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.Object == null || !GameManager.Instance.Object.IsValid) return;
        timerText.text = Mathf.Ceil(GameManager.Instance.CurrentTime).ToString();
    }
}