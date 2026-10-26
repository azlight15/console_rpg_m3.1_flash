using System;

namespace Console_RPG;

/*
    Heal 负责所有"治疗"行为的统一实现。
    菜单中的治疗与战斗中的治疗共用同一份逻辑，避免数值与提示不一致。
    每次治疗都会消耗 1 株草药，草药是治疗的唯一成本。
*/
public static class Heal
{
    /*
        对玩家执行一次治疗。
        返回实际恢复的血量，若血量已满则返回 0。
        本方法不负责扣除草药，由调用方通过 TryHeal 统一处理。
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

    /*
        尝试治疗一次。
        血量已满或没有草药时返回 false，且不会消耗草药。
        返回 true 时草药已扣除，recovered 为实际恢复的血量。
    */
    public static bool TryHeal(out double recovered)
    {
        recovered = 0;

        // 血量已满，不需要治疗，也不该浪费草药
        if (PlayerStatistics.Hp >= PlayerStatistics.MaxHp)
        {
            return false;
        }

        // 没有草药，无法治疗
        if (PlayerStatistics.Hp < PlayerStatistics.MaxHp && PlayerStatistics.Herbs <= 0)
        {
            return false;
        }

        if (!PlayerStatistics.UseHerb())
        {
            return false;
        }

        recovered = HealPlayer();
        return true;
    }
}
