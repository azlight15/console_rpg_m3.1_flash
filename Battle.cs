// ==========================
// 对战系统
// 负责处理玩家与怪物之间的完整战斗流程
// ==========================

using System;

namespace Console_RPG;

/*
    Battle 模块负责控制战斗入口、战斗流程与战斗结果结算。
    包括刷怪循环、战斗操作、胜负判断以及经验奖励。
*/
public static class Battle
{
    /*
        玩家承受伤害时的减伤上限。
        减伤 = 5% × 玩家等级，最高 65%（即怪物伤害至少保留 35%）。
    */
    private const double MaxMitigation = 0.65;

    /*
        开始战斗入口
        控制是否继续刷怪的整体流程
    */
    public static void StartBattle()
    {
        // 玩家血量不足时无法进入战斗
        if (PlayerStatistics.Hp <= 0)
        {
            Console.WriteLine("你的HP不足，帮你回到选择页面");
            Program.Loading();
            return;
        }

        // 玩家存活时可以连续刷怪
        while (PlayerStatistics.Hp > 0)
        {
            // 从怪物工厂随机生成怪物
            var monster = MonsterFactory.Monster;

            // 进入单次战斗流程
            bool playerAlive = Start(monster);

            // 玩家倒下：结束刷怪循环，回到菜单
            if (!playerAlive)
            {
                Program.Loading();
                return;
            }

            if (ConsoleHelper.InputEnded)
            {
                return;
            }

            Console.WriteLine("是否继续刷怪？（选择Y/y则继续，否则返回菜单）");
            char choice = ConsoleHelper.ReadKeyChar();

            // 非 Y 则退出刷怪循环
            if (choice != 'Y' && choice != 'y')
            {
                Program.Loading();
                break;
            }
        }
    }

