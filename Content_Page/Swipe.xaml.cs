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
        //After receving token now we can Spotify API in our window Application.
        Dictionary<string, string> headers = new Dictionary<string, string>();
        // Setting Client and builder to call Spotify API.
        SpotifyWebClient client = new SpotifyWebClient();
        SpotifyWebBuilder builder = new SpotifyWebBuilder();
        // Mouse position
        private Point _positionInBlock;
        public bool UserplaylistBool = false;
        // Current spotify applied songpreview uri. It will be used to play songs in Swipe Cardboard layout.
        public string songPreview;
        // New player to play Current Tracks.
        WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();
        public double curpos = 0;
        MainWindow mainwin = new MainWindow();
        //Add Track To user Profile if Liked. From Database
        private string connectionString = @"Server=tcp:partyhubserver.database.windows.net,1433;Initial Catalog=Partyhub-Database;Persist Security Info=False;User ID=partyhub946;Password=PartyHub12;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        SqlConnection sqlAzureConnection;
        SqlDataReader dataReader;
        SqlCommand Command;
        SqlDataAdapter adapter = new SqlDataAdapter();
        // User current iD and current tracknumber.
        public string profileID = "";
        public string TrackID = "";
        public FullTrack CurrentTrackDB;
        public int CurrentTrackNum;
        bool FirstRandom = true;
        public Swipe()
        {

            this.InitializeComponent();
            ContentUsercontrol.RenderTransform = new TranslateTransform(0, 0);
            //Headers is being added and builder is ready to use.
            headers.Add("Authorization", "Bearer " + LoginWindow.SpotifyLogin.AccessToken);
            // Calling PrivateProfile Objects from Spotify.
            Tuple<ResponseInfo, string> Profile = client.Download(builder.GetPrivateProfile(), headers);
            var Profileobj = JsonConvert.DeserializeObject<PrivateProfile>(Profile.Item2);
            //Playing Current Track
            if (FirstRandom == true)
            {
                // Callings methods to load Swipe function only once, so it does not override while loading next song.
                string CurrentTrack = RandomTrackFromPlaylist(Profileobj);
                Tuple<ResponseInfo, string> Track = client.Download(builder.GetTrack(CurrentTrack), headers);
                var Trackobj = JsonConvert.DeserializeObject<FullTrack>(Track.Item2);
                PrintTrackAsSwipe(Trackobj, Profileobj);
                PlayPreview.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                PlayPreview.Focus();
                GlobalPlaylistText.Text = "Playing From Global Top Playlists";
                UserPlaylist.Opacity = 0.6;
                partyhubPlaylist.Opacity = 1;
            }
            // Volume of media to 70.
            player.settings.volume = 70;
            notliked.Visibility = Visibility.Hidden;
            liked.Visibility = Visibility.Hidden;

        }

        private string RandomTrackFromPlaylist(PrivateProfile Profileobj)
        {
            //Random Track
            Random random = new Random();
            var CurrentTrack = "";
            // Search Keyword "TOP 50" and type Playlist for random tracks after Swipe is done.
            if (UserplaylistBool == true)
            {
                Tuple<ResponseInfo, string> ProfilePlaylist = client.Download(builder.GetUserPlaylists(Profileobj.Id), headers);
                var ProfilePlaylistObj = JsonConvert.DeserializeObject<Paging<SimplePlaylist>>(ProfilePlaylist.Item2);
                int Playlistnumber2 = random.Next(1, ProfilePlaylistObj.Items.Count - 1);
                if (ProfilePlaylistObj.Items.Count <= 3)
                {
                    MessageBox.Show("NOT ENOUGH PLAYLISTS. ATLEAST MORE THAN 3 PLAYLISTS IS REQUIRED!");

                    CurrentTrack = "0zzVTGyRrWpQu8Fr28NRAv";
                    UserplaylistBool = false;
                    UserPlaylistText.Text = "User Playlists";
                    GlobalPlaylistText.Text = "Playing From Global Top Playlists";
                    UserPlaylist.Opacity = 0.6;
                    partyhubPlaylist.Opacity = 1;
                }
                else
                {

                    if (Playlistnumber2 > ProfilePlaylistObj.Items.Count)
                    {
                        Playlistnumber2 = random.Next(1, ProfilePlaylistObj.Items.Count - 1);

                    }
                    // Get playlist Id from search object.
                    var UserPlaylistId = ProfilePlaylistObj.Items[Playlistnumber2].Id;
                    if (ProfilePlaylistObj.Items[Playlistnumber2].Tracks.Total > 50)
                    {
                        Playlistnumber2 = random.Next(1, ProfilePlaylistObj.Items.Count - 1);
                        UserPlaylistId = ProfilePlaylistObj.Items[Playlistnumber2].Id;
                    }
                    if (ProfilePlaylistObj.Items[Playlistnumber2].Tracks.Total <= 0)
                    {
                        Playlistnumber2 = random.Next(1, ProfilePlaylistObj.Items.Count - 1);
                        UserPlaylistId = ProfilePlaylistObj.Items[Playlistnumber2].Id;
                    }
                    //Converting Playlist Id to tracks of playlist.
                    Tuple<ResponseInfo, string> PlaylistTop50 = client.Download(builder.GetPlaylistTracks(Profileobj.Id, UserPlaylistId), headers);
                    var Playlistobj = JsonConvert.DeserializeObject<Paging<PlaylistTrack>>(PlaylistTop50.Item2);
                    var playlistTrackstotal = Playlistobj.Items.Count;
                    var playlistTracks = random.Next(1, playlistTrackstotal);
                    //MessageBox.Show(playlistTracks.ToString());
                    // Get random Track Number To print on Swipe function after swipe.

                    if (playlistTracks >= playlistTrackstotal)
                    {
                        playlistTracks = random.Next(1, playlistTrackstotal);
                    }
                    if (playlistTracks <= 0)
                    {
                        playlistTracks = random.Next(1, playlistTrackstotal);
                    }

                    CurrentTrackNum = playlistTracks;
                    // MessageBox.Show(Playlistobj.Total.ToString()+" "+ searchObj.Playlists.Items[Playlistnumber].Name +" "+ searchObj.Playlists.Items[Playlistnumber].Tracks.Total+" trackn : "+ CurrentTrackNum);
                    Test();
                    CurrentTrack = Playlistobj.Items[CurrentTrackNum].Track.Id;
                    //MessageBox.Show("Playing from User Playlist");

                }

            }
            else
            {
                Tuple<ResponseInfo, string> Search = client.Download(builder.SearchItems("TOP 50", Spotify.Enums.SearchType.Playlist, 22, 0, "US"), headers);
                var searchObj = JsonConvert.DeserializeObject<SearchItem>(Search.Item2);
                // To make it random Both tracks and playlist are random. Playlist number from 1- 20 and track from 1-50.
                int Playlistnumber = random.Next(1, 20);
                if (Playlistnumber > searchObj.Playlists.Items.Count)
                {
                    Playlistnumber = random.Next(1, 20);

                }
                // Get playlist Id from search object.
                var PlaylistId = searchObj.Playlists.Items[Playlistnumber].Id;

                if (searchObj.Playlists.Items[Playlistnumber].Tracks.Total > 50)
                {
                    Playlistnumber = random.Next(1, 20);
                    PlaylistId = searchObj.Playlists.Items[Playlistnumber].Id;
                }
                if (searchObj.Playlists.Items[Playlistnumber].Tracks.Total <= 5)
                {
                    Playlistnumber = random.Next(1, 20);
                    
                    PlaylistId = searchObj.Playlists.Items[Playlistnumber].Id;
                }
                //Converting Playlist Id to tracks of playlist.
                Tuple<ResponseInfo, string> PlaylistTop50 = client.Download(builder.GetPlaylistTracks(Profileobj.Id, PlaylistId), headers);
                var Playlistobj = JsonConvert.DeserializeObject<Paging<PlaylistTrack>>(PlaylistTop50.Item2);
                var playlistTracks = searchObj.Playlists.Items[Playlistnumber].Tracks.Total;
                // Get random Track Number To print on Swipe function after swipe.
                if (playlistTracks >= 50)
                {
                    playlistTracks = random.Next(1, 49);
                }
                int number = random.Next(0, playlistTracks - 1);
                CurrentTrackNum = number;
                // MessageBox.Show(Playlistobj.Total.ToString()+" "+ searchObj.Playlists.Items[Playlistnumber].Name +" "+ searchObj.Playlists.Items[Playlistnumber].Tracks.Total+" trackn : "+ CurrentTrackNum);
                Test();
                CurrentTrack = Playlistobj.Items[CurrentTrackNum].Track.Id;
                //MessageBox.Show("Playing from party hub");

            }
            return CurrentTrack;
            //User Playlist


        }
        private void partyhubPlaylistclick(object sender, RoutedEventArgs e)
        {
            // NextSongTrackPreview();
            UserplaylistBool = false;
            UserPlaylistText.Text = "User Playlists";
            GlobalPlaylistText.Text = "Playing From Global Top Playlists";
            UserPlaylist.Opacity = 0.6;
            partyhubPlaylist.Opacity = 1;
        }
        private void UserPlaylistclick(object sender, RoutedEventArgs e)
        {
            // NextSongTrackPreview();
            UserplaylistBool = true;
            UserPlaylistText.Text = "Playing From User Playlists";
            GlobalPlaylistText.Text = "Global Top Playlists";
            UserPlaylist.Opacity = 1;
            partyhubPlaylist.Opacity = 0.6;
        }
        void Start()
        {
            // do stuff
        }

        void Test()
        {
            new Thread(new ThreadStart(Start)).Start();
            new Thread(new ThreadStart(Start)).Abort();


        }
        // Number will be changed for track.
        private int ChangeTrack(int number)
        {

            return number;

        }
        // This function will printout Swipe Function for user to swipe.
        private void PrintTrackAsSwipe(FullTrack Track, PrivateProfile Profile)
        {

            //New track
            CurrentTrackDB = Track;
            TrackName.Text = Track.Name;
            if (Track.Name.Length >= 45)
            {
                TrackName.FontSize = 16;
            }
            // Setting Api data to variables or objects from XAML to print out.
            AlbumImage.Source = GetImage(Track.Album.UrlImage);
            TrackArtist.Text = Track.Artists[0].Name;
            TrackNameSmall.Text = Track.Name;
            if (Track.Name.Length >= 50)
            {
                TrackNameSmall.FontSize = 16;
            }
            // User Profile Image and text
            AlbumImageSmall.Source = GetImage(Track.Album.UrlImage);
            ArtistNameSmall.Text = Track.Artists[0].Name;
            songPreview = Track.PreviewUrl;
            // If song preview is not available , Random track again until preview song is available.

            if (songPreview == null)
            {
                NextSongTrackPreview();

            }
            
            
            profileID = Profile.Id;
            TrackID = null;
            TrackID = Track.Id;
            
        }
        private void FulltrackLink(object sender, RoutedEventArgs e)
        {
            var spotifyOpen = "https://open.spotify.com/track/";
            var TrackSpotifyLink = spotifyOpen + CurrentTrackDB.Id;
            System.Diagnostics.Process.Start(TrackSpotifyLink);
            // Tuple<ResponseInfo, string> createPlaylis1t = client.Download(builder.SaveTracks(), headers);
            
        }
        // If User Swipe right this function will load which will add liked track to the database.
        public void AddLikedTrackToDataBase(string userID)
        {
            string CheckSql;
            // Opens new connection for database to add track.
            sqlAzureConnection = new SqlConnection(connectionString);
            sqlAzureConnection.Open();
            // Checks if database already exists.
            CheckSql = $"select TrackID,Bruger_SpotifyID from ProfileLikedTrack WHERE TrackID  = '{CurrentTrackDB.Id}' AND Bruger_SpotifyID = '{userID}'";
            SqlCommand CommandCheck = new SqlCommand(CheckSql, sqlAzureConnection);
            dataReader = CommandCheck.ExecuteReader();
            if (!dataReader.HasRows)
            {
                // Closes datareader before using insertcommand from adapter.
                dataReader.Close();
                // Inserts tracks to database.

                adapter.InsertCommand = new SqlCommand("INSERT INTO ProfileLikedTrack (TrackID,TrackImage,TrackName,ArtistsName ,AlbumName ,TrackDuration,Bruger_SpotifyID) values (@ID, @Image, @Name, @ArtistsName, @AlbumName, @Duration, @UserID)", sqlAzureConnection);

                adapter.InsertCommand.Parameters.Add(new SqlParameter("@ID", CurrentTrackDB.Id));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@Image", CurrentTrackDB.Album.UrlImage));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@Name", CurrentTrackDB.Name));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@ArtistsName", CurrentTrackDB.Artists[0].Name));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@AlbumName", CurrentTrackDB.Album.Name));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@Duration", CurrentTrackDB.GetDuration));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@UserID", userID));
                adapter.InsertCommand.ExecuteNonQuery();
                adapter.InsertCommand.Dispose();
                //Closes database connections.
                sqlAzureConnection.Close();
            }

        }
        // Tracks is also added to global if it already does not exists.
        private void AddTrackToGlobal()
        {
            // Checks if track is already liked by someone and if it is then adds +1 Likes to it otherwise adds tracks to the database and +1 likes from user.
            string sql = "";
            string CheckSql = "";
            // opens connections.
            sqlAzureConnection = new SqlConnection(connectionString);
            sqlAzureConnection.Open();
            CheckSql = $"SELECT TrackId,Likes from partyhubGlobal WHERE TrackId = '{CurrentTrackDB.Id}'";
            SqlCommand CommandCheck = new SqlCommand(CheckSql, sqlAzureConnection);
            dataReader = CommandCheck.ExecuteReader();
            // Runs until datareader is availabel.
            if (dataReader.HasRows)
            {
                dataReader.Read();
                // Updates likes to global playlist.
                long likes = (long)dataReader.GetValue(1) + 1;
                string updatedlike = likes.ToString();
                long testlike = Int64.Parse(updatedlike);
                sql = $"UPDATE PartyHubGlobal Set Likes = '{testlike}' where TrackId = '{CurrentTrackDB.Id}'";
                // After updating sends data to sql database and closes datareader and all the connections.
                Command = new SqlCommand(sql, sqlAzureConnection);
                adapter.InsertCommand = new SqlCommand(sql, sqlAzureConnection);
                dataReader.Close();
                adapter.InsertCommand.ExecuteNonQuery();
                Command.Dispose();
                sqlAzureConnection.Close();


            }
            else
            {
                // Else adds track to global data table and adds user +1 like.
                dataReader.Close();
                long likes = 1;

                adapter.InsertCommand = new SqlCommand("INSERT INTO partyhubGlobal (TrackID,TrackImage,TrackName,ArtistsName ,AlbumName ,TrackDuration,Likes) values (@ID, @Image, @Name, @ArtistsName, @AlbumName, @Duration, @Likes)", sqlAzureConnection);
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@ID", CurrentTrackDB.Id));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@Image", CurrentTrackDB.Album.UrlImage));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@Name", CurrentTrackDB.Name));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@ArtistsName", CurrentTrackDB.Artists[0].Name));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@AlbumName", CurrentTrackDB.Album.Name));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@Duration", CurrentTrackDB.GetDuration));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@Likes", likes));

                adapter.InsertCommand.ExecuteNonQuery();
                adapter.InsertCommand.Dispose();
                sqlAzureConnection.Close();
            }
        }
        // Button click will play the current swipe preview song.
        private void btnPlayPreview(object sender, RoutedEventArgs e)
        {
            // Checks if mediaplayer is playing then it will stop the song and vice versa to else statement.
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
        // Checks is play or stop setting is called.
        private void playmp3(string path, string playState)
        {
            // Current song preview path.
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
        // If user mouse down the swipe preview then user can drag the swipe preview to left or right. also called SWIPE.
        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _positionInBlock = Mouse.GetPosition(ContentUsercontrol);
            ContentUsercontrol.CaptureMouse();
            //  ContentUsercontrol.Opacity = 0.7;
            notliked.Visibility = Visibility.Visible;
            liked.Visibility = Visibility.Visible;
            partyhubPlaylist.Visibility = Visibility.Hidden;
            UserPlaylist.Visibility = Visibility.Hidden;
        }

        // Image converter.
        public ImageSource GetImage(string Link)
        {
            return BitmapFrame.Create(new Uri(Link));
        }
        // This is the head or main code to how Swipe works. We have used Translatetransform to check if user is dragging out of specific X-Axis value.
        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            // If content is dragging, then it will select the page content.
            if (ContentUsercontrol.IsMouseCaptured)
            {
                var container = VisualTreeHelper.GetParent(ContentUsercontrol) as UIElement;
                var mousePosition = e.GetPosition(container);
                ContentUsercontrol.RenderTransform = new TranslateTransform(mousePosition.X - _positionInBlock.X, 0);
                // This is where User swipes to Left. It is check if Swipe content is dragging to +60 X-axis value.
                if (ContentUsercontrol.RenderTransform.Value.OffsetX <= (ContentGrid.ActualWidth * -1) + 60)
                {


                    // If it x-axis value meets then this code will run which will stop the song and call next Track to Swipe.
                    FirstRandom = false;
                    ContentUsercontrol.RenderTransform = new TranslateTransform(mousePosition.X - _positionInBlock.X, 0);
                    playmp3(songPreview, "Stop");
                    PlayandPause.Source = new BitmapImage(new Uri(@"/Content\Play.png", UriKind.Relative));

                    ContentUsercontrol.RenderTransform = new TranslateTransform(0, 0);
                    //ContentGrid.Children.Clear();
                    //Play next Song.
                    //Thread.Sleep(200);

                    //MessageBox.Show("TRACK NOT LIKED!");
                    //AutoClosingMessageBox.Show("Track Not Liked!", "", 03);
                    NextSongTrackPreview();
                    ContentUsercontrol.ReleaseMouseCapture();
                   
                }
                // This is where User swipes to Right. It is check if Swipe content is dragging to +60 X-axis value.

                else if (ContentUsercontrol.RenderTransform.Value.OffsetX >= ContentGrid.ActualWidth - 40)
                {


                    // If it x-axis value meets then this code will run which will stop the song and call next Track to Swipe.
                    AddTrackToGlobal();
                    FirstRandom = false;
                    ContentUsercontrol.RenderTransform = new TranslateTransform(mousePosition.X - _positionInBlock.X, 0);
                    playmp3(songPreview, "Stop");
                    PlayandPause.Source = new BitmapImage(new Uri(@"/Content\Play.png", UriKind.Relative));

                    ContentUsercontrol.RenderTransform = new TranslateTransform(0, 0);
                    AddLikedTrackToDataBase(profileID);
                    //
                    // Stupid Error fixing with stupid Solution. Might delete Later. Cause of +1 becoming +2 while liking a track.
                    //AutoClosingMessageBox.Show("Track Liked!","",10);
                    ContentGrid.Focus();
                    ContentUsercontrol.Focus();
                    //Play next Song.
                    // MessageBox.Show("TRACK LIKED!");
                    // Thread.Sleep(200);
                    //  AutoClosingMessageBox.Show("Track Liked!", "", 03);

                    NextSongTrackPreview();
                    ContentUsercontrol.ReleaseMouseCapture();


                    // Thread.Sleep(1000);


                }


            }


        }
        // Nextsongtrack methods will be called on both swipe left or right and it will call next random track from search playlist.
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
            ContentUsercontrol.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            PlayPreview.Focus();
            notliked.Visibility = Visibility.Hidden;
            liked.Visibility = Visibility.Hidden;
            partyhubPlaylist.Visibility = Visibility.Visible;
            UserPlaylist.Visibility = Visibility.Visible;
            PlayPreview.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        }
        // Autoclosing messagebox for temporary uses. 
        public class AutoClosingMessageBox
        {
            System.Threading.Timer _timeoutTimer;
            string _caption;
            AutoClosingMessageBox(string text, string caption, int timeout)
            {
                _caption = caption;
                _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                    null, timeout, System.Threading.Timeout.Infinite);
                using (_timeoutTimer)
                {
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
        // If this swipe is mouseuped the it will release the content from mouse grip.
        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ContentUsercontrol.ReleaseMouseCapture();
            ContentUsercontrol.Opacity = 1;
            ContentUsercontrol.RenderTransform = new TranslateTransform(0, 0);
            notliked.Visibility = Visibility.Hidden;
            liked.Visibility = Visibility.Hidden;
            partyhubPlaylist.Visibility = Visibility.Visible;
            UserPlaylist.Visibility = Visibility.Visible;
            // FocusManager.SetFocusedElement(FocusManager.GetFocusScope(ContentUsercontrol), null);
            // Keyboard.ClearFocus();
        }
        // When user goes to another frame or loses focus from content then music will be stopped.
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
