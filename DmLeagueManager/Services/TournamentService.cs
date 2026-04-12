using DmLeagueManager.Models;
using Supabase;

namespace DmLeagueManager.Services;

public class TournamentService(Client supabase, AppConfig config)
{
    public async Task<List<Tournament>> GetBySeasonAsync(int seasonId)
    {
        if (config.UseMock)
        {
            await Task.Delay(150);
            return MockData.Tournaments.Where(t => t.SeasonId == seasonId)
                .OrderByDescending(t => t.CreatedAt).ToList();
        }

        var result = await supabase.From<Tournament>()
            .Where(t => t.SeasonId == seasonId)
            .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();
        return result.Models;
    }

    public async Task<Tournament?> GetActiveTournamentAsync(int seasonId)
    {
        if (config.UseMock)
        {
            await Task.Delay(100);
            return MockData.Tournaments
                .FirstOrDefault(t => t.SeasonId == seasonId && t.Status == "active");
        }

        var result = await supabase.From<Tournament>()
            .Where(t => t.SeasonId == seasonId && t.Status == "active")
            .Single();
        return result;
    }

    public async Task<Tournament> CreateAsync(int seasonId, string name, int deckCount)
    {
        if (config.UseMock)
        {
            await Task.Delay(300);
            var t = new Tournament
            {
                Id = MockData.Tournaments.Any() ? MockData.Tournaments.Max(x => x.Id) + 1 : 1,
                SeasonId = seasonId,
                Name = name,
                DeckCount = deckCount,
                Status = "active",
                CreatedAt = DateTime.Now
            };
            MockData.Tournaments.Add(t);
            return t;
        }

        var tournament = new Tournament
        {
            SeasonId = seasonId,
            Name = name,
            DeckCount = deckCount,
            Status = "active"
        };
        var result = await supabase.From<Tournament>().Insert(tournament);
        return result.Model!;
    }

    public async Task FinishAsync(int tournamentId, List<SeasonPlayer> standings)
    {
        if (config.UseMock)
        {
            await Task.Delay(300);
            var t = MockData.Tournaments.FirstOrDefault(x => x.Id == tournamentId);
            if (t != null) t.Status = "finished";
            return;
        }

        // statusをfinishedに変更
        var tournament = await supabase.From<Tournament>()
            .Where(t => t.Id == tournamentId).Single();
        if (tournament != null)
        {
            tournament.Status = "finished";
            await supabase.From<Tournament>().Update(tournament);
        }

        // recalculate_tournamentで勝ち点を再計算（rank_pointsを自動で参照）
        await supabase.Rpc("recalculate_tournament", new { p_tournament_id = tournamentId });

        // 全デッキをactiveに戻す
        await supabase.Rpc("reset_decks_for_season", new { p_season_id = tournament!.SeasonId });

    }
}
