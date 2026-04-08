namespace DmLeagueManager.Services;

public class AppConfig
{
    /// <summary>
    /// trueの間はSupabaseに繋がずモックデータを返す。
    /// Supabase接続準備ができたらfalseに変える。
    /// </summary>
    public bool UseMock { get; set; } = true;
    ///password = w#8RP2F2TE/ykz
}
