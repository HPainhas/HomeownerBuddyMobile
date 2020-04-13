﻿using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Net.Http;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace HOB_Mobile.Views

{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    [DesignTimeVisible(false)]

    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();

            //Check and see if user registered previously, if they have, redirect them to the HomePage
            //set the testing boolean to true to see register page for testing purposes
            SetUpLoginPageHabitatHumanityLogo();
            var testing = false;
            if (!Preferences.Get("user_home_code", "default_value").Equals("default_value") && !testing)
            {
                
                Navigation.PushAsync(new HomePage(Preferences.Get("user_first_name", "")));
                // Call function that adds the logo to the register page
                
            }

        }

        /*
         * Handle the display of the Habitat For Humanity Mid-Ohio logo
         */
        private void SetUpLoginPageHabitatHumanityLogo()
        {
            // Add the Humanity Mid-Ohio logo stored in the Resources folder to its respective Image in the RegisterPage.xaml file
            habitat_humanity_logo.Source = ImageSource.FromResource("HOB_Mobile.Resources.habitat_midohio_logo.jpg");
        }

        /*
         * Listener for "Register" button
         */
        private void HandleRegisterButtonClick(object sender, EventArgs e)
        {
            // Get the text entered by the user in each of the input forms (Home Code, First Name and Last Name)
            // by it's x:Name followed by .Text
            string userHomeCode = homeowner_buddy_home_code.Text;
            string userFirstName = homeowner_buddy_first_name.Text;
            string userLastName = homeowner_buddy_last_name.Text;

            // Check if any of the input forms are empty when the user clicks the "Register" button
            if (userHomeCode == null || userFirstName == null || userLastName == null)
            { 
                DisplayAlert("", "All fields are required", "OK");

            } else
            {
                //This is where we store the home code and name, we are going to use Preferences to see if a user is logged in
                Preferences.Set("user_home_code", userHomeCode);
                Preferences.Set("user_first_name", userFirstName);
                Preferences.Set("user_last_name", userLastName);

                //POST
                PostUserInfo(userHomeCode, userFirstName, userLastName);

                
            }
        }

        public async void PostUserInfo(String userHomeCode, String userFirstName, String userLastName)
        {
            // Set up new HttpClientHandler and its credentials so we can perform the web request
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            // Create new httpClient using our client handler created above
            HttpClient httpClient = new HttpClient(clientHandler);

            String apiUrl = null;
            if (Device.RuntimePlatform == Device.Android) apiUrl = "https://10.0.2.2:5001/api/MobileUsersAPI";
            else if (Device.RuntimePlatform == Device.iOS) apiUrl = "https://localhost:5001/api/MobileUsersAPI";

            // Create new URI with the API url so we can perform the web request
            var uri = new Uri(string.Format(apiUrl, string.Empty));

            MobileUsers user = new MobileUsers();
            user.FName = userFirstName;
            user.Lname = userLastName;
            user.Code = userHomeCode;

            string JSONresult = JsonConvert.SerializeObject(user);
            var content = new StringContent(JSONresult, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

            // Keep track of id from the database - will be used to unregister home
            if (response.IsSuccessStatusCode)
            {
                // Get response from POST request
                var tokenJson = await response.Content.ReadAsStringAsync();
                var array = tokenJson.Split('"');
                String id = array[2];
                String address = array[17];
                id = id.Substring(1);
                id = id.TrimEnd(',');
                // Save the id in preferences
                Preferences.Set("user_id", id);
                Preferences.Set("user_address", address);
                //Preferences.Set("user_address", )
                await Navigation.PushAsync(new HomePage(Preferences.Get("user_first_name", "")));
            } else
            {
                await DisplayAlert("", "HomeCode does not exist", "OK");
            }
        }
    }
}
