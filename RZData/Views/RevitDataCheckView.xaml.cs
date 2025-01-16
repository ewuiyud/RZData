﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RZData.Models;
using RZData.ViewModels;
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

namespace RZData.Views
{
    /// <summary>
    /// RevitDataCheckView.xaml 的交互逻辑
    /// </summary>
    public partial class RevitDataCheckView : Window
    {
        public RevitDataCheckView(UIDocument uiDocument)
        {
            InitializeComponent();
            DataContext = RevitDataCheckViewModel.Instance();
            Loaded += (s, e) =>
            {
                var textBox = this.FindName("SearchTextBox") as System.Windows.Controls.TextBox;
                if (textBox != null && string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = "请输入关键词搜索";
                    textBox.Foreground = System.Windows.Media.Brushes.Gray;
                }
            };
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var viewModel = DataContext as RevitDataCheckViewModel;
            if (e.NewValue is FamilyExtend familyExtend)
            {
                viewModel.SelectedItem = familyExtend;
            }
        }

        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = DataContext as RevitDataCheckViewModel;
            viewModel.DoubleClickAndPickObjects((sender as TreeView).SelectedValue);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var viewModel = DataContext as RevitDataCheckViewModel;
            if (viewModel.SearchKeyword != null&& viewModel.SearchKeyword != "请输入关键词搜索")
                viewModel.SearchCommand.Execute(null);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox.Text == "请输入关键词搜索")
            {
                textBox.Text = string.Empty;
                textBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = "请输入关键词搜索";
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }
    }
}
