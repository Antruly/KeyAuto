using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;



namespace KeyAuto
{
    public partial class Mainfrm : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        delegate void NumberDelegate(int nb);
        delegate void VoidType(object sender, EventArgs e);

        

        /// <summary>
        /// 按下
        /// </summary>
        const uint KEYEVENTF_EXTENDEDKEY = 0x1;
        /// <summary>
        /// 抬起
        /// </summary>
        const uint KEYEVENTF_KEYUP = 0x2;
        const byte VK_CONTROL = 0x11;
        const byte VK_C = 0x43;


        Thread _workThread = null;

        bool _isStop = false;

        // 将单个字符转换为对应的VK码  
        static byte CharToVkCode(char c)
        {
            if (c >= 'A' && c <= 'Z')
            {
                return (byte)(0x41 + (c - 'A')); // A的VK码是0x41，之后依次加1  
            }
            else if (c >= 'a' && c <= 'z')
            {
                // 小写字母的VK码与大写字母相同，因此我们可以直接使用大写字母的逻辑  
                return (byte)(0x41 + (c - 'a')); // 或者使用(c - 'A' + 0x41) & 0xFF来确保结果是byte类型  
            }
            else if (c >= '0' && c <= '9')
            {
                return (byte)(0x30 + (c - '0')); // 0的VK码是0x30，之后依次加1  
            }
            else
            {
                return 0;
            }
        }

        private void test()
        {

            // 模拟按下 Ctrl 键  
            keybd_event(VK_CONTROL, 0, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);

            // 模拟按下 C 键  
            keybd_event(VK_C, 0, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);

            // 模拟松开 C 键  
            keybd_event(VK_C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

            // 模拟松开 Ctrl 键  
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        public Mainfrm()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_workThread != null)
            {
                return;
            }
            _isStop = false;
            txtKey.Enabled = false;
            txtTime.Enabled = false;
            txtNumber.Enabled = false;

            _workThread = new Thread(runKey);

            _workThread.IsBackground = true;
            _workThread.Start();
        }

        private void runKey()
        {


            string strKey = txtKey.Text;
            int nTime = Int32.Parse(txtTime.Text);
            int nNumber = Int32.Parse(txtNumber.Text);

            byte bKey = CharToVkCode(strKey[0]);

            if (bKey == 0) return;


            while (!_isStop)
            {
                
                // 模拟按下
                keybd_event(bKey, 0, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
                Random rd = new Random(DateTime.Now.Second);
                Thread.Sleep(100 + rd.Next(200));
                // 模拟松开
                keybd_event(bKey, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                nNumber--;

                NumberDelegate dlg = new NumberDelegate(updateNumber);
                this.Invoke(dlg, nNumber);
               
                if (nNumber <= 0)
                {
                    nNumber = 0;
                    VoidType vd = new VoidType(btnStop_Click);
                    this.Invoke(vd, null, null);
                    return;
                }
                
                int unRunTime = nTime;
                int runTime = 0;
                while (unRunTime > 0 && !_isStop)
                {

                    if(unRunTime > 100){
                        unRunTime -= 100;
                        runTime = 100;
                    }
                    else
                    {
                        unRunTime = 0;
                        runTime = unRunTime;
                    }
                    Thread.Sleep(runTime);
                }
               
            }

        }

        private void updateNumber(int nb)
        {
            txtNumber.Text = nb.ToString();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _isStop = true;

            if (sender != null)
            _workThread.Join();

            _workThread = null;
            txtKey.Enabled = true;
            txtTime.Enabled = true;
            txtNumber.Enabled = true;
        }

    }
}
