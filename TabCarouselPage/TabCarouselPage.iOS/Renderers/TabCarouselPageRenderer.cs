/***************************************************************************************************************
	* TabCarouselPageRenderer.cs
	* 
	* Copyright (c) 2015, Shahman Teh Sharifuddin
	* All rights reserved.     
	* 
**************************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using CoreGraphics;
using TabCarouselPage.Core;
using TabCarouselPage.iOS.Renderers;
using TabCarouselPage.iOS.UIKit;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TabCarouselPage.Core.TabCarouselPage), typeof(TabCarouselPageRenderer))]

namespace TabCarouselPage.iOS.Renderers
{
	public class TabCarouselPageRenderer : UIViewController , IVisualElementRenderer
	{
		#region Tab Bar View

		protected UITabBar tabBarView;
		private List <UITabBarItem> tabBarItems;

		private static int TabBarHeight {
			get {
				if ( Device.Idiom == TargetIdiom.Tablet ) {
					try {
						return int.Parse ( UIDevice.CurrentDevice.SystemVersion.Substring ( 0 , 1 ) ) == 7 ? 56 : 49;
					} catch {
						return 49;
					}
				}
				return 49;
			}
		}
		#endregion

		private UIScrollView scrollView;
		private Dictionary <Page , UIView> containerMap;
		private VisualElementTracker tracker;
		private EventTracker events;
		private bool disposed;
		private bool appeared;
		private bool ignoreNativeScrolling;
		private EventHandler <VisualElementChangedEventArgs> elementChanged;

		public VisualElement Element { get; private set; }

		/// <summary>
		/// Empty functions in order for Xamarin.Forms to load this class
		/// </summary>
		public static void Load () {}


		protected internal int SelectedIndex {
			get { return ( int ) ( scrollView.ContentOffset.X / scrollView.Frame.Width ); }
			set { ScrollToPage ( value , true ); }
		}

		protected internal TabCarouselPage.Core.TabCarouselPage TabbedCarousel {
			get { return ( TabCarouselPage.Core.TabCarouselPage ) Element; }
		}

		public UIView NativeView {
			get { return View; }
		}

		public UIViewController ViewController {
			get { return this; }
		}

		public event EventHandler <VisualElementChangedEventArgs> ElementChanged {
			add {
				EventHandler <VisualElementChangedEventArgs> eventHandler = elementChanged;
				EventHandler <VisualElementChangedEventArgs> comparand;
				do {
					comparand = eventHandler;
					eventHandler = Interlocked.CompareExchange ( ref elementChanged , comparand + value , comparand );
				} while ( eventHandler != comparand );
			}
			remove {
				EventHandler <VisualElementChangedEventArgs> eventHandler = elementChanged;
				EventHandler <VisualElementChangedEventArgs> comparand;
				do {
					comparand = eventHandler;
					eventHandler = Interlocked.CompareExchange ( ref elementChanged , comparand - value , comparand );
				} while ( eventHandler != comparand );
			}
		}

		public TabCarouselPageRenderer () {
			bool isiOS7OrNewer;
			//do an extension 
			try {
				isiOS7OrNewer = int.Parse ( UIDevice.CurrentDevice.SystemVersion.Substring ( 0 , 1 ) ) >= 7;
			} catch ( Exception ) {
				isiOS7OrNewer = false;
			}

			if ( isiOS7OrNewer ) {
				return;
			}
			WantsFullScreenLayout = false;

		}

		public void SetElement ( VisualElement element ) {
			VisualElement element1 = Element;
			Element = element;
			containerMap = new Dictionary <Page , UIView> ();
			OnElementChanged ( new VisualElementChangedEventArgs ( element1 , element ) );
			if ( element == null ) {
				return;
			}
			Forms_SendViewInitialized ( element , NativeView );
		}

		public void SetElementSize ( Size size ) {
			Element.Layout ( new Rectangle ( Element.X , Element.Y , size.Width , size.Height ) );
		}

		public SizeRequest GetDesiredSize ( double widthConstraint , double heightConstraint ) {
			return NativeView.GetSizeRequest ( widthConstraint , heightConstraint );
		}

		public override void ViewDidLoad () {
			base.ViewDidLoad ();

			//Initialize the tab bar items
			tabBarItems = new List <UITabBarItem> ();

			tracker = new VisualElementTracker ( this );
			events = new EventTracker ( this );
			events.LoadEvents ( View );
			scrollView = new UIScrollView {
					ShowsHorizontalScrollIndicator = false
			};
			scrollView.DecelerationEnded += OnDecelerationEnded;
			View.AddSubview ( scrollView );
			int num = 0;
			foreach ( var page in TabbedCarousel.Children ) {
				InsertPage ( page , num++ );
			}
			PositionChildren ();
			TabbedCarousel.PropertyChanged += OnPropertyChanged;
			TabbedCarousel.PagesChanged += OnPagesChanged;

			//Initialized the tab bar view
			tabBarView = new UITabBar {
					Items = tabBarItems.ToArray () ,
					Translucent = true
			};
			View.AddSubview ( tabBarView );
		}

		private void OnElementChanged ( VisualElementChangedEventArgs e ) {
			EventHandler <VisualElementChangedEventArgs> eventHandler = elementChanged;
			if ( eventHandler == null ) {
				return;
			}
			eventHandler ( this , e );
		}

		private void OnDecelerationEnded ( object sender , EventArgs e ) {
			if ( ignoreNativeScrolling || SelectedIndex >= TabbedCarousel.Children.Count ) {
				return;
			}
			TabbedCarousel.CurrentPage = TabbedCarousel.Children [ SelectedIndex ];
		}

		private void Reset () {
			Clear ();
			int num = 0;
			foreach ( ContentPage page in TabbedCarousel.Children ) {
				InsertPage ( page , num++ );
			}
		}

		private void InsertPage ( ContentPage page , int index ) {
			IVisualElementRenderer renderer = GetRenderer ( page );
			if ( renderer == null ) {
				renderer = RendererFactory.GetRenderer ( page );
				SetRenderer ( page , renderer );
			}
			UIView view = new UIPageContainer ( page );
			view.AddSubview ( renderer.NativeView );
			containerMap [ page ] = view;
			AddChildViewController ( renderer.ViewController );
			scrollView.InsertSubview ( view , index );

			//Add TabBarItem into the list
			switch ( TabbedCarousel.TabType ) {
				case TabType.TitleOnly :
					renderer.ViewController.TabBarItem = new UITabBarItem ( page.Title , null , index );
					break;
				case TabType.TitleWithIcon :
					renderer.ViewController.TabBarItem = new UITabBarItem ( page.Title , !string.IsNullOrEmpty ( page.Icon ) ? UIImage.FromFile ( page.Icon ) : null , index );
					break;
				case TabType.IconOnly :
					if ( !string.IsNullOrEmpty ( page.Icon ) ) {
						renderer.ViewController.TabBarItem = new UITabBarItem ( string.Empty , UIImage.FromFile ( page.Icon ) , index );
					} else {
						throw new NullReferenceException ( "Please include a tab icon for this" );
					}
					break;
				default :
					throw new ArgumentOutOfRangeException ();
			}
			tabBarItems.Insert ( index , renderer.ViewController.TabBarItem );
		}

		private void RemovePage ( ContentPage page , int index ) {
			containerMap [ page ].RemoveFromSuperview ();
			containerMap.Remove ( page );
			IVisualElementRenderer renderer = GetRenderer ( page );
			if ( renderer == null ) {
				return;
			}
			renderer.ViewController.RemoveFromParentViewController ();
			renderer.NativeView.RemoveFromSuperview ();

			//Remove TabBarItem from the list
			tabBarItems.RemoveAt ( index );
		}

		private void SelectSetTabBar ( int index ) {
			if ( tabBarView == null || Equals ( tabBarView.SelectedItem , tabBarItems [ index ] ) ) {
				return;
			}
			tabBarView.SelectedItem = tabBarItems [ index ];
			tabBarView.BackgroundColor = TabbedCarousel.CurrentPage.BackgroundColor.ToUIColor ();
		}

		private void OnPropertyChanged ( object sender , PropertyChangedEventArgs e ) {
			if ( e.PropertyName != "CurrentPage" ) {
				return;
			}
			UpdateCurrentPage ( true );
		}

		private void OnPagesChanged ( object sender , NotifyCollectionChangedEventArgs e ) {
			ignoreNativeScrolling = true;
			NotifyCollectionChangedAction collectionChangedAction = Apply ( self : e ,
																			insert : ( o , i , c ) => InsertPage ( ( ContentPage ) o , i ) ,
																			removeAt : ( o , i ) => RemovePage ( ( ContentPage ) o , i ) ,
																			reset : Reset );
			PositionChildren ();
			ignoreNativeScrolling = false;
			if ( collectionChangedAction != NotifyCollectionChangedAction.Reset ) {
				return;
			}
			int index = TabbedCarousel.CurrentPage != null ? TabbedCarousel.Children.IndexOf ( TabbedCarousel.CurrentPage ) : 0;
			if ( index < 0 ) {
				index = 0;
			}

			//Set the selected
			SelectSetTabBar ( index );
			ScrollToPage ( index , true );
		}

		public override void ViewDidLayoutSubviews () {
			base.ViewDidLayoutSubviews ();

			//Setup tabbar frame and delegate
			tabBarView.Frame = new CGRect ( 0 , View.Bounds.Height - TabBarHeight , View.Bounds.Width , TabBarHeight );
			tabBarView.Delegate = new TabCarouselBarDelegate ( this );

			//Adjust the scrollview height to accomodate the tabbar
			scrollView.Frame = new CGRect ( View.Bounds.X , View.Bounds.Y , View.Bounds.Width , View.Bounds.Height - tabBarView.Frame.Height );

			//Children need to set a different bounds than the parent, therefore the following needs to be called
			Element.Layout ( new Rectangle ( 0.0 , 0.0 , View.Bounds.Width , View.Bounds.Height ) );
			//this is where we layout the view
			foreach ( var page in TabbedCarousel.Children ) {
				page.Layout ( new Rectangle ( Element.X , Element.Y , Element.Width , Element.Height - TabBarHeight ) );
			}

			PositionChildren ();
			UpdateCurrentPage ( false );
		}

		public override void WillRotate ( UIInterfaceOrientation toInterfaceOrientation , double duration ) {
			ignoreNativeScrolling = true;
		}

		public override void DidRotate ( UIInterfaceOrientation fromInterfaceOrientation ) {
			ignoreNativeScrolling = false;
		}

		private void Clear () {
			foreach ( var keyValuePair in containerMap ) {
				keyValuePair.Value.RemoveFromSuperview ();
				var renderer = GetRenderer ( keyValuePair.Key );
				if ( renderer != null ) {
					renderer.ViewController.RemoveFromParentViewController ();
					renderer.NativeView.RemoveFromSuperview ();
					SetRenderer ( keyValuePair.Key , null );
				}
			}
			containerMap.Clear ();
		}

		protected override void Dispose ( bool disposing ) {
			if ( disposing && !disposed ) {
				TabbedCarousel.PropertyChanged -= OnPropertyChanged;
				SetRenderer ( Element , null );
				Clear ();
				if ( scrollView != null ) {
					scrollView.DecelerationEnded -= OnDecelerationEnded;
					scrollView.RemoveFromSuperview ();
					scrollView = null;
				}
				if ( appeared ) {
					appeared = false;
					SendDisappearing ();
				}
				if ( tabBarView != null ) {
					if ( tabBarView.Delegate != null ) {
						tabBarView.Delegate.Dispose ();
						tabBarView.Delegate = null;
					}
					tabBarView.RemoveFromSuperview ();
					tabBarView = null;
				}
				if ( events != null ) {
					events.Dispose ();
					events = null;
				}
				if ( tracker != null ) {
					tracker.Dispose ();
					tracker = null;
				}
				Element = null;
				disposed = true;
			}
			base.Dispose ( disposing );
		}

		private void UpdateCurrentPage ( bool animated = true ) {
			ContentPage currentPage = TabbedCarousel.CurrentPage;
			if ( currentPage == null ) {
				return;
			}
			//set the tab bar again
			SelectSetTabBar ( TabbedCarousel.Children.IndexOf ( TabbedCarousel.CurrentPage ) );
			ScrollToPage ( TabbedCarousel.Children.IndexOf ( TabbedCarousel.CurrentPage ) , animated );
		}

		private void ScrollToPage ( int index , bool animated = true ) {
			if ( scrollView.ContentOffset.X == index * scrollView.Frame.Width ) {
				return;
			}
			scrollView.SetContentOffset ( new CGPoint ( index * scrollView.Frame.Width , 0 ) , animated );
		}

		private void PositionChildren () {
			nfloat x = 0;
			CGRect bounds = View.Bounds;
			foreach ( var index in TabbedCarousel.Children ) {
				//Adjust the container's view based on the bound's width and scrollview's height
				containerMap [ index ].Frame = new CGRect ( x , bounds.Y , bounds.Width , scrollView.Frame.Height );
				x += bounds.Width;
			}
			scrollView.PagingEnabled = true;
			//Set the scrollview's contentsize based on the container's total width and scrollView's height
			scrollView.ContentSize = new CGSize ( bounds.Width * TabbedCarousel.Children.Count , scrollView.Frame.Height );
		}

		public override void ViewDidAppear ( bool animated ) {
			base.ViewDidAppear ( animated );
			SendAppearing ();
		}

		public override void ViewDidDisappear ( bool animated ) {
			base.ViewDidDisappear ( animated );
			SendDisappearing ();
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

		private delegate NotifyCollectionChangedAction ApplyNotifyCollectionChangedActionDelegate ( NotifyCollectionChangedEventArgs self , Action <object , int , bool> insert , Action <object , int> removeAt , Action reset );

		private static ApplyNotifyCollectionChangedActionDelegate applyNotifyCollectionChangedActionDelegate;

		/// <summary>
		/// Reflection from function Xamarin.Forms.Forms::Apply(this NotifyCollectionChangedEventArgs self, Action( object, int, bool ) insert, Action( object, int ) removeAt, Action reset)
		/// </summary>
		/// <param name="self">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		/// <param name="insert">The insert.</param>
		/// <param name="removeAt">The remove at.</param>
		/// <param name="reset">The reset.</param>
		/// <returns></returns>
		private static NotifyCollectionChangedAction Apply ( NotifyCollectionChangedEventArgs self , Action <object , int , bool> insert , Action <object , int> removeAt , Action reset ) {
			if ( applyNotifyCollectionChangedActionDelegate == null ) {
				var assembly = typeof ( Xamarin.Forms.Application ).Assembly;
				var platformType = assembly.GetType ( "Xamarin.Forms.NotifyCollectionChangedEventArgsExtensions" );
				var method = platformType.GetMethod ( "Apply" , BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
				applyNotifyCollectionChangedActionDelegate = ( ApplyNotifyCollectionChangedActionDelegate ) method.CreateDelegate ( typeof ( ApplyNotifyCollectionChangedActionDelegate ) );
			}
			return applyNotifyCollectionChangedActionDelegate ( self , insert , removeAt , reset );
		}

		private delegate void Forms_SendViewInitializedDelegate ( VisualElement element , UIView nativeView );

		private static Forms_SendViewInitializedDelegate formsSendViewInitializedDelegate;

		/// <summary>
		/// Reflection from function Xamarin.Forms.Forms::SendViewInitialized(this VisualElement self, UIView nativeView)
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="nativeView">The native view.</param>
		private static void Forms_SendViewInitialized ( VisualElement element , UIView nativeView ) {
			if ( formsSendViewInitializedDelegate == null ) {
				var assembly = typeof ( CarouselPageRenderer ).Assembly;
				var platformType = assembly.GetType ( "Xamarin.Forms.Forms" );
				var method = platformType.GetMethod ( "SendViewInitialized" , BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
				formsSendViewInitializedDelegate = ( Forms_SendViewInitializedDelegate ) method.CreateDelegate ( typeof ( Forms_SendViewInitializedDelegate ) );
			}
			formsSendViewInitializedDelegate ( element , nativeView );
		}

		/// <summary>
		/// Reflection from property GET Rectangle Xamarin.Forms.Page.ContainerArea
		/// </summary>
		/// <returns></returns>
		private Rectangle Page_GetContainerArea () {
			var page = Element as Page;
			if ( page == null ) {
				return Rectangle.Zero;
			}
			var platformType = page.GetType ();
			var property = platformType.GetProperty ( "ContainerArea" , BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			return ( Rectangle ) property.GetValue ( page , null );
		}

		/// <summary>
		/// Reflection from property SET Rectangle Xamarin.Forms.Page.ContainerArea
		/// </summary>
		/// <param name="rectangle">The rectangle.</param>
		private void Page_SetContainerArea ( Rectangle rectangle ) {
			var page = Element as Page;
			if ( page == null ) {
				return;
			}

			var platformType = page.GetType ();
			var property = platformType.GetProperty ( "ContainerArea" , BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			property.SetValue ( page , rectangle );
		}

		/// <summary>
		/// Reflection from function Xamarin.Forms.Page.SendAppearing()
		/// </summary>
		private void SendAppearing () {
			var page = Element as Page;
			if ( page == null ) {
				return;
			}
			var assembly = typeof ( Page ).Assembly;
			var platformType = assembly.GetType ( "Xamarin.Forms.Page" );
			var method = platformType.GetMethod ( "SendAppearing" , BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance );
			method.Invoke ( page , null );
		}

		/// <summary>
		/// Reflection from function Xamarin.Forms.Page.SendDisappearing()
		/// </summary>
		private void SendDisappearing () {
			var page = Element as Page;
			if ( page == null ) {
				return;
			}
			var assembly = typeof ( Page ).Assembly;
			var platformType = assembly.GetType ( "Xamarin.Forms.Page" );
			var method = platformType.GetMethod ( "SendDisappearing" , BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance );
			method.Invoke ( page , null );
		}

		private delegate IVisualElementRenderer GetRendererDelegate ( BindableObject bindable );

		private static GetRendererDelegate getRendererDelegate;

		/// <summary>
		/// Reflection from function IVisualElementRenderer Xamarin.Forms.Platform.iOS.Platform.GetRenderer(BindableObject bindable)
		/// </summary>
		/// <param name="bindable">The bindable.</param>
		/// <returns></returns>
		private static IVisualElementRenderer GetRenderer ( BindableObject bindable ) {
			if ( bindable == null ) {
				return null;
			}
			if ( getRendererDelegate == null ) {
				var assembly = typeof ( CarouselPageRenderer ).Assembly;
				var platformType = assembly.GetType ( "Xamarin.Forms.Platform.iOS.Platform" );
				var method = platformType.GetMethod ( "GetRenderer" );
				getRendererDelegate = ( GetRendererDelegate ) method.CreateDelegate ( typeof ( GetRendererDelegate ) );
			}
			return getRendererDelegate ( bindable );
		}

		private delegate void SetRendererDelegate ( BindableObject bindable , IVisualElementRenderer value );

		private static SetRendererDelegate setRendererDelegate;

		/// <summary>
		/// Reflection from function Xamarin.Forms.Platform.iOS.Platform.SetRenderer ( BindableObject bindable , IVisualElementRenderer value )
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
				var platformType = assembly.GetType ( "Xamarin.Forms.Platform.iOS.Platform" );
				var method = platformType.GetMethod ( "SetRenderer" );
				setRendererDelegate = ( SetRendererDelegate ) method.CreateDelegate ( typeof ( SetRendererDelegate ) );
			}
			setRendererDelegate ( bindable , value );
		}

		#endregion
	}
}