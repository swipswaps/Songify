﻿using Newtonsoft.Json;
using System;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using AutoUpdaterDotNET;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using Songify_Slim.Util.Settings;
using Songify_Slim.Util.Songify;

namespace Songify_Slim
{
    // ReSharper disable once InconsistentNaming
    public partial class Window_Settings
    {
        private bool useOwnApiClient = Settings.UseOwnApp;

        private readonly string[] _colors = {
                                                   "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald",
                                                   "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta",
                                                   "Crimson", "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve",
                                                   "Taupe", "Sienna"
                                       };

        private readonly FolderBrowserDialog _fbd = new FolderBrowserDialog();
        private Window _mW;

        public Window_Settings()
        {
            InitializeComponent();
            this.Title = Properties.Resources.mw_menu_Settings;
        }

        public void SetControls()
        {
            // Sets all the controls from settings
            ThemeToggleSwitch.IsChecked = Settings.Theme == "BaseDark";
            TxtbxOutputdirectory.Text = Assembly.GetEntryAssembly()?.Location ?? throw new InvalidOperationException();
            if (!string.IsNullOrEmpty(Settings.Directory))
                TxtbxOutputdirectory.Text = Settings.Directory;
            ChbxAutostart.IsChecked = Settings.Autostart;
            ChbxMinimizeSystray.IsChecked = Settings.Systray;
            ChbxCustomPause.IsChecked = Settings.CustomPauseTextEnabled;
            ChbxTelemetry.IsChecked = Settings.Telemetry;
            TxtbxCustompausetext.Text = Settings.CustomPauseText;
            TxtbxOutputformat.Text = Settings.OutputString;
            txtbx_nbuser.Text = Settings.NbUser;
            ChbxUpload.IsChecked = Settings.Upload;
            NudChrome.Value = Settings.ChromeFetchRate;
            ChbxCover.IsChecked = Settings.DownloadCover;
            ChbxSplit.IsChecked = Settings.SplitOutput;
            txtbx_twChannel.Text = Settings.TwChannel;
            txtbx_twOAuth.Password = Settings.TwOAuth;
            txtbx_twUser.Text = Settings.TwAcc;
            txtbx_RewardID.Text = Settings.TwRewardId;
            Chbx_TwReward.IsChecked = Settings.TwSrReward;
            Chbx_TwCommand.IsChecked = Settings.TwSrCommand;
            NudMaxReq.Value = Settings.TwSrMaxReq;
            NudCooldown.Value = Settings.TwSrCooldown;
            Chbx_MessageLogging.IsChecked = Settings.MsgLoggingEnabled;
            Chbx_TwAutoconnect.IsChecked = Settings.TwAutoConnect;
            ChbxSplit.IsChecked = Settings.SplitOutput;
            Chbx_AutoClear.IsChecked = Settings.AutoClearQueue;
            ChbxSpaces.IsChecked = Settings.AppendSpaces;
            nud_Spaces.Value = Settings.SpaceCount;
            tb_ClientID.Text = Settings.ClientId;
            tb_ClientSecret.Password = Settings.ClientSecret;
            Tglsw_Spotify.IsChecked = Settings.UseOwnApp;
            NudMaxlength.Value = Settings.MaxSongLength;

            if (Settings.NbUserId != null)
            {
                lbl_nightbot.Content = "Nightbot (ID: " + Settings.NbUserId + ")";
            }
            if (ApiHandler.Spotify != null)
                lbl_SpotifyAcc.Content = Properties.Resources.sw_Integration_SpotifyLinked + " " + ApiHandler.Spotify.GetPrivateProfile().DisplayName;

            ThemeHandler.ApplyTheme();

            cbx_Language.SelectionChanged -= ComboBox_SelectionChanged;
            switch (Settings.Language)
            {
                case "en":
                    cbx_Language.SelectedIndex = 0;
                    break;
                case "de-DE":
                    cbx_Language.SelectedIndex = 1;
                    break;
            }

            cbx_Language.SelectionChanged += ComboBox_SelectionChanged;
        }

        private void AppendText(System.Windows.Controls.TextBox tb, string text)
        {
            // Appends Rightclick-Text from the output text box (parameters)
            tb.AppendText(text);
            tb.Select(TxtbxOutputformat.Text.Length, 0);
            if (tb.ContextMenu != null) tb.ContextMenu.IsOpen = false;
        }

        private void Btn_ExportConfig_Click(object sender, RoutedEventArgs e)
        {
            // calls confighandler
            ConfigHandler.SaveConfig();
        }

