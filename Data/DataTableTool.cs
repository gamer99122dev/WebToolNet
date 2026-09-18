using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebToolNet.Extensions;

namespace WebToolNet.Data
{
    public class DataTableTool
    {
        DBTool _dbtool = new DBTool();

        public string sqlErrtext = "";

        public DataTableTool()
        {
        }


        /// <summary>
        /// 
        /// 處理存在硬碟中離線的 xml datatable
        /// 當 schema 變動後，讀進舊 datatable 資料會出錯
        /// 這個 function 在做校正 schema
        /// 
        /// </summary>
        /// <param name="orgDT">傳入從 xml 讀出資料的 datatable(有資料)</param>
        /// <param name="schemaDT">傳入 schema(沒帶資料)</param>
        /// <returns> 二邊比對後的輸出 </returns>
        public DataTable processTableSchema(DataTable orgDT, DataTable schemaDT)
        {
            string tableName = schemaDT.TableName;

            DataRow tRow;
            foreach (DataRow dr in orgDT.Rows)
            {
                tRow = schemaDT.NewRow();

                foreach (DataColumn col in schemaDT.Columns)
                {
                    if (orgDT.Columns.Contains(col.ColumnName))
                    {
                        tRow[col.ColumnName] = dr[col.ColumnName];
                    }
                }

                schemaDT.Rows.Add(tRow);

            }

            return schemaDT;
        }


        public string DataTableToInsertSQL(DataTable dt)
        {

            string SQL = "";
            foreach (DataRow dr in dt.Rows)
            {

                SQL += "INSERT INTO " + dt.TableName + " (";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    SQL += dt.Columns[i].ColumnName + ", ";
                }

                SQL = SQL.Substring(0, SQL.Length - 2);
                SQL += ")\n VALUES (";


                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    SQL += _dbtool.ISGenerator(dr[i]) + ", ";
                }
                SQL = SQL.Substring(0, SQL.Length - 2);
                SQL += ");" + "\n";
            }

