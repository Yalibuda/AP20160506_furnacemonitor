using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace Dashboard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private static Mutex mutex = new Mutex(true, "{EA816E80-C562-4D19-BF74-45CFC78950F7}");
        private static MainWindow mainWindow = null;
        App()
        {
            InitializeComponent();
        }


        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                SplashScreen splashScreen = new SplashScreen("SplashScreen.png");
                
                splashScreen.Show(true,true);
                App app = new App();
                mainWindow = new MainWindow();
                
                app.Run(mainWindow);
                mutex.ReleaseMutex();
                
            }
            else
            {
                MessageBox.Show("一次只能開啟一個應用程式");
            }
            
            
        }
    }
}
