// ==========================
// 存档系统
// 负责将玩家当前状态保存为 JSON 文件，并在下次启动时读取恢复
// ==========================

using System;
using System.Collections.Generic;
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
    // 存档格式版本号，用于以后判断旧存档是否需要迁移
    public int SaveVersion { get; set; } = 1;

    public string Name { get; set; } = "";   // 玩家名字
    public int Level { get; set; }           // 等级
    public double Exp { get; set; }          // 当前经验值
    public double Hp { get; set; }           // 当前血量
    public double MaxHp { get; set; }        // 最大血量
    public double Attack { get; set; }       // 攻击力
    public int? Herbs { get; set; }          // 草药数量（旧存档没有该字段时按默认值处理）

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

    /*
        候选存档路径，按优先级排列。
        1) 程序所在目录的 saves 子目录：保持旧版"存档跟着程序走"的直觉
        2) 用户数据目录：程序目录不可写（例如装在 Program Files）时兜底
        3) 旧版本写在运行目录下的 save.json：仅用于兼容读取
    */
    private static IEnumerable<string> CandidateSavePaths()
    {
        yield return Path.Combine(AppContext.BaseDirectory, SaveFolder, SaveFileName);

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrEmpty(appData))
        {
            yield return Path.Combine(appData, "Console_RPG", SaveFolder, SaveFileName);
        }

        yield return Path.Combine(Environment.CurrentDirectory, SaveFileName);
    }

    // 保存当前玩家数据到本地文件
    public static void Save()
    {
        bool saved = false;
        string lastError = "";

        foreach (string path in CandidateSavePaths())
        {
            try
            {
                // 确保存档目录存在后再写入
                string? dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // 从 PlayerStatistics 中拷贝当前玩家数据
                var data = new SaveData
                {
                    SaveVersion = 1,
                    Name = PlayerStatistics.Name,
                    Level = PlayerStatistics.Level,
                    Exp = PlayerStatistics.Exp,
                    Hp = PlayerStatistics.Hp,
                    MaxHp = PlayerStatistics.MaxHp,
                    Attack = PlayerStatistics.Attack,
                    Herbs = PlayerStatistics.Herbs,
                    Treatment = PlayerStatistics.Treatment
                };

                // 将数据序列化为 JSON 字符串
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true // 格式化输出，方便查看
                });

                File.WriteAllText(path, json);

                Console.WriteLine("游戏已保存");
                Console.WriteLine($"存档位置：{path}");
                saved = true;
                break;
            }
            catch (Exception e)
            {
                lastError = e.Message;
            }
        }

        if (!saved)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"保存失败：{lastError}");
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

            // 还原玩家数据
            PlayerStatistics.Name = string.IsNullOrWhiteSpace(data.Name) ? PlayerStatistics.Name : data.Name;
            PlayerStatistics.Level = data.Level;
            PlayerStatistics.Exp = data.Exp;
            PlayerStatistics.Hp = data.Hp;
            PlayerStatistics.MaxHp = data.MaxHp;
            PlayerStatistics.Attack = data.Attack;
            PlayerStatistics.Herbs = data.Herbs ?? PlayerStatistics.Herbs;

            // 修正可能出现的非法数值（老存档或被手改过的存档）
            PlayerStatistics.ClampValues();

            // 等级与属性必须自洽：以等级推导的数值为下限，
            // 避免手改存档后出现"等级很高但血量攻击力还是很低"的崩坏数值
            PlayerStatistics.MaxHp = Math.Max(PlayerStatistics.MaxHp, PlayerStatistics.MaxHpForLevel(PlayerStatistics.Level));
            PlayerStatistics.Attack = Math.Max(PlayerStatistics.Attack, PlayerStatistics.AttackForLevel(PlayerStatistics.Level));
            if (PlayerStatistics.Hp > PlayerStatistics.MaxHp)
            {
                PlayerStatistics.Hp = PlayerStatistics.MaxHp;
            }

            Console.WriteLine("存档读取成功！");

            // 读档相当于重新开始，允许再次读取输入
            ConsoleHelper.ResetInputEnded();
            Program.ResetMeditation();
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
        foreach (string path in CandidateSavePaths())
        {
            if (File.Exists(path)) return path;
        }

        return null;
    }
}
