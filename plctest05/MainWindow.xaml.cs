using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using plctest05.Services;
using System.Windows.Controls;

namespace plctest05
{
    public partial class MainWindow : Window
    {
        private readonly PlcService plcService;
        private DispatcherTimer? timer;

        public MainWindow()
        {
            InitializeComponent();
            plcService = new PlcService();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (plcService.IsConnected)
                {
                    MessageBox.Show("이미 PLC에 연결되어 있습니다.");
                    return;
                }

                if (plcService.Connect(out int errorCode))
                {
                    MessageBox.Show("PLC 연결 성공 (Logical Station Number = 0)");

                    timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(1)
                    };
                    timer.Tick += Timer_Tick;
                    timer.Start();
                }
                else
                {
                    MessageBox.Show($"PLC 연결 실패\nErrorCode = {errorCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"연결 중 예외 발생\n{ex.Message}");
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                var d = plcService.ReadAll();
                string d0ErrorText = GetD0ErrorText(d.D0_Error);
                // 왼쪽 기존 출력
                StringBuilder leftSb = new StringBuilder();

                leftSb.AppendLine("========= 설비 상태 =========");
                leftSb.AppendLine($"M200 : {d.M200_AutoRun}");
                leftSb.AppendLine($"M201 : {d.M201_AutoStop}");
                leftSb.AppendLine($"D0   : {d.D0_Error} ({d0ErrorText})");

                leftSb.AppendLine();
                leftSb.AppendLine("========= 초기화 상태 =========");
                leftSb.AppendLine($"M122 : {d.M122}");
                leftSb.AppendLine($"M127 : {d.M127}");
                leftSb.AppendLine($"M132 : {d.M132}");
                leftSb.AppendLine($"M137 : {d.M137}");
                leftSb.AppendLine($"M142 : {d.M142}");
                leftSb.AppendLine($"M147 : {d.M147}");
                leftSb.AppendLine($"M152 : {d.M152}");
                leftSb.AppendLine($"M157 : {d.M157}");
                leftSb.AppendLine($"M162 : {d.M162}");
                leftSb.AppendLine($"M167 : {d.M167}");

                leftSb.AppendLine();
                leftSb.AppendLine("========= Loading =========");
                leftSb.AppendLine($"M858 : {d.M858_GlassLoaded}");
                leftSb.AppendLine($"M863 : {d.M863_NanoDone}");
                leftSb.AppendLine($"D10  : {d.D10_WorkCount}");

                leftSb.AppendLine();
                leftSb.AppendLine("========= Dry Zone =========");
                leftSb.AppendLine($"X07  : {d.X07_UpperTray}");
                leftSb.AppendLine($"X08  : {d.X08_LowerTray}");
                leftSb.AppendLine($"L1   : {d.L1_DryStartUpper}");
                leftSb.AppendLine($"L2   : {d.L2_DryStartLower}");
                leftSb.AppendLine($"L3   : {d.L3_DryEndUpper}");
                leftSb.AppendLine($"L4   : {d.L4_DryEndLower}");

                leftSb.AppendLine();
                leftSb.AppendLine("========= Unloading =========");
                leftSb.AppendLine($"D20  : {d.D20_StackInput}");
                leftSb.AppendLine($"D22  : {d.D22_DottingCount}");
                leftSb.AppendLine($"D26  : {d.D26_StackOutCount}");
                leftSb.AppendLine($"M906 : {d.M906_StackDone}");
                leftSb.AppendLine($"M991 : {d.M991_DotDone}");
                leftSb.AppendLine($"M922 : {d.M922_UVRun}");
                leftSb.AppendLine($"M937 : {d.M937_StackOut}");

                leftSb.AppendLine();
                leftSb.AppendLine("========= Robot Position Index =========");
                for (int i = 0; i < 10; i++)
                {
                    leftSb.AppendLine($"D{60 + i} : {d.PositionIndex[i]}");
                }

                leftSb.AppendLine();
                leftSb.AppendLine("========= Robot Position (mm) =========");
                int[] mmAddr = { 100, 102, 104, 106, 108, 110, 112, 114, 116, 118 };
                for (int i = 0; i < 10; i++)
                {
                    leftSb.AppendLine($"D{mmAddr[i]} : {d.PositionMM[i]} mm");
                }

                txtLeftOutput.Text = leftSb.ToString();

                // 오른쪽 M3000 ~ M3015 출력
                StringBuilder rightSb = new StringBuilder();

                rightSb.AppendLine("========= M3000 ~ M3015 =========");
                rightSb.AppendLine($"M3000 : {d.M3000}");
                rightSb.AppendLine($"M3001 : {d.M3001}");
                rightSb.AppendLine($"M3002 : {d.M3002}");
                rightSb.AppendLine($"M3003 : {d.M3003}");
                rightSb.AppendLine($"M3004 : {d.M3004}");
                rightSb.AppendLine($"M3005 : {d.M3005}");
                rightSb.AppendLine($"M3006 : {d.M3006}");
                rightSb.AppendLine($"M3007 : {d.M3007}");
                rightSb.AppendLine($"M3008 : {d.M3008}");
                rightSb.AppendLine($"M3009 : {d.M3009}");
                rightSb.AppendLine($"M3010 : {d.M3010}");
                rightSb.AppendLine($"M3011 : {d.M3011}");
                rightSb.AppendLine($"M3012 : {d.M3012}");
                rightSb.AppendLine($"M3013 : {d.M3013}");
                rightSb.AppendLine($"M3014 : {d.M3014}");
                rightSb.AppendLine($"M3015 : {d.M3015}");
                rightSb.AppendLine($"M3017 : {d.M3017}");

                txtRightOutput.Text = rightSb.ToString();
            }
            catch (Exception ex)
            {
                timer?.Stop();
                MessageBox.Show($"PLC 데이터 읽기 실패\n{ex.Message}");
            }
        }
        private void M3017On_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!plcService.IsConnected)
                {
                    MessageBox.Show("PLC가 연결되어 있지 않습니다.");
                    return;
                }

