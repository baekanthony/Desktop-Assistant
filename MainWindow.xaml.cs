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
                txtInput.Clear();
            }
        }
    }
}
