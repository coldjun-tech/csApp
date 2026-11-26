using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

using MySql.Data.MySqlClient;
using cnData;

namespace ppt
{
    //public class TReptItem
    //{
    //    public string SheetCode { get; set; }
    //    public string SheetCell { get; set; }
    //    public string SheetValue { get; set; }
    //}
    public class TInspItem
    {
        public string OrderNo { get; set; }
        public int OrderSeq { get; set; }
        public DateTime TestDate { get; set; }
        public string CustCode { get; set; }
        public string ItemNo { get; set; }
        public string Plant { get; set; }
        public string Inlet { get; set; }
        public string Outlet { get; set; }
        public string Capct { get; set; }
        public string CapctUnit { get; set; }
        public string Fluid { get; set; }
        public string Maker { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }
        public string Type { get; set; }
        public string SetPress { get; set; }
        public string BackPress { get; set; }
        public string TestPress { get; set; }
        public string CDS { get; set; }
        public string Unit { get; set; }
        public string Reglt { get; set; }
        public string TestRslt { get; set; }
        public string BodyMatl { get; set; }
        public string DiscMatl { get; set; }
        public string ChkCap { get; set; }
        public string ChkBonnet { get; set; }
        public string ChkBody { get; set; }
        public string ChkNozzle { get; set; }
        public string ChkNozzleSeat { get; set; }
        public string ChkDisc { get; set; }
        public string ChkDiscSeat { get; set; }
        public string ChkBellows { get; set; }
        public string ChkSpring { get; set; }
        public string ChkSpindle { get; set; }
        public string ChkLowerRing { get; set; }
        public string ChkUpperRing { get; set; }
        public string PresVals1 { get; set; }
        public string PresVals2 { get; set; }
        public byte[] PresChtImg { get; set; }
    }

    public class TInspData
    {

        public const string _ORD_SEQ = "1019";
        public const string _ITEM_NO = "1001";
        public const string _PLANT = "1002";
        public const string _SET_PRESS = "1003";
        public const string _BACK_PRESS = "1004";
        public const string _CDS = "1005";
        public const string _UNIT = "1006";
        public const string _IN_LET = "1007";
        public const string _OUT_LET = "1008";
        public const string _FLUID = "1009";
        public const string _CAPCT = "1010";
        public const string _CAPCT_UNIT = "1011";
        public const string _MAKER = "1012";
        public const string _TEST_FLUID = "1013";
        public const string _MODEL = "1014";
        public const string _TYPE = "1015";
        public const string _SER_NO = "1016";
        public const string _BODY_MATL = "1017";
        public const string _DISC_MATL = "1018";
        public const string _TEST_DATE = "2001";
        public const string _TEST_YY = "2002";
        public const string _TEST_MM = "2003";
        public const string _TEST_DD = "2004";
        public const string _POPP_PRES1 = "2005";
        public const string _CLOS_PRES1 = "2006";
        public const string _POPP_PRES2 = "2007";
        public const string _CLOS_PRES2 = "2008";
        public const string _PRES_CHT = "2009";
        public const string _CAP_OK = "3001";
        public const string _CAP_NG = "3002";
        public const string _BONNET_OK = "3003";
        public const string _BONNET_NG = "3004";
        public const string _BODY_OK = "3005";
        public const string _BODY_NG = "3006";
        public const string _NOZZlE_OK = "3007";
        public const string _NOZZlE_NG = "3008";
        public const string _NOZZlE_SEAT1 = "3009";
        public const string _NOZZlE_SEAT2 = "3010";
        public const string _DISC_OK = "3011";
        public const string _DISC_NG = "3012";
        public const string _DISC_SEAT1 = "3013";
        public const string _DISC_SEAT2 = "3014";
        public const string _BLLOWS_OK = "3015";
        public const string _BLLOWS_NG = "3016";
        public const string _SPRING_OK = "3017";
        public const string _SPRING_NG = "3018";
        public const string _SPINDLE_OK = "3019";
        public const string _SPINDLE_NG = "3020";
        public const string _LOW_RING_OK = "3021";
        public const string _LOW_RING_NG = "3022";
        public const string _UPP_RING_OK = "3023";
        public const string _UPP_RING_NG = "3024";

