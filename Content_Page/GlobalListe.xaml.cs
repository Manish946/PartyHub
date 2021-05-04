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

namespace PartyHub.Content_Page
{
    /// <summary>
    /// Interaction logic for GlobalListe.xaml
    /// </summary>
    public partial class GlobalListe : Page
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        SpotifyWebClient client = new SpotifyWebClient();
        SpotifyWebBuilder builder = new SpotifyWebBuilder();
        public GlobalListe()
        {
            InitializeComponent();
            headers.Add("Authorization", "Bearer " + LoginWindow.SpotifyLogin.AccessToken);
            Tuple<ResponseInfo, string> Profile = client.Download(builder.GetPrivateProfile(), headers);
            var Profileobj = JsonConvert.DeserializeObject<PrivateProfile>(Profile.Item2);

            string TopGlobalString = "37i9dQZEVXbMDoHDwVN2tF";
            Tuple<ResponseInfo, string> Playlist = client.Download(builder.GetPlaylistTracks(Profileobj.Id, TopGlobalString), headers);
            var Playlistobj = JsonConvert.DeserializeObject<Paging<PlaylistTrack>>(Playlist.Item2);
            var Playlistobj2 = JsonConvert.DeserializeObject<PlaylistTrack>(Playlist.Item2);
            GlobalTop50.ItemsSource = Playlistobj.Items;
            //var image = Playlistobj2.Track.Popularit
            //GetImage(obj.Images[0].Url);
            // MessageBox.Show(image);

        }
        public ImageSource GetImage(string Link)
        {
            return BitmapFrame.Create(new Uri(Link));
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
}
