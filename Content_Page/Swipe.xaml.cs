using Microsoft.Win32;
using NAudio.Wave;
using Newtonsoft.Json;
using Spotify;
using Spotify.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
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
    /// Interaction logic for Swipe.xaml
    /// </summary>
    public partial class Swipe : Page
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        SpotifyWebClient client = new SpotifyWebClient();
        SpotifyWebBuilder builder = new SpotifyWebBuilder();
        private Point _positionInBlock;
        public string songPreview;
        WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();
        public double curpos = 0;
        MainWindow mainwin = new MainWindow();
        //Add Track To user Profile if Liked. From Database
        private string connectionString = @"Server=tcp:partyhubserver.database.windows.net,1433;Initial Catalog=Partyhub_Database;Persist Security Info=False;User ID=partyhublogin;Password=Passw0rd;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        SqlConnection sqlAzureConnection;
        SqlDataReader dataReader;
        SqlCommand Command;
        SqlDataAdapter adapter = new SqlDataAdapter();
        public string profileID = "";
        public string TrackID = "";
        public int CurrentTrackNum;
        bool FirstRandom = true;
        public Swipe()
        {
           
            this.InitializeComponent();
            ContentUsercontrol.RenderTransform = new TranslateTransform(0, 0);
            headers.Add("Authorization", "Bearer " + LoginWindow.SpotifyLogin.AccessToken);
            Tuple<ResponseInfo, string> Profile = client.Download(builder.GetPrivateProfile(), headers);
            var Profileobj = JsonConvert.DeserializeObject<PrivateProfile>(Profile.Item2);
            //Playing Current Track
            if (FirstRandom == true)
            {
                string CurrentTrack = RandomTrackFromPlaylist(Profileobj);
                Tuple<ResponseInfo, string> Track = client.Download(builder.GetTrack(CurrentTrack), headers);
                var Trackobj = JsonConvert.DeserializeObject<FullTrack>(Track.Item2);
                PrintTrackAsSwipe(Trackobj, Profileobj);

            }

            player.settings.volume = 70;
            

        }

        private string RandomTrackFromPlaylist(PrivateProfile Profileobj)
        {
            //Random Track
            Random random = new Random();

            Tuple<ResponseInfo, string> Search = client.Download(builder.SearchItems("TOP 50", Spotify.Enums.SearchType.Playlist, 22, 0, "US"), headers);
            var searchObj = JsonConvert.DeserializeObject<SearchItem>(Search.Item2);
            int Playlistnumber = random.Next(1, 20);
            if (Playlistnumber > searchObj.Playlists.Items.Count)
            {
                Playlistnumber = random.Next(1, 20);

            }
            var PlaylistId = searchObj.Playlists.Items[Playlistnumber].Id;
            if (searchObj.Playlists.Items[Playlistnumber].Tracks.Total > 50)
            {
                Playlistnumber = random.Next(1, 20);
                PlaylistId = searchObj.Playlists.Items[Playlistnumber].Id;
            }
            //Converting Playlist Id to tracks of playlist.
            Tuple<ResponseInfo, string> PlaylistTop50 = client.Download(builder.GetPlaylistTracks(Profileobj.Id, PlaylistId), headers);
            var Playlistobj = JsonConvert.DeserializeObject<Paging<PlaylistTrack>>(PlaylistTop50.Item2);
            var playlistTracks = searchObj.Playlists.Items[Playlistnumber].Tracks.Total;
            if (playlistTracks >= 50)
            {
                playlistTracks = random.Next(1, 49);
            }
            int number = random.Next(0, playlistTracks - 1);
            CurrentTrackNum = number;
            // MessageBox.Show(Playlistobj.Total.ToString()+" "+ searchObj.Playlists.Items[Playlistnumber].Name +" "+ searchObj.Playlists.Items[Playlistnumber].Tracks.Total+" trackn : "+ CurrentTrackNum);
            var CurrentTrack = Playlistobj.Items[CurrentTrackNum].Track.Id;
            return CurrentTrack;
        }

        private int ChangeTrack(int number)
        {

            return number;

        }
        private void PrintTrackAsSwipe(FullTrack Track, PrivateProfile Profile)
        {

            //New track
            TrackName.Text = Track.Name;
            if (Track.Name.Length >= 45)
            {
                TrackName.FontSize = 16;
            }
            AlbumImage.Source = GetImage(Track.Album.UrlImage);
            TrackArtist.Text = Track.Artists[0].Name;
            TrackNameSmall.Text = Track.Name;
            if (Track.Name.Length >= 50)
            {
                TrackNameSmall.FontSize = 16;
            }
            AlbumImageSmall.Source = GetImage(Track.Album.UrlImage);
            ArtistNameSmall.Text = Track.Artists[0].Name;
            songPreview = Track.PreviewUrl;
            if (songPreview == null)
            {
                NextSongTrackPreview();

            }
            profileID = Profile.Id;
            TrackID = Track.Id;

        }
        public void AddLikedTrackToDataBase(string trackId, string userID)
        {
            string sql = "";
            string CheckSql = "";
            sqlAzureConnection = new SqlConnection(connectionString);
            sqlAzureConnection.Open();
            CheckSql = $"select TrackID,Bruger_SpotifyID from ProfileLikedTrack WHERE TrackID  = '{trackId}' AND Bruger_SpotifyID = '{userID}'";
            SqlCommand CommandCheck = new SqlCommand(CheckSql, sqlAzureConnection);
            dataReader = CommandCheck.ExecuteReader();
            if (!dataReader.HasRows)
            {
                dataReader.Close();
                sql = $"insert into ProfileLikedTrack (TrackID,Bruger_SpotifyID) values('{trackId}','{userID}');";
                Command = new SqlCommand(sql, sqlAzureConnection);
                adapter.InsertCommand = new SqlCommand(sql, sqlAzureConnection);
                adapter.InsertCommand.ExecuteNonQuery();
                Command.Dispose();
                sqlAzureConnection.Close();
            }

        }
        private void AddTrackToGlobal(string trackId)
        {
            string sql = "";
            string CheckSql = "";
            sqlAzureConnection = new SqlConnection(connectionString);
            sqlAzureConnection.Open();
            CheckSql = $"SELECT TrackId,Likes from partyhubGlobal WHERE TrackId = '{trackId}'";
            SqlCommand CommandCheck = new SqlCommand(CheckSql, sqlAzureConnection);
            dataReader = CommandCheck.ExecuteReader();
            if (dataReader.HasRows)
            {
                dataReader.Read();
                long likes = (long)dataReader.GetValue(1) + 1;
                string updatedlike = likes.ToString();
                long testlike = Int64.Parse(updatedlike);
                sql = $"UPDATE PartyHubGlobal Set Likes = '{testlike}' where TrackId = '{trackId}'";

                Command = new SqlCommand(sql, sqlAzureConnection);
                adapter.InsertCommand = new SqlCommand(sql, sqlAzureConnection);
                dataReader.Close();
                adapter.InsertCommand.ExecuteNonQuery();
                Command.Dispose();
                sqlAzureConnection.Close();


            }
            else
            {
                dataReader.Close();
                sql = $"INSERT INTO partyhubGlobal (TrackId,Likes) values ('{trackId}',1);";
                Command = new SqlCommand(sql, sqlAzureConnection);
                adapter.InsertCommand = new SqlCommand(sql, sqlAzureConnection);
                adapter.InsertCommand.ExecuteNonQuery();
                Command.Dispose();
                sqlAzureConnection.Close();
            }
        }

        private void btnPlayPreview(object sender, RoutedEventArgs e)
        {
            if (player.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                curpos = player.controls.currentPosition;
                playmp3(songPreview, "Stop");
                PlayandPause.Source = new BitmapImage(new Uri(@"/Content\Play.png", UriKind.Relative));
                PlayandPause.ToolTip = "Play Preview";
            }
            else
            {
                playmp3(songPreview, "Play");
                PlayandPause.Source = new BitmapImage(new Uri(@"/Content\Pause.png", UriKind.Relative));
                PlayandPause.ToolTip = "Pause";



            }
        }
        private void playmp3(string path, string playState)
        {

                player.URL = path;


            player.settings.setMode("Loop", true);
            if (playState.Equals("Play"))
            {

                player.controls.currentPosition = curpos;
                player.controls.play();


            }
            else if (playState.Equals("Stop"))
            {
                player.controls.stop();

            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _positionInBlock = Mouse.GetPosition(ContentUsercontrol);
            ContentUsercontrol.CaptureMouse();
            ContentUsercontrol.Opacity = 0.7;
        }


        public ImageSource GetImage(string Link)
        {
            return BitmapFrame.Create(new Uri(Link));
        }
        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (ContentUsercontrol.IsMouseCaptured)
            {
                var container = VisualTreeHelper.GetParent(ContentUsercontrol) as UIElement;
                var mousePosition = e.GetPosition(container);
                ContentUsercontrol.RenderTransform = new TranslateTransform(mousePosition.X - _positionInBlock.X, 0);

                if (ContentUsercontrol.RenderTransform.Value.OffsetX <= (ContentGrid.ActualWidth * -1)+60)
                {
                    /*
                    liked.Visibility = Visibility.Hidden;
                    if (notliked.Visibility == Visibility.Hidden)
                    {
                        notliked.Visibility = Visibility.Visible;
                    }
                    */

                    FirstRandom = false;
                    ContentUsercontrol.RenderTransform = new TranslateTransform(mousePosition.X - _positionInBlock.X, 0);
                    playmp3(songPreview, "Stop");
                    PlayandPause.Source = new BitmapImage(new Uri(@"/Content\Play.png", UriKind.Relative));
                    
                    ContentUsercontrol.RenderTransform = new TranslateTransform(0, 0);
                    //ContentGrid.Children.Clear();
                    //Play next Song.
                    //Thread.Sleep(200);

                    MessageBox.Show("TRACK NOT LIKED!");



                    NextSongTrackPreview();

                    // Thread.Sleep(1000);

                }
                else if (ContentUsercontrol.RenderTransform.Value.OffsetX >= ContentGrid.ActualWidth-40)
                {
                    /*
                     
                      
                    notliked.Visibility = Visibility.Hidden;
                    if (liked.Visibility == Visibility.Hidden)
                    {
                        liked.Visibility = Visibility.Visible;
                    }                    
                    */
                    AddTrackToGlobal(TrackID);
                    FirstRandom = false;
                    ContentUsercontrol.RenderTransform = new TranslateTransform(mousePosition.X - _positionInBlock.X, 0);
                    playmp3(songPreview, "Stop");
                    PlayandPause.Source = new BitmapImage(new Uri(@"/Content\Play.png", UriKind.Relative));

                    ContentUsercontrol.RenderTransform = new TranslateTransform(0, 0);
                    AddLikedTrackToDataBase(TrackID, profileID);
                    //
                    // Stupid Error fixing with stupid Solution. Might delete Later. Cause of +1 becoming +2 while liking a track.
                    //AutoClosingMessageBox.Show("Track Liked!","",10);
                    ContentGrid.Focus();
                    ContentUsercontrol.Focus();
                    //Play next Song.
                    MessageBox.Show("TRACK LIKED!");
                   // Thread.Sleep(200);
                    NextSongTrackPreview();

                    // Thread.Sleep(1000);


                }


            }

            
        }
        public void NextSongTrackPreview()
        {
            Tuple<ResponseInfo, string> Profile = client.Download(builder.GetPrivateProfile(), headers);
            var Profileobj = JsonConvert.DeserializeObject<PrivateProfile>(Profile.Item2);
            string CurrentTrack = RandomTrackFromPlaylist(Profileobj);
            //AutoClosingMessageBox.Show("Track Liked!", "", 5);
            Tuple<ResponseInfo, string> Track = client.Download(builder.GetTrack(CurrentTrack), headers);
            var Trackobj = JsonConvert.DeserializeObject<FullTrack>(Track.Item2);
            //AutoClosingMessageBox.Show("Track Liked!", "", 10);
            
            
            PrintTrackAsSwipe(Trackobj, Profileobj);

        }

        public class AutoClosingMessageBox
        {
            System.Threading.Timer _timeoutTimer;
            string _caption;
            AutoClosingMessageBox(string text, string caption, int timeout)
            {
                _caption = caption;
                _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                    null, timeout, System.Threading.Timeout.Infinite);
                using (_timeoutTimer) { 
                  MessageBox.Show(text, caption);
                };
            }
            public static void Show(string text, string caption, int timeout)
            {
                new AutoClosingMessageBox(text, caption, timeout);
            }
            void OnTimerElapsed(object state)
            {
                IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
                if (mbWnd != IntPtr.Zero)
                    SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                _timeoutTimer.Dispose();
            }
            const int WM_CLOSE = 0x0010;
            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
            
        }
        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ContentUsercontrol.ReleaseMouseCapture();
            ContentUsercontrol.Opacity = 0.8;
            ContentUsercontrol.RenderTransform = new TranslateTransform(0, 0);
        }

        private void lostPageFocus(object sender, RoutedEventArgs e)
        {
            playmp3(songPreview, "Stop");
            this.KeepAlive = false;            
            PlayandPause.Source = new BitmapImage(new Uri(@"/Content\Play.png", UriKind.Relative));
        }
        /*
        private void OnSliderFocus(object sender, RoutedEventArgs e)
        {
            if (player.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {

            }
            else
            {
                playmp3(songPreview, "Play");
                PlayandPause.Source = new BitmapImage(new Uri(@"/Content\Pause.png", UriKind.Relative));
                PlayandPause.ToolTip = "Play";
            }

        }
        */
    }
}
