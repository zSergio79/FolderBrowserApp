using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EditableTextBox
{
    public class PathTextBox : TextBox
    {
        public static RoutedEvent TextInputErrorEvent = EventManager.RegisterRoutedEvent("TextInputError", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(PathTextBox));
        public event RoutedEventHandler TextInputError
        {
            add
            {
                base.AddHandler(PathTextBox.TextInputErrorEvent, value);
            }
            remove
            {
                base.RemoveHandler(PathTextBox.TextInputErrorEvent, value);
            }
        }
        public PathTextBox():base()
        {
            base.PreviewTextInput += PathTextBox_PreviewTextInput;
        }

        private void PathTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (e.Text != "\r")
            {
                if (e.Text.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) != -1)
                {
                    e.Handled = true;
                    RoutedEventArgs args = new RoutedEventArgs(TextInputErrorEvent);
                    RaiseEvent(args);
                }
            }
        }
    }
}
