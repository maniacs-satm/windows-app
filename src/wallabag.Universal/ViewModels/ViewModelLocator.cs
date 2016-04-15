using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using wallabag.Data.Interfaces;
using wallabag.Data.Services;

namespace wallabag.ViewModels
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
                SimpleIoc.Default.Register<IDataService, TestDataService>();
            else
                SimpleIoc.Default.Register<IDataService, TestDataService>();


            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<AddItemPageViewModel>();
            SimpleIoc.Default.Register<FirstStartPageViewModel>();
            SimpleIoc.Default.Register<SettingsPageViewModel>();
            SimpleIoc.Default.Register<SingleItemPageViewModel>();
        }

        public static IDataService CurrentDataService => SimpleIoc.Default.GetInstance<IDataService>();
        public MainViewModel Main => SimpleIoc.Default.GetInstance<MainViewModel>();
        public AddItemPageViewModel AddItem => SimpleIoc.Default.GetInstance<AddItemPageViewModel>();
        public FirstStartPageViewModel FirstStart => SimpleIoc.Default.GetInstance<FirstStartPageViewModel>();
        public SettingsPageViewModel Settings => SimpleIoc.Default.GetInstance<SettingsPageViewModel>();
        public SingleItemPageViewModel SingleItem => SimpleIoc.Default.GetInstance<SingleItemPageViewModel>();

    }
}
