using System;

namespace Console_RPG;

/*
    UpLevel 负责管理玩家经验获取与升级逻辑。
    当玩家获得经验后，会自动判断是否满足升级条件并执行升级。
*/
public static class UpLevel
{
    /*
        增加玩家经验值
        并检查是否需要连续升级
    */
    public static void GainExp(double getExp)
    {
        // 增加经验
        PlayerStatistics.Exp += getExp;
        Console.WriteLine($"获得经验 {getExp:F0} 点");

        // 经验溢出时连续升级（每次升级只升一级）
        while (PlayerStatistics.Exp >= PlayerStatistics.ExpToNextLevel)
        {
            LevelUp();
        }
    }

    /*
        执行一次等级提升
        提升等级并刷新玩家属性
    */
    private static void LevelUp()
    {
        // 扣除本级所需经验
        PlayerStatistics.Exp -= PlayerStatistics.ExpToNextLevel;

        // 等级提升
        PlayerStatistics.Level++;

        // 提升属性
        PlayerStatistics.MaxHp += PlayerStatistics.MaxHpPerLevel;   // 最大生命增加
        PlayerStatistics.Attack += PlayerStatistics.AttackPerLevel;   // 攻击力增加

        // 升级只恢复部分血量，避免靠连续升级无限续航
        double recovered = PlayerStatistics.MaxHp * PlayerStatistics.LevelUpHealRatio;
        PlayerStatistics.Hp = Math.Round(PlayerStatistics.Hp + recovered, 2);
        if (PlayerStatistics.Hp > PlayerStatistics.MaxHp)
        {
            PlayerStatistics.Hp = PlayerStatistics.MaxHp;
        }

        Console.WriteLine(" ");
        Console.WriteLine("🎉 升级了！");
        Console.WriteLine($"当前等级：{PlayerStatistics.Level}");
        Console.WriteLine($"最大HP +{PlayerStatistics.MaxHpPerLevel:F0}\n攻击值 +{PlayerStatistics.AttackPerLevel:F0}\n恢复 {recovered:F0} HP");
        Console.WriteLine($"治疗量提升至 {PlayerStatistics.Treatment:F0}");

        Program.Loading();
    }
}
