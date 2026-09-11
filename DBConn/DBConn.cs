using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebToolNet.DBConn
{
    public class DBConn
    {
        public bool ExecuteFail = false;
        public string _ExecuteFailMsg = string.Empty;

        // 連哪個 DB 由 Program.cs 的 Database:Target 決定後注入進來。
        // （.NET Framework 版本是讀 C:\HIS2\DBConfig.xml 自己判斷，那個做法檔案不見時會靜靜
        //   連上正式 DB，已經拿掉，見 Docs/NET10-MVC-MSSQL-DPAPI-Spec.md）
        public string connectString = string.Empty;

        public DBConn()
        {

        }

        public void executesqltrans(string sqlString)
        {
            ////先建立連線的字串，並宣告連線
            //string connectString = "Data Source = localhost; Initial Catalog = BlogTest; Integrated Security = SSPI";
            //SqlConnection sqlConnection = new SqlConnection(connectString);

            ////開啟連線
            //sqlConnection.Open();

            ////執行sql語法
            //SqlCommand command = new SqlCommand(sqlString, sqlConnection);

            ////關閉連線
            //sqlConnection.Close();

            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                ExecuteFail = false;
                _ExecuteFailMsg = string.Empty;
                sqlConnection.Open();
                using (SqlTransaction transaction = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = new SqlCommand(sqlString, sqlConnection, transaction);
                        command.ExecuteNonQuery();
                        transaction.Commit();
                        ExecuteFail = false;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ExecuteFail = true;
                        _ExecuteFailMsg = ex.Message;
                    }
                }
            }
        }

        // <summary>
        /// 對資料庫Execute SQL
        /// </summary>
        /// <param name="SQL">SQL語法</param>
        /// <param name="conn">傳入SqlConnection</param>
        /// <returns>回傳 DataTable</returns>
        public DataTable executesqldt(string sqlString)
        {
            using (SqlConnection connection = new SqlConnection(connectString))
            {
                DataTable dataTable = new DataTable();

                // 例外往上拋，不要吞掉，否則「DB掛掉」跟「查無資料」都會回傳空DataTable而分不出來
                connection.Open();

                using (SqlCommand command = new SqlCommand(sqlString, connection))
                {
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
                    {
                        dataAdapter.Fill(dataTable);
                    }
                }

                return dataTable;
            }
        }
    }
}
