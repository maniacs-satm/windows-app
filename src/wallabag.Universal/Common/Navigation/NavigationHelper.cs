using System;
using System.Collections.Generic;
using wallabag.Common.MVVM;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Common.Navigation
{
    /// <summary>
    /// NavigationHelper bietet Unterstützung bei der Navigation zwischen Seiten.  Es werden Befehle zum 
    /// rückwärts und vorwärts navigieren sowie Register für Standardmaus und -tastatur 
    /// Verknüpfungen, um in Windows nach vorne oder zurück zu wechseln sowie Hardwaretaste im
    /// Windows Phone.  Darüber hinaus wird SuspensionManger zur Verwaltung der Prozesslebensdauer
    /// und der Zustandsverwaltung beim Navigieren zwischen Seiten integriert.
    /// </summary>
    /// <example>
    /// Zur Verwendung von NavigationHelper diesen Schritten folgen, oder
    /// Starten Sie mit der Elementvorlage "Standardseite" oder einer beliebigen anderen Elementvorlage vom Typ "Seite" außer "Leere Seite".
    /// 
    /// 1) Eine Instanz von NavigationHelper erstellen an einem Ort wie 
    ///     Konstruktor für die Seite und Registrierung eines Rückrufs für LoadState und 
    ///     SaveState-Ereignisse.
    /// <code>
    ///     public MyPage()
    ///     {
    ///         this.InitializeComponent();
    ///         var navigationHelper = new NavigationHelper(this);
    ///         this.navigationHelper.LoadState += navigationHelper_LoadState;
    ///         this.navigationHelper.SaveState += navigationHelper_SaveState;
    ///     }
    ///     
    ///     private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
    ///     { }
    ///     private async void navigationHelper_SaveState(object sender, LoadStateEventArgs e)
    ///     { }
    /// </code>
    /// 
    /// 2) Die Seite für den Aufruf an NavigationHelper registrieren, wenn die Seite 
    ///     bei der Navigation, indem <see cref="Windows.UI.Xaml.Controls.Page.OnNavigatedTo"/> überschrieben wird 
    ///     und <see cref="Windows.UI.Xaml.Controls.Page.OnNavigatedFrom"/>-Ereignisse.
    /// <code>
    ///     protected override void OnNavigatedTo(NavigationEventArgs e)
    ///     {
    ///         navigationHelper.OnNavigatedTo(e);
    ///     }
    ///     
    ///     protected override void OnNavigatedFrom(NavigationEventArgs e)
    ///     {
    ///         navigationHelper.OnNavigatedFrom(e);
    ///     }
    /// </code>
    /// </example>
    [Windows.Foundation.Metadata.WebHostHidden]
    public class NavigationHelper : DependencyObject
    {
        private Page Page { get; set; }
        private Frame Frame { get { return Page.Frame; } }

        public NavigationHelper(Page page)
        {
            Page = page;

            // Zwei Änderungen vornehmen, wenn diese Seite Teil der visuellen Struktur ist:
            // 1) Den Ansichtszustand der Anwendung dem visuellen Zustand für die Seite zuordnen
            // 2) Behandeln von Hardwarenavigationsanforderungen
            Page.Loaded += (sender, e) =>
            {
#if WINDOWS_PHONE_APP
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#else
                // Tastatur- und Mausnavigation trifft nur zu, wenn das gesamte Fenster ausgefüllt wird.
                if (Page.ActualHeight == Window.Current.Bounds.Height &&
                    Page.ActualWidth == Window.Current.Bounds.Width)
                {
                    // Direkt am Fenster lauschen, sodass kein Fokus erforderlich ist
                    Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated +=
                        CoreDispatcher_AcceleratorKeyActivated;
                    Window.Current.CoreWindow.PointerPressed +=
                        CoreWindow_PointerPressed;
                }
#endif
            };

            // Dieselben Änderungen rückgängig machen, wenn die Seite nicht mehr sichtbar ist
            Page.Unloaded += (sender, e) =>
            {
#if WINDOWS_PHONE_APP
                Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
#else
                Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -=
                    CoreDispatcher_AcceleratorKeyActivated;
                Window.Current.CoreWindow.PointerPressed -=
                    CoreWindow_PointerPressed;
#endif
            };
        }

        #region Navigationsunterstützung

        RelayCommand _goBackCommand;
        RelayCommand _goForwardCommand;

        public RelayCommand GoBackCommand
        {
            get
            {
                if (_goBackCommand == null)
                {
                    _goBackCommand = new RelayCommand(
                        () => GoBack(),
                        () => CanGoBack());
                }
                return _goBackCommand;
            }
            set
            {
                _goBackCommand = value;
            }
        }
        public RelayCommand GoForwardCommand
        {
            get
            {
                if (_goForwardCommand == null)
                {
                    _goForwardCommand = new RelayCommand(
                        () => GoForward(),
                        () => CanGoForward());
                }
                return _goForwardCommand;
            }
        }
        
        public virtual bool CanGoBack()
        {
            return Frame != null && Frame.CanGoBack;
        }
        public virtual bool CanGoForward()
        {
            return Frame != null && Frame.CanGoForward;
        }

        public virtual void GoBack()
        {
            if (Frame != null && Frame.CanGoBack) Frame.GoBack();
        }
        public virtual void GoForward()
        {
            if (Frame != null && Frame.CanGoForward) Frame.GoForward();
        }

#if WINDOWS_PHONE_APP
        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (this.GoBackCommand.CanExecute(null))
            {
                e.Handled = true;
                this.GoBackCommand.Execute(null);
            }
        }
#else
        /// <summary>
        /// Wird bei jeder Tastatureingabe aufgerufen, einschließlich Systemtasten wie ALT-Tastenkombinationen, wenn
        /// diese Seite aktiv ist und das gesamte Fenster ausfüllt.  Wird zum Erkennen von Tastaturnavigation verwendet
        /// zwischen Seiten, auch wenn die Seite selbst nicht den Fokus hat.
        /// </summary>
        /// <param name="sender">Instanz, von der das Ereignis ausgelöst wurde.</param>
        /// <param name="e">Ereignisdaten, die die Bedingungen beschreiben, die zu dem Ereignis geführt haben.</param>
        private void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender,
            AcceleratorKeyEventArgs e)
        {
            var virtualKey = e.VirtualKey;

            // Weitere Untersuchungen nur durchführen, wenn die Taste "Nach links", "Nach rechts" oder die dezidierten Tasten "Zurück" oder "Weiter"
            // gedrückt werden
            if ((e.EventType == CoreAcceleratorKeyEventType.SystemKeyDown ||
                e.EventType == CoreAcceleratorKeyEventType.KeyDown) &&
                (virtualKey == VirtualKey.Left || virtualKey == VirtualKey.Right ||
                (int)virtualKey == 166 || (int)virtualKey == 167))
            {
                var coreWindow = Window.Current.CoreWindow;
                var downState = CoreVirtualKeyStates.Down;
                bool menuKey = (coreWindow.GetKeyState(VirtualKey.Menu) & downState) == downState;
                bool controlKey = (coreWindow.GetKeyState(VirtualKey.Control) & downState) == downState;
                bool shiftKey = (coreWindow.GetKeyState(VirtualKey.Shift) & downState) == downState;
                bool noModifiers = !menuKey && !controlKey && !shiftKey;
                bool onlyAlt = menuKey && !controlKey && !shiftKey;

                if (((int)virtualKey == 166 && noModifiers) ||
                    (virtualKey == VirtualKey.Left && onlyAlt))
                {
                    // Wenn die Taste "Zurück" oder ALT+NACH-LINKS-TASTE gedrückt wird, zurück navigieren
                    e.Handled = true;
                    GoBackCommand.Execute(null);
                }
                else if (((int)virtualKey == 167 && noModifiers) ||
                    (virtualKey == VirtualKey.Right && onlyAlt))
                {
                    // Wenn die Taste "Weiter" oder ALT+NACH-RECHTS-TASTE gedrückt wird, vorwärts navigieren
                    e.Handled = true;
                    GoForwardCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Wird bei jedem Mausklick, jeder Touchscreenberührung oder einer äquivalenten Interaktion aufgerufen, wenn diese
        /// Seite aktiv ist und das gesamte Fenster ausfüllt.  Wird zum Erkennen von "Weiter"- und "Zurück"-Maustastenklicks
        /// im Browserstil verwendet, um zwischen Seiten zu navigieren.
        /// </summary>
        /// <param name="sender">Instanz, von der das Ereignis ausgelöst wurde.</param>
        /// <param name="e">Ereignisdaten, die die Bedingungen beschreiben, die zu dem Ereignis geführt haben.</param>
        private void CoreWindow_PointerPressed(CoreWindow sender,
            PointerEventArgs e)
        {
            var properties = e.CurrentPoint.Properties;

            // Tastenkombinationen mit der linken, rechten und mittleren Taste ignorieren
            if (properties.IsLeftButtonPressed || properties.IsRightButtonPressed ||
                properties.IsMiddleButtonPressed) return;

            // Wenn "Zurück" oder "Vorwärts" gedrückt wird (jedoch nicht gleichzeitig), entsprechend navigieren
            bool backPressed = properties.IsXButton1Pressed;
            bool forwardPressed = properties.IsXButton2Pressed;
            if (backPressed ^ forwardPressed)
            {
                e.Handled = true;
                if (backPressed) GoBackCommand.Execute(null);
                if (forwardPressed) GoForwardCommand.Execute(null);
            }
        }
#endif

        #endregion

        #region Verwaltung der Prozesslebensdauer

        private String _pageKey;
        public event LoadStateEventHandler LoadState;
        public event SaveStateEventHandler SaveState;

        public void OnNavigatedTo(NavigationEventArgs e)
        {
            var frameState = SuspensionManager.SessionStateForFrame(Frame);
            _pageKey = "Page-" + Frame.BackStackDepth;

            if (e.NavigationMode == NavigationMode.New)
            {
                // Bestehenden Zustand für die Vorwärtsnavigation löschen, wenn dem Navigationsstapel eine neue
                // Seite hinzugefügt wird
                var nextPageKey = _pageKey;
                int nextPageIndex = Frame.BackStackDepth;
                while (frameState.Remove(nextPageKey))
                {
                    nextPageIndex++;
                    nextPageKey = "Page-" + nextPageIndex;
                }

                // Den Navigationsparameter an die neue Seite übergeben
                if (LoadState != null)
                {
                    LoadState(this, new LoadStateEventArgs(e.Parameter, null));
                }
            }
            else
            {
                // Den Navigationsparameter und den beibehaltenen Seitenzustand an die Seite übergeben,
                // dabei die gleiche Strategie verwenden wie zum Laden des angehaltenen Zustands und zum erneuten Erstellen von im Cache verworfenen
                // Seiten
                if (LoadState != null)
                {
                    LoadState(this, new LoadStateEventArgs(e.Parameter, (Dictionary<String, Object>)frameState[_pageKey]));
                }
            }
        }

        public void OnNavigatedFrom(NavigationEventArgs e)
        {
            var frameState = SuspensionManager.SessionStateForFrame(Frame);
            var pageState = new Dictionary<String, Object>();
            if (SaveState != null)
            {
                SaveState(this, new SaveStateEventArgs(pageState));
            }
            frameState[_pageKey] = pageState;
        }

        #endregion
    }

    /// <summary>
    /// Repräsentiert die Methode, mit der das <see cref="NavigationHelper.LoadState"/>-Ereignis behandelt wird
    /// </summary>
    public delegate void LoadStateEventHandler(object sender, LoadStateEventArgs e);
    
    /// <summary>
    /// Repräsentiert die Methode, mit der das <see cref="NavigationHelper.SaveState"/>-Ereignis behandelt wird
    /// </summary>
    public delegate void SaveStateEventHandler(object sender, SaveStateEventArgs e);

    /// <summary>
    /// Klasse, die zum Speichern der erforderlichen Ereignisdaten verwendet wird, wenn eine Seite versucht, den Zustand zu laden.
    /// </summary>
    public class LoadStateEventArgs : EventArgs
    {
        public Object NavigationParameter { get; private set; }
        public Dictionary<string, Object> PageState { get; private set; }

        public LoadStateEventArgs(Object navigationParameter, Dictionary<string, Object> pageState)
            : base()
        {
            NavigationParameter = navigationParameter;
            PageState = pageState;
        }
    }
    
    /// <summary>
    /// Klasse, die zum Speichern der erforderlichen Ereignisdaten verwendet wird, wenn eine Seite versucht, den Zustand zu speichern.
    /// </summary>
    public class SaveStateEventArgs : EventArgs
    {
        public Dictionary<string, Object> PageState { get; private set; }

        public SaveStateEventArgs(Dictionary<string, Object> pageState)
            : base()
        {
            PageState = pageState;
        }
    }
}
