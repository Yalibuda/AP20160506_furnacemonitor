using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Mtb.Application mtbApp = new Mtb.Application();
            Mtb.Project proj = mtbApp.ActiveProject;
            Mtb.Worksheet ws = proj.ActiveWorksheet;
            mtbApp.UserInterface.Visible = true;
            mtbApp.UserInterface.DisplayAlerts = false;
            
            proj.ExecuteCommand("rand 10 c1");
            for (int i = 0; i < 1100; i++)
            {
                Console.WriteLine(i+1);
                proj.ExecuteCommand(string.Format("Note line {0}",i+1));
                proj.Commands.Delete();
            }

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
