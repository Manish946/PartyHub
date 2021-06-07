﻿using System;
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
    /// Interaction logic for Minliste.xaml
    /// </summary>
    public partial class Minliste : Page
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
        public Minliste()
        {
            InitializeComponent();
            //Headers is being added and builder is ready to use.
            headers.Add("Authorization", "Bearer " + LoginWindow.SpotifyLogin.AccessToken);
            // Spotify User Objects.
            Tuple<ResponseInfo, string> Profile = client.Download(builder.GetPrivateProfile(), headers);
            var Profileobj = JsonConvert.DeserializeObject<PrivateProfile>(Profile.Item2);
            // Get spotify track
            Tuple<ResponseInfo, string> Track = client.Download(builder.GetTrack("3CVb6hkMrlF7eHhXi5B3PZ"), headers);
            var Trackobj = JsonConvert.DeserializeObject<FullTrack>(Track.Item2);
            getUserLikedTracks(Trackobj, Profileobj);

        }
        private void getUserLikedTracks(Spotify.Models.FullTrack Track, Spotify.Models.PrivateProfile User)
        {

            // Strings to help check if sql already exists and images.
            string CheckSql = "";
            List<Spotify.Models.FullTrack> LikedUserTracksList = new List<FullTrack>();
            // Database opening and making queries.
            sqlAzureConnection = new SqlConnection(connectionString);
            sqlAzureConnection.Open();
            CheckSql = $"select TrackID from ProfileLikedTrack WHERE Bruger_SpotifyID = '{User.Id}'";
            SqlCommand CommandCheck = new SqlCommand(CheckSql, sqlAzureConnection);
            dataReader = CommandCheck.ExecuteReader();
            if (dataReader.HasRows)
            {
                // While loop will be ran until database items is available.
                while (dataReader.Read())
                {
                    // User Swiped / Liked Tracks will be added and displayed on datagrid.
                    var UserLikedTrack = dataReader.GetValue(0).ToString();
                    Tuple<ResponseInfo, string> TrackAPI = client.Download(builder.GetTrack(UserLikedTrack), headers);
                    var Trackobj = JsonConvert.DeserializeObject<FullTrack>(TrackAPI.Item2);
                    LikedUserTracksList.Add(Trackobj);

                };
                MinListeGrid.ItemsSource = null;
                MinListeGrid.ItemsSource = LikedUserTracksList;
                /*
                SqlDataAdapter data = new SqlDataAdapter(CheckSql, sqlAzureConnection) ;
                DataTable FullTrack = new DataTable();
                data.Fill(FullTrack);
                MinListeGrid.ItemsSource = FullTrack.DefaultView;
                */
                // After executing query, we are closing connecting because we are not using it anymore and it is also good for the best practice.
                dataReader.Close();
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
            button.Visibility = Visibility.Hidden;

        }
    }
}