                plcService.SetM3017On();
                MessageBox.Show("M3017 ON 완료");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"M3017 ON 실패\n{ex.Message}");
            }
        }

        private void M3017Off_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!plcService.IsConnected)
                {
                    MessageBox.Show("PLC가 연결되어 있지 않습니다.");
                    return;
                }

                plcService.SetM3017Off();
                MessageBox.Show("M3017 OFF 완료");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"M3017 OFF 실패\n{ex.Message}");
            }
        }
        private void D0Write_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!plcService.IsConnected)
                {
                    MessageBox.Show("PLC가 연결되어 있지 않습니다.");
                    return;
                }

                if (cmbD0Error.SelectedItem is not ComboBoxItem selectedItem)
                {
                    MessageBox.Show("D0 값을 선택하세요.");
                    return;
                }

                string content = selectedItem.Content?.ToString() ?? string.Empty;
                string[] parts = content.Split('-');

                if (parts.Length == 0 || !short.TryParse(parts[0].Trim(), out short d0Value))
                {
                    MessageBox.Show("선택된 D0 값을 해석할 수 없습니다.");
                    return;
                }

                plcService.SetD0ErrorCode(d0Value);
                MessageBox.Show($"D0 = {d0Value} 쓰기 완료");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"D0 쓰기 실패\n{ex.Message}");
            }
        }
        private string GetD0ErrorText(short value)
        {
            return value switch
            {
                1 => "L-X Servo Alarm",
                2 => "L-Y Servo Alarm",
                3 => "L-Z Servo Alarm",
                4 => "ELV Servo Alarm",
                5 => "T-X Servo Alarm",
                6 => "T-Y Servo Alarm",
                7 => "U-X Servo Alarm",
                8 => "U-Y Servo Alarm",
                9 => "U-Z Servo Alarm",
                10 => "DDM Servo Alarm",
                11 => "Emergency Stop",
                12 => "Loader Z-Axis is Not in Up state",
                13 => "Unloader Z-Axis is Not in Up state",
                14 => "Loader Z Vacuum Error",
                15 => "Unoader Z Vacuum Error",
                _ => "No Error / Unknown"
            };
        }

        protected override void OnClosed(EventArgs e)
        {
            timer?.Stop();
            plcService.Dispose();
            base.OnClosed(e);
        }
    }
}