using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace JoyConCustomGUI
{
    public class PresetItem
    {
        public string Label { get; set; }
        public string Code { get; set; }
        public PresetItem(string label, string code)
        {
            Label = label;
            Code = code;
        }
        public override string ToString()
        {
            return string.Format("{0} [{1}]", Label, Code);
        }
    }

    public class JoyConHitRegion
    {
        public string Key;
        public RectangleF Bounds;
        public string Shape;
        public string Text;
    }

    public class DualJoyConCanvas : Control
    {
        public string SelectedKey { get; set; }
        public string HoveredKey { get; set; }
        public event Action<string> ButtonClicked;

        private List<JoyConHitRegion> regions = new List<JoyConHitRegion>();
        private static readonly Font FontLabel = new Font("Malgun Gothic", 9.5f, FontStyle.Bold);
        private static readonly Font FontStick = new Font("Malgun Gothic", 8.5f, FontStyle.Bold);
        private static readonly Font FontBtn = new Font("Malgun Gothic", 8f, FontStyle.Bold);
        private static readonly Font FontHome = new Font("Malgun Gothic", 9f, FontStyle.Bold);

        public DualJoyConCanvas()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.Cursor = Cursors.Hand;
            SelectedKey = "ZR";
            BuildRegions();
        }

        private void BuildRegions()
        {
            regions.Clear();
            float lx = 42f, ly = 26f;
            float rx = 190f, ry = 26f;

            regions.Add(new JoyConHitRegion { Key = "ZL", Bounds = new RectangleF(lx + 14, ly - 15, 46, 15), Shape = "zl", Text = "ZL" });
            regions.Add(new JoyConHitRegion { Key = "L", Bounds = new RectangleF(lx + 4, ly - 4, 66, 18), Shape = "shoulderL", Text = "L" });
            regions.Add(new JoyConHitRegion { Key = "LSL", Bounds = new RectangleF(lx + 108, ly + 32, 10, 30), Shape = "rail", Text = "" });
            regions.Add(new JoyConHitRegion { Key = "LSR", Bounds = new RectangleF(lx + 108, ly + 238, 10, 30), Shape = "rail", Text = "" });

            regions.Add(new JoyConHitRegion { Key = "-", Bounds = new RectangleF(lx + 76, ly + 26, 16, 7), Shape = "minus", Text = "" });
            regions.Add(new JoyConHitRegion { Key = "L3", Bounds = new RectangleF(lx + 28, ly + 58, 46, 46), Shape = "stick", Text = "L3" });
            regions.Add(new JoyConHitRegion { Key = "UP", Bounds = new RectangleF(lx + 41, ly + 136, 21, 21), Shape = "round", Text = "▲" });
            regions.Add(new JoyConHitRegion { Key = "DOWN", Bounds = new RectangleF(lx + 41, ly + 178, 21, 21), Shape = "round", Text = "▼" });
            regions.Add(new JoyConHitRegion { Key = "LEFT", Bounds = new RectangleF(lx + 20, ly + 157, 21, 21), Shape = "round", Text = "◀" });
            regions.Add(new JoyConHitRegion { Key = "RIGHT", Bounds = new RectangleF(lx + 62, ly + 157, 21, 21), Shape = "round", Text = "▶" });
            regions.Add(new JoyConHitRegion { Key = "CAPTURE", Bounds = new RectangleF(lx + 70, ly + 220, 18, 18), Shape = "capture", Text = "" });

            regions.Add(new JoyConHitRegion { Key = "ZR", Bounds = new RectangleF(rx + 48, ry - 15, 46, 15), Shape = "zr", Text = "ZR" });
            regions.Add(new JoyConHitRegion { Key = "R", Bounds = new RectangleF(rx + 38, ry - 4, 66, 18), Shape = "shoulderR", Text = "R" });
            regions.Add(new JoyConHitRegion { Key = "RSR", Bounds = new RectangleF(rx - 10, ry + 32, 10, 30), Shape = "rail", Text = "" });
            regions.Add(new JoyConHitRegion { Key = "RSL", Bounds = new RectangleF(rx - 10, ry + 238, 10, 30), Shape = "rail", Text = "" });

            regions.Add(new JoyConHitRegion { Key = "+", Bounds = new RectangleF(rx + 15, ry + 21, 16, 16), Shape = "plus", Text = "" });
            regions.Add(new JoyConHitRegion { Key = "N", Bounds = new RectangleF(rx + 45, ry + 48, 21, 21), Shape = "round", Text = "X" });
            regions.Add(new JoyConHitRegion { Key = "S", Bounds = new RectangleF(rx + 45, ry + 90, 21, 21), Shape = "round", Text = "B" });
            regions.Add(new JoyConHitRegion { Key = "W", Bounds = new RectangleF(rx + 24, ry + 69, 21, 21), Shape = "round", Text = "Y" });
            regions.Add(new JoyConHitRegion { Key = "E", Bounds = new RectangleF(rx + 66, ry + 69, 21, 21), Shape = "round", Text = "A" });
            regions.Add(new JoyConHitRegion { Key = "R3", Bounds = new RectangleF(rx + 32, ry + 146, 46, 46), Shape = "stick", Text = "R3" });
            regions.Add(new JoyConHitRegion { Key = "HOME", Bounds = new RectangleF(rx + 18, ry + 218, 20, 20), Shape = "home", Text = "⌂" });
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            string hit = null;
            for (int i = regions.Count - 1; i >= 0; i--)
            {
                RectangleF inflated = RectangleF.Inflate(regions[i].Bounds, 2, 2);
                if (inflated.Contains(e.Location))
                {
                    hit = regions[i].Key;
                    break;
                }
            }
            if (hit != HoveredKey)
            {
                HoveredKey = hit;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (HoveredKey != null)
            {
                HoveredKey = null;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            for (int i = regions.Count - 1; i >= 0; i--)
            {
                RectangleF inflated = RectangleF.Inflate(regions[i].Bounds, 2, 2);
                if (inflated.Contains(e.Location))
                {
                    if (ButtonClicked != null) ButtonClicked(regions[i].Key);
                    break;
                }
            }
        }

        private GraphicsPath CreateJoyConBodyPath(float x, float y, float w, float h, bool isLeft)
        {
            GraphicsPath gp = new GraphicsPath();
            float rOuter = 68f;
            float rInner = 16f;
            if (isLeft)
            {
                gp.AddArc(x, y, rOuter, rOuter, 180, 90);
                gp.AddArc(x + w - rInner, y, rInner, rInner, 270, 90);
                gp.AddArc(x + w - rInner, y + h - rInner, rInner, rInner, 0, 90);
                gp.AddArc(x, y + h - rOuter, rOuter, rOuter, 90, 90);
            }
            else
            {
                gp.AddArc(x, y, rInner, rInner, 180, 90);
                gp.AddArc(x + w - rOuter, y, rOuter, rOuter, 270, 90);
                gp.AddArc(x + w - rOuter, y + h - rOuter, rOuter, rOuter, 0, 90);
                gp.AddArc(x, y + h - rInner, rInner, rInner, 90, 90);
            }
            gp.CloseFigure();
            return gp;
        }

        private GraphicsPath RoundedRect(RectangleF b, float r)
        {
            GraphicsPath gp = new GraphicsPath();
            float d = r * 2f;
            gp.AddArc(b.X, b.Y, d, d, 180, 90);
            gp.AddArc(b.Right - d, b.Y, d, d, 270, 90);
            gp.AddArc(b.Right - d, b.Bottom - d, d, d, 0, 90);
            gp.AddArc(b.X, b.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.FromArgb(15, 23, 42));

            float lx = 42f, ly = 26f, w = 108f, h = 300f;
            float rx = 190f, ry = 26f;

            foreach (var reg in regions)
            {
                if (reg.Shape == "zl" || reg.Shape == "zr" || reg.Shape == "shoulderL" || reg.Shape == "shoulderR" || reg.Shape == "rail")
                {
                    DrawRegion(g, reg);
                }
            }

            using (Brush railBrush = new SolidBrush(Color.FromArgb(23, 25, 27)))
            using (Pen railPen = new Pen(Color.FromArgb(15, 17, 19), 1f))
            {
                g.FillRectangle(railBrush, lx + w - 1, ly + 14, 9, h - 28);
                g.DrawRectangle(railPen, lx + w - 1, ly + 14, 9, h - 28);
                g.FillRectangle(railBrush, rx - 8, ry + 14, 9, h - 28);
                g.DrawRectangle(railPen, rx - 8, ry + 14, 9, h - 28);
            }

            using (Brush ledOn = new SolidBrush(Color.FromArgb(74, 222, 128)))
            using (Brush ledOff = new SolidBrush(Color.FromArgb(100, 116, 139)))
            using (Brush syncBr = new SolidBrush(Color.FromArgb(37, 40, 43)))
            {
                for (int i = 0; i < 4; i++)
                {
                    g.FillRectangle(i == 0 ? ledOn : ledOff, lx + w + 2, ly + 130 + i * 6, 3, 3);
                    g.FillRectangle(i == 0 ? ledOn : ledOff, rx - 5, ry + 130 + i * 6, 3, 3);
                }
                g.FillEllipse(syncBr, lx + w + 1, ly + 160, 5, 5);
                g.FillEllipse(syncBr, rx - 6, ry + 160, 5, 5);
            }

            using (GraphicsPath gpL = CreateJoyConBodyPath(lx, ly, w, h, true))
            using (GraphicsPath gpR = CreateJoyConBodyPath(rx, ry, w, h, false))
            using (Brush bodyBrush = new SolidBrush(Color.FromArgb(85, 89, 93)))
            using (Pen bodyBorder = new Pen(Color.FromArgb(32, 35, 38), 2f))
            {
                g.FillPath(bodyBrush, gpL);
                g.DrawPath(bodyBorder, gpL);
                g.FillPath(bodyBrush, gpR);
                g.DrawPath(bodyBorder, gpR);
            }

            foreach (var reg in regions)
            {
                if (reg.Shape != "zl" && reg.Shape != "zr" && reg.Shape != "shoulderL" && reg.Shape != "shoulderR" && reg.Shape != "rail")
                {
                    DrawRegion(g, reg);
                }
            }

            using (Brush bLabel = new SolidBrush(Color.FromArgb(148, 163, 184)))
            {
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("Joy-Con (L)", FontLabel, bLabel, lx + w / 2f, ly + h + 8, sf);
                g.DrawString("Joy-Con (R)", FontLabel, bLabel, rx + w / 2f, ry + h + 8, sf);
            }
        }

        private void DrawRegion(Graphics g, JoyConHitRegion reg)
        {
            bool active = string.Equals(reg.Key, SelectedKey, StringComparison.OrdinalIgnoreCase);
            bool hover = string.Equals(reg.Key, HoveredKey, StringComparison.OrdinalIgnoreCase);
            bool hi = active || hover;

            Color fillCol = hi ? Color.FromArgb(230, 0, 18) : Color.FromArgb(32, 35, 38);
            Color borderCol = hi ? Color.White : Color.FromArgb(16, 18, 20);
            float borderW = hi ? 2f : 1.2f;

            RectangleF b = reg.Bounds;
            if (hi && reg.Shape != "plus")
            {
                b = RectangleF.Inflate(b, 1.5f, 1.5f);
            }

            Font fTxt = reg.Shape == "stick" ? FontStick : FontBtn;

            using (Brush fill = new SolidBrush(fillCol))
            using (Pen pen = new Pen(borderCol, borderW))
            using (Brush txtBr = new SolidBrush(hi ? Color.White : Color.FromArgb(226, 232, 240)))
            {
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                if (reg.Shape == "round")
                {
                    g.FillEllipse(fill, b);
                    g.DrawEllipse(pen, b);
                    g.DrawString(reg.Text, fTxt, txtBr, b, sf);
                }
                else if (reg.Shape == "stick")
                {
                    using (Brush stickFill = new SolidBrush(hi ? Color.FromArgb(230, 0, 18) : Color.FromArgb(25, 28, 31)))
                    using (Pen ringPen = new Pen(hi ? Color.White : Color.FromArgb(57, 60, 63), 2f))
                    {
                        g.FillEllipse(stickFill, b);
                        g.DrawEllipse(ringPen, b);
                        RectangleF inner = RectangleF.Inflate(b, -5, -5);
                        using (Pen innerPen = new Pen(hi ? Color.FromArgb(255, 200, 200) : Color.FromArgb(56, 59, 62), 1f))
                        {
                            g.DrawEllipse(innerPen, inner);
                        }
                        g.DrawString(reg.Text, fTxt, txtBr, b, sf);
                    }
                }
                else if (reg.Shape == "minus")
                {
                    using (GraphicsPath gp = RoundedRect(b, 1.5f))
                    {
                        g.FillPath(fill, gp);
                        g.DrawPath(pen, gp);
                    }
                }
                else if (reg.Shape == "plus")
                {
                    RectangleF hBar = new RectangleF(b.X, b.Y + 5f, b.Width, 6f);
                    RectangleF vBar = new RectangleF(b.X + 5f, b.Y, 6f, b.Height);
                    g.FillRectangle(fill, hBar);
                    g.FillRectangle(fill, vBar);
                    g.DrawRectangle(pen, hBar.X, hBar.Y, hBar.Width, hBar.Height);
                    g.DrawRectangle(pen, vBar.X, vBar.Y, vBar.Width, vBar.Height);
                }
                else if (reg.Shape == "capture")
                {
                    using (GraphicsPath gp = RoundedRect(b, 3f))
                    {
                        g.FillPath(fill, gp);
                        g.DrawPath(pen, gp);
                    }
                    RectangleF inner = RectangleF.Inflate(b, -4.5f, -4.5f);
                    using (Brush inBr = new SolidBrush(hi ? Color.FromArgb(180, 0, 12) : Color.FromArgb(23, 25, 27)))
                    using (Pen inPen = new Pen(hi ? Color.White : Color.FromArgb(45, 49, 53), 1f))
                    {
                        g.FillEllipse(inBr, inner);
                        g.DrawEllipse(inPen, inner);
                    }
                }
                else if (reg.Shape == "home")
                {
                    g.FillEllipse(fill, b);
                    using (Pen homeRing = new Pen(hi ? Color.White : Color.FromArgb(140, 145, 150), 2f))
                    {
                        g.DrawEllipse(homeRing, b);
                    }
                    g.DrawString("⌂", FontHome, txtBr, b, sf);
                }
                else
                {
                    using (GraphicsPath gp = RoundedRect(b, 5f))
                    {
                        g.FillPath(fill, gp);
                        g.DrawPath(pen, gp);
                        if (!string.IsNullOrEmpty(reg.Text))
                        {
                            g.DrawString(reg.Text, fTxt, txtBr, b, sf);
                        }
                    }
                }
            }
        }
    }

    public class MainForm : Form
    {
        private static readonly Color BgDark = Color.FromArgb(15, 23, 42);
        private static readonly Color CardBg = Color.FromArgb(30, 41, 59);
        private static readonly Color SecBg = Color.FromArgb(51, 65, 85);
        private static readonly Color BorderCol = Color.FromArgb(71, 85, 105);
        private static readonly Color TextMain = Color.FromArgb(248, 250, 252);
        private static readonly Color TextMuted = Color.FromArgb(148, 163, 184);
        private static readonly Color PrimarySky = Color.FromArgb(56, 189, 248);
        private static readonly Color EmeraldBtn = Color.FromArgb(16, 185, 129);
        private static readonly Color RoseBtn = Color.FromArgb(225, 29, 72);

        private string baseDir;
        private string jsmDir;
        private string configPath;

        private string activeTab = "tab-r";
        private string selectedKey = "ZR";
        private int stickSens = 360;
        private int slowSens = 140;
        private decimal deadzone = 0.18m;
        private int gyroMode = 1;
        private int minGyro = 10;
        private int maxGyro = 18;
        private bool suppressEvents = false;

        private Dictionary<string, string> bindings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> buttonMeta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string[]> tabGroups = new Dictionary<string, string[]>();
        private List<PresetItem> presets = new List<PresetItem>();

        private DualJoyConCanvas joyconCanvas;
        private Label lblActiveCodeBadge;
        private Label lblSelectedBtnTitle;
        private ComboBox cmbQuickPreset;
        private Panel tabHostPanel;
        private Dictionary<string, Button> tabButtons = new Dictionary<string, Button>();

        // Cached Tab Panels & Controls (Zero recreation on click!)
        private Dictionary<string, Panel> cachedTabPanels = new Dictionary<string, Panel>();
        private Dictionary<string, Panel> rowPanels = new Dictionary<string, Panel>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, Label> rowLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, ComboBox> rowCombos = new Dictionary<string, ComboBox>(StringComparer.OrdinalIgnoreCase);
        private TrackBar tbStickSens, tbSlowSens, tbDeadzone, tbMinGyro, tbMaxGyro;
        private Label lblValStickSens, lblValSlowSens, lblValDeadzone, lblValMinGyro, lblValMaxGyro;
        private ComboBox cmbGyroModeControl;
        private TextBox txtLiveCode;

        private Label lblStatusText;
        private Label lblFileNameBadge;
        private Label lblEngineStatus;

        public MainForm()
        {
            this.DoubleBuffered = true;
            baseDir = AppDomain.CurrentDomain.BaseDirectory;
            jsmDir = ResolveJsmDir(baseDir);
            configPath = Path.Combine(jsmDir, "GyroConfigs", "JoyCon_Individual_Hybrid.txt");

            InitData();
            InitUI();
            BuildAllTabPanelsOnce();
            LoadConfigFromFile(configPath);
            SyncUIFromState();
            SwitchTab("tab-r");
            CheckEngineStatusAsync();
        }

        private void InitData()
        {
            presets.Add(new PresetItem("마우스 좌클릭 / 드래그", "LMOUSE"));
            presets.Add(new PresetItem("마우스 우클릭", "RMOUSE"));
            presets.Add(new PresetItem("마우스 휠클릭 (길게: 스페이스바)", "MMOUSE SPACE"));
            presets.Add(new PresetItem("마우스 휠클릭 (단독)", "MMOUSE"));
            presets.Add(new PresetItem("마우스 휠 위로 연속 스크롤", @"!SCROLLUP\ SCROLLUP+"));
            presets.Add(new PresetItem("마우스 휠 아래로 연속 스크롤", @"!SCROLLDOWN\ SCROLLDOWN+"));
            presets.Add(new PresetItem("왼쪽 방향키 (길게: 뒤로가기)", "LEFT BMOUSE"));
            presets.Add(new PresetItem("오른쪽 방향키 (길게: 앞으로가기)", "RIGHT FMOUSE"));
            presets.Add(new PresetItem("브라우저 뒤로가기", "BMOUSE"));
            presets.Add(new PresetItem("브라우저 앞으로가기", "FMOUSE"));
            presets.Add(new PresetItem("엔터 (Enter)", "ENTER"));
            presets.Add(new PresetItem("스페이스바 (Space)", "SPACE"));
            presets.Add(new PresetItem("백스페이스 (Backspace)", "BACKSPACE"));
            presets.Add(new PresetItem("ESC (취소)", "ESC"));
            presets.Add(new PresetItem("ESC (길게: 작업 보기 Win+Tab)", "ESC' LWINDOWS_ !TAB_"));
            presets.Add(new PresetItem("탭 (Tab)", "TAB"));
            presets.Add(new PresetItem("현재 탭 닫기 (Ctrl + W)", @"LCONTROL\ !W\"));
            presets.Add(new PresetItem("새 탭 열기 (Ctrl + T)", @"LCONTROL\ !T\"));
            presets.Add(new PresetItem("복사 (Ctrl + C)", @"LCONTROL\ !C\"));
            presets.Add(new PresetItem("붙여넣기 (Ctrl + V)", @"LCONTROL\ !V\"));
            presets.Add(new PresetItem("실행 취소 (Ctrl + Z)", @"LCONTROL\ !Z\"));
            presets.Add(new PresetItem("바탕화면 보기 (Win + D)", @"LWINDOWS\ !D\"));
            presets.Add(new PresetItem("창 닫기 (Alt + F4)", @"LALT\ !F4\"));
            presets.Add(new PresetItem("미디어 재생/일시정지 (조합키 겸용)", "PLAY_PAUSE NONE"));
            presets.Add(new PresetItem("볼륨 증가 (연속)", @"!VOLUME_UP\ VOLUME_UP+"));
            presets.Add(new PresetItem("볼륨 감소 (연속)", @"!VOLUME_DOWN\ VOLUME_DOWN+"));
            presets.Add(new PresetItem("음소거 (Mute)", "MUTE"));
            presets.Add(new PresetItem("자이로 켜기/끄기 토글 (홈/캡처 기본)", "^GYRO_OFF"));
            presets.Add(new PresetItem("자이로 영점 수동 재보정 (누르는 동안)", "CALIBRATE"));
            presets.Add(new PresetItem("비활성화 (NONE)", "NONE"));

            ResetStateToDefault();

            buttonMeta["ZR"] = "ZR 버튼 (Joy-Con R 후면 상단 트리거)";
            buttonMeta["R"] = "R 버튼 (Joy-Con R 상단 숄더)";
            buttonMeta["R3"] = "R 스틱 누르기 (R3)";
            buttonMeta["N"] = "X 버튼 (상단 / 코드: N)";
            buttonMeta["S"] = "B 버튼 (하단 / 코드: S)";
            buttonMeta["W"] = "Y 버튼 (좌측 / 코드: W)";
            buttonMeta["E"] = "A 버튼 (우측 / 코드: E)";
            buttonMeta["+"] = "+ 버튼 (플러스)";
            buttonMeta["HOME"] = "HOME 버튼 (홈)";
            buttonMeta["RSR"] = "우측 SR 버튼 (레일 상단 / 조합키)";
            buttonMeta["ZL"] = "ZL 버튼 (Joy-Con L 후면 상단 트리거)";
            buttonMeta["L"] = "L 버튼 (Joy-Con L 상단 숄더)";
            buttonMeta["L3"] = "L 스틱 누르기 (L3)";
            buttonMeta["UP"] = "방향 버튼 ▲ (상단 / UP)";
            buttonMeta["DOWN"] = "방향 버튼 ▼ (하단 / DOWN)";
            buttonMeta["LEFT"] = "방향 버튼 ◀ (좌측 / LEFT)";
            buttonMeta["RIGHT"] = "방향 버튼 ▶ (우측 / RIGHT)";
            buttonMeta["-"] = "- 버튼 (마이너스)";
            buttonMeta["CAPTURE"] = "캡처 버튼 (CAPTURE)";
            buttonMeta["LSR"] = "좌측 SR 버튼 (레일 하단 / 조합키)";
            buttonMeta["RSR,N"] = "[우측] SR + X (상단)";
            buttonMeta["RSR,S"] = "[우측] SR + B (하단)";
            buttonMeta["RSR,W"] = "[우측] SR + Y (좌측)";
            buttonMeta["RSR,E"] = "[우측] SR + A (우측)";
            buttonMeta["RSR,ZR"] = "[우측] SR + ZR";
            buttonMeta["RSR,R"] = "[우측] SR + R";
            buttonMeta["RSR,R3"] = "[우측] SR + R3";
            buttonMeta["LSR,UP"] = "[좌측] SR + 방향키 ▲";
            buttonMeta["LSR,DOWN"] = "[좌측] SR + 방향키 ▼";
            buttonMeta["LSR,LEFT"] = "[좌측] SR + 방향키 ◀";
            buttonMeta["LSR,RIGHT"] = "[좌측] SR + 방향키 ▶";
            buttonMeta["LSR,ZL"] = "[좌측] SR + ZL";
            buttonMeta["LSR,L"] = "[좌측] SR + L";
            buttonMeta["LSR,L3"] = "[좌측] SR + L3";

            tabGroups["tab-r"] = new[] { "ZR", "R", "R3", "N", "S", "W", "E", "+", "HOME", "RSR" };
            tabGroups["tab-l"] = new[] { "ZL", "L", "L3", "UP", "DOWN", "LEFT", "RIGHT", "-", "CAPTURE", "LSR" };
            tabGroups["tab-chord"] = new[] { "RSR,N", "RSR,S", "RSR,W", "RSR,E", "RSR,ZR", "RSR,R", "RSR,R3", "LSR,UP", "LSR,DOWN", "LSR,LEFT", "LSR,RIGHT", "LSR,ZL", "LSR,L", "LSR,L3" };
        }

        private void ResetStateToDefault()
        {
            stickSens = 360;
            slowSens = 140;
            deadzone = 0.18m;
            gyroMode = 1;
            minGyro = 10;
            maxGyro = 18;

            bindings["ZR"] = "LMOUSE";
            bindings["R"] = "RMOUSE";
            bindings["R3"] = "MMOUSE SPACE";
            bindings["N"] = @"!SCROLLUP\ SCROLLUP+";
            bindings["S"] = @"!SCROLLDOWN\ SCROLLDOWN+";
            bindings["W"] = "LEFT BMOUSE";
            bindings["E"] = "RIGHT FMOUSE";
            bindings["+"] = "ESC' LWINDOWS_ !TAB_";
            bindings["HOME"] = "CALIBRATE";
            bindings["RSR"] = "PLAY_PAUSE NONE";

            bindings["ZL"] = "LMOUSE";
            bindings["L"] = "RMOUSE";
            bindings["L3"] = "MMOUSE SPACE";
            bindings["UP"] = @"!SCROLLUP\ SCROLLUP+";
            bindings["DOWN"] = @"!SCROLLDOWN\ SCROLLDOWN+";
            bindings["LEFT"] = "LEFT BMOUSE";
            bindings["RIGHT"] = "RIGHT FMOUSE";
            bindings["-"] = "ESC' LWINDOWS_ !TAB_";
            bindings["CAPTURE"] = "CALIBRATE";
            bindings["LSR"] = "PLAY_PAUSE NONE";

            bindings["RSR,N"] = @"!VOLUME_UP\ VOLUME_UP+";
            bindings["RSR,S"] = @"!VOLUME_DOWN\ VOLUME_DOWN+";
            bindings["RSR,W"] = "BMOUSE";
            bindings["RSR,E"] = "FMOUSE";
            bindings["RSR,ZR"] = "ENTER";
            bindings["RSR,R"] = "BACKSPACE";
            bindings["RSR,R3"] = "MUTE";

            bindings["LSR,UP"] = @"!VOLUME_UP\ VOLUME_UP+";
            bindings["LSR,DOWN"] = @"!VOLUME_DOWN\ VOLUME_DOWN+";
            bindings["LSR,LEFT"] = "BMOUSE";
            bindings["LSR,RIGHT"] = "FMOUSE";
            bindings["LSR,ZL"] = "ENTER";
            bindings["LSR,L"] = "BACKSPACE";
            bindings["LSR,L3"] = "MUTE";
        }

        private Button CreateStyledButton(string text, Color bg, Color fg, int width, int height, bool bold = true)
        {
            Button b = new Button
            {
                Text = text,
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Malgun Gothic", 9f, bold ? FontStyle.Bold : FontStyle.Regular),
                Size = new Size(width, height),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderColor = BorderCol;
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        private void InitUI()
        {
            this.Text = "🎮 스위치 1 조이콘 마우스 - 버튼 커스텀 GUI (공식 사양 매칭)";
            this.Size = new Size(1060, 720);
            this.MinimumSize = new Size(1020, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BgDark;
            this.ForeColor = TextMain;
            this.Font = new Font("Malgun Gothic", 9f);

            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = SecBg,
                Padding = new Padding(16, 10, 16, 10)
            };

            Label lblTitle = new Label
            {
                Text = "🎮 스위치 1 조이콘 마우스 - 버튼 커스텀 GUI (공식 사양 매칭)",
                Font = new Font("Malgun Gothic", 12f, FontStyle.Bold),
                ForeColor = TextMain,
                AutoSize = true,
                Location = new Point(16, 10)
            };
            Label lblSub = new Label
            {
                Text = "닌텐도 공식 사양(L/R) 컨트롤러의 물리 버튼을 클릭해 매핑을 수정하고 .txt 설정 파일로 바로 저장할 수 있습니다.",
                Font = new Font("Malgun Gothic", 8.5f),
                ForeColor = TextMuted,
                AutoSize = true,
                Location = new Point(18, 36)
            };
            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSub);

            FlowLayoutPanel topBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 6, 0, 0)
            };

            Button btnOpen = CreateStyledButton("📂 설정 열기", BgDark, TextMain, 96, 32);
            btnOpen.Click += (s, e) =>
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.InitialDirectory = Path.Combine(jsmDir, "GyroConfigs");
                    ofd.Filter = "JoyShockMapper Config (*.txt)|*.txt";
                    if (ofd.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadConfigFromFile(ofd.FileName);
                        SyncUIFromState();
                    }
                }
            };

            Button btnReset = CreateStyledButton("↺ 초기화", BgDark, TextMain, 84, 32);
            btnReset.Click += (s, e) =>
            {
                ResetStateToDefault();
                lblStatusText.Text = "기본값으로 초기화되었습니다.";
                SyncUIFromState();
            };

            Button btnCopy = CreateStyledButton("📋 코드 복사", BgDark, TextMain, 96, 32);
            btnCopy.Click += (s, e) =>
            {
                Clipboard.SetText(GenerateConfigText());
                lblStatusText.Text = "✅ 클립보드에 설정 코드가 복사되었습니다!";
            };

            Button btnSaveOnly = CreateStyledButton("💾 파일로만 저장", BgDark, TextMain, 118, 32);
            btnSaveOnly.Click += (s, e) =>
            {
                SaveCurrentConfigToFile(configPath);
                lblStatusText.Text = "💾 [" + Path.GetFileName(configPath) + "] 파일에 성공적으로 저장되었습니다!";
            };

            topBtns.Controls.Add(btnOpen);
            topBtns.Controls.Add(btnReset);
            topBtns.Controls.Add(btnCopy);
            topBtns.Controls.Add(btnSaveOnly);
            headerPanel.Controls.Add(topBtns);

            Panel launcherBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = BgDark,
                Padding = new Padding(16, 7, 16, 7)
            };
            launcherBar.Paint += (s, e) =>
            {
                using (Pen p = new Pen(BorderCol, 1))
                {
                    e.Graphics.DrawLine(p, 0, 0, launcherBar.Width, 0);
                    e.Graphics.DrawLine(p, 0, launcherBar.Height - 1, launcherBar.Width, launcherBar.Height - 1);
                }
            };

            lblEngineStatus = new Label
            {
                Text = "⚪ 대기 중 (실행 버튼을 누르세요)",
                Font = new Font("Malgun Gothic", 9f, FontStyle.Bold),
                ForeColor = TextMain,
                BackColor = SecBg,
                AutoSize = true,
                Padding = new Padding(10, 5, 10, 5),
                Location = new Point(16, 9)
            };
            launcherBar.Controls.Add(lblEngineStatus);

            FlowLayoutPanel launchBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            Button btnRunHybrid = CreateStyledButton("🚀 저장 및 마우스 즉시 실행 (기본 하이브리드)", EmeraldBtn, Color.FromArgb(10, 20, 15), 275, 32);
            btnRunHybrid.FlatAppearance.BorderSize = 0;
            btnRunHybrid.Click += (s, e) =>
            {
                SaveCurrentConfigToFile(configPath);
                lblStatusText.Text = "🚀 설정을 저장하고 조이콘 마우스(기본 하이브리드 모드)를 실행하는 중...";
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    StartJsmProfile(baseDir, "JoyCon_Individual_Hybrid.txt");
                    CheckEngineStatusAsync();
                });
            };

            Button btnRunGyroScroll = CreateStyledButton("🎯 모드2 실행 (자이로+스틱스크롤)", PrimarySky, BgDark, 210, 32);
            btnRunGyroScroll.FlatAppearance.BorderSize = 0;
            btnRunGyroScroll.Click += (s, e) =>
            {
                lblStatusText.Text = "🎯 모드2 (자이로 포인터 + 스틱 스크롤)로 조이콘 마우스를 실행하는 중...";
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    StartJsmProfile(baseDir, "JoyCon_Individual_GyroScroll.txt");
                    CheckEngineStatusAsync();
                });
            };

            Button btnRunStickOnly = CreateStyledButton("🕹️ 모드3 실행 (스틱전용)", SecBg, TextMain, 165, 32);
            btnRunStickOnly.Click += (s, e) =>
            {
                lblStatusText.Text = "🕹️ 모드3 (아날로그 스틱 전용)으로 조이콘 마우스를 실행하는 중...";
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    StartJsmProfile(baseDir, "JoyCon_Individual_StickOnly.txt");
                    CheckEngineStatusAsync();
                });
            };

            Button btnStop = CreateStyledButton("⏹ 마우스 종료", RoseBtn, Color.White, 110, 32);
            btnStop.FlatAppearance.BorderSize = 0;
            btnStop.Click += (s, e) =>
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    StopJsmAll();
                    CheckEngineStatusAsync();
                });
                lblStatusText.Text = "⏹ 실행 중인 조이콘 마우스 엔진(SDL2)을 종료했습니다.";
            };

            launchBtns.Controls.Add(btnRunHybrid);
            launchBtns.Controls.Add(btnRunGyroScroll);
            launchBtns.Controls.Add(btnRunStickOnly);
            launchBtns.Controls.Add(btnStop);
            launcherBar.Controls.Add(launchBtns);

            TableLayoutPanel mainGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = BorderCol,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            mainGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360f));
            mainGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            Panel leftCol = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgDark,
                Padding = new Padding(14)
            };

            Panel leftTopRow = new Panel { Dock = DockStyle.Top, Height = 24 };
            Label lblLeftGuide = new Label
            {
                Text = "Joy-Con (L) / (R) 공식 사양 버튼 선택",
                Font = new Font("Malgun Gothic", 9f, FontStyle.Bold),
                ForeColor = TextMuted,
                AutoSize = true,
                Location = new Point(2, 2)
            };
            lblActiveCodeBadge = new Label
            {
                Text = "선택 코드: ZR",
                Font = new Font("Consolas", 9.5f, FontStyle.Bold),
                ForeColor = PrimarySky,
                AutoSize = true,
                Dock = DockStyle.Right
            };
            leftTopRow.Controls.Add(lblLeftGuide);
            leftTopRow.Controls.Add(lblActiveCodeBadge);

            Panel quickBox = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 82,
                BackColor = CardBg,
                Padding = new Padding(10)
            };
            quickBox.Paint += (s, e) =>
            {
                using (Pen p = new Pen(BorderCol, 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, quickBox.Width - 1, quickBox.Height - 1);
                }
            };

            lblSelectedBtnTitle = new Label
            {
                Text = "선택됨: ZR 버튼",
                Font = new Font("Malgun Gothic", 9f, FontStyle.Bold),
                ForeColor = TextMain,
                AutoSize = true,
                Location = new Point(10, 10)
            };

            Button btnQuickCap = CreateStyledButton("⌨️ 키보드 직접 입력", SecBg, TextMain, 130, 24);
            btnQuickCap.Font = new Font("Malgun Gothic", 8f, FontStyle.Bold);
            btnQuickCap.Location = new Point(192, 6);
            btnQuickCap.Click += (s, e) => OpenCaptureModal(selectedKey);

            cmbQuickPreset = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = BgDark,
                ForeColor = TextMain,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Malgun Gothic", 9f),
                Location = new Point(10, 40),
                Width = 312
            };
            foreach (var p in presets) cmbQuickPreset.Items.Add(p);
            cmbQuickPreset.SelectedIndexChanged += (s, e) =>
            {
                if (suppressEvents) return;
                PresetItem pi = cmbQuickPreset.SelectedItem as PresetItem;
                if (pi != null && bindings.ContainsKey(selectedKey))
                {
                    bindings[selectedKey] = pi.Code;
                    lblStatusText.Text = (buttonMeta.ContainsKey(selectedKey) ? buttonMeta[selectedKey] : selectedKey) + " → " + pi.Code + " 로 변경됨";
                    suppressEvents = true;
                    if (rowCombos.ContainsKey(selectedKey))
                        SelectComboItem(rowCombos[selectedKey], pi.Code);
                    if (activeTab == "tab-code")
                        txtLiveCode.Text = GenerateConfigText();
                    suppressEvents = false;
                }
            };

            quickBox.Controls.Add(lblSelectedBtnTitle);
            quickBox.Controls.Add(btnQuickCap);
            quickBox.Controls.Add(cmbQuickPreset);

            joyconCanvas = new DualJoyConCanvas { Dock = DockStyle.Fill };
            joyconCanvas.ButtonClicked += k =>
            {
                if (k == "RSL" || k == "LSL")
                {
                    lblStatusText.Text = k + " (SL) 버튼은 [스틱 & 자이로 감도] 탭에서 누르는 동안 자이로 활성화 및 정밀 모드를 담당합니다.";
                    SwitchTab("tab-sens");
                    return;
                }
                SelectButtonFast(k, true);
            };

            leftCol.Controls.Add(joyconCanvas);
            leftCol.Controls.Add(leftTopRow);
            leftCol.Controls.Add(quickBox);

            Panel rightCol = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg
            };

            FlowLayoutPanel tabBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = SecBg,
                Padding = new Padding(8, 6, 8, 0),
                WrapContents = false
            };

            AddTabButton(tabBar, "tab-r", "우측 조이콘 (R)");
            AddTabButton(tabBar, "tab-l", "좌측 조이콘 (L)");
            AddTabButton(tabBar, "tab-chord", "SL/SR 조합키");
            AddTabButton(tabBar, "tab-sens", "스틱 & 자이로 감도");
            AddTabButton(tabBar, "tab-code", "실시간 설정 코드");

            Panel statusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                BackColor = BgDark,
                Padding = new Padding(12, 8, 12, 8)
            };
            lblStatusText = new Label
            {
                Text = "좌측 조이콘(L/R) 그림의 버튼을 클릭하거나 목록에서 기능을 변경하세요.",
                Font = new Font("Malgun Gothic", 8.5f),
                ForeColor = TextMuted,
                AutoSize = true,
                Location = new Point(12, 9)
            };
            lblFileNameBadge = new Label
            {
                Text = "JoyCon_Individual_Hybrid.txt",
                Font = new Font("Consolas", 8.5f, FontStyle.Bold),
                ForeColor = TextMain,
                BackColor = CardBg,
                AutoSize = true,
                Padding = new Padding(6, 2, 6, 2),
                Dock = DockStyle.Right
            };
            statusBar.Controls.Add(lblStatusText);
            statusBar.Controls.Add(lblFileNameBadge);

            tabHostPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                Padding = new Padding(12)
            };

            rightCol.Controls.Add(tabHostPanel);
            rightCol.Controls.Add(tabBar);
            rightCol.Controls.Add(statusBar);

            mainGrid.Controls.Add(leftCol, 0, 0);
            mainGrid.Controls.Add(rightCol, 1, 0);

            this.Controls.Add(mainGrid);
            this.Controls.Add(launcherBar);
            this.Controls.Add(headerPanel);
        }

        private void BuildAllTabPanelsOnce()
        {
            foreach (string tabId in new[] { "tab-r", "tab-l", "tab-chord" })
            {
                Panel p = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Visible = false, BackColor = CardBg };
                string[] keys = tabGroups[tabId];
                int y = 6;
                foreach (string k in keys)
                {
                    string rowKey = k;
                    Panel row = new Panel
                    {
                        Location = new Point(6, y),
                        Size = new Size(620, 36),
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                        BackColor = BgDark
                    };

                    Label lbl = new Label
                    {
                        Text = buttonMeta.ContainsKey(rowKey) ? buttonMeta[rowKey] : rowKey,
                        Font = new Font("Malgun Gothic", 8.8f, FontStyle.Bold),
                        ForeColor = TextMain,
                        Location = new Point(10, 9),
                        Size = new Size(215, 20)
                    };

                    ComboBox cmb = new ComboBox
                    {
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        BackColor = CardBg,
                        ForeColor = TextMain,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Malgun Gothic", 8.8f),
                        Location = new Point(230, 5),
                        Width = 300,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    };
                    foreach (var pr in presets) cmb.Items.Add(pr);

                    cmb.SelectedIndexChanged += (s, e) =>
                    {
                        if (suppressEvents) return;
                        PresetItem pi = cmb.SelectedItem as PresetItem;
                        if (pi != null)
                        {
                            bindings[rowKey] = pi.Code;
                            lblStatusText.Text = (buttonMeta.ContainsKey(rowKey) ? buttonMeta[rowKey] : rowKey) + " → " + pi.Code + " 로 변경됨";
                            SelectButtonFast(rowKey, false);
                        }
                    };

                    Button btnCap = CreateStyledButton("키 입력", SecBg, TextMain, 72, 26);
                    btnCap.Font = new Font("Malgun Gothic", 8f, FontStyle.Bold);
                    btnCap.Location = new Point(540, 5);
                    btnCap.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                    btnCap.Click += (s, e) => OpenCaptureModal(rowKey);

                    row.Controls.Add(lbl);
                    row.Controls.Add(cmb);
                    row.Controls.Add(btnCap);
                    p.Controls.Add(row);

                    rowPanels[rowKey] = row;
                    rowLabels[rowKey] = lbl;
                    rowCombos[rowKey] = cmb;
                    y += 40;
                }
                cachedTabPanels[tabId] = p;
                tabHostPanel.Controls.Add(p);
            }

            // Tab 4: Sensitivity
            Panel pSens = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Visible = false, BackColor = CardBg };
            Panel box1 = new Panel { Location = new Point(6, 6), Size = new Size(620, 175), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackColor = BgDark };
            box1.Controls.Add(new Label { Text = "1. 아날로그 스틱 마우스 속도 & 데드존 (L 스틱 / R 스틱)", Font = new Font("Malgun Gothic", 9.5f, FontStyle.Bold), ForeColor = PrimarySky, AutoSize = true, Location = new Point(14, 12) });
            tbStickSens = CreateSliderRow(box1, "기본 스틱 속도 (STICK_SENS)", 44, 80, 1000, stickSens, out lblValStickSens, v => stickSens = v);
            tbSlowSens = CreateSliderRow(box1, "SL 버튼(우:RSL / 좌:LSL) 정밀 모드 속도", 86, 40, 500, slowSens, out lblValSlowSens, v => slowSens = v);
            tbDeadzone = CreateSliderRow(box1, "스틱 중앙 데드존 (DEADZONE %)", 128, 5, 40, (int)Math.Round(deadzone * 100m), out lblValDeadzone, v => deadzone = v / 100m);

            Panel box2 = new Panel { Location = new Point(6, 192), Size = new Size(620, 185), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackColor = BgDark };
            box2.Controls.Add(new Label { Text = "2. 자이로(모션) 에어마우스 활성화 방식 (SL 버튼: 우 RSL / 좌 LSL)", Font = new Font("Malgun Gothic", 9.5f, FontStyle.Bold), ForeColor = PrimarySky, AutoSize = true, Location = new Point(14, 12) });
            cmbGyroModeControl = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, BackColor = CardBg, ForeColor = TextMain, FlatStyle = FlatStyle.Flat, Font = new Font("Malgun Gothic", 9f), Location = new Point(14, 42), Width = 590, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            cmbGyroModeControl.Items.Add("항상 자이로 켜기 (상시 에어마우스 + SL 버튼 누르면 커서 고정/정밀모드)");
            cmbGyroModeControl.Items.Add("SL 버튼(우:RSL / 좌:LSL)을 누르고 있는 동안만 자이로 켜기 [기본 추천]");
            cmbGyroModeControl.Items.Add("자이로 완전히 끄기 (아날로그 스틱 전용)");
            cmbGyroModeControl.SelectedIndexChanged += (s, e) => { if (!suppressEvents) gyroMode = cmbGyroModeControl.SelectedIndex; };
            box2.Controls.Add(cmbGyroModeControl);
            tbMinGyro = CreateSliderRow(box2, "자이로 최소 감도 (MIN_GYRO_SENS)", 86, 1, 30, minGyro, out lblValMinGyro, v => minGyro = v);
            tbMaxGyro = CreateSliderRow(box2, "자이로 최대 감도 (MAX_GYRO_SENS)", 128, 2, 50, maxGyro, out lblValMaxGyro, v => maxGyro = v);

            pSens.Controls.Add(box1);
            pSens.Controls.Add(box2);
            cachedTabPanels["tab-sens"] = pSens;
            tabHostPanel.Controls.Add(pSens);

            // Tab 5: Live Code
            Panel pCode = new Panel { Dock = DockStyle.Fill, Visible = false, BackColor = CardBg };
            txtLiveCode = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = BgDark,
                ForeColor = TextMain,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 9.5f),
                Dock = DockStyle.Fill
            };
            pCode.Controls.Add(txtLiveCode);
            cachedTabPanels["tab-code"] = pCode;
            tabHostPanel.Controls.Add(pCode);
        }

        private TrackBar CreateSliderRow(Panel parent, string labelText, int y, int min, int max, int cur, out Label valLbl, Action<int> onChanged)
        {
            Label lbl = new Label { Text = labelText, Font = new Font("Malgun Gothic", 8.8f), ForeColor = TextMuted, Location = new Point(14, y + 6), Size = new Size(280, 20) };
            Label vLabel = new Label { Text = cur.ToString(), Font = new Font("Consolas", 9.5f, FontStyle.Bold), ForeColor = TextMain, Location = new Point(555, y + 6), Size = new Size(48, 20), TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            TrackBar tb = new TrackBar { Minimum = min, Maximum = max, Value = Math.Max(min, Math.Min(max, cur)), TickStyle = TickStyle.None, AutoSize = false, Location = new Point(300, y), Size = new Size(250, 32), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            tb.Scroll += (s, e) =>
            {
                vLabel.Text = tb.Value.ToString();
                onChanged(tb.Value);
            };
            parent.Controls.Add(lbl);
            parent.Controls.Add(tb);
            parent.Controls.Add(vLabel);
            valLbl = vLabel;
            return tb;
        }

        private void SelectComboItem(ComboBox cmb, string code)
        {
            foreach (PresetItem p in cmb.Items)
            {
                if (string.Equals(p.Code, code, StringComparison.OrdinalIgnoreCase))
                {
                    cmb.SelectedItem = p;
                    return;
                }
            }
            PresetItem custom = new PresetItem("직접 지정", code);
            cmb.Items.Add(custom);
            cmb.SelectedItem = custom;
        }

        private void SelectButtonFast(string newKey, bool autoSwitchTab)
        {
            string prevKey = selectedKey;
            selectedKey = newKey;

            if (rowPanels.ContainsKey(prevKey))
            {
                rowPanels[prevKey].BackColor = BgDark;
                rowLabels[prevKey].ForeColor = TextMain;
            }
            if (rowPanels.ContainsKey(selectedKey))
            {
                rowPanels[selectedKey].BackColor = SecBg;
                rowLabels[selectedKey].ForeColor = PrimarySky;
            }

            joyconCanvas.SelectedKey = selectedKey;
            joyconCanvas.Invalidate();
            lblActiveCodeBadge.Text = "선택 코드: " + selectedKey;
            lblSelectedBtnTitle.Text = "선택됨: " + (buttonMeta.ContainsKey(selectedKey) ? buttonMeta[selectedKey] : selectedKey);

            suppressEvents = true;
            SelectComboItem(cmbQuickPreset, bindings.ContainsKey(selectedKey) ? bindings[selectedKey] : "NONE");
            suppressEvents = false;

            if (autoSwitchTab)
            {
                if (Array.IndexOf(tabGroups["tab-r"], selectedKey) >= 0 && activeTab != "tab-r") SwitchTab("tab-r");
                else if (Array.IndexOf(tabGroups["tab-l"], selectedKey) >= 0 && activeTab != "tab-l") SwitchTab("tab-l");
                lblStatusText.Text = "선택된 물리 버튼: " + (buttonMeta.ContainsKey(selectedKey) ? buttonMeta[selectedKey] : selectedKey);
            }
        }

        private void SwitchTab(string tabId)
        {
            activeTab = tabId;
            foreach (var kv in tabButtons)
            {
                bool isAct = kv.Key == activeTab;
                kv.Value.BackColor = isAct ? CardBg : SecBg;
                kv.Value.ForeColor = isAct ? PrimarySky : TextMuted;
                kv.Value.Font = new Font("Malgun Gothic", 9f, isAct ? FontStyle.Bold : FontStyle.Regular);
            }
            if (activeTab == "tab-code")
            {
                txtLiveCode.Text = GenerateConfigText();
            }
            foreach (var kv in cachedTabPanels)
            {
                kv.Value.Visible = (kv.Key == activeTab);
            }
        }

        private void SyncUIFromState()
        {
            suppressEvents = true;
            foreach (var kv in rowCombos)
            {
                string val = bindings.ContainsKey(kv.Key) ? bindings[kv.Key] : "NONE";
                SelectComboItem(kv.Value, val);
            }
            tbStickSens.Value = Math.Max(tbStickSens.Minimum, Math.Min(tbStickSens.Maximum, stickSens));
            lblValStickSens.Text = stickSens.ToString();
            tbSlowSens.Value = Math.Max(tbSlowSens.Minimum, Math.Min(tbSlowSens.Maximum, slowSens));
            lblValSlowSens.Text = slowSens.ToString();
            int dzInt = (int)Math.Round(deadzone * 100m);
            tbDeadzone.Value = Math.Max(tbDeadzone.Minimum, Math.Min(tbDeadzone.Maximum, dzInt));
            lblValDeadzone.Text = dzInt.ToString();
            cmbGyroModeControl.SelectedIndex = gyroMode;
            tbMinGyro.Value = Math.Max(tbMinGyro.Minimum, Math.Min(tbMinGyro.Maximum, minGyro));
            lblValMinGyro.Text = minGyro.ToString();
            tbMaxGyro.Value = Math.Max(tbMaxGyro.Minimum, Math.Min(tbMaxGyro.Maximum, maxGyro));
            lblValMaxGyro.Text = maxGyro.ToString();
            suppressEvents = false;

            SelectButtonFast(selectedKey, false);
            if (activeTab == "tab-code") txtLiveCode.Text = GenerateConfigText();
        }

        private void AddTabButton(FlowLayoutPanel bar, string tabId, string title)
        {
            Button b = new Button
            {
                Text = title,
                Tag = tabId,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(125, 34),
                Margin = new Padding(2, 0, 2, 0),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += (s, e) => SwitchTab(tabId);
            tabButtons[tabId] = b;
            bar.Controls.Add(b);
        }

        public static string ResolveJsmDir(string baseDir)
        {
            string p1 = Path.Combine(baseDir, "JoyShockMapper", "JoyShockMapper_SDL2_x64");
            if (Directory.Exists(p1)) return p1;
            string p2 = Path.Combine(baseDir, "JoyShockMapper_SDL2_x64");
            if (Directory.Exists(p2)) return p2;
            return p1;
        }

        private void CheckEngineStatusAsync()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    Thread.Sleep(120);
                    bool running = Sdl2JoyConEngine.IsEngineRunning();
                    if (this.IsHandleCreated && !this.IsDisposed)
                    {
                        this.BeginInvoke((Action)(() =>
                        {
                            if (running)
                            {
                                lblEngineStatus.Text = "🟢 마우스 실행 중 (SDL2 내장 엔진)";
                                lblEngineStatus.ForeColor = Color.FromArgb(74, 222, 128);
                            }
                            else
                            {
                                lblEngineStatus.Text = "⚪ 대기 중 (실행 버튼을 누르세요)";
                                lblEngineStatus.ForeColor = TextMain;
                            }
                        }));
                    }
                }
                catch { }
            });
        }

        private void OpenCaptureModal(string key)
        {
            using (Form dlg = new Form())
            {
                dlg.Text = "키 입력 대기 - " + (buttonMeta.ContainsKey(key) ? buttonMeta[key] : key);
                dlg.Size = new Size(380, 190);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = CardBg;
                dlg.ForeColor = TextMain;
                dlg.KeyPreview = true;

                Label info = new Label
                {
                    Text = "배정할 키보드 키를 누르세요. (ESC: 취소)\n\n키 입력 대기 중...",
                    Font = new Font("Malgun Gothic", 10f, FontStyle.Bold),
                    ForeColor = TextMain,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                dlg.Controls.Add(info);

                dlg.KeyDown += (s, e) =>
                {
                    e.SuppressKeyPress = true;
                    if (e.KeyCode == Keys.Escape)
                    {
                        dlg.DialogResult = DialogResult.Cancel;
                        dlg.Close();
                        return;
                    }
                    string mapped = MapKeyCodeToJsm(e.KeyCode);
                    if (!string.IsNullOrEmpty(mapped))
                    {
                        bindings[key] = mapped;
                        selectedKey = key;
                        lblStatusText.Text = (buttonMeta.ContainsKey(key) ? buttonMeta[key] : key) + " → " + mapped + " 키로 지정됨";
                        dlg.DialogResult = DialogResult.OK;
                        dlg.Close();
                    }
                };
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    SyncUIFromState();
                }
            }
        }

        private string MapKeyCodeToJsm(Keys k)
        {
            if (k >= Keys.A && k <= Keys.Z) return k.ToString();
            if (k >= Keys.D0 && k <= Keys.D9) return ((int)(k - Keys.D0)).ToString();
            if (k >= Keys.F1 && k <= Keys.F12) return k.ToString();
            switch (k)
            {
                case Keys.Return: return "ENTER";
                case Keys.Space: return "SPACE";
                case Keys.Back: return "BACKSPACE";
                case Keys.Tab: return "TAB";
                case Keys.Up: return "UP";
                case Keys.Down: return "DOWN";
                case Keys.Left: return "LEFT";
                case Keys.Right: return "RIGHT";
                case Keys.PageUp: return "PAGEUP";
                case Keys.PageDown: return "PAGEDOWN";
                case Keys.Home: return "HOME";
                case Keys.End: return "END";
                case Keys.Insert: return "INSERT";
                case Keys.Delete: return "DELETE";
                case Keys.ShiftKey:
                case Keys.LShiftKey: return "LSHIFT";
                case Keys.ControlKey:
                case Keys.LControlKey: return "LCONTROL";
                case Keys.Menu:
                case Keys.LMenu: return "LALT";
                case Keys.LWin: return "LWINDOWS";
                default: return null;
            }
        }

        private void LoadConfigFromFile(string path)
        {
            if (!File.Exists(path)) return;
            configPath = path;
            if (lblFileNameBadge != null) lblFileNameBadge.Text = Path.GetFileName(path);

            decimal baseMin = 0, baseMax = 0, holdMin = 0, holdMax = 0;
            bool hasGyroOnSl = false;
            foreach (string raw in File.ReadAllLines(path, Encoding.ASCII))
            {
                string line = raw.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                int eq = line.IndexOf('=');
                if (eq <= 0) continue;
                string k = line.Substring(0, eq).Trim();
                string v = line.Substring(eq + 1).Trim();

                decimal d;
                if (k == "STICK_SENS" && decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out d)) stickSens = (int)d;
                else if ((k == "RSL,STICK_SENS" || k == "LSL,STICK_SENS" || k == "RSR,STICK_SENS") && decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out d)) slowSens = (int)d;
                else if (k == "STICK_DEADZONE_INNER" && decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out d)) deadzone = d;
                else if (k == "GYRO_ON" && (v == "LSL" || v == "RSL")) hasGyroOnSl = true;
                else if (k == "MIN_GYRO_SENS" && decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out d)) baseMin = d;
                else if (k == "MAX_GYRO_SENS" && decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out d)) baseMax = d;
                else if ((k == "RSL,MIN_GYRO_SENS" || k == "LSL,MIN_GYRO_SENS" || k == "RSR,MIN_GYRO_SENS") && decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out d)) holdMin = d;
                else if ((k == "RSL,MAX_GYRO_SENS" || k == "LSL,MAX_GYRO_SENS" || k == "RSR,MAX_GYRO_SENS") && decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out d)) holdMax = d;
                else if (bindings.ContainsKey(k))
                {
                    if (v == "CALIBRATE CALIBRATE" || v == "^GYRO_OFF CALIBRATE") v = "^GYRO_OFF";
                    bindings[k] = v;
                }
            }

            if (hasGyroOnSl && baseMin > 0)
            {
                gyroMode = 1;
                minGyro = (int)baseMin;
                maxGyro = baseMax > 0 ? (int)baseMax : 18;
            }
            else if (baseMin > 0)
            {
                gyroMode = 0;
                minGyro = (int)baseMin;
                maxGyro = baseMax > 0 ? (int)baseMax : 18;
            }
            else if (holdMin > 0)
            {
                gyroMode = 1;
                minGyro = (int)holdMin;
                maxGyro = holdMax > 0 ? (int)holdMax : 18;
            }
            else
            {
                gyroMode = 2;
            }
        }

        private string GenerateConfigText()
        {
            int baseMin = (gyroMode == 0) ? minGyro : 0;
            int baseMax = (gyroMode == 0) ? maxGyro : 0;
            int holdMin = (gyroMode == 1) ? minGyro : 0;
            int holdMax = (gyroMode == 1) ? maxGyro : 0;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# =============================================================================");
            sb.AppendLine("# [Nintendo Switch 1 Left / Right Joy-Con] Individual Hybrid Mouse Config");
            sb.AppendLine("# Generated by JoyCon_Custom_GUI");
            sb.AppendLine("# =============================================================================");
            sb.AppendLine("RESET_MAPPINGS");
            sb.AppendLine("AUTOLOAD = OFF");
            sb.AppendLine("AUTO_CALIBRATE_GYRO = ON");
            sb.AppendLine();
            sb.AppendLine("REAL_WORLD_CALIBRATION = 5.3333");
            sb.AppendLine("IN_GAME_SENS = 1");
            sb.AppendLine("COUNTER_OS_MOUSE_SPEED");
            sb.AppendLine();
            sb.AppendLine("GYRO_SPACE = PLAYER_TURN");
            sb.AppendLine("MIN_GYRO_SENS = " + baseMin);
            sb.AppendLine("MAX_GYRO_SENS = " + baseMax);
            sb.AppendLine("MIN_GYRO_THRESHOLD = 0.5");
            sb.AppendLine("MAX_GYRO_THRESHOLD = 60");
            sb.AppendLine("GYRO_SMOOTH_THRESHOLD = 1.0");
            sb.AppendLine("GYRO_CUTOFF_RECOVERY = 0");
            sb.AppendLine();
            sb.AppendLine("LEFT_STICK_MODE = AIM");
            sb.AppendLine("RIGHT_STICK_MODE = AIM");
            sb.AppendLine("STICK_SENS = " + stickSens);
            sb.AppendLine("STICK_POWER = 1.5");
            sb.AppendLine("STICK_ACCELERATION_RATE = 1.2");
            sb.AppendLine("STICK_ACCELERATION_CAP = 2.5");
            sb.AppendLine("STICK_DEADZONE_INNER = " + deadzone.ToString("0.00", CultureInfo.InvariantCulture));
            sb.AppendLine("STICK_DEADZONE_OUTER = 0.10");
            sb.AppendLine();
            sb.AppendLine("ZL = " + bindings["ZL"]);
            sb.AppendLine("ZR = " + bindings["ZR"]);
            sb.AppendLine("L  = " + bindings["L"]);
            sb.AppendLine("R  = " + bindings["R"]);
            sb.AppendLine("L3 = " + bindings["L3"]);
            sb.AppendLine("R3 = " + bindings["R3"]);
            sb.AppendLine();
            sb.AppendLine("UP    = " + bindings["UP"]);
            sb.AppendLine("DOWN  = " + bindings["DOWN"]);
            sb.AppendLine("LEFT  = " + bindings["LEFT"]);
            sb.AppendLine("RIGHT = " + bindings["RIGHT"]);
            sb.AppendLine("N     = " + bindings["N"]);
            sb.AppendLine("S     = " + bindings["S"]);
            sb.AppendLine("W     = " + bindings["W"]);
            sb.AppendLine("E     = " + bindings["E"]);
            sb.AppendLine();
            sb.AppendLine("LSL = NONE");
            sb.AppendLine("RSL = NONE");
            if (gyroMode == 1)
            {
                sb.AppendLine("LSL,MIN_GYRO_SENS = " + holdMin);
                sb.AppendLine("LSL,MAX_GYRO_SENS = " + holdMax);
                sb.AppendLine("RSL,MIN_GYRO_SENS = " + holdMin);
                sb.AppendLine("RSL,MAX_GYRO_SENS = " + holdMax);
            }
            sb.AppendLine("LSL,STICK_SENS = " + slowSens);
            sb.AppendLine("RSL,STICK_SENS = " + slowSens);
            sb.AppendLine();
            sb.AppendLine("LSR = " + bindings["LSR"]);
            sb.AppendLine("LSR,UP    = " + bindings["LSR,UP"]);
            sb.AppendLine("LSR,DOWN  = " + bindings["LSR,DOWN"]);
            sb.AppendLine("LSR,LEFT  = " + bindings["LSR,LEFT"]);
            sb.AppendLine("LSR,RIGHT = " + bindings["LSR,RIGHT"]);
            sb.AppendLine("LSR,ZL    = " + bindings["LSR,ZL"]);
            sb.AppendLine("LSR,L     = " + bindings["LSR,L"]);
            sb.AppendLine("LSR,L3    = " + bindings["LSR,L3"]);
            sb.AppendLine();
            sb.AppendLine("RSR = " + bindings["RSR"]);
            sb.AppendLine("RSR,N     = " + bindings["RSR,N"]);
            sb.AppendLine("RSR,S     = " + bindings["RSR,S"]);
            sb.AppendLine("RSR,W     = " + bindings["RSR,W"]);
            sb.AppendLine("RSR,E     = " + bindings["RSR,E"]);
            sb.AppendLine("RSR,ZR    = " + bindings["RSR,ZR"]);
            sb.AppendLine("RSR,R     = " + bindings["RSR,R"]);
            sb.AppendLine("RSR,R3    = " + bindings["RSR,R3"]);
            sb.AppendLine();
            sb.AppendLine("-       = " + bindings["-"]);
            sb.AppendLine("+       = " + bindings["+"]);
            sb.AppendLine("CAPTURE = " + bindings["CAPTURE"]);
            sb.AppendLine("HOME    = " + bindings["HOME"]);
            return sb.ToString();
        }

        private void SaveCurrentConfigToFile(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, GenerateConfigText(), Encoding.ASCII);
        }

        public static void StartJsmProfile(string baseDir, string profileFileName)
        {
            StopJsmAll();
            string exePath = Application.ExecutablePath;
            ProcessStartInfo psi = new ProcessStartInfo(exePath, "--engine \"" + profileFileName + "\"");
            psi.WorkingDirectory = baseDir;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

        public static int StopJsmAll()
        {
            return Sdl2JoyConEngine.StopRunningEngine() ? 1 : 0;
        }

        public static void RegisterUrlProtocol()
        {
            try
            {
                string exePath = Application.ExecutablePath;
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\joycon-remote"))
                {
                    key.SetValue("", "URL:JoyCon Smart Remote Protocol");
                    key.SetValue("URL Protocol", "");
                    using (RegistryKey cmdKey = key.CreateSubKey(@"shell\open\command"))
                    {
                        cmdKey.SetValue("", "\"" + exePath + "\" \"%1\"");
                    }
                }
            }
            catch { }
        }

        public static void StartHttpBridge(string baseDir)
        {
            Thread t = new Thread(() =>
            {
                try
                {
                    TcpListener listener = new TcpListener(IPAddress.Loopback, 18970);
                    listener.Start();
                    while (true)
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        ThreadPool.QueueUserWorkItem(_ => HandleHttpClient(client, baseDir));
                    }
                }
                catch { }
            });
            t.IsBackground = true;
            t.Start();
        }

        private static void HandleHttpClient(TcpClient client, string baseDir)
        {
            try
            {
                using (client)
                using (NetworkStream ns = client.GetStream())
                {
                    byte[] buf = new byte[65536];
                    int read = ns.Read(buf, 0, buf.Length);
                    if (read <= 0) return;
                    string req = Encoding.UTF8.GetString(buf, 0, read);
                    string[] lines = req.Split(new[] { "\r\n" }, StringSplitOptions.None);
                    string[] first = lines[0].Split(' ');
                    string method = first.Length > 0 ? first[0] : "GET";
                    string path = first.Length > 1 ? first[1] : "/";

                    int bodyIdx = req.IndexOf("\r\n\r\n");
                    string body = bodyIdx >= 0 ? req.Substring(bodyIdx + 4) : "";

                    if (method == "OPTIONS")
                    {
                        SendHttpResponse(ns, "200 OK", "text/plain", "OK");
                        return;
                    }

                    string jsmDir = ResolveJsmDir(baseDir);
                    string hybridPath = Path.Combine(jsmDir, "GyroConfigs", "JoyCon_Individual_Hybrid.txt");

                    if (path.StartsWith("/api/status"))
                    {
                        bool running = Sdl2JoyConEngine.IsEngineRunning();
                        SendHttpResponse(ns, "200 OK", "application/json", "{\"running\":" + (running ? "true" : "false") + "}");
                    }
                    else if (path.StartsWith("/api/apply") && method == "POST")
                    {
                        if (!string.IsNullOrWhiteSpace(body))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(hybridPath));
                            File.WriteAllText(hybridPath, body, Encoding.ASCII);
                        }
                        StartJsmProfile(baseDir, "JoyCon_Individual_Hybrid.txt");
                        SendHttpResponse(ns, "200 OK", "application/json", "{\"ok\":true,\"running\":true}");
                    }
                    else if (path.StartsWith("/api/run"))
                    {
                        string profile = "JoyCon_Individual_Hybrid.txt";
                        if (path.Contains("gyroscroll")) profile = "JoyCon_Individual_GyroScroll.txt";
                        else if (path.Contains("stickonly")) profile = "JoyCon_Individual_StickOnly.txt";
                        StartJsmProfile(baseDir, profile);
                        SendHttpResponse(ns, "200 OK", "application/json", "{\"ok\":true,\"running\":true,\"profile\":\"" + profile + "\"}");
                    }
                    else if (path.StartsWith("/api/stop"))
                    {
                        StopJsmAll();
                        SendHttpResponse(ns, "200 OK", "application/json", "{\"ok\":true,\"running\":false}");
                    }
                    else
                    {
                        string htmlPath = Path.Combine(baseDir, "조이콘_버튼_커스텀_GUI.html");
                        if (File.Exists(htmlPath))
                        {
                            SendHttpResponse(ns, "200 OK", "text/html; charset=utf-8", File.ReadAllText(htmlPath, Encoding.UTF8));
                        }
                        else
                        {
                            SendHttpResponse(ns, "404 Not Found", "text/plain", "Not Found");
                        }
                    }
                }
            }
            catch { }
        }

        private static void SendHttpResponse(NetworkStream ns, string status, string contentType, string content)
        {
            byte[] bodyBytes = Encoding.UTF8.GetBytes(content);
            string header = "HTTP/1.1 " + status + "\r\n" +
                            "Content-Type: " + contentType + "\r\n" +
                            "Access-Control-Allow-Origin: *\r\n" +
                            "Access-Control-Allow-Methods: GET, POST, OPTIONS\r\n" +
                            "Access-Control-Allow-Headers: Content-Type\r\n" +
                            "Content-Length: " + bodyBytes.Length + "\r\n" +
                            "Connection: close\r\n\r\n";
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);
            ns.Write(headerBytes, 0, headerBytes.Length);
            ns.Write(bodyBytes, 0, bodyBytes.Length);
        }

        [STAThread]
        static void Main(string[] args)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            RegisterUrlProtocol();

            if (args != null && args.Length > 0)
            {
                string arg0 = args[0].ToLowerInvariant();
                if (arg0 == "--engine")
                {
                    string profile = args.Length > 1 ? args[1] : "JoyCon_Individual_Hybrid.txt";
                    int maxRunMs = 0;
                    if (args.Length > 2) int.TryParse(args[2], out maxRunMs);
                    Sdl2JoyConEngine.RunConsoleEngine(baseDir, profile, maxRunMs);
                    return;
                }
                if (arg0 == "--stop")
                {
                    StopJsmAll();
                    return;
                }
                if (arg0.Contains("joycon-remote://"))
                {
                    if (arg0.Contains("stop"))
                    {
                        StopJsmAll();
                    }
                    else if (arg0.Contains("gyroscroll"))
                    {
                        StartJsmProfile(baseDir, "JoyCon_Individual_GyroScroll.txt");
                    }
                    else if (arg0.Contains("stickonly"))
                    {
                        StartJsmProfile(baseDir, "JoyCon_Individual_StickOnly.txt");
                    }
                    else
                    {
                        StartJsmProfile(baseDir, "JoyCon_Individual_Hybrid.txt");
                    }
                    return;
                }
                if (arg0 == "--server")
                {
                    StartHttpBridge(baseDir);
                    Thread.Sleep(Timeout.Infinite);
                    return;
                }
            }

            StartHttpBridge(baseDir);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public static class Sdl2JoyConEngine
    {
        private const string StopEventName = @"Local\JoyConSdl2EngineStopEvent";
        private const string ActiveMutexName = @"Local\JoyConSdl2EngineActiveMutex";

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetConsoleTitle(string lpConsoleTitle);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, ushort wAttributes);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool WriteConsoleW(
            IntPtr hConsoleOutput,
            string lpBuffer,
            uint nNumberOfCharsToWrite,
            out uint lpNumberOfCharsWritten,
            IntPtr lpReserved);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        private delegate bool ConsoleCtrlDelegate(int sig);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handler, bool add);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_Init(uint flags);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SDL_Quit();

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SDL_SetHint([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string value);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SDL_PumpEvents();

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SDL_GameControllerUpdate();

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_NumJoysticks();

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SDL_IsGameController(int joystick_index);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr SDL_GameControllerOpen(int joystick_index);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SDL_GameControllerClose(IntPtr gamecontroller);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern byte SDL_GameControllerGetAttached(IntPtr gamecontroller);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_GameControllerGetType(IntPtr gamecontroller);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr SDL_GameControllerName(IntPtr gamecontroller);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern byte SDL_GameControllerGetButton(IntPtr gamecontroller, int button);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern short SDL_GameControllerGetAxis(IntPtr gamecontroller, int axis);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_GameControllerSetSensorEnabled(IntPtr gamecontroller, int type, int enabled);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern byte SDL_GameControllerIsSensorEnabled(IntPtr gamecontroller, int type);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_GameControllerGetSensorData(IntPtr gamecontroller, int type, float[] data, int num_values);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private const uint INPUT_MOUSE = 0;
        private const uint INPUT_KEYBOARD = 1;

        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_XDOWN = 0x0080;
        private const uint MOUSEEVENTF_XUP = 0x0100;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;
        private const uint MOUSEEVENTF_HWHEEL = 0x01000;

        private const uint XBUTTON1 = 0x0001;
        private const uint XBUTTON2 = 0x0002;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private const uint SDL_INIT_JOYSTICK = 0x00000200;
        private const uint SDL_INIT_GAMECONTROLLER = 0x00002000;
        private const uint SDL_INIT_SENSOR = 0x00008000;
        private const int SDL_SENSOR_GYRO = 2;

        public static bool IsEngineRunning()
        {
            try
            {
                using (Mutex m = Mutex.OpenExisting(ActiveMutexName))
                {
                    try
                    {
                        if (m.WaitOne(0))
                        {
                            m.ReleaseMutex();
                            return false;
                        }
                        return true;
                    }
                    catch (AbandonedMutexException)
                    {
                        try { m.ReleaseMutex(); } catch { }
                        return false;
                    }
                }
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool StopRunningEngine()
        {
            bool wasRunning = IsEngineRunning();
            try
            {
                using (EventWaitHandle ev = new EventWaitHandle(false, EventResetMode.ManualReset, StopEventName))
                {
                    ev.Set();
                }
            }
            catch { }

            if (wasRunning)
            {
                for (int i = 0; i < 40; i++)
                {
                    if (!IsEngineRunning()) break;
                    Thread.Sleep(10);
                }
            }
            return wasRunning;
        }

        private class ButtonRuntimeState
        {
            public bool IsDown;
            public long DownTickMs;
            public bool HoldFired;
            public long LastRepeatTickMs;
            public string ActiveChordKey;
            public string HeldActionToken;
        }

        private class ControllerSlot
        {
            public int JoystickIndex;
            public IntPtr Handle;
            public string Name;
            public bool IsLeft;
            public bool IsRight;
            public float[] GyroBias = new float[3];
            public int CalibSamples = 0;
        }

        private static volatile bool _consoleClosing = false;
        private static ConsoleCtrlDelegate _ctrlHandler;
        private static IntPtr _hConOut = IntPtr.Zero;

        private static void SetConColor(ConsoleColor c)
        {
            try
            {
                if (_hConOut != IntPtr.Zero && _hConOut != new IntPtr(-1))
                {
                    SetConsoleTextAttribute(_hConOut, (ushort)c);
                }
            }
            catch { }
        }

        private static void ResetConColor()
        {
            try
            {
                if (_hConOut != IntPtr.Zero && _hConOut != new IntPtr(-1))
                {
                    SetConsoleTextAttribute(_hConOut, 7);
                }
            }
            catch { }
        }

        private static void WriteConLine(string msg)
        {
            try
            {
                if (_hConOut != IntPtr.Zero && _hConOut != new IntPtr(-1))
                {
                    string line = msg + "\r\n";
                    uint written;
                    WriteConsoleW(_hConOut, line, (uint)line.Length, out written, IntPtr.Zero);
                }
            }
            catch { }
        }

        public static void RunConsoleEngine(string baseDir, string profileFileName, int maxRunMs = 0)
        {
            StopRunningEngine();

            bool createdNew;
            using (Mutex activeMutex = new Mutex(true, ActiveMutexName, out createdNew))
            using (EventWaitHandle stopEvent = new EventWaitHandle(false, EventResetMode.ManualReset, StopEventName))
            {
                if (!createdNew)
                {
                    try
                    {
                        if (!activeMutex.WaitOne(1000)) return;
                    }
                    catch (AbandonedMutexException) { }
                }
                stopEvent.Reset();

                AllocConsole();
                try
                {
                    SetConsoleTitle("🎮 스위치 1 조이콘 마우스 엔진 (SDL2 직접 구동 - JoyShockMapper.exe 의존성 제거됨)");
                    _hConOut = CreateFile("CONOUT$", 0x40000000u | 0x80000000u, 2u, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
                    IntPtr hConIn = CreateFile("CONIN$", 0x80000000u | 0x40000000u, 1u, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
                    if (hConIn != IntPtr.Zero && hConIn != new IntPtr(-1))
                    {
                        uint mode;
                        if (GetConsoleMode(hConIn, out mode))
                        {
                            mode &= ~0x0040u; // Disable ENABLE_QUICK_EDIT_MODE so clicking console never freezes loop
                            mode |= 0x0080u;  // ENABLE_EXTENDED_FLAGS
                            SetConsoleMode(hConIn, mode);
                        }
                    }
                }
                catch { }

                _ctrlHandler = new ConsoleCtrlDelegate(sig =>
                {
                    _consoleClosing = true;
                    try { stopEvent.Set(); } catch { }
                    return true;
                });
                SetConsoleCtrlHandler(_ctrlHandler, true);

                string jsmDir = MainForm.ResolveJsmDir(baseDir);
                SetDllDirectory(jsmDir);

                string configPath = Path.Combine(jsmDir, "GyroConfigs", profileFileName);
                if (!File.Exists(configPath) && File.Exists(profileFileName))
                {
                    configPath = Path.GetFullPath(profileFileName);
                }

                // Parse configuration
                Dictionary<string, string> bindings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                string[] allKeys = new[] {
                    "ZL", "ZR", "L", "R", "L3", "R3",
                    "UP", "DOWN", "LEFT", "RIGHT", "N", "S", "W", "E",
                    "-", "+", "CAPTURE", "HOME", "LSL", "RSL", "LSR", "RSR",
                    "RSR,N", "RSR,S", "RSR,W", "RSR,E", "RSR,ZR", "RSR,R", "RSR,R3",
                    "LSR,UP", "LSR,DOWN", "LSR,LEFT", "LSR,RIGHT", "LSR,ZL", "LSR,L", "LSR,L3"
                };
                foreach (string k in allKeys) bindings[k] = "NONE";

                float stickSens = 360f;
                float slowStickSens = 140f;
                float deadzoneInner = 0.18f;
                float deadzoneOuter = 0.10f;
                float stickPower = 1.5f;
                float realWorldCalib = 5.3333f;
                float baseMinGyro = 0f, baseMaxGyro = 0f;
                float holdMinGyro = 0f, holdMaxGyro = 0f;
                float minGyroThreshold = 0.5f;
                float maxGyroThreshold = 60f;
                bool stickScrollMode = profileFileName.IndexOf("GyroScroll", StringComparison.OrdinalIgnoreCase) >= 0;
                bool hasGyroOnSl = false;

                if (File.Exists(configPath))
                {
                    foreach (string raw in File.ReadAllLines(configPath, Encoding.ASCII))
                    {
                        string line = raw.Trim();
                        if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                        int eq = line.IndexOf('=');
                        if (eq <= 0) continue;
                        string k = line.Substring(0, eq).Trim();
                        string v = line.Substring(eq + 1).Trim();

                        float f;
                        if (k.Equals("STICK_SENS", StringComparison.OrdinalIgnoreCase) && float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out f)) stickSens = f;
                        else if ((k.Equals("LSL,STICK_SENS", StringComparison.OrdinalIgnoreCase) || k.Equals("RSL,STICK_SENS", StringComparison.OrdinalIgnoreCase)) && float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out f)) slowStickSens = f;
                        else if (k.Equals("STICK_DEADZONE_INNER", StringComparison.OrdinalIgnoreCase) && float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out f)) deadzoneInner = f;
                        else if (k.Equals("STICK_DEADZONE_OUTER", StringComparison.OrdinalIgnoreCase) && float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out f)) deadzoneOuter = f;
                        else if (k.Equals("STICK_POWER", StringComparison.OrdinalIgnoreCase) && float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out f)) stickPower = f;
                        else if (k.Equals("REAL_WORLD_CALIBRATION", StringComparison.OrdinalIgnoreCase) && float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out f)) realWorldCalib = f;
                        else if (k.Equals("MIN_GYRO_SENS", StringComparison.OrdinalIgnoreCase) && float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out f)) baseMinGyro = f;
                        else if (k.Equals("MAX_GYRO_SENS", StringComparison.OrdinalIgnoreCase) && float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out f)) baseMaxGyro = f;
                        else if ((k.Equals("LSL,MIN_GYRO_SENS", StringComparison.OrdinalIgnoreCase) || k.Equals("RSL,MIN_GYRO_SENS", StringComparison.OrdinalIgnoreCase)) && float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out f)) holdMinGyro = f;
                        else if ((k.Equals("LSL,MAX_GYRO_SENS", StringComparison.OrdinalIgnoreCase) || k.Equals("RSL,MAX_GYRO_SENS", StringComparison.OrdinalIgnoreCase)) && float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out f)) holdMaxGyro = f;
                        else if (k.Equals("MIN_GYRO_THRESHOLD", StringComparison.OrdinalIgnoreCase) && float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out f)) minGyroThreshold = f;
                        else if (k.Equals("MAX_GYRO_THRESHOLD", StringComparison.OrdinalIgnoreCase) && float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out f)) maxGyroThreshold = f;
                        else if (k.Equals("GYRO_ON", StringComparison.OrdinalIgnoreCase) && (v.Equals("LSL", StringComparison.OrdinalIgnoreCase) || v.Equals("RSL", StringComparison.OrdinalIgnoreCase))) hasGyroOnSl = true;
                        else if (k.Equals("LEFT_STICK_MODE", StringComparison.OrdinalIgnoreCase) && v.IndexOf("SCROLL", StringComparison.OrdinalIgnoreCase) >= 0) stickScrollMode = true;
                        else if (k.Equals("LUP", StringComparison.OrdinalIgnoreCase) && v.IndexOf("SCROLLUP", StringComparison.OrdinalIgnoreCase) >= 0) stickScrollMode = true;
                        else if (bindings.ContainsKey(k)) bindings[k] = v;
                    }
                }

                // Determine Gyro Mode:
                // 0 = Always On, 1 = SL Hold Only, 2 = Off (Stick Only)
                int gyroMode = 1;
                float activeMinGyro = 10f;
                float activeMaxGyro = 18f;

                if (hasGyroOnSl && baseMinGyro > 0f)
                {
                    gyroMode = 1;
                    activeMinGyro = baseMinGyro;
                    activeMaxGyro = baseMaxGyro > 0f ? baseMaxGyro : baseMinGyro;
                }
                else if (holdMinGyro > 0f)
                {
                    gyroMode = 1;
                    activeMinGyro = holdMinGyro;
                    activeMaxGyro = holdMaxGyro > 0f ? holdMaxGyro : holdMinGyro;
                }
                else if (baseMinGyro > 0f)
                {
                    gyroMode = 0;
                    activeMinGyro = baseMinGyro;
                    activeMaxGyro = baseMaxGyro > 0f ? baseMaxGyro : baseMinGyro;
                }
                else
                {
                    gyroMode = 2;
                    activeMinGyro = 0f;
                    activeMaxGyro = 0f;
                }

                SetConColor(ConsoleColor.Cyan);
                WriteConLine("==========================================================================");
                WriteConLine("  🎮 스위치 1 조이콘 스마트 리모컨 엔진 (SDL2.dll 직접 구동 모드)");
                WriteConLine("  * JoyShockMapper.exe 외부 프로세스 의존성이 완전히 제거되었습니다.");
                WriteConLine("==========================================================================");
                ResetConColor();
                WriteConLine("  📂 불러온 설정 파일 : " + Path.GetFileName(configPath));
                if (gyroMode == 1)
                {
                    SetConColor(ConsoleColor.Green);
                    WriteConLine(string.Format("  🎯 작동 모드        : [모드 1] 하이브리드 (평소 스틱 마우스 {0} / SL 홀드 시 자이로 {1}~{2} + 스틱 정밀감속 {3})",
                        stickSens, activeMinGyro, activeMaxGyro, slowStickSens));
                }
                else if (gyroMode == 0)
                {
                    SetConColor(ConsoleColor.Yellow);
                    WriteConLine(string.Format("  🎯 작동 모드        : [모드 2] 상시 자이로 포인터 (감도 {0}~{1}) + {2}",
                        activeMinGyro, activeMaxGyro, stickScrollMode ? "아날로그 스틱 휠 스크롤" : ("스틱 마우스 " + stickSens)));
                }
                else
                {
                    SetConColor(ConsoleColor.Magenta);
                    WriteConLine(string.Format("  🕹️ 작동 모드        : [모드 3] 아날로그 스틱 전용 마우스 (감도 {0} / SL 홀드 시 정밀감속 {1})",
                        stickSens, slowStickSens));
                }
                ResetConColor();
                WriteConLine("  💡 안내             : 이 콘솔 창을 닫거나 GUI에서 [마우스 종료]를 누르면 즉시 종료됩니다.");
                WriteConLine("--------------------------------------------------------------------------");

                // Configure SDL2 hints for independent Left/Right Joy-Cons with Gyro
                SDL_SetHint("SDL_JOYSTICK_HIDAPI", "1");
                SDL_SetHint("SDL_JOYSTICK_HIDAPI_SWITCH", "1");
                SDL_SetHint("SDL_JOYSTICK_HIDAPI_JOY_CONS", "1");
                SDL_SetHint("SDL_JOYSTICK_HIDAPI_COMBINE_JOY_CONS", "0");
                SDL_SetHint("SDL_JOYSTICK_HIDAPI_VERTICAL_JOY_CONS", "1");
                SDL_SetHint("SDL_GAMECONTROLLER_USE_BUTTON_LABELS", "0");
                SDL_SetHint("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS", "1");

                int initRes = SDL_Init(SDL_INIT_JOYSTICK | SDL_INIT_GAMECONTROLLER | SDL_INIT_SENSOR);
                if (initRes < 0)
                {
                    SetConColor(ConsoleColor.Red);
                    WriteConLine("[오류] SDL2 초기화에 실패했습니다. (코드: " + initRes + ")");
                    ResetConColor();
                    return;
                }

                WriteConLine("[SDL2] SDL2.dll (x64) 로드 및 센서 서브시스템 초기화 완료.");

                List<ControllerSlot> slots = new List<ControllerSlot>();
                Dictionary<string, ButtonRuntimeState> btnStates = new Dictionary<string, ButtonRuntimeState>(StringComparer.OrdinalIgnoreCase);
                foreach (string k in allKeys)
                {
                    btnStates[k] = new ButtonRuntimeState();
                }

                bool prevSlHeld = false;
                bool gyroToggleOverride = false;
                bool lsrChordUsed = false;
                bool rsrChordUsed = false;
                float remMoveX = 0f, remMoveY = 0f;
                float remScrollY = 0f, remScrollX = 0f;

                Stopwatch sw = Stopwatch.StartNew();
                long lastTickUs = sw.ElapsedTicks;
                long lastScanMs = -5000;

                try
                {
                    while (!_consoleClosing && !stopEvent.WaitOne(0))
                    {
                        long nowMs = sw.ElapsedMilliseconds;
                        if (maxRunMs > 0 && nowMs >= maxRunMs)
                        {
                            WriteConLine("[테스트 완료] 지정된 테스트 시간이 경과하여 정상 종료합니다.");
                            break;
                        }

                        long nowTicks = sw.ElapsedTicks;
                        float dt = (float)(nowTicks - lastTickUs) / Stopwatch.Frequency;
                        if (dt <= 0f || dt > 0.1f) dt = 0.004f;
                        lastTickUs = nowTicks;

                        SDL_PumpEvents();
                        SDL_GameControllerUpdate();

                        // Scan for newly connected or disconnected Joy-Cons every 1000ms
                        if (nowMs - lastScanMs >= 1000)
                        {
                            lastScanMs = nowMs;
                            for (int i = slots.Count - 1; i >= 0; i--)
                            {
                                if (SDL_GameControllerGetAttached(slots[i].Handle) == 0)
                                {
                                    SetConColor(ConsoleColor.DarkYellow);
                                    WriteConLine(string.Format("[연결 해제] {0} 컨트롤러 연결이 해제되었습니다.", slots[i].Name));
                                    ResetConColor();
                                    try { SDL_GameControllerClose(slots[i].Handle); } catch { }
                                    slots.RemoveAt(i);
                                }
                                else
                                {
                                    if (SDL_GameControllerIsSensorEnabled(slots[i].Handle, SDL_SENSOR_GYRO) == 0)
                                    {
                                        SDL_GameControllerSetSensorEnabled(slots[i].Handle, SDL_SENSOR_GYRO, 1);
                                    }
                                }
                            }

                            int numJoy = SDL_NumJoysticks();
                            for (int joyIdx = 0; joyIdx < numJoy; joyIdx++)
                            {
                                bool alreadyOpen = false;
                                for (int s = 0; s < slots.Count; s++)
                                {
                                    if (slots[s].JoystickIndex == joyIdx) { alreadyOpen = true; break; }
                                }
                                if (alreadyOpen) continue;
                                if (!SDL_IsGameController(joyIdx)) continue;

                                IntPtr pad = SDL_GameControllerOpen(joyIdx);
                                if (pad == IntPtr.Zero) continue;

                                int cType = SDL_GameControllerGetType(pad);
                                IntPtr namePtr = SDL_GameControllerName(pad);
                                string cName = namePtr != IntPtr.Zero ? Marshal.PtrToStringAnsi(namePtr) : "Joy-Con";
                                bool isLeft = (cType == 6) || (cName != null && cName.IndexOf("(L)", StringComparison.OrdinalIgnoreCase) >= 0);
                                bool isRight = (cType == 7) || (cName != null && cName.IndexOf("(R)", StringComparison.OrdinalIgnoreCase) >= 0);
                                if (!isLeft && !isRight)
                                {
                                    isLeft = true;
                                    isRight = true;
                                }

                                SDL_GameControllerSetSensorEnabled(pad, SDL_SENSOR_GYRO, 1);

                                ControllerSlot slot = new ControllerSlot
                                {
                                    JoystickIndex = joyIdx,
                                    Handle = pad,
                                    Name = cName ?? "Nintendo Switch Joy-Con",
                                    IsLeft = isLeft,
                                    IsRight = isRight
                                };
                                slots.Add(slot);

                                SetConColor(ConsoleColor.Green);
                                WriteConLine(string.Format("[연결됨] {0} 감지 완료! (타입: {1}, 자이로 센서: 활성화됨)",
                                    slot.Name, isLeft && !isRight ? "좌측 Joy-Con L" : (!isLeft && isRight ? "우측 Joy-Con R" : "통합 컨트롤러")));
                                ResetConColor();
                            }
                        }

                        // Poll raw physical state across connected Joy-Cons
                        Dictionary<string, bool> rawDown = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                        foreach (string k in allKeys) rawDown[k] = false;

                        float rawStickX = 0f, rawStickY = 0f;

                        for (int s = 0; s < slots.Count; s++)
                        {
                            ControllerSlot slot = slots[s];
                            IntPtr pad = slot.Handle;

                            if (slot.IsLeft)
                            {
                                short lx = SDL_GameControllerGetAxis(pad, 0);
                                short ly = SDL_GameControllerGetAxis(pad, 1);
                                float fx = lx / 32767f;
                                float fy = ly / 32767f;
                                if (Math.Abs(fx) > Math.Abs(rawStickX)) rawStickX = fx;
                                if (Math.Abs(fy) > Math.Abs(rawStickY)) rawStickY = fy;

                                if (SDL_GameControllerGetAxis(pad, 4) > 8000) rawDown["ZL"] = true;
                                if (SDL_GameControllerGetButton(pad, 9) != 0) rawDown["L"] = true;
                                if (SDL_GameControllerGetButton(pad, 7) != 0) rawDown["L3"] = true;
                                if (SDL_GameControllerGetButton(pad, 11) != 0) rawDown["UP"] = true;
                                if (SDL_GameControllerGetButton(pad, 12) != 0) rawDown["DOWN"] = true;
                                if (SDL_GameControllerGetButton(pad, 13) != 0) rawDown["LEFT"] = true;
                                if (SDL_GameControllerGetButton(pad, 14) != 0) rawDown["RIGHT"] = true;
                                if (SDL_GameControllerGetButton(pad, 4) != 0) rawDown["-"] = true;
                                if (SDL_GameControllerGetButton(pad, 15) != 0) rawDown["CAPTURE"] = true;
                                if (SDL_GameControllerGetButton(pad, 17) != 0) rawDown["LSL"] = true;
                                if (SDL_GameControllerGetButton(pad, 19) != 0) rawDown["LSR"] = true;
                            }

                            if (slot.IsRight)
                            {
                                short rx = SDL_GameControllerGetAxis(pad, 2);
                                short ry = SDL_GameControllerGetAxis(pad, 3);
                                // Also fallback to axis 0/1 if single vertical Right Joy-Con maps stick to LeftX/LeftY
                                short rxAlt = SDL_GameControllerGetAxis(pad, 0);
                                short ryAlt = SDL_GameControllerGetAxis(pad, 1);
                                float fx = (Math.Abs((int)rx) >= Math.Abs((int)rxAlt)) ? (rx / 32767f) : (rxAlt / 32767f);
                                float fy = (Math.Abs((int)ry) >= Math.Abs((int)ryAlt)) ? (ry / 32767f) : (ryAlt / 32767f);
                                if (Math.Abs(fx) > Math.Abs(rawStickX)) rawStickX = fx;
                                if (Math.Abs(fy) > Math.Abs(rawStickY)) rawStickY = fy;

                                if (SDL_GameControllerGetAxis(pad, 5) > 8000 || (!slot.IsLeft && SDL_GameControllerGetAxis(pad, 4) > 8000)) rawDown["ZR"] = true;
                                if (SDL_GameControllerGetButton(pad, 10) != 0 || (!slot.IsLeft && SDL_GameControllerGetButton(pad, 9) != 0)) rawDown["R"] = true;
                                if (SDL_GameControllerGetButton(pad, 8) != 0 || (!slot.IsLeft && SDL_GameControllerGetButton(pad, 7) != 0)) rawDown["R3"] = true;
                                if (SDL_GameControllerGetButton(pad, 0) != 0) rawDown["S"] = true;
                                if (SDL_GameControllerGetButton(pad, 1) != 0) rawDown["E"] = true;
                                if (SDL_GameControllerGetButton(pad, 2) != 0) rawDown["W"] = true;
                                if (SDL_GameControllerGetButton(pad, 3) != 0) rawDown["N"] = true;
                                if (SDL_GameControllerGetButton(pad, 6) != 0) rawDown["+"] = true;
                                if (SDL_GameControllerGetButton(pad, 5) != 0) rawDown["HOME"] = true;
                                if (SDL_GameControllerGetButton(pad, 18) != 0 || (!slot.IsLeft && SDL_GameControllerGetButton(pad, 17) != 0)) rawDown["RSL"] = true;
                                if (SDL_GameControllerGetButton(pad, 16) != 0 || (!slot.IsLeft && SDL_GameControllerGetButton(pad, 19) != 0)) rawDown["RSR"] = true;
                            }
                        }

                        bool lslHeld = rawDown["LSL"];
                        bool rslHeld = rawDown["RSL"];
                        bool slHeld = lslHeld || rslHeld;

                        if (slHeld != prevSlHeld)
                        {
                            prevSlHeld = slHeld;
                            if (slHeld)
                            {
                                string whichSl = (lslHeld && rslHeld) ? "LSL+RSL" : (lslHeld ? "좌측 SL(LSL)" : "우측 SL(RSL)");
                                SetConColor(ConsoleColor.Green);
                                if (gyroMode == 1)
                                    WriteConLine(string.Format("[SL 홀드 ON] 🎯 {0} 누름 -> 자이로 마우스 활성화 + 스틱 정밀 감속({1}) 작동 중", whichSl, slowStickSens));
                                else
                                    WriteConLine(string.Format("[SL 홀드 ON] 🎯 {0} 누름 -> 스틱 정밀 감속({1}) 작동 중", whichSl, slowStickSens));
                                ResetConColor();
                            }
                            else
                            {
                                SetConColor(ConsoleColor.DarkGray);
                                if (gyroMode == 1)
                                    WriteConLine("[SL 홀드 OFF] ⏹ SL 버튼 뗌 -> 자이로 일시정지 / 기본 스틱 속도 복귀");
                                else
                                    WriteConLine("[SL 홀드 OFF] ⏹ SL 버튼 뗌 -> 기본 스틱 속도 복귀");
                                ResetConColor();
                            }
                        }

                        // Determine if manual CALIBRATE button is currently held
                        bool manualCalibrate = false;
                        foreach (var kv in rawDown)
                        {
                            if (kv.Value && bindings.ContainsKey(kv.Key) && bindings[kv.Key].IndexOf("CALIBRATE", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                manualCalibrate = true;
                                break;
                            }
                        }

                        // Process Gyro Sensor Data
                        bool gyroActive = false;
                        if (gyroMode == 0) gyroActive = !gyroToggleOverride;
                        else if (gyroMode == 1) gyroActive = slHeld || gyroToggleOverride;
                        else gyroActive = gyroToggleOverride;

                        float totalYawDegPerSec = 0f;
                        float totalPitchDegPerSec = 0f;
                        float[] sensorBuf = new float[3];

                        for (int s = 0; s < slots.Count; s++)
                        {
                            ControllerSlot slot = slots[s];
                            if (SDL_GameControllerGetSensorData(slot.Handle, SDL_SENSOR_GYRO, sensorBuf, 3) == 0)
                            {
                                if (float.IsNaN(sensorBuf[0]) || float.IsNaN(sensorBuf[1]) || float.IsNaN(sensorBuf[2])) continue;

                                float gx = sensorBuf[0];
                                float gy = sensorBuf[1];
                                float gz = sensorBuf[2];

                                float magRad = (float)Math.Sqrt(gx * gx + gy * gy + gz * gz);
                                if (manualCalibrate || slot.CalibSamples < 50 || (magRad < 0.025f && !slHeld))
                                {
                                    float alpha = (manualCalibrate || slot.CalibSamples < 50) ? 0.15f : 0.005f;
                                    slot.GyroBias[0] = slot.GyroBias[0] * (1f - alpha) + gx * alpha;
                                    slot.GyroBias[1] = slot.GyroBias[1] * (1f - alpha) + gy * alpha;
                                    slot.GyroBias[2] = slot.GyroBias[2] * (1f - alpha) + gz * alpha;
                                    slot.CalibSamples++;
                                }

                                // If holding SL on a specific Joy-Con, only use that Joy-Con's gyro
                                if (slHeld)
                                {
                                    if (lslHeld && !rslHeld && !slot.IsLeft) continue;
                                    if (rslHeld && !lslHeld && !slot.IsRight) continue;
                                }

                                float cx = (gx - slot.GyroBias[0]) * 57.2957795f;
                                float cy = (gy - slot.GyroBias[1]) * 57.2957795f;
                                float cz = (gz - slot.GyroBias[2]) * 57.2957795f;

                                // Natural one-hand vertical Joy-Con pointer mapping (Yaw + slight Roll coupling for X, Pitch for Y)
                                float yaw = -(cy + cz * 0.30f);
                                float pitch = -cx;

                                totalYawDegPerSec += yaw;
                                totalPitchDegPerSec += pitch;
                            }
                        }

                        if (gyroActive && activeMaxGyro > 0f)
                        {
                            float gyroSpeed = (float)Math.Sqrt(totalYawDegPerSec * totalYawDegPerSec + totalPitchDegPerSec * totalPitchDegPerSec);
                            if (gyroSpeed > minGyroThreshold)
                            {
                                float t = 0f;
                                if (maxGyroThreshold > minGyroThreshold)
                                {
                                    t = (gyroSpeed - minGyroThreshold) / (maxGyroThreshold - minGyroThreshold);
                                    if (t < 0f) t = 0f;
                                    if (t > 1f) t = 1f;
                                }
                                float sens = activeMinGyro + (activeMaxGyro - activeMinGyro) * t;
                                float scale = (gyroSpeed - minGyroThreshold) / gyroSpeed;
                                float effYaw = totalYawDegPerSec * scale;
                                float effPitch = totalPitchDegPerSec * scale;

                                remMoveX += effYaw * realWorldCalib * sens * 0.022f * dt;
                                remMoveY += effPitch * realWorldCalib * sens * 0.022f * dt;
                            }
                        }

                        // Process Analog Stick (Mouse Movement or Scroll Wheel)
                        float stickMag = (float)Math.Sqrt(rawStickX * rawStickX + rawStickY * rawStickY);
                        float maxZone = 1.0f - deadzoneOuter;
                        if (maxZone <= deadzoneInner + 0.05f) maxZone = deadzoneInner + 0.05f;

                        if (stickMag > deadzoneInner)
                        {
                            float norm = (stickMag - deadzoneInner) / (maxZone - deadzoneInner);
                            if (norm > 1f) norm = 1f;
                            float curved = (float)Math.Pow(norm, stickPower);
                            float dirX = rawStickX / stickMag;
                            float dirY = rawStickY / stickMag;

                            if (stickScrollMode)
                            {
                                float scrollFactor = slHeld ? 360f : 900f;
                                remScrollY += (-dirY * curved) * scrollFactor * dt;
                                remScrollX += (dirX * curved) * (scrollFactor * 0.7f) * dt;

                                int wheelY = (int)remScrollY;
                                if (Math.Abs(wheelY) >= 15)
                                {
                                    remScrollY -= wheelY;
                                    SendMouseWheel(wheelY, false);
                                }
                                int wheelX = (int)remScrollX;
                                if (Math.Abs(wheelX) >= 15)
                                {
                                    remScrollX -= wheelX;
                                    SendMouseWheel(wheelX, true);
                                }
                            }
                            else
                            {
                                float currentStickSens = slHeld ? slowStickSens : stickSens;
                                // Convert JSM STICK_SENS (deg/sec) to pixels/sec
                                float pxPerSec = currentStickSens * 3.2f;
                                remMoveX += dirX * curved * pxPerSec * dt;
                                remMoveY += dirY * curved * pxPerSec * dt;
                            }
                        }
                        else
                        {
                            remScrollX = 0f;
                            remScrollY = 0f;
                        }

                        int moveDx = (int)remMoveX;
                        int moveDy = (int)remMoveY;
                        if (moveDx != 0 || moveDy != 0)
                        {
                            remMoveX -= moveDx;
                            remMoveY -= moveDy;
                            SendMouseMove(moveDx, moveDy);
                        }

                        // Process Buttons & SR Chords
                        bool lsrDown = rawDown["LSR"];
                        bool rsrDown = rawDown["RSR"];
                        if (!lsrDown) lsrChordUsed = false;
                        if (!rsrDown) rsrChordUsed = false;

                        string[] primaryButtons = new[] {
                            "ZL", "ZR", "L", "R", "L3", "R3",
                            "UP", "DOWN", "LEFT", "RIGHT", "N", "S", "W", "E",
                            "-", "+", "CAPTURE", "HOME", "LSR", "RSR"
                        };

                        foreach (string btn in primaryButtons)
                        {
                            bool isDown = rawDown[btn];
                            ButtonRuntimeState st = btnStates[btn];

                            if (isDown && !st.IsDown)
                            {
                                st.IsDown = true;
                                st.DownTickMs = nowMs;
                                st.HoldFired = false;
                                st.LastRepeatTickMs = nowMs;
                                st.ActiveChordKey = null;
                                st.HeldActionToken = null;

                                string effectiveKey = btn;
                                if (lsrDown && btn != "LSR" && bindings.ContainsKey("LSR," + btn) && bindings["LSR," + btn] != "NONE")
                                {
                                    effectiveKey = "LSR," + btn;
                                    lsrChordUsed = true;
                                }
                                else if (rsrDown && btn != "RSR" && bindings.ContainsKey("RSR," + btn) && bindings["RSR," + btn] != "NONE")
                                {
                                    effectiveKey = "RSR," + btn;
                                    rsrChordUsed = true;
                                }
                                st.ActiveChordKey = effectiveKey;

                                string bindExpr = bindings.ContainsKey(effectiveKey) ? bindings[effectiveKey] : "NONE";
                                OnButtonDown(effectiveKey, bindExpr, st, ref gyroToggleOverride);
                            }
                            else if (isDown && st.IsDown)
                            {
                                string effectiveKey = st.ActiveChordKey ?? btn;
                                string bindExpr = bindings.ContainsKey(effectiveKey) ? bindings[effectiveKey] : "NONE";
                                bool isChordMod = (btn == "LSR" && lsrChordUsed) || (btn == "RSR" && rsrChordUsed);
                                if (!isChordMod)
                                {
                                    OnButtonHold(effectiveKey, bindExpr, st, nowMs, ref gyroToggleOverride);
                                }
                            }
                            else if (!isDown && st.IsDown)
                            {
                                st.IsDown = false;
                                string effectiveKey = st.ActiveChordKey ?? btn;
                                string bindExpr = bindings.ContainsKey(effectiveKey) ? bindings[effectiveKey] : "NONE";
                                bool isChordMod = (btn == "LSR" && lsrChordUsed) || (btn == "RSR" && rsrChordUsed);
                                OnButtonUp(effectiveKey, bindExpr, st, nowMs, isChordMod, ref gyroToggleOverride);
                                st.ActiveChordKey = null;
                                st.HeldActionToken = null;
                            }
                        }

                        Thread.Sleep(4);
                    }
                }
                finally
                {
                    foreach (var kv in btnStates)
                    {
                        if (!string.IsNullOrEmpty(kv.Value.HeldActionToken))
                        {
                            SendActionUp(kv.Value.HeldActionToken);
                            kv.Value.HeldActionToken = null;
                        }
                    }
                    for (int i = 0; i < slots.Count; i++)
                    {
                        try { SDL_GameControllerClose(slots[i].Handle); } catch { }
                    }
                    try { SDL_Quit(); } catch { }
                    try { activeMutex.ReleaseMutex(); } catch { }
                }
            }
        }

        private static void OnButtonDown(string keyName, string expr, ButtonRuntimeState st, ref bool gyroToggleOverride)
        {
            if (string.IsNullOrWhiteSpace(expr) || expr == "NONE" || expr == "CALIBRATE") return;
            expr = expr.Trim();

            // 1. Repeat bindings: "!SCROLLUP\ SCROLLUP+" or "!VOLUME_UP\ VOLUME_UP+"
            if (expr.EndsWith("+") && expr.Contains("\\"))
            {
                string token = ExtractRepeatToken(expr);
                WriteConLine(string.Format("[버튼 입력] {0} -> {1} (연속 입력 시작)", keyName, token));
                TriggerInstantAction(token, ref gyroToggleOverride);
                return;
            }

            // 2. Shortcut combo: "LCONTROL\ !W\"
            if (expr.Contains("\\") && expr.Contains("!"))
            {
                WriteConLine(string.Format("[단축키 입력] {0} -> {1}", keyName, expr));
                ExecuteShortcutCombo(expr);
                return;
            }

            // 3. Special tap + Win+Tab hold: "ESC' LWINDOWS_ !TAB_"
            if (expr.Contains("LWINDOWS_") && expr.Contains("!TAB_"))
            {
                return; // Handled on release (tap) or hold threshold
            }

            // 4. Tap + Hold pair ("MMOUSE SPACE", "LEFT BMOUSE", "PLAY_PAUSE NONE", "NONE ^GYRO_OFF")
            string[] parts = expr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                return; // Wait to see if tap (<300ms) or hold (>=300ms)
            }

            // 5. Single toggle command
            if (expr == "^GYRO_OFF")
            {
                TriggerInstantAction(expr, ref gyroToggleOverride);
                return;
            }

            // 6. Standard immediate hold button (LMOUSE, RMOUSE, ENTER, UP, etc.)
            WriteConLine(string.Format("[버튼 입력] {0} -> {1}", keyName, expr));
            st.HeldActionToken = expr;
            SendActionDown(expr);
        }

        private static void OnButtonHold(string keyName, string expr, ButtonRuntimeState st, long nowMs, ref bool gyroToggleOverride)
        {
            if (string.IsNullOrWhiteSpace(expr) || expr == "NONE" || expr == "CALIBRATE") return;
            expr = expr.Trim();

            // 1. Continuous repeat while held
            if (expr.EndsWith("+") && expr.Contains("\\"))
            {
                string token = ExtractRepeatToken(expr);
                long interval = token.StartsWith("SCROLL") ? 65 : 95;
                if (nowMs - st.LastRepeatTickMs >= interval)
                {
                    st.LastRepeatTickMs = nowMs;
                    TriggerInstantAction(token, ref gyroToggleOverride);
                }
                return;
            }

            // 2. "ESC' LWINDOWS_ !TAB_" hold
            if (expr.Contains("LWINDOWS_") && expr.Contains("!TAB_"))
            {
                if (!st.HoldFired && (nowMs - st.DownTickMs >= 320))
                {
                    st.HoldFired = true;
                    WriteConLine(string.Format("[길게 누름] {0} -> 작업 보기 (Win + Tab)", keyName));
                    SendKeyStrokeCombo(0x5B, 0x09); // VK_LWIN + VK_TAB
                }
                return;
            }

            // 3. Tap + Hold pair
            string[] parts = expr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                if (!st.HoldFired && (nowMs - st.DownTickMs >= 300))
                {
                    st.HoldFired = true;
                    string holdAct = parts[1].Trim();
                    if (holdAct != "NONE")
                    {
                        WriteConLine(string.Format("[길게 누름] {0} -> {1}", keyName, holdAct));
                        if (holdAct == "^GYRO_OFF")
                        {
                            TriggerInstantAction(holdAct, ref gyroToggleOverride);
                        }
                        else
                        {
                            st.HeldActionToken = holdAct;
                            SendActionDown(holdAct);
                        }
                    }
                }
            }
        }

        private static void OnButtonUp(string keyName, string expr, ButtonRuntimeState st, long nowMs, bool isChordModUsed, ref bool gyroToggleOverride)
        {
            if (!string.IsNullOrEmpty(st.HeldActionToken))
            {
                SendActionUp(st.HeldActionToken);
                st.HeldActionToken = null;
            }

            if (string.IsNullOrWhiteSpace(expr) || expr == "NONE" || expr == "CALIBRATE") return;
            expr = expr.Trim();

            if (expr.Contains("LWINDOWS_") && expr.Contains("!TAB_"))
            {
                if (!st.HoldFired && !isChordModUsed)
                {
                    WriteConLine(string.Format("[짧게 탭] {0} -> ESC", keyName));
                    TriggerInstantAction("ESC", ref gyroToggleOverride);
                }
                return;
            }

            string[] parts = expr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && !expr.Contains("\\"))
            {
                if (!st.HoldFired && !isChordModUsed)
                {
                    string tapAct = parts[0].Trim();
                    if (tapAct != "NONE")
                    {
                        WriteConLine(string.Format("[짧게 탭] {0} -> {1}", keyName, tapAct));
                        TriggerInstantAction(tapAct, ref gyroToggleOverride);
                    }
                }
            }
        }

        private static string ExtractRepeatToken(string expr)
        {
            int idx = expr.LastIndexOf(' ');
            string last = idx >= 0 ? expr.Substring(idx + 1) : expr;
            return last.TrimEnd('+').TrimStart('!').TrimEnd('\\');
        }

        private static void ExecuteShortcutCombo(string expr)
        {
            string[] parts = expr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                string mod = parts[0].Trim('\\', '!', '_', '+');
                string key = parts[1].Trim('\\', '!', '_', '+');
                ushort vkMod = MapTokenToVk(mod);
                ushort vkKey = MapTokenToVk(key);
                if (vkMod != 0 && vkKey != 0)
                {
                    SendKeyStrokeCombo(vkMod, vkKey);
                }
            }
        }

        private static void TriggerInstantAction(string token, ref bool gyroToggleOverride)
        {
            if (string.IsNullOrEmpty(token) || token == "NONE") return;
            if (token == "^GYRO_OFF")
            {
                gyroToggleOverride = !gyroToggleOverride;
                SetConColor(ConsoleColor.Yellow);
                WriteConLine("[자이로 토글] 상시 자이로 상태 전환 -> " + (gyroToggleOverride ? "ON" : "OFF"));
                ResetConColor();
                return;
            }
            if (token == "SCROLLUP")
            {
                SendMouseWheel(120, false);
                return;
            }
            if (token == "SCROLLDOWN")
            {
                SendMouseWheel(-120, false);
                return;
            }
            SendActionDown(token);
            Thread.Sleep(12);
            SendActionUp(token);
        }

        private static void SendActionDown(string token)
        {
            if (token == "LMOUSE") { SendMouseButton(MOUSEEVENTF_LEFTDOWN, 0); return; }
            if (token == "RMOUSE") { SendMouseButton(MOUSEEVENTF_RIGHTDOWN, 0); return; }
            if (token == "MMOUSE") { SendMouseButton(MOUSEEVENTF_MIDDLEDOWN, 0); return; }
            if (token == "BMOUSE") { SendMouseButton(MOUSEEVENTF_XDOWN, XBUTTON1); return; }
            if (token == "FMOUSE") { SendMouseButton(MOUSEEVENTF_XDOWN, XBUTTON2); return; }

            ushort vk = MapTokenToVk(token);
            if (vk != 0) SendKey(vk, false);
        }

        private static void SendActionUp(string token)
        {
            if (token == "LMOUSE") { SendMouseButton(MOUSEEVENTF_LEFTUP, 0); return; }
            if (token == "RMOUSE") { SendMouseButton(MOUSEEVENTF_RIGHTUP, 0); return; }
            if (token == "MMOUSE") { SendMouseButton(MOUSEEVENTF_MIDDLEUP, 0); return; }
            if (token == "BMOUSE") { SendMouseButton(MOUSEEVENTF_XUP, XBUTTON1); return; }
            if (token == "FMOUSE") { SendMouseButton(MOUSEEVENTF_XUP, XBUTTON2); return; }

            ushort vk = MapTokenToVk(token);
            if (vk != 0) SendKey(vk, true);
        }

        private static ushort MapTokenToVk(string token)
        {
            if (string.IsNullOrEmpty(token)) return 0;
            token = token.ToUpperInvariant();
            if (token.Length == 1)
            {
                char c = token[0];
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')) return (ushort)c;
            }
            switch (token)
            {
                case "ENTER": return 0x0D;
                case "SPACE": return 0x20;
                case "BACKSPACE": return 0x08;
                case "TAB": return 0x09;
                case "ESC": return 0x1B;
                case "UP": return 0x26;
                case "DOWN": return 0x28;
                case "LEFT": return 0x25;
                case "RIGHT": return 0x27;
                case "PAGEUP": return 0x21;
                case "PAGEDOWN": return 0x22;
                case "HOME": return 0x24;
                case "END": return 0x23;
                case "INSERT": return 0x2D;
                case "DELETE": return 0x2E;
                case "LSHIFT": return 0xA0;
                case "LCONTROL": return 0xA2;
                case "LALT": return 0xA4;
                case "LWINDOWS": return 0x5B;
                case "MUTE": return 0xAD;
                case "VOLUME_DOWN": return 0xAE;
                case "VOLUME_UP": return 0xAF;
                case "PLAY_PAUSE": return 0xB3;
                case "F1": return 0x70;
                case "F2": return 0x71;
                case "F3": return 0x72;
                case "F4": return 0x73;
                case "F5": return 0x74;
                case "F6": return 0x75;
                case "F7": return 0x76;
                case "F8": return 0x77;
                case "F9": return 0x78;
                case "F10": return 0x79;
                case "F11": return 0x7A;
                case "F12": return 0x7B;
                default: return 0;
            }
        }

        private static void SendMouseMove(int dx, int dy)
        {
            INPUT[] inp = new INPUT[1];
            inp[0].type = INPUT_MOUSE;
            inp[0].u.mi.dx = dx;
            inp[0].u.mi.dy = dy;
            inp[0].u.mi.dwFlags = MOUSEEVENTF_MOVE;
            SendInput(1, inp, Marshal.SizeOf(typeof(INPUT)));
        }

        private static void SendMouseButton(uint flags, uint mouseData)
        {
            INPUT[] inp = new INPUT[1];
            inp[0].type = INPUT_MOUSE;
            inp[0].u.mi.dwFlags = flags;
            inp[0].u.mi.mouseData = mouseData;
            SendInput(1, inp, Marshal.SizeOf(typeof(INPUT)));
        }

        private static void SendMouseWheel(int delta, bool horizontal)
        {
            INPUT[] inp = new INPUT[1];
            inp[0].type = INPUT_MOUSE;
            inp[0].u.mi.dwFlags = horizontal ? MOUSEEVENTF_HWHEEL : MOUSEEVENTF_WHEEL;
            inp[0].u.mi.mouseData = unchecked((uint)delta);
            SendInput(1, inp, Marshal.SizeOf(typeof(INPUT)));
        }

        private static void SendKey(ushort vk, bool keyUp)
        {
            INPUT[] inp = new INPUT[1];
            inp[0].type = INPUT_KEYBOARD;
            inp[0].u.ki.wVk = vk;
            inp[0].u.ki.dwFlags = keyUp ? KEYEVENTF_KEYUP : 0u;
            SendInput(1, inp, Marshal.SizeOf(typeof(INPUT)));
        }

        private static void SendKeyStrokeCombo(ushort vkMod, ushort vkKey)
        {
            SendKey(vkMod, false);
            Thread.Sleep(10);
            SendKey(vkKey, false);
            Thread.Sleep(10);
            SendKey(vkKey, true);
            Thread.Sleep(10);
            SendKey(vkMod, true);
        }
    }
}
