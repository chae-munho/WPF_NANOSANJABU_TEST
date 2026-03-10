using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using plctest05.Services;

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
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("========= 설비 상태 =========");
                sb.AppendLine($"M200 : {d.M200_AutoRun}");
                sb.AppendLine($"M201 : {d.M201_AutoStop}");
                sb.AppendLine($"D0   : {d.D0_Error}");

                sb.AppendLine();
                sb.AppendLine("========= 초기화 상태 =========");
                sb.AppendLine($"M122 : {d.M122}");
                sb.AppendLine($"M127 : {d.M127}");
                sb.AppendLine($"M132 : {d.M132}");
                sb.AppendLine($"M137 : {d.M137}");
                sb.AppendLine($"M142 : {d.M142}");
                sb.AppendLine($"M147 : {d.M147}");
                sb.AppendLine($"M152 : {d.M152}");
                sb.AppendLine($"M157 : {d.M157}");
                sb.AppendLine($"M162 : {d.M162}");
                sb.AppendLine($"M167 : {d.M167}");

                sb.AppendLine();
                sb.AppendLine("========= Loading =========");
                sb.AppendLine($"M858 : {d.M858_GlassLoaded}");
                sb.AppendLine($"M863 : {d.M863_NanoDone}");
                sb.AppendLine($"D10  : {d.D10_WorkCount}");

                sb.AppendLine();
                sb.AppendLine("========= Dry Zone =========");
                sb.AppendLine($"X07  : {d.X07_UpperTray}");
                sb.AppendLine($"X08  : {d.X08_LowerTray}");
                sb.AppendLine($"L1   : {d.L1_DryStartUpper}");
                sb.AppendLine($"L2   : {d.L2_DryStartLower}");
                sb.AppendLine($"L3   : {d.L3_DryEndUpper}");
                sb.AppendLine($"L4   : {d.L4_DryEndLower}");

                sb.AppendLine();
                sb.AppendLine("========= Unloading =========");
                sb.AppendLine($"D20  : {d.D20_StackInput}");
                sb.AppendLine($"D22  : {d.D22_DottingCount}");
                sb.AppendLine($"D26  : {d.D26_StackOutCount}");
                sb.AppendLine($"M906 : {d.M906_StackDone}");
                sb.AppendLine($"M991 : {d.M991_DotDone}");
                sb.AppendLine($"M922 : {d.M922_UVRun}");
                sb.AppendLine($"M937 : {d.M937_StackOut}");

                sb.AppendLine();
                sb.AppendLine("========= Robot Position Index =========");
                for (int i = 0; i < 10; i++)
                {
                    sb.AppendLine($"D{60 + i} : {d.PositionIndex[i]}");
                }

                sb.AppendLine();
                sb.AppendLine("========= Robot Position (mm) =========");
                int[] mmAddr = { 100, 102, 104, 106, 108, 110, 112, 114, 116, 118 };
                for (int i = 0; i < 10; i++)
                {
                    sb.AppendLine($"D{mmAddr[i]} : {d.PositionMM[i]} mm");
                }

                txtOutput.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                timer?.Stop();
                MessageBox.Show($"PLC 데이터 읽기 실패\n{ex.Message}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            timer?.Stop();
            plcService.Dispose();
            base.OnClosed(e);
        }
    }
}