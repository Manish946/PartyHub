using Newtonsoft.Json;
using Spotify;
using Spotify.Models;
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

    public partial class Minprofil : Page
    {
        //After receving token now we can Spotify API in our window Application.
        Dictionary<string, string> headers = new Dictionary<string, string>();
        // Setting Client and builder to call Spotify API.
        SpotifyWebClient client = new SpotifyWebClient();
        SpotifyWebBuilder builder = new SpotifyWebBuilder();
        public Minprofil()
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
            userDisplay(PrivateProfileObj,UserPlaylistObj);
        }
        // Converting image to bitmapframe for wpf image uses.
        public ImageSource GetImage(string Link)
        {
            return BitmapFrame.Create(new Uri(Link));
        }
        // User Layout will be displayed from this method.
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

            //First Playlist
            playlistImage1.ImageSource = GetImage(UserPlaylist.Items[0].UrlImage);
            PlaylistName1.Text = UserPlaylist.Items[0].Name;
            PlaylistFollowers1.Text = UserPlaylist.Items[0].Tracks.Total.ToString() + " Tracks";
            playlistImage2.ImageSource = GetImage(UserPlaylist.Items[1].UrlImage);
            PlaylistName2.Text = UserPlaylist.Items[1].Name;
            PlaylistFollowers2.Text = UserPlaylist.Items[1].Tracks.Total.ToString() + " Tracks";
            playlistImage3.ImageSource = GetImage(UserPlaylist.Items[2].UrlImage);
            PlaylistName3.Text = UserPlaylist.Items[2].Name;
            PlaylistFollowers3.Text = UserPlaylist.Items[2].Tracks.Total.ToString() + " Tracks";

        }

    }
}
