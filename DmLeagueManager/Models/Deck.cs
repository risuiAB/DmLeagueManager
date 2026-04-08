using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DmLeagueManager.Models;

[Table("decks")]
public class Deck : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("season_player_id")]
    public int SeasonPlayerId { get; set; }

    [Column("player_id")]
    public int PlayerId { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("status")]
    public string Status { get; set; } = "active";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public bool IsActive => Status == "active";
}
