using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DmLeagueManager.Models;

[Table("players")]
public class Player : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
