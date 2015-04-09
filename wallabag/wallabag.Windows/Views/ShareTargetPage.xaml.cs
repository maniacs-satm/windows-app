using System;
using wallabag.Common;
using wallabag.DataModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace wallabag.Views
{
    public sealed partial class ShareTargetPage : Page
    {
        /// <summary>
        /// Stellt einen Kanal zum Kommunizieren mit Windows über den Freigabevorgang bereit.
        /// </summary>
        private Windows.ApplicationModel.DataTransfer.ShareTarget.ShareOperation _shareOperation;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// Dies kann in ein stark typisiertes Anzeigemodell geändert werden.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public ShareTargetPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Wird aufgerufen, wenn eine andere Anwendung Inhalte durch diese Anwendung freigeben möchte.
        /// </summary>
        /// <param name="e">Aktivierungsdaten zum Koordinieren des Prozesses mit Windows.</param>
        public async void Activate(ShareTargetActivatedEventArgs e)
        {
            this._shareOperation = e.ShareOperation;

            // Metadaten über den freigegebenen Inhalt durch das Anzeigemodell kommunizieren
            var shareProperties = this._shareOperation.Data.Properties;
            var thumbnailImage = new BitmapImage();
            this.DefaultViewModel["Title"] = shareProperties.Title;
            this.DefaultViewModel["Description"] = shareProperties.Description;
            this.DefaultViewModel["Sharing"] = false;
            this.DefaultViewModel["Tags"] = string.Empty;
            this.defaultViewModel["Url"] = await this._shareOperation.Data.GetWebLinkAsync();
            Window.Current.Content = this;
            Window.Current.Activate();
        }

        /// <summary>
        /// Wird aufgerufen, wenn der Benutzer auf die Schaltfläche "Gemeinsam verwenden" klickt.
        /// </summary>
        /// <param name="sender">Instanz der Schaltfläche, die zum Initiieren der Freigabe verwendet wird.</param>
        /// <param name="e">Ereignisdaten, die beschreiben, wie auf die Schaltfläche geklickt wurde.</param>
        private async void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            this.DefaultViewModel["Sharing"] = true;
            this._shareOperation.ReportStarted();
            this._shareOperation.ReportSubmittedBackgroundTask();

            bool success = await DataSource.AddItem(((Uri)this.defaultViewModel["Url"]).ToString(), this.defaultViewModel["Tags"].ToString());
            if (!success)
                this._shareOperation.ReportError("Error during article save.");

            this._shareOperation.ReportCompleted();
        }
    }
}
