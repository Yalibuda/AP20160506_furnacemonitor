using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Model
{
    public static class AccManagerTool
    {
        /// <summary>
        /// 將具有指定規範的 DataTable (Username、Password, FirstName, LastName, Site_id, AccRole)上傳至指定的資料表中。
        /// 上傳至 Furnacemonitor 的 User_info 和 User_acc_info
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int UploadUsers(DataTable dt)
        {
            string connStr = DBTool.GetConnString();
            int r = 0;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand sqlCmnd = new SqlCommand("TRUNCATE TABLE UPLOAD_USER_ACC", conn))
                {
                    //先清除暫存表上的資料
                    sqlCmnd.ExecuteNonQuery();
                }
                //上傳 dt 內容至 upload_user_acc
                using (SqlBulkCopy sqlBC = new SqlBulkCopy(conn))
                {
                    sqlBC.BatchSize = 1000;
                    sqlBC.BulkCopyTimeout = 6000;

                    //設定要寫入的資料庫
                    sqlBC.DestinationTableName = "UPLOAD_USER_ACC";

                    //對應資料行                           
                    sqlBC.ColumnMappings.Add("USERNAME", "USERNAME");
                    sqlBC.ColumnMappings.Add("PASSWORD", "PASSWORD");
                    sqlBC.ColumnMappings.Add("FIRSTNAME", "FIRSTNAME");
                    sqlBC.ColumnMappings.Add("LASTNAME", "LASTNAME");
                    sqlBC.ColumnMappings.Add("SITE_ID", "SITE_ID");
                    sqlBC.ColumnMappings.Add("ACC_ROLE", "ACC_ROLE");

                    //開始寫入
                    sqlBC.WriteToServer(dt);
                }
                //執行 sql
                using (SqlCommand sqlCmnd = new SqlCommand())
                {
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("");
                    //建立暫存表
                    query.AppendLine("IF OBJECT_ID('tempdb..#NEWUSERS') IS NOT NULL DROP TABLE #NEWUSERS");
                    query.AppendLine("CREATE TABLE #NEWUSERS(");
                    query.AppendLine("USERNAME VARCHAR(32),");
                    query.AppendLine("PASSWORD VARCHAR(32),");
                    query.AppendLine("FIRSTNAME VARCHAR(32),");
                    query.AppendLine("LASTNAME VARCHAR(32),");
                    query.AppendLine("SITE_ID VARCHAR(10))");

                    query.AppendLine("IF OBJECT_ID('tempdb..#INSERED_ACCROLE') IS NOT NULL DROP TABLE #INSERED_ACCROLE");
                    query.AppendLine("CREATE TABLE #INSERED_ACCROLE(");
                    query.AppendLine("USERNAME VARCHAR(32),SITE_ID VARCHAR(10),ACC_ROLE VARCHAR(10))");

                    //把新增的項目加到暫存表
                    query.AppendLine("insert into #NEWUSERS(USERNAME, PASSWORD, FIRSTNAME, LASTNAME, SITE_ID)");
                    query.AppendLine("select * from (");
                    query.AppendLine("select a.USERNAME, a.PASSWORD, a.FIRSTNAME, a.LASTNAME, a.SITE_ID from UPLOAD_USER_ACC a, ");
                    query.AppendLine("(select USERNAME, SITE_ID from UPLOAD_USER_ACC");
                    query.AppendLine("except");
                    query.AppendLine("select USERNAME, SITE_ID from USERS) b");
                    query.AppendLine("where a.USERNAME = b.USERNAME and a.SITE_ID = b.SITE_ID) a");
                    
                    //把要新增的項目與角色加入暫存表
                    query.AppendLine("INSERT INTO #INSERED_ACCROLE(USERNAME,SITE_ID, ACC_ROLE)");
                    query.AppendLine("SELECT * FROM ( ");
                    query.AppendLine("SELECT a.USERNAME,a.SITE_ID, a.ACC_ROLE FROM UPLOAD_USER_ACC a, #NEWUSERS b");
                    query.AppendLine("WHERE a.USERNAME = b.USERNAME AND a.SITE_ID = b.SITE_ID ) a");
                    //新增 User
                    query.AppendLine("INSERT INTO USERS(USERNAME, PASSWORD, FIRSTNAME, LASTNAME, SITE_ID)");
                    query.AppendLine("SELECT USERNAME, PASSWORD, FIRSTNAME, LASTNAME, SITE_ID FROM #NEWUSERS");
                    //建立關係
                    query.AppendLine("INSERT INTO USER_ACC_INFO (USER_INDEX, ACC_INDEX)");
                    query.AppendLine("SELECT * FROM (");
                    query.AppendLine("SELECT a.USER_INDEX, c.ACC_INDEX FROM USERS a, #INSERED_ACCROLE b, ACCOUNTS c ");
                    query.AppendLine("WHERE a.USERNAME = b.USERNAME AND a.SITE_ID = b.SITE_ID AND b.ACC_ROLE = c.NAME) a");

                    //刪除暫存表
                    query.AppendLine("DROP TABLE #NEWUSERS");
                    query.AppendLine("DROP TABLE #INSERED_ACCROLE");

                    //建立暫存表
                    query.AppendLine("IF OBJECT_ID('tempdb..#DELINDEX') IS NOT NULL DROP TABLE #DELINDEX");
                    query.AppendLine("CREATE TABLE #DELINDEX (DEL_INDEX VARCHAR(32))");
                    //取得要刪除的User資訊
                    query.AppendLine("INSERT INTO #DELINDEX (DEL_INDEX)");
                    query.AppendLine("SELECT USER_INDEX FROM USERS a, ");
                    query.AppendLine("(SELECT USERNAME, SITE_ID FROM USERS");
                    query.AppendLine("EXCEPT");
                    query.AppendLine("SELECT USERNAME, SITE_ID FROM UPLOAD_USER_ACC) b");
                    query.AppendLine("WHERE a.USERNAME = b.USERNAME and a.SITE_ID = b.SITE_ID");
                    //刪除不存在的帳號與關聯
                    query.AppendLine("DELETE FROM USERS WHERE USER_INDEX in (SELECT DEL_INDEX FROM #DELINDEX)");
                    query.AppendLine("DELETE FROM USER_ACC_INFO WHERE USER_INDEX in (SELECT DEL_INDEX FROM #DELINDEX)");

                    //刪除暫存表
                    query.AppendLine("DROP TABLE #DELINDEX");

                    //更新帳號基本資訊(First name/Last name)
                    query.AppendLine("update USERS ");
                    query.AppendLine("set USERS.FIRSTNAME = b.FIRSTNAME, USERS.LASTNAME = b.LASTNAME");
                    query.AppendLine("from USERS a ");
                    query.AppendLine("inner join UPLOAD_USER_ACC b on ");
                    query.AppendLine("a.USERNAME = b.USERNAME and a.SITE_ID = b.SITE_ID and (a.FIRSTNAME!=b.FIRSTNAME OR a.LASTNAME!= b.LASTNAME)");

                    //更新關聯
                    query.AppendLine("update USER_ACC_INFO");
                    query.AppendLine("set USER_ACC_INFO.ACC_INDEX = b.ACC_INDEX");
                    query.AppendLine("from USER_ACC_INFO a ");
                    query.AppendLine("join (");
                    query.AppendLine("select a.USERNAME, a.SITE_ID, c.USER_INDEX, b.ACC_INDEX from UPLOAD_USER_ACC a ");
                    query.AppendLine("inner join ACCOUNTS b on a.ACC_ROLE = b.NAME");
                    query.AppendLine("inner join USERS c on a.USERNAME = c.USERNAME and a.SITE_ID = c.SITE_ID ");
                    query.AppendLine(") b on a.USER_INDEX = b.USER_INDEX");
                    query.AppendLine("where a.ACC_INDEX!=b.ACC_INDEX");

                    sqlCmnd.Connection = conn;
                    sqlCmnd.CommandText = query.ToString();
                    r = sqlCmnd.ExecuteNonQuery();
                }
            }
            return r;
        }
    }
}
