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
using Microsoft.Extensions.Configuration;

using Azure.Core;
using Azure.Core.Serialization;
using Azure.AI.Language.Conversations;
using Azure;

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
        }

        private void UserInputTxt(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) 
            {
                //TODO: just here temporarily to test synth working
                synthesizer.Speak(txtInput.Text);

                prevInput.Text += txtInput.Text + "\n";
                ProcessCommand(client, txtInput.Text);
                txtInput.Clear();
            }
        }

        private void ProcessCommand(ConversationAnalysisClient client, String text)
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
            Response response = client.AnalyzeConversation(RequestContent.Create(data));

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
            CommandSelection(conversationPrediction.TopIntent, conversationPrediction.Entities[0]);
        }

        private void CommandSelection(String command, dynamic target = null)
        {
            switch(command)
            {
                case "Open":
                    Console.WriteLine("Open");
                    Console.WriteLine($"{target.Text}");
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
    }
}