        private TOptionData _optData = TOptionData.Instance;
        public Dictionary<string, string> ItemCodes = new Dictionary<string, string>();

        private static readonly TInspData _inspData = new TInspData();
        public static TInspData Instance { get { return _inspData; } }
        private TInspData()
        {
            // Report 에서 Column 이름과 Code 를 매칭하기 위한 정보
            ItemCodes.Clear();
            ItemCodes.Add(_ITEM_NO, "ItemNo");
            ItemCodes.Add(_PLANT, "Plant");
            ItemCodes.Add(_SET_PRESS, "SetPress");
            ItemCodes.Add(_BACK_PRESS, "BackPress");
            ItemCodes.Add(_CDS, "CDS");
            ItemCodes.Add(_UNIT, "Unit");
            ItemCodes.Add(_IN_LET, "Inlet");
            ItemCodes.Add(_OUT_LET, "Outlet");
            ItemCodes.Add(_FLUID, "Fluid");
            ItemCodes.Add(_CAPCT, "Capct");
            ItemCodes.Add(_CAPCT_UNIT, "CapctUnit");
            ItemCodes.Add(_MAKER, "Maker");
            ItemCodes.Add(_TEST_FLUID, "TestPress");
            ItemCodes.Add(_MODEL, "Model");
            ItemCodes.Add(_TYPE, "Type");
            ItemCodes.Add(_SER_NO, "Serial");
            ItemCodes.Add(_BODY_MATL, "BodyMatl");
            ItemCodes.Add(_DISC_MATL, "DiscMatl");
            ItemCodes.Add(_ORD_SEQ, "OrderSeq");
            ItemCodes.Add(_TEST_DATE, "TestDate");
            ItemCodes.Add(_TEST_YY, "TestDate");
            ItemCodes.Add(_TEST_MM, "TestDate");
            ItemCodes.Add(_TEST_DD, "TestDate");
            ItemCodes.Add(_POPP_PRES1, "PoppPress1");
            ItemCodes.Add(_CLOS_PRES1, "ClosPress1");
            ItemCodes.Add(_POPP_PRES2, "PoppPress2");
            ItemCodes.Add(_CLOS_PRES2, "ClosPress2");
            ItemCodes.Add(_CAP_OK, "ChkCap");
            ItemCodes.Add(_CAP_NG, "ChkCap");
            ItemCodes.Add(_BONNET_OK, "ChkBonnet");
            ItemCodes.Add(_BONNET_NG, "ChkBonnet");
            ItemCodes.Add(_BODY_OK, "ChkBody");
            ItemCodes.Add(_BODY_NG, "ChkBody");
            ItemCodes.Add(_NOZZlE_OK, "ChkNozzle");
            ItemCodes.Add(_NOZZlE_NG, "ChkNozzle");
            ItemCodes.Add(_NOZZlE_SEAT1, "ChkNozzSeat");
            ItemCodes.Add(_NOZZlE_SEAT2, "ChkNozzSeat");
            ItemCodes.Add(_DISC_OK, "ChkDisc");
            ItemCodes.Add(_DISC_NG, "ChkDisc");
            ItemCodes.Add(_DISC_SEAT1, "ChkDiscSeat");
            ItemCodes.Add(_DISC_SEAT2, "ChkDiscSeat");
            ItemCodes.Add(_BLLOWS_OK, "ChkBellows");
            ItemCodes.Add(_BLLOWS_NG, "ChkBellows");
            ItemCodes.Add(_SPRING_OK, "ChkSpring");
            ItemCodes.Add(_SPRING_NG, "ChkSpring");
            ItemCodes.Add(_SPINDLE_OK, "ChkSpindle");
            ItemCodes.Add(_SPINDLE_NG, "ChkSpindle");
            ItemCodes.Add(_LOW_RING_OK, "ChkLowRing");
            ItemCodes.Add(_LOW_RING_NG, "ChkLowRing");
            ItemCodes.Add(_UPP_RING_OK, "ChkUppRing");
            ItemCodes.Add(_UPP_RING_NG, "ChkUppRing");
            ItemCodes.Add(_PRES_CHT, "PressChtImg");
        }

        ~TInspData()
        {
            //Disconnect();
        }

