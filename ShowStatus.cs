using System;

namespace Console_RPG;

/*
    ShowStatus 负责展示玩家当前状态面板。
*/
public static class ShowStatus
{
    /*
        显示玩家当前状态
    */
    public static void ShowPlayerStatus()
    {
        ConsoleHelper.Clear();
        Console.WriteLine($"你的名字是：{PlayerStatistics.Name}");
        Console.WriteLine($"等级：{PlayerStatistics.Level}");
        Console.WriteLine($"Exp:{PlayerStatistics.Exp:F0}/{PlayerStatistics.ExpToNextLevel:F0}");
        Console.WriteLine($"HP：{PlayerStatistics.Hp:F0}/{PlayerStatistics.MaxHp:F0}");
        Console.WriteLine($"攻击值：{PlayerStatistics.Attack:F0}");
        Console.WriteLine($"治疗量：{PlayerStatistics.Treatment:F0}");
        Console.WriteLine($"草药：{PlayerStatistics.Herbs}/{PlayerStatistics.MaxHerbs} 株（每次治疗消耗 1 株）");
        Console.WriteLine($"距离升级还差：{PlayerStatistics.ExpToNextLevel - PlayerStatistics.Exp:F0} 经验");

        Program.Loading();
    }
}
