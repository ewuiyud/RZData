using RZData.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RZData.UserControls
{
    public partial class MultiSelectTreeView : UserControl
    {
        // 定义依赖属性，用于绑定数据源
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<object>), typeof(MultiSelectTreeView),
                new PropertyMetadata(null/*, OnItemsSourceChanged*/));

        // 定义TreeView双击事件 - 依赖事件
        public static readonly RoutedEvent TreeViewDoubleClickEvent =
            EventManager.RegisterRoutedEvent(
                "TreeViewDoubleClick",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(MultiSelectTreeView));

        // CLR事件包装器
        public event RoutedEventHandler TreeViewDoubleClick
        {
            add { AddHandler(TreeViewDoubleClickEvent, value); }
            remove { RemoveHandler(TreeViewDoubleClickEvent, value); }
        }

        public MultiSelectTreeView()
        {
            InitializeComponent();
        }

        // 处理TreeView的鼠标双击事件
        private void MultiSelectTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 获取双击的节点
            var hitTestResult = VisualTreeHelper.HitTest(multiSelectTreeView, e.GetPosition(multiSelectTreeView));
            if (hitTestResult != null && hitTestResult.VisualHit != null)
            {
                var treeViewItem = FindAncestor<TreeViewItem>(hitTestResult.VisualHit);
                if (treeViewItem != null)
                {
                    // 创建双击事件参数，包含选中的节点数据
                    var doubleClickEventArgs = new RoutedEventArgs(TreeViewDoubleClickEvent);
                    doubleClickEventArgs.Source = treeViewItem.DataContext;

                    // 触发自定义的TreeViewDoubleClick事件
                    RaiseEvent(doubleClickEventArgs);
                }
            }
        }

        // 数据源属性
        public IEnumerable<object> ItemsSource
        {
            get { return (IEnumerable<object>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // 复选框点击事件处理
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)checkBox);

            if (treeViewItem != null && treeViewItem.DataContext != null)
            {
                var isChecked = checkBox.IsChecked ?? false;

                // 更新子节点状态
                UpdateChildrenNodeSelection(treeViewItem, isChecked);

                // 更新父节点状态
                UpdateParentNodeSelection(treeViewItem);
            }
        }

        // 树节点选中事件处理
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue != null)
            {
                // 当通过鼠标点击文本区域选中节点时，同步复选框状态
                if (multiSelectTreeView.ItemContainerGenerator.ContainerFromItem(e.NewValue) is TreeViewItem treeViewItem)
                {
                    var checkBox = FindChild<CheckBox>(treeViewItem);
                    if (checkBox != null && !(checkBox.IsChecked ?? false))
                    {
                        checkBox.IsChecked = true;
                    }
                }
            }
        }

        // 更新子节点选中状态
        private void UpdateChildrenNodeSelection(TreeViewItem treeViewItem, bool isChecked)
        {
            // 获取当前节点的数据上下文
            var item = treeViewItem.DataContext;

            // 获取子项集合
            var childItems = GetChildItems(item);
            if (childItems == null) return;

            // 遍历子项并更新选中状态
            foreach (dynamic childItem in childItems)
            {
                childItem.IsChecked = isChecked;
                // 查找子项对应的TreeViewItem容器
                var childContainer = GetTreeViewItemContainer(treeViewItem, childItem);
                if (childContainer != null)
                {
                    // 递归更新子节点的子节点
                    UpdateChildrenNodeSelection(childContainer, isChecked);
                }
            }
        }

        // 查找特定数据项对应的TreeViewItem容器
        private TreeViewItem GetTreeViewItemContainer(ItemsControl parentContainer, object item)
        {
            if (parentContainer == null) return null;

            // 尝试直接从ItemContainerGenerator获取容器
            if (parentContainer.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem container) return container;

            // 如果容器尚未生成，则强制生成
            parentContainer.ApplyTemplate();
            var itemsPresenter = (ItemsPresenter)parentContainer.Template.FindName("ItemsHost", parentContainer);
            if (itemsPresenter != null)
            {
                itemsPresenter.ApplyTemplate();
            }
            else
            {
                parentContainer.UpdateLayout();
            }

            // 再次尝试获取容器
            container = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
            if (container != null) return container;

            // 如果仍然找不到，递归搜索子项
            for (int i = 0; i < parentContainer.Items.Count; i++)
            {
                if (!(parentContainer.ItemContainerGenerator.ContainerFromIndex(i) is TreeViewItem subContainer)) continue;

                var found = GetTreeViewItemContainer(subContainer, item);
                if (found != null) return found;
            }

            return null;
        }

        // 更新父节点选中状态
        private void UpdateParentNodeSelection(TreeViewItem treeViewItem)
        {
            var parentItem = FindAncestor<TreeViewItem>(treeViewItem);
            if (parentItem != null && parentItem.DataContext != null)
            {
                var childItems = GetAllLeafNodes(parentItem.DataContext);
                if (childItems != null)
                {
                    var allSelected = childItems.All( item  => (item as ElementInstanceViewModel).IsChecked);
                    var noneSelected = childItems.All(item => !(item as ElementInstanceViewModel).IsChecked);

                    var checkBox = FindChild<CheckBox>(parentItem);
                    if (checkBox != null)
                    {
                        if (allSelected)
                        {
                            checkBox.IsChecked = true;
                        }
                        else if (noneSelected)
                        {
                            checkBox.IsChecked = false;
                        }
                        else
                        {
                            checkBox.IsChecked = null; // 半选状态
                        }
                    }

                    // 递归更新父节点
                    UpdateParentNodeSelection(parentItem);
                }
            }
        }

        // 检查节点是否有子节点
        private bool HasChildren(object item)
        {
            var childItems = GetChildItems(item);
            return childItems != null && childItems.Any();
        }

        // 获取节点的子项
        private IEnumerable<object> GetChildItems(object item)
        {
            var type = item.GetType();
            var property = type.GetProperty("Children");
            if (property != null)
            {
                return property.GetValue(item) as IEnumerable<object>;
            }
            return null;
        }

        // 获取节点的所有叶节点（即没有子节点的节点）
        private IEnumerable<object> GetAllLeafNodes(object item)
        {
            var type = item.GetType();
            var property = type.GetProperty("Children");

            // 如果没有Children属性，视为叶节点
            if (property == null)
            {
                yield return item;
                yield break;
            }

            // 如果没有子节点，视为叶节点
            if (!(property.GetValue(item) is IEnumerable<object> children) || !children.Any())
            {
                yield return item;
                yield break;
            }

            // 递归查找子节点中的叶节点
            foreach (var child in children)
            {
                foreach (var leaf in GetAllLeafNodes(child))
                {
                    yield return leaf;
                }
            }
        }

        // 查找父级元素
        private T FindAncestor<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(dependencyObject);
            while (parent != null)
            {
                if (parent is T correctlyTyped)
                {
                    return correctlyTyped;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        // 查找子元素
        private T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            var childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                {
                    return typedChild;
                }

                var recurse = FindChild<T>(child);
                if (recurse != null)
                {
                    return recurse;
                }
            }
            return null;
        }

        // 加载时展开
        private void TreeViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                if (!(item.DataContext is FamilyExtendViewModel))
                {
                    item.IsExpanded = true;
                }
            }
        }
    }
}