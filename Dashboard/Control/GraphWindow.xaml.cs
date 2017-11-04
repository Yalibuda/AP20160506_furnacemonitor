using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dashboard.Control
{
    /// <summary>
    /// GraphWindow.xaml 的互動邏輯
    /// </summary>
    public partial class GraphWindow
    {
        public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(GraphWindow), new UIPropertyMetadata(string.Empty));
        public static readonly DependencyProperty TitleToolTipProperty =
        DependencyProperty.Register("TitleToolTip", typeof(string), typeof(GraphWindow), new UIPropertyMetadata(string.Empty));
        public static readonly DependencyProperty IconPathProperty =
        DependencyProperty.Register("IconPath", typeof(string), typeof(GraphWindow), new UIPropertyMetadata(string.Empty));
        public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register("ImgSource", typeof(ImageSource), typeof(GraphWindow), new UIPropertyMetadata(null));
        //public static readonly DependencyProperty MyWidthProperty =
        //DependencyProperty.Register("MyWidth", typeof(string), typeof(GraphWindow), new UIPropertyMetadata("410"));
        //public static readonly DependencyProperty MyHeightProperty =
        //DependencyProperty.Register("MyHeight", typeof(string), typeof(GraphWindow), new UIPropertyMetadata("320"));
        public static readonly DependencyProperty GWidthProperty =
        DependencyProperty.Register("GWidth", typeof(string), typeof(GraphWindow), new UIPropertyMetadata("400"));
        public static readonly DependencyProperty GHeightProperty =
        DependencyProperty.Register("GHeight", typeof(string), typeof(GraphWindow), new UIPropertyMetadata("270"));

        public GraphWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 指定或取得圖形視窗的標題
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        /// <summary>
        /// 指定或取得圖形視窗的標題提示
        /// </summary>
        public string TitleToolTip
        {
            get { return (string)GetValue(TitleToolTipProperty); }
            set { SetValue(TitleToolTipProperty, value); }
        }

        /// <summary>
        /// 指定或取得圖形視窗的圖標
        /// </summary>
        public string IconPath
        {
            get { return (string)GetValue(IconPathProperty); }
            set { SetValue(IconPathProperty, value); }
        }

        /// <summary>
        /// 指定或取得圖形來源
        /// </summary>
        public ImageSource ImgSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set
            {
                SetValue(ImageSourceProperty, value);
            }
        }

        /// <summary>
        /// 指定或取得圖形視窗寬度
        /// </summary>
        public string GWidth
        {
            get { return (string)GetValue(GWidthProperty); }
            set { SetValue(GWidthProperty, value); }
        }

        /// <summary>
        /// 指定或取得圖形視窗高度
        /// </summary>
        public string GHeight
        {
            get { return (string)GetValue(GHeightProperty); }
            set { SetValue(GHeightProperty, value); }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {

        }
    }

}
