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
            return Console.ReadKey().KeyChar;
        }

        string? line = Console.ReadLine();

        // 输入流已结束（EOF），标记后由调用方决定退出
        if (line == null)
        {
            InputEnded = true;
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
                InputEnded = true;
            }

            return;
        }

        Console.ReadKey();
    }
}
