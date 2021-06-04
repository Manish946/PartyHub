using System;
using System.Collections.Generic;
using System.Windows;
using Newtonsoft.Json;
using System.Linq;
using Spotify;
using Spotify.Models;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Globalization;
using System.Data;
using System.Windows.Input;
using System.Net;
using System.Threading;
using System.Data.SqlClient;

namespace PartyHub.Content_Page
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    /// 
    public partial class Dashboard : Page
    {
        MainWindow main = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

        public Dashboard()
        {
            InitializeComponent();
        }

        private void Swipe_click(object sender, RoutedEventArgs e)
        {
            if (main != null)
            {
                main.Frame_Partyhub.NavigationService.Navigate(new Content_Page.Swipe());
            }
        }
        public static void Navigate(object target)
        {
            ((MainWindow)Application.Current.Windows[2]).Frame_Partyhub.Content = target;
        }

        private void GlobalListe_click(object sender, RoutedEventArgs e)
        {
            if (main != null)
            {
                main.Frame_Partyhub.NavigationService.Navigate(new Content_Page.GlobalListe());
            }
        }

        private void MinListe_click(object sender, RoutedEventArgs e)
        {
            if (main != null)
            {
                main.Frame_Partyhub.NavigationService.Navigate(new Content_Page.Minliste());
            }
        }
    }
}