        // ColumnName, ColunmValue 쌍으로 만든 데이터를 만든다.
        public IEnumerable<dynamic> TableToList(DataTable aTable)
        {
            foreach (DataRow row in aTable.Rows)
            {
                //var vExo = new System.Dynamic.ExpandoObject();
                var vDic = new Dictionary<string, object>();

                foreach (DataColumn col in aTable.Columns)
                {
                    var vColName = col.ColumnName;
                    var vColVal = row[col];

                    if (vColVal == DBNull.Value)
                    {
                        vColVal = null;
                    }

                    vDic[vColName] = vColVal;
                }
                yield return vDic;
            }
        }

        public void GenMeasData(List<double> aMeasValues, double afPopping, double afClosing, double afLeak, int aiLeakPattern)
        {
            PressureCurve.Generate(aMeasValues, afPopping, afClosing, afLeak, aiLeakPattern,
                _optData.StartTime, _optData.PoppTime, _optData.ClosSTime,
                _optData.ClosETime, _optData.LeakTime);
        }

        // OrderData 를 저장할 때 같이 저장되는 InspData 따라서 모든 데이터를 저장하지 않고 발주서에 있는 정보만을 저장한다.
        public async void InsertInspData(TInspItem aInspVal)
        {
            string sCommandText = "INSERT INTO tb_insp_data" +
                                  " ( OrderNo, OrderSeq, TestDate, CustCode, ItemNo, Plant, Inlet, Outlet, Capct, CapctUnit" +
                                  " , Fluid, Maker, Type, SetPress, BackPress, CDS, Unit, BodyMatl, DiscMatl" +
                                  " , ChkCap, ChkBonnet, ChkBody, ChkNozz, ChkNozzSeat, ChkDisc, ChkDiscSeat, ChkBellows, ChkSpring, ChkSpindle, ChkLowRing, ChkUppRing )" +
                                  " VALUES " +
                                  " ( @OrderNo, @OrderSeq, @TestDate, @CustCode, @ItemNo, @Plant, @Inlet, @Outlet, @Capct, @CapctUnit" +
                                  " , @Fluid, @Maker, @Type, @SetPress, @BackPress, @CDS, @Unit, @BodyMatl, @DiscMatl " +
                                  " , @ChkCap, @ChkBonnet, @ChkBody, @ChkNozz, @ChkNozzSeat, @ChkDisc, @ChkDiscSeat, @ChkBellows, @ChkSpring, @ChkSpindle, @ChkLowRing, @ChkUppRing)";

            await Task.Run(() =>
            {
                //string sInspData = JsonConvert.SerializeObject(_inspDatas, Formatting.Indented);
                //byte[] imgBytes;
                //using (MemoryStream msChtImg = new MemoryStream())
                //{
                //    chtPresData.ExportToImage(msChtImg, ImageFormat.Png);
                //    imgBytes = msChtImg.ToArray();
                //}
                // new TDataParameter("@Capct", MySqlDbType.Blob, imgBytes)
                // new TDataParameter("@Fluid", MySqlDbType.String, string.Join(",", _measVals1))

                TDataParameter[] vParams = new TDataParameter[] {
                    new TDataParameter("@OrderNo", MySqlDbType.String, aInspVal.OrderNo),
                    new TDataParameter("@OrderSeq", MySqlDbType.Int32, aInspVal.OrderSeq),
                    new TDataParameter("@TestDate", MySqlDbType.DateTime, DateTime.Now),
                    new TDataParameter("@CustCode", MySqlDbType.String, aInspVal.CustCode),
                    new TDataParameter("@ItemNo", MySqlDbType.String, aInspVal.ItemNo),
                    new TDataParameter("@Plant", MySqlDbType.String, aInspVal.Plant),
                    new TDataParameter("@Inlet", MySqlDbType.String, aInspVal.Inlet),
                    new TDataParameter("@Outlet", MySqlDbType.String, aInspVal.Outlet),
                    new TDataParameter("@Capct", MySqlDbType.String, aInspVal.Capct),
                    new TDataParameter("@CapctUnit", MySqlDbType.String, aInspVal.CapctUnit),
                    new TDataParameter("@Fluid", MySqlDbType.String, aInspVal.Fluid),
                    new TDataParameter("@Maker", MySqlDbType.String, aInspVal.Maker),
                    new TDataParameter("@Type", MySqlDbType.String, aInspVal.Type),
                    new TDataParameter("@SetPress", MySqlDbType.String, aInspVal.SetPress),
                    new TDataParameter("@BackPress", MySqlDbType.String, aInspVal.BackPress),
                    new TDataParameter("@CDS", MySqlDbType.String, aInspVal.CDS),
                    new TDataParameter("@Unit", MySqlDbType.String, aInspVal.Unit),
                    new TDataParameter("@BodyMatl", MySqlDbType.String, aInspVal.BodyMatl),
                    new TDataParameter("@DiscMatl", MySqlDbType.String, aInspVal.DiscMatl),
                    new TDataParameter("@ChkCap", MySqlDbType.String, "OK"),
                    new TDataParameter("@ChkBonnet", MySqlDbType.String, "OK"),
                    new TDataParameter("@ChkBody", MySqlDbType.String, "OK"),
                    new TDataParameter("@ChkNozz", MySqlDbType.String, "OK"),
                    new TDataParameter("@ChkNozzSeat", MySqlDbType.String, "NG"),
                    new TDataParameter("@ChkDisc", MySqlDbType.String, "OK"),
                    new TDataParameter("@ChkDiscSeat", MySqlDbType.String, "NG"),
                    new TDataParameter("@ChkBellows", MySqlDbType.String, "OK"),
                    new TDataParameter("@ChkSpring", MySqlDbType.String, "OK"),
                    new TDataParameter("@ChkSpindle", MySqlDbType.String, "OK"),
                    new TDataParameter("@ChkLowRing", MySqlDbType.String, "OK"),
                    new TDataParameter("@ChkUppRing", MySqlDbType.String, "OK")
                };

                TDataMySql.SetDataCommand(sCommandText, vParams);
            });
        }

    } // class TDataMySql

