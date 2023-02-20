using System;
using System.Net.Sockets;
using SysBot.Net.model;

namespace SysBot.Net
{
    public abstract class CommandHandler
    {

        public abstract String getCommand();

        public abstract void execute(ref Server server, ref Socket socket, CommandModel command);

        public static byte[] decodeBase64(String data)
        {
            return System.Convert.FromBase64String(data);
        }

        public static String encodeBase64(byte[] data)
        {
            return System.Convert.ToBase64String(data);
        }

        public static String encodeBase64(String data)
        {
            return System.Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(data));
        }
    }
}

