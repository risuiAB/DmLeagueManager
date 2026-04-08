using DmLeagueManager.Models;
using Supabase;

namespace DmLeagueManager.Services;

public class PlayerService(Client supabase, AppConfig config)
{
    public async Task<List<Player>> GetAllAsync()
    {
        if (config.UseMock)
        {
            await Task.Delay(100);
            return MockData.Players.ToList();
        }

        var result = await supabase.From<Player>().Get();
        return result.Models;
    }

    public async Task<Player> CreateAsync(string name)
    {
        if (config.UseMock)
        {
            await Task.Delay(200);
            var p = new Player { Id = MockData.Players.Max(x => x.Id) + 1, Name = name };
            MockData.Players.Add(p);
            return p;
        }

        var player = new Player { Name = name };
        var result = await supabase.From<Player>().Insert(player);
        return result.Model!;
    }

    /// <summary>大会の順位表を取得</summary>
    public async Task<List<SeasonPlayer>> GetTournamentStandingsAsync(int tournamentId)
    {
        if (config.UseMock)
        {
            await Task.Delay(200);
            return MockData.SeasonPlayers
                .Where(sp => sp.TournamentId == tournamentId)
                .OrderBy(sp => sp.Rank.HasValue ? sp.Rank.Value : 0)
                .ThenByDescending(sp => sp.RemainingDecks)
                .ThenByDescending(sp => sp.WinCount)
                .ToList();
        }

        var result = await supabase.From<SeasonPlayer>()
            .Where(sp => sp.TournamentId == tournamentId)
            .Get();

        var players = await GetAllAsync();
        var playerMap = players.ToDictionary(p => p.Id);
        foreach (var sp in result.Models)
            if (playerMap.TryGetValue(sp.PlayerId, out var player))
                sp.Player = player;

        return result.Models
            .OrderBy(sp => sp.Rank.HasValue ? sp.Rank.Value : 0)
            .ThenByDescending(sp => sp.RemainingDecks)
            .ThenByDescending(sp => sp.WinCount)
            .ToList();
    }

    /// <summary>シーズン総合順位（全大会の勝ち点合計）を取得</summary>
    public async Task<List<SeasonRanking>> GetSeasonRankingsAsync(int seasonId)
    {
        if (config.UseMock)
        {
            await Task.Delay(200);
            var players = MockData.Players.ToDictionary(p => p.Id);
            return MockData.SeasonPlayers
                .Where(sp => sp.SeasonId == seasonId)
                .GroupBy(sp => sp.PlayerId)
                .Select(g => new SeasonRanking
                {
                    Player = players.GetValueOrDefault(g.Key) ?? new Player { Name = "?" },
                    TotalPoints = g.Sum(sp => sp.TotalPoints),
                    TournamentCount = g.Count(),
                    TotalWins = g.Sum(sp => sp.WinCount),
                    TotalLoses = g.Sum(sp => sp.LoseCount),
                })
                .OrderByDescending(r => r.TotalPoints)
                .ToList();
        }

        var result = await supabase.From<SeasonPlayer>()
            .Where(sp => sp.SeasonId == seasonId)
            .Get();

        var allPlayers = await GetAllAsync();
        var playerMap = allPlayers.ToDictionary(p => p.Id);

        return result.Models
            .GroupBy(sp => sp.PlayerId)
            .Select(g => new SeasonRanking
            {
                Player = playerMap.GetValueOrDefault(g.Key) ?? new Player { Name = "?" },
                TotalPoints = g.Sum(sp => sp.TotalPoints),
                TournamentCount = g.Count(),
                TotalWins = g.Sum(sp => sp.WinCount),
                TotalLoses = g.Sum(sp => sp.LoseCount),
            })
            .OrderByDescending(r => r.TotalPoints)
            .ToList();
    }

    /// <summary>大会にプレイヤーを参加登録</summary>
    public async Task JoinTournamentAsync(int seasonId, int tournamentId, int playerId, int deckCount, int initialPoints = 0)
    {
        if (config.UseMock)
        {
            await Task.Delay(300);
            if (MockData.SeasonPlayers.Any(sp => sp.TournamentId == tournamentId && sp.PlayerId == playerId))
                return;

            var sp = new SeasonPlayer
            {
                Id = MockData.SeasonPlayers.Max(x => x.Id) + 1,
                SeasonId = seasonId,
                TournamentId = tournamentId,
                PlayerId = playerId,
                RemainingDecks = deckCount,
                TotalPoints = initialPoints,  // ← 初期勝ち点を設定
                Player = MockData.Players.FirstOrDefault(p => p.Id == playerId)
            };
            MockData.SeasonPlayers.Add(sp);
            return;
        }

        var existing = await supabase.From<SeasonPlayer>()
            .Where(sp => sp.TournamentId == tournamentId && sp.PlayerId == playerId)
            .Get();
        if (existing.Models.Any()) return;

        var spNew = new SeasonPlayer
        {
            SeasonId = seasonId,
            TournamentId = tournamentId,
            PlayerId = playerId,
            RemainingDecks = deckCount,
            TotalPoints = initialPoints  // ← 初期勝ち点を設定
        };
        await supabase.From<SeasonPlayer>().Insert(spNew);
    }

    public async Task<SeasonPlayer?> GetSeasonPlayerAsync(int tournamentId, int playerId)
    {
        if (config.UseMock)
        {
            await Task.Delay(50);
            return MockData.SeasonPlayers
                .FirstOrDefault(sp => sp.TournamentId == tournamentId && sp.PlayerId == playerId);
        }

        return await supabase.From<SeasonPlayer>()
            .Where(sp => sp.TournamentId == tournamentId && sp.PlayerId == playerId)
            .Single();
    }
}

public class SeasonRanking
{
    public Player Player { get; set; } = null!;
    public int TotalPoints { get; set; }
    public int TournamentCount { get; set; }
    public int TotalWins { get; set; }
    public int TotalLoses { get; set; }
}
