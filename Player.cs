namespace Console_RPG;

/*
    PlayerStatistics 用于保存玩家的全局状态数据。
    包含等级、经验、血量、攻击力等核心属性。
    所有战斗、升级、治疗、存档模块都会直接读取或修改这里的数据。
*/
public static class PlayerStatistics
{
    public static string Name = null!;   // 玩家名字
    public static int Level = 1;         // 玩家等级
    public static double Exp = 0;        // 当前经验值

    /*
        当前等级升级所需经验。
        采用"基数 + 线性成长"曲线：需求随等级线性增长，
        同时略低于怪物经验的平均增速，避免后期出现"打怪收益追不上升级需求"的卡级。
    */
    public static double ExpToNextLevel => 100 + Level * 60;

    public static double Hp = 100;       // 当前血量
    public static double MaxHp = 100;    // 最大血量
    public static double Attack = 15;    // 攻击力

    /*
        每次治疗恢复的血量。
        改为随等级成长（早期回复低、后期回复高），避免高等级时治疗量相对过强。
    */
    public static double Treatment => 20 + Level * 6;

    /*
        单场战斗中可使用的治疗次数上限。
        治疗不再完全无成本，避免"无限嗑药零风险"的最优解。
    */
    public const int MaxHealPerBattle = 3;

    /*
        升级时恢复的血量比例。
        升级不再直接回满，避免在连续刷怪中靠升级无限续航。
    */
    public const double LevelUpHealRatio = 0.35;

    /*
        升级时增加的属性。
    */
    public const double MaxHpPerLevel = 18;
    public const double AttackPerLevel = 5;

    /*
        死亡时损失的经验比例。
    */
    public const double DeathExpLossRatio = 0.1;

    /*
        把数值限制在安全区间内，防止读档或运算产生非法数值。
    */
    public static void ClampValues()
    {
        if (Level < 1) Level = 1;
        if (MaxHp <= 0) MaxHp = 100;
        if (Attack <= 0) Attack = 15;
        if (Exp < 0) Exp = 0;
        if (Hp < 0) Hp = 0;
        if (Hp > MaxHp) Hp = MaxHp;
    }
}