            //MessageBox.Show(SQL);
            return SQL;
        }

        //update 用
        public string DataTableToDeleteSQL(DataTable dt)
        {
            string SQL = "Delete ";


            SQL += dt.pSQLInsert();

            return SQL;
        }

        public string DataTableToInsertSQL(DataTable dt, string DbName = "DB_OPD")
        {

            string SQL = "";
            StringBuilder sb = new StringBuilder();
            foreach (DataRow dr in dt.Rows)
            {

                SQL = "INSERT INTO " + DbName + ".dbo." + dt.TableName + " (";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    SQL += dt.Columns[i].ColumnName + ", ";
                }

                sb.Append(SQL.Substring(0, SQL.Length - 2));
                sb.Append(")\n VALUES (");

                SQL = "";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    SQL += _dbtool.ISGenerator(dr[i]) + ", ";
                }
                sb.Append(SQL.Substring(0, SQL.Length - 2));
                sb.Append(");" + "\n");
            }

            //MessageBox.Show(SQL);
            return sb.ToString();
        }



        public string DataTableToSingleInsertSQL(DataTable dt, string DbName = "DB_OPD")
        {

            string SQL = "";
            SQL += "INSERT INTO " + DbName + ".dbo." + dt.TableName + " (";
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                SQL += dt.Columns[i].ColumnName + ", ";
            }

            SQL = SQL.Substring(0, SQL.Length - 2);
            SQL += ")\n VALUES ";

            string tempSQL = string.Empty;
            List<string> SQLRow = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                tempSQL = "(";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    tempSQL += _dbtool.ISGenerator(dr[i]) + ", ";
                }
                tempSQL = tempSQL.Substring(0, tempSQL.Length - 2);
                tempSQL += ")";

                SQLRow.Add(tempSQL);
            }

            SQL += SQLRow.ToArray().pJoin<string>(",\n");
            SQL += ";\n";

            return SQL;
        }
        public string DataTableToSingleInsertMIDDLESQL(DataTable dt, string DbName = "DB_MIDDLE")
        {

            string SQL = "";
            SQL += "INSERT INTO " + DbName + ".dbo." + dt.TableName + " (";
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                SQL += dt.Columns[i].ColumnName + ", ";
            }

            SQL = SQL.Substring(0, SQL.Length - 2);
            SQL += ")\n VALUES ";

            string tempSQL = string.Empty;
            List<string> SQLRow = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                tempSQL = "(";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    tempSQL += _dbtool.ISGenerator(dr[i]) + ", ";
                }
                tempSQL = tempSQL.Substring(0, tempSQL.Length - 2);
                tempSQL += ")";

                SQLRow.Add(tempSQL);
            }

            SQL += SQLRow.ToArray().pJoin<string>(",\n");
            SQL += ";\n";

            return SQL;
        }

        // DataTable 依PK Columns轉成 Upsert(Update/Insert) 語法
        public string DataTableToUpsertSQL(DataTable dt, string DbName, params string[] PkCols)
        {
            if (PkCols.Length == 0)
            {
                Exception ex = new Exception("未指定PK欄位");
                throw ex;
            }
            List<string> SQLRow = new List<string>();

            foreach (DataRow r in dt.Rows)
            {
                List<string> InsertCols = new List<string>();
                List<string> InsertVals = new List<string>();

                List<string> SetKV = new List<string>();
                List<string> WhereKV = new List<string>();

                foreach (DataColumn col in dt.Columns)
                {
                    string colName = col.ColumnName;
                    string value = _dbtool.ISGenerator(r[colName]);

                    InsertCols.Add(colName);
                    InsertVals.Add(value);

                    if (colName.pIn(PkCols))
                    {
                        WhereKV.Add($"{colName} = {value}");
                    }
                    else
                    {
                        SetKV.Add($"{colName} = {value}");
                    }
                }

                string sql = $"Update {DbName}.dbo.{dt.TableName}\n";
                sql += $"Set {SetKV.pJoin(", ")}\n";
                sql += $"Where {WhereKV.pJoin(" And ")}\n";
                sql += $"if @@ROWCOUNT=0\n";
                sql += $"Insert into {DbName}.dbo.{dt.TableName}\n";
                sql += $" ({InsertCols.pJoin(", ")})\n";
                sql += $" Values ({InsertVals.pJoin(", ")});\n";

                SQLRow.Add(sql);
            }

            string SQL = $"{SQLRow.pJoin("\n")}";

            return SQL;
        }

        // DataTable 依PK Columns轉成 Update 語法
        public string DataTableToUpdateSQL(DataTable dt, string DbName = "DB_OPD", params string[] PkCols)
        {
            List<string> SQLRow = new List<string>();

            foreach (DataRow r in dt.Rows)
            {
                List<string> SetKV = new List<string>();
                List<string> WhereKV = new List<string>();

                foreach (DataColumn col in dt.Columns)
                {
                    string colName = col.ColumnName;
                    string value = _dbtool.ISGenerator(r[colName]);

                    if (colName.pIn(PkCols))
                    {
                        WhereKV.Add($"{colName} = {value}");
                    }
                    else
                    {
                        SetKV.Add($"{colName} = {value}");
                    }
                }

                string sql = $"Update {DbName}.dbo.{dt.TableName}\n";
                sql += $"Set {SetKV.pJoin(", ")}\n";
                sql += $"Where {WhereKV.pJoin(" And ")};\n";

                SQLRow.Add(sql);
            }

            string SQL = $"{SQLRow.pJoin("\n")}";

            return SQL;
        }


        public void DataTableFill(DataSet ds, string tablename, string SQL, SqlConnection conn)
        {
            SqlDataAdapter sqlAdpt = new SqlDataAdapter();
            SqlCommand cmd = new SqlCommand();

            //System.Diagnostics.Debug.Print(SQL);

            cmd = conn.CreateCommand();
            cmd.CommandText = SQL;
            sqlAdpt.SelectCommand = cmd;

            if (ds.Tables.CanRemove(ds.Tables[tablename]))
            {
                ds.Tables.Remove(tablename);
            }
            sqlAdpt.Fill(ds, tablename);
            cmd = null;
        }


        public void DataTableInsertDB(DataTable dt, string tablename, SqlConnection conn)
        {
            SqlBulkCopy SBC = new SqlBulkCopy(conn);
            SBC.BatchSize = 100;
            SBC.DestinationTableName = ".dbo." + tablename;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                SBC.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
            }
            SBC.WriteToServer(dt);

            SBC.Close();
        }

        // 指定DB時使用獨立的Overloading Function
        // 以防TmpDB出問題
        public void DataTableInsertDB(DataTable dt, string tablename, SqlConnection conn, string DB)
        {
            SqlBulkCopy SBC = new SqlBulkCopy(conn);
            SBC.BatchSize = 100;
            SBC.DestinationTableName = $"{DB}.dbo." + tablename;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                SBC.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
            }
            SBC.WriteToServer(dt);

            SBC.Close();
        }


        /// <summary>
        /// 將CSV內容字串轉成DataTable(全部欄位都視為string，有需要請自行轉換), 不支援""包夾格式
        /// </summary>
        /// <param name="csvContent">整個CSV的內容</param>
        /// <param name="firstRowAsHeader">第一列是否為欄位名稱，若無則以C1, C2自動命名</param>
        /// <returns></returns>
        public DataTable ConvertCSVtoDataTable(string csvContent, bool firstRowAsHeader)
        {
            DataTable t = new DataTable();
            using (StringReader sr = new StringReader(csvContent))
            {
                string line = null;
                bool colCreated = false;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] p = line.Split(',');
                    if (!colCreated)
                    {
                        int idx = 1;
                        foreach (string s in p)
                        {
                            try
                            {
                                t.Columns.Add(
                                    //第一欄是否有欄位名稱? 無則用C1, C2自動編號
                                    firstRowAsHeader ? s.ToString() : "C" + idx.ToString(),
                                    typeof(string)
                                    );
                            }
                            catch (Exception e1)
                            {
                                throw new ApplicationException(
                                    "新增欄位失敗! 欄位名稱=" + s + "\n" + e1.ToString());
                            }
                            idx++;
                        }
                        colCreated = true;
                        //首欄若為欄名，則不當資料處理
                        if (firstRowAsHeader) continue;
                    }
                    try
                    {
                        t.Rows.Add(p);
                    }
                    catch (Exception e2)
                    {
                        throw new ApplicationException("資料匯入失敗!\n資料=" + line +
                            "\n錯誤訊息:" + e2.ToString());
                    }
                }
            }
            return t;
        }

        public Boolean DataTableToCSV(DataTable Table, string fileName, bool IsShowTitle = false)
        {
            StreamWriter sw = new StreamWriter(fileName, false);

            StringBuilder result = new StringBuilder();

            if (Table.Rows.Count > 0)
            {
                if (IsShowTitle)
                {
                    for (int i = 0; i < Table.Columns.Count; i++)
                    {
                        result.Append(Table.Columns[i].ColumnName);
                        result.Append(i == Table.Columns.Count - 1 ? "\r\n" : ",");
                    }
                    //sw.Write(result);
                    //result.Clear();
                }
                foreach (DataRow row in Table.Rows)
                {
                    for (int i = 0; i < Table.Columns.Count; i++)
                    {
                        result.Append(row[i].ToString());
                        result.Append(i == Table.Columns.Count - 1 ? "\r\n" : ",");
                    }
                    sw.Write(result);
                    result.Clear();
                }
            }

            sw.Close();
            return true;
        }


        // 清 dataSet 裡所有 DataTable 字串欄位的空白
        public void TrimAllTables(DataSet ds)
        {
            foreach (DataTable table in ds.Tables)
            {
                TrimTable(table);
            }
        }


        // 清所有 DataTable 字串欄位的空白
        public void TrimTable(DataTable table)
        {
            foreach (DataRow r in table.Rows)
            {
                foreach (DataColumn c in table.Columns)
                {
                    if (c.DataType == typeof(System.String) && !c.ReadOnly)
                        r[c] = r[c].ToString().Trim();
                    //Debug.Print(c.DataType.ToString());
                }
            }

            table.AcceptChanges();
        }

        public bool InsertDT(DataTable dt, SqlConnection conn, string SQL)
        {
            SqlTransaction sqlTrans = conn.BeginTransaction();

            if (SQL != "")
            {
                SqlCommand scmd = new SqlCommand();
                scmd.Connection = conn;

                try
                {
                    scmd.Transaction = sqlTrans;
                    scmd.CommandText = SQL;
                    scmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    sqlTrans.Rollback();
                    sqlErrtext = ex.Message.ToString();
                    return false;
                }
            }

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.KeepIdentity, sqlTrans))
            {
                bulkCopy.ColumnMappings.Clear();
                bulkCopy.DestinationTableName = "dbo." + dt.TableName.ToString();
                //設定一個批次量寫入多少筆資料
                bulkCopy.BatchSize = 1000;
                //設定逾時的秒數
                bulkCopy.BulkCopyTimeout = 60;
                //設定 NotifyAfter 屬性，以便在每複製 10000 個資料列至資料表後，呼叫事件處理常式。
                //bulkCopy.NotifyAfter = 1000;
                //bulkCopy.SqlRowsCopied += new SqlRowsCopiedEventHandler(OnSqlRowsCopied);                
                foreach (DataColumn dtcol in dt.Columns)
                {
                    bulkCopy.ColumnMappings.Add(dtcol.ColumnName, dtcol.ColumnName);
                }

                try
                {
                    bulkCopy.WriteToServer(dt, DataRowState.Added);
                }
                catch (Exception ex)
                {
                    sqlTrans.Rollback();
                    sqlErrtext = ex.Message.ToString();
                    return false;
                }

                sqlTrans.Commit();
                bulkCopy.Close();
                sqlTrans.Dispose();
                return true;
            }
        }

        public bool InsertDS(DataSet ds, SqlConnection conn, string SQL, string DbName)
        {
            SqlTransaction sqlTrans = conn.BeginTransaction();

            if (SQL != "")
            {
                SqlCommand scmd = new SqlCommand();
                scmd.Connection = conn;

                try
                {
                    scmd.Transaction = sqlTrans;
                    scmd.CommandText = SQL;
                    scmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    sqlTrans.Rollback();
                    sqlErrtext = ex.Message.ToString();
                    return false;
                }
            }

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.KeepIdentity, sqlTrans))
            {
                foreach (DataTable InsertDt in ds.Tables)
                {
                    bulkCopy.ColumnMappings.Clear();
                    bulkCopy.DestinationTableName = DbName + ".dbo." + InsertDt.TableName.ToString();
                    //設定一個批次量寫入多少筆資料
                    bulkCopy.BatchSize = 1000;
                    //設定逾時的秒數
                    bulkCopy.BulkCopyTimeout = 60;
                    //設定 NotifyAfter 屬性，以便在每複製 10000 個資料列至資料表後，呼叫事件處理常式。
                    //bulkCopy.NotifyAfter = 10000;
                    //bulkCopy.SqlRowsCopied += new SqlRowsCopiedEventHandler(OnSqlRowsCopied);

                    foreach (DataColumn dtcol in InsertDt.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(dtcol.ColumnName, dtcol.ColumnName);
                    }

                    try
                    {
                        bulkCopy.WriteToServer(InsertDt, DataRowState.Added);
                    }
                    catch (Exception ex)
                    {
                        sqlTrans.Rollback();
                        sqlErrtext = ex.Message.ToString();
                        return false;
                    }
                }

                sqlTrans.Commit();
                bulkCopy.Close();
                sqlTrans.Dispose();
                return true;
            }
        }

        public void OnSqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            //InsCnt = e.RowsCopied.ToString();
            //寫入中途要做的事 , e裡面就能取到受影響行數
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //sw.Stop();
            //xxx.Text = string.Format("耗時:{0}毫秒", sw.ElapsedMilliseconds.ToString());
        }

        /// <summary>
        /// RowFilter List陣列轉為查詢語法 like ... Or
        /// </summary>
        /// <param name="column"></param>
        /// <param name="strList"></param>
        /// <returns>LikeSyntax</returns>
        public string RowFilterstrListtoLikeSyntax(List<string> columns, List<string> strList, string liketype, bool IsRunSQL = false)
        {
            string str = "";
            foreach (string s in columns)
            {
                foreach (string l in strList)
                {
                    switch (liketype)
                    {
                        case "All":
                            if (IsRunSQL == true)
                            {
                                str += s + " collate Chinese_Taiwan_Stroke_CI_AS" + " like '%" + l + "%' or ";
                            }
                            else
                            {
                                str += s + " like '%" + l + "%' or ";
                            }
                            break;
                        case "First":
                            if (IsRunSQL == true)
                            {
                                str += s + " collate Chinese_Taiwan_Stroke_CI_AS" + " like '%" + l + "' or ";
                            }
                            else
                            {
                                str += s + " like '%" + l + "' or ";
                            }
                            break;
                        case "End":
                            if (IsRunSQL == true)
                            {
                                str += s + " collate Chinese_Taiwan_Stroke_CI_AS" + " like '" + l + "%' or ";
                            }
                            else
                            {
                                str += s + " like '" + l + "%' or ";
                            }
                            break;
                    }
                }
            }
            str = str.Substring(0, str.Length - 3);
            return str;
        }

        /// <summary>
        /// RowFilter List陣列轉為查詢語法 like ... Or
        /// </summary>
        /// <param name="column"></param>
        /// <param name="strList"></param>
        /// <returns>LikeSyntax</returns>
        public string RowFilterstrListtoLikeSyntax_Mutile(List<string> columns, List<string> strList, string liketype = "All", bool IsRunSQL = false)
        {
            string FiType = liketype;
            string str = "";
            foreach (string s in columns)
            {
                foreach (string l in strList)
                {
                    string[] Val = l.Split(' ');
                    str += $"(";
                    int i = 0;
                    foreach (var stV in Val)
                    {
                        if (i == 0)
                        {
                            FiType = "End";
                        }
                        i++;
                        switch (FiType)
                        {
                            case "All":
                                if (IsRunSQL == true)
                                {
                                    str += s + " collate Chinese_Taiwan_Stroke_CI_AS" + " like '%" + stV + "%' and ";
                                }
                                else
                                {
                                    str += s + " like '%" + stV + "%' and ";
                                }
                                break;
                            case "First":
                                if (IsRunSQL == true)
                                {
                                    str += s + " collate Chinese_Taiwan_Stroke_CI_AS" + " like '%" + stV + "' and ";
                                }
                                else
                                {
                                    str += s + " like '%" + stV + "' and ";
                                }
                                break;
                            case "End":
                                if (IsRunSQL == true)
                                {
                                    str += s + " collate Chinese_Taiwan_Stroke_CI_AS" + " like '" + stV + "%' and ";
                                }
                                else
                                {
                                    str += s + " like '" + stV + "%' and ";
                                }
                                break;
                        }
                        FiType = liketype;
                    }
                    str = $"{str.Substring(0, str.Length - 4)}) or ";
                }
            }
            str = str.Substring(0, str.Length - 3);
            return str;
        }

        /// <summary>
        /// DataTable轉置(Row==> Column)
        /// </summary>
        /// <param name="Source">來源(DataTable)</param>
        /// <param name="listKeepColumn">保留不轉置欄位</param>
        /// <param name="strPivotColumnName">需要轉置的欄位</param>
        /// <param name="strPivotColumnValue">轉置欄位對應的值</param>
        /// <returns></returns>
        public DataTable PivotDataTable(DataTable Source, List<string> listKeepColumn, string strPivotColumnName, string strPivotColumnValue, List<string> listOrderColumnName)
        {

            DataTable result = new DataTable();
            DataRow dr = null;
            List<string> oLastKey = new List<string>();
            int i = 0;
            int keyColumnIndex = 0;
            int pValIndex = 0;
            int pNameIndex = 0;
            string strTmpColumn = string.Empty;
            bool FirstRow = true;
            bool keyChanged = false;

            try
            {
                pValIndex = Source.Columns.IndexOf(strPivotColumnValue.Trim());
                pNameIndex = Source.Columns.IndexOf(strPivotColumnName.Trim());

                #region 建立欄位
                foreach (string _strColumnName in listKeepColumn)
                {
                    result.Columns.Add(Source.Columns[_strColumnName].ColumnName.ToString(), Source.Columns[_strColumnName].DataType);
                }
                #endregion

                dr = result.NewRow();

                #region 建立資料
                foreach (DataRow row in Source.Rows)
                {
                    keyColumnIndex = 0;
                    keyChanged = false;
                    if (!FirstRow)
                    {
                        while (!keyChanged && keyColumnIndex < listKeepColumn.Count)
                        {
                            if (row[listKeepColumn[keyColumnIndex]].ToString() != oLastKey[keyColumnIndex])
                            {
                                keyChanged = true;
                            }
                            keyColumnIndex++;
                        }
                    }
                    else
                    {
                        for (keyColumnIndex = 0; keyColumnIndex < listKeepColumn.Count; keyColumnIndex++)
                        {
                            oLastKey.Add(row[listKeepColumn[keyColumnIndex]].ToString());
                        }
                    }

                    if (keyChanged || FirstRow)
                    {

                        if (!FirstRow)
                        {
                            result.Rows.Add(dr);
                        }
                        dr = result.NewRow();

                        i = 0;
                        foreach (string _strColumnName in listKeepColumn)
                        {
                            dr[i] = row[result.Columns[_strColumnName].ColumnName];
                            i++;
                        }

                        FirstRow = false;
                        for (keyColumnIndex = 0; keyColumnIndex < listKeepColumn.Count; keyColumnIndex++)
                        {
                            oLastKey[keyColumnIndex] = row[listKeepColumn[keyColumnIndex]].ToString();
                        }
                    }

                    strTmpColumn = row[pNameIndex].ToString();
                    if (strTmpColumn.Length > 0)
                    {
                        if (!result.Columns.Contains(strTmpColumn) && strTmpColumn != null)
                        {
                            result.Columns.Add(strTmpColumn, Source.Columns[pValIndex].DataType);
                        }
                        dr[strTmpColumn] = row[pValIndex];
                    }
                }
                #endregion

                result.Rows.Add(dr);

            }
            catch (Exception)
            {
                throw;
            }
            return result;

        }

    }
}
