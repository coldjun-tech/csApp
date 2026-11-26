using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Drawing.Imaging;

using DevExpress.XtraEditors;
using DevExpress.Skins;
using DevExpress.LookAndFeel;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraCharts;
using MySql.Data.MySqlClient;

using cnData;

namespace ppt
{
    public partial class dlgInspEdit : XtraForm
    {
        //private DataTable _datCheckStds;
        //private DataRow _darCheckStd;
        private bool _isEditing = false;
        private TextEdit _focusTextEdit = null;
        private string _orderNo = "";
        private int _orderSeq = -1;
        private string _custCode = "";

        public string ItemCode = null;
        private TInspData _inspData = TInspData.Instance;
        private List<double> _presValues1 = new List<double>();
        private List<double> _presValues2 = new List<double>();
        private double _presInterval1 = 1.2;
        private double _presInterval2 = 1.2;
        private const string OneSecondPrefix = "1s:";

        public dlgInspEdit()
        {
            InitializeComponent();

            this.Load += (sender, e) =>
            {
                //
            };

            this.Shown += (sender, e) =>
            {
                //selectCheckStds();
            };

            this.FormClosing += (sender, e) =>
            {
                //
            };

            this.FormClosed += (sender, e) =>
            {
                //
            };

            this.btnMeasGen.Click += (sender, e) =>
            {
                try
                {
                    bool first = chePres1.Checked;
                    List<double> values = first ? _presValues1 : _presValues2;
                    _inspData.GenMeasData(values,
                        Convert.ToDouble(first ? edtPoppPress1.EditValue : edtPoppPress2.EditValue),
                        Convert.ToDouble(first ? edtClosPress1.EditValue : edtClosPress2.EditValue),
                        first ? Convert.ToDouble(edtLeakPress.EditValue) : 0.0,
                        first ? edtLeakData.SelectedIndex : 0);
                    if (first)
                        _presInterval1 = 1.0;
                    else
                        _presInterval2 = 1.0;
                    DrawPressureCurve(first ? 0 : 1, values, 1.0);
                }
                catch (ArgumentException ex)
                {
                    XtraMessageBox.Show(ex.Message, "자동생성 설정 확인");
                }
                catch (FormatException)
                {
                    XtraMessageBox.Show("압력을 숫자로 입력하세요.", "자동생성 설정 확인");
                }
            };

            this.chtPresData.CustomDrawAxisLabel += (object sender, CustomDrawAxisLabelEventArgs e) =>
            {
                if (e.Item.Axis is AxisX)
                {
                    double fLabelValue = (double)e.Item.AxisValue;
                    if (fLabelValue == 120)
                    {
                        // 라벨 내용을 비움
                        e.Item.Text = "";
                    }
                }
            };

            this.btnOk.Click += (sender, e) =>
            {
                if (_isEditing)
                {
                    updateInspData();
                }
                else
                {
                    //insertInspData();
                }
                
                DialogResult = DialogResult.OK;
                //this.Close();
            };

            this.btnCancel.Click += (sender, e) =>
            {
                //saveAsExcelFile();
                DialogResult = DialogResult.Cancel;
                //this.Close();
            };
        }

        private void DrawPressureCurve(int seriesIndex, List<double> values, double interval)
        {
            Series series = chtPresData.Series[seriesIndex];
            series.Points.Clear();
            for (int i = 0; i < values.Count; i++)
                series.Points.Add(new SeriesPoint(i * interval, values[i]));
        }

        private static double LoadPressureCurve(string text, List<double> values)
        {
            values.Clear();
            bool oneSecond = text.StartsWith(OneSecondPrefix, StringComparison.Ordinal);
            if (oneSecond)
                text = text.Substring(OneSecondPrefix.Length);
            if (!string.IsNullOrWhiteSpace(text))
            {
                var culture = oneSecond ? System.Globalization.CultureInfo.InvariantCulture
                    : System.Globalization.CultureInfo.CurrentCulture;
                values.AddRange(text.Split(',').Select(s => double.Parse(s.Trim(), culture)));
            }
            return oneSecond ? 1.0 : 1.2;
        }

