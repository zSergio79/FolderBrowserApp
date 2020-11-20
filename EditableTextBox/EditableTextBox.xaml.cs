using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

namespace EditableTextBox
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class EditableText : UserControl
    {

        

        #region Private Field`s
        private int timestamp = 0;
        private string oldValue = string.Empty;
        private bool isCancel = false;
        #endregion

        #region Public Field`s

        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty IsEditProperty =
            DependencyProperty.Register("IsEdit", typeof(bool), typeof(EditableText), new PropertyMetadata(false, IsEditChange));

        private static void IsEditChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
        }

        public bool IsEdit
        {
            get {return (bool)GetValue(IsEditProperty); }
            set
            {
                SetValue(IsEditProperty, value);
            }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(EditableText), new PropertyMetadata("some text"));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }

            set
            {
                SetValue(TextProperty, value);
            }
        }
        #endregion

        #region .ctor
        public EditableText()
        {
            InitializeComponent();
        }
        #endregion

        #region Private method
        private bool isValid()
        {
          
            return !isCancel;
        }
        #endregion

        #region Events Processing
        private void EditableBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if((bool)e.NewValue == true)
            {
                TextBox txt = sender as TextBox;
                if (txt != null)
                {
                    txt.Focus();
                    txt.SelectionStart = 0;
                    txt.SelectionLength = txt.Text.Length;
                    oldValue = Text;
                }
            }
            else
            {
                if (isValid() != true)
                    Text = oldValue;
            }
        }

        private void TextBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            isCancel = false;
            IsEdit = false;
        }

        private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {           
            switch(e.Key)
            {
                case Key.Escape: isCancel = true; ;break;
                case Key.Enter: isCancel = false;break;
                default:return;
            }
            IsEdit = false;
        }               

        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            timestamp = e.Timestamp - timestamp;
            if (timestamp > 500)
                IsEdit = true;
            timestamp = 0;
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            timestamp = e.Timestamp;
        }
        #endregion

        private void TxtEdit_ToolTipClosing(object sender, ToolTipEventArgs e)
        {

        }

        private void TxtEdit_ToolTipOpening(object sender, ToolTipEventArgs e)
        {

        }
    }
}