        private void Btn_ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            // calls confighandler
            ConfigHandler.LoadConfig();
        }

        private void Btn_nblink_Click(object sender, RoutedEventArgs e)
        {
            // Links the nightbot account using username
            try
            {
                // accessing nightbot API with username to get user id
                string jsn;
                using (WebClient wc = new WebClient())
                {
                    jsn = wc.DownloadString("https://api.nightbot.tv/1/channels/t/" + Settings.NbUser);
                }

                NbObj json = JsonConvert.DeserializeObject<NbObj>(jsn);
                string temp = json.Channel._id;
                temp = temp.Replace("{", "").Replace("}", "");
                Settings.NbUserId = temp;
                Lbl_Status.Content = @"Account " + Settings.NbUser + " linked.";
            }
            catch (Exception ex)
            {
                Logger.LogExc(ex);
            }

            SetControls();
        }

        private void BtnCopyToClipClick(object sender, RoutedEventArgs e)
        {
            // Copies the txt path to the clipboard and shows a notification
            if (string.IsNullOrEmpty(Settings.Directory))
            {
                System.Windows.Clipboard.SetDataObject(Assembly.GetEntryAssembly()?.Location.Replace("Songify Slim.exe", "Songify.txt") ?? throw new InvalidOperationException());
            }
            else
            {
                System.Windows.Clipboard.SetDataObject(Settings.Directory + "\\Songify.txt");
            }
            Lbl_Status.Content = @"Path copied to clipboard.";
        }

        private void BtnCopyURL_Click(object sender, RoutedEventArgs e)
        {
            // Copies the song info URL to the clipboard and shows notification
            System.Windows.Clipboard.SetDataObject("https://songify.rocks/getsong.php?id=" + Settings.Uuid);
            Lbl_Status.Content = @"URL copied to clipboard.";
        }

        private void BtnOutputdirectoryClick(object sender, RoutedEventArgs e)
        {
            // Where the user wants the text file to be saved in
            _fbd.Description = @"Path where the text file will be located.";
            _fbd.SelectedPath = Assembly.GetExecutingAssembly().Location;

            if (_fbd.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;
            TxtbxOutputdirectory.Text = _fbd.SelectedPath;
            Settings.Directory = _fbd.SelectedPath;
        }

        private void BtnUpdatesClick(object sender, RoutedEventArgs e)
        {
            // checks for updates
            AutoUpdater.Mandatory = true;
            AutoUpdater.UpdateMode = Mode.ForcedDownload;
            AutoUpdater.RunUpdateAsAdmin = false;

            AutoUpdater.Start("https://songify.rocks/update.xml");
        }

        private void ChbxAutostartChecked(object sender, RoutedEventArgs e)
        {
            // checkbox for autostart
            bool? chbxAutostartIsChecked = ChbxAutostart.IsChecked;
            MainWindow.RegisterInStartup(chbxAutostartIsChecked != null && (bool)chbxAutostartIsChecked);
        }

        private void ChbxCustompauseChecked(object sender, RoutedEventArgs e)
        {
            // enables / disables custom pause
            if (ChbxCustomPause.IsChecked == null) return;
            Settings.CustomPauseTextEnabled = (bool)ChbxCustomPause.IsChecked;
            if (!(bool)ChbxCustomPause.IsChecked)
            {
                TxtbxCustompausetext.IsEnabled = false;
            }
            else
            {
                TxtbxCustompausetext.IsEnabled = true;
            }
        }

        private void ChbxMinimizeSystrayChecked(object sender, RoutedEventArgs e)
        {
            // enables / disbales minimize to systray
            bool? isChecked = ChbxMinimizeSystray.IsChecked;
            Settings.Systray = isChecked != null && (bool)isChecked;
        }

        private void ChbxTelemetry_IsCheckedChanged(object sender, EventArgs e)
        {
            // enables / disables telemetry
            if (ChbxTelemetry.IsChecked == null) return;
            Settings.Telemetry = (bool)ChbxTelemetry.IsChecked;
        }

        private void ChbxUpload_Checked(object sender, RoutedEventArgs e)
        {
            // enables / disables upload
            if (ChbxUpload.IsChecked != null)
                Settings.Upload = (bool)ChbxUpload.IsChecked;
            ((MainWindow)_mW).UploadSong(((MainWindow)_mW).CurrSong);
        }

        private void ComboBoxColorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // sets the color, when selecting yellow it changes foreground color because else its hard to read
            Settings.Color = ComboBoxColor.SelectedValue.ToString();
            ThemeHandler.ApplyTheme();
            if (Settings.Color != "Yellow")
            {
                ((MainWindow)_mW).LblStatus.Foreground = Brushes.White;
                ((MainWindow)_mW).LblCopyright.Foreground = Brushes.White;
            }
            else
            {
                ((MainWindow)_mW).LblStatus.Foreground = Brushes.Black;
                ((MainWindow)_mW).LblCopyright.Foreground = Brushes.Black;
            }
        }

