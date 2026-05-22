using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEntry : MonoBehaviour
{
    [SerializeField] private Image profileImage;
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI readyText;

    private static readonly Color[] profileColors =
    {
        new(0.9f, 0.4f, 0.4f),
        new(0.4f, 0.6f, 0.9f),
        new(0.4f, 0.85f, 0.5f),
        new(0.95f, 0.8f, 0.3f),
    };

    private BombHolder target;

    public void Setup(BombHolder bombHolder, int index)
    {
        target = bombHolder;
        profileImage.color = profileColors[index % profileColors.Length];
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (target == null) return;
        nicknameText.text = target.Nickname.ToString();
        readyText.text = target.IsReady ? "Ready" : "Waiting...";
    }
}
