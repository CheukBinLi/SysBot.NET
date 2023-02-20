using System;
using SysBot.Net.model;
using System.Net.Sockets;

namespace SysBot.Net
{
    public class CommandManager
    {
        public CommandManager()
        {

        }
        public Dictionary<String, CommandHandler> commandHandlerPool { get; set; } = new Dictionary<string, CommandHandler>();

        public CommandManager appendHandle(CommandHandler commandHandler)
        {
            commandHandlerPool.Add(commandHandler.getCommand(), commandHandler);
            return this;
        }

        public void execute(ref Server server, ref Socket socket, CommandModel command)
        {
            CommandHandler handler = commandHandlerPool[command.command];
            if (null == handler)
            {
                return;
            }
            handler.execute(ref server,ref socket, command);
        }

    }
}


