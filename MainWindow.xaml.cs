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
using System.Threading;
using System.Data.SqlClient;

namespace PartyHub
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //After receving token now we can Spotify API in our window Application.
        Dictionary<string, string> headers = new Dictionary<string, string>();
        // Setting Client and builder to call Spotify API.
        SpotifyWebClient client = new SpotifyWebClient();
        SpotifyWebBuilder builder = new SpotifyWebBuilder();
        //Database Connection Setup
        private string connectionString = @"Server=tcp:partyhubserver.database.windows.net,1433;Initial Catalog=Partyhub_Database;Persist Security Info=False;User ID=partyhublogin;Password=Passw0rd;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        SqlConnection sqlAzureConnection;
        SqlDataReader dataReader;
        SqlCommand Command;
        SqlDataAdapter adapter = new SqlDataAdapter();
        public MainWindow()
        {
            InitializeComponent();
            // This will set PartyHub Window to user screen size.
            MaxHeight = SystemParameters.FullPrimaryScreenHeight - 60;
            MaxWidth = SystemParameters.FullPrimaryScreenWidth - 60;
            //Headers is being added and builder is ready to use.
            headers.Add("Authorization", "Bearer " + LoginWindow.SpotifyLogin.AccessToken);
            // Calling PrivateProfile Objects from Spotify.
            Tuple<ResponseInfo, string> tuple = client.Download(builder.GetPrivateProfile(), headers);
            //Object is converted to json for easier uses. obj Object is used to declare privateProfile API.
            var obj = JsonConvert.DeserializeObject<PrivateProfile>(tuple.Item2);
            //Setting user and full name to Users' Spotifys username and Displayname.
            UserName.Content = obj.DisplayName.Substring(0, 1).ToUpper() + obj.DisplayName.Substring(1, obj.DisplayName.Length - 1);
            fullname.Content = obj.DisplayName.Substring(0, 1).ToUpper() + obj.DisplayName.Substring(1, obj.DisplayName.Length - 1);
            LoginWindow.SpotifyLogin.IdCurrentUser = obj.Id;
            LoginWindow.SpotifyLogin.CurrentLocation = obj.Country;
            // If the user does not have set image, there will be set a default image otherwise, spotify image will be used.
            if (obj.Images != null && !obj.Images.Any())
            {
                //Default Image already set!
            }
            else
            {
                userImage.ImageSource = GetImage(obj.Images[0].Url);
            }
            //SQL CONNECTION SETUP IMPLEMENT!
            AddUserToDatabase(obj);
            //Frame_Partyhub.Content = new Content_Page.GlobalListe();
            Frame_Partyhub.Content = new Content_Page.Dashboard();

        }
        private void AddUserToDatabase(Spotify.Models.PrivateProfile User)
        {
            // Strings to help check if sql already exists and images.
            string sql = "";
            string CheckSql = "";
            string ValidImage = "";
            // Database opening and making queries.
            sqlAzureConnection = new SqlConnection(connectionString);
            sqlAzureConnection.Open();
            CheckSql = $"select Bruger_SpotifyId from Partyhub_Konto WHERE Bruger_SpotifyId = '{User.Id}'";
            SqlCommand CommandCheck = new SqlCommand(CheckSql, sqlAzureConnection);
            dataReader = CommandCheck.ExecuteReader();
            if (!dataReader.HasRows)
            {
                // If userdoesnt have set image then NO IMAGE Text will be sent to Database , That way We won't get error while receving no image user is logged in.
                if (User.Images != null && !User.Images.Any())
                {
                    ValidImage = "NO IMAGE";
                }
                else
                {
                    ValidImage = User.Images[0].Url;
                }
                dataReader.Close();
                sql = $"insert into Partyhub_Konto (Bruger_SpotifyId,DisplayNavn,Profile_picture,Email) values('{User.Id}','{User.DisplayName}','{ValidImage}','{User.Email}');";
                Command = new SqlCommand(sql, sqlAzureConnection);
                adapter.InsertCommand = new SqlCommand(sql, sqlAzureConnection);
                adapter.InsertCommand.ExecuteNonQuery();
                // After executing query, we are closing connecting because we are not using it anymore and it is also good for the best practice.
                Command.Dispose();
                sqlAzureConnection.Close();
            }

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }


        public ImageSource GetImage(string Link)
        {
            return BitmapFrame.Create(new Uri(Link));
        }

        private void OnAfslutClick(object sender, RoutedEventArgs e)
        {
            this.Close();
            Environment.Exit(Environment.ExitCode);
        }
        // This is a logout button which allows user to logout if they want to change user or other purposes.
        private void Logud_click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://accounts.spotify.com/en/logout");
            LoginWindow loginWin = new LoginWindow();
            // loginWin.stopServer();
            loginWin.Show();
            Close();
        }

        private void Swipe_click(object sender, RoutedEventArgs e)
        {

            Frame_Partyhub.NavigationService.Navigate(new Content_Page.Swipe());

            // Content_Page.Swipe swipePage = new Content_Page.Swipe();
            // Navigate(swipePage);
        }
        // These following Click Buttons are using Navigate method to navigate to different frame from Content pages.
        public static void Navigate(object target)
        {
            ((MainWindow)Application.Current.Windows[2]).Frame_Partyhub.Content = target;
        }

        private void Globalliste_click(object sender, RoutedEventArgs e)
        {


            Frame_Partyhub.NavigationService.Navigate(new Content_Page.GlobalListe());


        }

        private void Minliste_click(object sender, RoutedEventArgs e)
        {

            Frame_Partyhub.NavigationService.Navigate(new Content_Page.Minliste());

           

        }

        private void Minprofil_click(object sender, RoutedEventArgs e)
        {
            Frame_Partyhub.NavigationService.Navigate(new Content_Page.Minprofil());

        }

        // This function will minimize the window if minimized icon is clicked.
        private void Minimize_click(object sender, RoutedEventArgs e)
        {
            this.ResizeMode = ResizeMode.CanMinimize;
            this.Focusable = false;
            this.ShowActivated = false;
            this.WindowState = WindowState.Minimized;
        }

        private void DashBoard_Click(object sender, RoutedEventArgs e)
        {
            Frame_Partyhub.NavigationService.Navigate(new Content_Page.Dashboard());

        }
    }
    // Image converter
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
