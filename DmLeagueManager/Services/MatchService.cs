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
                .OrderBy(m => m.MatchIndex ?? int.MaxValue)
                .ThenByDescending(m => m.PlayedAt)
                .ToList();
        }

        var result = await supabase.From<Match>()
            .Where(m => m.TournamentId == tournamentId)
            .Order("match_index", Supabase.Postgrest.Constants.Ordering.Ascending)
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

    /// <summary>次のmatch_indexを取得</summary>
    public async Task<int> GetNextMatchIndexAsync(int tournamentId)
    {
        if (config.UseMock)
        {
            var existing = MockData.Matches.Where(m => m.TournamentId == tournamentId && m.MatchIndex.HasValue);
            return existing.Any() ? existing.Max(m => m.MatchIndex!.Value) + 1 : 1;
        }

        var result = await supabase.From<Match>()
            .Where(m => m.TournamentId == tournamentId)
            .Get();

        var maxIndex = result.Models
            .Where(m => m.MatchIndex.HasValue)
            .Select(m => m.MatchIndex!.Value)
            .DefaultIfEmpty(0)
            .Max();

        return maxIndex + 1;
    }

    /// <summary>同じ大会内でmatch_indexが重複していないかチェック</summary>
    public async Task<bool> IsMatchIndexDuplicateAsync(int tournamentId, int matchIndex, int? excludeMatchId = null)
    {
        if (config.UseMock)
        {
            return MockData.Matches.Any(m =>
                m.TournamentId == tournamentId &&
                m.MatchIndex == matchIndex &&
                m.Id != (excludeMatchId ?? -1));
        }

        var result = await supabase.From<Match>()
            .Where(m => m.TournamentId == tournamentId && m.MatchIndex == matchIndex)
            .Get();

        return result.Models.Any(m => m.Id != (excludeMatchId ?? -1));
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

            // 自動でmatch_indexを振る
            var nextIndex = await GetNextMatchIndexAsync(tournamentId);

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
                MatchIndex = nextIndex,
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

        // 自動でmatch_indexを振る
        var nextIdx = await GetNextMatchIndexAsync(tournamentId);

        var matchReal = new Match
        {
            SeasonId = seasonId,
            TournamentId = tournamentId,
            WinnerPlayerId = winnerPlayerId,
            LoserPlayerId = loserPlayerId,
            WinnerDeckId = winnerDeckId,
            LostDeckId = lostDeckId,
            FirstPlayerId = firstPlayerId,
            MatchIndex = nextIdx,
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

    public async Task UpdateMatchAsync(
        int matchId,
        int winnerPlayerId, int loserPlayerId,
        int winnerDeckId, int lostDeckId,
        int? firstPlayerId, string? memo,
        int? matchIndex = null)
    {
        if (config.UseMock)
        {
            await Task.Delay(300);
            var match = MockData.Matches.FirstOrDefault(m => m.Id == matchId);
            if (match == null) return;
            match.WinnerPlayerId = winnerPlayerId;
            match.LoserPlayerId = loserPlayerId;
            match.WinnerDeckId = winnerDeckId;
            match.LostDeckId = lostDeckId;
            match.FirstPlayerId = firstPlayerId;
            match.Memo = memo;
            if (matchIndex.HasValue) match.MatchIndex = matchIndex;
            return;
        }

        await supabase.Rpc("update_match", new
        {
            p_match_id = matchId,
            p_winner_player_id = winnerPlayerId,
            p_loser_player_id = loserPlayerId,
            p_winner_deck_id = winnerDeckId,
            p_lost_deck_id = lostDeckId,
            p_first_player_id = firstPlayerId,
            p_memo = memo
        });

        // match_indexを別途更新
        if (matchIndex.HasValue)
        {
            var m = await supabase.From<Match>().Where(x => x.Id == matchId).Single();
            if (m != null)
            {
                m.MatchIndex = matchIndex;
                await supabase.From<Match>().Update(m);
            }
        }
    }

    public async Task DeleteMatchAsync(int matchId)
    {
        if (config.UseMock)
        {
            await Task.Delay(300);
            var match = MockData.Matches.FirstOrDefault(m => m.Id == matchId);
            if (match == null) return;

            // 勝者の勝利数を戻す
            var winnerSp = MockData.SeasonPlayers
                .FirstOrDefault(sp => sp.TournamentId == match.TournamentId && sp.PlayerId == match.WinnerPlayerId);
            if (winnerSp != null) winnerSp.WinCount--;

            // 敗者の敗北数・残デッキ数を戻す
            var loserSp = MockData.SeasonPlayers
                .FirstOrDefault(sp => sp.TournamentId == match.TournamentId && sp.PlayerId == match.LoserPlayerId);
            if (loserSp != null)
            {
                loserSp.LoseCount--;
                loserSp.RemainingDecks++;
            }

            // 消滅デッキをactiveに戻す
            var deck = MockData.Decks.FirstOrDefault(d => d.Id == match.LostDeckId);
            if (deck != null) deck.Status = "active";

            MockData.Matches.Remove(match);
            return;
        }

        await supabase.Rpc("delete_match", new { p_match_id = matchId });
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
