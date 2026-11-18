using System;

class TestSyntax
{
    static void Main()
    {
        // 测试第251行的语法
        float x = 10.5f;
        float y = 20.7f;
        
        // 修复后的语法
        int seed = (int)(x * 100) ^ (int)(y * 100);
        
        Console.WriteLine("Seed: " + seed);
        Console.WriteLine("Syntax is correct!");
    }
}