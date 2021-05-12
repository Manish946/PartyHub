using Microsoft.Win32;
using NAudio.Wave;
using Newtonsoft.Json;
using Spotify;
using Spotify.Models;
using System;
using System.Collections.Generic;
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
        private MediaPlayer mediaplayer = new MediaPlayer();
        public string songPreview;
        WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();
        public double curpos = 0;
        public Swipe()
        {



            InitializeComponent();
            ContentUsercontrol.RenderTransform = new TranslateTransform(0, 0);
            headers.Add("Authorization", "Bearer " + LoginWindow.SpotifyLogin.AccessToken);
            Tuple<ResponseInfo, string> Profile = client.Download(builder.GetPrivateProfile(), headers);
            var Profileobj = JsonConvert.DeserializeObject<PrivateProfile>(Profile.Item2);

            Tuple<ResponseInfo, string> Track = client.Download(builder.GetTrack("5nujrmhLynf4yMoMtj8AQF"), headers);
            var Trackobj = JsonConvert.DeserializeObject<FullTrack>(Track.Item2);
            TrackName.Text = Trackobj.Name;
            AlbumImage.Source = GetImage(Trackobj.Album.UrlImage); 
            TrackArtist.Text = Trackobj.Artists[0].Name;
            TrackNameSmall.Text = Trackobj.Name;
            AlbumImageSmall.Source = GetImage(Trackobj.Album.UrlImage);
            ArtistNameSmall.Text = Trackobj.Artists[0].Name;
            songPreview = Trackobj.PreviewUrl;

            
        }

        private void btnPlayPreview(object sender, RoutedEventArgs e)
        {
            if (player.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                curpos = player.controls.currentPosition;
                playmp3(songPreview, "Stop");
                PlayandPause.Source = new BitmapImage(new Uri(@"/Content\Play.png", UriKind.Relative));

            }
            else
            {
                
                playmp3(songPreview, "Play");
                PlayandPause.Source = new BitmapImage(new Uri(@"/Content\Pause.png", UriKind.Relative));

            }
        }
        private void playmp3(string path, string playState)
        {            

            player.URL = path;
            player.settings.setMode("Loop",true);
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

                if (ContentUsercontrol.RenderTransform.Value.OffsetX <= ContentGrid.ActualWidth * -1)
                {

                    ContentUsercontrol.RenderTransform = new TranslateTransform(mousePosition.X - _positionInBlock.X, 0);
                    Thread.Sleep(500);
                    ContentUsercontrol.Visibility = Visibility.Visible;
                    ContentUsercontrol.RenderTransform = new TranslateTransform(0, 0);
                    //ContentGrid.Children.Clear();
                    Choice.Text = "Not Liked";
                }
                else if (ContentUsercontrol.RenderTransform.Value.OffsetX >= ContentGrid.ActualWidth)
                {
                    ContentUsercontrol.RenderTransform = new TranslateTransform(mousePosition.X - _positionInBlock.X, 0);
                    Thread.Sleep(500);
                    Choice.Text = "Liked";
                    ContentUsercontrol.RenderTransform = new TranslateTransform(0, 0);
                    //ContentGrid.Children.Clear();




                }

            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ContentUsercontrol.ReleaseMouseCapture();
            ContentUsercontrol.Opacity = 1;
            ContentUsercontrol.RenderTransform = new TranslateTransform(0, 0);


        }

       
    }
}
