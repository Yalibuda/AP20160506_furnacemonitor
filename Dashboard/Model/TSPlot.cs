using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Dashboard.Model
{
    public class TSPlot : Report
    {
        public override System.Data.DataTable RawData
        {
            get
            {
                return base.RawData;
            }
            set
            {
                base.RawData = value;
            }
        }
        public override void Execute(Mtb.Project proj)
        {
            // 將資料匯入 Minitab
            if (_rawdata == null || _rawdata.Rows.Count == 0)
            {
                throw new ArgumentNullException("查無對應資料");
            }

            _rptLst = new List<IRptOutput>(); //重新建立一個分析結果列舉
            proj.Commands.Delete();
            proj.Worksheets.Delete();
            Mtb.Worksheet ws = proj.Worksheets.Add(1); //新增工作表            
            Mtblib.Tools.MtbTools.InsertDataTableToMtbWs(_rawdata, ws); //匯入資料至 Minitab

            StringBuilder cmnd = new StringBuilder();
            /*
            * Create TSPlot
            * 該圖會將數據於多個平面中顯示(k*1的方式)，所以要先設定 Panel 的安排方式
            * 
            */

            cmnd.AppendLine("Unstack ('DateTime' 'Temperature');");
            cmnd.AppendLine("Subscripts 'Area';");
            cmnd.AppendLine("After;");
            cmnd.AppendLine("VarNames.");
            for (int i = 0; i < CountPlotArea; i++)
            {
                cmnd.AppendLine(string.Format("NAME C{0} '{1}'.", 4 + 2 * (i + 1), AreaArray[i]));
            }
                
            string tmpCmnd = "Tsplot ";
            for (int i = 0; i < CountPlotArea; i++)
            {
                tmpCmnd = tmpCmnd + string.Format("C{0} ", (i + 3) * 2);
            }
            tmpCmnd = tmpCmnd + ";";
            cmnd.AppendLine(tmpCmnd);
            string gpath_tsplot = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab", string.Format("trend_{0}.jpg", _rawdata.TableName));
            cmnd.AppendFormat("gsave \"{0}\";\r\n", gpath_tsplot);
            cmnd.AppendLine("repl;");
            cmnd.AppendLine("jpeg;");
            cmnd.AppendLine("Scale 2;");
            cmnd.AppendLine("Min 100;");
            cmnd.AppendLine("Max 600;");
            cmnd.AppendLine("Min 100;");
            cmnd.AppendLine("Max 600;");
            cmnd.AppendLine("Same 2;");
            cmnd.AppendLine("Panel;");
            cmnd.AppendLine("Index;");
            cmnd.AppendLine("Stamp C3;");
            cmnd.AppendLine("Connect;");
            cmnd.AppendLine("Symbol;");
            cmnd.AppendLine("Type 6;");
            cmnd.AppendLine("Color 4;");
            cmnd.AppendLine("Size 0.7;");
            cmnd.AppendLine("Title;");
            cmnd.AppendLine("Footnote;");
            cmnd.AppendLine("FPer;");
            cmnd.AppendLine("Footnote;");
            cmnd.AppendLine("FPanel;");
            cmnd.AppendLine("NoDTitle;");
            cmnd.AppendLine("NoPerFootnote.");

            //刪除所有圖形
            cmnd.AppendLine("gmana;");
            cmnd.AppendLine("all;");
            cmnd.AppendLine("close;");
            cmnd.AppendLine("nopr.");

            string fpath = Mtblib.Tools.MtbTools.BuildTemporaryMacro("mymacro.mtb", cmnd.ToString());
            proj.ExecuteCommand(string.Format("exec \"{0}\" 1", fpath), ws);


            //將檔案轉為二進位陣列
            if (File.Exists(gpath_tsplot))
            {
                this.Contents.Add(new RptOutput()
                {
                    OType = MtbOType.GRAPH,
                    OutputInByteArr = File.ReadAllBytes(gpath_tsplot),
                    Tag = "TSPlot"
                });
            }
            //Tsplot 'Temperature_1' 'Temperature_2';
                //Scale 2;
                //  Min 100;
                //  Max 600;
                //  Min 100;
                //  Max 600;
                //Same  2;
                //Panel;
                //Index;
                //Stamp 'Plant_2';
                //Connect;
                //Symbol;
                //Title;
                //Footnote;
                //  FPer;
                //Footnote;
                //  FPanel;
                //NoDTitle;
                //NoPerFootnote.
            this.Tag = "TSPlot";
        }

        public string Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        private string _tag = "";

        public int CountPlotArea 
        {
            get { return _countPlotArea; }
            set { _countPlotArea = value; }
        }
        private int _countPlotArea;

        public List<string> AreaArray
        {
            get 
            {
                return _areaArray;
            }
            set
            {
                _areaArray = value;
            }
        }
        private List<string> _areaArray;
    }
}
