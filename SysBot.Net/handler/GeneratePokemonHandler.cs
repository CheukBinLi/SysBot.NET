using System;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
//using PKHeX.Core;
using SysBot.Net.model;
using SysBot.Base;
using Newtonsoft.Json;
using PKHeX.Core;
using SysBot.Pokemon;
using System.Text;
using SysBot.Net.service;
using System.Collections.Generic;
using System.Data.SqlTypes;

namespace SysBot.Net.handler
{
    public class GeneratePokemonHandler : CommandHandler
    {
        public GeneratePokemonHandler()
        {

        }

        public override void execute(ref Server server, ref Socket socket, CommandModel command)
        {
            ITrainerInfo sav = AutoLegalityWrapper.GetTrainerInfo<PK9>();
            CommandModel response = new CommandModel();
            List<String> responseList = new List<string>();
            List<String> species = new List<String>();
            Dictionary<String, List<String>> responseData = new Dictionary<string, List<String>>();
            responseData.Add("data", responseList);
            responseData.Add("species", species);

            Newtonsoft.Json.Linq.JArray param = (Newtonsoft.Json.Linq.JArray)command.param["data"];
            String dataType = $"{command.param["dataType"]}";
            Newtonsoft.Json.Linq.JObject additionalResult;
            if (null != param && param.Count() > 0)
            {
                for (int i = 0; i < param.Count(); i++)
                {
                    PK9 pkm;
                    var additional = command.param.ContainsKey("additional") ? (Newtonsoft.Json.Linq.JObject)command.param["additional"] : new Newtonsoft.Json.Linq.JObject();
                    if ("txt".Equals(dataType))
                    {
                        String content = Encoding.UTF8.GetString(CommandHandler.decodeBase64($"{param[i]}"));
                        //String converName = content;
                        if (!content.Contains(":"))
                        {
                            content = ShowdownTranslator<PK9>.Chinese2Showdown(content, ref additional, out additionalResult);
                            additional = additionalResult;
                        }

                        //Console.WriteLine(converName);
                        var set = SysBot.Pokemon.ShowdownUtil.ConvertToShowdown(content);
                        var template = AutoLegalityWrapper.GetTemplate(set);
                        if (template.Species < 1)
                        {
                            response.error += "请输入正确的宝可梦名称。\n";
                            response.code = -1;
                            continue;
                        }

                        if (set.InvalidLines.Count != 0)
                        {
                            response.error += "属性值异常。\n";
                            response.code = -1;
                            continue;
                        }

                        pkm = (PK9)sav.GetLegal(template, out var result);
                    }
                    else
                    {
                        pkm = new PK9(CommandHandler.decodeBase64($"{param[i]}"));
                    }

                    if (command.param.ContainsKey("additional"))
                    {
                        pkm = copyRebuild(
                            pkm,
                            $"{additional["GenerateOT"]}",
                            sav.Game,
                            sav.Language,
                            pkm.Gender,
                            (uint)additional["DisplayTID"],
                            (uint)additional["DisplaySID"],
                            additional
                            );
                    }

                    if (!pkm.CanBeTraded())
                    {
                        response.error += $"官方禁止该《{ShowdownTranslator<PK9>.getPkmName(pkm.Species)}》宝可梦交易。\n";
                        response.code = -1;
                        continue;
                    }
                    var a = new LegalityAnalysis(pkm).Valid;
                    if (!new LegalityAnalysis(pkm).Valid)
                    {
                        response.error += $"没办法创造非法属性(例如：闪耀)/版本专有宝可梦《{ShowdownTranslator<PK9>.getPkmName(pkm.Species)}》。\n";
                        response.code = -1;
                        continue;
                    }
                    // Update PKM to the current save's handler data

                    pkm.RefreshChecksum();
                    pkm.ResetPartyStats();

                    //加密
                    byte[] data = pkm.EncryptedBoxData;
                    responseList.Add(CommandHandler.encodeBase64(pkm.EncryptedBoxData));
                    species.Add($"{pkm.Species}");
                }
            }

            response.data = responseData;
            server.sendMessage(socket, response);

        }

        public override string getCommand()
        {
            return "GeneratePokemon";
        }


        private PK9 copyRebuild(PK9 toSend, String otName, int gameVersion, int language, int gender, uint displayTID, uint displaySID, Newtonsoft.Json.Linq.JObject additional)
        {
            PK9 cln = (PK9)toSend.Clone();
            cln.OT_Gender = gender;
            //cln.TrainerTID7 = (uint)Math.Abs(displayTID);
            //cln.TrainerSID7 = (uint)Math.Abs(displaySID);

            cln.TrainerSID7 = displaySID;
            cln.TrainerTID7 = displayTID;
            //Console.WriteLine(cln.TrainerTID7);
            cln.Language = language;
            //cln.OT_Name = otName;
            cln.Version = gameVersion;
            cln.ClearNickname();

            if (additional.ContainsKey("全技能"))
            {
                //全技能
                var permit = cln.Permit;
                for (int i = 0; i < permit.RecordCountUsed; i++)
                {
                    if (permit.IsRecordPermitted(i))
                        cln.SetMoveRecordFlag(i);
                }
            }
            //全奖章
            if (additional.ContainsKey("大个子"))
            {
                cln.RibbonMarkJumbo = true;
                cln.Scale = 255;
            }
            else if (additional.ContainsKey("大个子"))
            {
                cln.RibbonMarkMini = true;
                cln.Scale = 0;
            }
            if (additional.ContainsKey("全技能"))
            {
                //cln.RibbonMarkItemfinder = true;
                cln.RibbonMarkPartner = true;
                cln.RibbonMarkGourmand = true;
                cln.RibbonBestFriends = true;
                //cln.RibbonMarkAlpha = true;
                //cln.RibbonMarkMightiest = true;
                //cln.RibbonMarkTitan = true;
                cln.RibbonEffort = true;
                cln.RibbonChampionPaldea = true;
            }

            if (toSend.IsShiny)
                cln.SetShiny();
            return cln;
        }

    }
}

