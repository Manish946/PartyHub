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

namespace PartyHub
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        SpotifyWebClient client = new SpotifyWebClient();
        SpotifyWebBuilder builder = new SpotifyWebBuilder();
        public MainWindow()
        {
            InitializeComponent();
            Frame_Partyhub.Content = new Content_Page.Dashboard();
            //Displayname
            headers.Add("Authorization", "Bearer " + LoginWindow.SpotifyLogin.AccessToken);
            Tuple<ResponseInfo, string> tuple = client.Download(builder.GetPrivateProfile(), headers);
            var obj = JsonConvert.DeserializeObject<PrivateProfile>(tuple.Item2);
            UserName.Content = obj.DisplayName.Substring(0, 1).ToUpper()+obj.DisplayName.Substring(1, obj.DisplayName.Length-1);
            fullname.Content = obj.DisplayName.Substring(0, 1).ToUpper() + obj.DisplayName.Substring(1, obj.DisplayName.Length - 1);
            LoginWindow.SpotifyLogin.IdCurrentUser = obj.Id;
            LoginWindow.SpotifyLogin.CurrentLocation = obj.Country;
            if (obj.Images != null && !obj.Images.Any())
            {
                     
            }
            else
            {
               
                userImage.ImageSource = GetImage(obj.Images[0].Url);

            }

            //userImage.Source = GetImage(obj.Images[0].Url.ToString());

        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            Frame_Partyhub.Content = new Content_Page.Dashboard();
        }

        public ImageSource GetImage(string Link)
        {
            return BitmapFrame.Create(new Uri(Link));
        }

        private void OnAfslutClick(object sender, RoutedEventArgs e)
        {
            this.Close();
            Environment.Exit(Environment.ExitCode);
        }

        private void logud_click(object sender, RoutedEventArgs e) 
        {
           System.Diagnostics.Process.Start("https://accounts.spotify.com/en/logout");
            LoginWindow loginWin = new LoginWindow();
           // loginWin.stopServer();
            loginWin.Show();
            Close();
        }
    }
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class ImageConverter : IValueConverter
    {
        public object Convert(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new BitmapImage(new Uri(value.ToString()));
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }


    }

}
