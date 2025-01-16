using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RZData.Controls
{
    public class SearchTextBox : TextBox
    {
        private const string DefaultSearchText = "ÇëÊäÈë¹Ø¼ü´ÊËÑË÷";

        public static readonly DependencyProperty SearchCommandProperty =
            DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(SearchTextBox), new PropertyMetadata(null));
        public ICommand SearchCommand
        {
            get => (ICommand)GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
        }

        public SearchTextBox()
        {
            this.Text = DefaultSearchText;
            this.Foreground = Brushes.Gray;
            this.GotFocus += OnGotFocus;
            this.LostFocus += OnLostFocus;
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (this.Text == DefaultSearchText)
            {
                this.Text = string.Empty;
                this.Foreground = Brushes.Black;
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.Text))
            {
                SetDefaultSearchText();
            }
        }


        private void SetDefaultSearchText()
        {
            this.Text = DefaultSearchText;
            this.Foreground = Brushes.Gray;
        }
    }
}
