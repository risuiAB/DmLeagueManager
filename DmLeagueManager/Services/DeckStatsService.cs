using DmLeagueManager.Models;
using Supabase;

namespace DmLeagueManager.Services;

public class DeckStats
{
    public Deck Deck { get; set; } = null!;
    public int WinCount { get; set; }
    public int LoseCount { get; set; }
    public int TotalGames => WinCount + LoseCount;
    public double WinRate => TotalGames == 0 ? 0 : Math.Round((double)WinCount / TotalGames * 100, 1);
    public List<DeckMatchRecord> WinRecords { get; set; } = new();
    public List<DeckMatchRecord> LoseRecords { get; set; } = new();
}

public class DeckMatchRecord
{
    public string OpponentDeckName { get; set; } = "";
    public string OpponentPlayerName { get; set; } = "";
    public DateTime PlayedAt { get; set; }
}

public class DeckStatsService(Client supabase, AppConfig config)
{
    public async Task<DeckStats> GetDeckStatsAsync(
        int deckId,
        List<Match> allMatches,
        Dictionary<int, Deck> deckMap,
        Dictionary<int, Player> playerMap)
    {
        var deck = deckMap.GetValueOrDefault(deckId);
        if (deck == null) return new DeckStats { Deck = new Deck { Name = "不明" } };

        var stats = new DeckStats { Deck = deck };

        foreach (var m in allMatches)
        {
            if (m.WinnerDeckId == deckId)
            {
                // このデッキが勝った試合
                var lostDeck = deckMap.GetValueOrDefault(m.LostDeckId);
                stats.WinCount++;
                stats.WinRecords.Add(new DeckMatchRecord
                {
                    OpponentDeckName = lostDeck?.Name ?? "不明",
                    OpponentPlayerName = playerMap.GetValueOrDefault(m.LoserPlayerId)?.Name ?? "不明",
                    PlayedAt = m.PlayedAt
                });
            }
            else if (m.LostDeckId == deckId)
            {
                // このデッキが負けた試合
                var winnerDeck = deckMap.GetValueOrDefault(m.WinnerDeckId);
                stats.LoseCount++;
                stats.LoseRecords.Add(new DeckMatchRecord
                {
                    OpponentDeckName = winnerDeck?.Name ?? "不明",
                    OpponentPlayerName = playerMap.GetValueOrDefault(m.WinnerPlayerId)?.Name ?? "不明",
                    PlayedAt = m.PlayedAt
                });
            }
        }

        return stats;
    }
}
