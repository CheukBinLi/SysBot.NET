﻿using System;
using PKHeX.Core;
using System.Text.RegularExpressions;

namespace SysBot.Net.service
{
    public class ShowdownTranslator<T> where T : PKM
    {
        public static GameStrings GameStringsZh = GameInfo.GetStrings("zh");
        public static GameStrings GameStringsEn = GameInfo.GetStrings("en");
        public static string Chinese2Showdown(string zh, ref Newtonsoft.Json.Linq.JObject additional, out Newtonsoft.Json.Linq.JObject additionalResult)
        {
            string result = "";
            additionalResult = new Newtonsoft.Json.Linq.JObject(additional);

            // 添加宝可梦
            int candidateSpecieNo = 0;
            int candidateSpecieStringLength = 0;
            for (int i = 1; i < GameStringsZh.Species.Count; i++)
            {
                if (zh.Contains(GameStringsZh.Species[i]) && GameStringsZh.Species[i].Length > candidateSpecieStringLength)
                {
                    candidateSpecieNo = i;
                    candidateSpecieStringLength = GameStringsZh.Species[i].Length;
                }
            }

            if (candidateSpecieNo <= 0)
            {
                return result;
            }

            // 处理蛋宝可梦
            if (zh.Contains("的蛋"))
            {
                result += "Egg ";
                zh = zh.Replace("的蛋", "");

                // Showdown 文本差异，29-尼多兰F，32-尼多朗M，876-爱管侍，
                if (candidateSpecieNo is (ushort)Species.NidoranF) result += "(Nidoran-F)";
                else if (candidateSpecieNo is (ushort)Species.NidoranM) result += "(Nidoran-M)";
                else if ((candidateSpecieNo is (ushort)Species.Indeedee) && zh.Contains('母')) result += $"({GameStringsEn.Species[candidateSpecieNo]}-F)";
                // 识别地区形态
                else if (zh.Contains("形态"))
                {
                    foreach (var s in formDict)
                    {
                        var searchKey = s.Key.EndsWith("形态") ? s.Key : s.Key + "形态";
                        if (!zh.Contains(searchKey)) continue;
                        result += $"({GameStringsEn.Species[candidateSpecieNo]}-{s.Value})";
                        zh = zh.Replace(searchKey, "");
                        break;
                    }
                }
                else result += $"({GameStringsEn.Species[candidateSpecieNo]})";
            }
            // 处理非蛋宝可梦
            else
            {
                // Showdown 文本差异，29-尼多兰F，32-尼多朗M，678-超能妙喵，876-爱管侍，902-幽尾玄鱼, 916-飘香豚
                if (candidateSpecieNo is (ushort)Species.NidoranF) result = "Nidoran-F";
                else if (candidateSpecieNo is (ushort)Species.NidoranM) result = "Nidoran-M";
                else if ((candidateSpecieNo is (ushort)Species.Meowstic or (ushort)Species.Indeedee or (ushort)Species.Basculegion or (ushort)Species.Oinkologne) && zh.Contains("母"))
                    result += $"{GameStringsEn.Species[candidateSpecieNo]}-F";
                // 识别地区形态
                else if (zh.Contains("形态"))
                {
                    foreach (var s in formDict)
                    {
                        var searchKey = s.Key.EndsWith("形态") ? s.Key : s.Key + "形态";
                        if (!zh.Contains(searchKey)) continue;
                        result = $"{GameStringsEn.Species[candidateSpecieNo]}-{s.Value}";
                        zh = zh.Replace(searchKey, "");
                        break;
                    }
                }
                else result = $"{GameStringsEn.Species[candidateSpecieNo]}";
                zh = zh.Replace(GameStringsZh.Species[candidateSpecieNo], "");
            }
            //if (candidateSpecieNo == (int)Species.NidoranF) result = "Nidoran-F";
            //else if (candidateSpecieNo == (int)Species.NidoranM) result = "Nidoran-M";
            //else result += GameStringsEn.Species[candidateSpecieNo];

            //zh = zh.Replace(GameStringsZh.Species[candidateSpecieNo], "");

            // 特殊性别差异
            // 29-尼多兰F，32-尼多朗M，678-超能妙喵F，876-爱管侍F，902-幽尾玄鱼F, 916-飘香豚
            if (((Species)candidateSpecieNo is Species.Meowstic or Species.Indeedee or Species.Basculegion or Species.Oinkologne)
                && zh.Contains("母")) result += "-F";


            //// 识别地区形态
            //foreach (var s in formDict)
            //{
            //    var searchKey = s.Key.EndsWith("形态") ? s.Key : s.Key + "形态";
            //    if (!zh.Contains(searchKey)) continue;
            //    result += $"-{s.Value}";
            //    zh = zh.Replace(searchKey, "");
            //    break;
            //}

            // 添加性别
            if (zh.Contains("公"))
            {
                result += " (M)";
                zh = zh.Replace("公", "");
            }
            else if (zh.Contains("母"))
            {
                result += " (F)";
                zh = zh.Replace("母", "");
            }

            // 添加持有物
            if (zh.Contains("持有"))
            {
                for (int i = 1; i < GameStringsZh.Item.Count; i++)
                {
                    if (GameStringsZh.Item[i].Length == 0) continue;
                    if (!zh.Contains("持有" + GameStringsZh.Item[i])) continue;
                    result += $" @ {GameStringsEn.Item[i]}";
                    zh = zh.Replace("持有" + GameStringsZh.Item[i], "");
                    break;
                }
            }
            else if (zh.Contains("携带"))
            {
                for (int i = 1; i < GameStringsZh.Item.Count; i++)
                {
                    if (GameStringsZh.Item[i].Length == 0) continue;
                    if (!zh.Contains("携带" + GameStringsZh.Item[i])) continue;
                    result += $" @ {GameStringsEn.Item[i]}";
                    zh = zh.Replace("携带" + GameStringsZh.Item[i], "");
                    break;
                }
            }

            // 添加等级
            if (Regex.IsMatch(zh, "\\d{1,3}级"))
            {
                string level = Regex.Match(zh, "(\\d{1,3})级").Groups?[1]?.Value ?? "100";
                result += $"\nLevel: {level}";
                zh = Regex.Replace(zh, "\\d{1,3}级", "");
            }

            // 添加超极巨化
            if (typeof(T) == typeof(PK8) && zh.Contains("超极巨"))
            {
                result += "\nGigantamax: Yes";
                zh = zh.Replace("超极巨", "");
            }

            // 添加异色
            if (zh.Contains("异色"))
            {
                result += "\nShiny: Yes";
                zh = zh.Replace("异色", "");
            }
            else if (zh.Contains("闪光"))
            {
                result += "\nShiny: Yes";
                zh = zh.Replace("闪光", "");
            }
            else if (zh.Contains("星闪"))
            {
                result += "\nShiny: Star";
                zh = zh.Replace("星闪", "");
            }
            else if (zh.Contains("方闪"))
            {
                result += "\nShiny: Square";
                zh = zh.Replace("方闪", "");
            }

            // 添加头目
            if (typeof(T) == typeof(PA8) && zh.Contains("头目"))
            {
                result += "\nAlpha: Yes";
                zh = zh.Replace("头目", "");
            }

            // 添加球种
            for (int i = 1; i < GameStringsZh.balllist.Length; i++)
            {
                if (GameStringsZh.balllist[i].Length == 0) continue;
                if (!zh.Contains(GameStringsZh.balllist[i])) continue;
                var ballStr = GameStringsEn.balllist[i];
                if (typeof(T) == typeof(PA8) && ballStr is "Poké Ball" or "Great Ball" or "Ultra Ball") ballStr = "LA" + ballStr;
                result += $"\nBall: {ballStr}";
                zh = zh.Replace(GameStringsZh.balllist[i], "");
                break;
            }

            // 添加特性
            for (int i = 1; i < GameStringsZh.Ability.Count; i++)
            {
                if (GameStringsZh.Ability[i].Length == 0) continue;
                if (!zh.Contains(GameStringsZh.Ability[i] + "特性")) continue;
                result += $"\nAbility: {GameStringsEn.Ability[i]}";
                zh = zh.Replace(GameStringsZh.Ability[i] + "特性", "");
                break;
            }

            // 添加性格
            for (int i = 0; i < GameStringsZh.Natures.Count; i++)
            {
                if (GameStringsZh.Natures[i].Length == 0) continue;
                if (!zh.Contains(GameStringsZh.Natures[i])) continue;
                result += $"\n{GameStringsEn.Natures[i]} Nature";
                zh = zh.Replace(GameStringsZh.Natures[i], "");
                break;
            }

            // 添加个体值
            if (zh.ToUpper().Contains("6V"))//默认
            {
                result += "\nIVs: 31 HP / 31 Atk / 31 Def / 31 SpA / 31 SpD / 31 Spe";
                zh = Regex.Replace(zh, "6V|6v", "");
            }
            else if (zh.ToUpper().Contains("5V0A"))
            {
                result += "\nIVs: 31 HP / 0 Atk / 31 Def / 31 SpA / 31 SpD / 31 Spe";
                zh = Regex.Replace(zh, "5V0A|5v0a", "");
            }
            else if (zh.ToUpper().Contains("5V0攻"))
            {
                result += "\nIVs: 31 HP / 0 Atk / 31 Def / 31 SpA / 31 SpD / 31 Spe";
                zh = Regex.Replace(zh, "5V0攻|5v0攻", "");
            }
            else if (zh.ToUpper().Contains("5V0S"))
            {
                result += "\nIVs: 31 HP / 31 Atk / 31 Def / 31 SpA / 31 SpD / 0 Spe";
                zh = Regex.Replace(zh, "5V0S|5v0s", "");
            }
            else if (zh.ToUpper().Contains("5V0速"))
            {
                result += "\nIVs: 31 HP / 31 Atk / 31 Def / 31 SpA / 31 SpD / 0 Spe";
                zh = Regex.Replace(zh, "5V0速|5v0速", "");
            }
            else if (zh.ToUpper().Contains("4V0A0S"))
            {
                result += "\nIVs: 31 HP / 0 Atk / 31 Def / 31 SpA / 31 SpD / 0 Spe";
                zh = Regex.Replace(zh, "4V0A0S|4v0a0s", "");
            }
            else if (zh.ToUpper().Contains("4V0攻0速"))
            {
                result += "\nIVs: 31 HP / 0 Atk / 31 Def / 31 SpA / 31 SpD / 0 Spe";
                zh = Regex.Replace(zh, "4V0攻0速|4v0攻0速", "");
            }

            // 添加努力值
            if (zh.Contains("努力值"))
            {
                result += "\nEVs: ";
                zh = zh.Replace("努力值", "");
                if (Regex.IsMatch(zh, "\\d{1,3}生命"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})生命").Groups?[1]?.Value ?? "";
                    result += $"{value} HP / ";
                    zh = Regex.Replace(zh, "\\d{1,3}生命", "");
                }
                else if (Regex.IsMatch(zh, "\\d{1,3}Hp"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})Hp").Groups?[1]?.Value ?? "";
                    result += $"{value} HP / ";
                    zh = Regex.Replace(zh, "\\d{1,3}Hp", "");
                }

