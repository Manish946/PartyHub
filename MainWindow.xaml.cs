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
        Dictionary<string, string> headers = new Dictionary<string, string>();
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

            MaxHeight = SystemParameters.FullPrimaryScreenHeight - 60;
            MaxWidth = SystemParameters.FullPrimaryScreenWidth - 60;

            Frame_Partyhub.Content = new Content_Page.Dashboard();
            //Displayname
            headers.Add("Authorization", "Bearer " + LoginWindow.SpotifyLogin.AccessToken);
            Tuple<ResponseInfo, string> tuple = client.Download(builder.GetPrivateProfile(), headers);
            var obj = JsonConvert.DeserializeObject<PrivateProfile>(tuple.Item2);
            UserName.Content = obj.DisplayName.Substring(0, 1).ToUpper() + obj.DisplayName.Substring(1, obj.DisplayName.Length - 1);
            fullname.Content = obj.DisplayName.Substring(0, 1).ToUpper() + obj.DisplayName.Substring(1, obj.DisplayName.Length - 1);
            LoginWindow.SpotifyLogin.IdCurrentUser = obj.Id;
            LoginWindow.SpotifyLogin.CurrentLocation = obj.Country;
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

        }

        private void AddUserToDatabase(Spotify.Models.PrivateProfile User)
        {
            string sql = "";
            string CheckSql = "";
            string ValidImage = "";

            sqlAzureConnection = new SqlConnection(connectionString);
            sqlAzureConnection.Open();
            CheckSql = $"select Bruger_SpotifyId from Partyhub_Konto WHERE Bruger_SpotifyId = '{User.Id}'";
            SqlCommand CommandCheck = new SqlCommand(CheckSql, sqlAzureConnection);
            dataReader = CommandCheck.ExecuteReader();
            if (!dataReader.HasRows)
            {
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
                Command.Dispose();
                sqlAzureConnection.Close();
            }
           
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }



        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {

            Frame_Partyhub.Content = new Content_Page.Dashboard();
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
            Frame_Partyhub.Content = null;
            while (Frame_Partyhub.NavigationService.RemoveBackEntry() != null) ;
            Frame_Partyhub.NavigationService.Navigate(new Content_Page.Swipe());

            // Content_Page.Swipe swipePage = new Content_Page.Swipe();
            // Navigate(swipePage);
        }

        public static void Navigate(object target)
        {
            ((MainWindow)Application.Current.Windows[2]).Frame_Partyhub.Content = target;
        }

        private void Party_click(object sender, RoutedEventArgs e)
        {
            Frame_Partyhub.Content = new Content_Page.Party();

        }

        private void Globalliste_click(object sender, RoutedEventArgs e)
        {


            Frame_Partyhub.Content = new Content_Page.GlobalListe();

        }

        private void Minliste_click(object sender, RoutedEventArgs e)
        {


            Frame_Partyhub.Content = new Content_Page.Minliste();

        }

        private void Minprofil_click(object sender, RoutedEventArgs e)
        {

            Frame_Partyhub.Content = new Content_Page.Minprofil();

        }

        private void Indstillinger_click(object sender, RoutedEventArgs e)
        {

            Frame_Partyhub.Content = new Content_Page.Indstillinger();

        }

        private void Minimize_click(object sender, RoutedEventArgs e)
        {
            this.ResizeMode = ResizeMode.CanMinimize;
            this.Focusable = false;
            this.ShowActivated = false;
            this.WindowState = WindowState.Minimized;
        }
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


}
