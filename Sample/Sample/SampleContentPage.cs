using Xamarin.Forms;

namespace Sample
{
    public class SampleContentPage : ContentPage
    {
        public Label PageLabel { get; set; }

        public SampleContentPage () {

            PageLabel = new Label {
                    XAlign = TextAlignment.Center ,
                    Text = Title ,
            };
            var stackLayout = new StackLayout {
                    VerticalOptions = LayoutOptions.Center ,
            };
            stackLayout.Children.Add ( PageLabel );
            Content = stackLayout;
        }

        protected override void OnAppearing () {
            base.OnAppearing ();
            PageLabel.Text = Title;
        }
    }
}