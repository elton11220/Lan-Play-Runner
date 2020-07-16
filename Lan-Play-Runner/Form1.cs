using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security.Cryptography;
using MetroFramework.Forms;
using System.Xml;
using System.Diagnostics;

namespace Lan_Play_Runner
{
    public partial class Form1 : MetroForm
    {
        #region User32.dll
        //User32.dll
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
        [DllImport("kernel32")]
        private static extern int WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);
        //ShowWindow参数
        private const int SW_SHOWNORMAL = 1;
        private const int SW_RESTORE = 9;
        private const int SW_SHOWNOACTIVATE = 4;
        //SendMessage参数
        private const int WM_KEYDOWN = 0X0100;
        private const int WM_KEYUP = 0X0101;
        private const int WM_CHAR = 0X102;
        #endregion
        #region 变量常量
        private const int Runner_Ver = 2;
        private static Boolean IfRunnerHaveUpdate = false; //登录器是否有更新赋值
        private static Boolean IfServerDown = false; //服务器是否维护
        private static Boolean IfServerFailed = false; //登录器服务器是否连接失败
        //
        public static string sys_path = System.Environment.GetFolderPath(Environment.SpecialFolder.Windows); //windows安装目录
        public static string me_path = System.IO.Directory.GetCurrentDirectory(); //程序路径
        //
        private static string status_page = ""; //登录器服务器信息寄存
        //
        public static IntPtr hwnd; //LanPlay窗口句柄传值
        //
        private static string A_Path1 = me_path + "\\data\\sys32\\wpcap.dll";
        private static string A_Path2 = me_path + "\\data\\sys32\\Packet.dll";
        private static string A_Path3 = me_path + "\\data\\sys32\\npf.sys";
        private static string A_Path4 = me_path + "\\data\\sys64\\wpcap.dll";
        private static string A_Path5 = me_path + "\\data\\sys64\\Packet.dll";
        private static string A_Path6 = me_path + "\\data\\sys64\\pthreadVC.dll";
        private static string B_Path1 = sys_path + "\\System32\\wpcap.dll";
        private static string B_Path2 = sys_path + "\\System32\\Packet.dll";
        private static string B_Path3 = sys_path + "\\System32\\drivers\\npf.sys";
        private static string B_Path4 = sys_path + "\\SysWOW64\\wpcap.dll";
        private static string B_Path5 = sys_path + "\\SysWOW64\\Packet.dll";
        private static string B_Path6 = sys_path + "\\SysWOW64\\pthreadVC.dll";
        #endregion

