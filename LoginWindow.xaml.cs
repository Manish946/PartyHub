using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;
using Newtonsoft.Json;
using Spotify;
using Spotify.Auth;
using Spotify.Enums;
using Spotify.Models;

namespace PartyHub
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        readonly AutorizationCodeAuth auth;
        readonly Dictionary<string, string> headers = new Dictionary<string, string>();
        public bool IsBackGround { get; set; }
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = this;
            auth = new AutorizationCodeAuth()
            {
                ShowDialog = false,
                Scope = Scope.UserReadRecentlyPlayed | Scope.UserReadPrivate | Scope.UserReadEmail | Scope.UserLibraryRead | Scope.PlaylistReadPrivate,
                ClientId = SpotifyLogin.GetClient_Id,
                RedirectUri = SpotifyLogin.GetRedirect_Uri
            };

            auth.OnResponseReceivedEvent += Auth_OnResponseReceivedEvent;

        }


        private void Auth_OnResponseReceivedEvent(AutorizationCodeAuthResponse response)
        {
            if (response.Error == null)
            {
                SpotifyLogin.GrantCode = response.Code;
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    auth.StopHttpServer();
                    Token a = auth.ExchangeAuthCode(SpotifyLogin.GrantCode, SpotifyLogin.GetClient_Secret);
                    headers.Add("Authorization", "Bearer " + a.AccessToken);
                    if (a != null)
                    {
                        SpotifyLogin.AccessToken = a.AccessToken;
                        SpotifyLogin.RefreshToken = a.RefreshToken;
                        MainWindow main = new MainWindow();
                        main.Show();
                        this.Visibility = Visibility.Hidden;
                        main.WindowState = WindowState.Normal;

                        if (main.WindowState == WindowState.Normal)
                        {
                            main.Focus();
                            main.Activate();
                        }

                        
                        

                    }
                });
            }
            else
            {
                MessageBox.Show("Error!");
            }
        }

        private void Login_Med_Spotify(object sender, RoutedEventArgs e)
        {
            Autorization();

        }

        private void Autorization()
        {

            auth.StartHttpServer();
            auth.DoAuth();


        }

        public class SpotifyLogin
        {
            public static string GetClient_Id { get; } = "e6e936eabb994075b4781bada6df5c4f";

            public static string GetClient_Secret { get; } = "82c3d6f6c406450dac8c81a1baeeb40f";

            public static string GetRedirect_Uri { get; } = "http://localhost:80";

            public static string GrantCode { get; set; }

            public static string AccessToken { get; set; }

            public static string RefreshToken { get; set; }

            public static string IdCurrentUser { get; set; }

            public static string CurrentLocation { get; set; }
         

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void Afslut_click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Environment.Exit(Environment.ExitCode);
        }
    }
}
