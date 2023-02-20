using System;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
//using PKHeX.Core;
using SysBot.Net.model;
using SysBot.Base;
using Newtonsoft.Json;

namespace SysBot.Net.handler
{
    public class DecodeFakeTrainerSAVHandler : CommandHandler
    {
        public DecodeFakeTrainerSAVHandler()
        {
        }

        public override void execute(ref Server server, ref Socket socket, CommandModel command)
        {

            var sav = new PKHeX.Core.SAV9SV();
            var info = sav.MyStatus;
            var read = Decoder.ConvertHexByteStringToBytes(CommandHandler.decodeBase64((String)command.param["data"]));
            read.CopyTo(info.Data, 0);

            Dictionary<String, String> result = new Dictionary<string, string>();
            result.Add("Language", ((PKHeX.Core.LanguageID)sav.Language).ToString());
            result.Add("GenerateOT", sav.OT);
            result.Add("DisplaySID", $"{sav.DisplaySID}");
            result.Add("DisplayTID", $"{sav.DisplayTID}");
            result.Add("FullDisplayTID", $"{sav.OT}-{sav.DisplayTID:000000}");
            CommandModel response = new CommandModel();
            response.data = result;

            server.sendMessage(socket, response);

        }

        public override string getCommand()
        {
            return "DecodeFakeTrainerSAV";
        }
    }
}

