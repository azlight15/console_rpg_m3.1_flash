using System;

namespace Console_RPG;

/*
    Program 是游戏的入口类。
    负责初始化游戏、显示菜单，并控制整个游戏主循环。
*/
public static class Program
{
    // 控制游戏是否继续运行
    private static bool _running = true;

    /*
        游戏入口方法
        初始化菜单并进入主循环
    */
    private static void Main()
    {
        StartMenu();     // 输入玩家名字
        GameConfirmed(); // 显示初始状态确认

        // 游戏主循环
        while (_running)
        {
            OptionsMenu();
        }
    }

    /*
        开始菜单
        负责获取玩家名字并做基本校验
    */
    private static void StartMenu()
    {
        ConsoleHelper.Clear();
        Console.WriteLine("===== 欢迎来玩Console RPG游戏 =====");
        Console.WriteLine($"当前版本：{GameInfo.FullVersion}");
        Console.WriteLine("此游戏是控制台游戏，没有ui");
        Console.WriteLine("那么接下来请好好享受游戏吧！");
        Console.WriteLine("=================================");
        Console.Write("请输入你的名字（取了名字后不能更改！）：");

        PlayerStatistics.Name = Console.ReadLine()!;

        // 防止输入空字符串或空格
        while (string.IsNullOrWhiteSpace(PlayerStatistics.Name))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("\n不能只输入空格或直接回车！请重新输入你的名字：");
            Console.ResetColor();
            PlayerStatistics.Name = Console.ReadLine()!;
        }
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
        while (_running)
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
            if (!int.TryParse(Console.ReadLine(), out int options))
            {
                // 输入流已结束，无法再读取，直接退出游戏
                if (ConsoleHelper.InputEnded)
                {
                    _running = false;
                    break;
                }

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
                    UpLevel.GainExp(60);     // 打坐修炼获得少量经验
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
                    _running = false;       // 结束主循环
                    break;
            }

            // 输入流结束后不再继续刷新菜单
            if (ConsoleHelper.InputEnded)
            {
                _running = false;
            }
        }
    }

    /*
        在菜单中休整并回复血量
    */
    private static void Rest()
    {
        ConsoleHelper.Clear();

        double recovered = Heal.HealPlayer();

        if (recovered <= 0)
        {
            Console.WriteLine("你的HP已满，无需治疗");
        }
        else
        {
            Console.WriteLine($"你治疗了自己，恢复 {recovered:F0} HP");
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
