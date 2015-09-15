/***************************************************************************************************************
	* TabCarouselPageRenderer.cs
	* 
	* Copyright (c) 2015, Shahman Teh Sharifuddin
	* All rights reserved.     
	* 
**************************************************************************************************************/
using System;
using System.ComponentModel;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using TabCarouselPage.Core;
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
		private PagerAdapter adapter;
        private LinearLayout linearLayout;

		protected override void OnElementPropertyChanged ( object sender , PropertyChangedEventArgs e ) {
			base.OnElementPropertyChanged ( sender , e );
			if ( e.PropertyName != "CurrentPage" || Element.CurrentPage == null ) {
				return;
			}
			UpdateCurrentItem ();
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
		protected override void OnLayout ( bool changed , int l , int t , int r , int b ) {
			base.OnLayout ( changed , l , t , r , b );
			if ( linearLayout == null )
				return;
			linearLayout.Measure ( MakeMeasureSpec ( r - l , MeasureSpecMode.Exactly ) , MakeMeasureSpec ( b - t , MeasureSpecMode.Exactly ) );
			linearLayout.Layout ( 0 , 0 , r - l , b - t );
		}

		protected override void OnMeasure ( int widthMeasureSpec , int heightMeasureSpec ) {
			viewPager.Measure ( widthMeasureSpec , heightMeasureSpec );
			SetMeasuredDimension ( viewPager.MeasuredWidth , viewPager.MeasuredHeight );
		}

		/// <summary>
		/// Based on internal class Xamarin.Forms.Platform.Android.MeasureSpecFactory
		/// </summary>
		/// <param name="size">The size.</param>
		/// <param name="mode">The mode.</param>
		/// <returns></returns>
		private static int MakeMeasureSpec ( int size , MeasureSpecMode mode ) {
			return ( int ) ( size + mode );
		}

		/// <summary>
		/// Gets the PagerAdapter from the parent class's ViewPager. 
		/// Since the parent's ViewPager adapter (CarouselPageAdapter) is an internal class. We have to extract it from the exising ones 
		/// and reapplied it in our own viewPager.
		/// Best used on OnAttachedToWindow where the PagerAdapter is assigned to the parent's ViewPager
		/// </summary>
		/// <returns></returns>
		private PagerAdapter GetCurrentAdapter () {
			if ( ChildCount <= 0 ) {
				return null;
			}
			for ( var i = 0; i < ChildCount; i++ ) {
				var v = GetChildAt ( i );
				if ( !( v is ViewPager ) ) {
					continue;
				}
				return ( ( ViewPager ) v ).Adapter;
			}
			return null;
		}

		/// <summary>
		/// Our Custom Carousel Page starts here. 
		/// First we have to get the working adapter. 
		/// Then remove the parent's view, and finally reapplied our own view.
		/// </summary>
		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			adapter = GetCurrentAdapter ();

			if ( viewPager != null ) {
				RemoveView ( viewPager );
				viewPager.SetOnPageChangeListener ( null );
				viewPager.Dispose ();
			}

			if ( indicator != null ) {
				RemoveView ( indicator );
				indicator.SetOnPageChangeListener ( null );
				indicator.Dispose ();
			}

			RemoveAllViews();

			linearLayout = new LinearLayout ( Forms.Context );
			linearLayout.SetGravity(GravityFlags.Center);
			linearLayout.Orientation = Orientation.Vertical;
			linearLayout.LayoutParameters = new LayoutParams(LayoutParams.FillParent, LayoutParams.FillParent);
			linearLayout.Focusable = true;
			linearLayout.Clickable = true;
			AddView(linearLayout);
			
			viewPager = new ViewPager(Context) {
			        OffscreenPageLimit = int.MaxValue
			};
		    //the following is needed to draw indicator based on the exisitng theme.
			var contextThemeWrapper = new ContextThemeWrapper(Context, Resource.Style.Theme_PageIndicatorDefaults);
			indicator = new TabCarouselPageIndicator(contextThemeWrapper, Element);

			viewPager.SetOnPageChangeListener((ViewPager.IOnPageChangeListener)adapter);
			indicator.SetOnPageChangeListener((ViewPager.IOnPageChangeListener)adapter);

			viewPager.Adapter = adapter;

			UpdateCurrentItem ();
			indicator.SetViewPager ( viewPager );

			linearLayout.AddView(indicator, new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent));
			linearLayout.AddView(viewPager, new LinearLayout.LayoutParams(LayoutParams.FillParent, 0, 1.0f));
		}

		/// <summary>
		/// based on both internal class Xamarin.Forms.Platform.Android.CarouselPageAdapter.UpdateCurrentItem() 
		/// and Xamarin.Forms.Platform.Android.CarouselPageRenderer.() . 
		/// the main purpose of this function is to update the viewpager based on the Carousel current page.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">CarouselPage has no children.</exception>
		private void UpdateCurrentItem () {
			if ( Element.CurrentPage == null ) {
				throw new InvalidOperationException ( "CarouselPage has no children." );
			}

			int index = Element.Children.IndexOf ( Element.CurrentPage );
			if ( index < 0 || index >= Element.Children.Count ) {
				return;
			}
			viewPager.CurrentItem = index;
		}
	}
}