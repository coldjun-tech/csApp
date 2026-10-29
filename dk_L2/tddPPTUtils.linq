<Query Kind="Program">
  <NuGetReference>MySql.Data</NuGetReference>
  <NuGetReference>Newtonsoft.Json.Bson</NuGetReference>
  <Namespace>MySql.Data</Namespace>
  <Namespace>MySql.Data.MySqlClient</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.Linq</Namespace>
</Query>

void Main()
{
	TInspItem _inspData = new TInspItem();
	_inspData.ItemNo = "TDD1111";
	_inspData.Plant = "plant1";
	StringBuilder sbQuery = new StringBuilder();
	int iNewSeq = 281;

	using (MySqlConnection connInsert = new MySqlConnection(TDataMySql._CONN_STR))
	{
		connInsert.Open();

		using (MySqlTransaction tranInsert = connInsert.BeginTransaction())
		{
			try
			{
				sbQuery.Clear();
				sbQuery.Append("UPDATE tb_insp_data");
				sbQuery.Append("   SET OrderSeq = OrderSeq + 10000");
				sbQuery.Append(" WHERE OrderNo = @OrderNo");
				sbQuery.Append("   AND OrderSeq > @OrderSeq");
				sbQuery.Append(" ORDER BY OrderSeq DESC;");
				TDataParameter[] vParams1 = new TDataParameter[] {
					new TDataParameter("@OrderNo", MySqlDbType.String, "260929-001"),
					new TDataParameter("@OrderSeq", MySqlDbType.Int32, iNewSeq)
				};
				TDataMySql.SetDataCommand(connInsert, sbQuery.ToString(), vParams1);

				iNewSeq = iNewSeq + 1;
				sbQuery.Clear();
				sbQuery.Append("INSERT INTO tb_insp_data");
				sbQuery.Append(" (OrderNo, OrderSeq, ProcStat, TestDate, CustCode, ItemNo, Plant)");
				sbQuery.Append(" VALUES ");
				sbQuery.Append(" (@OrderNo, @OrderSeq, @ProcStat, @TestDate, @CustCode, @ItemNo, @Plant)");
				TDataParameter[] vParams2 = new TDataParameter[] {
					new TDataParameter("@OrderNo", MySqlDbType.String, "260929-001"),
					new TDataParameter("@OrderSeq", MySqlDbType.Int32, iNewSeq),
					new TDataParameter("@ProcStat", MySqlDbType.String, "1"),
					new TDataParameter("@TestDate", MySqlDbType.DateTime, DateTime.Now),
					new TDataParameter("@CustCode", MySqlDbType.String, "260726-58398"),
					new TDataParameter("@ItemNo", MySqlDbType.String, "ItemNo-" + iNewSeq.ToString()),
					new TDataParameter("@Plant", MySqlDbType.String, "Plant-" + iNewSeq.ToString())
				};
				TDataMySql.SetDataCommand(connInsert, sbQuery.ToString(), vParams2);

				sbQuery.Clear();
				sbQuery.Append("UPDATE tb_insp_data T");
				sbQuery.Append(" JOIN (");
				sbQuery.Append(" SELECT OrderNo, OrderSeq, - (ROW_NUMBER() OVER(ORDER BY OrderSeq ASC)) AS NewSeq");
				sbQuery.Append("   FROM tb_insp_data");
				sbQuery.Append(" WHERE OrderNo = @OrderNo");
				sbQuery.Append(" ) S ON T.OrderNo = S.OrderNo AND T.OrderSeq = S.OrderSeq");
				sbQuery.Append(" SET T.OrderSeq = S.NewSeq");
				TDataParameter[] vParams3 = new TDataParameter[] {
					new TDataParameter("@OrderNo", MySqlDbType.String, "260929-001")
				};
				TDataMySql.SetDataCommand(connInsert, sbQuery.ToString(), vParams3);

				sbQuery.Clear();
				sbQuery.Append("UPDATE tb_insp_data");
				sbQuery.Append("   SET OrderSeq = ABS(OrderSeq)");
				sbQuery.Append(" WHERE OrderNo = @OrderNo");
				sbQuery.Append("   AND OrderSeq < 0");
				TDataParameter[] vParams4 = new TDataParameter[] {
					new TDataParameter("@OrderNo", MySqlDbType.String, "260929-001")
				};
				TDataMySql.SetDataCommand(connInsert, sbQuery.ToString(), vParams4);

				tranInsert.Commit();
			}
			catch (Exception)
			{
				tranInsert.Rollback();

				//throw;
			}
		}
	}
}

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

