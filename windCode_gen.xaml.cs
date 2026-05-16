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
using System.Windows.Shapes;

namespace Code_Generatore
{
    /// <summary>
    /// Interaction logic for windCode_gen.xaml
    /// </summary>
    public partial class windCode_gen : Window
    {
        private MainWindow _loginWindow;
        public windCode_gen(MainWindow loginWindow)
        {
            InitializeComponent();
            _loginWindow = loginWindow;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _loginWindow.Show();
        }
    }
}
