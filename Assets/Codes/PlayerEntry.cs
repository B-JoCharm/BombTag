using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEntry : MonoBehaviour
{
    [SerializeField] private Image profileImage;
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI readyText;
    [SerializeField] private Sprite[] characterProfiles;

    private BombHolder target;

    public void Setup(BombHolder bombHolder, int index)
    {
        target = bombHolder;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (target == null) return;
        nicknameText.text = target.Nickname.ToString();
        readyText.text = target.IsReady ? "Ready" : "Waiting...";

        int charIdx = target.CharacterIndex;
        if (characterProfiles != null && charIdx < characterProfiles.Length && characterProfiles[charIdx] != null)
        {
            profileImage.sprite = characterProfiles[charIdx];
            profileImage.color = Color.white;
        }
    }
}
