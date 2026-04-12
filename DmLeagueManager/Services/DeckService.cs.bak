using DmLeagueManager.Models;
using Supabase;

namespace DmLeagueManager.Services;

public class DeckService(Client supabase, AppConfig config)
{
    /// <summary>
    /// プレイヤーの全デッキ履歴を取得
    /// </summary>
    public async Task<List<Deck>> GetAllByPlayerAsync(int playerId)
    {
        if (config.UseMock)
        {
            await Task.Delay(100);
            var spIds = MockData.SeasonPlayers
                .Where(sp => sp.PlayerId == playerId)
                .Select(sp => sp.Id)
                .ToList();
            return MockData.Decks
                .Where(d => spIds.Contains(d.SeasonPlayerId))
                .DistinctBy(d => d.Name)
                .ToList();
        }

        var result = await supabase.From<Deck>()
            .Where(d => d.PlayerId == playerId)
            .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();

        // 名前の重複を除去して返す
        return result.Models.DistinctBy(d => d.Name).ToList();
    }

    /// <summary>
    /// 今シーズンでそのプレイヤーが使ったデッキを取得
    /// </summary>
    public async Task<List<Deck>> GetBySeasonPlayerAsync(int seasonPlayerId)
    {
        if (config.UseMock)
        {
            await Task.Delay(100);
            return MockData.Decks
                .Where(d => d.SeasonPlayerId == seasonPlayerId)
                .ToList();
        }

        var result = await supabase.From<Deck>()
            .Where(d => d.SeasonPlayerId == seasonPlayerId)
            .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();
        return result.Models;
    }

    /// <summary>
    /// デッキを新規登録して返す（試合入力時に使用）
    /// </summary>
    public async Task<Deck> RegisterDeckAsync(int seasonPlayerId, int playerId, string name)
    {
        if (config.UseMock)
        {
            await Task.Delay(200);
            // 既存チェック
            var existing = MockData.Decks.FirstOrDefault(d =>
                d.SeasonPlayerId == seasonPlayerId && d.Name == name);
            if (existing != null) return existing;

            var deck = new Deck
            {
                Id = MockData.Decks.Max(d => d.Id) + 1,
                SeasonPlayerId = seasonPlayerId,
                PlayerId = playerId,
                Name = name,
                Status = "active"
            };
            MockData.Decks.Add(deck);
            return deck;
        }

        // 既存チェック（同シーズン内で同名デッキがあればそれを返す）
        var existingResult = await supabase.From<Deck>()
            .Where(d => d.SeasonPlayerId == seasonPlayerId && d.Name == name)
            .Get();

        if (existingResult.Models.Any())
            return existingResult.Models.First();

        var newDeck = new Deck
        {
            SeasonPlayerId = seasonPlayerId,
            PlayerId = playerId,
            Name = name,
            Status = "active"
        };
        var insertResult = await supabase.From<Deck>().Insert(newDeck);
        return insertResult.Model!;
    }
}
