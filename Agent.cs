using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Assist
{
    internal class Agent
    {
        private DoubleAgent.AxControl.AxControl newAgent;

        public Agent() {
            newAgent = new DoubleAgent.AxControl.AxControl();

            newAgent.CreateControl();

            newAgent.Characters.Load("Bonzi", "PATH");
            //TODO: Remove
            newAgent.Characters["Bonzi"].Show();
        }

        public void showAssistant(object sender, RoutedEventArgs e)
        {

            //Need to get coords of window first
            newAgent.Characters["Bonzi"].MoveTo(5, 5);
            newAgent.Characters["Bonzi"].Show();
            //newAgent.Characters["Bonzi"].Speak("Hello there my friend");
        }
    }
}
