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
        //After receving token now we can Spotify API in our window Application.
        Dictionary<string, string> headers = new Dictionary<string, string>();
        // Setting Client and builder to call Spotify API.
        SpotifyWebClient client = new SpotifyWebClient();
        SpotifyWebBuilder builder = new SpotifyWebBuilder();
        public Dashboard()
        {
            InitializeComponent();
            //Headers is being added and builder is ready to use.
            headers.Add("Authorization", "Bearer " + LoginWindow.SpotifyLogin.AccessToken);
            // Calling PrivateProfile Objects from Spotify.
            Tuple<ResponseInfo, string> tuple = client.Download(builder.GetPrivateProfile(), headers);
            var PrivateProfileObj = JsonConvert.DeserializeObject<PrivateProfile>(tuple.Item2);
            // Calling UserPlaylist Objects from Spotify.
            Tuple<ResponseInfo, string> UserPlaylist = client.Download(builder.GetUserPlaylists(PrivateProfileObj.Id), headers);
            var UserPlaylistObj = JsonConvert.DeserializeObject<Paging<SimplePlaylist>>(UserPlaylist.Item2);
            userDisplay(PrivateProfileObj, UserPlaylistObj);
        }
        // Swipe Click to naviate to Content page swipe.
        private void Swipe_click(object sender, RoutedEventArgs e)
        {
            if (main != null)
            {
                main.Frame_Partyhub.NavigationService.Navigate(new Content_Page.Swipe());
            }
        }
        // Naviate method to navigte around different frame of pages.
        public static void Navigate(object target)
        {
            ((MainWindow)Application.Current.Windows[2]).Frame_Partyhub.Content = target;
        }
        // Following methods to navigate to specific Pages.
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
        private void userDisplay(PrivateProfile User, Paging<SimplePlaylist> UserPlaylist)
        {
            // implementing Spotify api to set username and total user playlist.
            UserDisplayNavn.Text = User.DisplayName;
            ProfilePlaylist.Text = UserPlaylist.Total.ToString() + ProfilePlaylist.Text;
            Followers.Text = User.Followers.Total.ToString() + Followers.Text;

            //User Profile Picture
            if (User.Images != null && !User.Images.Any())
            {
                //Default Image already set!
            }
            else
            {
                userImage.ImageSource = GetImage(User.Images[0].Url);
            }

        }
        public ImageSource GetImage(string Link)
        {
            return BitmapFrame.Create(new Uri(Link));
        }

        private void demoClick(object sender, RoutedEventArgs e)
        {
            swipeDemo.Height = demobtn.Height;
            demobtn.Background = Brushes.Transparent;
            swipeDemo.Visibility = Visibility.Visible;
            stacktext.Visibility = Visibility.Collapsed;
        }
    }

}
