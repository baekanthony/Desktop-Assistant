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
using static Assist.MainWindow;

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
        }

        private void UserInputTxt(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessCommand(client, txtInput.Text);

                //TODO: just here temporarily to test synth working
                //synthesizer.Speak(txtInput.Text);

                prevInput.Text += txtInput.Text + "\n";

                txtInput.Clear();
            }
        }

        private async void ProcessCommand(ConversationAnalysisClient client, string text)
        {
            string projectName = "Assistant";
            string deploymentName = "assistant-intent-monitorNums";

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

            dynamic entities = conversationPrediction.Entities;
            CommandSelection(conversationPrediction.TopIntent, entities);
        }

        private void CommandSelection(string command, dynamic entities = null)
        {
            Console.WriteLine($"Yes {command} {entities}");
            switch (command)
            {
                case "Open":
                    Console.WriteLine("Open");
                    OpenProgram(entities);
                    break;
                case "Focus":
                    //TODO: have a timer to track deep work hours?
                    Console.WriteLine("Focus");
                    break;
                case "Swap":
                    HandleSwap(entities);
                    Console.WriteLine("Swap");
                    break;
                default:
                    Console.WriteLine("Invalid");
                    break;
            }
        }

        private void HandleSwap(dynamic entities = null)
        {
            int monitor1 = 1;
            int monitor2 = 2;

            if (entities != null && entities.Length == 2)
            {
                monitor1 = Int32.Parse(entities[0].text);
                monitor2 = Int32.Parse(entities[1].text);
            }
            HandleWindows windowsHandler = new HandleWindows();
            windowsHandler.SwapMonitors(monitor1, monitor2);
        }

        private void OpenProgram(dynamic entities)
        {
            if (entities != null && entities.Length == 1)
            {
                String program = entities[0].text;
                string programsPath = Properties.Settings.Default.ProgramsLocation + "\\";

                //TODO: what if program isn't spelled properly? Or if shorthand is used (ex. chrome vs google chrome)
                //TODO: what if program name isn't capitalised?
                string processedName = program.Substring(0, 1).ToUpper() + program.Substring(1);

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
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            Window settings = new SettingsWindow();
            //TODO: Disable actions in main window while settings window open
            settings.Show();
        }

    }
}
