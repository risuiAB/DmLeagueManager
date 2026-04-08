using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DmLeagueManager.Models;

[Table("matches")]
public class Match : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("season_id")]
    public int SeasonId { get; set; }

    [Column("tournament_id")]
    public int TournamentId { get; set; }

    [Column("winner_player_id")]
    public int WinnerPlayerId { get; set; }

    [Column("loser_player_id")]
    public int LoserPlayerId { get; set; }

    [Column("winner_deck_id")]
    public int WinnerDeckId { get; set; }

    [Column("lost_deck_id")]
    public int LostDeckId { get; set; }

    [Column("first_player_id")]
    public int? FirstPlayerId { get; set; }

    [Column("played_at")]
    public DateTime PlayedAt { get; set; }

    [Column("memo")]
    public string? Memo { get; set; }

    // ナビゲーション（非DB）
    [Newtonsoft.Json.JsonIgnore]
    public Player? Winner { get; set; }
    [Newtonsoft.Json.JsonIgnore]
    public Player? Loser { get; set; }
    [Newtonsoft.Json.JsonIgnore]
    public Deck? WinnerDeck { get; set; }
    [Newtonsoft.Json.JsonIgnore]
    public Deck? LostDeck { get; set; }

    // 先攻が勝ったかどうか
    [Newtonsoft.Json.JsonIgnore]
    public bool? FirstPlayerWon => FirstPlayerId.HasValue
        ? FirstPlayerId == WinnerPlayerId
        : null;
}
