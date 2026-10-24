// ==========================
// 存档系统
// 负责将玩家当前状态保存为 JSON 文件，并在下次启动时读取恢复
// ==========================

using System;
using System.IO;
using System.Text.Json;

namespace Console_RPG;

/*
    SaveData 是用于序列化存档的数据结构。
    它从 PlayerStatistics 复制数据，用来写入 JSON 文件。
    读取存档时再把数据还原回 PlayerStatistics。
*/
public class SaveData
{
    public string Name { set; get; } = "";   // 玩家名字
    public int Level { get; set; }           // 等级
    public double Exp { get; set; }          // 当前经验值
    public double Hp { get; set; }           // 当前血量
    public double MaxHp { get; set; }        // 最大血量
    public double Attack { get; set; }       // 攻击力

    // 旧版本存档遗留字段：治疗值现已由等级推导，读档时忽略
    public double Treatment { get; set; }
}

/*
    SaveManager 负责存档的保存与读取。
    将玩家数据序列化为 JSON 文件，或从 JSON 文件中恢复数据。
*/
public static class SaveManager
{
    // 存档文件夹名
    private const string SaveFolder = "saves";

    // 存档文件名
    private const string SaveFileName = "save.json";

    // 旧版本把存档直接写在运行目录下，这里保留用于兼容读取
    private static string LegacySavePath => SaveFileName;

    /*
        当前存档文件路径。
        固定到程序所在目录，避免因启动时工作目录不同导致"读不到自己的存档"。
    */
    private static string SavePath
    {
        get
        {
            string dir = Path.Combine(AppContext.BaseDirectory, SaveFolder);
            return Path.Combine(dir, SaveFileName);
        }
    }

    // 保存当前玩家数据到本地文件
    public static void Save()
    {
        try
        {
            // 从 PlayerStatistics 中拷贝当前玩家数据
            var data = new SaveData
            {
                Name = PlayerStatistics.Name,
                Level = PlayerStatistics.Level,
                Exp = PlayerStatistics.Exp,
                Hp = PlayerStatistics.Hp,
                MaxHp = PlayerStatistics.MaxHp,
                Attack = PlayerStatistics.Attack,
                Treatment = PlayerStatistics.Treatment
            };

            // 将数据序列化为 JSON 字符串
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true // 格式化输出，方便查看
            });

            // 确保存档目录存在后再写入
            string dir = Path.GetDirectoryName(SavePath)!;
            Directory.CreateDirectory(dir);

            File.WriteAllText(SavePath, json);

            Console.WriteLine("游戏已保存");
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"保存失败：{e.Message}");
            Console.ResetColor();
        }

        Program.Loading();
    }

    // 从本地文件读取存档并恢复玩家数据
    public static void Load()
    {
        string? path = ResolveSavePath();

        // 如果不存在存档文件则提示
        if (path == null)
        {
            Console.WriteLine("没有找到存档文件");
            Program.Loading();
            return;
        }

        try
        {
            // 读取 JSON 文本
            string json = File.ReadAllText(path);

            // 反序列化为 SaveData 对象
            var data = JsonSerializer.Deserialize<SaveData>(json);

            if (data == null)
            {
                Console.WriteLine("存档内容为空，无法读取");
                Program.Loading();
                return;
            }

            // 将存档数据还原回 PlayerStatistics
            PlayerStatistics.Name = string.IsNullOrWhiteSpace(data.Name) ? "勇者" : data.Name;
            PlayerStatistics.Level = data.Level;
            PlayerStatistics.Exp = data.Exp;
            PlayerStatistics.Hp = data.Hp;
            PlayerStatistics.MaxHp = data.MaxHp;
            PlayerStatistics.Attack = data.Attack;

            // 修正可能出现的非法数值（老存档或被手改过的存档）
            PlayerStatistics.ClampValues();

            Console.WriteLine("存档读取成功！");
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"读取存档失败：{e.Message}");
            Console.ResetColor();
        }

        Program.Loading();
    }

    /*
        查找可用的存档文件。
        优先使用新目录下的存档，其次回退到旧版本写在运行目录下的存档。
        都没有时返回 null。
    */
    private static string? ResolveSavePath()
    {
        if (File.Exists(SavePath)) return SavePath;
        if (File.Exists(LegacySavePath)) return LegacySavePath;
        return null;
    }
}
