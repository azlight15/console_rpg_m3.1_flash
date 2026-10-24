namespace Console_RPG;

/*
    MonsterStatistics 用于保存怪物的基础属性数据。
    该类本身不包含战斗逻辑，只作为数据容器使用。
    在 MonsterFactory 创建怪物后，会被 Battle 模块读取和修改。
*/
public class MonsterStatistics
{
    public string Name = "";   // 怪物名字
    public int Level;         // 怪物等级
    public double Hp;         // 当前血量
    public double MaxHp;      // 最大血量
    public double Attack;     // 攻击力
    public double ExpReward;  // 击败后给予玩家的经验值
}