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
	TDataParam[] vParams1 = new TDataParam[] {
		new TDataParam("@OrderNo", MySqlDbType.String, "260929-001"),
		new TDataParam("@OrderSeq", MySqlDbType.Int32, 1)
	};
	TDataMySql.GetDataTable("SELECT * FROM tb_insp_data WHERE 1=1 AND OrderNo = @OrderNo AND OrderSeq = @OrderSeq", vParams1).Dump();
	
	List<TDataParam> vParams2 = new List<UserQuery.TDataParam>();
	vParams2.Add(new TDataParam("OrderNo", MySqlDbType.String, "260929-001"));
	vParams2.Add(new TDataParam("OrderSeq", MySqlDbType.Int32, 1));
	TDataMySql.GetDataTable("SELECT * FROM tb_insp_data", vParams2).Dump();
	//TDataMySql.GetDataTable("SELECT * FROM tb_insp_data WHERE 1=1", vParams2).Dump();
	
	//TDataMySql.GetMD5Str("aaa").Dump();
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

public class TDataParam
{
	public string ParamName { get; set; }
	public MySqlDbType ParamType { get; set; }
	public object ParamValue { get; set; }

	public TDataParam(string asParamName, MySqlDbType aParamType, object aParamValue)
	{
		ParamName = asParamName;
		ParamType = aParamType;
		ParamValue = aParamValue;
	}

	~TDataParam()
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

	public static DataTable GetDataTable(string asQuery, params TDataParam[] aParams)
	{
		DataTable datResult = new DataTable();

		using (MySqlConnection conn = new MySqlConnection(TDataMySql._CONN_STR))
		{
			try
			{
				MySqlCommand cmd = new MySqlCommand();
				cmd.Connection = conn;
				cmd.CommandTimeout = 60 * 30;
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = asQuery;

				foreach (TDataParam vParam in aParams)
				{
					cmd.Parameters.Add(vParam.ParamName, vParam.ParamType).Value = vParam.ParamValue;
				}

				MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
				DataSet dataSet = new DataSet();
				adapter.Fill(dataSet);
				if (dataSet.Tables.Count < 1)
				{
					throw new Exception("조건에 만족하는 데이터가 없습니다.");
				}
				else
				{
					datResult = dataSet.Tables[0];
				}
				conn.Close();
			}
			catch (Exception ex)
			{
				if (conn != null)
				{
					conn.Close();
				}
				//throw new Exception(ex.Message);
				//throw;
			}
			return datResult;
		}
	}

