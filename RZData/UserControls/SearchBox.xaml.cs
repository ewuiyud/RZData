using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace RZData.UserControls
{
    public partial class SearchBox : UserControl
    {
        public SearchBox()
        {
            InitializeComponent();
            //若不不在此处引用icon，会导致找不到dll错误
            var icon = new PackIconMaterial
            {
                Kind = PackIconMaterialKind.Magnify
            };
        }


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SearchBox));
    }
}
