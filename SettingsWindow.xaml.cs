using System;
using System.Threading;
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

        private void DisplayMonitors(object sender, RoutedEventArgs e)
        {
            double canvasHeight = monitorsCanvasBorder.Height;
            double canvasWidth = monitorsCanvasBorder.Width;

            /*
            Rectangle test = new Rectangle();
            test.Width = 100;
            test.Height = 50;
            test.Fill = System.Windows.Media.Brushes.Red;
            Canvas.SetLeft(test, 150);
            Canvas.SetTop(test, (canvasHeight / 2 - (50 / canvasHeight / 3)));
            monitorsCanvas.Children.Add(test);
            */

            HandleWindows windowsHandler = new HandleWindows();
            foreach (HandleWindows.RECT monitor in windowsHandler.monitors)
            {
                Console.WriteLine($"Displaying {monitor.Left}, {monitor.Top}, {monitor.Right}, { monitor.Bottom}");
                Rectangle monitorRectangle = new Rectangle();
                //TODO: Need to scale properly
                double rectangleWidth = (monitor.Right - monitor.Left) / 6;
                double rectangleHeight = (monitor.Bottom - monitor.Top) / 6;
                Console.WriteLine($"Width: {rectangleWidth}, Height: {rectangleHeight}");

                monitorRectangle.Width = rectangleWidth;
                monitorRectangle.Height = rectangleHeight;
                monitorRectangle.Fill = System.Windows.Media.Brushes.Gray;

                Canvas.SetLeft(monitorRectangle, (canvasWidth / 2 - rectangleWidth + (monitor.Left / 5)));
                Canvas.SetTop(monitorRectangle, (canvasHeight / 2 - rectangleHeight + (monitor.Top /5)));

                monitorsCanvas.Children.Add(monitorRectangle);
            }
        }
    }
}
