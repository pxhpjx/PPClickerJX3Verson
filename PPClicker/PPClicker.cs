using System;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;
using NAudio.CoreAudioApi;
using System.Linq;

namespace PPClicker
{
    public partial class PPClicker : Form
    {
        private static System.Timers.Timer timerClick = new System.Timers.Timer(500);
        private static System.Timers.Timer timerMove = new System.Timers.Timer(100);
        private static System.Timers.Timer timerPress = new System.Timers.Timer(100);
        private static System.Timers.Timer timerFarm = new System.Timers.Timer(1);
        private static System.Timers.Timer timerFarmWalker = new System.Timers.Timer(1200);
        private static System.Timers.Timer timerOpenBox = new System.Timers.Timer(200);
        private static System.Timers.Timer timerFishing = new System.Timers.Timer(100);
        private static System.Timers.Timer timerBreak = new System.Timers.Timer(100);
        private static System.Timers.Timer timerLightSword = new System.Timers.Timer(100);
        private static System.Timers.Timer timerReputation = new System.Timers.Timer(300);
        private static System.Timers.Timer timerDefent = new System.Timers.Timer(100);
        private static System.Timers.Timer timerMovePress = new System.Timers.Timer(10);

        int i = 0;
        int j = 0;
        int iF3;
        string VegetableIndex = "OFF";
        bool IllegalLabel = false;
        string PresserString;
        Point MousePoint = new Point();
        List<Point> RecordedPoints = new List<Point>();

        public PPClicker()
        {
            InitializeComponent();
            timerClick.Elapsed += new ElapsedEventHandler(TimerClick_Elapsed);
            timerMove.Elapsed += new ElapsedEventHandler(TimerMove_Elapsed);
            timerPress.Elapsed += new ElapsedEventHandler(TimerPress_Elapsed);
            timerFarm.Elapsed += new ElapsedEventHandler(TimerFarm_Elapsed);
            timerFarmWalker.Elapsed += new ElapsedEventHandler(TimerFarmWalker_Elapsed);
            timerOpenBox.Elapsed += new ElapsedEventHandler(TimerOpenBox_Elapsed);
            timerFishing.Elapsed += new ElapsedEventHandler(TimerFishing_Elapsed);
            timerBreak.Elapsed += new ElapsedEventHandler(TimerBreak_Elapsed);
            timerLightSword.Elapsed += new ElapsedEventHandler(TimerLightSword_Elapsed);
            timerReputation.Elapsed += new ElapsedEventHandler(TimerReputation_Elapsed);
            timerDefent.Elapsed += new ElapsedEventHandler(TimerDefent_Elapsed);
            timerMovePress.Elapsed += TimerMovePress_Elapsed;

            notifyIcon1.MouseClick += new MouseEventHandler(notifyIcon1_MouseClick);
            label1.Text = "当前点击间隔：" + (timerClick.Interval / 1000).ToString() + "秒";
            label4.Text = "当前晃动触发间隔：" + (timerMove.Interval / 1000).ToString() + "秒";
            lblCTimer.Text = "当前按键间隔：" + (timerPress.Interval / 1000).ToString() + "秒";
            Form_Activated(this, EventArgs.Empty);

        }

        void TimerMovePress_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (Point rp in RecordedPoints)
            {
                SetCursorPos(rp.X, rp.Y);
                mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
                Thread.Sleep(10);
                mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
            }
        }





        #region API

