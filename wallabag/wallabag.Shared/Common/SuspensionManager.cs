using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using wallabag.ViewModel;

namespace wallabag.Common
{
    internal sealed class SuspensionManager
    {
        private static Dictionary<string, object> _sessionState = new Dictionary<string, object>();
        private static List<Type> _knownTypes = new List<Type>() {typeof(MainViewModel), 
            typeof(ItemPageViewModel),
            typeof(SettingsViewModel)};
        private const string sessionStateFilename = "_sessionState.xml";

        public static Dictionary<string, object> SessionState
        {
            get { return _sessionState; }
        }

        public static List<Type> KnownTypes
        {
            get { return _knownTypes; }
        }

        public static async Task SaveAsync()
        {
            try
            {
                // Navigationszustand für alle registrierten Rahmen speichern
                foreach (var weakFrameReference in _registeredFrames)
                {
                    Frame frame;
                    if (weakFrameReference.TryGetTarget(out frame))
                    {
                        SaveFrameNavigationState(frame);
                    }
                }

                // Sitzungszustand synchron serialisieren, um einen asynchronen Zugriff auf den freigegebenen Zustand zu bieten
                MemoryStream sessionData = new MemoryStream();
                DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<string, object>), _knownTypes);
                serializer.WriteObject(sessionData, _sessionState);

                // Einen Ausgabedatenstrom für die SessionState-Datei abrufen und den Zustand asynchron schreiben
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(sessionStateFilename, CreationCollisionOption.ReplaceExisting);
                using (Stream fileStream = await file.OpenStreamForWriteAsync())
                {
                    sessionData.Seek(0, SeekOrigin.Begin);
                    await sessionData.CopyToAsync(fileStream);
                }
            }
            catch (Exception e)
            {
                throw new SuspensionManagerException(e);
            }
        }

        public static async Task RestoreAsync(String sessionBaseKey = null)
        {
            _sessionState = new Dictionary<String, Object>();

            try
            {
                // Eingabedatenstrom für die SessionState-Datei abrufen
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(sessionStateFilename);
                using (IInputStream inStream = await file.OpenSequentialReadAsync())
                {
                    // Sitzungszustand deserialisieren
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<string, object>), _knownTypes);
                    _sessionState = (Dictionary<string, object>)serializer.ReadObject(inStream.AsStreamForRead());
                }

                // Alle registrierten Rahmen auf den gespeicherten Zustand wiederherstellen
                foreach (var weakFrameReference in _registeredFrames)
                {
                    Frame frame;
                    if (weakFrameReference.TryGetTarget(out frame) && (string)frame.GetValue(FrameSessionBaseKeyProperty) == sessionBaseKey)
                    {
                        frame.ClearValue(FrameSessionStateProperty);
                        RestoreFrameNavigationState(frame);
                    }
                }
            }
            catch (Exception e)
            {
                throw new SuspensionManagerException(e);
            }
        }

        private static DependencyProperty FrameSessionStateKeyProperty =
            DependencyProperty.RegisterAttached("_FrameSessionStateKey", typeof(String), typeof(SuspensionManager), null);
        private static DependencyProperty FrameSessionBaseKeyProperty =
            DependencyProperty.RegisterAttached("_FrameSessionBaseKeyParams", typeof(String), typeof(SuspensionManager), null);
        private static DependencyProperty FrameSessionStateProperty =
            DependencyProperty.RegisterAttached("_FrameSessionState", typeof(Dictionary<String, Object>), typeof(SuspensionManager), null);
        private static List<WeakReference<Frame>> _registeredFrames = new List<WeakReference<Frame>>();

        public static void RegisterFrame(Frame frame, String sessionStateKey, String sessionBaseKey = null)
        {
            if (frame.GetValue(FrameSessionStateKeyProperty) != null)
            {
                throw new InvalidOperationException("Frames can only be registered to one session state key");
            }

            if (frame.GetValue(FrameSessionStateProperty) != null)
            {
                throw new InvalidOperationException("Frames must be either be registered before accessing frame session state, or not registered at all");
            }

            if (!string.IsNullOrEmpty(sessionBaseKey))
            {
                frame.SetValue(FrameSessionBaseKeyProperty, sessionBaseKey);
                sessionStateKey = sessionBaseKey + "_" + sessionStateKey;
            }

            // Eine Abhängigkeitseigenschaft verwenden, um den Sitzungsschlüssel mit einem Rahmen zu verknüpfen, und eine Liste von Rahmen speichern, deren
            // Navigationszustand verwaltet werden soll
            frame.SetValue(FrameSessionStateKeyProperty, sessionStateKey);
            _registeredFrames.Add(new WeakReference<Frame>(frame));

            // Überprüfen, ob der Navigationszustand wiederhergestellt werden kann
            RestoreFrameNavigationState(frame);
        }

        public static void UnregisterFrame(Frame frame)
        {
            // Sitzungszustand und Rahmen aus der Liste der Rahmen entfernen, deren Navigationszustand
            // gespeichert wird (gemeinsam mit allen schwachen Verweisen, die nicht mehr erreichbar sind)
            SessionState.Remove((String)frame.GetValue(FrameSessionStateKeyProperty));
            _registeredFrames.RemoveAll((weakFrameReference) =>
            {
                Frame testFrame;
                return !weakFrameReference.TryGetTarget(out testFrame) || testFrame == frame;
            });
        }

        public static Dictionary<String, Object> SessionStateForFrame(Frame frame)
        {
            var frameState = (Dictionary<String, Object>)frame.GetValue(FrameSessionStateProperty);

            if (frameState == null)
            {
                var frameSessionKey = (String)frame.GetValue(FrameSessionStateKeyProperty);
                if (frameSessionKey != null)
                {
                    // Registrierte Rahmen geben den entsprechenden Sitzungszustand wieder.
                    if (!_sessionState.ContainsKey(frameSessionKey))
                    {
                        _sessionState[frameSessionKey] = new Dictionary<String, Object>();
                    }
                    frameState = (Dictionary<String, Object>)_sessionState[frameSessionKey];
                }
                else
                {
                    // Rahmen, die nicht registriert sind, verfügen über einen vorübergehenden Zustand
                    frameState = new Dictionary<String, Object>();
                }
                frame.SetValue(FrameSessionStateProperty, frameState);
            }
            return frameState;
        }

        private static void RestoreFrameNavigationState(Frame frame)
        {
            var frameState = SessionStateForFrame(frame);
            if (frameState.ContainsKey("Navigation"))
            {
                frame.SetNavigationState((String)frameState["Navigation"]);
            }
        }

        private static void SaveFrameNavigationState(Frame frame)
        {
            var frameState = SessionStateForFrame(frame);
            frameState["Navigation"] = frame.GetNavigationState();
        }
    }
    public class SuspensionManagerException : Exception
    {
        public SuspensionManagerException()
        {
        }

        public SuspensionManagerException(Exception e)
            : base("SuspensionManager failed", e)
        {

        }
    }
}