                if (Regex.IsMatch(zh, "\\d{1,3}攻击"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})攻击").Groups?[1]?.Value ?? "";
                    result += $"{value} Atk / ";
                    zh = Regex.Replace(zh, "\\d{1,3}攻击", "");
                }
                else if (Regex.IsMatch(zh, "\\d{1,3}Atk"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})Atk").Groups?[1]?.Value ?? "";
                    result += $"{value} Atk / ";
                    zh = Regex.Replace(zh, "\\d{1,3}Atk", "");
                }

                if (Regex.IsMatch(zh, "\\d{1,3}防御"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})防御").Groups?[1]?.Value ?? "";
                    result += $"{value} Def / ";
                    zh = Regex.Replace(zh, "\\d{1,3}防御", "");
                }
                else if (Regex.IsMatch(zh, "\\d{1,3}Def"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})Def").Groups?[1]?.Value ?? "";
                    result += $"{value} Def / ";
                    zh = Regex.Replace(zh, "\\d{1,3}Def", "");
                }

                if (Regex.IsMatch(zh, "\\d{1,3}特攻"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})特攻").Groups?[1]?.Value ?? "";
                    result += $"{value} SpA / ";
                    zh = Regex.Replace(zh, "\\d{1,3}特攻", "");
                }
                else if (Regex.IsMatch(zh, "\\d{1,3}SpA"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})SpA").Groups?[1]?.Value ?? "";
                    result += $"{value} SpA / ";
                    zh = Regex.Replace(zh, "\\d{1,3}SpA", "");
                }

                if (Regex.IsMatch(zh, "\\d{1,3}特防"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})特防").Groups?[1]?.Value ?? "";
                    result += $"{value} SpD / ";
                    zh = Regex.Replace(zh, "\\d{1,3}特防", "");
                }
                else if (Regex.IsMatch(zh, "\\d{1,3}SpD"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})SpD").Groups?[1]?.Value ?? "";
                    result += $"{value} SpD / ";
                    zh = Regex.Replace(zh, "\\d{1,3}SpD", "");
                }
                if (Regex.IsMatch(zh, "\\d{1,3}速度"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})速度").Groups?[1]?.Value ?? "";
                    result += $"{value} Spe";
                    zh = Regex.Replace(zh, "\\d{1,3}速度", "");
                }
                else if (Regex.IsMatch(zh, "\\d{1,3}Spe"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})Spe").Groups?[1]?.Value ?? "";
                    result += $"{value} Spe";
                    zh = Regex.Replace(zh, "\\d{1,3}Spe", "");
                }
                if (result.EndsWith("/ "))
                {
                    result = result.Substring(0, result.Length - 2);
                }
            }

            // 添加太晶属性
            if (typeof(T) == typeof(PK9))
            {
                for (int i = 0; i < GameStringsZh.Types.Count; i++)
                {
                    if (GameStringsZh.Types[i].Length == 0) continue;
                    if (!zh.Contains("太晶" + GameStringsZh.Types[i])) continue;
                    result += $"\nTera Type: {GameStringsEn.Types[i]}";
                    zh = zh.Replace("太晶" + GameStringsZh.Types[i], "");
                    break;
                }
            }

            // 补充后天获得的全奖章 注意开启Legality=>AllowBatchCommands
            if (typeof(T) == typeof(PK9) && zh.Contains("全奖章"))
            {
                result += "\n.Ribbons=$suggestAll\n.RibbonMarkPartner=True\n.RibbonMarkGourmand=True";
                zh = zh.Replace("全奖章", "");
                additionalResult.Add("全奖章", 0);
            }
            // 体型大小并添加证章
            if (typeof(T) == typeof(PK9) && zh.Contains("大个子"))
            {
                result += $"\n.Scale=255\n.RibbonMarkJumbo=True";
                zh = zh.Replace("大个子", "");
                additionalResult.Add("大个子", 0);
            }
            else if (typeof(T) == typeof(PK9) && zh.Contains("小不点"))
            {
                result += $"\n.Scale=0\n.RibbonMarkMini=True";
                zh = zh.Replace("小不点", "");
                additionalResult.Add("小不点", 0);
            }

            //添加全回忆技能
            if (typeof(T) == typeof(PK9) || typeof(T) == typeof(PK8))
            {
                if (zh.Contains("全技能"))
                {
                    result += "\n.RelearnMoves=$suggestAll";
                    zh = zh.Replace("全技能", "");
                    additionalResult.Add("全技能", 0);

                }
                else if (zh.Contains("全招式"))
                {
                    result += "\n.RelearnMoves=$suggestAll";
                    zh = zh.Replace("全招式", "");
                    additionalResult.Add("全技能", 0);
                }
            }
            else if (typeof(T) == typeof(PA8))
            {
                if (zh.Contains("全技能"))
                {
                    result += "\n.MoveMastery=$suggestAll";
                    zh = zh.Replace("全技能", "");
                }
                else if (zh.Contains("全招式"))
                {
                    result += "\n.MoveMastery=$suggestAll";
                    zh = zh.Replace("全招式", "");
                }
            }

            if (zh.Contains("异国")) { result += "\nLanguage: Italian"; zh = zh.Replace("异国", ""); }
            else if (zh.Contains("日语")) { result += "\nLanguage: Japanese"; zh = zh.Replace("日语", ""); }
            else if (zh.Contains("英语")) { result += "\nLanguage: English"; zh = zh.Replace("英语", ""); }
            else if (zh.Contains("法语")) { result += "\nLanguage: French"; zh = zh.Replace("法语", ""); }
            else if (zh.Contains("意大利语")) { result += "\nLanguage: Italian"; zh = zh.Replace("意大利语", ""); }
            else if (zh.Contains("德语")) { result += "\nLanguage: German"; zh = zh.Replace("德语", ""); }
            else if (zh.Contains("西班牙语")) { result += "\nLanguage: Spanish"; zh = zh.Replace("西班牙语", ""); }
            else if (zh.Contains("韩语")) { result += "\nLanguage: Korean"; zh = zh.Replace("韩语", ""); }
            else if (zh.Contains("简体中文")) { result += "\nLanguage: ChineseS"; zh = zh.Replace("简体中文", ""); }
            else if (zh.Contains("繁体中文")) { result += "\nLanguage: ChineseT"; zh = zh.Replace("繁体中文", ""); }
            else { result += "\nLanguage: ChineseS"; zh = zh.Replace("简体中文", ""); }

            //补充后天获得的全奖章
            if (typeof(T) == typeof(PK9) && zh.Contains("全奖章"))
            {
                result += "\n.Ribbons=$suggestAll\n.RibbonMarkPartner=True\n.RibbonMarkGourmand=True";
                zh = zh.Replace("全奖章", "");
            }
            //为野生宝可梦添加证章
            if (typeof(T) == typeof(PK9))
            {
                if (zh.Contains("最强之证")) { result += "\n.RibbonMarkMightiest=True"; }
                else if (zh.Contains("未知之证")) { result += "\n.RibbonMarkRare=True"; }
                else if (zh.Contains("命运之证")) { result += "\n.RibbonMarkDestiny=True"; }
                else if (zh.Contains("暴雪之证")) { result += "\n.RibbonMarkBlizzard=True"; }
                else if (zh.Contains("阴云之证")) { result += "\n.RibbonMarkCloudy=True"; }
                else if (zh.Contains("正午之证")) { result += "\n.RibbonMarkLunchtime=True"; }
                else if (zh.Contains("浓雾之证")) { result += "\n.RibbonMarkMisty=True"; }
                else if (zh.Contains("降雨之证")) { result += "\n.RibbonMarkRainy=True"; }
                else if (zh.Contains("沙尘之证")) { result += "\n.RibbonMarkSandstorm=True"; }
                else if (zh.Contains("午夜之证")) { result += "\n.RibbonMarkSleepyTime=True"; }
                else if (zh.Contains("降雪之证")) { result += "\n.RibbonMarkSnowy=True"; }
                else if (zh.Contains("落雷之证")) { result += "\n.RibbonMarkStormy=True"; }
                else if (zh.Contains("干燥之证")) { result += "\n.RibbonMarkDry=True"; }
                else if (zh.Contains("黄昏之证")) { result += "\n.RibbonMarkDusk=True"; }
                else if (zh.Contains("拂晓之证")) { result += "\n.RibbonMarkDawn=True"; }
                else if (zh.Contains("上钩之证")) { result += "\n.RibbonMarkFishing=True"; }
                else if (zh.Contains("咖喱之证")) { result += "\n.RibbonMarkCurry=True"; }
                else if (zh.Contains("无虑之证")) { result += "\n.RibbonMarkAbsentMinded=True"; }
                else if (zh.Contains("愤怒之证")) { result += "\n.RibbonMarkAngry=True"; }
                else if (zh.Contains("冷静之证")) { result += "\n.RibbonMarkCalmness=True"; }
                else if (zh.Contains("领袖之证")) { result += "\n.RibbonMarkCharismatic=True"; }
                else if (zh.Contains("狡猾之证")) { result += "\n.RibbonMarkCrafty=True"; }
                else if (zh.Contains("期待之证")) { result += "\n.RibbonMarkExcited=True"; }
                else if (zh.Contains("本能之证")) { result += "\n.RibbonMarkFerocious=True"; }
                else if (zh.Contains("动摇之证")) { result += "\n.RibbonMarkFlustered=True"; }
                else if (zh.Contains("木讷之证")) { result += "\n.RibbonMarkHumble=True"; }
                else if (zh.Contains("理性之证")) { result += "\n.RibbonMarkIntellectual=True"; }
                else if (zh.Contains("热情之证")) { result += "\n.RibbonMarkIntense=True"; }
                else if (zh.Contains("捡拾之证")) { result += "\n.RibbonMarkItemfinder=True"; }
                else if (zh.Contains("紧张之证")) { result += "\n.RibbonMarkJittery=True"; }
                else if (zh.Contains("幸福之证")) { result += "\n.RibbonMarkJoyful=True"; }
                else if (zh.Contains("优雅之证")) { result += "\n.RibbonMarkKindly=True"; }
                else if (zh.Contains("激动之证")) { result += "\n.RibbonMarkPeeved=True"; }
                else if (zh.Contains("自信之证")) { result += "\n.RibbonMarkPrideful=True"; }
                else if (zh.Contains("昂扬之证")) { result += "\n.RibbonMarkPumpedUp=True"; }
                else if (zh.Contains("未知之证")) { result += "\n.RibbonMarkRare=True"; }
                else if (zh.Contains("淘气之证")) { result += "\n.RibbonMarkRowdy=True"; }
                else if (zh.Contains("凶悍之证")) { result += "\n.RibbonMarkScowling=True"; }
                else if (zh.Contains("不振之证")) { result += "\n.RibbonMarkSlump=True"; }
                else if (zh.Contains("微笑之证")) { result += "\n.RibbonMarkSmiley=True"; }
                else if (zh.Contains("悲伤之证")) { result += "\n.RibbonMarkTeary=True"; }
                else if (zh.Contains("不纯之证")) { result += "\n.RibbonMarkThorny=True"; }
                else if (zh.Contains("偶遇之证")) { result += "\n.RibbonMarkUncommon=True"; }
                else if (zh.Contains("自卑之证")) { result += "\n.RibbonMarkUnsure=True"; }
                else if (zh.Contains("爽快之证")) { result += "\n.RibbonMarkUpbeat=True"; }
                else if (zh.Contains("活力之证")) { result += "\n.RibbonMarkVigor=True"; }
                else if (zh.Contains("倦怠之证")) { result += "\n.RibbonMarkZeroEnergy=True"; }
                else if (zh.Contains("疏忽之证")) { result += "\n.RibbonMarkZonedOut=True"; }
                else if (zh.Contains("宝主之证")) { result += "\n.RibbonMarkTitan=True"; }
                if (zh.Contains("大个子之证")) { result += "\n.Scale=255\n.RibbonMarkJumbo=True"; }
                else if (zh.Contains("小个子之证")) { result += "\n.Scale=0\n.RibbonMarkMini=True"; }

            }

            // 添加技能 原因：PKHeX.Core.ShowdownSet#ParseLines中，若招式数满足4个则不再解析，所以招式文本应放在最后
            for (int moveCount = 0; moveCount < 4; moveCount++)
            {
                int candidateIndex = -1;
                int candidateLength = 0;
                for (int i = 0; i < GameStringsZh.Move.Count; i++)
                {
                    if (GameStringsZh.Move[i].Length == 0) continue;
                    if (!zh.Contains("-" + GameStringsZh.Move[i])) continue;
                    // 吸取 吸取拳
                    if (candidateIndex == -1 || GameStringsZh.Move[i].Length > candidateLength)
                    {
                        candidateIndex = i;
                        candidateLength = GameStringsZh.Move[i].Length;
                    }
                }
                if (candidateIndex == -1) continue;

                result += $"\n-{GameStringsEn.Move[candidateIndex]}";
                zh = zh.Replace("-" + GameStringsZh.Move[candidateIndex], "");
            }

            return result;
        }


        public static String getPkmName(int species)
        {
            return GameStringsZh.Species[species];

        }

        #region 形态中文ps字典，感谢ppllouf
        public static Dictionary<string, string> formDict = new Dictionary<string, string> {
            {"阿罗拉","Alola"},
            {"初始","Original"},
            {"丰缘","Hoenn"},
            {"神奥","Sinnoh"},
            {"合众","Unova"},
            {"卡洛斯","Kalos"},
            {"就决定是你了","Partner"},
            {"搭档","Starter"},
            {"世界","World"},
            {"摇滚巨星","Rock-Star"},
            {"贵妇","Belle"},
            {"流行偶像","Pop-Star"},
            {"博士","PhD"},
            {"面罩摔角手","Libre"},
            {"换装","Cosplay"},
            {"伽勒尔","Galar"},
            {"洗翠","Hisui"},
            {"帕底亚的样子斗战种","Paldea-Combat"},
            {"帕底亚的样子火炽种","Paldea-Blaze"},
            {"帕底亚的样子水澜种","Paldea-Aqua"},
            {"刺刺耳","Spiky-eared"},
            {"帕底亚","Paldea"},
            {"B","B"},
            {"C","C"},
            {"D","D"},
            {"E","E"},
            {"F","F"},
            {"G","G"},
            {"H","H"},
            {"I","I"},
            {"J","J"},
            {"K","K"},
            {"L","L"},
            {"M","M"},
            {"N","N"},
            {"O","O"},
            {"P","P"},
            {"Q","Q"},
            {"R","R"},
            {"S","S"},
            {"T","T"},
            {"U","U"},
            {"V","V"},
            {"W","W"},
            {"X","X"},
            {"Y","Y"},
            {"Z","Z"},
            {"！","Exclamation"},
            {"？","Question"},
            {"太阳的样子","Sunny"},
            {"雨水的样子","Rainy"},
            {"雪云的样子","Snowy"},
            {"原始回归的样子","Primal"},
            {"攻击形态","Attack"},
            {"防御形态","Defense"},
            {"速度形态","Speed"},
            {"砂土蓑衣","Sandy"},
            {"垃圾蓑衣","Trash"},
            {"晴天形态","Sunshine"},
            {"东海","East"},
            {"加热","Heat"},
            {"清洗","Wash"},
            {"结冰","Frost"},
            {"旋转","Fan"},
            {"切割","Mow"},
            {"起源","Origin"},
            {"天空","Sky"},
            {"格斗","Fighting"},
            {"飞行","Flying"},
            {"毒","Poison"},
            {"地面","Ground"},
            {"岩石","Rock"},
            {"虫","Bug"},
            {"幽灵","Ghost"},
            {"钢","Steel"},
            {"火","Fire"},
            {"水","Water"},
            {"草","Grass"},
            {"电","Electric"},
            {"超能力","Psychic"},
            {"冰","Ice"},
            {"龙","Dragon"},
            {"恶","Dark"},
            {"妖精","Fairy"},
            {"蓝条纹的样子","Blue-Striped"},
            {"白条纹的样子","White-Striped"},
            {"夏天的样子","Summer"},
            {"秋天的样子","Autumn"},
            {"冬天的样子","Winter"},
            {"灵兽形态","Therian"},
            {"暗黑","White"},
            {"焰白","Black"},
            {"觉悟的样子","Resolute"},
            {"舞步形态","Pirouette"},
            {"水流卡带","Douse"},
            {"闪电卡带","Shock"},
            {"火焰卡带","Burn"},
            {"冰冻卡带","Chill"},
            {"小智版","Ash"},
            {"冰雪花纹","Icy Snow"},
            {"雪国花纹","Polar"},
            {"雪原花纹","Tundra"},
            {"大陆花纹","Continental"},
            {"庭院花纹","Garden"},
            {"高雅花纹","Elegant"},
            {"摩登花纹","Modern"},
            {"大海花纹","Marine"},
            {"群鸟花纹","Archipelago"},
            {"荒野花纹","High Plains"},
            {"沙尘花纹","Sandstorm"},
            {"大河花纹","River"},
            {"骤雨花纹","Monsoon"},
            {"热带草原花纹","Savanna"},
            {"太阳花纹","Sun"},
            {"大洋花纹","Ocean"},
            {"热带雨林花纹","Jungle"},
            {"幻彩花纹","Fancy"},
            {"球球花纹","Pokeball"},
            {"蓝花","Blue"},
            {"橙花","Orange"},
            {"白花","White"},
            {"黄花","Yellow"},
            {"永恒","Eternal"},
            {"心形造型","Heart"},
            {"星形造型","Star"},
            {"菱形造型","Diamond"},
            {"淑女造型","Debutante"},
            {"贵妇造型","Matron"},
            {"绅士造型","Dandy"},
            {"女王造型","La Reine"},
            {"歌舞伎造型","Kabuki"},
            {"国王造型","Pharaoh"},
            {"小尺寸","Small"},
            {"大尺寸","Large"},
            {"特大尺寸","Super"},
            {"解放","Unbound"},
            {"啪滋啪滋风格","Pom-Pom"},
            {"呼拉呼拉风格","Pa'u"},
            {"轻盈轻盈风格","Sensu"},
            {"黑夜的样子","Midnight"},
            {"黄昏的样子","Dusk"},
            {"流星的样子","Meteor"},
            {"橙色核心","Orange"},
            {"黄色核心","Yellow"},
            {"绿色核心","Green"},
            {"浅蓝色核心","Blue"},
            {"蓝色核心","Indigo"},
            {"紫色核心","Violet"},
            {"黄昏之鬃","Dusk-Mane"},
            {"拂晓之翼","Dawn-Wings"},
            {"究极","Ultra"},
            {"５００年前的颜色","Original"},
            {"低调的样子","Low-Key"},
            {"真品","Antique"},
            {"奶香红钻","Ruby-Cream"},
            {"奶香抹茶","Matcha-Cream"},
            {"奶香薄荷","Mint-Cream"},
            {"奶香柠檬","Lemon-Cream"},
            {"奶香雪盐","Salted-Cream"},
            {"红钻综合","Ruby-Swirl"},
            {"焦糖综合","Caramel-Swirl"},
            {"三色综合","Rainbow-Swirl"},
            {"剑之王","Crowned"},
            {"盾之王","Crowned"},
            {"无极巨化","Eternamax"},
            {"连击流","Rapid-Strike"},
            {"阿爸","Dada"},
            {"骑白马的样子","Ice"},
            {"骑黑马的样子","Shadow"},
            {"四只家庭","Four"},
            {"蓝羽毛","Blue"},
            {"黄羽毛","Yellow"},
            {"白羽毛","White"},
            {"下垂姿势","Droopy"},
            {"平挺姿势","Stretchy"},
            {"三节形态","Three-Segment"},
        };
        #endregion

    }


}