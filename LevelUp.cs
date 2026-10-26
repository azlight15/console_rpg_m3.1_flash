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

        pauseWhenNoLevelUp：
        当本次经验不足以升级时，是否要停下来让玩家看清提示。
        战斗流程里紧接着还有"是否继续刷怪"的提示，不需要额外停顿；
        菜单里的"打坐修炼"则需要停顿，否则提示会被清屏吞掉。
    */
    public static void GainExp(double getExp, bool pauseWhenNoLevelUp = false)
    {
        // 增加经验
        PlayerStatistics.Exp += getExp;
        Console.WriteLine($"获得经验 {getExp:F0} 点");

        int levelUps = 0;

        // 经验溢出时连续升级（每次升级只升一级）
        while (PlayerStatistics.Exp >= PlayerStatistics.ExpToNextLevel)
        {
            LevelUp();
            levelUps++;
        }

        // 没升级时也要让玩家看到经验提示
        if (levelUps == 0 && pauseWhenNoLevelUp)
        {
            Program.Loading();
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

        // 升级赠送草药，作为持续恢复的稳定来源
        int herbGained = PlayerStatistics.AddHerbs(PlayerStatistics.HerbsPerLevelUp);

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
        if (herbGained > 0)
        {
            Console.WriteLine($"获得草药 {herbGained} 株（当前 {PlayerStatistics.Herbs} 株）");
        }

        Program.Loading();
    }
}
