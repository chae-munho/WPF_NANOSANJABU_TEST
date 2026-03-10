using System;
using ActUtlTypeLib;
using plctest05.Models;

namespace plctest05.Services
{
    /// <summary>
    /// MX Component (ActUtlType) 기반 PLC 서비스
    /// - Connection using logical station number
    /// - Logical station number = 0
    /// </summary>
    public class PlcService : IDisposable
    {
        private readonly ActUtlType _plc;
        private bool _isConnected;

        // MX Component에 설정된 Logical Station Number
        private const int LogicalStationNumber = 0;

        public PlcService()
        {
            _plc = new ActUtlType
            {
                ActLogicalStationNumber = LogicalStationNumber
            };
        }

        /// <summary>
        /// PLC 연결
        /// </summary>
        public bool Connect(out int errorCode)
        {
            if (_isConnected)
            {
                errorCode = 0;
                return true;
            }

            _plc.ActLogicalStationNumber = LogicalStationNumber;

            errorCode = _plc.Open();
            _isConnected = (errorCode == 0);
            return _isConnected;
        }

        /// <summary>
        /// PLC 연결(간단)
        /// </summary>
        public bool Connect()
        {
            return Connect(out _);
        }

        /// <summary>
        /// PLC 연결 해제
        /// </summary>
        public void Disconnect()
        {
            if (!_isConnected)
                return;

            try
            {
                _plc.Close();
            }
            finally
            {
                _isConnected = false;
            }
        }

        public bool IsConnected => _isConnected;

        // -------------------------
        // Low-level Read / Write
        // -------------------------

        /// <summary>
        /// 비트 디바이스 읽기 (M, X, Y, L 등)
        /// </summary>
        public bool ReadBit(string address)
        {
            EnsureConnected();

            int value;
            int ret = _plc.GetDevice(address, out value);
            if (ret != 0)
                throw new PlcException($"GetDevice 실패: {address}", ret);

            return value != 0;
        }

        /// <summary>
        /// 워드 디바이스 읽기 (D 등)
        /// </summary>
        public short ReadWord(string address)
        {
            EnsureConnected();

            int value;
            int ret = _plc.GetDevice(address, out value);
            if (ret != 0)
                throw new PlcException($"GetDevice 실패: {address}", ret);

            return unchecked((short)value);
        }

        /// <summary>
        /// 비트 디바이스 쓰기
        /// </summary>
        public void WriteBit(string address, bool on)
        {
            EnsureConnected();

            int ret = _plc.SetDevice(address, on ? 1 : 0);
            if (ret != 0)
                throw new PlcException($"SetDevice 실패: {address}", ret);
        }
        /// <summary>
        /// M3017 ON
        /// </summary>
        public void SetM3017On()
        {
            WriteBit("M3017", true);
        }

        /// <summary>
        /// M3017 OFF
        /// </summary>
        public void SetM3017Off()
        {
            WriteBit("M3017", false);
        }
        /// <summary>
        /// D0 에 에러 코드 쓰기
        /// </summary>
        public void SetD0ErrorCode(short value)
        {
            WriteWord("D0", value);
        }

        /// <summary>
        /// 워드 디바이스 쓰기
        /// </summary>
        public void WriteWord(string address, short value)
        {
            EnsureConnected();

            int ret = _plc.SetDevice(address, value);
            if (ret != 0)
                throw new PlcException($"SetDevice 실패: {address}", ret);
        }
       

        // -------------------------
        // High-level ReadAll
        // -------------------------

        public PlcData ReadAll()
        {
            EnsureConnected();

            PlcData d = new PlcData
            {
                // 설비 상태
                M200_AutoRun = ReadBit("M200"),
                M201_AutoStop = ReadBit("M201"),
                D0_Error = ReadWord("D0"),

                // 초기화
                M122 = ReadBit("M122"),
                M127 = ReadBit("M127"),
                M132 = ReadBit("M132"),
                M137 = ReadBit("M137"),
                M142 = ReadBit("M142"),
                M147 = ReadBit("M147"),
                M152 = ReadBit("M152"),
                M157 = ReadBit("M157"),
                M162 = ReadBit("M162"),
                M167 = ReadBit("M167"),

                // Loading
                M858_GlassLoaded = ReadBit("M858"),
                M863_NanoDone = ReadBit("M863"),
                D10_WorkCount = ReadWord("D10"),

                // Dry
                X07_UpperTray = ReadBit("X7"),
                X08_LowerTray = ReadBit("X8"),
                L1_DryStartUpper = ReadBit("L1"),
                L2_DryStartLower = ReadBit("L2"),
                L3_DryEndUpper = ReadBit("L3"),
                L4_DryEndLower = ReadBit("L4"),

                // Unloading
                D20_StackInput = ReadWord("D20"),
                D22_DottingCount = ReadWord("D22"),
                D26_StackOutCount = ReadWord("D26"),

                M906_StackDone = ReadBit("M906"),
                M991_DotDone = ReadBit("M991"),
                M922_UVRun = ReadBit("M922"),
                M937_StackOut = ReadBit("M937"),

                // 우측 UI용 M3000 ~ M3015
                M3000 = ReadBit("M3000"),
                M3001 = ReadBit("M3001"),
                M3002 = ReadBit("M3002"),
                M3003 = ReadBit("M3003"),
                M3004 = ReadBit("M3004"),
                M3005 = ReadBit("M3005"),
                M3006 = ReadBit("M3006"),
                M3007 = ReadBit("M3007"),
                M3008 = ReadBit("M3008"),
                M3009 = ReadBit("M3009"),
                M3010 = ReadBit("M3010"),
                M3011 = ReadBit("M3011"),
                M3012 = ReadBit("M3012"),
                M3013 = ReadBit("M3013"),
                M3014 = ReadBit("M3014"),
                M3015 = ReadBit("M3015"),
                M3017 = ReadBit("M3017")
            };

            // 위치 번호 D60 ~ D69
            for (int i = 0; i < 10; i++)
            {
                d.PositionIndex[i] = ReadWord($"D{60 + i}");
            }

            // 위치 거리 D100, D102, ... D118
            int[] mmAddresses = { 100, 102, 104, 106, 108, 110, 112, 114, 116, 118 };
            for (int i = 0; i < 10; i++)
            {
                d.PositionMM[i] = ReadWord($"D{mmAddresses[i]}");
            }

            return d;
        }

        private void EnsureConnected()
        {
            if (!_isConnected)
                throw new InvalidOperationException("PLC가 연결되어 있지 않습니다. Connect()를 먼저 호출하세요.");
        }

        public void Dispose()
        {
            Disconnect();
        }
    }

    /// <summary>
    /// PLC 통신 에러를 코드와 함께 던지기 위한 예외
    /// </summary>
    public class PlcException : Exception
    {
        public int ErrorCode { get; }

        public PlcException(string message, int errorCode)
            : base($"{message} (ErrorCode={errorCode})")
        {
            ErrorCode = errorCode;
        }
    }
}