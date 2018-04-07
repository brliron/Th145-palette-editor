using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Th145_palette_editor
{
    /// <summary>
    /// Commands et properties pour MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty isGameSelectedProperty = DependencyProperty.Register("isGameSelected", typeof(bool), typeof(MainWindow));
        public bool isGameSelected
        {
            get { return (bool)this.GetValue(isGameSelectedProperty); }
            set { this.SetValue(isGameSelectedProperty, value); }
        }
        public static readonly DependencyProperty isPaletteSelectedProperty = DependencyProperty.Register("isPaletteSelected", typeof(bool), typeof(MainWindow));
        bool isPaletteSelected
        {
            get { return (bool)this.GetValue(isPaletteSelectedProperty); }
            set { this.SetValue(isPaletteSelectedProperty, value); }
        }

        private void Generic_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = stack != null && stack.CanUndo;
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Tuple<ColorPicker, Color> t = stack.Undo();
            stack.ignoreAdd = true;
            t.Item1.SelectedColor = t.Item2;
            stack.ignoreAdd = false;
        }

        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = stack != null && stack.CanRedo;
        }

        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Tuple<ColorPicker, Color> t = stack.Redo();
            stack.ignoreAdd = true;
            t.Item1.SelectedColor = t.Item2;
            stack.ignoreAdd = false;
        }

        private void OpenTh145_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Load_tfpk("th145");
        }

        private void OpenTh155_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Load_tfpk("th155");
        }
    }

    public static class CustomCommands
    {
        public static readonly RoutedUICommand OpenTh145 = new RoutedUICommand("Open Touhou 1_4.5...", "OpenTh145", typeof(CustomCommands));
        public static readonly RoutedUICommand OpenTh155 = new RoutedUICommand("Open Touhou 1_5.5...", "OpenTh155", typeof(CustomCommands));
    }
}
