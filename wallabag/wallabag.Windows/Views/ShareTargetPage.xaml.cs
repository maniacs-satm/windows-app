using wallabag.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Freigabezielvertrag" ist unter http://go.microsoft.com/fwlink/?LinkId=234241 dokumentiert.

namespace wallabag.Views
{
    /// <summary>
    /// Über diese Seite können andere Anwendungen Inhalte durch diese Anwendung freigeben.
    /// </summary>
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
            this.DefaultViewModel["Image"] = thumbnailImage;
            this.DefaultViewModel["Sharing"] = false;
            this.DefaultViewModel["ShowImage"] = false;
            this.DefaultViewModel["Comment"] = String.Empty;
            this.DefaultViewModel["Placeholder"] = "Add a comment";
            this.DefaultViewModel["SupportsComment"] = true;
            Window.Current.Content = this;
            Window.Current.Activate();

            // Das Miniaturbild des freigegebenen Inhalts im Hintergrund aktualisieren
            if (shareProperties.Thumbnail != null)
            {
                var stream = await shareProperties.Thumbnail.OpenReadAsync();
                thumbnailImage.SetSource(stream);
                this.DefaultViewModel["ShowImage"] = true;
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn der Benutzer auf die Schaltfläche "Gemeinsam verwenden" klickt.
        /// </summary>
        /// <param name="sender">Instanz der Schaltfläche, die zum Initiieren der Freigabe verwendet wird.</param>
        /// <param name="e">Ereignisdaten, die beschreiben, wie auf die Schaltfläche geklickt wurde.</param>
        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            this.DefaultViewModel["Sharing"] = true;
            this._shareOperation.ReportStarted();

            // TODO: Aufgaben durchführen, die für Ihr Freigabeszenario geeignet sind, hierbei
            //       this._shareOperation.Data verwenden. Üblicherweise werden hierbei zusätzliche Informationen
            //       über individuelle Benutzeroberflächenelemente erfasst, die dieser Seite hinzugefügt wurden, z. B. 
            //       this.DefaultViewModel["Comment"]

            this._shareOperation.ReportCompleted();
        }
    }
}