        private static string SavePressureCurve(List<double> values, double interval)
        {
            if (values.Count == 0)
                return "";
            if (interval == 1.0)
                return OneSecondPrefix + string.Join(",", values.Select(v =>
                    v.ToString("R", System.Globalization.CultureInfo.InvariantCulture)));
            return string.Join(",", values);
        }

        private void textEdit_Properties_Click(object sender, EventArgs e)
        {
            _focusTextEdit = (sender as TextEdit);
            dlgKeypad _dlgKeypad = new dlgKeypad();
            _dlgKeypad.Left = _focusTextEdit.Left + 505;
            _dlgKeypad.Top = _focusTextEdit.Top + 205;
            _dlgKeypad.ShowDialog();
            if (_dlgKeypad.DialogResult == DialogResult.OK)
            {
                _focusTextEdit.EditValue = _dlgKeypad.EditingValue;
            }
        }

        private void setEditEnabled()
        {
            //bool is4T, isPawUse, isTigUse;

            //is4T = edtCatgCode.EditValue.ToString().CompareTo("4T") == 0;
            //isPawUse = cheUseYN.EditValue.ToString().CompareTo("Y") == 0;

            //edtPawInitCurt.Enabled = (is4T && isPawUse);
            //edtPawInitTime.Enabled = (is4T && isPawUse);
            //edtPawUpTime.Enabled = (is4T && isPawUse);
            //edtPawMainCurt.Enabled = (isPawUse);
            //edtPawDownTime.Enabled = (is4T && isPawUse);
            //edtPawCratCurt.Enabled = (is4T && isPawUse);
            //edtPawCratTime.Enabled = (is4T && isPawUse);
            //edtPawMainVolt.Enabled = (isPawUse);

            //edtTigInitCurt.Enabled = (is4T && isTigUse);
            //edtTigInitTime.Enabled = (is4T && isTigUse);
            //edtTigUpTime.Enabled = (is4T && isTigUse);
            //edtTigMainCurt.Enabled = (isTigUse);
            //edtTigDownTime.Enabled = (is4T && isTigUse);
            //edtTigCratCurt.Enabled = (is4T && isTigUse);
            //edtTigCratTime.Enabled = (is4T && isTigUse);
            //edtTigMainVolt.Enabled = (isTigUse);
        }

        public void SetIInspData(DataRow aRow)
        {
            _isEditing = true;

            _orderNo = aRow["OrderNo"].ToString();
            _orderSeq = Convert.ToInt32(aRow["OrderSeq"]);
            _custCode = aRow["CustCode"].ToString();

            edtItemNo.EditValue = aRow["ItemNo"].ToString();
            edtPlant.EditValue = aRow["Plant"].ToString();
            edtMaker.EditValue = aRow["Maker"].ToString();
            edtSetPress.EditValue = aRow["SetPress"].ToString();
            edtBackPress.EditValue = aRow["BackPress"].ToString();
            edtCDS.EditValue = aRow["CDS"].ToString();
            edtUnit.EditValue = aRow["Unit"].ToString();
            edtInLet.EditValue = aRow["Inlet"].ToString();
            edtOutLet.EditValue = aRow["Outlet"].ToString();
            edtFluid.EditValue = aRow["Fluid"].ToString();
            edtCapct.EditValue = aRow["Capct"].ToString();
            edtModel.EditValue = aRow["Model"].ToString();
            edtType.EditValue = aRow["Type"].ToString();
            edtSerialNo.EditValue = aRow["Serial"].ToString();

            edtTestFluid.EditValue = aRow["TestFluid"].ToString();

            bool isOk = aRow["ChkCap"] is "OK";
            cheCapOK.Checked = isOk;
            cheCapNG.Checked = !isOk;

            isOk = aRow["ChkBonnet"] is "OK";
            cheBonnetOK.Checked = isOk;
            cheBonnetNG.Checked = !isOk;

            isOk = aRow["ChkBody"] is "OK";
            cheBodyOK.Checked = isOk;
            cheBodyNG.Checked = !isOk;

            isOk = aRow["ChkNozz"] is "OK";
            cheNozzOK.Checked = isOk;
            cheNozzNG.Checked = !isOk;

            isOk = aRow["ChkNozzSeat"] is "OK";
            cheNozzSeatOK.Checked = isOk;
            cheNozzSeatNG.Checked = !isOk;

            isOk = aRow["ChkDisc"] is "OK";
            cheDiscOK.Checked = isOk;
            cheDiscNG.Checked = !isOk;

            isOk = aRow["ChkDiscSeat"] is "OK";
            cheDiscSeatOK.Checked = isOk;
            cheDiscSeatNG.Checked = !isOk;

            isOk = aRow["ChkBellows"] is "OK";
            cheBellowsOK.Checked = isOk;
            cheBellowsNG.Checked = !isOk;

            isOk = aRow["ChkSpring"] is "OK";
            cheSpringOK.Checked = isOk;
            cheSpringNG.Checked = !isOk;

            isOk = aRow["ChkSpindle"] is "OK";
            cheSpindleOK.Checked = isOk;
            cheSpindleNG.Checked = !isOk;

            isOk = aRow["ChkLowRing"] is "OK";
            cheLowRingOK.Checked = isOk;
            cheLowRingNG.Checked = !isOk;

            isOk = aRow["ChkUppRing"] is "OK";
            cheUppRingOK.Checked = isOk;
            cheUppRingNG.Checked = !isOk;

            _presInterval1 = LoadPressureCurve(aRow["PressVals1"].ToString(), _presValues1);
            _presInterval2 = LoadPressureCurve(aRow["PressVals2"].ToString(), _presValues2);
            DrawPressureCurve(0, _presValues1, _presInterval1);
            DrawPressureCurve(1, _presValues2, _presInterval2);

            //if (aRow["ChkCap"].ToString().CompareTo("OK") == 0)
            //{
            //    cheCapOK.Checked = true;
            //}
            //else
            //{
            //    cheCapNG.Checked = true;
            //}
            //edtTestFluid.EditValue = aRow[""].ToString();
        }

