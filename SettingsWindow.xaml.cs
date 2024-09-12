using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Assist
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void SetDirectory(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\Users";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Properties.Settings.Default.ProgramsLocation = dialog.FileName;
                Properties.Settings.Default.Save();
            }
        }
        
        private void FillComboBox(ComboBox comboBox, int numbering, int monitorsCount)
        {
            for (int i = 1; i <= monitorsCount; i++)
            {
                comboBox.Items.Add(i.ToString());
            }
            if (Properties.Settings.Default.MonitorNums != null && Properties.Settings.Default.MonitorNums.Length > 0)
            {
                comboBox.SelectedIndex = Properties.Settings.Default.MonitorNums[numbering - 1];
            } else
            {
                comboBox.SelectedIndex = numbering - 1;
            }
        }

        //TODO settings case when setup is changed?

        private bool ValidMonitorNums()
        {
            var monitorNumsSet = new HashSet<int>();
            foreach (Grid monitorGrid in monitorsCanvas.Children)
            {
                if (monitorGrid.Children[1] is ComboBox numberingComboBox)
                {
                    int monitorNum = numberingComboBox.SelectedIndex;
                    if (!monitorNumsSet.Add(monitorNum))
                    {
                        return false;
                    }
                }
            }

            int[] monitorNumsArray = new int[monitorNumsSet.Count];
            monitorNumsSet.CopyTo(monitorNumsArray);
            Properties.Settings.Default.TempMonitorNums = monitorNumsArray;
            Properties.Settings.Default.Save();
            return true;
        }

        private void MonitorNumChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Changed");
            applyBtn.Visibility = Visibility.Visible;
            applyBtn.IsEnabled = false;
            resetBtn.Visibility = Visibility.Visible;
            resetBtn.IsEnabled = true;
            if (ValidMonitorNums())
            {
                applyBtn.IsEnabled = true;
            }
        }

        private void DisplayMonitors(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.MonitorNums != null)
            {
                resetBtn.Visibility = Visibility.Visible;
            }

            double canvasHeight = monitorsCanvasBorder.Height;
            double canvasWidth = monitorsCanvasBorder.Width;

            HandleWindows windowsHandler = new HandleWindows();
            int numbering = 1;
            foreach (HandleWindows.RECT monitor in windowsHandler.monitors)
            {
                Console.WriteLine($"Displaying {monitor.Left}, {monitor.Top}, {monitor.Right}, { monitor.Bottom}");
                Grid grid = new Grid();
                Rectangle monitorRectangle = new Rectangle();
                //TODO: Need to scale properly
                double rectangleWidth = (monitor.Right - monitor.Left) / 12;
                double rectangleHeight = (monitor.Bottom - monitor.Top) / 12;
                Console.WriteLine($"Width: {rectangleWidth}, Height: {rectangleHeight}");

                monitorRectangle.Width = rectangleWidth;
                monitorRectangle.Height = rectangleHeight;
                monitorRectangle.Stroke = System.Windows.Media.Brushes.Gray;

                //Taskbar needs to be taken into account when setting coordinate (probably best to have a rect with rcMonitor instead of rcWork) 
                Canvas.SetLeft(grid, (canvasWidth / 2 - rectangleWidth + (monitor.Left / 12)));
                Canvas.SetTop(grid, (canvasHeight / 2 - rectangleHeight + (monitor.Top /12)));

                ComboBox numberingComboBox = new ComboBox();
                numberingComboBox.Name = "numberingComboBox";
                FillComboBox(numberingComboBox, numbering, windowsHandler.monitors.Count);
                numberingComboBox.SelectionChanged += new SelectionChangedEventHandler(MonitorNumChanged);

                grid.Children.Add(monitorRectangle);
                grid.Children.Add(numberingComboBox);

                monitorsCanvas.Children.Add(grid);
                numbering++;

            }
        }

        private void ApplyMonitorNums(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.MonitorNums = Properties.Settings.Default.TempMonitorNums;
            Properties.Settings.Default.Save();
            applyBtn.IsEnabled = false;
        }

        private void ResetMonitorNums(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.MonitorNums = null;
            Properties.Settings.Default.Save();
            DisplayMonitors(sender, e);
            resetBtn.IsEnabled = false;
        }

        private void showAssistant(object sender, RoutedEventArgs e)
        {

        }
    }
}
