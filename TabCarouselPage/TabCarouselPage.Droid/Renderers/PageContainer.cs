/***************************************************************************************************************
	* PageContainer.cs
	* 
	* Copyright (c) 2015, Shahman Teh Sharifuddin
	* All rights reserved.     
	* 
**************************************************************************************************************/
using Android.Content;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace TabCarouselPage.Droid.Renderers
{
	/// <summary>
	/// Based on the internal class Xamarin.Forms.Platform.Android.PageContainer.
	/// </summary>
	public class PageContainer : ViewGroup
	{
		private readonly IVisualElementRenderer child;

		public PageContainer ( Context context , IVisualElementRenderer child ) : base ( context ) {
			AddView ( child.ViewGroup );
			this.child = child;
		}

		protected override void OnMeasure ( int widthMeasureSpec , int heightMeasureSpec ) {
			child.ViewGroup.Measure ( widthMeasureSpec , heightMeasureSpec );
			SetMeasuredDimension ( child.ViewGroup.MeasuredWidth , child.ViewGroup.MeasuredHeight );
		}

		protected override void OnLayout ( bool changed , int l , int t , int r , int b ) {
			//We need to layout the Element based on the container's position and size in order to get the proper rendering. 
			//This is the only way the page rendering to render's properly at the moment
			child.Element.Layout ( new Rectangle ( child.Element.X , child.Element.Y , child.Element.Width , Context.FromPixels ( b - t ) ) );
			child.UpdateLayout ();
		}
	}
}