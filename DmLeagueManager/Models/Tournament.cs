using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DmLeagueManager.Models;

[Table("tournaments")]
public class Tournament : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("season_id")]
    public int SeasonId { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("deck_count")]
    public int DeckCount { get; set; } = 6;

    [Column("status")]
    public string Status { get; set; } = "active";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public bool IsActive => Status == "active";
}
