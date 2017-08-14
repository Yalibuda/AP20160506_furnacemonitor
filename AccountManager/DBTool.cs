using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager
{
    public static class DBTool
    {
        /// <summary>
        /// 根據設定檔回傳 Sql Client 的 Connection string
        /// </summary>
        /// <returns></returns>
        public static string GetConnString()
        {
            string _srvname = DBServer.Default.ServerName;
            string _port = DBServer.Default.Port;
            string _db = DBServer.Default.DBName;
            string _uid = DBServer.Default.UserID;
            string _psw = DBServer.Default.Password;
            string connString =
                string.Format("Server={0},{1};Initial Catalog={2};Persist Security Info=True;UID={3};PWD={4};Connection Timeout=10",
                _srvname, _port, _db, _uid, _psw);
            return connString;
        }

        /// <summary>
        /// 取得指定資料庫特定搜尋條件下的資料表
        /// </summary>
        /// <param name="queryString">搜尋SQL字串</param>
        /// <param name="connString">連接SQL字串</param>
        /// <returns></returns>
        public static DataTable GetData(string queryString, string connString)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand sql = new SqlCommand(queryString, conn))
                {
                    try
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(sql);
                        da.Fill(dt);
                        conn.Close();
                        da.Dispose();
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// Read csv file to a System.Data.DataTable
        /// </summary>
        /// <param name="filepath">a path with file name and extension</param>
        /// <returns></returns>
        public static DataTable ReadCSVFile(string filepath)
        {
            //Check if the file & path exist?
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException(filepath);
            }

            string fileDir = Directory.GetParent(filepath).ToString();
            string fileName = Path.GetFileName(filepath);

            string connString = string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}; Extended Properties=""text;HDR=Yes;FMT=Delimited;"";", fileDir);
           
            using (OleDbConnection conn = new OleDbConnection(connString))
            {
                try
                {
                    conn.Open();
                    OleDbCommand cmnd = new OleDbCommand("SELECT * FROM " + fileName, conn);
                    OleDbDataAdapter adapter = new OleDbDataAdapter(cmnd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }


        }
    }
}
