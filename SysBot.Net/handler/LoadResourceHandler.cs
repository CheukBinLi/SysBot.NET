using System;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
//using PKHeX.Core;
using SysBot.Net.model;
using SysBot.Base;
using Newtonsoft.Json;
using System.Reflection;
using PKHeX.Core;

namespace SysBot.Net.handler
{
    public class LoadResourceHandler : CommandHandler
    {
        public static GameStrings GameStringsZh = GameInfo.GetStrings("zh");

        public LoadResourceHandler()
        {
        }

        public override void execute(ref Server server, ref Socket socket, CommandModel command)
        {
            CommandModel response = new CommandModel();
            response.data = GameStringsZh.specieslist;
            server.sendMessage(socket, response);

        }

        public override string getCommand()
        {
            return "LoadResource";
        }
    }
}

