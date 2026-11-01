using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

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
            return string.Format("{0}  [{1}]", Label, Code);
        }
    }

    public class BindingRow
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string DefaultValue { get; set; }
        public ComboBox Combo { get; set; }
    }

    public class MainForm : Form
    {
        private string baseDir;
        private string jsmDir;
        private string configPath;
        private List<BindingRow> allRows = new List<BindingRow>();
        private List<PresetItem> presets = new List<PresetItem>();

        private NumericUpDown numStickSens;
        private NumericUpDown numDeadzone;
        private ComboBox cmbGyroMode;
        private NumericUpDown numMinGyro;
        private NumericUpDown numMaxGyro;
        private NumericUpDown numSlowSens;
        private Label lblStatus;

        public MainForm()
        {
            baseDir = AppDomain.CurrentDomain.BaseDirectory;
            jsmDir = Path.Combine(baseDir, "JoyShockMapper", "JoyShockMapper_SDL2_x64");
            configPath = Path.Combine(jsmDir, "GyroConfigs", "JoyCon_Individual_Hybrid.txt");

            InitPresets();
            InitUI();
            LoadCurrentConfig();
        }

        private void InitPresets()
        {
            presets.Add(new PresetItem("마우스 좌클릭 / 드래그", "LMOUSE"));
            presets.Add(new PresetItem("마우스 우클릭", "RMOUSE"));
            presets.Add(new PresetItem("마우스 휠클릭 (길게: 스페이스바)", "MMOUSE SPACE"));
            presets.Add(new PresetItem("마우스 휠클릭 (단독)", "MMOUSE"));
            presets.Add(new PresetItem("마우스 휠 위로 연속 스크롤", @"!SCROLLUP\ SCROLLUP+"));
            presets.Add(new PresetItem("마우스 휠 아래로 연속 스크롤", @"!SCROLLDOWN\ SCROLLDOWN+"));
            presets.Add(new PresetItem("왼쪽 방향키 (길게: 브라우저 뒤로가기)", "LEFT BMOUSE"));
            presets.Add(new PresetItem("오른쪽 방향키 (길게: 브라우저 앞으로가기)", "RIGHT FMOUSE"));
            presets.Add(new PresetItem("브라우저 뒤로가기", "BMOUSE"));
            presets.Add(new PresetItem("브라우저 앞으로가기", "FMOUSE"));
            presets.Add(new PresetItem("엔터 (Enter)", "ENTER"));
            presets.Add(new PresetItem("스페이스바 (Space)", "SPACE"));
            presets.Add(new PresetItem("백스페이스 (Backspace)", "BACKSPACE"));
            presets.Add(new PresetItem("ESC (취소/닫기)", "ESC"));
            presets.Add(new PresetItem("ESC (길게: 작업 보기 Win+Tab)", "ESC' LWINDOWS_ !TAB_"));
            presets.Add(new PresetItem("탭 (Tab)", "TAB"));
            presets.Add(new PresetItem("방향키 위 (UP)", "UP"));
            presets.Add(new PresetItem("방향키 아래 (DOWN)", "DOWN"));
            presets.Add(new PresetItem("방향키 왼쪽 (LEFT)", "LEFT"));
            presets.Add(new PresetItem("방향키 오른쪽 (RIGHT)", "RIGHT"));
            presets.Add(new PresetItem("페이지 업 (PAGEUP)", "PAGEUP"));
            presets.Add(new PresetItem("페이지 다운 (PAGEDOWN)", "PAGEDOWN"));
            presets.Add(new PresetItem("현재 탭 닫기 (Ctrl + W)", @"LCONTROL\ !W\"));
            presets.Add(new PresetItem("새 탭 열기 (Ctrl + T)", @"LCONTROL\ !T\"));
            presets.Add(new PresetItem("복사 (Ctrl + C)", @"LCONTROL\ !C\"));
            presets.Add(new PresetItem("붙여넣기 (Ctrl + V)", @"LCONTROL\ !V\"));
            presets.Add(new PresetItem("실행 취소 (Ctrl + Z)", @"LCONTROL\ !Z\"));
            presets.Add(new PresetItem("바탕화면 보기 (Win + D)", @"LWINDOWS\ !D\"));
            presets.Add(new PresetItem("창 닫기 (Alt + F4)", @"LALT\ !F4\"));
            presets.Add(new PresetItem("미디어 재생 / 일시정지 (조합키 겸용)", "PLAY_PAUSE NONE"));
            presets.Add(new PresetItem("미디어 재생 / 일시정지 (단독)", "PLAY_PAUSE"));
            presets.Add(new PresetItem("볼륨 증가 (연속)", @"!VOLUME_UP\ VOLUME_UP+"));
            presets.Add(new PresetItem("볼륨 감소 (연속)", @"!VOLUME_DOWN\ VOLUME_DOWN+"));
            presets.Add(new PresetItem("음소거 (Mute)", "MUTE"));
            presets.Add(new PresetItem("이전 트랙 (PREV_TRACK)", "PREV_TRACK"));
            presets.Add(new PresetItem("다음 트랙 (NEXT_TRACK)", "NEXT_TRACK"));
            presets.Add(new PresetItem("자이로 토글 / 길게: 자이로 영점 재보정", "CALIBRATE CALIBRATE"));
            presets.Add(new PresetItem("비활성화 (NONE)", "NONE"));
        }

        private void InitUI()
        {
            this.Text = "닌텐도 스위치 1 조이콘 마우스 - 버튼 & 감도 커스텀 설정기";
            this.Size = new Size(920, 740);
            this.MinimumSize = new Size(860, 660);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Malgun Gothic", 9.5f, FontStyle.Regular);
            this.BackColor = Color.FromArgb(245, 247, 250);

            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 64;
            headerPanel.BackColor = Color.FromArgb(30, 41, 59);
            headerPanel.Padding = new Padding(18, 10, 18, 10);

            Label lblTitle = new Label();
            lblTitle.Text = "조이콘 마우스 버튼 & 감도 커스텀 설정";
            lblTitle.Font = new Font("Malgun Gothic", 13f, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(18, 10);

            Label lblSub = new Label();
            lblSub.Text = "원하는 버튼 동작을 목록에서 선택하거나 직접 입력한 뒤 하단의 [저장 및 즉시 적용]을 누르세요.";
            lblSub.Font = new Font("Malgun Gothic", 9f, FontStyle.Regular);
            lblSub.ForeColor = Color.FromArgb(203, 213, 225);
            lblSub.AutoSize = true;
            lblSub.Location = new Point(20, 36);

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSub);

            Panel footerPanel = new Panel();
            footerPanel.Dock = DockStyle.Bottom;
            footerPanel.Height = 76;
            footerPanel.BackColor = Color.White;
            footerPanel.Padding = new Padding(16, 10, 16, 10);

            lblStatus = new Label();
            lblStatus.Text = "준비됨: " + configPath;
            lblStatus.ForeColor = Color.FromArgb(71, 85, 105);
            lblStatus.AutoSize = false;
            lblStatus.Size = new Size(860, 20);
            lblStatus.Location = new Point(18, 6);

            Button btnSaveApply = CreateStyledButton("저장 및 즉시 적용 (마우스 재시작)", Color.FromArgb(37, 99, 235), Color.White, 250);
            btnSaveApply.Location = new Point(18, 30);
            btnSaveApply.Click += (s, e) => SaveConfig(true);

            Button btnSaveOnly = CreateStyledButton("설정만 저장", Color.FromArgb(71, 85, 105), Color.White, 130);
            btnSaveOnly.Location = new Point(278, 30);
            btnSaveOnly.Click += (s, e) => SaveConfig(false);

            Button btnReload = CreateStyledButton("현재 파일 다시 불러오기", Color.FromArgb(226, 232, 240), Color.FromArgb(30, 41, 59), 180);
            btnReload.Location = new Point(418, 30);
            btnReload.Click += (s, e) => LoadCurrentConfig();

            Button btnReset = CreateStyledButton("기본값 복원", Color.FromArgb(254, 226, 226), Color.FromArgb(185, 28, 28), 120);
            btnReset.Location = new Point(608, 30);
            btnReset.Click += (s, e) => ResetToDefaults();

            Button btnStop = CreateStyledButton("조이콘 마우스 종료", Color.FromArgb(241, 245, 249), Color.FromArgb(51, 65, 85), 150);
            btnStop.Location = new Point(738, 30);
            btnStop.Click += (s, e) => StopJsm();

            footerPanel.Controls.Add(lblStatus);
            footerPanel.Controls.Add(btnSaveApply);
            footerPanel.Controls.Add(btnSaveOnly);
            footerPanel.Controls.Add(btnReload);
            footerPanel.Controls.Add(btnReset);
            footerPanel.Controls.Add(btnStop);

            TabControl tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            tabs.Padding = new Point(14, 8);
            tabs.Font = new Font("Malgun Gothic", 10f, FontStyle.Bold);

            TabPage tabRight = new TabPage("우측 조이콘 (R) 버튼");
            TabPage tabLeft = new TabPage("좌측 조이콘 (L) 버튼");
            TabPage tabChord = new TabPage("SL / SR 조합 단축키");
            TabPage tabSens = new TabPage("스틱 & 자이로 감도 설정");

            BuildButtonTab(tabRight, new BindingRow[] {
                new BindingRow { Key = "ZR", Title = "ZR (우측 하단 트리거)", DefaultValue = "LMOUSE" },
                new BindingRow { Key = "R", Title = "R (우측 상단 범퍼)", DefaultValue = "RMOUSE" },
                new BindingRow { Key = "R3", Title = "R3 (우측 스틱 클릭)", DefaultValue = "MMOUSE SPACE" },
                new BindingRow { Key = "N", Title = "X 버튼 (상단 / N)", DefaultValue = @"!SCROLLUP\ SCROLLUP+" },
                new BindingRow { Key = "S", Title = "B 버튼 (하단 / S)", DefaultValue = @"!SCROLLDOWN\ SCROLLDOWN+" },
                new BindingRow { Key = "W", Title = "Y 버튼 (좌측 / W)", DefaultValue = "LEFT BMOUSE" },
                new BindingRow { Key = "E", Title = "A 버튼 (우측 / E)", DefaultValue = "RIGHT FMOUSE" },
                new BindingRow { Key = "+", Title = "+ (플러스 버튼 탭 / 길게)", DefaultValue = "ESC' LWINDOWS_ !TAB_" },
                new BindingRow { Key = "+,+", Title = "+,+ (플러스 더블탭)", DefaultValue = @"LCONTROL\ !W\" },
                new BindingRow { Key = "HOME", Title = "HOME (홈 버튼 탭 / 길게)", DefaultValue = "CALIBRATE CALIBRATE" },
                new BindingRow { Key = "HOME,HOME", Title = "HOME,HOME (홈 더블탭)", DefaultValue = @"LWINDOWS\ !D\" },
                new BindingRow { Key = "RSL", Title = "측면 SL 버튼 (단독 탭)", DefaultValue = "PLAY_PAUSE NONE" }
            });

            BuildButtonTab(tabLeft, new BindingRow[] {
                new BindingRow { Key = "ZL", Title = "ZL (좌측 하단 트리거)", DefaultValue = "LMOUSE" },
                new BindingRow { Key = "L", Title = "L (좌측 상단 범퍼)", DefaultValue = "RMOUSE" },
                new BindingRow { Key = "L3", Title = "L3 (좌측 스틱 클릭)", DefaultValue = "MMOUSE SPACE" },
                new BindingRow { Key = "UP", Title = "십자키 위 (UP)", DefaultValue = @"!SCROLLUP\ SCROLLUP+" },
                new BindingRow { Key = "DOWN", Title = "십자키 아래 (DOWN)", DefaultValue = @"!SCROLLDOWN\ SCROLLDOWN+" },
                new BindingRow { Key = "LEFT", Title = "십자키 왼쪽 (LEFT)", DefaultValue = "LEFT BMOUSE" },
                new BindingRow { Key = "RIGHT", Title = "십자키 오른쪽 (RIGHT)", DefaultValue = "RIGHT FMOUSE" },
                new BindingRow { Key = "-", Title = "- (마이너스 버튼 탭 / 길게)", DefaultValue = "ESC' LWINDOWS_ !TAB_" },
                new BindingRow { Key = "-,-", Title = "-,- (마이너스 더블탭)", DefaultValue = @"LCONTROL\ !W\" },
                new BindingRow { Key = "CAPTURE", Title = "CAPTURE (캡처 버튼 탭 / 길게)", DefaultValue = "CALIBRATE CALIBRATE" },
                new BindingRow { Key = "CAPTURE,CAPTURE", Title = "CAPTURE,CAPTURE (캡처 더블탭)", DefaultValue = @"LWINDOWS\ !D\" },
                new BindingRow { Key = "LSR", Title = "측면 SR 버튼 (단독 탭)", DefaultValue = "PLAY_PAUSE NONE" }
            });

            BuildButtonTab(tabChord, new BindingRow[] {
                new BindingRow { Key = "RSL,N", Title = "[우측] SL + X (상단)", DefaultValue = @"!VOLUME_UP\ VOLUME_UP+" },
                new BindingRow { Key = "RSL,S", Title = "[우측] SL + B (하단)", DefaultValue = @"!VOLUME_DOWN\ VOLUME_DOWN+" },
                new BindingRow { Key = "RSL,W", Title = "[우측] SL + Y (좌측)", DefaultValue = "BMOUSE" },
                new BindingRow { Key = "RSL,E", Title = "[우측] SL + A (우측)", DefaultValue = "FMOUSE" },
                new BindingRow { Key = "RSL,ZR", Title = "[우측] SL + ZR", DefaultValue = "ENTER" },
                new BindingRow { Key = "RSL,R", Title = "[우측] SL + R", DefaultValue = "BACKSPACE" },
                new BindingRow { Key = "RSL,R3", Title = "[우측] SL + 스틱클릭(R3)", DefaultValue = "MUTE" },
                new BindingRow { Key = "LSR,UP", Title = "[좌측] SR + 십자키 위", DefaultValue = @"!VOLUME_UP\ VOLUME_UP+" },
                new BindingRow { Key = "LSR,DOWN", Title = "[좌측] SR + 십자키 아래", DefaultValue = @"!VOLUME_DOWN\ VOLUME_DOWN+" },
                new BindingRow { Key = "LSR,LEFT", Title = "[좌측] SR + 십자키 왼쪽", DefaultValue = "BMOUSE" },
                new BindingRow { Key = "LSR,RIGHT", Title = "[좌측] SR + 십자키 오른쪽", DefaultValue = "FMOUSE" },
                new BindingRow { Key = "LSR,ZL", Title = "[좌측] SR + ZL", DefaultValue = "ENTER" },
                new BindingRow { Key = "LSR,L", Title = "[좌측] SR + L", DefaultValue = "BACKSPACE" },
                new BindingRow { Key = "LSR,L3", Title = "[좌측] SR + 스틱클릭(L3)", DefaultValue = "MUTE" }
            });

            BuildSensTab(tabSens);

            tabs.TabPages.Add(tabRight);
            tabs.TabPages.Add(tabLeft);
            tabs.TabPages.Add(tabChord);
            tabs.TabPages.Add(tabSens);

            this.Controls.Add(tabs);
            this.Controls.Add(footerPanel);
            this.Controls.Add(headerPanel);
        }

        private Button CreateStyledButton(string text, Color back, Color fore, int width)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(width, 36);
            btn.BackColor = back;
            btn.ForeColor = fore;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Malgun Gothic", 9.5f, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private void BuildButtonTab(TabPage page, BindingRow[] rows)
        {
            page.BackColor = Color.White;
            Panel scrollPanel = new Panel();
            scrollPanel.Dock = DockStyle.Fill;
            scrollPanel.AutoScroll = true;
            scrollPanel.Padding = new Padding(20, 14, 20, 14);

            int y = 14;
            foreach (var row in rows)
            {
                Panel rowPanel = new Panel();
                rowPanel.Size = new Size(820, 38);
                rowPanel.Location = new Point(16, y);
                rowPanel.BackColor = (y / 42) % 2 == 0 ? Color.FromArgb(248, 250, 252) : Color.White;

                Label lbl = new Label();
                lbl.Text = row.Title;
                lbl.Font = new Font("Malgun Gothic", 9.5f, FontStyle.Bold);
                lbl.ForeColor = Color.FromArgb(30, 41, 59);
                lbl.AutoSize = false;
                lbl.Size = new Size(270, 28);
                lbl.TextAlign = ContentAlignment.MiddleLeft;
                lbl.Location = new Point(12, 5);

                ComboBox cmb = new ComboBox();
                cmb.Font = new Font("Malgun Gothic", 9.5f, FontStyle.Regular);
                cmb.DropDownStyle = ComboBoxStyle.DropDown;
                cmb.Size = new Size(420, 28);
                cmb.Location = new Point(290, 5);
                foreach (var p in presets)
                {
                    cmb.Items.Add(p);
                }

                Button btnKeyCap = new Button();
                btnKeyCap.Text = "키 직접 입력";
                btnKeyCap.Font = new Font("Malgun Gothic", 8.5f, FontStyle.Regular);
                btnKeyCap.Size = new Size(92, 28);
                btnKeyCap.Location = new Point(718, 5);
                btnKeyCap.BackColor = Color.FromArgb(226, 232, 240);
                btnKeyCap.FlatStyle = FlatStyle.Flat;
                btnKeyCap.FlatAppearance.BorderSize = 0;
                btnKeyCap.Click += (s, e) => CaptureKeyForCombo(cmb, row.Title);

                row.Combo = cmb;
                allRows.Add(row);

                rowPanel.Controls.Add(lbl);
                rowPanel.Controls.Add(cmb);
                rowPanel.Controls.Add(btnKeyCap);
                scrollPanel.Controls.Add(rowPanel);

                y += 42;
            }

            page.Controls.Add(scrollPanel);
        }

        private void CaptureKeyForCombo(ComboBox targetCombo, string buttonTitle)
        {
            using (Form capForm = new Form())
            {
                capForm.Text = "키 입력 대기 - " + buttonTitle;
                capForm.Size = new Size(400, 180);
                capForm.StartPosition = FormStartPosition.CenterParent;
                capForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                capForm.MaximizeBox = false;
                capForm.MinimizeBox = false;
                capForm.KeyPreview = true;

                Label info = new Label();
                info.Text = "배정할 키보드 키나 마우스 버튼을 누르세요.\n(취소하려면 창 닫기)";
                info.Font = new Font("Malgun Gothic", 10f, FontStyle.Bold);
                info.TextAlign = ContentAlignment.MiddleCenter;
                info.Dock = DockStyle.Fill;
                capForm.Controls.Add(info);

                capForm.KeyDown += (s, e) =>
                {
                    string mapped = MapWinKeyToJsm(e.KeyCode);
                    if (!string.IsNullOrEmpty(mapped))
                    {
                        SetComboByCode(targetCombo, mapped);
                        e.Handled = true;
                        capForm.DialogResult = DialogResult.OK;
                        capForm.Close();
                    }
                };

                capForm.ShowDialog(this);
            }
        }

        private string MapWinKeyToJsm(Keys key)
        {
            if (key >= Keys.A && key <= Keys.Z) return key.ToString();
            if (key >= Keys.D0 && key <= Keys.D9) return ((int)(key - Keys.D0)).ToString();
            if (key >= Keys.F1 && key <= Keys.F12) return key.ToString();
            switch (key)
            {
                case Keys.Return: return "ENTER";
                case Keys.Space: return "SPACE";
                case Keys.Back: return "BACKSPACE";
                case Keys.Escape: return "ESC";
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
                case Keys.RShiftKey: return "RSHIFT";
                case Keys.ControlKey:
                case Keys.LControlKey: return "LCONTROL";
                case Keys.RControlKey: return "RCONTROL";
                case Keys.Menu:
                case Keys.LMenu: return "LALT";
                case Keys.RMenu: return "RALT";
                case Keys.LWin: return "LWINDOWS";
                default: return null;
            }
        }

        private void BuildSensTab(TabPage page)
        {
            page.BackColor = Color.White;
            Panel p = new Panel();
            p.Dock = DockStyle.Fill;
            p.Padding = new Padding(28);

            Font boldFont = new Font("Malgun Gothic", 10f, FontStyle.Bold);
            Font regFont = new Font("Malgun Gothic", 9.5f, FontStyle.Regular);

            int y = 20;

            Label lblStickHdr = new Label { Text = "1. 아날로그 스틱 마우스 커서 설정", Font = boldFont, AutoSize = true, Location = new Point(20, y) };
            p.Controls.Add(lblStickHdr);
            y += 34;

            p.Controls.Add(new Label { Text = "기본 스틱 마우스 속도 (STICK_SENS):", Font = regFont, Size = new Size(280, 26), Location = new Point(36, y) });
            numStickSens = new NumericUpDown { Minimum = 50, Maximum = 1500, Value = 360, Increment = 20, Font = regFont, Size = new Size(120, 28), Location = new Point(330, y) };
            p.Controls.Add(numStickSens);
            y += 38;

            p.Controls.Add(new Label { Text = "SR/SL 정밀 모드 시 스틱 속도:", Font = regFont, Size = new Size(280, 26), Location = new Point(36, y) });
            numSlowSens = new NumericUpDown { Minimum = 30, Maximum = 800, Value = 140, Increment = 10, Font = regFont, Size = new Size(120, 28), Location = new Point(330, y) };
            p.Controls.Add(numSlowSens);
            y += 38;

            p.Controls.Add(new Label { Text = "스틱 중앙 데드존 (STICK_DEADZONE_INNER):", Font = regFont, Size = new Size(280, 26), Location = new Point(36, y) });
            numDeadzone = new NumericUpDown { Minimum = 0.05M, Maximum = 0.50M, DecimalPlaces = 2, Increment = 0.02M, Value = 0.18M, Font = regFont, Size = new Size(120, 28), Location = new Point(330, y) };
            p.Controls.Add(numDeadzone);
            y += 52;

            Label lblGyroHdr = new Label { Text = "2. 자이로(모션) 에어마우스 설정", Font = boldFont, AutoSize = true, Location = new Point(20, y) };
            p.Controls.Add(lblGyroHdr);
            y += 34;

            p.Controls.Add(new Label { Text = "자이로 활성화 방식:", Font = regFont, Size = new Size(280, 26), Location = new Point(36, y) });
            cmbGyroMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = regFont, Size = new Size(400, 28), Location = new Point(330, y) };
            cmbGyroMode.Items.Add("측면 SR(우) / SL(좌) 누르는 동안만 자이로 켜기 (포인터 고정 추천)");
            cmbGyroMode.Items.Add("항상 자이로 켜기 (상시 에어마우스 + 스틱 동시 사용)");
            cmbGyroMode.Items.Add("자이로 완전히 끄기 (아날로그 스틱 전용)");
            cmbGyroMode.SelectedIndex = 0;
            p.Controls.Add(cmbGyroMode);
            y += 38;

            p.Controls.Add(new Label { Text = "자이로 최소 감도 (MIN_GYRO_SENS):", Font = regFont, Size = new Size(280, 26), Location = new Point(36, y) });
            numMinGyro = new NumericUpDown { Minimum = 1, Maximum = 50, Value = 7, Increment = 1, Font = regFont, Size = new Size(120, 28), Location = new Point(330, y) };
            p.Controls.Add(numMinGyro);
            y += 38;

            p.Controls.Add(new Label { Text = "자이로 최대 감도 (MAX_GYRO_SENS):", Font = regFont, Size = new Size(280, 26), Location = new Point(36, y) });
            numMaxGyro = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 15, Increment = 1, Font = regFont, Size = new Size(120, 28), Location = new Point(330, y) };
            p.Controls.Add(numMaxGyro);

            page.Controls.Add(p);
        }

        private void SetComboByCode(ComboBox cmb, string code)
        {
            code = (code ?? "").Trim();
            foreach (PresetItem item in cmb.Items)
            {
                if (string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase))
                {
                    cmb.SelectedItem = item;
                    return;
                }
            }
            cmb.Text = code;
        }

        private string GetCodeFromCombo(ComboBox cmb)
        {
            PresetItem selected = cmb.SelectedItem as PresetItem;
            if (selected != null) return selected.Code;

            string txt = (cmb.Text ?? "").Trim();
            int lb = txt.LastIndexOf('[');
            int rb = txt.LastIndexOf(']');
            if (lb >= 0 && rb > lb)
            {
                return txt.Substring(lb + 1, rb - lb - 1).Trim();
            }
            return string.IsNullOrEmpty(txt) ? "NONE" : txt;
        }

        private void ResetToDefaults()
        {
            foreach (var row in allRows)
            {
                SetComboByCode(row.Combo, row.DefaultValue);
            }
            numStickSens.Value = 360;
            numSlowSens.Value = 140;
            numDeadzone.Value = 0.18M;
            cmbGyroMode.SelectedIndex = 0;
            numMinGyro.Value = 7;
            numMaxGyro.Value = 15;
            lblStatus.Text = "기본값으로 복원되었습니다. [저장 및 즉시 적용]을 누르면 반영됩니다.";
        }

        private void LoadCurrentConfig()
        {
            ResetToDefaults();
            if (!File.Exists(configPath))
            {
                lblStatus.Text = "설정 파일이 없어 기본값을 표시합니다: " + configPath;
                return;
            }

            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string rawLine in File.ReadAllLines(configPath, Encoding.ASCII))
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                int eq = line.IndexOf('=');
                if (eq <= 0) continue;
                string k = line.Substring(0, eq).Trim();
                string v = line.Substring(eq + 1).Trim();
                map[k] = v;
            }

            foreach (var row in allRows)
            {
                string val;
                if (map.TryGetValue(row.Key, out val))
                {
                    SetComboByCode(row.Combo, val);
                }
            }

            decimal dVal;
            string sVal;
            if (map.TryGetValue("STICK_SENS", out sVal) && decimal.TryParse(sVal, NumberStyles.Any, CultureInfo.InvariantCulture, out dVal))
                numStickSens.Value = Math.Max(numStickSens.Minimum, Math.Min(numStickSens.Maximum, dVal));
            if (map.TryGetValue("RSR,STICK_SENS", out sVal) && decimal.TryParse(sVal, NumberStyles.Any, CultureInfo.InvariantCulture, out dVal))
                numSlowSens.Value = Math.Max(numSlowSens.Minimum, Math.Min(numSlowSens.Maximum, dVal));
            if (map.TryGetValue("STICK_DEADZONE_INNER", out sVal) && decimal.TryParse(sVal, NumberStyles.Any, CultureInfo.InvariantCulture, out dVal))
                numDeadzone.Value = Math.Max(numDeadzone.Minimum, Math.Min(numDeadzone.Maximum, dVal));

            decimal baseMin = 0, chordMin = 7, chordMax = 15;
            if (map.TryGetValue("MIN_GYRO_SENS", out sVal)) decimal.TryParse(sVal, NumberStyles.Any, CultureInfo.InvariantCulture, out baseMin);
            if (map.TryGetValue("RSR,MIN_GYRO_SENS", out sVal)) decimal.TryParse(sVal, NumberStyles.Any, CultureInfo.InvariantCulture, out chordMin);
            if (map.TryGetValue("RSR,MAX_GYRO_SENS", out sVal)) decimal.TryParse(sVal, NumberStyles.Any, CultureInfo.InvariantCulture, out chordMax);

            if (baseMin > 0)
            {
                cmbGyroMode.SelectedIndex = 1;
                numMinGyro.Value = Math.Max(numMinGyro.Minimum, Math.Min(numMinGyro.Maximum, baseMin));
                if (map.TryGetValue("MAX_GYRO_SENS", out sVal) && decimal.TryParse(sVal, NumberStyles.Any, CultureInfo.InvariantCulture, out dVal))
                    numMaxGyro.Value = Math.Max(numMaxGyro.Minimum, Math.Min(numMaxGyro.Maximum, dVal));
            }
            else if (chordMin > 0)
            {
                cmbGyroMode.SelectedIndex = 0;
                numMinGyro.Value = Math.Max(numMinGyro.Minimum, Math.Min(numMinGyro.Maximum, chordMin));
                numMaxGyro.Value = Math.Max(numMaxGyro.Minimum, Math.Min(numMaxGyro.Maximum, chordMax));
            }
            else
            {
                cmbGyroMode.SelectedIndex = 2;
            }

            lblStatus.Text = "현재 설정 파일을 불러왔습니다: " + Path.GetFileName(configPath);
        }

        private void SaveConfig(bool restartJsm)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configPath));
                int gyroMode = cmbGyroMode.SelectedIndex;
                int minGyro = (int)numMinGyro.Value;
                int maxGyro = (int)numMaxGyro.Value;

                int baseMinGyro = (gyroMode == 1) ? minGyro : 0;
                int baseMaxGyro = (gyroMode == 1) ? maxGyro : 0;
                int holdMinGyro = (gyroMode == 2) ? 0 : minGyro;
                int holdMaxGyro = (gyroMode == 2) ? 0 : maxGyro;

                Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var r in allRows)
                {
                    values[r.Key] = GetCodeFromCombo(r.Combo);
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("# [Custom Profile] Switch 1 Left/Right Joy-Con Individual Hybrid Mouse");
                sb.AppendLine("RESET_MAPPINGS");
                sb.AppendLine("AUTOLOAD = OFF");
                sb.AppendLine("AUTO_CALIBRATE_GYRO = ON");
                sb.AppendLine();
                sb.AppendLine("# --- OS Mouse Calibration ---");
                sb.AppendLine("REAL_WORLD_CALIBRATION = 5.3333");
                sb.AppendLine("IN_GAME_SENS = 1");
                sb.AppendLine("COUNTER_OS_MOUSE_SPEED");
                sb.AppendLine();
                sb.AppendLine("# --- Gyro Air-Mouse ---");
                sb.AppendLine("GYRO_SPACE = PLAYER_TURN");
                sb.AppendLine("MIN_GYRO_SENS = " + baseMinGyro);
                sb.AppendLine("MAX_GYRO_SENS = " + baseMaxGyro);
                sb.AppendLine("MIN_GYRO_THRESHOLD = 5");
                sb.AppendLine("MAX_GYRO_THRESHOLD = 75");
                sb.AppendLine("GYRO_SMOOTH_THRESHOLD = 5");
                sb.AppendLine("GYRO_CUTOFF_RECOVERY = 5");
                sb.AppendLine();
                sb.AppendLine("# --- Analog Stick Mouse Movement ---");
                sb.AppendLine("LEFT_STICK_MODE = AIM");
                sb.AppendLine("RIGHT_STICK_MODE = AIM");
                sb.AppendLine("STICK_SENS = " + ((int)numStickSens.Value));
                sb.AppendLine("STICK_POWER = 1.5");
                sb.AppendLine("STICK_ACCELERATION_RATE = 1.2");
                sb.AppendLine("STICK_ACCELERATION_CAP = 2.5");
                sb.AppendLine("STICK_DEADZONE_INNER = " + numDeadzone.Value.ToString("0.00", CultureInfo.InvariantCulture));
                sb.AppendLine("STICK_DEADZONE_OUTER = 0.10");
                sb.AppendLine();
                sb.AppendLine("# --- Mouse Clicks & Stick Clicks ---");
                sb.AppendLine("ZL = " + values["ZL"]);
                sb.AppendLine("ZR = " + values["ZR"]);
                sb.AppendLine("L  = " + values["L"]);
                sb.AppendLine("R  = " + values["R"]);
                sb.AppendLine("L3 = " + values["L3"]);
                sb.AppendLine("R3 = " + values["R3"]);
                sb.AppendLine();
                sb.AppendLine("# --- Face Buttons & D-Pad ---");
                sb.AppendLine("UP    = " + values["UP"]);
                sb.AppendLine("DOWN  = " + values["DOWN"]);
                sb.AppendLine("LEFT  = " + values["LEFT"]);
                sb.AppendLine("RIGHT = " + values["RIGHT"]);
                sb.AppendLine("N     = " + values["N"]);
                sb.AppendLine("S     = " + values["S"]);
                sb.AppendLine("W     = " + values["W"]);
                sb.AppendLine("E     = " + values["E"]);
                sb.AppendLine();
                sb.AppendLine("# --- Precision & Gyro Hold Mode (LSL / RSR) ---");
                sb.AppendLine("LSL,STICK_SENS = " + ((int)numSlowSens.Value));
                sb.AppendLine("LSL,MIN_GYRO_SENS = " + holdMinGyro);
                sb.AppendLine("LSL,MAX_GYRO_SENS = " + holdMaxGyro);
                sb.AppendLine("RSR,STICK_SENS = " + ((int)numSlowSens.Value));
                sb.AppendLine("RSR,MIN_GYRO_SENS = " + holdMinGyro);
                sb.AppendLine("RSR,MAX_GYRO_SENS = " + holdMaxGyro);
                sb.AppendLine();
                sb.AppendLine("# --- Media & Shortcut Chord Mode (LSR / RSL) ---");
                sb.AppendLine("LSR = " + values["LSR"]);
                sb.AppendLine("LSR,UP    = " + values["LSR,UP"]);
                sb.AppendLine("LSR,DOWN  = " + values["LSR,DOWN"]);
                sb.AppendLine("LSR,LEFT  = " + values["LSR,LEFT"]);
                sb.AppendLine("LSR,RIGHT = " + values["LSR,RIGHT"]);
                sb.AppendLine("LSR,ZL    = " + values["LSR,ZL"]);
                sb.AppendLine("LSR,L     = " + values["LSR,L"]);
                sb.AppendLine("LSR,L3    = " + values["LSR,L3"]);
                sb.AppendLine();
                sb.AppendLine("RSL = " + values["RSL"]);
                sb.AppendLine("RSL,N     = " + values["RSL,N"]);
                sb.AppendLine("RSL,S     = " + values["RSL,S"]);
                sb.AppendLine("RSL,W     = " + values["RSL,W"]);
                sb.AppendLine("RSL,E     = " + values["RSL,E"]);
                sb.AppendLine("RSL,ZR    = " + values["RSL,ZR"]);
                sb.AppendLine("RSL,R     = " + values["RSL,R"]);
                sb.AppendLine("RSL,R3    = " + values["RSL,R3"]);
                sb.AppendLine();
                sb.AppendLine("# --- System Shortcuts ---");
                sb.AppendLine("-   = " + values["-"]);
                sb.AppendLine("-,- = " + values["-,-"]);
                sb.AppendLine("+   = " + values["+"]);
                sb.AppendLine("+,+ = " + values["+,+"]);
                sb.AppendLine("CAPTURE         = " + values["CAPTURE"]);
                sb.AppendLine("CAPTURE,CAPTURE = " + values["CAPTURE,CAPTURE"]);
                sb.AppendLine("HOME            = " + values["HOME"]);
                sb.AppendLine("HOME,HOME       = " + values["HOME,HOME"]);

                File.WriteAllText(configPath, sb.ToString(), Encoding.ASCII);

                if (restartJsm)
                {
                    File.WriteAllText(Path.Combine(jsmDir, "OnStartup.txt"), "GyroConfigs/JoyCon_Individual_Hybrid.txt\r\n", Encoding.ASCII);
                    foreach (var proc in Process.GetProcessesByName("JoyShockMapper"))
                    {
                        try { proc.Kill(); proc.WaitForExit(1000); } catch { }
                    }
                    string exePath = Path.Combine(jsmDir, "JoyShockMapper.exe");
                    if (File.Exists(exePath))
                    {
                        ProcessStartInfo psi = new ProcessStartInfo(exePath);
                        psi.WorkingDirectory = jsmDir;
                        Process.Start(psi);
                        lblStatus.Text = "저장 완료 및 조이콘 마우스(JoyShockMapper)를 재시작하여 즉시 적용했습니다!";
                    }
                    else
                    {
                        lblStatus.Text = "설정을 저장했습니다. (JoyShockMapper.exe 경로를 찾을 수 없음)";
                    }
                }
                else
                {
                    lblStatus.Text = "설정을 파일에 저장했습니다: " + DateTime.Now.ToString("HH:mm:ss");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("설정 저장 중 오류가 발생했습니다:\n" + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StopJsm()
        {
            int killed = 0;
            foreach (var proc in Process.GetProcessesByName("JoyShockMapper"))
            {
                try { proc.Kill(); killed++; } catch { }
            }
            lblStatus.Text = killed > 0 ? "실행 중인 조이콘 마우스 프로세스를 종료했습니다." : "현재 실행 중인 조이콘 마우스 프로세스가 없습니다.";
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
