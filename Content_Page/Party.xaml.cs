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

namespace PartyHub.Content_Page
{
    /// <summary>
    /// Interaction logic for Party.xaml
    /// </summary>
    public partial class Party : Page
    {
        private CreatePartyWindow createPartySession = null;
        private JoinPartyWindow joinPartySession = null;
        public Party()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            if (createPartySession == null)
            {
                createPartySession = new CreatePartyWindow();
                createPartySession.Closed += SecondWindowClosed;
                createPartySession.Show();

            }


        }
        public void SecondWindowClosed(object sender, System.EventArgs e)
        {
            createPartySession = null;
            joinPartySession = null;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (joinPartySession == null)
            {
                joinPartySession = new JoinPartyWindow();
                joinPartySession.Closed += SecondWindowClosed;
                joinPartySession.Show();

            }
        }
    }
}
