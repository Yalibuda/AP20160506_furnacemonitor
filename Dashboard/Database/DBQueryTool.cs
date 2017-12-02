using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashboard.Database
{
    public static class DBQueryTool
    {
        /// <summary>
        /// 根據設定檔回傳 Sql Client 的 Connection string
        /// </summary>
        /// <param name="dbName">指定要連接的資料庫名稱</param>
        /// <returns></returns>
        public static string GetConnString(string dbName = null)
        {
            string _srvname = DBServer.Default.ServerName;
            string _port = DBServer.Default.Port;
            string _db = DBServer.Default.DBName;
            if (dbName != null) _db = dbName;
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
        /// 將簡單反覆運算的列舉集合轉換成 DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">可以是 Linq 查詢後的結果</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        /// <summary>
        /// 取得廠別的資訊
        /// </summary>
        /// <returns></returns>
        public static DataTable GetSiteInfo()
        {
            StringBuilder query = new StringBuilder();
            query.Append("SELECT DISTINCT SITE_ID FROM FURN_ITEM_INFO");
            DataTable dt = GetData(query.ToString(), GetConnString());
            return dt;
        }

        /// <summary>
        /// 取得指定廠別的熔爐項目資訊
        /// </summary>
        /// <param name="site_id"></param>
        /// <returns></returns>
        public static DataTable GetFurnItemInfo(string site_id = "501")
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT FURN_ITEM_INDEX, ITEM_NAME FROM FURN_ITEM_INFO");
            query.AppendFormat("WHERE SITE_ID='{0}'\r\n", site_id);
            DataTable dt = GetData(query.ToString(), GetConnString());
            return dt;
        }

        /// <summary>
        /// 取得 Real time SPC 的項目，並依照 FLAG 排序
        /// </summary>
        /// <param name="site_id"></param>
        /// <returns></returns>
        public static DataTable GetRealTimeSPCItems(string site_id = "501")
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT * FROM SPC_ITEM_INFO");
            query.AppendFormat("WHERE SITE='{0}'\r\n", site_id);
            query.AppendLine("ORDER BY FLAG");
            DataTable dt = GetData(query.ToString(), GetConnString());
            return dt;
        }

        /// <summary>
        /// 給定項目與時間範圍，取得多變量管制圖的資料(Pivot data)，產出欄位 [TIMESTAMP]、[各項目]、[CHART_PARA_INDEX]，
        /// 並且 DataTable 名稱以 SITE_ID+ 各ITEM_INDEX 串接而成
        /// </summary>
        /// <param name="site_id">廠別</param>
        /// <param name="items">項目列表(FURN_ITEM_INDEX)，不需要加引號</param>
        /// <param name="start">起始時間</param>
        /// <param name="end">結束時間</param>
        /// <returns></returns>
        public static DataTable GetPivotDataForMultivariateChart(string site_id, string[] items, string start, string end)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT ITEM_NAME FROM FURN_ITEM_INFO");
            query.AppendFormat("WHERE FURN_ITEM_INDEX IN ({0})\r\n", string.Join(",", items.Select(x => "'" + x + "'").ToArray()));
            query.AppendLine("ORDER BY CONVERT(INT, FURN_ITEM_INDEX)");
            string[] itemname
                = GetData(query.ToString(), GetConnString()).AsEnumerable()
                .Select(x => x.Field<string>("ITEM_NAME")).ToArray();
            string[] pivotCol = itemname.Zip(items, (x, y) => "[" + x + "]" + "=[" + y + "]").ToArray();

            query.Clear();
            query.AppendFormat("SELECT [TIMESTAMP],{0}, MAX(CHART_PARA_INDEX) AS CHART_PARA_INDEX FROM (\r\n", string.Join(",", pivotCol));
            query.AppendLine("SELECT PVT.*,");
            query.AppendFormat("FIRST_VALUE(C.CHART_PARA_INDEX) OVER (PARTITION BY PVT.TIMESTAMP, {0} ORDER BY C.APPLY_DATE DESC) AS CHART_PARA_INDEX\r\n",
                string.Join(",", items.Select(x => "PVT.[" + x + "]").ToArray()));
            query.AppendLine("FROM (");
            query.AppendLine("SELECT T.FURN_ITEM_INDEX, VALUE=AVG(T.VALUE), T.[TIMESTAMP] FROM (");
            query.AppendLine("SELECT A.FURN_DET_INDEX, A.FURN_ITEM_INDEX, A.RPT_DATETIME, A.VALUE , [TIMESTAMP] = DATEADD(HOUR, DATEDIFF(HOUR, 0, A.RPT_DATETIME), 0) FROM FURN_DETAIL A LEFT JOIN FURN_ITEM_INFO B ON A.FURN_ITEM_INDEX = B.FURN_ITEM_INDEX");
            query.AppendFormat("WHERE B.SITE_ID='{0}' AND A.FURN_ITEM_INDEX IN ({1})\r\n"
                , site_id, string.Join(",", items.Select(x => "'" + x + "'").ToArray()));
            query.AppendFormat("AND RPT_DATETIME BETWEEN '{0}' AND '{1}'\r\n", start, end);
            query.AppendLine(") AS T GROUP BY T.FURN_ITEM_INDEX, T.[TIMESTAMP]) AS TT");
            query.AppendFormat("PIVOT ( AVG(TT.VALUE) FOR TT.FURN_ITEM_INDEX IN ({0})) AS PVT LEFT JOIN CHART_PARAMETER C\r\n",
                string.Join(",", items.Select(x => "[" + x + "]").ToArray()));
            query.AppendFormat("ON C.APPLY_DATE <= PVT.TIMESTAMP AND C.ITEM_LIST='{0}') AS FINAL\r\n",
                string.Join(",", items));
            query.AppendFormat("GROUP BY [TIMESTAMP],{0}\r\n", string.Join(",", items.Select(x => "[" + x + "]").ToArray()));
            query.AppendLine("ORDER BY [TIMESTAMP]");
            DataTable dt = GetData(query.ToString(), GetConnString());
            dt.TableName = site_id + string.Join("", items);
            return dt;
        }


        /// <summary>
        /// 依據指定的 SPC ITEM TABLE 查詢所有資料
        /// </summary>
        /// <param name="spcItemTable"></param>
        /// <returns></returns>
        public static DataTable SplitItemListFromRealTimeSPCItem(DataTable spcItemTable)
        {
            string[] item_list = spcItemTable.AsEnumerable().Select(r => r.Field<string>("ITEM_LIST")).ToArray();
            List<string> splitedItemList = new List<string>();
            for (int i = 0; i < item_list.Length; i++)
            {
                string itemLst = item_list[i];
                splitedItemList.AddRange(itemLst.Split(','));
            }
            splitedItemList = splitedItemList.Select(x => "'" + x + "'").ToList();

            string[] site_list = spcItemTable.AsEnumerable().Select(r => "'" + r.Field<string>("SITE") + "'").ToArray();
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT FURN_ITEM_INDEX, SITE_ID, ITEM_NAME, FLAG FROM FURN_ITEM_INFO");
            query.AppendFormat("WHERE SITE IN ({0}) AND FURN_ITEM_INDEX IN ({1})\r\n",
                string.Join(",", site_list), string.Join(",", splitedItemList));
            DataTable dt = GetData(query.ToString(), GetConnString());
            return dt;
        }

        /// <summary>
        /// 比較兩個 DataTable 的內容是否完全相同
        /// </summary>
        /// <param name="dt1"></param>
        /// <param name="dt2"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool CompareDataTableRow(DataTable dt1, DataTable dt2)
        {
            if (dt1 == null && dt2 == null) return true; // 兩個都是 null 直接相等XD
            if (dt1 == null || dt2 == null) return false; // 一個 null 一個不是...直接 say no

            bool result = true;
            //長度
            if (dt1.Rows.Count != dt2.Rows.Count || dt1.Columns.Count != dt2.Columns.Count)
            {
                return false;
            }

            //內容
            DataRow[] dt1Rows = dt1.Rows.Cast<DataRow>().ToArray();
            DataRow[] dt2Rows = dt2.Rows.Cast<DataRow>().ToArray();
            for (int i = 0; i < dt1Rows.Length; i++)
            {
                var arr1 = dt1Rows[i].ItemArray;
                var arr2 = dt2Rows[i].ItemArray;
                if (!arr1.SequenceEqual(arr2))
                {
                    result = false;
                    break;
                }
            }
            return result;

        }

        /// <summary>
        /// 取得 ItemList 對應的所有熔爐量測項名稱，返回文字陣列
        /// </summary>
        /// <param name="itemList"></param>
        /// <returns></returns>
        public static string[] GetFurnNameByItemList(string itemList)
        {
            if (itemList == null || itemList == string.Empty) return null;
            string[] item_List = itemList.Split(',').Select(x => "'" + x + "'").ToArray();
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT ITEM_NAME FROM FURN_ITEM_INFO");
            query.AppendFormat("WHERE FURN_ITEM_INDEX IN ({0})\r\n", string.Join(",", item_List));
            DataTable dt = GetData(query.ToString(), GetConnString());
            string[] item_Name = dt.Rows.Cast<DataRow>().Select(x => x.Field<string>("ITEM_NAME")).ToArray();
            return item_Name;
        }

        /// <summary>
        /// 給定項目與時間範圍，取得相關性分析的資料(Pivot data)，產出欄位 [TIMESTAMP]、[BK]、[各項目]，
        /// 並且 DataTable 名稱以 SITE_ID + 各ITEM_INDEX 串接而成
        /// </summary>
        /// <param name="site_id">廠別</param>
        /// <param name="items">項目列表(FURN_ITEM_INDEX)，不需要加引號</param>
        /// <param name="start">起始時間</param>
        /// <param name="end">結束時間</param>
        /// <returns></returns>
        public static DataTable GetPivotDataForBKCorrelation(string site_id, string[] items, string start, string end)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT ITEM_NAME FROM FURN_ITEM_INFO");
            query.AppendFormat("WHERE FURN_ITEM_INDEX IN ({0})\r\n", string.Join(",", items.Select(x => "'" + x + "'").ToArray()));
            query.AppendLine("ORDER BY CONVERT(INT, FURN_ITEM_INDEX)");
            string[] itemname
                = GetData(query.ToString(), GetConnString()).AsEnumerable()
                .Select(x => x.Field<string>("ITEM_NAME")).ToArray();
            string[] pivotCol = itemname.Zip(items, (x, y) => "[" + x + "]" + "=[" + y + "]").ToArray();
            query.Clear();
            //query.AppendFormat("SELECT BK.GROUP_ID, BK.TIMESTAMP, FURN.RPT_TIMEHOUR ,BK.BK, {0} FROM vw_bkhour BK LEFT JOIN (\r\n",
            //    string.Join(",", itemname.Select(x => "FURN.[" + x + "]")));
            //query.AppendFormat("SELECT {0}, RPT_TIME, RPT_TIMEHOUR, SITE_ID FROM (\r\n",
            //    string.Join(",", pivotCol));
            //query.AppendLine("SELECT A.SITE_ID, A.FURN_ITEM_INDEX, A.RPT_TIMEHOUR, RPT_TIME=DATEADD(HOUR,8, A.RPT_TIMEHOUR), VALUE=AVG(A.VALUE) FROM (");
            //query.AppendLine("SELECT Y.SITE_ID, Y.FURN_ITEM_INDEX, X.VALUE, RPT_TIMEHOUR= DATEADD(HOUR,DATEDIFF(HOUR,0, X.RPT_DATETIME),0) FROM FURN_DETAIL X INNER JOIN FURN_ITEM_INFO Y ");
            //query.AppendLine("ON X.FURN_ITEM_INDEX = Y.FURN_ITEM_INDEX) A");
            //query.AppendLine("GROUP BY A.SITE_ID, A.FURN_ITEM_INDEX, A.RPT_TIMEHOUR");
            //query.AppendLine(") GPTABLE");
            //query.AppendLine("PIVOT (");
            //query.AppendFormat("AVG(GPTABLE.VALUE) FOR GPTABLE.FURN_ITEM_INDEX IN ({0})\r\n",
            //    string.Join(",", items.Select(x => "[" + x + "]").ToArray()));
            //query.AppendLine(") AS PVT");
            //query.AppendLine(") FURN on FURN.RPT_TIME=BK.TIMESTAMP and BK.SITE_ID=FURN.SITE_ID");
            //query.AppendFormat("WHERE BK.GROUP_ID='0' AND BK.SITE_ID='{0}' AND BK.TIMESTAMP between '{1}' and '{2}'\r\n",
            //    site_id, start, end);
            //query.AppendLine("ORDER BY BK.TIMESTAMP");
            query.AppendFormat("select a.GROUP_ID, a.TIMESTAMP, a.BK, {0} from vw_bkhour a \r\n", string.Join(",", pivotCol));
            for (int i = 0; i < itemname.Length; i++)
            {
                query.AppendLine("left join (");
                query.AppendLine("select a.SITE_ID,[RPT_TIMEHOUR]= DATEADD(HOUR, ");
                query.AppendFormat("(select isnull((select LAGHOUR from FURN_BK_LAG_INFO where FURN_ITEM_INDEX='{0}'),0)),RPT_TIMEHOUR),\r\n", items[i]);
                query.AppendFormat("[{0}]= avg(a.VALUE) from vw_furnacedata a where a.FURN_ITEM_INDEX='{0}' group by SITE_ID, RPT_TIMEHOUR) x{1} \r\n"
                    , items[i], i + 1);
                query.AppendFormat("on a.TIMESTAMP = x{0}.RPT_TIMEHOUR \r\n", i + 1);
            }

            query.AppendFormat("where a.SITE_ID='{0}' and a.GROUP_ID=0 and a.TIMESTAMP between '{1}' and '{2}' \r\n", site_id, start, end);

            query.AppendFormat("order by TIMESTAMP");

            query.AppendLine();

            DataTable dt = GetData(query.ToString(), GetConnString());
            dt.TableName = site_id + string.Join("", items);
            return dt;
        }

        /// <summary>
        /// 給定項目與時間範圍，取得相關性分析的資料(Pivot data)，產出欄位 [TIMESTAMP]、[BK1]~[BK?]、[各項目]，
        /// 並且 DataTable 名稱以 SITE_ID + 各ITEM_INDEX 串接而成 
        /// </summary>
        /// <param name="site_id"></param>
        /// <param name="items"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static DataTable GetPivotDataForMulBKCorrelation(string site_id, string[] items, string start, string end)
        {
            //Get information of item from db
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT ITEM_NAME FROM FURN_ITEM_INFO");
            query.AppendFormat("WHERE FURN_ITEM_INDEX IN ({0})\r\n", string.Join(",", items.Select(x => "'" + x + "'").ToArray()));
            query.AppendLine("ORDER BY CONVERT(INT, FURN_ITEM_INDEX)");
            string[] itemname
                = GetData(query.ToString(), GetConnString()).AsEnumerable()
                .Select(x => x.Field<string>("ITEM_NAME")).ToArray();
            string[] pivotCol = itemname.Zip(items.OrderBy(x => x), (x, y) => "[" + x + "]" + "=[" + y + "]").ToArray();
            query.Clear();

            query.AppendLine("select GROUP_ID from VW_BKHOUR");
            query.AppendFormat("where SITE_ID={0} and TIMESTAMP between '{1}' and '{2}' and GROUP_ID<>0 \r\n", site_id, start, end); //exclude overall group_id
            query.AppendLine("group by GROUP_ID order by GROUP_ID");
            decimal[] bks
                = GetData(query.ToString(), GetConnString()).AsEnumerable().Select(x => x.Field<decimal>("group_id")).ToArray();
            string[] bkCols = bks.Select(x => "[BK" + x + "]=" + "[" + x + "]").ToArray();
            query.Clear();

            StringBuilder pivotBkqueryString = new StringBuilder();
            pivotBkqueryString.AppendFormat("select SITE_ID, TIMESTAMP, {0} from ( \r\n", string.Join(",", bkCols));
            pivotBkqueryString.AppendLine("select SITE_ID, TIMESTAMP, GROUP_ID, BK from vw_bkhour");
            pivotBkqueryString.AppendFormat("where SITE_ID={0} and GROUP_ID<>0 and TIMESTAMP between '{1}' and '{2}' \r\n", site_id, start, end);
            pivotBkqueryString.AppendLine(") aa ");
            pivotBkqueryString.AppendLine("pivot (");
            pivotBkqueryString.AppendFormat("AVG(BK) for GROUP_ID in ({0})", string.Join(",", bks.Select(x => "[" + x + "]")));
            pivotBkqueryString.AppendLine(") pv");

            query.AppendFormat("SELECT a.*, {0} FROM ({1}) a \r\n", string.Join(",", pivotCol), pivotBkqueryString);
            for (int i = 0; i < itemname.Length; i++)
            {
                query.AppendLine("left join (");
                query.AppendLine("select a.SITE_ID,[RPT_TIMEHOUR]= DATEADD(HOUR, ");
                query.AppendFormat("(select isnull((select LAGHOUR from FURN_BK_LAG_INFO where FURN_ITEM_INDEX='{0}'),0)),RPT_TIMEHOUR),\r\n", items[i]);
                query.AppendFormat("[{0}]= avg(a.VALUE) from vw_furnacedata a where a.FURN_ITEM_INDEX='{0}' group by SITE_ID, RPT_TIMEHOUR) x{1} \r\n"
                    , items[i], i + 1);
                query.AppendFormat("on a.TIMESTAMP = x{0}.RPT_TIMEHOUR and a.SITE_ID=x{0}.SITE_ID \r\n", i + 1);
            }
            query.AppendFormat("order by TIMESTAMP");

            query.AppendLine();

            DataTable dt = GetData(query.ToString(), GetConnString());
            dt.TableName = site_id + string.Join("", items);
            return dt;
        }

        #region

        /// <summary>
        /// 給定項目與時間範圍，取得相關性分析的資料(Pivot data)，產出欄位 [TIMESTAMP]、[BK]、[各項目]，
        /// 並且 DataTable 名稱以 SITE_ID + 各ITEM_INDEX 串接而成
        /// </summary>
        /// <param name="site_id">廠別</param>
        /// <param name="items">項目列表(FURN_ITEM_INDEX)，不需要加引號</param>
        /// <param name="start">起始時間</param>
        /// <param name="end">結束時間</param>
        /// <returns></returns>
        public static DataTable GetPivotDataForItemsCorrelation(string site_id, string[] items, string start, string end)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT ITEM_NAME FROM FURN_ITEM_INFO");
            query.AppendFormat("WHERE FURN_ITEM_INDEX IN ({0})\r\n", string.Join(",", items.Select(x => "'" + x + "'").ToArray()));
            query.AppendLine("ORDER BY CONVERT(INT, FURN_ITEM_INDEX)");
            string[] itemname
                = GetData(query.ToString(), GetConnString()).AsEnumerable()
                .Select(x => x.Field<string>("ITEM_NAME")).ToArray();
            string[] pivotCol = itemname.Zip(items, (x, y) => "[" + x + "]" + "=[" + y + "]").ToArray();
            query.Clear();
            //query.AppendFormat("SELECT BK.GROUP_ID, BK.TIMESTAMP, FURN.RPT_TIMEHOUR ,BK.BK, {0} FROM vw_bkhour BK LEFT JOIN (\r\n",
            //    string.Join(",", itemname.Select(x => "FURN.[" + x + "]")));
            //query.AppendFormat("SELECT {0}, RPT_TIME, RPT_TIMEHOUR, SITE_ID FROM (\r\n",
            //    string.Join(",", pivotCol));
            //query.AppendLine("SELECT A.SITE_ID, A.FURN_ITEM_INDEX, A.RPT_TIMEHOUR, RPT_TIME=DATEADD(HOUR,8, A.RPT_TIMEHOUR), VALUE=AVG(A.VALUE) FROM (");
            //query.AppendLine("SELECT Y.SITE_ID, Y.FURN_ITEM_INDEX, X.VALUE, RPT_TIMEHOUR= DATEADD(HOUR,DATEDIFF(HOUR,0, X.RPT_DATETIME),0) FROM FURN_DETAIL X INNER JOIN FURN_ITEM_INFO Y ");
            //query.AppendLine("ON X.FURN_ITEM_INDEX = Y.FURN_ITEM_INDEX) A");
            //query.AppendLine("GROUP BY A.SITE_ID, A.FURN_ITEM_INDEX, A.RPT_TIMEHOUR");
            //query.AppendLine(") GPTABLE");
            //query.AppendLine("PIVOT (");
            //query.AppendFormat("AVG(GPTABLE.VALUE) FOR GPTABLE.FURN_ITEM_INDEX IN ({0})\r\n",
            //    string.Join(",", items.Select(x => "[" + x + "]").ToArray()));
            //query.AppendLine(") AS PVT");
            //query.AppendLine(") FURN on FURN.RPT_TIME=BK.TIMESTAMP and BK.SITE_ID=FURN.SITE_ID");
            //query.AppendFormat("WHERE BK.GROUP_ID='0' AND BK.SITE_ID='{0}' AND BK.TIMESTAMP between '{1}' and '{2}'\r\n",
            //    site_id, start, end);
            //query.AppendLine("ORDER BY BK.TIMESTAMP");
            query.AppendFormat("select a.GROUP_ID, a.TIMESTAMP, a.BK, {0} from vw_bkhour a\r\n", string.Join(",", pivotCol));
            for (int i = 0; i < itemname.Length; i++)
            {
                query.AppendLine("left join (");
                query.AppendLine("select a.SITE_ID,[RPT_TIMEHOUR]= DATEADD(HOUR, ");
                query.AppendFormat("(select isnull((select LAGHOUR from FURN_BK_LAG_INFO where FURN_ITEM_INDEX='{0}'),0)),RPT_TIMEHOUR),\r\n", items[i]);
                query.AppendFormat("[{0}]= avg(a.VALUE) from vw_furnacedata a where a.FURN_ITEM_INDEX='{0}' group by SITE_ID, RPT_TIMEHOUR) x{1}\r\n"
                    , items[i], i + 1);
                query.AppendFormat("on a.TIMESTAMP = x{0}.RPT_TIMEHOUR\r\n", i + 1);
            }
            query.AppendFormat("where a.SITE_ID='{0}' and a.GROUP_ID=0 and a.TIMESTAMP between '{1}' and '{2}'\r\n", site_id, start, end);
            query.AppendFormat("order by TIMESTAMP");

            query.AppendLine();

            DataTable dt = GetData(query.ToString(), GetConnString());
            dt.TableName = site_id + string.Join("", items);
            return dt;
        }

        #endregion

        /// <summary>
        /// 給定項目與時間範圍，取得實驗室物性測試的結果
        /// </summary>
        /// <param name="items">測試項(PROP_INDEX)和成分(TEST_ITEM_INDEX)組成的陣列，e.g.{"12/38","13/45"}</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static DataTable GetPropertyTestData(string[] items, string start, string end)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("select b.PROD_NAME, c.ITEM_NAME,c.ITEM_UNIT,a.VALUE,a.TEST_DATE,a.PROP_INDEX, a.TEST_ITEM_INDEX ,a.MAT_INDEX,a.TEST_SAMPLE_ID from TEST_DETAIL a");
            query.AppendLine("left join PROPERTY_INFO b on a.PROP_INDEX = b.PROP_INDEX");
            query.AppendLine("left join TEST_ITEM_INFO c on a.TEST_ITEM_INDEX = c.TEST_ITEM_INDEX");
            query.AppendFormat("where CONCAT(a.PROP_INDEX,'/',a.TEST_ITEM_INDEX) in ({0}) \r\n", string.Join(",", items.Select(x => "'" + x + "'")));
            query.AppendFormat("and TEST_DATE between '{0}' and '{1}' \r\n", start, end);
            query.AppendLine("order by a.PROP_INDEX, c.ITEM_NAME, a.TEST_DATE");

            DataTable dt = GetData(query.ToString(), GetConnString("LIMS"));
            dt.TableName = "PROP_ANALYSIS";
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
            //string connString = "";
            //if (Environment.Is64BitOperatingSystem)
            //{
            //    connString = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Extended Properties=""text;HDR=Yes;FMT=Delimited;"";", fileDir);
            //}
            //else
            //{
            //    connString = string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}; Extended Properties=""text;HDR=Yes;FMT=Delimited;"";", fileDir);
            //}            
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


        #region Furnace Wall DBtool

        /// <summary>
        /// 取得爐壁廠別的資訊
        /// </summary>
        /// <returns></returns>
        public static DataTable GetWallSiteInfo()
        {
            StringBuilder query = new StringBuilder();
            query.Append("SELECT DISTINCT Plant FROM WallTemperature");
            DataTable dt = GetData(query.ToString(), GetWallConnString());
            return dt;
        }

        /// <summary>
        /// 根據設定檔回傳 Sql Client 的 Connection string
        /// </summary>
        /// <returns></returns>
        public static string GetWallConnString()
        {
            string _srvname = DBServer.Default.ServerName;
            string _port = DBServer.Default.Port;
            string _db = "furnace";
            string _uid = DBServer.Default.UserID;
            string _psw = DBServer.Default.Password;
            string connString =
                string.Format("Server={0},{1};Initial Catalog={2};Persist Security Info=True;UID={3};PWD={4};Connection Timeout=10",
                _srvname, _port, _db, _uid, _psw);
            return connString;
        }


        /// <summary>
        /// get furnace wall data
        /// </summary>
        /// <returns></returns>
        public static DataTable GetWallData(string site_id, string start, string end)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT * FROM WallTemperature a");
            query.AppendFormat("where a.Plant='{0}' and a.DateTime between '{1}' and '{2}'\r\n", site_id, start, end);
            query.AppendFormat("order by a.Area, a.DateTime");
            query.AppendLine();

            DataTable dt = GetData(query.ToString(), GetWallConnString());
            dt.TableName = site_id + "rawdata";
            return dt;
        }

        /// <summary>
        /// get furnace wall distinct area data
        /// </summary>
        /// <returns></returns>
        public static DataTable GetWallAreaData(string site_id, string start, string end)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT DISTINCT AREA FROM WallTemperature a");
            query.AppendFormat("where a.Plant='{0}' and a.DateTime between '{1}' and '{2}'\r\n", site_id, start, end);
            query.AppendLine();

            DataTable dt = GetData(query.ToString(), GetWallConnString());
            dt.TableName = "Distinct Area" + site_id;
            return dt;
        }
        #endregion
    }
}
