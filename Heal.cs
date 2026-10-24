using System;

namespace Console_RPG;

/*
    Heal 负责所有"治疗"行为的统一实现。
    菜单中的治疗与战斗中的治疗共用同一份逻辑，避免数值与提示不一致。
*/
public class Heal
{
    /*
        对玩家执行一次治疗。
        返回实际恢复的血量，若血量已满则返回 0。
    */
    public static double HealPlayer()
    {
        // 血量已满时不再浪费治疗次数
        if (PlayerStatistics.Hp >= PlayerStatistics.MaxHp)
        {
            return 0;
        }

        double before = PlayerStatistics.Hp;

        PlayerStatistics.Hp += PlayerStatistics.Treatment;

        // 防止血量超过最大值
        if (PlayerStatistics.Hp > PlayerStatistics.MaxHp)
        {
            PlayerStatistics.Hp = PlayerStatistics.MaxHp;
        }

        // 保留两位小数，避免浮点误差累积
        PlayerStatistics.Hp = Math.Round(PlayerStatistics.Hp, 2);

        return Math.Round(PlayerStatistics.Hp - before, 2);
    }
}
