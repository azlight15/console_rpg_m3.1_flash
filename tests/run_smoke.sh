#!/usr/bin/env bash
#
# 冒烟测试脚本
#
# 把 tests/smoke/ 下的输入文件当作标准输入喂给游戏，
# 检查游戏能否在超时内正常结束（不崩溃、不死循环）。
#
# 用法：
#   bash tests/run_smoke.sh
#
# 可选环境变量：
#   TARGET_FRAMEWORK  目标框架，默认 net10.0（例如 net9.0）
#   BUILD_ONLY=1      只编译不跑用例
#
set -u

cd "$(dirname "$0")/.." || exit 1

FRAMEWORK="${TARGET_FRAMEWORK:-net10.0}"
EXE="bin/Debug/${FRAMEWORK}/Console_RPG"
TIMEOUT_SECONDS=30

echo "==> 编译 (${FRAMEWORK})"
if ! dotnet build -v q --nologo; then
    echo "编译失败"
    exit 1
fi

if [ ! -f "$EXE" ]; then
    echo "找不到可执行文件：$EXE"
    exit 1
fi

if [ "${BUILD_ONLY:-0}" = "1" ]; then
    echo "仅编译模式，结束"
    exit 0
fi

failures=0

run_case() {
    local name="$1"
    local input="$2"
    local extra_setup="${3:-}"

    echo "==> 用例：${name}"

    if [ -n "$extra_setup" ]; then
        eval "$extra_setup"
    fi

    timeout "$TIMEOUT_SECONDS" "$EXE" < "$input" > /tmp/console_rpg_smoke.log 2>&1
    local code=$?

    if [ $code -eq 124 ]; then
        echo "    FAIL：超时（疑似死循环）"
        failures=$((failures + 1))
    elif [ $code -ne 0 ]; then
        echo "    FAIL：退出码 ${code}"
        tail -5 /tmp/console_rpg_smoke.log
        failures=$((failures + 1))
    else
        echo "    OK"
    fi
}

# 1. 输入流立刻结束（EOF）：必须干净退出而不是死循环
printf '' > /tmp/console_rpg_eof.txt
run_case "空输入（EOF）" /tmp/console_rpg_eof.txt

# 2. 名字为空 / 空格：应该重新提示而不是崩溃
printf '\n   \n阿狸\n' > /tmp/console_rpg_name.txt
run_case "名字为空" /tmp/console_rpg_name.txt

# 3. 菜单各项功能巡检
run_case "菜单巡检" tests/smoke/menu.txt

# 4. 战斗中治疗（消耗草药）
run_case "战斗治疗" tests/smoke/heal.txt

# 5. 连续刷怪（会遇到精英怪）
run_case "连续刷怪" tests/smoke/elite.txt

# 6. 死亡：先放一份残血存档，读档后被打死
run_case "死亡流程" tests/smoke/death.txt "mkdir -p 'bin/Debug/${FRAMEWORK}/saves' && cp tests/smoke/lowhp_save.json 'bin/Debug/${FRAMEWORK}/saves/save.json'"

# 7. 存读档往返
run_case "存读档" tests/smoke/saveload.txt

echo
if [ $failures -eq 0 ]; then
    echo "全部用例通过 ✅"
    exit 0
fi

echo "${failures} 个用例失败 ❌"
exit 1
