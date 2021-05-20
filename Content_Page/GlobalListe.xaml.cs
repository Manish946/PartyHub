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
using System.Data.SqlClient;

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
        //Add Track To user Profile if Liked. From Database
        private string connectionString = @"Server=tcp:partyhubserver.database.windows.net,1433;Initial Catalog=Partyhub_Database;Persist Security Info=False;User ID=partyhublogin;Password=Passw0rd;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        SqlConnection sqlAzureConnection;
        SqlDataReader dataReader;
        SqlCommand Command;
        SqlDataAdapter adapter = new SqlDataAdapter();
        public string profileID = "";
        public string TrackID = "";
        private Brush BackgroundColor;

        public GlobalListe()
        {
            InitializeComponent();
            headers.Add("Authorization", "Bearer " + LoginWindow.SpotifyLogin.AccessToken);
            Tuple<ResponseInfo, string> Profile = client.Download(builder.GetPrivateProfile(), headers);
            var Profileobj = JsonConvert.DeserializeObject<PrivateProfile>(Profile.Item2);
            GlobalTop50.Visibility = Visibility.Hidden;
            PrintPartyHubGlobalList();



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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PartyHubTopListe.Visibility = Visibility.Hidden;
            GlobalTop50.Visibility = Visibility.Visible;

            Tuple<ResponseInfo, string> Profile = client.Download(builder.GetPrivateProfile(), headers);
            var Profileobj = JsonConvert.DeserializeObject<PrivateProfile>(Profile.Item2);
            string TopGlobalString = "37i9dQZEVXbMDoHDwVN2tF";
            Tuple<ResponseInfo, string> PlaylistGlobal50 = client.Download(builder.GetPlaylistTracks(Profileobj.Id, TopGlobalString), headers);
            var Playlistobj = JsonConvert.DeserializeObject<Paging<PlaylistTrack>>(PlaylistGlobal50.Item2);
            GlobalTop50.ItemsSource = null;
            GlobalTop50.ItemsSource = Playlistobj.Items;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PartyHubTopListe.Visibility = Visibility.Visible;
            GlobalTop50.Visibility = Visibility.Hidden;

            PrintPartyHubGlobalList();
        }
        private void PrintPartyHubGlobalList()
        {
            string sql = "";
            string CheckSql = "";
            string testdata = "";
            List<Spotify.Models.FullTrack> LikedUserTracksList = new List<FullTrack>();
            List<long> Likes = new List<long>();

            List<GlobalListClass> lst = new List<GlobalListClass>();
            sqlAzureConnection = new SqlConnection(connectionString);
            sqlAzureConnection.Open();
            CheckSql = $"SELECT TrackId,Likes FROM PartyhubGlobal order by  Likes  DESC";
            SqlCommand CommandCheck = new SqlCommand(CheckSql, sqlAzureConnection);
            dataReader = CommandCheck.ExecuteReader();
            if (dataReader.HasRows)
            {

                while (dataReader.Read())
                {
                    GlobalListClass PartyHubTopList = new GlobalListClass();
                    PartyHubTopList.Track = new List<FullTrack>();
                    PartyHubTopList.Likes = new List<long>();
                    var Track = dataReader.GetValue(0).ToString();
                    long TrackLikes = (long)dataReader.GetValue(1);
                    Tuple<ResponseInfo, string> TrackAPI = client.Download(builder.GetTrack(Track), headers);
                    var Trackobj = JsonConvert.DeserializeObject<FullTrack>(TrackAPI.Item2);
                    PartyHubTopList.Track.Add(Trackobj);
                    PartyHubTopList.Likes.Add(TrackLikes);
                    lst.Add(PartyHubTopList);

                    // PartyHubTopList.Track.Add(Trackobj);
                    //PartyHubTopList.Likes.Add(TrackLikes);

                };
                PartyHubTopListe.ItemsSource = lst;

                /*
                SqlDataAdapter data = new SqlDataAdapter(CheckSql, sqlAzureConnection) ;
                DataTable FullTrack = new DataTable();
                data.Fill(FullTrack);
                MinListeGrid.ItemsSource = FullTrack.DefaultView;
                */
                dataReader.Close();
                //


                sqlAzureConnection.Close();
            }
        }
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
            var dataGrid = (DataGrid)sender;
            var border = (Border)VisualTreeHelper.GetChild(dataGrid, 0);
            var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
            var grid = (Grid)VisualTreeHelper.GetChild(scrollViewer, 0);
            var button = (Button)VisualTreeHelper.GetChild(grid, 0);
            button.IsEnabled = false;
            button.Content = "#";
           // button.Background = Brushes.Black;
           button.Visibility = Visibility.Hidden;
        }

    }




}

public class GlobalListClass
{
    public List<Spotify.Models.FullTrack> Track { get; set; }
    public List<long> Likes { get; set; }

}

