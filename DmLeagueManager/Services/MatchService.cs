using DmLeagueManager.Models;
using Supabase;

namespace DmLeagueManager.Services;

public class MatchService(Client supabase, AppConfig config)
{
    public async Task<List<Match>> GetTournamentMatchesAsync(int tournamentId)
    {
        if (config.UseMock)
        {
            await Task.Delay(150);
            return MockData.Matches
                .Where(m => m.TournamentId == tournamentId)
                .OrderByDescending(m => m.PlayedAt)
                .ToList();
        }

        var result = await supabase.From<Match>()
            .Where(m => m.TournamentId == tournamentId)
            .Order("played_at", Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();
        return result.Models;
    }

    public async Task<List<Match>> GetSeasonMatchesAsync(int seasonId)
    {
        if (config.UseMock)
        {
            await Task.Delay(150);
            return MockData.Matches
                .Where(m => m.SeasonId == seasonId)
                .OrderByDescending(m => m.PlayedAt)
                .ToList();
        }

        var result = await supabase.From<Match>()
            .Where(m => m.SeasonId == seasonId)
            .Order("played_at", Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();
        return result.Models;
    }

    public async Task<MatchResult> RecordMatchAsync(
        int seasonId, int tournamentId,
        int winnerPlayerId, int loserPlayerId,
        int winnerDeckId, int lostDeckId,
        int? firstPlayerId = null,
        string? memo = null)
    {
        if (config.UseMock)
        {
            await Task.Delay(400);

            var match = new Match
            {
                Id = MockData.Matches.Count + 1,
                SeasonId = seasonId,
                TournamentId = tournamentId,
                WinnerPlayerId = winnerPlayerId,
                LoserPlayerId = loserPlayerId,
                WinnerDeckId = winnerDeckId,
                LostDeckId = lostDeckId,
                FirstPlayerId = firstPlayerId,
                Memo = memo,
                PlayedAt = DateTime.Now
            };
            MockData.Matches.Add(match);

            var deck = MockData.Decks.FirstOrDefault(d => d.Id == lostDeckId);
            if (deck != null) deck.Status = "lost";

            var winnerSp = MockData.SeasonPlayers
                .FirstOrDefault(sp => sp.TournamentId == tournamentId && sp.PlayerId == winnerPlayerId);
            var loserSp = MockData.SeasonPlayers
                .FirstOrDefault(sp => sp.TournamentId == tournamentId && sp.PlayerId == loserPlayerId);

            bool eliminated = false;
            int? eliminatedRank = null;

            if (winnerSp != null) winnerSp.WinCount++;

            if (loserSp != null)
            {
                loserSp.LoseCount++;
                loserSp.RemainingDecks--;

                if (loserSp.RemainingDecks <= 0)
                {
                    int totalPlayers = MockData.SeasonPlayers.Count(sp => sp.TournamentId == tournamentId);
                    int eliminatedCount = MockData.SeasonPlayers.Count(sp => sp.TournamentId == tournamentId && sp.Rank != null);
                    int actualRank = totalPlayers - eliminatedCount;
                    loserSp.Rank = actualRank;
                    int rankPoints = actualRank switch { 1 => 4, 2 => 1, _ => 0 };
                    loserSp.TotalPoints = rankPoints + loserSp.DeckDiff;
                    eliminated = true;
                    eliminatedRank = actualRank;

                    // 残り1人になったら自動で1位確定
                    var remaining = MockData.SeasonPlayers
                        .Where(sp => sp.TournamentId == tournamentId && sp.Rank == null)
                        .ToList();

                    if (remaining.Count == 1)
                    {
                        var lastPlayer = remaining.First();
                        lastPlayer.Rank = 1;
                        lastPlayer.TotalPoints = 3 + lastPlayer.DeckDiff;
                    }
                }
            }

            return new MatchResult { Match = match, LoserEliminated = eliminated, EliminatedRank = eliminatedRank };
        }

        // 本番Supabase処理
        var matchReal = new Match
        {
            SeasonId = seasonId,
            TournamentId = tournamentId,
            WinnerPlayerId = winnerPlayerId,
            LoserPlayerId = loserPlayerId,
            WinnerDeckId = winnerDeckId,
            LostDeckId = lostDeckId,
            FirstPlayerId = firstPlayerId,
            Memo = memo,
            PlayedAt = DateTime.UtcNow
        };
        var matchResult = await supabase.From<Match>().Insert(matchReal);

        var lostDeck = await supabase.From<Deck>().Where(d => d.Id == lostDeckId).Single();
        if (lostDeck != null) { lostDeck.Status = "lost"; await supabase.From<Deck>().Update(lostDeck); }

        var winnerSpReal = await supabase.From<SeasonPlayer>()
            .Where(sp => sp.TournamentId == tournamentId && sp.PlayerId == winnerPlayerId).Single();
        if (winnerSpReal != null) { winnerSpReal.WinCount++; await supabase.From<SeasonPlayer>().Update(winnerSpReal); }

        var loserSpReal = await supabase.From<SeasonPlayer>()
            .Where(sp => sp.TournamentId == tournamentId && sp.PlayerId == loserPlayerId).Single();

        bool eliminatedReal = false;
        int? eliminatedRankReal = null;

        if (loserSpReal != null)
        {
            loserSpReal.LoseCount++;
            loserSpReal.RemainingDecks--;

            if (loserSpReal.RemainingDecks <= 0)
            {
                var all = await supabase.From<SeasonPlayer>()
                    .Where(sp => sp.TournamentId == tournamentId).Get();
                int eliminatedCount = all.Models.Count(sp => sp.Rank != null);
                int actualRank = all.Models.Count - eliminatedCount;
                loserSpReal.Rank = actualRank;
                int rankPoints = actualRank switch { 1 => 3, 2 => 1, _ => 0 };
                loserSpReal.TotalPoints = rankPoints + loserSpReal.DeckDiff;
                eliminatedReal = true;
                eliminatedRankReal = actualRank;

                await supabase.From<SeasonPlayer>().Update(loserSpReal);

                // 残り1人になったら自動で1位確定
                var allAfter = await supabase.From<SeasonPlayer>()
                    .Where(sp => sp.TournamentId == tournamentId).Get();
                var remaining = allAfter.Models.Where(sp => sp.Rank == null).ToList();
                if (remaining.Count == 1)
                {
                    var lastPlayer = remaining.First();
                    lastPlayer.Rank = 1;
                    lastPlayer.TotalPoints = 3 + lastPlayer.DeckDiff; // 1位の点数
                    await supabase.From<SeasonPlayer>().Update(lastPlayer);
                }
            }
            else
            {
                await supabase.From<SeasonPlayer>().Update(loserSpReal);
            }
        }

        return new MatchResult
        {
            Match = matchResult.Model!,
            LoserEliminated = eliminatedReal,
            EliminatedRank = eliminatedRankReal
        };
    }

    /// <summary>先攻勝率統計を計算</summary>
    public FirstPlayerStats CalcFirstPlayerStats(List<Match> matches, int? playerId = null, int? deckId = null)
    {
        var filtered = matches.Where(m => m.FirstPlayerId.HasValue).ToList();

        if (playerId.HasValue)
        {
            // そのプレイヤーが先攻だった試合のみ
            filtered = filtered.Where(m => m.FirstPlayerId == playerId).ToList();
        }

        if (deckId.HasValue)
        {
            filtered = filtered.Where(m => m.WinnerDeckId == deckId || m.LostDeckId == deckId).ToList();
        }

        int total = filtered.Count;
        int firstWin = filtered.Count(m => m.FirstPlayerId == m.WinnerPlayerId);

        return new FirstPlayerStats
        {
            TotalGames = total,
            FirstPlayerWins = firstWin,
            FirstPlayerWinRate = total == 0 ? 0 : Math.Round((double)firstWin / total * 100, 1)
        };
    }
}

public class MatchResult
{
    public Match Match { get; set; } = null!;
    public bool WinnerEliminated { get; set; }
    public bool LoserEliminated { get; set; }
    public int? EliminatedRank { get; set; }
}

public class FirstPlayerStats
{
    public int TotalGames { get; set; }
    public int FirstPlayerWins { get; set; }
    public double FirstPlayerWinRate { get; set; }
}
