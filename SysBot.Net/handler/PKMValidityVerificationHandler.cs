using System;
using SysBot.Net.model;
using System.Net.Sockets;
using SysBot.Pokemon;
using PKHeX.Core;
using SysBot.Net.service;
using System.Text;

namespace SysBot.Net.handler
{
    public class PKMValidityVerificationHandler : CommandHandler
    {
        public PKMValidityVerificationHandler()
        {

        }


        public override void execute(ref Server server, ref Socket socket, CommandModel command)
        {

            //if (command.param.ContainsKey("additional"))
            //{
            //    Newtonsoft.Json.Linq.JObject additional = (Newtonsoft.Json.Linq.JObject)command.param["additional"];
            //    if (null != additional)
            //    {
            //        if (additional.ContainsKey("OT"))
            //        {
            //        }
            //    }
            //}

            List<String> responseList = new List<string>();
            CommandModel response = new CommandModel();
            Newtonsoft.Json.Linq.JArray param = (Newtonsoft.Json.Linq.JArray)command.param["data"];
            if (null != param && param.Count() > 0)
            {
                for (int i = 0; i < param.Count(); i++)
                {
                    PK9 pk = new PK9(CommandHandler.decodeBase64($"{param[i]}"));
                    if (pk.Species != 0 && pk.ChecksumValid || !pk.CanBeTraded())
                    {
                        response.code = -1;
                        response.error += "宝可梦：" + pk.Nickname + " 不合法。\n";
                        continue;
                    }

                    responseList.Add($"{param[i]}");
                }
            }


            response.data = responseList;

            server.sendMessage(socket, response);
        }

        public override string getCommand()
        {
            return "PKMValidityVerification";
        }
    }


}

