namespace Console_RPG;

/*
    GameInfo 保存游戏的版本信息。
    版本号在这里集中维护，界面显示与程序集版本都从这里取值，避免各处写死后不一致。
*/
public static class GameInfo
{
    // 版本号（遵循 语义化版本：主版本.次版本.修订号）
    public const string Version = "1.1.0";

    // 版本名称，用于界面显示
    public const string VersionName = "Flash";

    // 完整的版本标识，例如 "Flash v1.1.0"
    public const string FullVersion = $"{VersionName} v{Version}";
}