    /*
        单次战斗流程
        玩家与指定怪物进行回合制战斗
        返回玩家是否存活（倒下返回 false）
    */
    private static bool Start(MonsterStatistics monster)
    {
        // 显示怪物信息面板
        ConsoleHelper.Clear();
        Console.WriteLine($"你遇到了 {monster.Name}");
        Console.WriteLine($"等级：{monster.Level}");
        Console.WriteLine($"血量 {monster.Hp:F0}/{monster.MaxHp:F0}");
        Console.WriteLine($"攻击力 {monster.Attack}");
        Console.WriteLine($"预计获得经验值：{monster.ExpReward}");
        if (monster.HerbDrop > 0)
        {
            Console.WriteLine($"预计掉落草药：{monster.HerbDrop} 株");
        }
        Console.WriteLine("按任意键进入战斗！");
        ConsoleHelper.WaitAnyKey();

        bool monsterCanBattle = true;

        // 本场战斗可使用的治疗次数（跨战斗不累计）
        int healUsed = 0;

        // 战斗主循环：双方存活并且没有撤退
        while ((PlayerStatistics.Hp > 0 && monster.Hp > 0) && monsterCanBattle)
        {
            ConsoleHelper.Clear();

            // 显示双方状态
            Console.WriteLine($"{PlayerStatistics.Name}");
            Console.WriteLine($"{PlayerStatistics.Level}");
            Console.WriteLine($"{PlayerStatistics.Hp:F0}/{PlayerStatistics.MaxHp:F0}    草药：{PlayerStatistics.Herbs}");
            Console.WriteLine("=======================");
            Console.WriteLine($"{monster.Name}");
            Console.WriteLine($"{monster.Level}");
            Console.WriteLine($"{monster.Hp:F0}/{monster.MaxHp:F0}");
            Console.WriteLine("=======================");

            Console.WriteLine("注：只能选对应按键，否则重新选择");
            Console.WriteLine($"普通攻击（a/A）| 治疗（d/D）[剩余{PlayerStatistics.MaxHealPerBattle - healUsed}次][草药{PlayerStatistics.Herbs}] | 退出（f/F）");

            char battleOption = ConsoleHelper.ReadKeyChar();

            // 输入流已结束，无法继续操作，中止本场战斗
            if (ConsoleHelper.InputEnded)
            {
                return PlayerStatistics.Hp > 0;
            }

            // 玩家回合
            switch (battleOption)
            {
                case 'A':
                case 'a':
                    // 玩家普通攻击
                    monster.Hp -= PlayerStatistics.Attack;
                    Console.WriteLine($"你攻击了 {monster.Name}，造成 {PlayerStatistics.Attack} 点伤害！");
                    break;

                case 'D':
                case 'd':
                    // 玩家治疗：受每场次数上限与草药数量双重约束
                    if (healUsed >= PlayerStatistics.MaxHealPerBattle)
                    {
                        Console.WriteLine($"本场战斗的治疗次数已用完（{PlayerStatistics.MaxHealPerBattle}次），只能用攻击了");
                        break;
                    }

                    if (PlayerStatistics.Hp >= PlayerStatistics.MaxHp)
                    {
                        // 血量已满时不消耗治疗次数
                        Console.WriteLine("你的HP已满，无需治疗");
                        break;
                    }

                    if (PlayerStatistics.Herbs <= 0)
                    {
                        Console.WriteLine("你已经没有草药了，只能硬打了！");
                        break;
                    }

                    if (!Heal.TryHeal(out double recovered))
                    {
                        Console.WriteLine("治疗失败，你没有草药了");
                        break;
                    }

                    healUsed++;
                    Console.WriteLine($"你使用了一株草药，恢复 {recovered:F0} HP（剩余草药 {PlayerStatistics.Herbs}）");
                    break;

                case 'F':
                case 'f':
                    // 玩家撤退：直接结束本场战斗
                    Console.WriteLine("你选择了撤退");
                    return true;

                default:
                    // 非法操作：提示后重新选择，不再直接结束战斗
                    Console.WriteLine("无效操作！请选择 a / d / f");
                    break;
            }

            // 胜利判定
            if (monster.Hp <= 0)
            {
                monster.Hp = 0;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"你击败了 {monster.Name}！");
                Console.ResetColor();

                // 掉落草药
                if (monster.HerbDrop > 0)
                {
                    int gained = PlayerStatistics.AddHerbs(monster.HerbDrop);
                    if (gained > 0)
                    {
                        Console.WriteLine($"你捡起了 {gained} 株草药（当前 {PlayerStatistics.Herbs} 株）");
                    }
                }

                // 给予经验并自动升级
                UpLevel.GainExp(monster.ExpReward);

                // 打怪后恢复精力，可以继续打坐修炼
                Program.ResetMeditation();

                return true;
            }

            // 怪物回合
            if (monsterCanBattle && monster.Hp > 0)
            {
                // 减伤：随玩家等级提升，上限 65%，保证高等级也不会完全免疫
                double mitigation = 0.05 * PlayerStatistics.Level;
                if (mitigation > MaxMitigation) mitigation = MaxMitigation;

                double damage = monster.Attack * (1 - mitigation);

                if (damage < 1)
                {
                    damage = 1;
                }

                // 保留两位小数，避免浮点误差累积出 39.19999999999999 这类数值
                PlayerStatistics.Hp = Math.Round(PlayerStatistics.Hp - damage, 2);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{monster.Name} 反击你，造成 {damage:F0} 点伤害");
                Console.ResetColor();
            }

            // 失败判定
            if (PlayerStatistics.Hp <= 0)
            {
                PlayerStatistics.Hp = 0;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("\n你倒下了……");
                Console.ResetColor();

                // 死亡惩罚：损失当前经验的 10%，只恢复一半血，且必须回城
                PlayerStatistics.Exp -= PlayerStatistics.Exp * PlayerStatistics.DeathExpLossRatio;
                if (PlayerStatistics.Exp < 0)
                {
                    PlayerStatistics.Exp = 0;
                }

                Console.WriteLine($"你损失了 {PlayerStatistics.DeathExpLossRatio * 100:F0}% 的经验值，被抬回了城镇");
                Console.WriteLine($"体力不支，只恢复了 {PlayerStatistics.DeathHpRestoreRatio * 100:F0}% 的血量，请先休息或打坐恢复");

                PlayerStatistics.Hp = Math.Round(PlayerStatistics.MaxHp * PlayerStatistics.DeathHpRestoreRatio, 2);

                return false;
            }

            Console.WriteLine("\n按任意键进入下一回合");
            ConsoleHelper.WaitAnyKey();
        }

        return PlayerStatistics.Hp > 0;
    }
}