        public void SetItemData(params string[] aRowDatas)
        {
            _isEditing = true;

            //ItemCode = aRowDatas[0];
            //edtItemCode.EditValue = ItemCode;
            //edtItemName.EditValue = aRowDatas[1];
            //edtCatgCode.EditValue = aRowDatas[2];

            //cheUseYN.Checked = (aRowDatas[3] == "Y");
        }

        public void NewItemData()
        {
            _isEditing = false;

            //ItemCode = DateTime.Now.ToString("yyMMdd") + "-" + Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds).ToString("00000");
            //edtItemCode.EditValue = ItemCode;
            //edtCatgCode.EditValue = "-";
            //edtItemName.EditValue = "-";
            //cheUseYN.EditValue = "Y";
        }

        public void NewItemData(DataRow aRow)
        {
            _isEditing = false;

            //ItemCode = DateTime.Now.ToString("yyMMdd") + "-" + Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds).ToString("00000");
            //edtItemCode.EditValue = ItemCode;
            //edtItemName.EditValue = aRow["ItemName"].ToString();
            //edtCatgCode.EditValue = aRow["CatgCode"].ToString();
            //cheUseYN.Checked = (aRow["UseYN"].ToString() == "Y");
        }

        public void NewItemData(params string[] aRowDatas)
        {   
            _isEditing = false;

            //ItemCode = DateTime.Now.ToString("yyMMdd") + "-" + Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds).ToString("00000");
            //edtItemCode.EditValue = ItemCode;
            //edtItemName.EditValue = "";
            //edtCatgCode.EditValue = aRowDatas[2];
            //cheUseYN.Checked = (aRowDatas[3] == "Y");
        }

