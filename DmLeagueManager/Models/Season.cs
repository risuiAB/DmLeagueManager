using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DmLeagueManager.Models;

[Table("seasons")]
public class Season : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("status")]
    public string Status { get; set; } = "active";

    [Column("deck_count")]
    public int DeckCount { get; set; } = 6;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public bool IsActive => Status == "active";
}
