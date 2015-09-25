/***************************************************************************************************************
	* TabCarouselPageRenderer.cs
	* 
	* Copyright (c) 2015, Shahman Teh Sharifuddin
	* All rights reserved.     
	* 
**************************************************************************************************************/

using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using TabCarouselPage.Droid.Renderers;
using TabCarouselPage.Droid.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(TabCarouselPage.Core.TabCarouselPage), typeof(TabCarouselPageRenderer))]
namespace TabCarouselPage.Droid.Renderers
{
	public class TabCarouselPageRenderer : CarouselPageRenderer
	{
		private ViewPager viewPager;
		private TabCarouselPageIndicator indicator;
		private TabCarouselPageAdapter adapter;
		private LinearLayout linearLayout;

		public static void Load () {}

		protected override void OnElementChanged(ElementChangedEventArgs<CarouselPage> e) {
			base.OnElementChanged(e);

			if ( viewPager != null ) {
				if ( viewPager.Adapter != null ) {
					viewPager.Adapter.Dispose ();
					viewPager.Adapter = null;
				}
				RemoveView ( viewPager );
				viewPager.SetOnPageChangeListener ( null );
				viewPager.Dispose ();
			}

			if ( indicator != null ) {
				RemoveView ( indicator );
				indicator.SetOnPageChangeListener ( null );
				indicator.Dispose ();
			}

			if ( linearLayout != null ) {
				RemoveView ( linearLayout );
				linearLayout.Dispose ();
			}

			RemoveAllViews();

			linearLayout = new LinearLayout(Forms.Context);
			linearLayout.SetGravity(GravityFlags.Center);
			linearLayout.Orientation = Orientation.Vertical;
			linearLayout.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
			linearLayout.Focusable = true;
			linearLayout.Clickable = true;
			AddView(linearLayout);

			viewPager = new ViewPager ( Context ) {
					OffscreenPageLimit = int.MaxValue
			};

			//the following is needed to draw indicator based on the exisitng theme.
			var contextThemeWrapper = new ContextThemeWrapper(Context, Resource.Style.Theme_PageIndicatorDefaults);
			indicator = new TabCarouselPageIndicator(contextThemeWrapper, Element);

			linearLayout.AddView(indicator, new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent));
			linearLayout.AddView(viewPager, new LinearLayout.LayoutParams(LayoutParams.MatchParent, 0, 1.0f));

		}

		/// <summary>
		/// Called when this view should assign a size and position to all of its children.
		/// <para> A Caveat of this is that always measure this based on the root most of the layout. in this case, linearlayout.
		/// For example, in the parent class, we did the measure on viewpager itself since it is the rootmost of the class</para>
		/// </summary>
		/// <param name="changed">if set to <c>true</c> This will be the new size or position for this view.</param>
		/// <param name="l">Left position, relative to parent.</param>
		/// <param name="t">Top position, relative to parent.</param>
		/// <param name="r">Right position, relative to parent.</param>
		/// <param name="b">Bottom position, relative to parent.</param>
		protected override void OnLayout(bool changed, int l, int t, int r, int b) {
			base.OnLayout(changed, l, t, r, b);
			if ( linearLayout == null ) {
				return;
			}
			linearLayout.Measure(MakeMeasureSpec(r - l, MeasureSpecMode.Exactly), MakeMeasureSpec(b - t, MeasureSpecMode.Exactly));
			linearLayout.Layout(0, 0, r - l, b - t);
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec) {
			viewPager.Measure(widthMeasureSpec, heightMeasureSpec);
			SetMeasuredDimension(viewPager.MeasuredWidth, viewPager.MeasuredHeight);
		}

		/// <summary>
		/// Based on internal class Xamarin.Forms.Platform.Android.MeasureSpecFactory
		/// </summary>
		/// <param name="size">The size.</param>
		/// <param name="mode">The mode.</param>
		/// <returns></returns>
		private static int MakeMeasureSpec(int size, MeasureSpecMode mode)
		{
			return (int)(size + mode);
		}

		/// <summary>
		/// Our Custom Carousel Page starts here. 
		/// First we have to get the working adapter. 
		/// Then remove the parent's view, and finally reapplied our own view.
		/// </summary>
		protected override void OnAttachedToWindow() {
			base.OnAttachedToWindow();

			adapter = new TabCarouselPageAdapter(viewPager, Element, Context);

			viewPager.SetOnPageChangeListener(adapter);
			indicator.SetOnPageChangeListener(adapter);

			viewPager.Adapter = adapter;

			UpdateCurrentItem();
			indicator.SetViewPager(viewPager);
		}

		/// <summary>
		/// based on both internal class Xamarin.Forms.Platform.Android.CarouselPageAdapter.UpdateCurrentItem() 
		/// and Xamarin.Forms.Platform.Android.CarouselPageRenderer.() . 
		/// the main purpose of this function is to update the viewpager based on the Carousel current page.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">CarouselPage has no children.</exception>
		private void UpdateCurrentItem () {

			int index = Element.Children.IndexOf ( Element.CurrentPage );
			if ( index < 0 || index >= Element.Children.Count ) {
				return;
			}
			viewPager.CurrentItem = index;
		}
	}

}