        private void updateInspData()
        {
            string sCheck = "OK";
            StringBuilder sbQuery = new StringBuilder();

            if (cheConvtn.Checked)
            {
                sCheck = "-";
            }
            else
            {
                sCheck = cheBellowsOK.Checked ? "OK" : "NG";
            }

            if (XtraMessageBox.Show("수정한 내용을 저장하시겠습니까?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.No) { return; }

            sbQuery.Clear();
            sbQuery.Append("UPDATE tb_insp_data SET ");
            sbQuery.Append(" ProcStat = @ProcStat");
            sbQuery.Append(" , TestDate = @TestDate");
            sbQuery.Append(" , CustCode = @CustCode");
            sbQuery.Append(" , ItemNo = @ItemNo");
            sbQuery.Append(" , Plant = @Plant");
            sbQuery.Append(" , Inlet = @Inlet");
            sbQuery.Append(" , Outlet = @Outlet");
            sbQuery.Append(" , Capct = @Capct");
            sbQuery.Append(" , CapctUnit = @CapctUnit");
            sbQuery.Append(" , Fluid = @Fluid");
            sbQuery.Append(" , Maker = @Maker");
            sbQuery.Append(" , Model = @Model");
            sbQuery.Append(" , Serial = @Serial");
            sbQuery.Append(" , Type = @Type");
            sbQuery.Append(" , SetPress = @SetPress");
            sbQuery.Append(" , BackPress = @BackPress");
            //sbQuery.Append(" , TestPress = @TestPress");
            sbQuery.Append(" , CDS = @CDS");
            sbQuery.Append(" , Unit = @Unit");
            sbQuery.Append(" , TestFluid = @TestFluid");
            sbQuery.Append(" , TestRslt = @TestRslt");
            sbQuery.Append(" , BodyMatl = @BodyMatl");
            sbQuery.Append(" , DiscMatl = @DiscMatl");
            sbQuery.Append(" , PoppPress1 = @PoppPress1");
            sbQuery.Append(" , ClosPress1 = @ClosPress1");
            sbQuery.Append(" , PoppPress2 = @PoppPress2");
            sbQuery.Append(" , ClosPress2 = @ClosPress2");
            sbQuery.Append(" , ChkCap = @ChkCap");
            sbQuery.Append(" , ChkBonnet = @ChkBonnet");
            sbQuery.Append(" , ChkBody = @ChkBody");
            sbQuery.Append(" , ChkNozz = @ChkNozz");
            sbQuery.Append(" , ChkNozzSeat = @ChkNozzSeat");
            sbQuery.Append(" , ChkDisc = @ChkDisc");
            sbQuery.Append(" , ChkDiscSeat = @ChkDiscSeat");
            sbQuery.Append(" , ChkBellows = @ChkBellows");
            sbQuery.Append(" , ChkSpring = @ChkSpring");
            sbQuery.Append(" , ChkSpindle = @ChkSpindle");
            sbQuery.Append(" , ChkLowRing = @ChkLowRing");
            sbQuery.Append(" , ChkUppRing = @ChkUppRing");
            sbQuery.Append(" , InspRemk = @InspRemk");
            sbQuery.Append(" , PressVals1 = @PressVals1");
            sbQuery.Append(" , PressVals2 = @PressVals2");
            sbQuery.Append(" , PressChtImg = @PressChtImg");
            sbQuery.Append(" WHERE OrderNo = @OrderNo");
            sbQuery.Append("   AND OrderSeq = @OrderSeq");

            //string sInspData = JsonConvert.SerializeObject(_inspDatas, Formatting.Indented);
            byte[] imgBytes;
            using (MemoryStream msChtImg = new MemoryStream())
            {
                chtPresData.ExportToImage(msChtImg, ImageFormat.Png);
                imgBytes = msChtImg.ToArray();
            }
            // new TDataParameter("@Capct", MySqlDbType.Blob, imgBytes)
            // new TDataParameter("@Fluid", MySqlDbType.String, string.Join(",", _measVals1))

            TDataParameter[] vParams = new TDataParameter[] {
                new TDataParameter("@OrderNo", MySqlDbType.String, _orderNo),
                new TDataParameter("@OrderSeq", MySqlDbType.Int32, _orderSeq),
                new TDataParameter("@ProcStat", MySqlDbType.String, "2"),
                new TDataParameter("@TestDate", MySqlDbType.DateTime, edtInspDate.Value),
                new TDataParameter("@CustCode", MySqlDbType.String, _custCode),
                new TDataParameter("@ItemNo", MySqlDbType.String, edtItemNo.EditValue),
                new TDataParameter("@Plant", MySqlDbType.String, edtPlant.EditValue),
                new TDataParameter("@Inlet", MySqlDbType.String, edtInLet.EditValue),
                new TDataParameter("@Outlet", MySqlDbType.String, edtOutLet.EditValue),
                new TDataParameter("@Capct", MySqlDbType.String, edtCapct.EditValue),
                new TDataParameter("@CapctUnit", MySqlDbType.String, edtCapctUnit.EditValue),
                new TDataParameter("@Fluid", MySqlDbType.String, edtFluid.EditValue),
                new TDataParameter("@Maker", MySqlDbType.String, edtMaker.EditValue),
                new TDataParameter("@Model", MySqlDbType.String, edtModel.EditValue),
                new TDataParameter("@Serial", MySqlDbType.String, edtSerialNo.EditValue),
                new TDataParameter("@Type", MySqlDbType.String, edtType.EditValue),
                new TDataParameter("@SetPress", MySqlDbType.String, edtSetPress.EditValue),
                new TDataParameter("@BackPress", MySqlDbType.String, edtBackPress.EditValue),
                //new TDataParameter("@TestPress", MySqlDbType.String, edtest.EditValue),
                new TDataParameter("@CDS", MySqlDbType.String, edtCDS.EditValue),
                new TDataParameter("@Unit", MySqlDbType.String, edtUnit.EditValue),
                new TDataParameter("@TestFluid", MySqlDbType.String, edtTestFluid.EditValue),
                new TDataParameter("@TestRslt", MySqlDbType.String, "OK"),
                new TDataParameter("@BodyMatl", MySqlDbType.String, edtBodyMatl.EditValue),
                new TDataParameter("@DiscMatl", MySqlDbType.String, edtDiscMatl.EditValue),
                new TDataParameter("@PoppPress1", MySqlDbType.String, edtPoppPress1.EditValue),
                new TDataParameter("@ClosPress1", MySqlDbType.String, edtClosPress1.EditValue),
                new TDataParameter("@PoppPress2", MySqlDbType.String, edtPoppPress2.EditValue),
                new TDataParameter("@ClosPress2", MySqlDbType.String, edtClosPress2.EditValue),
                new TDataParameter("@ChkCap", MySqlDbType.String, cheCapOK.Checked ? "OK" : "NG"),
                new TDataParameter("@ChkBonnet", MySqlDbType.String, cheBonnetOK.Checked ? "OK" : "NG"),
                new TDataParameter("@ChkBody", MySqlDbType.String, cheBodyOK.Checked ? "OK" : "NG"),
                new TDataParameter("@ChkNozz", MySqlDbType.String, cheNozzOK.Checked ? "OK" : "NG"),
                new TDataParameter("@ChkNozzSeat", MySqlDbType.String, cheNozzSeatOK.Checked ? "OK" : "NG"),
                new TDataParameter("@ChkDisc", MySqlDbType.String, cheDiscOK.Checked ? "OK" : "NG"),
                new TDataParameter("@ChkDiscSeat", MySqlDbType.String, cheDiscSeatOK.Checked ? "OK" : "NG"),
                new TDataParameter("@ChkBellows", MySqlDbType.String, sCheck),
                new TDataParameter("@ChkSpring", MySqlDbType.String, cheSpringOK.Checked ? "OK" : "NG"),
                new TDataParameter("@ChkSpindle", MySqlDbType.String, cheSpindleOK.Checked ? "OK" : "NG"),
                new TDataParameter("@ChkLowRing", MySqlDbType.String, cheLowRingOK.Checked ? "OK" : "NG"),
                new TDataParameter("@ChkUppRing", MySqlDbType.String, cheUppRingOK.Checked ? "OK" : "NG"),
                new TDataParameter("@InspRemk", MySqlDbType.String, edtRemk.EditValue),
                new TDataParameter("@PressVals1", MySqlDbType.String, SavePressureCurve(_presValues1, _presInterval1)),
                new TDataParameter("@PressVals2", MySqlDbType.String, SavePressureCurve(_presValues2, _presInterval2)),
                new TDataParameter("@PressChtImg", MySqlDbType.Blob, imgBytes)
            };

            TDataMySql.SetDataCommand(sbQuery.ToString(), vParams);
        }
    } // class dfCheckEdit
}
