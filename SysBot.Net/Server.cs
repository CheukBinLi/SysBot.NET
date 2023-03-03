using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using SysBot.Net.service;
using SysBot.Pokemon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SysBot.Net.model;
using SysBot.Net.handler;
using PKHeX.Core;

namespace SysBot.Net
{
    public struct Server
    {
        private Thread serverThread;
        private volatile Boolean interrupt = false;
        private CommandManager commandManager { get; } = new CommandManager();
        public LegalitySettings legalitySettings { get; } = new LegalitySettings();

        public Server()
        {
            commandManager
                 .appendHandle(new DecodeFakeTrainerSAVHandler())
                 .appendHandle(new DecodeTradePartnerMyStatusHandler())
                 .appendHandle(new GeneratePokemonHandler())
                 .appendHandle(new LoadResourceHandler())
                 .appendHandle(new PKMValidityVerificationHandler());
        }

        public void start()
        {
            serverThread = new Thread(serverStart);
            serverThread.Start();
        }

        public void stop()
        {
            if (null != serverThread)
            {
                serverThread.Interrupt();
            }
        }

        public void restart()
        {
            stop();
            start();
        }

        public void serverStart()
        {

            //string ip = "192.168.50.114";
            string ip = "127.0.0.1";
            int port = 1111;
            Console.WriteLine("服务器启动....." + new DateTime());
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            EndPoint point = new IPEndPoint(IPAddress.Parse(ip), port);
            server.Bind(point);
            server.Listen(100);

            //初始化]
            //legalitySettings.GenerateLanguage = LanguageID.ChineseS;
            legalitySettings.GenerateOT = "Makeiio";
            AutoLegalityWrapper.EnsureInitialized(legalitySettings);

            Console.WriteLine("服务启动。");

            while (true && !interrupt)
            {
                Socket socket = server.Accept();
                Console.WriteLine("客服接入成功。");
                try
                {
                    doReceive(socket);
                }
                catch (Exception e)
                {
                    Console.WriteLine("服务异常。");
                    Console.WriteLine(e.StackTrace);
                    try
                    {
                        sendMessage(socket, e.StackTrace);
                    }catch (Exception ex) { }
                }
                Console.WriteLine("客服结束服务。");
            }
        }

