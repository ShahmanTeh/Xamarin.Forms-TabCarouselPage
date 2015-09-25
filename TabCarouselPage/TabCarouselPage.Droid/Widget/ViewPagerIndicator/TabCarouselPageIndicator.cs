/***************************************************************************************************************
   * TabCarouselPageIndicator.cs
   * 
   * Copyright (c) 2015, Shahman Teh Sharifuddin
   * All rights reserved.     
   * 
**************************************************************************************************************/
using System;
using Android.Content;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using TabCarouselPage.Core;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace TabCarouselPage.Droid.Widget
{
    public class TabCarouselPageIndicator : TabPageIndicator
    {
        private readonly CarouselPage element;

        private Core.TabCarouselPage TabbedCarousel {
            get { return ( Core.TabCarouselPage ) element; }
        }

        public TabCarouselPageIndicator ( Context context , CarouselPage element )
                : base ( context ) {
            this.element = element;
        }

        public TabCarouselPageIndicator ( Context context , CarouselPage element , IAttributeSet attrs )
                : base ( context , attrs ) {
            this.element = element;
        }

        public override void SetViewPager ( ViewPager view ) {
            var adapter = view.Adapter;
            if ( adapter == null ) {
                throw new IllegalStateException ( "ViewPager does not have adapter instance." );
            }
            mViewPager = view;
            view.SetOnPageChangeListener ( this );
            NotifyDataSetChanged ();
        }

        public override void NotifyDataSetChanged () {
            mTabLayout.RemoveAllViews ();
            var adapter = mViewPager.Adapter;
            var count = adapter.Count;
            for ( var i = 0; i < count; i++ ) {
                string title;
                int iconResId = 0;
                switch ( TabbedCarousel.TabType ) {
                    case TabType.TitleOnly :
                        title = element.Children [ i ].Title ?? string.Empty;
                        break;
                    case TabType.TitleWithIcon :
                        title = element.Children [ i ].Title ?? string.Empty;
                        if ( element.Children [ i ].Icon != null ) {
                            var filename = element.Children [ i ].Icon.File.Contains ( ".png" )
                                                   ? element.Children [ i ].Icon.File.Replace ( ".png" , string.Empty )
                                                   : element.Children [ i ].Icon.File;
                            iconResId = Context.Resources.GetIdentifier ( filename , "drawable" , Context.PackageName );
                        } else {
                            iconResId = global::Android.Resource.Color.Transparent;
                        }
                        break;
                    case TabType.IconOnly :
                        title = string.Empty;
                        if ( element.Children [ i ].Icon != null ) {
                            var filename = element.Children [ i ].Icon.File.Contains ( ".png" )
                                                   ? element.Children [ i ].Icon.File.Replace ( ".png" , string.Empty )
                                                   : element.Children [ i ].Icon.File;
                            iconResId = Context.Resources.GetIdentifier ( filename , "drawable" , Context.PackageName );
                        } else {
                            iconResId = global::Android.Resource.Color.Transparent;
                        }
                        break;
                    default :
                        throw new ArgumentOutOfRangeException ();
                }
                AddTab ( title.ToUpperInvariant () , i , iconResId );
            }
            if ( mSelectedTabIndex > count ) {
                mSelectedTabIndex = count - 1;
            }
            SetCurrentItem ( mSelectedTabIndex );
            RequestLayout ();
        }

        protected override void AddTab ( string text , int index , int iconResId = 0 ) {
            base.AddTab ( text , index , iconResId );
            for ( var i = 0; i < mTabLayout.ChildCount; i++ ) {
                if ( !( mTabLayout.GetChildAt ( i ) is TabView ) ) {
                    continue;
                }
                var tabView = mTabLayout.GetChildAt ( i ) as TabView;
                if ( tabView == null ) {
                    continue;
                }
                switch ( TabbedCarousel.TabType ) {
                    case TabType.TitleOnly :
                        break;
                    case TabType.TitleWithIcon :
                        tabView.TextView.SetTextSize ( ComplexUnitType.Sp , 10 );
                        break;
                    case TabType.IconOnly :
                        break;
                    default :
                        throw new ArgumentOutOfRangeException ();
                }
            }
        }
    }
}