    // New curves use one sample per second so every configured integer time is represented.
    internal static class PressureCurve
    {
        public static void Generate(List<double> values, double popping, double closing,
            double leak, int pattern, int start, int peak, int closeStart, int closeEnd, int end)
        {
            if (start < 0 || peak <= start || closeStart < peak || closeEnd < closeStart ||
                closeEnd <= peak || end <= closeEnd || end > 120)
                throw new ArgumentException("시간은 0 ≤ 시작 < Popping ≤ Closing 시작 ≤ Closing 종료 < Leak ≤ 120 순서로 설정하고, Closing 종료는 Popping 이후로 설정하세요.");
            if (!IsPressure(popping) || !IsPressure(closing) || closing > popping ||
                (pattern != 0 && (!IsPressure(leak) || leak > closing)))
                throw new ArgumentException("압력은 0 이상으로 입력하고, Closing은 Popping 이하, Leak은 Closing 이하로 설정하세요.");
            if (pattern < 0 || pattern > 2)
                throw new ArgumentException("누설 패턴을 선택하세요.");

            // Validate before replacing an existing curve.
            values.Clear();
            const int steps = 5;
            for (int t = 0; t <= 120; t++)
            {
                double pressure;
                if (t <= start)
                    pressure = 0.0;
                else if (t <= peak)
                {
                    double ratio = (double)(t - start) / (peak - start);
                    // Bounded monotone rise: exact endpoints, no regression overshoot.
                    pressure = t == peak ? popping : popping * Math.Pow(ratio, 1.15);
                }
                else if (t == closeEnd)
                    pressure = closing;
                else if (t <= closeStart)
                    pressure = popping;
                else if (t <= closeEnd)
                {
                    double ratio = (double)(t - closeStart) / (closeEnd - closeStart);
                    pressure = t == closeEnd ? closing : popping + (closing - popping) * ratio;
                }
                else if (pattern == 0)
                    pressure = closing;
                else if (t >= end)
                    pressure = leak;
                else
                {
                    double ratio = (double)(t - closeEnd) / (end - closeEnd);
                    if (pattern == 2)
                        ratio = Math.Floor(ratio * steps) / steps;
                    pressure = closing + (leak - closing) * ratio;
                }
                // Keep input precision, including low MPa values.
                values.Add(pressure);
            }
        }

        private static bool IsPressure(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0.0;
        }
    }

}
