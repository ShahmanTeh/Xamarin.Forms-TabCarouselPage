/***************************************************************************************************************
   * TabPageIndicator.cs
   * 
   * Copyright (c) 2015, Shahman Teh Sharifuddin
   * All rights reserved.     
   * 
**************************************************************************************************************/

using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace TabCarouselPage.Droid.Widget
{
    [Register("tabcarouselpage.droid.widget.TabPageIndicator")]
    public class TabPageIndicator : HorizontalScrollView, IPageIndicator
    {
        protected readonly IcsLinearLayout mTabLayout;
        protected ViewPager mViewPager;
        protected ViewPager.IOnPageChangeListener mListener;
        protected readonly LayoutInflater mInflater;
        public int mMaxTabWidth;
        protected int mSelectedTabIndex;

        public TabPageIndicator ( Context context ) : base ( context , null ) {

            HorizontalScrollBarEnabled = false;

            mInflater = LayoutInflater.From ( context );

            mTabLayout = new IcsLinearLayout ( Context , Resource.Attribute.vpiTabPageIndicatorStyle );
            mTabLayout.SetBackgroundColor ( Color.Rgb ( 33 , 33 , 33 ) );
            AddView ( mTabLayout , new ViewGroup.LayoutParams ( ViewGroup.LayoutParams.WrapContent , ViewGroup.LayoutParams.WrapContent ) );
        }

        public TabPageIndicator(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            HorizontalScrollBarEnabled = false;

            mInflater = LayoutInflater.From(context);

            mTabLayout = new IcsLinearLayout(Context, Resource.Attribute.vpiTabPageIndicatorStyle);
            AddView(mTabLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
            var lockedExpanded = widthMode == MeasureSpecMode.Exactly;
            FillViewport = lockedExpanded;

            var childCount = mTabLayout.ChildCount;
            if (childCount > 1 && (widthMode == MeasureSpecMode.Exactly || widthMode == MeasureSpecMode.AtMost))
            {
                if (childCount > 2)
                {
                    mMaxTabWidth = (int)(MeasureSpec.GetSize(widthMeasureSpec) * 0.4f);
                }
                else
                {
                    mMaxTabWidth = MeasureSpec.GetSize(widthMeasureSpec) / 2;
                }
            }
            else
            {
                mMaxTabWidth = -1;
            }

            var oldWidth = MeasuredWidth;
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            var newWidth = MeasuredWidth;

            if (lockedExpanded && oldWidth != newWidth)
            {
                // Recenter the tab display if we're at a new (scrollable) size.
                //SetCurrentItem(mSelectedTabIndex);
            }
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();

            Console.WriteLine("OnAttachedToWindow");
            /*
             * 
             *  super.onAttachedToWindow();
                if (mTabSelector != null) {
                    // Re-post the selector we saved
                    post(mTabSelector);
                }
            */
        }

        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();

            Console.WriteLine("OnDetachedFromWindow...");
            //			super.onDetachedFromWindow();
            //        if (mTabSelector != null) {
            //            removeCallbacks(mTabSelector);
            //        }
        }

        protected virtual void AnimateToTab(int position)
        {
            var tabView = mTabLayout.GetChildAt(position);

            // Do we not have any call backs because we're handling this with Post?
            /*if (mTabSelector != null) {
                RemoveCallbacks(mTabSelector);
            }*/

            Post(() =>
            {
                var scrollPos = tabView.Left - (Width - tabView.Width) / 2;
                SmoothScrollTo(scrollPos, 0);
            });
        }

        /// <summary>
        /// Adds the tab. This is a Workaround for not being able to pass a defStyle on pre-3.0
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="index">The index.</param>
        /// <param name="iconResId">icon Resource Id if exist </param>
        protected virtual void AddTab(string text, int index, int iconResId = 0)
        {
            var tabView = (TabView)mInflater.Inflate(Resource.Layout.vpi__tab, null);
            tabView.Init(this, text.ToUpperInvariant (), index);
            tabView.Focusable = true;
            tabView.Click += delegate(object sender, EventArgs e)
            {
                var tView = (TabView)sender;
                mViewPager.CurrentItem = tView.GetIndex();
            };
            if ( iconResId != 0 ) {
                tabView.ImageView.Visibility = ViewStates.Visible;
                tabView.ImageView.SetImageResource ( iconResId );
            } else {
                tabView.ImageView.Visibility = ViewStates.Gone;
            }
            tabView.TextView.Visibility = string.IsNullOrEmpty ( tabView.TextView.Text ) ? ViewStates.Gone : ViewStates.Visible;

            mTabLayout.AddView(tabView, new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.FillParent, 1));
        }

        public virtual void OnPageScrollStateChanged(int p0)
        {
            if (mListener != null)
            {
                mListener.OnPageScrollStateChanged(p0);
            }
        }

        public virtual void OnPageScrolled(int p0, float p1, int p2)
        {
            if (mListener != null)
            {
                mListener.OnPageScrolled(p0, p1, p2);
            }
        }

        public virtual void OnPageSelected(int p0)
        {
            SetCurrentItem(p0);
            if (mListener != null)
            {
                mListener.OnPageSelected(p0);
            }
        }

        //from IPageIndicator

        public virtual void SetViewPager(ViewPager view)
        {
            var adapter = view.Adapter;
            if (adapter == null)
            {
                throw new IllegalStateException("ViewPager does not have adapter instance.");
            }
            if (!(adapter is ITitleProvider))
            {
                throw new IllegalStateException("ViewPager adapter must implement TitleProvider to be used with TitlePageIndicator.");
            }
            mViewPager = view;
            view.SetOnPageChangeListener(this);
            NotifyDataSetChanged();
        }

        public virtual void SetViewPager(ViewPager view, int initialPosition)
        {
            SetViewPager(view);
            SetCurrentItem(initialPosition);
        }

        public virtual void SetCurrentItem(int item)
        {
            if (mViewPager == null)
            {
                throw new IllegalStateException("ViewPager has not been bound.");
            }
            mSelectedTabIndex = item;
            var tabCount = mTabLayout.ChildCount;
            for (var i = 0; i < tabCount; i++)
            {
                var child = mTabLayout.GetChildAt(i);
                var isSelected = (i == item);
                child.Selected = isSelected;
                if (isSelected)
                {
                    AnimateToTab(item);
                }
            }
        }

        public virtual void SetOnPageChangeListener(ViewPager.IOnPageChangeListener listener)
        {
            mListener = listener;
        }

        public virtual void NotifyDataSetChanged()
        {
            mTabLayout.RemoveAllViews();
            var adapter = (ITitleProvider)mViewPager.Adapter;
            IIconPagerAdapter iconAdapter = null;
            if (adapter is IIconPagerAdapter)
            {
                iconAdapter = adapter as IIconPagerAdapter;
            }
            var count = ((PagerAdapter)adapter).Count;
            for (var i = 0; i < count; i++)
            {
                var title = adapter.GetTitle(i) ?? string.Empty;
                var iconResId = 0;
                if (iconAdapter != null)
                {
                    iconResId = iconAdapter.GetIconResId(i);
                }
                AddTab(title, i, iconResId);
            }
            if (mSelectedTabIndex > count)
            {
                mSelectedTabIndex = count - 1;
            }
            SetCurrentItem(mSelectedTabIndex);
            RequestLayout();
        }
    }
}