using System;
using wallabag.Common;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace wallabag.Views
{
    public sealed partial class ShareTarget : Page
    {
        private ShareOperation shareOperation;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public ApplicationSettings AppSettings { get { return ApplicationSettings.Instance; } }

        public ShareTarget()
        {
            this.InitializeComponent();
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Wird aufgerufen, wenn eine andere Anwendung Inhalte durch diese Anwendung freigeben möchte.
        /// </summary>
        /// <param name="e">Aktivierungsdaten zum Koordinieren des Prozesses mit Windows.</param>
        public async void Activate(ShareTargetActivatedEventArgs e)
        {
            this.shareOperation = e.ShareOperation;

            var shareProperties = this.shareOperation.Data.Properties;
            this.DefaultViewModel["Title"] = shareProperties.Title; // Gets the title from the sender app.
            this.DefaultViewModel["Description"] = shareProperties.Description; // Gets the description from the sender app.
            this.DefaultViewModel["Sharing"] = false;
            this.DefaultViewModel["Url"] = await shareOperation.Data.GetWebLinkAsync(); // Because wallabag is saving links, the web link is the most important thing in this code block.
            Window.Current.Content = this;
            Window.Current.Activate();
        }

        /// <summary>
        /// Encodes the Url for the WebView to save the link in wallabag.
        /// </summary>
        private string finalUrl()
        {
            string wallabagUrl = AppSettings["wallabagUrl", string.Empty];
            string encodedUrl = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(this.DefaultViewModel["Url"].ToString()));
            return string.Format("{0}?action=add&url={1}", wallabagUrl, encodedUrl);
        }

        /// <summary>
        /// Wird aufgerufen, wenn der Benutzer auf die Schaltfläche "Gemeinsam verwenden" klickt.
        /// </summary>
        /// <param name="sender">Instanz der Schaltfläche, die zum Initiieren der Freigabe verwendet wird.</param>
        /// <param name="e">Ereignisdaten, die beschreiben, wie auf die Schaltfläche geklickt wurde.</param>
        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            this.DefaultViewModel["Sharing"] = true;
            this.webView.Navigate(new Uri(finalUrl()));

            // TODO: Aufgaben durchführen, die für Ihr Freigabeszenario geeignet sind, hierbei
            //       this._shareOperation.Data verwenden. Üblicherweise werden hierbei zusätzliche Informationen
            //       über individuelle Benutzeroberflächenelemente erfasst, die dieser Seite hinzugefügt wurden, z. B. 
            //       this.DefaultViewModel["Comment"]

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.shareOperation.ReportCompleted();
        }
    }
}
