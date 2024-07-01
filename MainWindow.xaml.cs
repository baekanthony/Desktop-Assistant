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
        public MainWindow()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
            var configuration = builder.Build();

            Uri endpoint = new Uri(configuration["ApiSettings:Endpoint"]);
            AzureKeyCredential credential = new AzureKeyCredential($"{configuration["ApiSettings:Key"]}");
            Console.WriteLine("endpoint: " + configuration["ApiSettings:Endpoint"]);
            Console.WriteLine($"{configuration["ApiSettings:Key"]}");

            ConversationAnalysisClient client = new ConversationAnalysisClient(endpoint, credential);
            TestConnection(client);
            InitializeComponent();
            synthesizer = new SpeechSynthesizer();
            synthesizer.SetOutputToDefaultAudioDevice();
            synthesizer.Speak("Synth working");
        }

        private void UserInputTxt(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) 
            {
                synthesizer.Speak(txtInput.Text);
                prevInput.Text += txtInput.Text + "\n";
                ProcessCommand(txtInput.Text);
                txtInput.Clear();
            }
        }

        //TODO: Connection working, start testing inputs and outputs
        private void TestConnection(ConversationAnalysisClient client)
        {
            string projectName = "Assistant";
            string deploymentName = "assistant-intent-v1";

            var data = new
            {
                AnalysisInput = new
                {
                    ConversationItem = new
                    {
                        Text = "open firefox",
                        Id = "1",
                        ParticipantId = "1",
                    }
                },
                Parameters = new
                {
                    ProjectName = projectName,
                    DeploymentName = deploymentName,

                    // Use Utf16CodeUnit for strings in .NET.
                    StringIndexType = "Utf16CodeUnit",
                },
                Kind = "Conversation",
            };

            Response response = client.AnalyzeConversation(RequestContent.Create(data));

            Console.WriteLine(response);
        }

        private String ProcessCommand(String text)
        {
            return "";
        }

        private void CommandSelection(String command, String target = "default")
        {
            switch(command)
            {
                case "open":
                    Console.WriteLine("open");
                    break;
                case "search":
                    Console.WriteLine("focus");
                    break;
                case "focus":
                    Console.WriteLine("swap");
                    break;
                default:
                    Console.WriteLine("invalid");
                    break;
            }
        }
    }
}
