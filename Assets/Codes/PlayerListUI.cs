using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerListUI : MonoBehaviour
{
    [SerializeField] private GameObject playerEntryPrefab;

    private readonly List<PlayerEntry> entries = new();
    private readonly List<BombHolder> lastPlayers = new();

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.Object == null || !GameManager.Instance.Object.IsValid) return;

        var players = GameManager.Instance.Players
            .Where(p => p != null && p.Object != null)
            .OrderBy(p => p.Object.InputAuthority.RawEncoded)
            .ToList();

        if (players.SequenceEqual(lastPlayers)) return;

        foreach (var entry in entries)
            Destroy(entry.gameObject);
        entries.Clear();
        lastPlayers.Clear();
        lastPlayers.AddRange(players);

        for (int i = 0; i < players.Count; i++)
        {
            var go = Instantiate(playerEntryPrefab, transform);
            var entry = go.GetComponent<PlayerEntry>();
            entry.Setup(players[i], i);
            entries.Add(entry);
        }
    }
}