	public static DataTable GetDataTable(string asQuery, List<TDataParam> aParams)
	{
		DataTable datResult = new DataTable();

    	using (MySqlConnection conn = new MySqlConnection(TDataMySql._CONN_STR))
    	using (MySqlCommand cmd = new MySqlCommand())
    	{
        	try
        	{
            	cmd.Connection = conn;
            	cmd.CommandTimeout = 60 * 30; // 30분
            	cmd.CommandType = CommandType.Text;
            
            	// aParams가 null인지 안전하게 확인
            	if (aParams != null && aParams.Count > 0)
            	{
					bool isFirst = true;
					
                	foreach (TDataParam vParam in aParams)
                	{
                    	if (isFirst)
						{
							asQuery += $" WHERE {vParam.ParamName} = @{vParam.ParamName}";
							isFirst = false;
						}
						else
						{
							asQuery += $" AND {vParam.ParamName} = @{vParam.ParamName}";
						}
						//asQuery += $" AND {vParam.ParamName} = @{vParam.ParamName}";
                    	cmd.Parameters.Add("@" + vParam.ParamName, vParam.ParamType).Value = vParam.ParamValue;
                	}	
            	}
            	cmd.CommandText = asQuery;

				using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
				{
					adapter.Fill(datResult);
				}

				if (datResult.Rows.Count == 0)
				{
					//throw new Exception("조건에 만족하는 데이터가 없습니다.");
				}
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		return datResult;
	}

	public static DataRow GetDataRow(string asQuery)
	{
		DataTable datResult = new DataTable();

		using (MySqlConnection conn = new MySqlConnection(TDataMySql._CONN_STR))
		using (MySqlCommand cmd = new MySqlCommand())
		{
			try
			{
				cmd.Connection = conn;
				cmd.CommandTimeout = 60 * 30; // 30분
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = asQuery;

				using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
				{
					adapter.Fill(datResult);
				}

				if (datResult.Rows.Count == 0)
				{
					//throw new Exception("조건에 만족하는 데이터가 없습니다.");
				}
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		return datResult.Rows[0];
	}

	public static MySqlDataReader GetDataReader(string asQuery, params string[] args)
	{
		MySqlDataReader darResult = null;

		using (MySqlConnection conn = new MySqlConnection(TDataMySql._CONN_STR))
		using (MySqlCommand cmd = new MySqlCommand())
		{
			try
			{
				cmd.Connection = conn;
				cmd.CommandTimeout = 60 * 30; // 30분
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = asQuery;
				
				conn.Open();
				
				darResult = cmd.ExecuteReader();
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		return darResult;
	}

	public static void SetDataCommand(MySqlConnection aConn, string asQuery, params TDataParam[] aParams)
	{
		using (MySqlCommand cmd = new MySqlCommand())
		{
			try
			{
				cmd.Connection = aConn;
				cmd.CommandTimeout = 60 * 30; // 30분
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = asQuery;

				foreach (TDataParam vParam in aParams)
				{
					cmd.Parameters.Add(vParam.ParamName, vParam.ParamType).Value = vParam.ParamValue;
				}
				
				cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				throw;
			}
		}
	}

	public static void SetDataCommand(string asQuery, params TDataParam[] aParams)
	{
		using (MySqlConnection conn = new MySqlConnection(TDataMySql._CONN_STR))
		using (MySqlCommand cmd = new MySqlCommand())
		{
			try
			{
				cmd.Connection = conn;
				cmd.CommandTimeout = 60 * 30; // 30분
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = asQuery;

				foreach (TDataParam vParam in aParams)
				{
					cmd.Parameters.Add(vParam.ParamName, vParam.ParamType).Value = vParam.ParamValue;
				}
				conn.Open();
				cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				throw;
			}
		}

//		MySqlConnection conSetData = null;
//		try
//		{
//			conSetData = new MySqlConnection(TDataMySql._CONN_STR);
//			MySqlCommand cmdSetData = new MySqlCommand();
//			cmdSetData.Connection = conSetData;
//			cmdSetData.CommandTimeout = 60 * 30;
//			cmdSetData.CommandType = CommandType.Text;
//			cmdSetData.CommandText = asQuery;
//
//			conSetData.Open();
//
//			for (int i = 0; i < aParam.Length; i++)
//			{
//				cmdSetData.Parameters.Add(aParam[i].ParamName, aParam[i].ParamType);
//				cmdSetData.Parameters[i].Value = aParam[i].ParamValue;
//			}
//
//			cmdSetData.ExecuteNonQuery();
//			conSetData.Close();
//		}
//		catch (Exception ex)
//		{
//			//MessageBox.Show(ex.Message);
//			if (conSetData != null)
//			{
//				conSetData.Close();
//			}
//			//throw;
//		}
	}

	public static void SetDataCommand(string asQuery, List<TDataParam> aParams)
	{
		using (MySqlConnection conn = new MySqlConnection(TDataMySql._CONN_STR))
		using (MySqlCommand cmd = new MySqlCommand())
		{
			try
			{
				cmd.Connection = conn;
				cmd.CommandTimeout = 60 * 30; // 30분
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = asQuery;

				foreach (TDataParam vParam in aParams)
				{
					cmd.Parameters.Add(vParam.ParamName, vParam.ParamType).Value = vParam.ParamValue;
				}

				cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				throw;
			}
		}
	}

	public static void SetDataCommand<T>(string asTableName, T model) where T : class
	{
		if (model == null) return;

		using (MySqlConnection conn = new MySqlConnection(TDataMySql._CONN_STR))
		using (MySqlCommand cmd = new MySqlCommand())
		{
			try
			{
				PropertyInfo[] properties = typeof(T).GetProperties();

				string sColumns = string.Join(", ", properties.Select(p => p.Name));
				string sParams = string.Join(", ", properties.Select(p => "@" + p.Name));
				string autoQuery = $"INSERT INTO {asTableName} ({sColumns}) VALUES ({sParams})";

				cmd.Connection = conn;
				cmd.CommandTimeout = 60 * 30; // 30분
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = autoQuery;

				foreach (PropertyInfo prop in properties)
				{
					string paramName = "@" + prop.Name;
					object paramValue = prop.GetValue(model) ?? DBNull.Value;

					cmd.Parameters.AddWithValue(paramName, paramValue);
				}

				conn.Open();
				cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				throw;
			}
		}
	}

	public static string GetNextItemCodeStr()
	{
		string sResult = "";
		
		using (MySqlConnection conn = new MySqlConnection(TDataMySql._CONN_STR))
		using (MySqlCommand cmd = new MySqlCommand())
		{
			try
			{
				cmd.Connection = conn;
				cmd.CommandTimeout = 60 * 30; // 30분
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "SELECT (MAX(ItemCode) + 1) AS ItemCode FROM tb_item_code WHERE ItemCode > '0900';";

				object scarValue = cmd.ExecuteScalar();
				sResult = scarValue.ToString();
			}
			catch (Exception ex)
			{
				throw;
			}
		}
		
		return sResult;
	}

	public static bool CompareMD5Str(string asPlainStr, string asEncryptedStr)
	{
		string sResult = "";

		using (MySqlConnection conn = new MySqlConnection(TDataMySql._CONN_STR))
		using (MySqlCommand cmd = new MySqlCommand())
		{
			try
			{
				cmd.Connection = conn;
				cmd.CommandTimeout = 60 * 30; // 30분
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "SELECT IF (MD5('" + asPlainStr + "') = '" + asEncryptedStr + "', 'Y', 'N') AS comp_result;";

				object scarValue = cmd.ExecuteScalar();
				//scarValue = cmd.ExecuteScalar();
				
				return (scarValue.ToString() == "Y");
			}
			catch (Exception ex)
			{
				throw;
			}
		}
	}

	public static string GetMD5Str(string asPlainStr)
	{
		string sResult = "";

		using (MySqlConnection conn = new MySqlConnection(TDataMySql._CONN_STR))
		using (MySqlCommand cmd = new MySqlCommand())
		{
			try
			{
				cmd.Connection = conn;
				cmd.CommandTimeout = 60 * 30; // 30분
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "SELECT MD5(@plainStr);";
				cmd.Parameters.AddWithValue("@plainStr", asPlainStr);

				conn.Open();
				
				object scarValue = cmd.ExecuteScalar();
				return Convert.ToString(scarValue);
			}
			catch (Exception ex)
			{
				throw;
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