        void doReceive0(Socket socket)
        {
            int len = 0;
            String startStr = "@#";//64,35
            bool readIng = false;
            byte[] buffer = new byte[256];
            byte pre = 0;
            byte[] data;
            int dataLenHaftReadIdex = 0;
            int dataLenHaftReadCurrentIdex = 0;
            int dataReadLen = 0;
            int dataLen = 0;
            MemoryStream memoryStream = new MemoryStream();
            do
            {
                len = socket.Receive(buffer);
                //Console.WriteLine("BUFFER：" + len + "  :" + Encoding.UTF8.GetString(buffer, 0, len));
                if (len == 0)
                {
                    Console.WriteLine("断开连接");
                    socket.Close();
                    return;
                }
                pre = buffer[0];
                for (int i = 0; i < len; i++)
                {
                    if (readIng)
                    {
                        if (dataLenHaftReadIdex > 0)
                        {
                            dataLenHaftReadCurrentIdex = read(buffer, ref memoryStream, i, len, dataLenHaftReadIdex);
                            if (dataLenHaftReadCurrentIdex <= 0)
                            {
                                data = memoryStream.ToArray();
                                dataLen = dataReadLen = BitConverter.ToInt32(data);
                                memoryStream.Seek(0, SeekOrigin.Begin);
                                memoryStream.SetLength(0);
                                i += dataLenHaftReadIdex;
                            }
                            else
                            { //继续读
                                readIng = true;
                                dataLenHaftReadIdex = dataLenHaftReadCurrentIdex;
                                break;
                            }
                        }
                        dataReadLen = read(buffer, ref memoryStream, i, len, dataReadLen);
                        if (dataReadLen <= 0)
                        {
                            //DO XXX
                            //Console.WriteLine(Encoding.UTF8.GetString(memoryStream.ToArray(), 0, dataLen));

                            CommandModel command = JsonConvert.DeserializeObject<CommandModel>(Encoding.UTF8.GetString(memoryStream.ToArray(), 0, dataLen));


                            readIng = false;
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            memoryStream.SetLength(0);
                            i += dataLen;
                            continue;
                        }
                        else
                        {
                            break;
                        }

                    }
                    else if (pre == '@' && buffer[i] == '#')
                    {
                        readIng = true;
                        dataLenHaftReadCurrentIdex = read(buffer, ref memoryStream, ++i, len, 4);
                        if (dataLenHaftReadIdex <= 0)
                        {
                            data = memoryStream.ToArray();
                            dataLen = dataReadLen = BitConverter.ToInt32(data);
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            memoryStream.SetLength(0);

                            dataReadLen = read(buffer, ref memoryStream, i += 4, len, dataReadLen);

                            if (dataReadLen <= 0)
                            {
                                readIng = false;
                                //DO XXX
                                //Console.WriteLine("方法下：" + Encoding.UTF8.GetString(memoryStream.ToArray()));

                                CommandModel command = JsonConvert.DeserializeObject<CommandModel>(Encoding.UTF8.GetString(memoryStream.ToArray(), 0, dataLen));


                                memoryStream.Seek(0, SeekOrigin.Begin);
                                memoryStream.SetLength(0);
                                i += dataLen;
                                continue;
                            }
                            else
                            {
                                break;
                            }

                        }
                        else
                        {
                            dataLenHaftReadIdex = dataLenHaftReadCurrentIdex;
                            break;
                        }
                    }
                    pre = buffer[i];
                }
            } while (true);
        }

        void doReceive(Socket socket)
        {
            int len = 0;
            String startStr = "@#";//64,35
            bool readIng = false;
            byte[] buffer = new byte[256];
            byte pre = 0;
            byte[] data;
            int dataLenHaftReadIdex = 0;
            int dataLenHaftReadCurrentIdex = 0;
            int dataReadLen = 0;
            int dataLen = 0;
            MemoryStream memoryStream = new MemoryStream();
            do
            {
                len = socket.Receive(buffer);
                //Console.WriteLine("BUFFER：" + len + "  :" + Encoding.UTF8.GetString(buffer, 0, len));
                if (len == 0)
                {
                    Console.WriteLine("断开连接");
                    socket.Close();
                    return;
                }
                pre = buffer[0];
                for (int i = 0; i < len; i++)
                {
                    if (readIng)
                    {
                        if (dataLenHaftReadIdex > 0)
                        {
                            dataLenHaftReadCurrentIdex = read(buffer, ref memoryStream, i, len, dataLenHaftReadIdex);
                            if (dataLenHaftReadCurrentIdex <= 0)
                            {
                                data = memoryStream.ToArray();
                                dataLen = dataReadLen = BitConverter.ToInt32(data);
                                memoryStream.Seek(0, SeekOrigin.Begin);
                                memoryStream.SetLength(0);
                                i += dataLenHaftReadIdex - 1;
                                dataLenHaftReadIdex = 0;
                                continue;
                            }
                            else
                            { //继续读
                                readIng = true;
                                dataLenHaftReadIdex = dataLenHaftReadCurrentIdex;
                                break;
                            }
                        }
                        dataReadLen = read(buffer, ref memoryStream, i, len, dataReadLen);
                        if (dataReadLen <= 0)
                        {
                            //DO XXX
                            String commandMsg = Encoding.UTF8.GetString(memoryStream.ToArray(), 0, dataLen);
                            //Console.WriteLine("收到命令:" + commandMsg);
                            CommandModel command = JsonConvert.DeserializeObject<CommandModel>(commandMsg);
                            commandManager.execute(ref this, ref socket, command);

                            readIng = false;
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            memoryStream.SetLength(0);
                            i += dataLen - 1;
                            continue;
                        }
                        else
                        {
                            break;
                        }

                    }
                    else if (pre == '@' && buffer[i] == '#')
                    {
                        readIng = true;
                        dataLenHaftReadIdex = 4;
                        continue;
                    }
                    pre = buffer[i];
                }
            } while (true);
        }