public class TDataParameter
{
	public string ParamName { get; set; }
	public MySqlDbType ParamType { get; set; }
	public object ParamValue { get; set; }

	public TDataParameter(string asParamName, MySqlDbType aParamType, object aParamValue)
	{
		ParamName = asParamName;
		ParamType = aParamType;
		ParamValue = aParamValue;
	}

	~TDataParameter()
	{
		//
	}
}

public static class TDataMySql
{
	public static string _CONN_STR = "server=127.0.0.1;user=root;database=tdd_db;port=3306;password=1122qqww;allow user variables=true;Convert Zero Datetime=true;";

	static TDataMySql()
	{
		//
	}

	public static DataTable GetDataTable(string asQuery, params string[] args)
	{
		DataTable dataResult = new DataTable();
		MySqlConnection conn = null;

		try
		{
			conn = new MySqlConnection(TDataMySql._CONN_STR);
			MySqlCommand cmd = new MySqlCommand();
			cmd.Connection = conn;
			cmd.CommandTimeout = 60 * 30;
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = asQuery;

			MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
			//adapter.SelectCommand = new MySqlCommand(asQuery, conn);
			DataSet dsData = new DataSet();
			adapter.Fill(dsData);
			if (dsData.Tables.Count < 1)
			{
				throw new Exception("조건에 만족하는 데이터가 없습니다.");
			}
			else
			{
				dataResult = dsData.Tables[0];
			}
			conn.Close();
		}
		catch (Exception ex)
		{
			//MessageBox.Show(ex.Message);
			//throw new Exception(ex.Message);
			if (conn != null)
			{
				conn.Close();
			}
			//throw;
		}
		return dataResult;
	}

	public static DataRow GetDataRow(string asQuery, params string[] args)
	{
		DataTable dataResult = new DataTable();
		MySqlConnection conGetData = null;

		try
		{
			conGetData = new MySqlConnection(TDataMySql._CONN_STR);
			MySqlCommand cmdGetData = new MySqlCommand();
			cmdGetData.Connection = conGetData;
			cmdGetData.CommandTimeout = 60 * 30;
			cmdGetData.CommandType = CommandType.Text;
			cmdGetData.CommandText = asQuery;

			MySqlDataAdapter adapter = new MySqlDataAdapter(cmdGetData);
			//adapter.SelectCommand = new MySqlCommand(asQuery, conn);
			DataSet dsData = new DataSet();
			adapter.Fill(dsData);
			if (dsData.Tables.Count < 1)
			{
				dataResult = null;
				throw new Exception("조건에 만족하는 데이터가 없습니다.");
			}
			else
			{
				dataResult = dsData.Tables[0];
			}
			conGetData.Close();
		}
		catch (Exception ex)
		{
			//MessageBox.Show(ex.Message);
			if (conGetData != null)
			{
				conGetData.Close();
			}
			//throw;
		}
		return dataResult.Rows[0];
	}

	public static MySqlDataReader GetDataReader(string asQuery, params string[] args)
	{
		MySqlDataReader darResult = null;
		MySqlConnection connGetData = new MySqlConnection(TDataMySql._CONN_STR);
		MySqlCommand cmdGetData = new MySqlCommand();
		try
		{

			cmdGetData.Connection = connGetData;
			cmdGetData.CommandTimeout = 60 * 30;
			cmdGetData.CommandType = CommandType.Text;
			cmdGetData.CommandText = asQuery;

			darResult = cmdGetData.ExecuteReader();
			connGetData.Close();
		}
		catch (Exception ex)
		{
			//MessageBox.Show(ex.Message);
			if (connGetData != null)
			{
				connGetData.Close();
			}
			//throw;
		}
		return darResult;
	}

	public static void SetDataCommand(MySqlConnection aConn, string asQuery, params TDataParameter[] aParam)
	{
		try
		{
			//conSetData = new MySqlConnection(TDataMySql._CONN_STR);
			MySqlCommand cmdSetData = new MySqlCommand();
			cmdSetData.Connection = aConn;
			cmdSetData.CommandTimeout = 60 * 30;
			cmdSetData.CommandType = CommandType.Text;
			cmdSetData.CommandText = asQuery;

			//conSetData.Open();

			for (int i = 0; i < aParam.Length; i++)
			{
				cmdSetData.Parameters.Add(aParam[i].ParamName, aParam[i].ParamType);
				cmdSetData.Parameters[i].Value = aParam[i].ParamValue;
			}

			cmdSetData.ExecuteNonQuery();
			//conSetData.Close();
		}
		catch (Exception ex)
		{
			//MessageBox.Show(ex.Message);
			//if (conSetData != null)
			//{
			//    conSetData.Close();
			//}
			throw;
		}
	}

