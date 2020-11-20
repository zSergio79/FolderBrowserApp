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
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FolderBrowserControl
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class FolderBrowser : UserControl
    {
        // Stub for collapsed tree item
        private readonly List<string> stub = new List<string>() { "Please wait..." };

        #region Dependency Properties
        public static readonly DependencyProperty FolderPathProperty =
            DependencyProperty.Register("FolderPath", typeof(string), typeof(FolderBrowser), new PropertyMetadata("", OnFolderChange), vvCallback);

        private static void OnFolderChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
                (d as FolderBrowser)?.SelectPath(e.NewValue.ToString());
        }

        private static bool vvCallback(object value)
        {
            return true;
        }

        public static readonly DependencyProperty IsBrowserOpenProperty =
            DependencyProperty.Register("IsBrowserOpen", typeof(bool), typeof(FolderBrowser), new PropertyMetadata(false));

        public string FolderPath
        {
            get { return (string)GetValue(FolderPathProperty); }
            set { SetValue(FolderPathProperty, value); }
        }

        public bool IsBrowserOpen
        {
            get { return (bool)GetValue(IsBrowserOpenProperty); }
            set { SetValue(IsBrowserOpenProperty, value); }
        }
        #endregion

        #region Command`s
        private RelayCommand okCommand;
        public RelayCommand OkCommand
        {
            get
            {
                if (okCommand == null)
                {
                    okCommand = new RelayCommand(o =>
                    {
                        FolderPath = text1.Text;
                        IsBrowserOpen = false;
                    });
                }
                return okCommand;
            }
        }
        private RelayCommand cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                if (cancelCommand == null)
                {
                    cancelCommand = new RelayCommand(o =>
                    {
                        IsBrowserOpen = false;
                    }
                    );
                }
                return cancelCommand;
            }
        }
        private RelayCommand newCommand;
        public RelayCommand NewCommand
        {
            get
            {
                if (newCommand == null)
                    newCommand = new RelayCommand(o => 
                    {
                        if (treeView1.SelectedItem != null)
                        {
                            TreeViewItem item = treeView1.SelectedItem as TreeViewItem;
                            string path = item.Tag.ToString();
                            DirectoryInfo di = new DirectoryInfo(path);
                            di.CreateSubdirectory("new folder");
                            if (item.IsExpanded == true)
                                item.ItemsSource = GetSubDir(path);
                            else
                                item.IsExpanded = true;
                            TreeViewItem newItem = (TreeViewItem)((item.ItemsSource as List<TreeViewItem>).Find(t => (string)t.Header == "new folder"));
                            newItem.IsSelected = true;
                        }
                    }
                    );
                return newCommand;
            }
        }
        #endregion

        #region Ctor
        public FolderBrowser()
        {
            InitializeComponent();
            
        }
        #endregion

        public void SelectPath(string path)
        {
            string[] els = FolderPath.Split('\\');
            var items = treeView1.ItemsSource;
            TreeViewItem item = null;
            if (items != null)
            {
                foreach (string s in els)
                {
                    var enumerator = items.GetEnumerator();
                    enumerator.Reset();
                    item = null;
                    while (enumerator.MoveNext() != false)
                    {
                        item = enumerator.Current as TreeViewItem;
                        if (item.Header.ToString().ToUpper() == s.ToUpper())
                        {
                            item.IsExpanded = true;
                            item.IsSelected = true;
                            items = item.ItemsSource;
                            break;
                        }
                    }
                    if (item != null)
                        continue;
                    return;
                }
            }
            
        }

        /// <summary>
        /// Create List of Logical Drives
        /// </summary>
        /// <returns> List Roots Tree Items of Logical Drives</returns>
        private List<TreeViewItem> GetTrees()
        {            
            List<TreeViewItem> result = new List<TreeViewItem>();
            foreach (string s in Directory.GetLogicalDrives())
            {
                TreeViewItem item = new TreeViewItem() { Header = s.TrimEnd('\\') ,Tag = s, ItemsSource = stub};
                item.Expanded += Item_Expanded;
                item.Collapsed += Item_Collapsed;
                result.Add(item);
            }
            return result;
        }

        private List<TreeViewItem> GetSubDir(string dir)
        {
            List<TreeViewItem> result = new List<TreeViewItem>();
            try
            {
                foreach (string s in Directory.GetDirectories(dir))
                {
                    TreeViewItem item = new TreeViewItem() { Header = System.IO.Path.GetFileName(s), Tag = s , ItemsSource=stub};
                    item.Expanded += Item_Expanded;
                    item.Collapsed += Item_Collapsed;
                    result.Add(item);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return result;
        }

        #region Expand/Collapse TreeItem Handler
        private void Item_Collapsed(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.ItemsSource = stub;
                e.Handled = true;
            }
        }

        private void Item_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {                
                item.ItemsSource = GetSubDir(item.Tag.ToString());
                item.BringIntoView();
                e.Handled = true;
            }          
        }
        #endregion

        private void TreeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem item = (TreeViewItem)e.NewValue;
            //if (item.IsVisible == false)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(item);
                while (parent != null && !(parent is ScrollViewer))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
                if (parent != null && (parent is ScrollViewer))
                {
                    ScrollViewer viewer = (ScrollViewer)parent;
                    Point offset = item.TransformToAncestor(viewer).Transform(new Point(0, 0));
                    Point pos = item.PointToScreen(new Point(0, 0));
                    Point posTree = treeView1.PointToScreen(new Point(0, 0));
                    Rect treeRect = new Rect(posTree, new Size(treeView1.ActualWidth, treeView1.ActualHeight));
                    if (treeRect.Bottom < pos.Y)
                        viewer.ScrollToVerticalOffset(offset.Y);
                }
            }
        }

        private void Popup1_Opened(object sender, EventArgs e)
        {
            treeView1.ItemsSource = GetTrees();
            if (string.IsNullOrEmpty(FolderPath.Trim()) == true || Directory.Exists(FolderPath) == false)
                if (treeView1.Items.Count > 0)
                    (treeView1.Items[0] as TreeViewItem).IsSelected = true;
            else
                SelectPath(FolderPath);
        }
    }
}
