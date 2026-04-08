using DmLeagueManager.Models;
using Supabase;

namespace DmLeagueManager.Services;

public class SeasonService(Client supabase, AppConfig config)
{
    public async Task<List<Season>> GetAllAsync()
    {
        if (config.UseMock)
        {
            await Task.Delay(200);
            return MockData.Seasons.OrderByDescending(s => s.CreatedAt).ToList();
        }

        var result = await supabase.From<Season>()
            .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();
        return result.Models;
    }

    public async Task<Season?> GetActiveAsync()
    {
        if (config.UseMock)
        {
            await Task.Delay(200);
            return MockData.Seasons.FirstOrDefault(s => s.Status == "active");
        }

        var result = await supabase.From<Season>()
            .Where(s => s.Status == "active")
            .Single();
        return result;
    }

    public async Task<Season> CreateAsync(string name)
    {
        if (config.UseMock)
        {
            await Task.Delay(300);
            var newSeason = new Season
            {
                Id = MockData.Seasons.Max(s => s.Id) + 1,
                Name = name,
                Status = "active",
                CreatedAt = DateTime.Now
            };
            MockData.Seasons.Add(newSeason);
            return newSeason;
        }

        var season = new Season { Name = name, Status = "active" };
        var result = await supabase.From<Season>().Insert(season);
        return result.Model!;
    }

    public async Task FinishAsync(int seasonId)
    {
        if (config.UseMock)
        {
            await Task.Delay(300);
            var season = MockData.Seasons.FirstOrDefault(s => s.Id == seasonId);
            if (season != null) season.Status = "finished";
            return;
        }

        var s = await supabase.From<Season>().Where(x => x.Id == seasonId).Single();
        if (s != null) { s.Status = "finished"; await supabase.From<Season>().Update(s); }
    }
}
