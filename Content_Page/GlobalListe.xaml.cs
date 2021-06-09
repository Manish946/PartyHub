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

        //After receving token now we can Spotify API in our window Application.
        Dictionary<string, string> headers = new Dictionary<string, string>();
        // Setting Client and builder to call Spotify API.
        SpotifyWebClient client = new SpotifyWebClient();
        SpotifyWebBuilder builder = new SpotifyWebBuilder();
        //Add Track To user Profile if Liked. From Database
        private string connectionString = @"Server=tcp:partyhubserver.database.windows.net,1433;Initial Catalog=Partyhub_Database;Persist Security Info=False;User ID=partyhublogin;Password=Passw0rd;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        SqlConnection sqlAzureConnection;
        SqlDataReader dataReader;
        public string profileID = "";
        public string TrackID = "";

        public GlobalListe()
        {
            InitializeComponent();
            //Headers is being added and builder is ready to use.
            headers.Add("Authorization", "Bearer " + LoginWindow.SpotifyLogin.AccessToken);
            // Calling PrivateProfile Objects from Spotify.
            Tuple<ResponseInfo, string> Profile = client.Download(builder.GetPrivateProfile(), headers);
            //Profileobj is converted to json for easier uses. Profileobj Object is used to declare privateProfile API.
            var Profileobj = JsonConvert.DeserializeObject<PrivateProfile>(Profile.Item2);
            GlobalTop50.Visibility = Visibility.Hidden;
            PrintPartyHubGlobalList();



            //var image = Playlistobj2.Track.Popularit
            //GetImage(obj.Images[0].Url);
            // MessageBox.Show(image);


        }
        // Converts images link to bitmapFrame for WPF image uses.
        public ImageSource GetImage(string Link)
        {
            return BitmapFrame.Create(new Uri(Link));
        }

        // Image converter.
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
        // This Button will load top 50 tracks from Spotify playlist.
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // this will hide previous list and load new.
            PartyHubTopListe.Visibility = Visibility.Hidden;
            GlobalTop50.Visibility = Visibility.Visible;
            // Calling PrivateProfile Objects from Spotify.
            Tuple<ResponseInfo, string> Profile = client.Download(builder.GetPrivateProfile(), headers);
            var Profileobj = JsonConvert.DeserializeObject<PrivateProfile>(Profile.Item2);
            // Top50 Playlist Id is being used and converted to json. We are using tracks to show as datagrid in our window.
            string TopGlobalString = "37i9dQZEVXbMDoHDwVN2tF";
            Tuple<ResponseInfo, string> PlaylistGlobal50 = client.Download(builder.GetPlaylistTracks(Profileobj.Id, TopGlobalString), headers);
            var Playlistobj = JsonConvert.DeserializeObject<Paging<PlaylistTrack>>(PlaylistGlobal50.Item2);
            GlobalTop50.ItemsSource = null;
            GlobalTop50.ItemsSource = Playlistobj.Items;
        }
        // This Button will load top 50 tracks from PartyHub Global 50.

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // this will hide previous list and load new.
            PartyHubTopListe.Visibility = Visibility.Visible;
            GlobalTop50.Visibility = Visibility.Hidden;
            PrintPartyHubGlobalList();
        }
        private void PrintPartyHubGlobalList()
        {
            // Strings to help check if sql already exists.

            string CheckSql = "";
            List<Spotify.Models.FullTrack> LikedUserTracksList = new List<FullTrack>();
            // This will show likes from the databases.
            List<long> Likes = new List<long>();
            // New list is being created to store tracks from global list. Later likes and this list are merged as One list.
            List<GlobalListClass> lst = new List<GlobalListClass>();
            // New Connection is being open to load database data.
            sqlAzureConnection = new SqlConnection(connectionString);
            sqlAzureConnection.Open();
            CheckSql = $"SELECT TrackId,Likes FROM PartyhubGlobal order by  Likes  DESC";
            SqlCommand CommandCheck = new SqlCommand(CheckSql, sqlAzureConnection);
            dataReader = CommandCheck.ExecuteReader();
            if (dataReader.HasRows)
            {
                // If database has row upcoming while loop will be ran.
                int i = 0;
                while (i < 50) {

                    dataReader.Read();
                    //Following codes will add tracks from database global and add to a new list then later add likes list to make one merged list which will be displayed.
                     i++;
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
        // When datagrid is loaded our data will received automated Number id.
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
// This class is used to marge 2 classes togehter and displayed later.
public class GlobalListClass
{
    public List<Spotify.Models.FullTrack> Track { get; set; }
    public List<long> Likes { get; set; }

}

