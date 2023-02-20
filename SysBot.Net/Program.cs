using SysBot.Net;

namespace SysBot.plugins
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine("服务初始化");
            Server server = new Server();
            server.restart();
            Console.WriteLine("服务启动");
        }
    }
}