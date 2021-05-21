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
        Dictionary<string, string> headers = new Dictionary<string, string>();
        SpotifyWebClient client = new SpotifyWebClient();
        SpotifyWebBuilder builder = new SpotifyWebBuilder();
        public Minprofil()
        {
            InitializeComponent();
            headers.Add("Authorization", "Bearer " + LoginWindow.SpotifyLogin.AccessToken);
            Tuple<ResponseInfo, string> tuple = client.Download(builder.GetPrivateProfile(), headers);
            var PrivateProfileObj = JsonConvert.DeserializeObject<PrivateProfile>(tuple.Item2);
            Tuple<ResponseInfo, string> UserPlaylist = client.Download(builder.GetUserPlaylists(PrivateProfileObj.Id), headers);
            var UserPlaylistObj = JsonConvert.DeserializeObject<Paging<SimplePlaylist>>(UserPlaylist.Item2);
            userDisplay(PrivateProfileObj,UserPlaylistObj);
        }
        public ImageSource GetImage(string Link)
        {
            return BitmapFrame.Create(new Uri(Link));
        }
        private void userDisplay(PrivateProfile User, Paging<SimplePlaylist> UserPlaylist)
        {
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

    }
}
