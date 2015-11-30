/***************************************************************************************************************
	* TabCarouselBarDelegate.cs
	* 
	* Copyright (c) 2015, Shahman Teh Sharifuddin
	* All rights reserved.     
	* 
**************************************************************************************************************/
using System;
using TabCarouselPage.iOS.Renderers;
using UIKit;
using Xamarin.Forms.Platform.iOS;

namespace TabCarouselPage.iOS.UIKit
{
	internal sealed class TabCarouselBarDelegate : UITabBarDelegate
	{
		public TabCarouselPageRenderer PageRenderer { get; set; }

		public TabCarouselBarDelegate ( TabCarouselPageRenderer pageRenderer ) {
			if ( pageRenderer == null ) {
				throw new NullReferenceException ();
			}
			PageRenderer = pageRenderer;
		}

		public override void ItemSelected ( UITabBar tabbar , UITabBarItem item ) {
			var selectedIndex = Array.IndexOf ( tabbar.Items , item );
			PageRenderer.SelectedIndex = selectedIndex;
			PageRenderer.TabbedCarousel.CurrentPage = PageRenderer.TabbedCarousel.Children [ selectedIndex ];
			tabbar.BackgroundColor = PageRenderer.TabbedCarousel.Children [ selectedIndex ].BackgroundColor.ToUIColor ();
		}
	}
}