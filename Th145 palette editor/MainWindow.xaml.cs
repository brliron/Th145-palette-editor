﻿using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace Th145_palette_editor
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TFPK tfpk;
        List<Character> characters;
        Character curChar;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            curChar.selectedBitmap.setPalette(curChar.selectedPalette);
            view.Source = CToBitmapSource.ToBitmapSource(curChar.selectedBitmap.bmp);
        }

        private void Load_tfpk(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog(this) != CommonFileDialogResult.Ok)
                return;

            TFPK tfpk = new TFPK(dialog.FileName, "th145.pak");
            if (tfpk.Exists == false)
            {
                MessageBox.Show("File " + tfpk.pak_path + "not found");
                return;
            }
            if (tfpk.IsExtracted && tfpk.ContainsDirectory(@"data\actor") == false)
            {
                MessageBox.Show("The directory " + tfpk.extracted_path + " exists and doesn't contains a dump of " + tfpk.pak_path + ".\n" +
                    "Remove, rename or move this directory and try again.");
                return;
            }

            if (tfpk.IsExtracted == false)
                tfpk.Extract();
            if (tfpk.ContainsDirectory(@"data\actor") == false)
            {
                MessageBox.Show("The directory " + tfpk.extracted_path + @"\data\actor haven't been created by th145arc.");
                return;
            }

            this.tfpk = tfpk;
            characters = new List<Character>();
            foreach (string dir in Directory.EnumerateDirectories(tfpk.extracted_path + @"\data\actor\"))
                if (File.Exists(dir + "\\palette000.bmp"))
                    characters.Add(new Character(dir.Substring(dir.LastIndexOf('\\') + 1), dir));

            CharsList.ItemsSource = characters;
            CharsList.Visibility = Visibility.Visible;
        }

        bool charChanging = false;

        private void LoadCharacter(object sender, SelectionChangedEventArgs e)
        {
            charChanging = true;

            curChar = e.AddedItems[0] as Character;
            curChar.load();

            BmpList.ItemsSource = curChar.bmpNames;
            PltList.ItemsSource = curChar.pltNames;

            BmpList.SelectedIndex = curChar.bmpNames.FindIndex(x => x == curChar.getDefaultBmp());
            PltList.SelectedIndex = 0;
            BmpListContainer.Visibility = Visibility.Visible;
            PltList.Visibility = Visibility.Visible;
            SavePanel.Visibility = Visibility.Visible;

            charChanging = false;
            curChar.selectBitmap(BmpList.SelectedItem as string);
            curChar.selectPalette(PltList.SelectedItem as string);
            view.Source = CToBitmapSource.ToBitmapSource(curChar.selectedBitmap.bmp);
            colors.ItemsSource = curChar.selectedPalette.list;
        }

        private void LoadBmp(object sender, SelectionChangedEventArgs e)
        {
            if (charChanging || e.AddedItems.Count == 0)
                return;
            curChar.selectBitmap(e.AddedItems[0] as string);
            view.Source = CToBitmapSource.ToBitmapSource(curChar.selectedBitmap.bmp);
        }

        private void LoadPlt(object sender, SelectionChangedEventArgs e)
        {
            if (charChanging || e.AddedItems.Count == 0)
                return;
            if (curChar.selectPalette(e.AddedItems[0] as string))
            {
                int oldSavePltIndex = PatchPaletteTarget.SelectedIndex;
                PltList.ItemsSource = null;
                PltList.ItemsSource = curChar.pltNames;
                PltList.SelectedIndex = curChar.pltNames.Count - 2;
                PatchPaletteTarget.SelectedIndex = oldSavePltIndex;
            }
            view.Source = CToBitmapSource.ToBitmapSource(curChar.selectedBitmap.bmp);
            colors.ItemsSource = curChar.selectedPalette.list;
        }

        private void ChangeSaveTarget(object sender, SelectionChangedEventArgs e)
        {
            if (StaticPatchTarget == null)
                return;
            switch ((e.AddedItems[0] as ComboBoxItem).Content as string)
            {
                case "Palette file":
                    StaticPatchTarget.Visibility = Visibility.Collapsed;
                    PatchCharacterTarget.Visibility = Visibility.Collapsed;
                    PatchPaletteTarget.Visibility = Visibility.Collapsed;
                    break;
                case "Static patch":
                    StaticPatchTarget.Visibility = Visibility.Visible;
                    PatchCharacterTarget.Visibility = Visibility.Visible;
                    PatchPaletteTarget.Visibility = Visibility.Visible;
                    break;
                case "Thcrap patch":
                    StaticPatchTarget.Visibility = Visibility.Collapsed;
                    PatchCharacterTarget.Visibility = Visibility.Visible;
                    PatchPaletteTarget.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void Prev_bmp(object sender, RoutedEventArgs e)
        {
            if (BmpList.SelectedIndex != 0)
                BmpList.SelectedIndex--;
            else
                BmpList.SelectedIndex = curChar.bmpNames.Count - 1;
        }

        private void Next_bmp(object sender, RoutedEventArgs e)
        {
            if (BmpList.SelectedIndex != curChar.bmpNames.Count - 1)
                BmpList.SelectedIndex++;
            else
                BmpList.SelectedIndex = 0;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            string SaveTarget           = (this.SaveTarget.SelectedItem             as ComboBoxItem).Content as string;
            string StaticPatchTarget    = (this.StaticPatchTarget.SelectedItem      as ComboBoxItem).Content as string;
            string PatchCharacterTarget = (this.PatchCharacterTarget.SelectedItem   as Character).name;
            string PatchPaletteTarget   = (this.PatchPaletteTarget.SelectedItem     as ComboBoxItem).Content as string;

            string filename = null;
            string out_dir;

            switch (SaveTarget)
            {
                case "Palette file":
                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.Filter = "Palette file (*.bmp)|*.bmp|All files (*.*)|*.*";
                    if (dlg.ShowDialog(this) != true)
                        return;
                    filename = dlg.FileName;
                    break;
                case "Static patch":
                    if (File.Exists(this.tfpk.game_path + "\\th145e.exe") == false)
                    {
                        if (MessageBox.Show("Do you want to install the static patch files?", "Static patch installation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            File.Copy(".\\th145e.exe", this.tfpk.game_path + "\\th145e.exe");
                            File.Copy(".\\th145e.dll", this.tfpk.game_path + "\\th145e.dll", true);
                            MessageBox.Show("Static patch files installed. To use your custom palettes, run th145e.exe instead of th145.exe.");
                        }
                        else
                            return;
                    }

                    TFPK tfpk = new TFPK(this.tfpk.game_path, StaticPatchTarget);
                    if (tfpk.Exists && tfpk.IsExtracted == false)
                        tfpk.Extract();
                    out_dir = tfpk.extracted_path + @"\data\actor\" + PatchCharacterTarget + '\\';
                    Directory.CreateDirectory(out_dir);
                    filename = out_dir + PatchPaletteTarget;
                    break;
                case "Thcrap patch":
                    CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                    dialog.IsFolderPicker = true;
                    if (dialog.ShowDialog(this) != CommonFileDialogResult.Ok)
                        return;
                    out_dir = dialog.FileName + @"\th145\data\actor\" + PatchCharacterTarget + '\\';
                    Directory.CreateDirectory(out_dir);
                    filename = out_dir + PatchPaletteTarget;
                    break;
            }

            curChar.selectedPalette.save(filename);

            if (SaveTarget == "Static patch")
                new TFPK(this.tfpk.game_path, StaticPatchTarget).Repack();

            MessageBox.Show("Palette saved!");
        }
    }
}
