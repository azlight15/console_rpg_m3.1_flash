using System;
using System.Text;

namespace Console_RPG;

/*
    Program 是游戏的入口类。
    负责初始化游戏、显示菜单，并控制整个游戏主循环。
*/
public static class Program
{
    // 玩家名字最大长度
    private const int MaxNameLength = 16;

    // 打坐修炼的基础收益与连续打坐的递减幅度
    private const int MeditationBaseExp = 40;
    private const int MeditationDecay = 10;

    // 连续打坐次数（收益递减计数）
    private static int _meditationStreak = 0;

    /*
        游戏入口方法
        初始化菜单并进入主循环
    */
    private static void Main()
    {
        // 输入流已结束（例如以重定向方式运行）时无法开始游戏
        if (!StartMenu())
        {
            return;
        }

        GameConfirmed(); // 显示初始状态确认

        // 游戏主循环
        OptionsMenu();
    }

    /*
        开始菜单
        负责获取玩家名字并做基本校验
        返回 false 表示标准输入已结束，游戏无法继续
    */
    private static bool StartMenu()
    {
        ConsoleHelper.Clear();
        Console.WriteLine("===== 欢迎来玩Console RPG游戏 =====");
        Console.WriteLine($"当前版本：{GameInfo.FullVersion}");
        Console.WriteLine("此游戏是控制台游戏，没有ui");
        Console.WriteLine("那么接下来请好好享受游戏吧！");
        Console.WriteLine("=================================");
        Console.Write("请输入你的名字（取了名字后不能更改！）：");

        while (true)
        {
            string? input = Console.ReadLine();

            // 标准输入已结束：标记并退出，避免陷入"反复读到 null"的死循环
            if (input == null)
            {
                ConsoleHelper.MarkInputEnded();
                Console.WriteLine();
                Console.WriteLine("检测到输入已结束，游戏无法继续。");
                return false;
            }

            string name = SanitizeName(input);

            // 防止输入空字符串或空格
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("\n不能只输入空格或直接回车！请重新输入你的名字：");
                Console.ResetColor();
                continue;
            }

            PlayerStatistics.Name = name;
            return true;
        }
    }

    /*
        清洗玩家名字：
        去掉首尾空白、剔除控制字符（例如 ANSI 转义序列）、限制最大长度。
    */
    private static string SanitizeName(string raw)
    {
        StringBuilder sb = new StringBuilder(raw.Length);

        foreach (char c in raw.Trim())
        {
            if (char.IsControl(c)) continue;
            sb.Append(c);
        }

        string name = sb.ToString().Trim();

        if (name.Length > MaxNameLength)
        {
            name = name.Substring(0, MaxNameLength);
        }

        return name;
    }

    /*
        游戏开始前的确认页面
        展示玩家初始属性
    */
    private static void GameConfirmed()
    {
        ConsoleHelper.Clear();
        Console.WriteLine("=================================");
        Console.WriteLine("正式游玩之前先看一下玩家状态");
        Console.WriteLine($"你的名字是：{PlayerStatistics.Name}");
        Console.WriteLine($"等级：{PlayerStatistics.Level}");
        Console.WriteLine($"HP：{PlayerStatistics.Hp:F0}/{PlayerStatistics.MaxHp:F0}");
        Console.WriteLine($"攻击值：{PlayerStatistics.Attack}");
        Console.WriteLine($"草药：{PlayerStatistics.Herbs} 株（每次治疗消耗 1 株）");
        Console.WriteLine($"那么祝你玩的开心，{PlayerStatistics.Name}勇者！");
        Console.WriteLine("=================================");
        Console.WriteLine("按下任意键开始游戏");
        WaitKey();
    }

    /*
        主菜单页面
        根据玩家输入分发到不同功能模块
    */
    private static void OptionsMenu()
    {
        while (true)
        {
            ConsoleHelper.Clear();
            Console.WriteLine("=========================");
            Console.WriteLine($"Console RPG  ({GameInfo.FullVersion})");
            Console.WriteLine("请选择选项：");
            Console.WriteLine("1.开始对战");
            Console.WriteLine("2.打坐修炼");
            Console.WriteLine("3.治疗");
            Console.WriteLine("4.查看状态");
            Console.WriteLine("5.存档");
            Console.WriteLine("6.读档");
            Console.WriteLine("7.退出游戏");
            Console.WriteLine("==========================");
            Console.Write("请选择选项：");

            // 使用 TryParse 读取，避免输入非数字时直接抛出异常导致游戏崩溃
            string? raw = Console.ReadLine();

            // 标准输入已结束：标记后直接退出，避免反复读到 null
            if (raw == null)
            {
                ConsoleHelper.MarkInputEnded();
                return;
            }

            if (!int.TryParse(raw, out int options))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("输入错误，请输入 1~7 之间的数字！");
                Console.ResetColor();
                Console.WriteLine("按任意键重新选择");
                ConsoleHelper.WaitAnyKey();
                continue;
            }

            if (options < 1 || options > 7)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("输入错误，请输入 1~7 之间的数字！");
                Console.ResetColor();
                Console.WriteLine("按任意键重新选择");
                ConsoleHelper.WaitAnyKey();
                continue;
            }

            switch (options)
            {
                case 1:
                    Battle.StartBattle();   // 进入战斗模块
                    break;
                case 2:
                    Meditate();              // 打坐修炼
                    break;
                case 3:
                    Rest();                 // 休息治疗
                    break;
                case 4:
                    ShowStatus.ShowPlayerStatus();  // 显示玩家状态
                    break;
                case 5:
                    SaveManager.Save();     // 保存游戏
                    break;
                case 6:
                    SaveManager.Load();     // 读取存档
                    break;
                case 7:
                    ConsoleHelper.Clear();
                    Console.WriteLine("欢迎再次玩Console RPG，谢谢");
                    Console.WriteLine("那么下次再见，勇者！");
                    return;                  // 结束主循环
            }

            // 输入流结束后不再继续刷新菜单
            if (ConsoleHelper.InputEnded)
            {
                return;
            }
        }
    }

    /*
        打坐修炼
        收益随连续打坐次数递减，避免"坐着不动也能稳定刷等级"
    */
    private static void Meditate()
    {
        ConsoleHelper.Clear();

        int gain = MeditationBaseExp - MeditationDecay * _meditationStreak;
        if (gain < 0) gain = 0;

        _meditationStreak++;

        if (gain <= 0)
        {
            Console.WriteLine("你已精疲力竭，这次打坐没有任何收获。");
            Console.WriteLine("去打个怪或读个档休息一下吧。");
        }
        else
        {
            UpLevel.GainExp(gain, pauseWhenNoLevelUp: true);
        }
    }

    /*
        重置打坐的连续计数。
        打怪之后精力恢复，可以重新从最高收益开始打坐。
    */
    public static void ResetMeditation()
    {
        _meditationStreak = 0;
    }

    /*
        在菜单中休整并回复血量
    */
    private static void Rest()
    {
        ConsoleHelper.Clear();

        if (PlayerStatistics.Hp >= PlayerStatistics.MaxHp)
        {
            Console.WriteLine("你的HP已满，无需治疗");
        }
        else if (PlayerStatistics.Herbs <= 0)
        {
            Console.WriteLine("你没有草药了，无法治疗");
            Console.WriteLine("去打个怪打点草药回来吧。");
        }
        else if (Heal.TryHeal(out double recovered))
        {
            Console.WriteLine($"你使用了一株草药，恢复 {recovered:F0} HP（剩余草药 {PlayerStatistics.Herbs}）");
        }
        else
        {
            Console.WriteLine("治疗失败，你没有草药了");
        }

        Program.Loading();
    }

    /*
        等待用户输入后返回主菜单
    */
    public static void Loading()
    {
        Console.WriteLine("按下任意键回到选择页面");
        WaitKey();
    }

    /*
        等待一次按键。
        当输入被重定向（例如脚本自动测试）时没有按键可读，改为读取一行文本。
    */
    private static void WaitKey()
    {
        ConsoleHelper.WaitAnyKey();
    }
}
