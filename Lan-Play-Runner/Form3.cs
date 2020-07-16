using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace Lan_Play_Runner
{
    public partial class Form3 : MetroForm
    {
        private static string Data_Path = Form1.me_path + "\\data\\config.dat";
        public static int Default_Entry = 0;
        public static string Server_1 = "";
        public static string Server_2 = "";
        public static string Server_3 = "";
        public static string Server_4 = "";
        public static string Server_5 = "";
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
        [DllImport("kernel32")]
        private static extern int WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);

        public Form3()
        {
            InitializeComponent();
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            WriteINI("servers", "1", metroTextBox1.Text, Data_Path);
            WriteINI("servers", "2", metroTextBox2.Text, Data_Path);
            WriteINI("servers", "3", metroTextBox3.Text, Data_Path);
            WriteINI("servers", "4", metroTextBox4.Text, Data_Path);
            WriteINI("servers", "5", metroTextBox5.Text, Data_Path);
            WriteINI("settings", "default", Convert.ToString(metroComboBox1.SelectedIndex), Data_Path);
            if (Form1.GetOriginalUrl(metroTextBox1.Text) == Server_1 && Form1.GetOriginalUrl(metroTextBox2.Text) == Server_2 && Form1.GetOriginalUrl(metroTextBox3.Text) == Server_3 && Form1.GetOriginalUrl(metroTextBox4.Text) == Server_4 && Form1.GetOriginalUrl(metroTextBox5.Text) == Server_5 && metroComboBox1.SelectedIndex == Default_Entry)
            {
                MessageBox.Show("保存成功", "成功");
                this.FormClosing -= new FormClosingEventHandler(this.Form3_FormClosing);
                this.Dispose();
            }
            else
            {
                MessageBox.Show("请重新打开软件以加载数据", "提示");
                this.FormClosing -= new FormClosingEventHandler(this.Form3_FormClosing);
                System.Environment.Exit(0);
            }
        }

        private void LoadConfig()
        {
            if(Form1.IfHaveFile(Data_Path))
            {
                Default_Entry = Convert.ToInt32(ReadINI("settings", "default", "0", Data_Path));
                Server_1 = ReadINI("servers", "1", "", Data_Path);
                Server_2 = ReadINI("servers", "2", "", Data_Path);
                Server_3 = ReadINI("servers", "3", "", Data_Path);
                Server_4 = ReadINI("servers", "4", "", Data_Path);
                Server_5 = ReadINI("servers", "5", "", Data_Path);
                metroComboBox1.SelectedIndex = Default_Entry;
                metroTextBox1.Text = Server_1;
                metroTextBox2.Text = Server_2;
                metroTextBox3.Text = Server_3;
                metroTextBox4.Text = Server_4;
                metroTextBox5.Text = Server_5;
                Server_1 = Form1.GetOriginalUrl(Server_1);
                Server_2 = Form1.GetOriginalUrl(Server_2);
                Server_3 = Form1.GetOriginalUrl(Server_3);
                Server_4 = Form1.GetOriginalUrl(Server_4);
                Server_5 = Form1.GetOriginalUrl(Server_5);
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
                metroComboBox1.SelectedIndex = 0;
            }
        }
        public static string ReadINI(string section, string key, string def, string filePath)
        {
            StringBuilder sb = new StringBuilder(1024);
            GetPrivateProfileString(section, key, def, sb, 1024, filePath);
            return sb.ToString();
        }

        public void WriteINI(string section, string key, string value, string filePath)
        {
            if (Form1.IfHaveFile(filePath))
            {
                WritePrivateProfileString(section, key, value, filePath);
            }
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            LoadConfig();
        }

        private void CheckListPing()
        {
            label1.Text = "内置服务器当前延迟：" + Form1.GetPing(Form1.Default_ServerIP,1) + " ms";
            metroLabel8.Text = "PING：" + Form1.GetPing(Server_1, 1) + " ms";
            metroLabel9.Text = "PING：" + Form1.GetPing(Server_2, 1) + " ms";
            metroLabel10.Text = "PING：" + Form1.GetPing(Server_3, 1) + " ms";
            metroLabel11.Text = "PING：" + Form1.GetPing(Server_4, 1) + " ms";
            metroLabel12.Text = "PING：" + Form1.GetPing(Server_5, 1) + " ms";
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            CheckListPing();
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Dispose();
        }

        private void Form3_Shown(object sender, EventArgs e)
        {
            this.TopMost = true;
            LoadConfig();
            CheckListPing();
        }
    }
}