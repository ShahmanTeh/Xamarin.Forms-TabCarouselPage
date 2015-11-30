/***************************************************************************************************************
	* UIPageContainer.cs
	* 
	* Copyright (c) 2015, Shahman Teh Sharifuddin
	* All rights reserved.     
	* 
**************************************************************************************************************/
using System;
using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace TabCarouselPage.iOS.UIKit
{
	internal sealed class UIPageContainer : UIView
	{
		private VisualElement Element { get; set; }

		public UIPageContainer ( VisualElement element ) {
			Element = element;
			AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
		}

		public override void LayoutSubviews () {
			base.LayoutSubviews ();
			if ( Subviews.Length == 0 ) {
				return;
			}
			if ( !Element.Bounds.Equals ( Frame.ToRectangle () ) ) {
				Console.WriteLine ( ">>>> Perform Re-layout" );
				Element.Layout ( Frame.ToRectangle () );
			}
			//Subviews [ 0 ].Frame = new CGRect ( 0 , 0 , ( float ) Element.Width , Element.Height);
			Subviews [ 0 ].Frame = new CGRect ( 0 , 0 , Frame.Width , Frame.Height );
		}
	}
}