using System;

using Android.App;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Sample.Droid
{
    [Activity(Label = "Sample", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());

//            if ( ( int ) Build.VERSION.SdkInt >= 21 ) {
//                ActionBar.SetIcon ( new ColorDrawable(Resources.GetColor ( Android.Resource.Color.Transparent )) );
//            }
        }
    }
}

