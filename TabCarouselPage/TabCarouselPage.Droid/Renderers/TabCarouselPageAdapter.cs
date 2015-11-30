/***************************************************************************************************************
	* TabCarouselPageAdapter.cs
	* 
	* Copyright (c) 2015, Shahman Teh Sharifuddin
	* All rights reserved.     
	* 
**************************************************************************************************************/

using System;
using System.Collections.Specialized;
using Android.Content;
using Android.Support.V4.View;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace TabCarouselPage.Droid.Renderers
{
	public class TabCarouselPageAdapter : PagerAdapter , ViewPager.IOnPageChangeListener
	{
		private bool ignoreAndroidSelection;
		private readonly ViewPager pager;
		private CarouselPage page;
		private readonly Context context;

		public override int Count {
			get { return page.Children.Count; }
		}

		public TabCarouselPageAdapter ( ViewPager pager , CarouselPage page , Context context ) {
			this.pager = pager;
			this.page = page;
			this.context = context;
			page.PagesChanged += OnPagesChanged;
		}

		protected override void Dispose ( bool disposing ) {
			if ( disposing && page != null ) {
				foreach ( var element in page.Children ) {
					IVisualElementRenderer renderer = GetRenderer ( element );
					if ( renderer != null ) {
						renderer.ViewGroup.RemoveFromParent ();
						renderer.Dispose ();
						SetRenderer ( element , null );
					}
				}
				page.PagesChanged -= OnPagesChanged;
				page = null;
			}
			base.Dispose ( disposing );
		}

		public void UpdateCurrentItem () {
			if ( page.CurrentPage == null ) {
				throw new InvalidOperationException ( "CarouselPage has no children." );
			}
			var index = page.Children.IndexOf ( page.CurrentPage );
			if ( index < 0 || index >= page.Children.Count ) {
				return;
			}
			pager.CurrentItem = index;
		}

		private void OnPagesChanged ( object sender , NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs ) {
			ignoreAndroidSelection = true;
			NotifyDataSetChanged ();
			ignoreAndroidSelection = false;
			if ( page.CurrentPage == null ) {
				return;
			}
			UpdateCurrentItem ();
		}

		public override int GetItemPosition ( Java.Lang.Object item ) {
			ObjectJavaBox <Tuple <ViewGroup , Page , int>> objectJavaBox = ( ObjectJavaBox <Tuple <ViewGroup , Page , int>> ) item;
			Element parent = objectJavaBox.Instance.Item2.Parent;
			if ( parent == null ) {
				return -2;
			}
			var num = ( ( CarouselPage ) parent ).Children.IndexOf ( ( ContentPage ) objectJavaBox.Instance.Item2 );
			if ( num == 1 ) {
				return -2;
			}
			if ( num == objectJavaBox.Instance.Item3 ) {
				return -1;
			}
			objectJavaBox.Instance = new Tuple <ViewGroup , Page , int> ( objectJavaBox.Instance.Item1 , objectJavaBox.Instance.Item2 , num );
			return num;
		}

		public override Java.Lang.Object InstantiateItem ( ViewGroup container , int position ) {
			ContentPage contentPage = page.Children [ position ];
			if ( GetRenderer ( contentPage ) == null ) {
				SetRenderer ( contentPage , RendererFactory.GetRenderer ( contentPage ) );
			}
			IVisualElementRenderer renderer = GetRenderer ( contentPage );
			renderer.ViewGroup.RemoveFromParent ();
			PageContainer pageContainer = new PageContainer ( context , renderer );
			container.AddView ( pageContainer );
			return
					new ObjectJavaBox <Tuple <ViewGroup , Page , int>> (
							new Tuple <ViewGroup , Page , int> ( pageContainer , contentPage , position ) );
		}

		public override void DestroyItem ( Android.Views.View container , int position , Java.Lang.Object item ) {
			ObjectJavaBox <Tuple <ViewGroup , Page , int>> objectJavaBox = ( ObjectJavaBox <Tuple <ViewGroup , Page , int>> ) item;
			GetRenderer ( objectJavaBox.Instance.Item2 ).ViewGroup.RemoveFromParent ();
			objectJavaBox.Instance.Item1.RemoveFromParent ();
		}

		public override bool IsViewFromObject ( Android.Views.View view , Java.Lang.Object item ) {
			ViewGroup viewGroup = ( ( ObjectJavaBox <Tuple <ViewGroup , Page , int>> ) item ).Instance.Item1;
			return view == viewGroup;
		}

		public void OnPageScrollStateChanged ( int state ) {}

		public void OnPageScrolled ( int position , float positionOffset , int positionOffsetPixels ) {}

		public void OnPageSelected ( int position ) {
			if ( ignoreAndroidSelection ) {
				return;
			}
			int currentItem = pager.CurrentItem;
			page.CurrentPage = currentItem < 0 || currentItem >= page.Children.Count ? null : page.Children [ currentItem ];
		}

		/* NOTES:
		 * 
		 *  @Shahman's Note:
		 *  Due to a plenty of internal functions that needs to be override, I've decided to take an approach to re-invented the wheel. doing so will require me 
		 *  to call couple of internal functions. Therefore, until it was made protected or public, I have no choice but to use reflection to call those functions.
		 *  
		 * Hopefully, in the future, these functions are available for developers to be used.
		 * 
		 * According to Adam Kemp:
		 *  "... some of those functions are internal to Xamarin.Forms, which means you need to use reflection to get them... "
		 * More Discussions here: http://forums.xamarin.com/discussion/comment/111954/#Comment_111954
		 *
		 */

		#region Hack via Reflection

		private delegate IVisualElementRenderer GetRendererDelegate ( BindableObject bindable );

		private static GetRendererDelegate getRendererDelegate;

		/// <summary>
		/// Reflection from function IVisualElementRenderer Xamarin.Forms.Platform.Android.Platform.GetRenderer(BindableObject bindable)
		/// </summary>
		/// <param name="bindable">The bindable.</param>
		/// <returns></returns>
		private static IVisualElementRenderer GetRenderer ( BindableObject bindable ) {
			if ( bindable == null ) {
				return null;
			}
			if ( getRendererDelegate == null ) {
				var assembly = typeof ( CarouselPageRenderer ).Assembly;
				var platformType = assembly.GetType ( "Xamarin.Forms.Platform.Android.Platform" );
				var method = platformType.GetMethod ( "GetRenderer" );
				getRendererDelegate = ( GetRendererDelegate ) method.CreateDelegate ( typeof ( GetRendererDelegate ) );
			}
			return getRendererDelegate ( bindable );
		}

		private delegate void SetRendererDelegate ( BindableObject bindable , IVisualElementRenderer value );

		private static SetRendererDelegate setRendererDelegate;

		/// <summary>
		/// Reflection from function Xamarin.Forms.Platform.Android.Platform.SetRenderer ( BindableObject bindable , IVisualElementRenderer value )
		/// </summary>
		/// <param name="bindable">The bindable.</param>
		/// <param name="value">The value.</param>
		private static void SetRenderer ( BindableObject bindable , IVisualElementRenderer value ) {
			if ( bindable == null ) {
				return;
			}
			if ( value == null ) {
				return;
			}
			if ( setRendererDelegate == null ) {
				var assembly = typeof ( CarouselPageRenderer ).Assembly;
				var platformType = assembly.GetType ( "Xamarin.Forms.Platform.Android.Platform" );
				var method = platformType.GetMethod ( "SetRenderer" );
				setRendererDelegate = ( SetRendererDelegate ) method.CreateDelegate ( typeof ( SetRendererDelegate ) );
			}
			setRendererDelegate ( bindable , value );
		}

		#endregion
	}
}