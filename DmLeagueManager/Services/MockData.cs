using DmLeagueManager.Models;

namespace DmLeagueManager.Services;

public static class MockData
{
    public static readonly List<Player> Players = new()
    {
        new Player { Id = 1, Name = "鈴木" },
        new Player { Id = 2, Name = "佐藤" },
        new Player { Id = 3, Name = "田中" },
    };

    public static readonly List<Season> Seasons = new()
    {
        new Season { Id = 1, Name = "2024年シーズン", Status = "finished", CreatedAt = DateTime.Now.AddMonths(-6) },
        new Season { Id = 2, Name = "2025年シーズン", Status = "active",   CreatedAt = DateTime.Now.AddDays(-30) },
    };

    public static readonly List<Tournament> Tournaments = new()
    {
        // 2024年シーズン
        new Tournament { Id=1, SeasonId=1, Name="第1回大会", DeckCount=6, Status="finished", CreatedAt=DateTime.Now.AddMonths(-5) },
        new Tournament { Id=2, SeasonId=1, Name="第2回大会", DeckCount=6, Status="finished", CreatedAt=DateTime.Now.AddMonths(-4) },
        // 2025年シーズン
        new Tournament { Id=3, SeasonId=2, Name="第1回大会", DeckCount=6, Status="finished", CreatedAt=DateTime.Now.AddDays(-20) },
        new Tournament { Id=4, SeasonId=2, Name="第2回大会", DeckCount=6, Status="active",   CreatedAt=DateTime.Now.AddDays(-5) },
    };

    public static readonly List<Deck> Decks = new()
    {
        new Deck { Id=1,  SeasonPlayerId=1, PlayerId=1, Name="アナカラーハンデス", Status="active" },
        new Deck { Id=2,  SeasonPlayerId=1, PlayerId=1, Name="赤白轟轟轟",         Status="active" },
        new Deck { Id=3,  SeasonPlayerId=1, PlayerId=1, Name="5cコントロール",      Status="active" },
        new Deck { Id=4,  SeasonPlayerId=1, PlayerId=1, Name="水闇自然墓地",        Status="active" },
        new Deck { Id=5,  SeasonPlayerId=1, PlayerId=1, Name="白緑天門",            Status="lost"   },
        new Deck { Id=6,  SeasonPlayerId=1, PlayerId=1, Name="黒単デスザーク",      Status="lost"   },
        new Deck { Id=7,  SeasonPlayerId=2, PlayerId=2, Name="青黒コントロール",    Status="active" },
        new Deck { Id=8,  SeasonPlayerId=2, PlayerId=2, Name="赤緑アグロ",          Status="active" },
        new Deck { Id=9,  SeasonPlayerId=2, PlayerId=2, Name="白単ヘブンズゲート",  Status="active" },
        new Deck { Id=10, SeasonPlayerId=2, PlayerId=2, Name="5cネバー",            Status="lost"   },
        new Deck { Id=11, SeasonPlayerId=2, PlayerId=2, Name="黒緑速攻",            Status="lost"   },
        new Deck { Id=12, SeasonPlayerId=2, PlayerId=2, Name="水単ムートピア",      Status="lost"   },
        new Deck { Id=13, SeasonPlayerId=3, PlayerId=3, Name="赤単我我我",          Status="active" },
        new Deck { Id=14, SeasonPlayerId=3, PlayerId=3, Name="青白スコーラー",      Status="lost"   },
        new Deck { Id=15, SeasonPlayerId=3, PlayerId=3, Name="黒単アビス",          Status="lost"   },
        new Deck { Id=16, SeasonPlayerId=3, PlayerId=3, Name="緑単サソリス",        Status="lost"   },
        new Deck { Id=17, SeasonPlayerId=3, PlayerId=3, Name="水火ツインパクト",    Status="lost"   },
        new Deck { Id=18, SeasonPlayerId=3, PlayerId=3, Name="白青退化",            Status="lost"   },
    };

    public static readonly List<SeasonPlayer> SeasonPlayers = new()
    {
        // 第2回大会（進行中）
        new SeasonPlayer { Id=1, SeasonId=2, TournamentId=4, PlayerId=1, RemainingDecks=4, WinCount=7, LoseCount=2, TotalPoints=0, Player=Players[0] },
        new SeasonPlayer { Id=2, SeasonId=2, TournamentId=4, PlayerId=2, RemainingDecks=3, WinCount=5, LoseCount=4, TotalPoints=0, Player=Players[1] },
        new SeasonPlayer { Id=3, SeasonId=2, TournamentId=4, PlayerId=3, RemainingDecks=1, WinCount=3, LoseCount=6, TotalPoints=0, Player=Players[2] },
        // 第1回大会（終了）
        new SeasonPlayer { Id=4, SeasonId=2, TournamentId=3, PlayerId=1, RemainingDecks=0, WinCount=8, LoseCount=4, TotalPoints=26, Rank=1, Player=Players[0] },
        new SeasonPlayer { Id=5, SeasonId=2, TournamentId=3, PlayerId=2, RemainingDecks=0, WinCount=6, LoseCount=6, TotalPoints=15, Rank=2, Player=Players[1] },
        new SeasonPlayer { Id=6, SeasonId=2, TournamentId=3, PlayerId=3, RemainingDecks=0, WinCount=4, LoseCount=8, TotalPoints=3,  Rank=3, Player=Players[2] },
    };

    public static readonly List<Match> Matches = new()
    {
        new Match { Id=1, SeasonId=2, TournamentId=4, WinnerPlayerId=1, LoserPlayerId=3, WinnerDeckId=1, LostDeckId=18, PlayedAt=DateTime.Now.AddMinutes(-40) },
        new Match { Id=2, SeasonId=2, TournamentId=4, WinnerPlayerId=2, LoserPlayerId=3, WinnerDeckId=7, LostDeckId=17, PlayedAt=DateTime.Now.AddHours(-1)   },
        new Match { Id=3, SeasonId=2, TournamentId=4, WinnerPlayerId=1, LoserPlayerId=2, WinnerDeckId=2, LostDeckId=12, PlayedAt=DateTime.Now.AddHours(-2)   },
    };

    public static readonly List<BannedCard> BannedCards = new()
    {
        new BannedCard { Id=1, SeasonId=2, CardName="CRYMAX ジャオウガ", CreatedAt=DateTime.Now.AddDays(-10) },
        new BannedCard { Id=2, SeasonId=2, CardName="頂上龍素 記録的イニ",  CreatedAt=DateTime.Now.AddDays(-10) },
    };
}
