using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DmLeagueManager.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<DmLeagueManager.App>("#app");

// ============================================
// ★ モック切り替えはここだけ変える
// true  = Supabase不要、ダミーデータで動く（開発中）
// false = Supabase接続（本番）
// ============================================
var useMock = false;

builder.Services.AddSingleton(new AppConfig { UseMock = useMock });

if (!useMock)
{
    var supabaseUrl = builder.Configuration["Supabase:Url"] ?? "";
    var supabaseKey = builder.Configuration["Supabase:AnonKey"] ?? "";

    var supabaseClient = new Supabase.Client(
        supabaseUrl,
        supabaseKey,
        new Supabase.SupabaseOptions { AutoConnectRealtime = false }
    );
    await supabaseClient.InitializeAsync();
    builder.Services.AddSingleton(supabaseClient);
}
else
{
    // モック時はダミーのClientを登録（DIエラー回避）
    builder.Services.AddSingleton(new Supabase.Client(
        "https://dummy.supabase.co",
        "dummy-key",
        new Supabase.SupabaseOptions { AutoConnectRealtime = false }
    ));
}

builder.Services.AddScoped<SeasonService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<MatchService>();
builder.Services.AddScoped<DeckService>();
builder.Services.AddScoped<TournamentService>();
builder.Services.AddScoped<DeckStatsService>();
builder.Services.AddScoped<BannedCardService>();

await builder.Build().RunAsync();
