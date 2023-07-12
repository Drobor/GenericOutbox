using System.Drawing;

namespace Test;

public interface ITestInterface
{
    public Task<int> WithReturnType(int arg1, string arg2);
    public Task WithoutReturnType(Point arg1, int[] arg2);
    public void DoSmth(int arg);
    public Task DoSmthAsync(int arg);
}

public class TestInterface : ITestInterface
{
    public async Task<int> WithReturnType(int arg1, string arg2)
    {
        var result = arg1 + arg2.Length;
        Console.WriteLine($"WithReturnType {result}");
        return result;
    }

    public async Task WithoutReturnType(Point arg1, int[] arg2)
    {
        Console.WriteLine("WithoutReturnType");
        return;
    }

    public void DoSmth(int arg)
    {
        throw new NotImplementedException();
    }

    public Task DoSmthAsync(int arg)
    {
        throw new NotImplementedException();
    }
}