        #region 多服务器切换变量
        private static string Data_Path = me_path + "\\data\\config.dat";
        public static int Default_Entry = 0;
        public static string Server_1 = "";
        public static string Server_2 = "";
        public static string Server_3 = "";
        public static string Server_4 = "";
        public static string Server_5 = "";
        //
        public static string Default_ServerIP = "ns.lianji123.top:11451";
        public static string Default_RunnerIP = "elton1122.top/App/status.html";
        public static string Default_RunnerOriginalIP = "elton1122.top";
        public static string Server_IP = "";
        public static string Runner_IP = "";
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void initializeDefault()  //默认服务器程序初始化主函数(Second-Func)
        {
            if(Default_Entry != 0)
            {
                initializeCustom(Default_Entry);
                return;
            }
            //检查登录器更新
            if (IfRunnerHaveUpdate == true)
            {
                if(Default_Entry ==0)
                {
                    this.Text = "需要更新";
                    metroLabel5.Text = "需要更新";
                    label2.Text = "";
                    metroLabel8.Text = "需要更新";
                    metroLabel6.Text = "需要更新";
                    metroLabel7.Text = "需要更新";
                    metroLabel10.Text = "需要更新";
                }
                if(MessageBox.Show("有可用更新，点击确定打开更新程序", "更新",MessageBoxButtons.YesNo,MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    if (System.IO.File.Exists(me_path + "\\Lan-Play-Runner-Updater.exe")) //判断更新程序是否存在
                    {
                        System.Diagnostics.Process.Start(me_path + "\\Lan-Play-Runner-Updater.exe");
                        Thread.Sleep(50);
                        System.Environment.Exit(0);
                    }
                    else
                    {
                        MessageBox.Show("更新程序被破坏，请重新下载", "错误");
                    }
                }
                else
                {
                    MessageBox.Show("您取消了更新，暂时不能连接内置服务器","注意");
                }
                return;
            }
            //
            switch (CheckStatus(status_page))
            {
                case 0 :
                    metroLabel5.Text = "正常";
                    label2.Text = "成功连接到服务器";
                    metroLabel8.Text = GetInfo("online");
                    metroLabel6.Text = GetInfo("version");
                    metroLabel7.Text = GetServerMessage();
                    metroLabel10.Text = GetPing(GetOriginalUrl(Server_IP), 1);
                    break;
                case 1 :
                    metroLabel5.Text = "服务器维护";
                    label2.Text = "服务器正在维护";
                    metroLabel8.Text = "";
                    metroLabel6.Text = "";
                    metroLabel7.Text = GetServerMessage();
                    metroLabel10.Text = "";
                    IfServerDown = true;
                    break;
                case 2 :
                    metroLabel5.Text = "公告服务器连接超时";
                    label2.Text = "公告服务器连接超时";
                    metroLabel8.Text = "";
                    metroLabel6.Text = "";
                    metroLabel7.Text = "";
                    metroLabel10.Text = "";
                    IfServerFailed = true;
                    break;
            }
        }

        private void initializeCustom(int Server_ID) //自定义服务器初始化主函数
        {
            if (Default_Entry == 0)
            {
                initializeDefault();
                return;
            }
            //检查登录器更新
            if (IfRunnerHaveUpdate == true)
            {
                this.Text = "需要更新";
                if (MessageBox.Show("有可用更新，点击确定打开更新程序", "更新", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    if (System.IO.File.Exists(me_path + "\\Lan-Play-Runner-Updater.exe")) //判断更新程序是否存在
                    {
                        System.Diagnostics.Process.Start(me_path + "\\Lan-Play-Runner-Updater.exe");
                        Thread.Sleep(50);
                        System.Environment.Exit(0);
                    }
                    else
                    {
                        MessageBox.Show("更新程序被破坏，请重新下载", "错误");
                    }
                }
                else
                {
                    MessageBox.Show("您取消了更新，暂时不能连接内置服务器","注意");
                }
            }
            //
            string nowping = "";
            nowping = GetPing(GetOriginalUrl(Server_IP), 1);
            if (nowping == "999")
            {
                metroLabel5.Text = "错误";
                label2.Text = "服务器超时或错误";
                metroLabel8.Text = "错误";
                metroLabel6.Text = "错误";
                metroLabel7.Text = "自定义服务器";
                metroLabel10.Text = "999";
                MessageBox.Show("您可能添加了错误的自定义服务器或自定义服务器超时\n请检查您的服务器链接或更换服务器\n否则可能导致程序崩溃", "警告");
            }
            else
            {
                metroLabel5.Text = "正常";
                label2.Text = "成功连接到自定义服务器";
                metroLabel8.Text = GetInfo("online");
                metroLabel6.Text = GetInfo("version");
                metroLabel7.Text = "自定义服务器";
                metroLabel10.Text = nowping;
            }
        }

        private static string GetServerMessage()
        {
            if (status_page == "")
            {
                return "获取消息失败";
            }
            else
            {
                JObject obj = JObject.Parse(status_page);
                string Server_Message = (string)obj["message"];
                return Server_Message;
            }
        } //获取登录器服务器消息(Second-Func)

        private void CheckUpdate() //检查登录器更新(Second-Func)
        {
            if (status_page == "")
            {
                IfRunnerHaveUpdate = false;
            }
            else
            {
                JObject obj = JObject.Parse(status_page);
                if (Runner_Ver < (int)obj["version"])
                {
                    IfRunnerHaveUpdate = true;
                }
                else
                {
                    IfRunnerHaveUpdate = false;
                }
            }
        }

        private static int CheckStatus(string Page_Content)
        {
            if (status_page == "")
            {
                return 2;
            }
            else
            {
                JObject obj = JObject.Parse(Page_Content);
                int Server_Status = (int)obj["status"];
                switch (Server_Status)
                {
                    case 0:
                        return 0; //启动器正常
                    case 1:
                        return 1; //服务器维护
                    default:
                        return 1; //服务器维护
                }
            }
        }  //校验登录器服务器状态(Second-Func)

        public static string GetInfo(string item)
        {
            string Server_info = GetServer(ConvertServerUrl(Server_IP) + "/info"); //连接服务器初步获取信息
            //Json解析
            JObject obj = JObject.Parse(Server_info);
            int Server_Online = (int)obj["online"];
            string Server_Version = (string)obj["version"];
            //格式化数据
            string Ser_Online = Convert.ToString(Server_Online);
            string Ser_Version = Convert.ToString(Server_Version);
            switch (item)
            {
                case "online":
                    return Ser_Online;
                case "version":
                    return Ser_Version;
                default:
                    return "Error";
            }
        }  //解析游戏服务器信息(Second-Func)

        private static string GetServer(string urladdress)
        {
            WebClient MyWebClient = new WebClient();
            MyWebClient.Credentials = CredentialCache.DefaultCredentials;
            Byte[] pageData = MyWebClient.DownloadData(urladdress); 
            //string pageHtml = Encoding.Default.GetString(pageData);  //GB2312 Page           
            string pageHtml = Encoding.UTF8.GetString(pageData); //UTF-8 Page
            Debug.WriteLine("[Debug]GetServer Output-> " + pageHtml);
            return pageHtml;
        }  //连接服务器获取JSON主函数(Main-Func)

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            status_page = webBrowser1.Document.Body.InnerHtml;
            Debug.WriteLine("statuspage" + status_page);
            if (Default_Entry == 0)
            {
                CheckUpdate();
                initializeDefault();
            }
            else
            {
                CheckUpdate();
                initializeCustom(Default_Entry);
            }
        } //下载登录器服务器消息完毕
        public static Boolean IfHaveFile(string file_address)
        {
            string MyfileNname = file_address;
            if (MyfileNname.Length < 1)
                return false;
            string ShortName = MyfileNname.Substring(MyfileNname.LastIndexOf("\\") + 1);
            if (File.Exists(MyfileNname))
            {
                return true;
            }
            else
            {
                return false;
            }
        } //检测文件是否存在（Main-Func)
        private string Execute(string command) //执行命令行主函数(Main-Func)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();
            p.StandardInput.WriteLine(command + "&exit");
            p.StandardInput.AutoFlush = true;
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            p.Close();
            return output;
        }
        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Version 2.0\n该软件仅供技术交流，请勿用于其他商业行为或违法犯罪行为\n请于下载后24小时之内删除\n制作人：Elton11220\nhttps://github.com/elton11220","关于");
        }
        private void RunLanPlay() //调用Lan-Play主函数(Main-Func)
        {
            if (Environment.Is64BitOperatingSystem)
            {
                if (System.IO.File.Exists(me_path + "\\data\\lan-play-win64.exe"))
                {
                    System.Diagnostics.Process.Start(me_path + "\\data\\lan-play-win64.exe");
                    LanPlayInject("win64",Server_IP);
                }
                else
                {
                    MessageBox.Show("主程序数据被破坏，请重新下载", "错误");
                    System.Environment.Exit(0);
                }
            }
            else
            {
                if (System.IO.File.Exists(me_path + "\\data\\lan-play-win32.exe"))
                {
                    System.Diagnostics.Process.Start(me_path + "\\data\\lan-play-win32.exe");
                    LanPlayInject("win32",Server_IP);
                }
                else
                {
                    MessageBox.Show("主程序数据被破坏，请重新下载", "错误");
                    System.Environment.Exit(0);
                }
            }
        }
        public static int Asc(string character)
        {
            if (character.Length == 1)
            {
                System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                int intAsciiCode = (int)asciiEncoding.GetBytes(character)[0];
                return (intAsciiCode);
            }
            else
            {
                throw new Exception("Character is not valid.");
            }

        }
        private void LanPlayInject(string SysType, string Url) //注入Lan-Play参数
        {
            string exe_path = me_path + "\\data\\lan-play-" + SysType + ".exe";
            Thread.Sleep(100);
            IntPtr myIntPtr = FindWindow(null, exe_path);
            hwnd = myIntPtr;
            Thread.Sleep(5);
            ShowWindow(myIntPtr, 0);
            string url2 = ConvertServerUrl2(Url);
            Debug.WriteLine("LanPlayInject将发送的URL为:" + url2);
            Char[] chArr = url2.ToCharArray();
            int conv;
            foreach (var c in chArr)
            {
                conv = Asc(Convert.ToString(c));
                Thread.Sleep(10);
                SendMessage(myIntPtr, WM_CHAR, conv, 0);
            }
            PostMessage(myIntPtr, WM_KEYDOWN, 0x0D, 0);
            PostMessage(myIntPtr,WM_KEYUP , 0x0D, 0);
            Form2 form = new Form2();
            form.Show();
            this.Hide();
        }
        /*
        private void LanPlayInjectB(string SysType, string Url)
        {
            Form2 form = new Form2();
            this.Hide();
            form.Show();
            string url2;
            string exe_path = me_path + "\\data\\lan-play-" + SysType + ".exe";
            url2 = ConvertServerUrl2(Url);
            Execute("start " + exe_path + " --relay-server-addr " + url2);
            Thread.Sleep(3000);
            IntPtr myIntPtr = FindWindow(null, exe_path);
            hwnd = myIntPtr;
            ShowWindow(myIntPtr, 0);
        }
        */
        private void FixRuntime() //修复运行环境函数
        {
            if (Environment.Is64BitOperatingSystem)
            {
                if (MessageBox.Show("检测到您的运行环境缺失，点击确定进行修复，点击取消退出程序", "修复运行环境-64Bit", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    if (IfHaveFile(A_Path1) == true && IfHaveFile(A_Path2) == true && IfHaveFile(A_Path3) == true && IfHaveFile(A_Path4) == true && IfHaveFile(A_Path5) == true && IfHaveFile(A_Path6) == true)
                    {
                        File.Copy(A_Path1, B_Path1, true);
                        File.Copy(A_Path2, B_Path2, true);
                        File.Copy(A_Path3, B_Path3, true);
                        File.Copy(A_Path4, B_Path4, true);
                        File.Copy(A_Path5, B_Path5, true);
                        File.Copy(A_Path6, B_Path6, true);
                        Execute("sc create npf binPath= System32\\drivers\\npf.sys type= kernel start= auto error= normal tag= no DisplayName= \"NetGroup Packet Filter Driver\"");
                        Execute("sc start npf");
                        MessageBox.Show("修复成功，请重新打开程序", "修复成功！");
                    }
                    else
                    {
                        MessageBox.Show("应用程序数据被破坏，请重新下载", "错误");
                    }
                }
            }
            else
            {
                if (MessageBox.Show("检测到您的运行环境缺失，点击确定进行修复，点击取消退出程序", "修复运行环境-32Bit", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    if (IfHaveFile(A_Path1) == true && IfHaveFile(A_Path2) == true && IfHaveFile(A_Path3) == true)
                    {
                        File.Copy(A_Path1, B_Path1, true);
                        File.Copy(A_Path2, B_Path2, true);
                        File.Copy(A_Path3, B_Path3, true);
                        Execute("sc create npf binPath= System32\\drivers\\npf.sys type= kernel start= auto error= normal tag= no DisplayName= \"NetGroup Packet Filter Driver\"");
                        Execute("sc start npf");
                        MessageBox.Show("修复成功，请重新打开程序", "修复成功！");
                    }
                    else
                    {
                        MessageBox.Show("应用程序数据被破坏，请重新下载", "错误");
                    }
                }
            }
        }
        private static Boolean CheckRuntime() //检查运行环境
        {
            if (Environment.Is64BitOperatingSystem)
            {
                if(IfHaveFile(B_Path1) == true && IfHaveFile(B_Path2) == true && IfHaveFile(B_Path3) == true && IfHaveFile(B_Path4) == true && IfHaveFile(B_Path5) == true && IfHaveFile(B_Path6) == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (IfHaveFile(B_Path1) == true && IfHaveFile(B_Path2) == true && IfHaveFile(B_Path3) == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        private void metroButton1_Click(object sender, EventArgs e)
        {
            if (CheckRuntime() == true)
            {
                if (IfRunnerHaveUpdate == true && Default_Entry == 0)
                {
                    MessageBox.Show("请更新程序或连接自定义服务器", "警告");
                }
                else
                {
                    if (IfServerDown == true)
                    {
                        MessageBox.Show("服务器维护中", "维护");
                    }
                    else
                    {
                        if (IfServerFailed == true)
                        {
                            if (MessageBox.Show("本程序可能已经停止维护或连接超时\n将尝试直接连接到游戏服务器\n若服务器仍无效程序可能崩溃\n\n仍要尝试连接吗？", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                RunLanPlay();
                            }
                        }
                        else
                        {
                            RunLanPlay();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("请先修复运行环境", "警告");
            }
        }
        public static string GetPing(string URL, int Mode)
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(URL,1000);
                if (reply != null)
                {
                    switch(Mode)
                    {
                        case 0:
                            return Convert.ToString(reply.Status); //状态
                        case 1:
                            return reply.RoundtripTime.ToString(); //时间
                        case 2:
                            return Convert.ToString(reply.Address); //地址
                        case 3:
                            return reply.ToString(); //响应
                        default:
                            return "999";
                    }
                }
                else
                {
                    return "999";
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("timeout error: {0}", e);
                return "999";
            }
        }
        private void metroButton2_Click(object sender, EventArgs e)
        {
            Form3 form = new Form3();
            form.Show();
            //this.Hide();
        }
        public static string ReadINI(string section, string key, string def, string filePath)
        {
            StringBuilder sb = new StringBuilder(1024);
            GetPrivateProfileString(section, key, def, sb, 1024, filePath);
            return sb.ToString();
        }
        public void WriteINI(string section, string key, string value, string filePath)
        {
            if(Form1.IfHaveFile(filePath))
            {
                 WritePrivateProfileString(section, key, value, filePath);
            }
        }
        private void LoadConfig()
        {
            if (IfHaveFile(Data_Path))
            {
                Default_Entry = Convert.ToInt32(ReadINI("settings", "default", "0", Data_Path));
                Server_1 = ReadINI("servers", "1", "", Data_Path);
                Server_2 = ReadINI("servers", "2", "", Data_Path);
                Server_3 = ReadINI("servers", "3", "", Data_Path);
                Server_4 = ReadINI("servers", "4", "", Data_Path);
                Server_5 = ReadINI("servers", "5", "", Data_Path); 
            }
            else
            {
                File.Create(Data_Path);
                WriteINI("servers", "1", "", Data_Path);
                WriteINI("servers", "2", "", Data_Path);
                WriteINI("servers", "3", "", Data_Path);
                WriteINI("servers", "4", "", Data_Path);
                WriteINI("servers", "5", "", Data_Path);
                WriteINI("settings", "default", "0", Data_Path);
                Default_Entry = Convert.ToInt32(ReadINI("settings", "default", "0", Data_Path));
                Server_1 = ReadINI("servers", "1", "", Data_Path);
                Server_2 = ReadINI("servers", "2", "", Data_Path);
                Server_3 = ReadINI("servers", "3", "", Data_Path);
                Server_4 = ReadINI("servers", "4", "", Data_Path);
                Server_5 = ReadINI("servers", "5", "", Data_Path);
            }
        }
        private void CheckDefaultEntry()
        {
            switch(Default_Entry)
            {
                case 0:
                    Server_IP = Default_ServerIP;
                    Runner_IP = Default_RunnerIP;
                    break;
                case 1:
                    Server_IP = Server_1;
                    Runner_IP = Default_RunnerIP;
                    label1.Text = "当前选择：服务器" + Convert.ToString(Default_Entry);
                    break;
                case 2:
                    Server_IP = Server_2;
                    Runner_IP = Default_RunnerIP;
                    label1.Text = "当前选择：服务器" + Convert.ToString(Default_Entry);
                    break;
                case 3:
                    Server_IP = Server_3;
                    Runner_IP = Default_RunnerIP;
                    label1.Text = "当前选择：服务器" + Convert.ToString(Default_Entry);
                    break;
                case 4:
                    Server_IP = Server_4;
                    Runner_IP = Default_RunnerIP;
                    label1.Text = "当前选择：服务器" + Convert.ToString(Default_Entry);
                    break;
                case 5:
                    Server_IP = Server_5;
                    Runner_IP = Default_RunnerIP;
                    label1.Text = "当前选择：服务器" + Convert.ToString(Default_Entry);
                    break;
                default:
                    Server_IP = Default_ServerIP;
                    Runner_IP = Default_RunnerIP;
                    break;
            }
        }
        public static string GetOriginalUrl(string Input)
        {
            string str = Input;
            string cache ="";
            char c = ':';
            if(str.Contains("/info"))
            {
                cache = str.Substring(0, str.Length - 5);
            }
            else
            {
                cache = str;
            }
            if (cache.Contains("http://"))
            {
                cache = str.Substring(7);
            }
            else
            {
                if (str.Contains("https://"))
                {
                    cache = str.Substring(8);
                }
                else
                {
                    cache = str;
                }
            }
            if(cache.Contains(":"))
            {
                cache = cache.Substring(0, cache.IndexOf(c));
            }
            return cache;
        } //xxx.xxx.xxx.xxx
        public static string ConvertServerUrl(string Input) //http://xxx.xxx.xxx.xxx:port
        {
             if(Input.Contains("ttp:"))
            {
                return Input;
            }
             else
            {
                Input = "http://" + Input;
                return Input;
            }
        }
        public static string ConvertServerUrl2(string Input) //xxx.xxx.xxx.xxx:port
        {
            if(Input.Contains("http://"))
            {
                Input = Input.Substring(7);
                return Input;
            }
            else
            {
                if (Input.Contains("https://"))
                {
                    Input = Input.Substring(8);
                    return Input;
                }
                else
                {
                    return Input;
                }
            }

        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            LoadConfig();//载入配置文件
            CheckDefaultEntry();
            if (CheckRuntime() == true) //检查运行环境是否安装
            {
                try
                {
                    webBrowser1.Navigate(Runner_IP);   
                }
                catch
                {
                    status_page = "";
                    if (Default_Entry == 0)
                    {
                        CheckUpdate();
                        initializeDefault();
                    }
                    else
                    {
                        CheckUpdate();
                        initializeCustom(Default_Entry);
                    }
                }
            }
            else
            {
                FixRuntime();
                System.Environment.Exit(0);
            }
            Debug.WriteLine("statuspage" + status_page);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}