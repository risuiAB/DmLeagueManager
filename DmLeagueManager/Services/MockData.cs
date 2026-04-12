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
        new Season { Id = 1, Name = "2024年シーズン", Status = "finished", RankPoints = "[3, 1, 0]", CreatedAt = DateTime.Now.AddMonths(-6) },
        new Season { Id = 2, Name = "2025年シーズン", Status = "active",   RankPoints = "[3, 1, 0]", CreatedAt = DateTime.Now.AddDays(-30) },
    };

    public static readonly List<Tournament> Tournaments = new()
    {
        new Tournament { Id=1, SeasonId=1, Name="第1回大会", DeckCount=6, Status="finished", CreatedAt=DateTime.Now.AddMonths(-5) },
        new Tournament { Id=2, SeasonId=1, Name="第2回大会", DeckCount=6, Status="finished", CreatedAt=DateTime.Now.AddMonths(-4) },
        new Tournament { Id=3, SeasonId=2, Name="第1回大会", DeckCount=6, Status="finished", CreatedAt=DateTime.Now.AddDays(-20) },
        new Tournament { Id=4, SeasonId=2, Name="第2回大会", DeckCount=6, Status="active",   CreatedAt=DateTime.Now.AddDays(-5) },
    };

    public static readonly List<SeasonPlayer> SeasonPlayers = new()
    {
        new SeasonPlayer { Id=1, SeasonId=2, TournamentId=4, PlayerId=1, RemainingDecks=4, WinCount=7, LoseCount=2, TotalPoints=0, Player=Players[0] },
        new SeasonPlayer { Id=2, SeasonId=2, TournamentId=4, PlayerId=2, RemainingDecks=3, WinCount=5, LoseCount=4, TotalPoints=0, Player=Players[1] },
        new SeasonPlayer { Id=3, SeasonId=2, TournamentId=4, PlayerId=3, RemainingDecks=1, WinCount=3, LoseCount=6, TotalPoints=0, Player=Players[2] },
        new SeasonPlayer { Id=4, SeasonId=2, TournamentId=3, PlayerId=1, RemainingDecks=0, WinCount=8, LoseCount=4, TotalPoints=26, Rank=1, Player=Players[0] },
        new SeasonPlayer { Id=5, SeasonId=2, TournamentId=3, PlayerId=2, RemainingDecks=0, WinCount=6, LoseCount=6, TotalPoints=15, Rank=2, Player=Players[1] },
        new SeasonPlayer { Id=6, SeasonId=2, TournamentId=3, PlayerId=3, RemainingDecks=0, WinCount=4, LoseCount=8, TotalPoints=3,  Rank=3, Player=Players[2] },
    };

    // デッキマスター（プレイヤーに紐づく）
    public static readonly List<Deck> Decks = new()
    {
        new Deck { Id=1,  PlayerId=1, Name="アナカラーハンデス" },
        new Deck { Id=2,  PlayerId=1, Name="赤白轟轟轟" },
        new Deck { Id=3,  PlayerId=1, Name="5cコントロール" },
        new Deck { Id=4,  PlayerId=1, Name="水闇自然墓地" },
        new Deck { Id=5,  PlayerId=1, Name="白緑天門" },
        new Deck { Id=6,  PlayerId=1, Name="黒単デスザーク" },
        new Deck { Id=7,  PlayerId=2, Name="青黒コントロール" },
        new Deck { Id=8,  PlayerId=2, Name="赤緑アグロ" },
        new Deck { Id=9,  PlayerId=2, Name="白単ヘブンズゲート" },
        new Deck { Id=10, PlayerId=2, Name="5cネバー" },
        new Deck { Id=11, PlayerId=2, Name="黒緑速攻" },
        new Deck { Id=12, PlayerId=2, Name="水単ムートピア" },
        new Deck { Id=13, PlayerId=3, Name="赤単我我我" },
        new Deck { Id=14, PlayerId=3, Name="青白スコーラー" },
        new Deck { Id=15, PlayerId=3, Name="黒単アビス" },
        new Deck { Id=16, PlayerId=3, Name="緑単サソリス" },
        new Deck { Id=17, PlayerId=3, Name="水火ツインパクト" },
        new Deck { Id=18, PlayerId=3, Name="白青退化" },
    };

    // 大会ごとのデッキ使用状況
    public static readonly List<TournamentDeck> TournamentDecks = new()
    {
        // 第2回大会（進行中）- 鈴木
        new TournamentDeck { Id=1,  SeasonPlayerId=1, DeckId=1,  Status="active" },
        new TournamentDeck { Id=2,  SeasonPlayerId=1, DeckId=2,  Status="active" },
        new TournamentDeck { Id=3,  SeasonPlayerId=1, DeckId=3,  Status="active" },
        new TournamentDeck { Id=4,  SeasonPlayerId=1, DeckId=4,  Status="active" },
        new TournamentDeck { Id=5,  SeasonPlayerId=1, DeckId=5,  Status="lost" },
        new TournamentDeck { Id=6,  SeasonPlayerId=1, DeckId=6,  Status="lost" },
        // 第2回大会（進行中）- 佐藤
        new TournamentDeck { Id=7,  SeasonPlayerId=2, DeckId=7,  Status="active" },
        new TournamentDeck { Id=8,  SeasonPlayerId=2, DeckId=8,  Status="active" },
        new TournamentDeck { Id=9,  SeasonPlayerId=2, DeckId=9,  Status="active" },
        new TournamentDeck { Id=10, SeasonPlayerId=2, DeckId=10, Status="lost" },
        new TournamentDeck { Id=11, SeasonPlayerId=2, DeckId=11, Status="lost" },
        new TournamentDeck { Id=12, SeasonPlayerId=2, DeckId=12, Status="lost" },
        // 第2回大会（進行中）- 田中
        new TournamentDeck { Id=13, SeasonPlayerId=3, DeckId=13, Status="active" },
        new TournamentDeck { Id=14, SeasonPlayerId=3, DeckId=14, Status="lost" },
        new TournamentDeck { Id=15, SeasonPlayerId=3, DeckId=15, Status="lost" },
        new TournamentDeck { Id=16, SeasonPlayerId=3, DeckId=16, Status="lost" },
        new TournamentDeck { Id=17, SeasonPlayerId=3, DeckId=17, Status="lost" },
        new TournamentDeck { Id=18, SeasonPlayerId=3, DeckId=18, Status="lost" },
    };

    public static readonly List<Match> Matches = new()
    {
        new Match { Id=1, SeasonId=2, TournamentId=4, WinnerPlayerId=1, LoserPlayerId=3, WinnerDeckId=1, LostDeckId=18, PlayedAt=DateTime.Now.AddMinutes(-40) },
        new Match { Id=2, SeasonId=2, TournamentId=4, WinnerPlayerId=2, LoserPlayerId=3, WinnerDeckId=7, LostDeckId=17, PlayedAt=DateTime.Now.AddHours(-1) },
        new Match { Id=3, SeasonId=2, TournamentId=4, WinnerPlayerId=1, LoserPlayerId=2, WinnerDeckId=2, LostDeckId=12, PlayedAt=DateTime.Now.AddHours(-2) },
    };

    public static readonly List<BannedCard> BannedCards = new()
    {
        new BannedCard { Id=1, SeasonId=2, CardName="CRYMAX ジャオウガ", CreatedAt=DateTime.Now.AddDays(-10) },
        new BannedCard { Id=2, SeasonId=2, CardName="頂上龍素 記録的イニ", CreatedAt=DateTime.Now.AddDays(-10) },
    };
}
