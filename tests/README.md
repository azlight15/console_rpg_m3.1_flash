# 测试

## 冒烟测试

```bash
bash tests/run_smoke.sh
```

脚本会先编译，再用 `tests/smoke/` 下的输入文件依次把游戏跑一遍，检查：

- 不会崩溃（退出码非 0 判失败）
- 不会死循环（超过 30 秒判失败）

需要 .NET 10 SDK。如果本机是 .NET 9，用：

```bash
TARGET_FRAMEWORK=net9.0 bash tests/run_smoke.sh
```

只想编译不跑用例：

```bash
BUILD_ONLY=1 bash tests/run_smoke.sh
```

## 用例说明

| 文件 | 覆盖的场景 |
| --- | --- |
| `menu.txt` | 主菜单各项：状态、打坐、治疗、存档、读档 |
| `heal.txt` | 战斗中治疗，确认草药被消耗、次数被限制 |
| `elite.txt` | 连续刷怪，会遇到精英怪 |
| `death.txt` | 读入 `lowhp_save.json` 后被打死，检查死亡惩罚与回城 |
| `saveload.txt` | 存档 → 读档往返 |
| `lowhp_save.json` | 残血存档夹具，供死亡用例使用 |
