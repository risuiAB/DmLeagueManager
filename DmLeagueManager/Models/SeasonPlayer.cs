using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DmLeagueManager.Models;

[Table("season_players")]
public class SeasonPlayer : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("season_id")]
    public int SeasonId { get; set; }

    [Column("tournament_id")]
    public int? TournamentId { get; set; }

    [Column("player_id")]
    public int PlayerId { get; set; }

    [Column("remaining_decks")]
    public int RemainingDecks { get; set; } = 6;

    [Column("win_count")]
    public int WinCount { get; set; } = 0;

    [Column("lose_count")]
    public int LoseCount { get; set; } = 0;

    [Column("rank")]
    public int? Rank { get; set; }

    [Column("total_points")]
    public int TotalPoints { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    [Column("initial_points")]
    public int InitialPoints { get; set; } = 0;

    [Newtonsoft.Json.JsonIgnore]
    public Player? Player { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public bool IsEliminated => RemainingDecks <= 0;

    [Newtonsoft.Json.JsonIgnore]
    public int DeckDiff => WinCount - LoseCount;
}