	public static void SetDataCommand(string asQuery, params TDataParameter[] aParam)
	{
		MySqlConnection conSetData = null;
		try
		{
			conSetData = new MySqlConnection(TDataMySql._CONN_STR);
			MySqlCommand cmdSetData = new MySqlCommand();
			cmdSetData.Connection = conSetData;
			cmdSetData.CommandTimeout = 60 * 30;
			cmdSetData.CommandType = CommandType.Text;
			cmdSetData.CommandText = asQuery;

			conSetData.Open();

			for (int i = 0; i < aParam.Length; i++)
			{
				cmdSetData.Parameters.Add(aParam[i].ParamName, aParam[i].ParamType);
				cmdSetData.Parameters[i].Value = aParam[i].ParamValue;
			}

			cmdSetData.ExecuteNonQuery();
			conSetData.Close();
		}
		catch (Exception ex)
		{
			//MessageBox.Show(ex.Message);
			if (conSetData != null)
			{
				conSetData.Close();
			}
			//throw;
		}
	}

	public static void AsyncSetDataCommand(string asQuery, params TDataParameter[] aParam)
	{
		MySqlConnection conSetData = null;
		try
		{
			conSetData = new MySqlConnection(TDataMySql._CONN_STR);
			MySqlCommand cmdSetData = new MySqlCommand();
			cmdSetData.Connection = conSetData;
			cmdSetData.CommandTimeout = 60 * 30;
			cmdSetData.CommandType = CommandType.Text;
			cmdSetData.CommandText = asQuery;

			conSetData.Open();

			for (int i = 0; i < aParam.Length; i++)
			{
				cmdSetData.Parameters.Add(aParam[i].ParamName, aParam[i].ParamType);
				cmdSetData.Parameters[i].Value = aParam[i].ParamValue;
			}

			cmdSetData.ExecuteNonQuery();
			conSetData.Close();
		}
		catch (Exception ex)
		{
			//MessageBox.Show(ex.Message);
			if (conSetData != null)
			{
				conSetData.Close();
			}
			//throw;
		}
	}

	public static void SetDataCommand(string asQuery, List<TDataParameter> aParam)
	{
		MySqlConnection conSetData = null;
		try
		{
			conSetData = new MySqlConnection(TDataMySql._CONN_STR);
			MySqlCommand cmdSetData = new MySqlCommand();
			cmdSetData.Connection = conSetData;
			cmdSetData.CommandTimeout = 60 * 30;
			cmdSetData.CommandType = CommandType.Text;
			cmdSetData.CommandText = asQuery;

			conSetData.Open();

			for (int i = 0; i < aParam.Count; i++)
			{
				cmdSetData.Parameters.Add(aParam[i].ParamName, aParam[i].ParamType);
				cmdSetData.Parameters[i].Value = aParam[i].ParamValue;
			}

			cmdSetData.ExecuteNonQuery();
			conSetData.Close();
		}
		catch (Exception ex)
		{
			//MessageBox.Show(ex.Message);
			if (conSetData != null)
			{
				conSetData.Close();
			}
			//throw;
		}
	}