        int read(byte[] data, ref MemoryStream memoryStream, int dataIndex, int dataLastIndex, int len)
        {
            if (data.Length > dataIndex && dataIndex < dataLastIndex)
            {
                memoryStream.WriteByte(data[dataIndex++]);
                if (--len > 0)
                {
                    return read(data, ref memoryStream, dataIndex, dataLastIndex, len);
                }
            }
            return len;
        }

        public void sendMessage(Socket socket, CommandModel commandModel)
        {
            socket.Send(System.Text.Encoding.Default.GetBytes(JsonConvert.SerializeObject(commandModel)));
        }

        public void sendMessage(Socket socket, String msg)
        {
            CommandModel commandModel = new CommandModel();
            commandModel.code = -1;
            commandModel.error = msg;
            socket.Send(System.Text.Encoding.Default.GetBytes(JsonConvert.SerializeObject(commandModel)));
        }


        public void test1()
        {
            byte[] data = System.Text.Encoding.Default.GetBytes("比卡丘");
            var additional = new Newtonsoft.Json.Linq.JObject();
            String converName = ShowdownTranslator<PK9>.Chinese2Showdown(Encoding.UTF8.GetString(data, 0, data.Length), ref additional, out additional);
            Console.WriteLine("converName");
        }

        void t2()
        {
            //string ip = "192.168.50.114";
            string ip = "127.0.0.1";
            int port = 1111;
            Console.WriteLine("服务器启动....." + new DateTime());
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            EndPoint point = new IPEndPoint(IPAddress.Parse(ip), port);
            server.Bind(point);
            server.Listen(100);

            Console.WriteLine("a client connect.....");
            byte[] buffer = new byte[1024];
            LegalitySettings legalitySettings = new Pokemon.LegalitySettings();
            legalitySettings.GenerateOT = "叼拿星";
            AutoLegalityWrapper.EnsureInitialized(legalitySettings);
            Socket socket = server.Accept();

            while (true && !interrupt)
            {
                int len = 0;
                bool isEnd = false;
                do
                {
                    len = socket.Receive(buffer);
                } while (isEnd);


                String converName = Encoding.UTF8.GetString(buffer, 0, len);
                Console.WriteLine(converName);
                //String converName = ShowdownTranslator<PK9>.Chinese2Showdown(Encoding.UTF8.GetString(buffer, 0, len));
                //ShowdownSet set = ShowdownUtil.ConvertToShowdown(converName);
                ShowdownSet set = ShowdownUtil.ConvertToShowdown(converName);

                var template = AutoLegalityWrapper.GetTemplate(set);

                ITrainerInfo sav = AutoLegalityWrapper.GetTrainerInfo<PK9>();
                PKM pkm = sav.GetLegal(template, out var result);
                if (sav != null)
                {
                    // Update PKM to the current save's handler data
                    ((PK9)pkm).Trade(sav);
                    pkm.RefreshChecksum();
                }

                pkm.ResetPartyStats();

                //加密
                //byte[] data = SysBot.Base.Decoder.ConvertHexByteStringToBytes(pkm.EncryptedBoxData);
                byte[] data = pkm.EncryptedBoxData;
                len = data.Length;
                byte[] src = new byte[4];
                src[3] = (byte)((len >> 24) & 0xFF);
                src[2] = (byte)((len >> 16) & 0xFF);
                src[1] = (byte)((len >> 8) & 0xFF);
                src[0] = (byte)(len & 0xFF);


                Thread.Sleep(50);
                socket.Send(src);

                Thread.Sleep(50);
                socket.Send(data);

                Thread.Sleep(50);
            }
        }
    }
}

