namespace Console_RPG;

/*
    MonsterStatistics 用于保存怪物的基础属性数据。
    该类本身不包含战斗逻辑，只作为数据容器使用。
    在 MonsterFactory 创建怪物后，会被 Battle 模块读取和修改。
*/
public class MonsterStatistics
{
    public string Name { get; set; } = "";   // 怪物名字
    public int Level { get; set; }           // 怪物等级
    public double Hp { get; set; }           // 当前血量
    public double MaxHp { get; set; }        // 最大血量
    public double Attack { get; set; }       // 攻击力
    public double ExpReward { get; set; }    // 击败后给予玩家的经验值
    public int HerbDrop { get; set; }        // 击败后掉落的草药数量
}
