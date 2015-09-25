using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TabCarouselPage.Core;
using Xamarin.Forms;

namespace Sample
{
    public class App : Application
    {
        public App()
        {
            var tab1 = new SampleContentPage()
            {
                BackgroundColor = Color.FromHex("#27ae60"),
                Title = "Tab1"
            };
            var tab2 = new SampleContentPage()
            {
                BackgroundColor = Color.FromHex("#2980b9"),
                Title = "Tab2"
            };
            var tab3 = new SampleContentPage()
            {
                BackgroundColor = Color.FromHex("#e67e22"),
                Title = "Tab3"
            };
            var customTab = new TabCarouselPage.Core.TabCarouselPage(TabType.TitleWithIcon);
            customTab.Children.Add(tab1);
            customTab.Children.Add(tab2);
            customTab.Children.Add(tab3);
            MainPage = new NavigationPage(customTab) { Title = "Sample Custom Tab"};
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
