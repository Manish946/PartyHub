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
        // Using AutorizationCodeAuth From Spotify Libary.
        readonly AutorizationCodeAuth auth;
        readonly Dictionary<string, string> headers = new Dictionary<string, string>();
        public bool IsBackGround { get; set; }
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = this;
            // Setting Information to get Spotify Autorization. Scope, ClientId, and redirectUri are set from Spotify Dashboard.
            auth = new AutorizationCodeAuth()
            {
                ShowDialog = false,
                Scope = Scope.UserReadRecentlyPlayed | Scope.UserReadPrivate | Scope.UserReadEmail | Scope.UserLibraryRead | Scope.PlaylistReadPrivate,
                ClientId = SpotifyLogin.GetClient_Id,
                RedirectUri = SpotifyLogin.GetRedirect_Uri
            };
            // This event handles the Autorization Response from spotify API.
            auth.OnResponseReceivedEvent += Auth_OnResponseReceivedEvent;

        }


        private void Auth_OnResponseReceivedEvent(AutorizationCodeAuthResponse response)
        {
            if (response.Error == null)
            {
                // After receving auth code, we are implementing it.
                SpotifyLogin.GrantCode = response.Code;
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    // Here we have gotten auth code from redirecturi and have stopped the Http Server.
                    //auth.StopHttpServer();
                    // Token is received.
                    Token a = auth.ExchangeAuthCode(SpotifyLogin.GrantCode, SpotifyLogin.GetClient_Secret);
                    headers.Add("Authorization", "Bearer " + a.AccessToken);
                    if (a != null)
                    {
                        // Tokens are being set to SpotifyLogin Class.
                        SpotifyLogin.AccessToken = a.AccessToken;
                        SpotifyLogin.RefreshToken = a.RefreshToken;
                        MainWindow main = new MainWindow();
                        
                        main.Show();
                        auth.StopHttpServer();

                        this.Visibility = Visibility.Hidden;
                        main.WindowState = WindowState.Normal;
                        // This code will make our main PartyHub window active and will be launched after spotify is logged in.

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
        //This button authorize spotify login.
        private void Login_Med_Spotify(object sender, RoutedEventArgs e)
        {
            Autorization();

        }
        // This function starts the HTTPSERVER to login with spotify.
        private void Autorization()
        {

            auth.StartHttpServer();
            auth.DoAuth();


        }
        // Spotify Login Class where User information and tokens are stored. We can use it later to call API.
        public class SpotifyLogin
        {
            public static string GetClient_Id { get; } = "xxxxxxxxxxxxxxxxxxxxxxx";

            public static string GetClient_Secret { get; } = "xxxxxxxxxxxxxxxxxxxxxx";

            public static string GetRedirect_Uri { get; } = "http://localhost:80";

            public static string GrantCode { get; set; }

            public static string AccessToken { get; set; }

            public static string RefreshToken { get; set; }

            public static string IdCurrentUser { get; set; }

            public static string CurrentLocation { get; set; }
         

        }
        // This following code will allow user to drag window af limited area is clicked on hold.
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        // To Close the enitre PartyHub Application.
        private void Afslut_click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Environment.Exit(Environment.ExitCode);
        }
    }
}
