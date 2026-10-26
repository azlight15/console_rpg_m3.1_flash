using System;
using System.IO;

/*
    ConsoleHelper 封装控制台输入读取。
    在正常终端下读取单个按键；
    当输入被重定向（例如脚本化自动测试）时没有按键可读，改为读取一行文本并取其首字符。
*/
public static class ConsoleHelper
{
    /*
        输入流是否已经结束。
        当标准输入被重定向且已读到文件末尾时为 true，
        此时继续读取会立刻返回，游戏会陷入死循环，因此各模块据此主动退出。
    */
    public static bool InputEnded { get; private set; }

    /*
        标记标准输入已结束。
        供各模块在发现 ReadLine 返回 null 时主动调用，
        避免"读到 null 之后继续循环"造成的死循环。
    */
    public static void MarkInputEnded()
    {
        InputEnded = true;
    }

    /*
        重置输入结束标记。
        读档会替换掉整个玩家状态，等同于一次新的开始，
        此时若之前只是因为重定向输入读到末尾，应该允许重新尝试读取。
    */
    public static void ResetInputEnded()
    {
        InputEnded = false;
    }

    /*
        清屏。
        在没有真实控制台（例如输出被重定向）时 Console.Clear 会抛出异常，这里做兜底处理。
    */
    public static void Clear()
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            // 没有可清空的控制台，忽略即可
        }
    }

    /*
        读取一个字符（用于选项选择）。
    */
    public static char ReadKeyChar()
    {
        if (!Console.IsInputRedirected)
        {
            try
            {
                return Console.ReadKey().KeyChar;
            }
            catch (InvalidOperationException)
            {
                // 没有可用的控制台输入，退化为读取一行文本
                MarkInputEnded();
                return '\0';
            }
        }

        string? line = Console.ReadLine();

        // 输入流已结束（EOF），标记后由调用方决定退出
        if (line == null)
        {
            MarkInputEnded();
            return '\0';
        }

        if (line.Length == 0)
        {
            return '\0';
        }

        return line[0];
    }

    /*
        等待任意按键（用于"按任意键继续"）。
    */
    public static void WaitAnyKey()
    {
        if (Console.IsInputRedirected)
        {
            if (Console.ReadLine() == null)
            {
                MarkInputEnded();
            }

            return;
        }

        try
        {
            Console.ReadKey();
        }
        catch (InvalidOperationException)
        {
            MarkInputEnded();
        }
    }
}
