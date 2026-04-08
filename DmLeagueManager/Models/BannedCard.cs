using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DmLeagueManager.Models;

[Table("banned_cards")]
public class BannedCard : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("season_id")]
    public int SeasonId { get; set; }

    [Column("card_name")]
    public string CardName { get; set; } = "";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
