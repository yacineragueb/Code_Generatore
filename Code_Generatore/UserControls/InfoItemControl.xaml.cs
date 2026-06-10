using System.Windows;
using System.Windows.Controls;

namespace Code_Generatore.UserControls
{
    /// <summary>
    /// Interaction logic for InfoItemControl.xaml
    /// </summary>
    public partial class InfoItemControl : UserControl
    {
        public InfoItemControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty = 
            DependencyProperty.Register("Title", typeof(string), typeof(InfoItemControl), 
                new PropertyMetadata(string.Empty));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(InfoItemControl),
                new PropertyMetadata(string.Empty));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
    }
}
