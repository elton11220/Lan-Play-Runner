using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using MetroFramework.Forms;
using MetroFramework;

namespace Lan_Play_Runner
{
    public partial class Form2 : MetroForm
    {
        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            metroLabel2.Text = "服务器在线人数：" + Form1.GetInfo("online");
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "确定要退出吗", "退出", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                PostMessage(Form1.hwnd, 0x10, 0, 0);
                this.FormClosing -= new FormClosingEventHandler(this.Form2_FormClosing);
                System.Environment.Exit(0);
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show(this, "确定要退出吗", "退出", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                PostMessage(Form1.hwnd, 0x10, 0, 0);
                this.FormClosing -= new FormClosingEventHandler(this.Form2_FormClosing);
                System.Environment.Exit(0);
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            metroLabel2.Text = "服务器在线人数：" + Form1.GetInfo("online");
            if (Form1.Default_Entry == 0)
            {
                metroLabel3.Text = "当前服务器：内置服务器";
            }
            else
            {
                metroLabel3.Text = "当前服务器：" + Form1.Server_IP;
            }
            Form1 form = new Form1();
            form.Dispose();
        }
    }
}
