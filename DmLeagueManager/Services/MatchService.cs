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

    /// <summary>éüÇÃmatch_indexÇéÊìæ</summary>
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

    /// <summary>match_indexÇÃèdï°É`ÉFÉbÉN</summary>
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

            // é©ìÆÇ≈match_indexÇêUÇÈ
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
            return new MatchResult { Match = match };
        }

        // á@ ééçáÇìoò^
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

        // áA recalculate_tournamentÇ≈èüîsêîÅEécÉfÉbÉLÅEíEóéÅEèüÇøì_ÇàÍäáçƒåvéZ
        await supabase.Rpc("recalculate_tournament", new { p_tournament_id = tournamentId });

        return new MatchResult { Match = matchResult.Model! };
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
            MockData.Matches.Remove(match);
            return;
        }

        await supabase.Rpc("delete_match", new { p_match_id = matchId });
    }

    public async Task RecalculateTournamentAsync(int tournamentId)
    {
        if (config.UseMock)
        {
            await Task.Delay(300);
            return;
        }

        await supabase.Rpc("recalculate_tournament", new { p_tournament_id = tournamentId });
    }

    public FirstPlayerStats CalcFirstPlayerStats(List<Match> matches, int? playerId = null, int? deckId = null)
    {
        var filtered = matches.Where(m => m.FirstPlayerId.HasValue).ToList();

        if (playerId.HasValue)
            filtered = filtered.Where(m => m.FirstPlayerId == playerId).ToList();

        if (deckId.HasValue)
            filtered = filtered.Where(m => m.WinnerDeckId == deckId || m.LostDeckId == deckId).ToList();

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
