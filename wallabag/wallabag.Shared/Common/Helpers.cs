using Windows.ApplicationModel.Resources;

namespace wallabag.Common
{
    public static class Helpers
    {
        /// <summary>
        /// There are several languages. To access them from code-behind this way is required.
        /// </summary>
        /// <param name="resourceName">The name of the resource in the dictionary.</param>
        /// <returns>A string representing the translation for the current language.</returns>
        public static string LocalizedString(string resourceName)
        {
            return ResourceLoader.GetForCurrentView().GetString(resourceName);
        }
    }
}
