using System;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using SysBot.Net.model;
using SysBot.Base;
using Newtonsoft.Json;
using SysBot.Pokemon;
using System.Reflection;
using PKHeX.Core;
using System.Buffers.Binary;

namespace SysBot.Net.handler
{
    public class DecodeTradePartnerMyStatusHandler : CommandHandler
    {
        public DecodeTradePartnerMyStatusHandler()
        {
        }

        public override void execute(ref Server server, ref Socket socket, CommandModel command)
        {

            List<Dictionary<String, Object>> list = new List<Dictionary<string, Object>>();

            Newtonsoft.Json.Linq.JArray param = (Newtonsoft.Json.Linq.JArray)command.param["data"];

            if (null != param && param.Count() > 0)
            {
                for (int i = 0; i < param.Count(); i++)
                {
                    var info = new TradeMyStatus();
                    var read = Decoder.ConvertHexByteStringToBytes(CommandHandler.decodeBase64($"{param[i]}"));
                    read.CopyTo(info.Data, 0);

                    Dictionary<String, Object> result = new Dictionary<string, Object>();
                    result.Add("DisplaySID", info.DisplaySID);
                    result.Add("DisplayTID", info.DisplayTID);
                    result.Add("Game", $"{info.Game}");
                    result.Add("Gender", $"{info.Gender}");
                    result.Add("Language", $"{info.Language}");
                    result.Add("OT", $"{info.OT}");
                    result.Add("FullDisplayTID", $"{info.OT}-{info.DisplayTID:000000}");
                    list.Add(result);
                }
            }
            CommandModel response = new CommandModel();

            response.data = list;

            server.sendMessage(socket, response);

        }

        public override string getCommand()
        {
            return "DecodeTradePartnerMyStatus";
        }
    }
}

