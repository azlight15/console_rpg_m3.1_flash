// ==========================
// 怪物工厂
// 负责根据玩家等级随机生成战斗怪物
// ==========================

using System;

namespace Console_RPG;

/*
    MonsterFactory 用于创建战斗中使用的怪物对象。
    每次调用都会随机生成一种怪物，并根据玩家等级进行数值缩放。
    同时存在一定概率生成精英怪物。
*/
public static class MonsterFactory
{
    // 精英怪出现概率（10%）
    private const int EliteChance = 10;

    // 精英怪额外提升的等级
    private const int EliteExtraLevel = 3;

    // 精英怪属性倍率
    private const double EliteMultiplier = 1.5;

    /*
        对外提供获取怪物的入口
        每次访问都会创建一个新的怪物实例
    */
    public static MonsterStatistics Monster => CreateMonster();

    /*
        创建怪物实例
        根据玩家等级动态计算怪物属性
    */
    private static MonsterStatistics CreateMonster()
    {
        // 随机怪物类型（1~3）
        int type = Random.Shared.Next(1, 4);

        // 是否生成精英怪（10%概率）
        bool isElite = Random.Shared.Next(100) < EliteChance;

        // 获取玩家等级用于数值缩放
        int playerLevel = PlayerStatistics.Level;

        MonsterStatistics monster = new MonsterStatistics();

        // 精英怪的等级直接定在"玩家等级 + 3"
        int level = isElite ? playerLevel + EliteExtraLevel : playerLevel;

        // 依类型确定基础等级偏移
        switch (type)
        {
            case 1:
                monster.Name = "史莱姆";
                level = isElite ? level : Math.Max(1, playerLevel - 1);
                break;

            case 2:
                monster.Name = "哥布林";
                level = isElite ? level : playerLevel;
                break;

            case 3:
                monster.Name = "骷髅兵";
                level = isElite ? level : playerLevel + 1;
                break;
        }

        // 精英怪额外提升等级
        if (isElite)
        {
            level = playerLevel + EliteExtraLevel;
            monster.Name = "[精英]" + monster.Name;
        }

        // 先确定最终等级，再据此计算属性，保证等级与数值始终匹配
        monster.Level = level;
        ApplyStats(monster, type);

        // 精英怪在正确等级的基础上再整体强化
        if (isElite)
        {
            monster.MaxHp *= EliteMultiplier;
            monster.Attack *= EliteMultiplier;
            monster.ExpReward *= EliteMultiplier;
        }

        // 初始化当前血量
        monster.Hp = monster.MaxHp;

        return monster;
    }

    /*
        按怪物类型与最终等级填充属性
    */
    private static void ApplyStats(MonsterStatistics monster, int type)
    {
        int level = monster.Level;

        switch (type)
        {
            case 1:
                monster.MaxHp = 30 + level * 4;
                monster.Attack = 5 + level * 1;
                monster.ExpReward = 30 + level * 12;
                break;

            case 2:
                monster.MaxHp = 50 + level * 5;
                monster.Attack = 8 + level * 2;
                monster.ExpReward = 50 + level * 16;
                break;

            case 3:
                monster.MaxHp = 70 + level * 6;
                monster.Attack = 10 + level * 3;
                monster.ExpReward = 70 + level * 20;
                break;
        }
    }
}
