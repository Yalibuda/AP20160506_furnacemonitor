using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashboard
{
    public static class Minitab
    {
        public static Mtb.Application App = null;
        private static int appID = 0;
        /// <summary>
        /// 啟動 Minitab
        /// </summary>
        public static void Initialize()
        {
            List<int> procIDs = new List<int>();
            try
            {
                //建立一開始系統內包含 Minitab 的程序
                System.Diagnostics.Process[] initProc = System.Diagnostics.Process.GetProcessesByName("Mtb");

                if (initProc.Length > 0)
                {
                    procIDs = initProc.Select(x => x.MainWindowHandle.ToInt32()).ToList();
                }

                bool isNew = false;
                while (!isNew) //判斷現在是否有以視窗模式開啟的 Minitab
                {
                    App = new Mtb.Application();
                    if (procIDs.Count > 0 && procIDs.Contains(App.Handle))
                        isNew = false;
                    else
                        isNew = true;
                }
                App.UserInterface.DisplayAlerts = false;
                App.UserInterface.Visible = false;
                System.Diagnostics.Process[] finalProc = System.Diagnostics.Process.GetProcessesByName("Mtb"); //取得目前所有 Minitab 的程序
                int[] ids = finalProc.Where(x => !initProc.Any(y => y.Id == x.Id)).Select(x => x.Id).ToArray(); //只取得剛剛建立的程序，ID 會固定
                appID = ids[0];
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\n無法啟動 Minitab，請確認是否有安裝 Minitab 或有足夠的授權數");
            }

        }
        public static Mtb.Project Project { get { return App.ActiveProject; } }
        /// <summary>
        /// 關閉 Minitab 程式
        /// </summary>
        public static void Quit()
        {
            try
            {
                App.Quit();
                System.Diagnostics.Process[] proc = System.Diagnostics.Process.GetProcessesByName("Mtb");
                for (int i = proc.Length; i-- > 0; )
                {
                    if (proc[i].Id == appID) proc[i].Kill(); //只將目前使用的 Minitab 關掉
                }
            }
            catch
            {

            }
            finally
            {
                App = null;
                GC.Collect();
            }


        }

        /// <summary>
        /// 重新啟動 Minitab 程式
        /// </summary>
        public static void ReStart()
        {
            Quit();
            Initialize();
        }
    }
}