        [DllImport("user32.dll")]
        static extern void mouse_event(MouseEventFlag flags, int dx, int dy, int data, UIntPtr extraInfo);
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        [Flags]
        enum MouseEventFlag : uint
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            VirtualDesk = 0x4000,
            Absolute = 0x8000
        }
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);
        [DllImport("gdi32.dll")]
        public static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        public Color GetColor(int x, int y)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            uint pixel = GetPixel(hdc, x, y);
            ReleaseDC(IntPtr.Zero, hdc);
            Color color = Color.FromArgb((int)(pixel & 0x000000FF), (int)(pixel & 0x0000FF00) >> 8, (int)(pixel & 0x00FF0000) >> 16);
            return color;
        }

        #endregion



        #region BaseFunc

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    switch (m.WParam.ToInt32())
                    {
                        case 100:
                            btnControl_Click(this, EventArgs.Empty);
                            break;
                        case 101:
                            btnWaggle_Click(this, EventArgs.Empty);
                            break;
                        case 102:
                            btnKeyGo_Click(this, EventArgs.Empty);
                            break;
                        case 103:
                            VegetablesControl("H");
                            break;
                        case 104:
                            VegetablesControl("S");
                            break;
                        case 105:
                            OpenBoxCnotrol();
                            break;
                        case 106:
                            FishingControl();
                            break;
                        case 107:
                            IllegalLabelControl();
                            break;
                        case 108:
                            if (IllegalLabel)
                                BreakControl();
                            break;
                        case 109:
                            if (IllegalLabel)
                                LightSwordHitControl();
                            break;
                        case 110:
                            //Reputation();
                            break;
                        case 111:
                            DefensivePositionControl();
                            break;
                        //case 112:
                        //    RecordedPoints.Add(Control.MousePosition);
                        //    lblRecordCount.Text = RecordedPoints.Count.ToString();
                        //    break;
                        case 113:
                            MovePress();
                            break;
                        case 999:
                            Close();
                            break;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        private void Form_Activated(object sender, EventArgs e)
        {
            HotKey.RegisterHotKey(Handle, 100, HotKey.KeyModifiers.Ctrl | HotKey.KeyModifiers.Alt, Keys.C);
            HotKey.RegisterHotKey(Handle, 101, HotKey.KeyModifiers.Ctrl | HotKey.KeyModifiers.Alt, Keys.M);
            HotKey.RegisterHotKey(Handle, 102, HotKey.KeyModifiers.Ctrl | HotKey.KeyModifiers.Alt, Keys.P);
            HotKey.RegisterHotKey(Handle, 103, HotKey.KeyModifiers.Ctrl | HotKey.KeyModifiers.Alt, Keys.H);
            HotKey.RegisterHotKey(Handle, 104, HotKey.KeyModifiers.Ctrl | HotKey.KeyModifiers.Alt, Keys.S);
            HotKey.RegisterHotKey(Handle, 105, HotKey.KeyModifiers.Ctrl | HotKey.KeyModifiers.Alt, Keys.O);
            HotKey.RegisterHotKey(Handle, 106, HotKey.KeyModifiers.Ctrl | HotKey.KeyModifiers.Alt, Keys.F);
            HotKey.RegisterHotKey(Handle, 107, HotKey.KeyModifiers.Ctrl | HotKey.KeyModifiers.Alt, Keys.I);
            HotKey.RegisterHotKey(Handle, 110, HotKey.KeyModifiers.Ctrl | HotKey.KeyModifiers.Alt, Keys.R);
            HotKey.RegisterHotKey(Handle, 112, HotKey.KeyModifiers.Ctrl, Keys.R);
            HotKey.RegisterHotKey(Handle, 113, HotKey.KeyModifiers.Alt, Keys.R);

            HotKey.RegisterHotKey(Handle, 999, HotKey.KeyModifiers.Ctrl | HotKey.KeyModifiers.Alt, Keys.Q);
        }

        private void FrmSale_Leave(object sender, EventArgs e)
        {
            HotKey.UnregisterHotKey(Handle, 100);
            HotKey.UnregisterHotKey(Handle, 101);
            HotKey.UnregisterHotKey(Handle, 102);
            HotKey.UnregisterHotKey(Handle, 103);
            HotKey.UnregisterHotKey(Handle, 104);
            HotKey.UnregisterHotKey(Handle, 105);
            HotKey.UnregisterHotKey(Handle, 106);
            HotKey.UnregisterHotKey(Handle, 107);
            HotKey.UnregisterHotKey(Handle, 110);
            HotKey.UnregisterHotKey(Handle, 111);
            HotKey.UnregisterHotKey(Handle, 112);
        }

        void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (ShowInTaskbar == false)
                    ShowInTaskbar = true;
                else
                    ShowInTaskbar = false;
            }
            Form_Activated(this, EventArgs.Empty);
        }

        #endregion



        #region OnOccurDo

        //Clicker
        private void TimerClick_Elapsed(object source, ElapsedEventArgs e)
        {
            if (chkVol.Checked && !CheckVol())
            {
                return;
            }

            if (rbClickL.Checked)
            {
                mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
                Thread.Sleep(100);
                mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
            }
            if (rbClickR.Checked)
            {
                mouse_event(MouseEventFlag.RightDown, 0, 0, 0, UIntPtr.Zero);
                mouse_event(MouseEventFlag.RightUp, 0, 0, 0, UIntPtr.Zero);
            }
            if (rbClickU.Checked)
            {
                mouse_event(MouseEventFlag.Wheel, 0, 0, 120, UIntPtr.Zero);
            }
            if (rbClickD.Checked)
            {
                mouse_event(MouseEventFlag.Wheel, 0, 0, -120, UIntPtr.Zero);
            }
        }

        //Mover
        void TimerMove_Elapsed(object sender, EventArgs e)
        {
            i++;
            if (rblr.Checked)
            {
                switch (i % 2)
                {
                    case 0:
                        mouse_event(MouseEventFlag.Move, 200, 0, 0, UIntPtr.Zero);
                        break;
                    case 1:
                        mouse_event(MouseEventFlag.Move, -200, 0, 0, UIntPtr.Zero);
                        break;
                }
            }
            else if (rbud.Checked)
            {
                switch (i % 2)
                {
                    case 0:
                        mouse_event(MouseEventFlag.Move, 0, 100, 0, UIntPtr.Zero);
                        break;
                    case 1:
                        mouse_event(MouseEventFlag.Move, 0, -100, 0, UIntPtr.Zero);
                        break;
                }
            }
            else if (rbround.Checked)
            {
                switch (i % 4)
                {
                    case 0:
                        mouse_event(MouseEventFlag.Move, 50, 50, 0, UIntPtr.Zero);
                        break;
                    case 1:
                        mouse_event(MouseEventFlag.Move, 50, -50, 0, UIntPtr.Zero);
                        break;
                    case 2:
                        mouse_event(MouseEventFlag.Move, -50, -50, 0, UIntPtr.Zero);
                        break;
                    case 3:
                        mouse_event(MouseEventFlag.Move, -50, 50, 0, UIntPtr.Zero);
                        break;
                }
            }
            else if (rbrs.Checked)
            {
                Random r = new Random();
                switch (i % 4)
                {
                    case 0:
                        mouse_event(MouseEventFlag.Move, r.Next(20), r.Next(20), 0, UIntPtr.Zero);
                        break;
                    case 1:
                        mouse_event(MouseEventFlag.Move, r.Next(20), -r.Next(20), 0, UIntPtr.Zero);
                        break;
                    case 2:
                        mouse_event(MouseEventFlag.Move, -r.Next(20), -r.Next(20), 0, UIntPtr.Zero);
                        break;
                    case 3:
                        mouse_event(MouseEventFlag.Move, -r.Next(20), r.Next(20), 0, UIntPtr.Zero);
                        break;
                }
            }
            else if (rbrb.Checked)
            {
                Random r = new Random();
                switch (i % 4)
                {
                    case 0:
                        mouse_event(MouseEventFlag.Move, r.Next(200), r.Next(200), 0, UIntPtr.Zero);
                        break;
                    case 1:
                        mouse_event(MouseEventFlag.Move, r.Next(200), -r.Next(200), 0, UIntPtr.Zero);
                        break;
                    case 2:
                        mouse_event(MouseEventFlag.Move, -r.Next(200), -r.Next(200), 0, UIntPtr.Zero);
                        break;
                    case 3:
                        mouse_event(MouseEventFlag.Move, -r.Next(200), r.Next(200), 0, UIntPtr.Zero);
                        break;
                }
            }
            else
            { }
        }

        //Presser
        void TimerPress_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (chkString.Checked)
            {
                if (rbHold.Checked)
                {
                    foreach (char c in PresserString)
                        keybd_event((byte)c, 0, 0, 0);
                }
                else
                {
                    foreach (char c in PresserString)
                    {
                        keybd_event((byte)c, 0, 0, 0);
                        keybd_event((byte)c, 0, 2, 0);
                    }
                }
            }
            else
            {
                if (rbHold.Checked)
                {
                    char c = PresserString[0];
                    keybd_event((byte)c, 0, 0, 0);
                }
                else
                {
                    char c = PresserString[0];
                    keybd_event((byte)c, 0, 0, 0);
                    keybd_event((byte)c, 0, 2, 0);
                }
            }
        }

        //FarmerAction
        void TimerFarm_Elapsed(object sender, ElapsedEventArgs e)
        {
            int X;
            if (VegetableIndex == "H")
                X = 1050;
            else
                X = 860;
            if (timerFarm.Interval != 3500)
                timerFarm.Interval = 3500;
            if (i < 24)
            {
                switch (i % 5)
                {
                    case 0:
                        mouse_event(MouseEventFlag.Move | MouseEventFlag.Absolute, 1220 * 65536 / 1920, 630 * 65536 / 1080, 0, UIntPtr.Zero);
                        mouse_event(MouseEventFlag.RightDown, 0, 0, 0, UIntPtr.Zero);
                        mouse_event(MouseEventFlag.RightUp, 0, 0, 0, UIntPtr.Zero);
                        timerFarm.Interval = 500;
                        break;
                    case 1:
                        mouse_event(MouseEventFlag.Move | MouseEventFlag.Absolute, X * 65536 / 1920, 550 * 65536 / 1080, 0, UIntPtr.Zero);
                        mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
                        mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
                        break;
                    case 2:
                        mouse_event(MouseEventFlag.Move | MouseEventFlag.Absolute, 700 * 65536 / 1920, 630 * 65536 / 1080, 0, UIntPtr.Zero);
                        mouse_event(MouseEventFlag.RightDown, 0, 0, 0, UIntPtr.Zero);
                        mouse_event(MouseEventFlag.RightUp, 0, 0, 0, UIntPtr.Zero);
                        timerFarm.Interval = 500;
                        break;
                    case 3:
                        mouse_event(MouseEventFlag.Move | MouseEventFlag.Absolute, X * 65536 / 1920, 550 * 65536 / 1080, 0, UIntPtr.Zero);
                        mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
                        mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
                        break;
                    case 4:
                        timerFarm.Enabled = false;
                        timerFarm.Interval = 1;
                        timerFarmWalker.Interval = 1;
                        j = 0;
                        timerFarmWalker.Enabled = true;
                        break;
                }
                i++;
            }
            else
            {
                VegetablesControl("");
            }
        }

        //FarmerWalker
        void TimerFarmWalker_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (j == 0)
            {
                keybd_event((byte)'W', 0, 0, 0);
                timerFarmWalker.Interval = 1200;
                j++;
            }
            else
            {
                keybd_event((byte)'W', 0, 2, 0);
                timerFarmWalker.Enabled = false;
                timerFarm.Enabled = true;
            }
        }

        //OpenBox
        void TimerOpenBox_Elapsed(object sender, ElapsedEventArgs e)
        {
            i++;
            int MoveY;
            if (radStone.Checked)
                MoveY = 80;
            else
                MoveY = 120;
            switch (i % 3)
            {
                case 0:
                    mouse_event(MouseEventFlag.RightDown, 0, 0, 0, UIntPtr.Zero);
                    mouse_event(MouseEventFlag.RightUp, 0, 0, 0, UIntPtr.Zero);
                    SetCursorPos(MousePoint.X - 110, MousePoint.Y - MoveY);
                    break;
                case 1:
                    mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
                    mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
                    SetCursorPos(MousePoint.X - 110, MousePoint.Y - 40);
                    break;
                case 2:
                    mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
                    mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
                    SetCursorPos(MousePoint.X, MousePoint.Y);
                    break;
            }
        }

        //Fishing
        void TimerFishing_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (i < 1000)
            {
                SetCursorPos(910, 545);
                mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
                mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
                i += 1000;
            }
            else if (i < 2000)
            {
                Color CheckPonit = GetColor(892, 793);
                if (CheckPonit.R > 100 && CheckPonit.G > 100 && CheckPonit.B > 100)
                {
                    SetCursorPos(1010, 545);
                    mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
                    mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
                    i += 1000;
                }
            }
            else
            {
                if (i > 2040)
                    i = 0;
                else
                    i++;
            }
        }

        //Break
        void TimerBreak_Elapsed(object sender, ElapsedEventArgs e)
        {
            Color Self = GetColor(1083, 796);
            Color Enemy = GetColor(682, 90);
            if (Enemy.R < 10 && Enemy.G < 10 && Enemy.B < 10)
            {
                if (Self.R > 35 && Self.G > 35 && Self.B > 35)
                {
                    keybd_event((byte)'3', 0, 0, 0);
                    keybd_event((byte)'3', 0, 2, 0);
                }
            }
        }

        //LightSwordHit
        void TimerLightSword_Elapsed(object sender, ElapsedEventArgs e)
        {
            Color LightSword = GetColor(63, 94);
            if (LightSword.R > 160 && LightSword.G > 160)
            {
                if (chkF2HP.Checked)
                {
                    keybd_event((byte)'5', 0, 0, 0);
                    keybd_event((byte)'5', 0, 2, 0);
                }
                keybd_event((byte)'R', 0, 0, 0);
                keybd_event((byte)'R', 0, 2, 0);
                keybd_event((byte)'E', 0, 0, 0);
                keybd_event((byte)'E', 0, 2, 0);
                keybd_event((byte)'4', 0, 0, 0);
                keybd_event((byte)'4', 0, 2, 0);
                keybd_event((byte)'2', 0, 0, 0);
                keybd_event((byte)'2', 0, 2, 0);
                keybd_event((byte)'F', 0, 0, 0);
                keybd_event((byte)'F', 0, 2, 0);
                keybd_event((byte)'8', 0, 0, 0);
                keybd_event((byte)'8', 0, 2, 0);
            }
            else if (chkF2ZJ.Checked)
            {
                keybd_event((byte)'R', 0, 0, 0);
                keybd_event((byte)'R', 0, 2, 0);
                keybd_event((byte)'E', 0, 0, 0);
                keybd_event((byte)'E', 0, 2, 0);
            }
        }

        //DefensivePositionControl
        void TimerDefent_Elapsed(object sender, ElapsedEventArgs e)
        {
            iF3++;
            if (iF3 == 3)
            {
                keybd_event((byte)'Q', 0, 0, 0);
                keybd_event((byte)'Q', 0, 2, 0);
            }
            if (iF3 > 5)
            {
                if (radF3ZY.Checked)
                {
                    if (iF3 < 15)
                    {
                        keybd_event((byte)'1', 0, 0, 0);
                        keybd_event((byte)'1', 0, 2, 0);
                    }
                    else
                    {
                        timerDefent.Enabled = false;
                    }
                }
                else
                {
                    keybd_event((byte)'9', 0, 0, 0);
                    keybd_event((byte)'9', 0, 2, 0);
                    timerDefent.Enabled = false;
                }
            }
        }

        //Reputation
        void TimerReputation_Elapsed(object sender, ElapsedEventArgs e)
        {
            switch (i % 5)
            {
                case 0:
                    mouse_event(MouseEventFlag.RightDown, 0, 0, 0, UIntPtr.Zero);
                    mouse_event(MouseEventFlag.RightUp, 0, 0, 0, UIntPtr.Zero);
                    SetCursorPos(395, 215);
                    break;
                case 1:
                    mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
                    mouse_event(MouseEventFlag.Move, 0, 1000, 0, UIntPtr.Zero);
                    mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
                    break;
                case 2:
                    SetCursorPos(150, 435 + j * 30);
                    mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
                    mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
                    SetCursorPos(MousePoint.X, MousePoint.Y);
                    break;
            }
            i++;
        }


        bool CheckVol()
        {
            return GetVoicePeakValue() > Double.Parse(txtVol.Text);
        }

        public float GetVoicePeakValue()
        {
            var enumerator = new MMDeviceEnumerator();
            var CaptureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToArray();
            var selectedDevice = CaptureDevices.FirstOrDefault(c => c.AudioMeterInformation.MasterPeakValue > 0);


            //var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
            //var selectedDevice = CaptureDevices.FirstOrDefault(c => c.ID == defaultDevice.ID);

            return selectedDevice == null ? 0 : selectedDevice.AudioMeterInformation.MasterPeakValue;
        }


        #endregion



        #region TimeSetter

        private void btnSet_Click(object sender, EventArgs e)
        {
            try
            {
                timerClick.Interval = (int)(double.Parse(txtTimer.Text) * 1000);
                label1.Text = "当前点击间隔：" + (timerClick.Interval / 1000).ToString() + "秒";
            }
            catch
            {
            }
        }

        private void btnbSetter_Click(object sender, EventArgs e)
        {
            try
            {
                timerMove.Interval = (int)(double.Parse(txtbSetter.Text) * 1000);
                label4.Text = "当前晃动触发间隔：" + (timerMove.Interval / 1000).ToString() + "秒";
            }
            catch
            {
            }
        }

        private void btnCSetter_Click(object sender, EventArgs e)
        {
            try
            {
                timerPress.Interval = (int)(double.Parse(txtCSetter.Text) * 1000);
                lblCTimer.Text = "当前按键间隔：" + (timerPress.Interval / 1000).ToString() + "秒";
            }
            catch
            {
            }
        }

        #endregion



        #region ActionControl

        private void btnControl_Click(object sender, EventArgs e)
        {
            if (timerClick.Enabled)
            {
                timerClick.Enabled = false;
                label2.Text = "已停止";
                btnControl.Text = "GO!";
                notifyIcon1.Icon = notifyIconOFF.Icon;
            }
            else
            {
                timerClick.Enabled = true;
                label2.Text = "已启动";
                btnControl.Text = "Stop";
                notifyIcon1.Icon = notifyIconON.Icon;
            }
        }

        private void btnWaggle_Click(object sender, EventArgs e)
        {
            if (timerMove.Enabled)
            {
                timerMove.Enabled = false;
                label3.Text = "已停止";
                btnWaggle.Text = "GO!";
                if (cbLeft.Checked)
                    mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
                notifyIcon1.Icon = notifyIconOFF.Icon;
            }
            else
            {
                timerMove.Enabled = true;
                label3.Text = "已启动";
                btnWaggle.Text = "Stop";
                if (cbLeft.Checked)
                    mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
                notifyIcon1.Icon = notifyIconON.Icon;
            }
        }

        private void btnKeyGo_Click(object sender, EventArgs e)
        {
            if (timerPress.Enabled)
            {
                timerPress.Enabled = false;
                lbl3S.Text = "已停止";
                btnKeyGo.Text = "GO!";
                notifyIcon1.Icon = notifyIconOFF.Icon;
            }
            else
            {
                PresserString = txtKey.Text.ToUpper();
                timerPress.Enabled = true;
                lbl3S.Text = "已启动";
                btnKeyGo.Text = "Stop";
                notifyIcon1.Icon = notifyIconON.Icon;
            }
        }

        private void VegetablesControl(string Index)
        {
            if (VegetableIndex != "OFF")
            {
                VegetableIndex = "OFF";
                lbl4H.Text = "已停止";
                lbl4S.Text = "已停止";
                timerFarm.Enabled = false;
                notifyIcon1.Icon = notifyIconOFF.Icon;
            }
            else
            {
                VegetableIndex = Index;
                if (Index == "H")
                    lbl4H.Text = "已开始";
                else
                    lbl4S.Text = "已开始";
                i = 0;
                j = 0;
                timerFarmWalker.Enabled = false;
                timerFarm.Interval = 1;
                timerFarm.Enabled = true;
                notifyIcon1.Icon = notifyIconON.Icon;
            }
        }

        private void OpenBoxCnotrol()
        {
            i = -1;
            if (timerOpenBox.Enabled)
            {
                timerOpenBox.Enabled = false;
                notifyIcon1.Icon = notifyIconOFF.Icon;
            }
            else
            {
                MousePoint = Control.MousePosition;
                timerOpenBox.Enabled = true;
                notifyIcon1.Icon = notifyIconON.Icon;
            }
        }

        private void FishingControl()
        {
            if (timerFishing.Enabled)
            {
                timerFishing.Enabled = false;
                notifyIcon1.Icon = notifyIconOFF.Icon;
            }
            else
            {
                i = 0;
                timerFishing.Enabled = true;
                notifyIcon1.Icon = notifyIconON.Icon;
            }
        }

        private void IllegalLabelControl()
        {
            IllegalLabel = !IllegalLabel;
            if (!IllegalLabel)
            {
                lblIllegalLabel.ForeColor = Color.Blue;
                timerBreak.Enabled = false;
                timerLightSword.Enabled = false;
                HotKey.UnregisterHotKey(Handle, 108);
                HotKey.UnregisterHotKey(Handle, 109);
                HotKey.UnregisterHotKey(Handle, 111);
            }
            else
            {
                lblIllegalLabel.ForeColor = Color.Red;
                HotKey.RegisterHotKey(Handle, 108, HotKey.KeyModifiers.None, Keys.F1);
                HotKey.RegisterHotKey(Handle, 109, HotKey.KeyModifiers.None, Keys.F2);
                HotKey.RegisterHotKey(Handle, 111, HotKey.KeyModifiers.None, Keys.F4);
            }
        }

        private void BreakControl()
        {
            if (timerBreak.Enabled)
            {
                timerBreak.Enabled = false;
                notifyIcon1.Icon = notifyIconOFF.Icon;
            }
            else
            {
                i = 0;
                timerBreak.Enabled = true;
                notifyIcon1.Icon = notifyIconON.Icon;
            }
        }

        private void LightSwordHitControl()
        {
            if (timerLightSword.Enabled)
            {
                timerLightSword.Enabled = false;
                notifyIcon1.Icon = notifyIconOFF.Icon;
            }
            else
            {
                i = 0;
                timerLightSword.Enabled = true;
                notifyIcon1.Icon = notifyIconON.Icon;
            }
        }

        private void DefensivePositionControl()
        {
            iF3 = 0;
            timerDefent.Enabled = true;
        }

        private void Reputation()
        {
            i = 0;
            if (timerReputation.Enabled)
            {
                timerReputation.Enabled = false;
                notifyIcon1.Icon = notifyIconOFF.Icon;
            }
            else
            {
                if (radRBHJ.Checked)
                    j = 0;
                else if (radRTNWJ.Checked)
                    j = 1;
                else if (radRXYS.Checked)
                    j = 3;
                else
                    j = -1;
                if (j >= 0)
                {
                    MousePoint = Control.MousePosition;
                    timerReputation.Enabled = true;
                    notifyIcon1.Icon = notifyIconON.Icon;
                }
            }

        }

        private void MovePress()
        {
            if (timerMovePress.Enabled)
            {
                timerMovePress.Enabled = false;
                notifyIcon1.Icon = notifyIconOFF.Icon;
            }
            else
            {
                timerMovePress.Enabled = true;
                notifyIcon1.Icon = notifyIconON.Icon;
            }
        }

        #endregion

    }
}
