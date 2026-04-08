using DmLeagueManager.Models;
using Supabase;

namespace DmLeagueManager.Services;

public class BannedCardService(Client supabase, AppConfig config)
{
    public async Task<List<BannedCard>> GetBySeasonAsync(int seasonId)
    {
        if (config.UseMock)
        {
            await Task.Delay(100);
            return MockData.BannedCards.Where(b => b.SeasonId == seasonId).ToList();
        }

        var result = await supabase.From<BannedCard>()
            .Where(b => b.SeasonId == seasonId)
            .Order("created_at", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();
        return result.Models;
    }

    public async Task<BannedCard> AddAsync(int seasonId, string cardName)
    {
        if (config.UseMock)
        {
            await Task.Delay(200);
            var card = new BannedCard
            {
                Id = MockData.BannedCards.Any() ? MockData.BannedCards.Max(b => b.Id) + 1 : 1,
                SeasonId = seasonId,
                CardName = cardName,
                CreatedAt = DateTime.Now
            };
            MockData.BannedCards.Add(card);
            return card;
        }

        var newCard = new BannedCard { SeasonId = seasonId, CardName = cardName };
        var result = await supabase.From<BannedCard>().Insert(newCard);
        return result.Model!;
    }

    public async Task DeleteAsync(int id)
    {
        if (config.UseMock)
        {
            await Task.Delay(200);
            var card = MockData.BannedCards.FirstOrDefault(b => b.Id == id);
            if (card != null) MockData.BannedCards.Remove(card);
            return;
        }

        await supabase.From<BannedCard>()
            .Where(b => b.Id == id)
            .Delete();
    }
}
