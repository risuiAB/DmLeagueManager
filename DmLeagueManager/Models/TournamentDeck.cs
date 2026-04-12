using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DmLeagueManager.Models;

[Table("tournament_decks")]
public class TournamentDeck : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("season_player_id")]
    public int SeasonPlayerId { get; set; }

    [Column("deck_id")]
    public int DeckId { get; set; }

    [Column("status")]
    public string Status { get; set; } = "active";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public bool IsActive => Status == "active";
}
