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
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

using Azure.Core;
using Azure.Core.Serialization;
using Azure.AI.Language.Conversations;
using Azure;
using System.Runtime.InteropServices;

namespace Assist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SpeechSynthesizer synthesizer;
        private ConversationAnalysisClient client;
        public MainWindow()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
            var configuration = builder.Build();

            Uri endpoint = new Uri(configuration["ApiSettings:Endpoint"]);
            AzureKeyCredential credential = new AzureKeyCredential($"{configuration["ApiSettings:Key"]}");

            client = new ConversationAnalysisClient(endpoint, credential);
            InitializeComponent();
            synthesizer = new SpeechSynthesizer();
            synthesizer.SetOutputToDefaultAudioDevice();
            synthesizer.Speak("Synth working");
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnumProc, IntPtr.Zero);
            EnumWindows(EnumWindowsTest, IntPtr.Zero);
        }

        private void UserInputTxt(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) 
            {
                ProcessCommand(client, txtInput.Text);

                //TODO: just here temporarily to test synth working
                synthesizer.Speak(txtInput.Text);

                prevInput.Text += txtInput.Text + "\n";

                txtInput.Clear();
            }
        }

        private async void ProcessCommand(ConversationAnalysisClient client, string text)
        {
            string projectName = "Assistant";
            string deploymentName = "assistant-intent-v1";

            var data = new
            {
                analysisInput = new
                {
                    conversationItem = new
                    {
                        text,
                        id = "1",
                        participantId = "1",
                    }
                },
                parameters = new
                {
                    projectName,
                    deploymentName,

                    // Use Utf16CodeUnit for strings in .NET.
                    stringIndexType = "Utf16CodeUnit",
                },
                kind = "Conversation",
            };

            Console.WriteLine(RequestContent.Create(data));
            Response response = await client.AnalyzeConversationAsync(RequestContent.Create(data));

            dynamic conversationalTaskResult = response.Content.ToDynamicFromJson(JsonPropertyNames.CamelCase);
            dynamic conversationPrediction = conversationalTaskResult.Result.Prediction;

            //TODO: try thinking about cases with combined commands (ex. open firefox, then turn on focus mode)

            /*
            Console.WriteLine($"Top intent: {conversationPrediction.TopIntent}");

            Console.WriteLine("Entities:");
            foreach (dynamic entity in conversationPrediction.Entities)
            {
                Console.WriteLine($"Category: {entity.Category}");
                Console.WriteLine($"Text: {entity.Text}");
                Console.WriteLine($"Confidence: {entity.ConfidenceScore}");
                Console.WriteLine();

            }
            */

            //TODO: maybe check confidenceScores here before proceeding

            dynamic target = (conversationPrediction.Entities.Length < 1) ? null: conversationPrediction.Entities[0];
            CommandSelection(conversationPrediction.TopIntent, target);
        }

        private void CommandSelection(string command, dynamic target = null)
        {
            switch(command)
            {
                case "Open":
                    Console.WriteLine("Open");
                    OpenProgram(target.Text);
                    break;
                case "Focus":
                    Console.WriteLine("Focus");
                    break;
                case "Swap":
                    Console.WriteLine("Swap");
                    break;
                default:
                    Console.WriteLine("Invalid");
                    break;
            }
        }

        private void OpenProgram(string program)
        {
            string programsPath = Properties.Settings.Default.ProgramsLocation + "\\";

            //TODO: what if program isn't spelled properly? Or if shorthand is used (ex. chrome vs google chrome)
            //TODO: what if program name isn't capitalised?
            string  processedName = program.Substring(0, 1).ToUpper() + program.Substring(1);

            //TODO: what if program is in a folder?
            Console.WriteLine($"Checking: {programsPath}");
            try
            {
                Process.Start(programsPath + processedName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error");
                //TODO: need to differentiate between exceptions?
            }
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            Window settings = new SettingsWindow();
            //TODO: Disable actions in main window while settings window open
            settings.Show();
        }

        //SAMPLE CODE
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumDisplayMonitorsProc lpfnEnum, IntPtr dwData);
        public delegate bool EnumDisplayMonitorsProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        private static bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
        {
            Console.WriteLine($"Monitor Handle: {hMonitor}");
            Console.WriteLine($"Monitor Coordinates: Left = {lprcMonitor.Left}, Top = {lprcMonitor.Top}, Right = {lprcMonitor.Right}, Bottom = {lprcMonitor.Bottom}");
            Console.WriteLine();
            return true;
        }


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private static bool EnumWindowsTest(IntPtr hWnd, IntPtr lParam)
        {
            if (IsWindowVisible(hWnd))
            {
                StringBuilder windowTitle = new StringBuilder(256);
                GetWindowText(hWnd, windowTitle, windowTitle.Capacity);
                Console.WriteLine($"Window Handle: {hWnd}");
                Console.WriteLine($"Window Title: {windowTitle}");
                Console.WriteLine();
            }
            return true; // Continue enumeration
        }
    }


}