        private void MenuBtnArtist_Click(object sender, RoutedEventArgs e)
        {
            // appends text
            AppendText(TxtbxOutputformat, "{artist}");
        }

        private void MenuBtnExtra_Click(object sender, RoutedEventArgs e)
        {
            // appends text
            AppendText(TxtbxOutputformat, "{extra}");
        }

        private void MenuBtnTitle_Click(object sender, RoutedEventArgs e)
        {
            // appends text
            AppendText(TxtbxOutputformat, "{title}");
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // assing mw to mainwindow for calling methods and setting texts etc
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    _mW = window;
                }
            }

            // add colors to the combobox
            foreach (string s in _colors)
            {
                ComboBoxColor.Items.Add(s);
            }

            // select the current color
            foreach (string s in ComboBoxColor.Items)
            {
                if (s != Settings.Color) continue;
                ComboBoxColor.SelectedItem = s;
                Settings.Color = s;
            }


            SetControls();
        }

        private void ThemeToggleSwitchIsCheckedChanged(object sender, EventArgs e)
        {
            // set the theme (BaseLight / BaseDark)
            if (ThemeToggleSwitch.IsChecked != null && (bool)ThemeToggleSwitch.IsChecked)
            {
                Settings.Theme = "BaseDark";
            }
            else
            {
                Settings.Theme = "BaseLight";

            }

            ThemeHandler.ApplyTheme();
        }

        private void Txtbx_nbuser_TextChanged(object sender, TextChangedEventArgs e)
        {
            // write Nightbot username to settings
            Settings.NbUser = txtbx_nbuser.Text;
        }

        private void TxtbxCustompausetext_TextChanged(object sender, TextChangedEventArgs e)
        {
            // write CustomPausetext to settings
            Settings.CustomPauseText = TxtbxCustompausetext.Text;
        }

        private void TxtbxOutputformat_TextChanged(object sender, TextChangedEventArgs e)
        {
            // write custom output format to settings
            Settings.OutputString = TxtbxOutputformat.Text;
        }

        // nightbot JSON object
        public class NbObj
        {
            public dynamic Channel { get; set; }
        }

        private void NudChrome_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            // Sets the source (Spotify, Youtube, Nightbot)
            if (!IsLoaded)
            {
                // This prevents that the selected is always 0 (initialize components)
                return;
            }

            if (NudChrome.Value != null) Settings.ChromeFetchRate = (int)NudChrome.Value;
        }

        private void ChbxCover_Checked(object sender, RoutedEventArgs e)
        {
            // enables / disables telemetry
            if (ChbxCover.IsChecked == null) return;
            Settings.DownloadCover = (bool)ChbxCover.IsChecked;
        }

        private void btn_spotifyLink_Click(object sender, RoutedEventArgs e)
        {
            // Links Spotify
            Settings.RefreshToken = "";
            try
            {
                ApiHandler.DoAuthAsync();
                SetControls();

            }
            catch (Exception ex)
            {
                Logger.LogExc(ex);
            }
        }

        private void ChbxSplit_Checked(object sender, RoutedEventArgs e)
        {
            // enables / disables telemetry
            if (ChbxSplit.IsChecked == null) return;
            if (ChbxCover.IsChecked != null) Settings.SplitOutput = (bool)ChbxCover.IsChecked;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Opens twitchapps to generate a TMI oAuth Token
            Process.Start("https://twitchapps.com/tmi/");
        }

        private void txtbx_RewardID_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Saves the RewardID
            Settings.TwRewardId = txtbx_RewardID.Text;
        }

        private void Chbx_TwReward_Checked(object sender, RoutedEventArgs e)
        {
            // enables / disables telemetry
            if (Chbx_TwReward.IsChecked == null) return;
            Settings.TwSrReward = (bool)Chbx_TwReward.IsChecked;
        }

        private void Chbx_TwCommand_Checked(object sender, RoutedEventArgs e)
        {
            // enables / disables telemetry
            if (Chbx_TwCommand.IsChecked == null) return;
            Settings.TwSrCommand = (bool)Chbx_TwCommand.IsChecked;
        }

        private void NudMaxReq_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            // Sets max requests per user value
            if (NudMaxReq.Value != null) Settings.TwSrMaxReq = (int)NudMaxReq.Value;
        }

        private void NudCooldown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            // Sets command cooldown
            if (NudCooldown.Value != null) Settings.TwSrCooldown = (int)NudCooldown.Value;
        }

        private void Chbx_TwAutoconnect_Checked(object sender, RoutedEventArgs e)
        {
            // Sets wether to autoconnect or not
            if (Chbx_TwAutoconnect.IsChecked != null) Settings.TwAutoConnect = (bool)Chbx_TwAutoconnect.IsChecked;
        }

        private void Chbx_MessageLogging_Checked(object sender, RoutedEventArgs e)
        {
            // Sets message loggint enabled or not
            if (Chbx_MessageLogging.IsChecked != null)
                Settings.MsgLoggingEnabled = (bool)Chbx_MessageLogging.IsChecked;
        }

        private void txtbx_twUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Sets the twitch acc
            Settings.TwAcc = txtbx_twUser.Text;
        }

        private void txtbx_twOAuth_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Sets the twitch oauth token
            Settings.TwOAuth = txtbx_twOAuth.Password;
        }

        private void txtbx_twChannel_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Sets the twitch channel
            Settings.TwChannel = txtbx_twChannel.Text;
        }

        private void Chbx_AutoClear_Checked(object sender, RoutedEventArgs e)
        {
            // Sets wether to clear the queue on startup or not
            if (Chbx_AutoClear.IsChecked != null) Settings.AutoClearQueue = (bool)Chbx_AutoClear.IsChecked;
        }

        private void MenuBtnReq_Click(object sender, RoutedEventArgs e)
        {
            // appends text
            AppendText(TxtbxOutputformat, "{{requested by {req}}}");
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cbx_Language.SelectedIndex)
            {
                case 0:
                    // English
                    System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
                    Settings.Language = "en";
                    break;
                case 1:
                    // German
                    System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("de-DE");
                    Settings.Language = "de-DE";
                    break;
            }

            Process.Start(System.Windows.Application.ResourceAssembly.Location);
            System.Windows.Application.Current.Shutdown();
        }

        private void btn_Botresponse_Click(object sender, RoutedEventArgs e)
        {
            Window_Botresponse wBr = new Window_Botresponse();
            wBr.Show();
        }

        private void nud_Spaces_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (nud_Spaces.Value != null) Settings.SpaceCount = (int)nud_Spaces.Value;
        }

        private void ChbxSpaces_Checked(object sender, RoutedEventArgs e)
        {
            if (ChbxSpaces.IsChecked != null) Settings.AppendSpaces = (bool)ChbxSpaces.IsChecked;
        }

        private void Tglsw_Spotify_IsCheckedChanged(object sender, EventArgs e)
        {
            if (Tglsw_Spotify.IsChecked != null) Settings.UseOwnApp = (bool)Tglsw_Spotify.IsChecked;
            btn_save.Visibility = Visibility.Visible;
            lbl_savingRestart.Visibility = Visibility.Visible;
        }

        private void tb_ClientID_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.ClientId = tb_ClientID.Text;
        }

        private void tb_ClientSecret_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Settings.ClientSecret = tb_ClientSecret.Password;
        }

        private async void btn_save_Click(object sender, RoutedEventArgs e)
        {
            MessageDialogResult msgResult = await this.ShowMessageAsync("Information", "The restart is only necessary if you switched the API clients.\n\nYou DO NOT have to do this when linking your account!", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "restart", NegativeButtonText = "cancel"});
            if (msgResult == MessageDialogResult.Affirmative)
            {
                Settings.AccessToken = "";
                Settings.RefreshToken = "";

                Process.Start(System.Windows.Application.ResourceAssembly.Location);
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void btn_OwnAppHelp_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://songify.rocks/faq.html#appid");
        }

        private void NudMaxlength_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (NudMaxlength.Value != null) Settings.MaxSongLength = (int)NudMaxlength.Value;
        }

        private async void Btn_ResetConfig_Click(object sender, RoutedEventArgs e)
        {
            MessageDialogResult msgResult = await this.ShowMessageAsync("Warning", "Are you sure you want to reset all settings?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });
            if (msgResult == MessageDialogResult.Affirmative)
            {
                File.Delete(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + "/config.xml");
                Properties.Settings.Default.Reset();
                Process.Start(System.Windows.Application.ResourceAssembly.Location);
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}