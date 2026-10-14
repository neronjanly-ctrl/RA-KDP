using RatCvsSimulation.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RatCvsSimulation.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AnimalSelectorPage : Page
    {
        public AnimalSelectorPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(480, 500);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(480, 500));
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        async Task ShowMainPage(AnimalArguments args)
        {
            var currentAV = ApplicationView.GetForCurrentView();
            var newAV = CoreApplication.CreateNewView();

            await newAV.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                async () =>
                {
                    var newWindow = Window.Current;
                    var newAppView = ApplicationView.GetForCurrentView();
                    //newAppView.Title = "New window";

                    // Create a AppShell to act as the navigation context and navigate to the first page
                    var page = new AppShell();

                    // Set the default language
                    page.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                    // Place our app shell in the current Window
                    Window.Current.Content = page;

                    if (page.AppFrame.Content == null)
                    {
                        // When the navigation stack isn't restored, navigate to the first page
                        // suppressing the initial entrance animation.
                        page.AppFrame.Navigate(typeof(MainPage), args);
                    }

                    newWindow.Content = page;
                    newWindow.Activate();

                    await ApplicationViewSwitcher.TryShowAsStandaloneAsync(
                        newAppView.Id,
                        ViewSizePreference.UseHalf,
                        currentAV.Id,
                        ViewSizePreference.UseHalf);

                    var di = DisplayInformation.GetForCurrentView();
                    var factor = di.RawPixelsPerViewPixel;
                    Size size = new Size(di.ScreenWidthInRawPixels / factor, di.ScreenHeightInRawPixels / factor);
                    size.Height -= 100;
                    size.Width -= 100;
                    newAppView.TryResizeView(size);
                });
        }

        private async void CatButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowMainPage(new AnimalArguments
            {
                PulseDelay = 32,
                Name = "Cat",
                MaxHR = 400,
                MeanHR = (170 + 120) / 2,
            });
        }

        private async void RabbitButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowMainPage(new AnimalArguments
            {
                PulseDelay = 40,
                Name = "Rabbit",
                MaxHR = 400,
                MeanHR = (150 + 120) / 2,
            });
        }

        private async void RatButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowMainPage(new AnimalArguments
            {
                PulseDelay = 16,
                Name = "Rat",
                MaxHR = 800,
                MeanHR = (310 + 240) / 2,
            });
        }

        private async void DogButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowMainPage(new AnimalArguments
            {
                PulseDelay = 24,
                Name = "Dog",
                MaxHR = 200,
                MeanHR = (120 + 70) / 2,
            });
        }
    }
}