	//var data = new { Title = "C# Guide", Version = 12.0 };
	//GenericInspector.Dump(data);
	// <T>를 사용하여 타입 안정성을 확보합니다.
	public static void SetDataCommand<T>(string asQuery, T aDataVal)
	{
		MySqlConnection conSetData = null;
		try
		{
			conSetData = new MySqlConnection(TDataMySql._CONN_STR);
			MySqlCommand cmdSetData = new MySqlCommand();
			cmdSetData.Connection = conSetData;
			cmdSetData.CommandTimeout = 60 * 30;
			cmdSetData.CommandType = CommandType.Text;
			cmdSetData.CommandText = asQuery;

			conSetData.Open();

			Type type = typeof(T);
			PropertyInfo[] properties = type.GetProperties();
			foreach (var vProp in properties)
			{
				//vParamList.Add(new TDataParameter($"@{vProp.Name}", vProp.PropertyType, vProp.GetValue(aDataVal)));
				//cmdSetData.Parameters.Add(vProp.Name, vProp.PropertyType);
				//cmdSetData.Parameters[i].Value = vProp.GetValue(aDataVal);
			}

			cmdSetData.ExecuteNonQuery();
			conSetData.Close();
		}
		catch (Exception ex)
		{
			//MessageBox.Show(ex.Message);
			if (conSetData != null)
			{
				conSetData.Close();
			}
			//throw;
		}

		//if (aDataVal == null) return;

		//string sCommandText = "INSERT INTO tb_insp_data" +
		//                      " ( OrderNo, CustCode, ItemNo, InspDate, PresAct1, BlowDown1, PresAct2, BlowDown2, ChtImg, InspDatas, MeasVals )" +
		//                      " VALUES " +
		//                      " ( @OrderNo, @CustCode, @ItemNo, @InspDate, @PresAct1, @BlowDown1, @PresAct2, @BlowDown2, @ChtImg, @InspDatas, @MeasVals )";

		//List<TDataParameter> vParamList = new List<TDataParameter>();
		//// typeof(T)를 사용하면 런타임 obj.GetType() 호출을 줄일 수 있습니다.
		//Type type = typeof(T);
		////Console.WriteLine($"[Type: {type.Name}]");

		//PropertyInfo[] properties = type.GetProperties();
		//foreach (var vProp in properties)
		//{
		//    //Console.WriteLine($"{prop.Name} = {prop.GetValue(obj)}");
		//    // Type 정보를 업데이트하자.
		//    //vParamList.Add(new TDataParameter($"@{vProp.Name}", vProp.PropertyType, vProp.GetValue(aDataVal)));
		//}
		//TDataParameter[] vParams = vParamList.ToArray();
		//TDataMySql.SetDataCommand(sCommandText, vParams);
	}

	public static string GetNextItemCodeStr()
	{
		string sResult = "";
		MySqlConnection conn = new MySqlConnection(TDataMySql._CONN_STR);
		MySqlCommand cmd = new MySqlCommand();
		try
		{
			cmd.Connection = conn;
			cmd.CommandTimeout = 60 * 30;
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT (MAX(ItemCode) + 1) AS ItemCode FROM tb_item_code WHERE ItemCode > '0900';";

			conn.Open();

			object scarValue = cmd.ExecuteScalar();
			sResult = scarValue.ToString();

			conn.Close();
		}
		catch (Exception ex)
		{
			if (conn != null)
			{
				conn.Close();
			}
		}
		return sResult;
	}

	public static bool CompareMD5Str(string asPlainStr, string asEncryptedStr)
	{
		MySqlConnection con = new MySqlConnection(TDataMySql._CONN_STR);
		MySqlCommand cmd = new MySqlCommand();

		try
		{
			cmd.Connection = con;
			cmd.CommandTimeout = 60 * 30;
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT IF (MD5('" + asPlainStr + "') = '" + asEncryptedStr + "', 'Y', 'N') AS comp_result;";

			con.Open();

			object scarValue = cmd.ExecuteScalar();
			scarValue = cmd.ExecuteScalar();
			return (scarValue.ToString() == "Y");

			con.Close();
		}
		catch (Exception ex)
		{
			return false;
			if (con != null)
			{
				con.Close();
			}
		}

	}

	public static string GetMD5Str(string asPlainStr)
	{
		string sResult = "";
		MySqlConnection conn = new MySqlConnection(TDataMySql._CONN_STR);
		MySqlCommand cmd = new MySqlCommand();
		try
		{
			cmd.Connection = conn;
			cmd.CommandTimeout = 60 * 30;
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT MD5('" + asPlainStr + "');";

			conn.Open();

			object scarValue = cmd.ExecuteScalar();
			sResult = scarValue.ToString();

			conn.Close();
		}
		catch (Exception ex)
		{
			if (conn != null)
			{
				conn.Close();
			}
		}
		return sResult;
	}

	//public static DataTable DeleteFile(string filePath)
	//{
	//  //DataTable result = new DataTable();

	//  try
	//  {
	//    string fileName = @"D:\\KJH\\WEB\\files\\" + filePath.Trim();
	//    if (File.Exists(fileName))
	//    {
	//      File.Delete(fileName);
	//    }
	//  }
	//  catch (Exception ex)
	//  {
	//    //result.isError = true;
	//    //result.ResultValue = ex.Message;
	//  }
	//  return result;
	//}

	public static string DataToJsonStr(Dictionary<string, int> aDicData)
	{
		return JsonConvert.SerializeObject(aDicData, Newtonsoft.Json.Formatting.Indented);
		//JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
		//string jsonString = System.Text.Json.JsonSerializer.Serialize(myDictionary);
	}

} // class TDataMySql