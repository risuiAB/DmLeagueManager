using DmLeagueManager.Models;
using Supabase;

namespace DmLeagueManager.Services;

public class DeckService(Client supabase, AppConfig config)
{
    /// <summary>プレイヤーの全デッキ取得（マスター）</summary>
    public async Task<List<Deck>> GetAllByPlayerAsync(int playerId)
    {
        if (config.UseMock)
        {
            await Task.Delay(100);
            return MockData.Decks.Where(d => d.PlayerId == playerId).ToList();
        }

        var result = await supabase.From<Deck>()
            .Where(d => d.PlayerId == playerId)
            .Order("created_at", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();
        return result.Models;
    }

    /// <summary>大会で使っているデッキ一覧（tournament_decks経由）</summary>
    public async Task<List<(Deck Deck, TournamentDeck TournamentDeck)>> GetTournamentDecksAsync(int seasonPlayerId)
    {
        if (config.UseMock)
        {
            await Task.Delay(100);
            var tds = MockData.TournamentDecks.Where(td => td.SeasonPlayerId == seasonPlayerId).ToList();
            var result = new List<(Deck, TournamentDeck)>();
            foreach (var td in tds)
            {
                var deck = MockData.Decks.FirstOrDefault(d => d.Id == td.DeckId);
                if (deck != null) result.Add((deck, td));
            }
            return result;
        }

        var tdResult = await supabase.From<TournamentDeck>()
            .Where(td => td.SeasonPlayerId == seasonPlayerId)
            .Order("created_at", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        var pairs = new List<(Deck, TournamentDeck)>();
        foreach (var td in tdResult.Models)
        {
            var deck = await supabase.From<Deck>().Where(d => d.Id == td.DeckId).Single();
            if (deck != null) pairs.Add((deck, td));
        }
        return pairs;
    }

    /// <summary>大会で使っているデッキIDのセットを取得</summary>
    public async Task<HashSet<int>> GetTournamentDeckIdsAsync(int seasonPlayerId)
    {
        if (config.UseMock)
        {
            return MockData.TournamentDecks
                .Where(td => td.SeasonPlayerId == seasonPlayerId)
                .Select(td => td.DeckId)
                .ToHashSet();
        }

        var result = await supabase.From<TournamentDeck>()
            .Where(td => td.SeasonPlayerId == seasonPlayerId)
            .Get();
        return result.Models.Select(td => td.DeckId).ToHashSet();
    }

    /// <summary>デッキを新規作成（マスター登録）</summary>
    public async Task<Deck> CreateDeckAsync(int playerId, string name)
    {
        if (config.UseMock)
        {
            await Task.Delay(200);
            var newDeck = new Deck
            {
                Id = MockData.Decks.Any() ? MockData.Decks.Max(d => d.Id) + 1 : 1,
                PlayerId = playerId,
                Name = name,
                CreatedAt = DateTime.Now
            };
            MockData.Decks.Add(newDeck);
            return newDeck;
        }

        var deck = new Deck { PlayerId = playerId, Name = name };
        var result = await supabase.From<Deck>().Insert(deck);
        return result.Model!;
    }

    /// <summary>デッキを大会に登録（tournament_decks登録）</summary>
    public async Task<TournamentDeck> AddDeckToTournamentAsync(int seasonPlayerId, int deckId)
    {
        if (config.UseMock)
        {
            await Task.Delay(100);
            var existing = MockData.TournamentDecks
                .FirstOrDefault(td => td.SeasonPlayerId == seasonPlayerId && td.DeckId == deckId);
            if (existing != null) return existing;

            var newTd = new TournamentDeck
            {
                Id = MockData.TournamentDecks.Any() ? MockData.TournamentDecks.Max(td => td.Id) + 1 : 1,
                SeasonPlayerId = seasonPlayerId,
                DeckId = deckId,
                Status = "active",
                CreatedAt = DateTime.Now
            };
            MockData.TournamentDecks.Add(newTd);
            return newTd;
        }

        // 既に登録済みなら返す
        var existingResult = await supabase.From<TournamentDeck>()
            .Where(td => td.SeasonPlayerId == seasonPlayerId && td.DeckId == deckId)
            .Get();
        if (existingResult.Models.Any()) return existingResult.Models.First();

        var td2 = new TournamentDeck { SeasonPlayerId = seasonPlayerId, DeckId = deckId, Status = "active" };
        var result = await supabase.From<TournamentDeck>().Insert(td2);
        return result.Model!;
    }

    /// <summary>新規デッキを作成して大会に登録（一括処理）</summary>
    public async Task<(Deck Deck, TournamentDeck TournamentDeck)> CreateAndAddDeckAsync(
        int playerId, int seasonPlayerId, string name)
    {
        var deck = await CreateDeckAsync(playerId, name);
        var td = await AddDeckToTournamentAsync(seasonPlayerId, deck.Id);
        return (deck, td);
    }

    /// <summary>既存デッキを大会に追加（全履歴から選択時）</summary>
    public async Task<TournamentDeck> RegisterExistingDeckToTournamentAsync(int seasonPlayerId, int deckId)
    {
        return await AddDeckToTournamentAsync(seasonPlayerId, deckId);
    }

    /// <summary>デッキ名の重複チェック（同プレイヤー内）</summary>
    public async Task<bool> IsDeckNameDuplicateAsync(int playerId, string name)
    {
        if (config.UseMock)
        {
            return MockData.Decks.Any(d => d.PlayerId == playerId && d.Name == name);
        }

        var result = await supabase.From<Deck>()
            .Where(d => d.PlayerId == playerId && d.Name == name)
            .Get();
        return result.Models.Any();
    }

    /// <summary>TournamentDeckのステータスを更新</summary>
    public async Task UpdateTournamentDeckStatusAsync(int tournamentDeckId, string status)
    {
        if (config.UseMock)
        {
            var td = MockData.TournamentDecks.FirstOrDefault(x => x.Id == tournamentDeckId);
            if (td != null) td.Status = status;
            return;
        }

        var td2 = await supabase.From<TournamentDeck>().Where(x => x.Id == tournamentDeckId).Single();
        if (td2 != null)
        {
            td2.Status = status;
            await supabase.From<TournamentDeck>().Update(td2);
        }
    }

    /// <summary>deck_idとseason_player_idからtournament_deckを取得</summary>
    public async Task<TournamentDeck?> GetTournamentDeckAsync(int seasonPlayerId, int deckId)
    {
        if (config.UseMock)
        {
            return MockData.TournamentDecks
                .FirstOrDefault(td => td.SeasonPlayerId == seasonPlayerId && td.DeckId == deckId);
        }

        var result = await supabase.From<TournamentDeck>()
            .Where(td => td.SeasonPlayerId == seasonPlayerId && td.DeckId == deckId)
            .Get();
        return result.Models.FirstOrDefault();
    }
}
