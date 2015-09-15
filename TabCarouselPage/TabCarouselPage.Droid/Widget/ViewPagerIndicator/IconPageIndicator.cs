/***************************************************************************************************************
   * IconPageIndicator.cs
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

namespace TabCarouselPage.Droid.Widget
{
    public class IconPageIndicator : HorizontalScrollView, IPageIndicator
    {

        private readonly LinearLayout mIconsLayout;
        private ViewPager mViewPager;
        private ViewPager.IOnPageChangeListener mListener;
        private Action mIconSelector;
        private int mSelectedIndex;


        public IconPageIndicator(Context context) : base(context, null) { }

        public IconPageIndicator(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            HorizontalScrollBarEnabled = false;

            mIconsLayout = new LinearLayout(context);
            AddView(mIconsLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.FillParent));
        }

        private void AnimateToIcon(int position)
        {
            var iconView = mIconsLayout.GetChildAt(position);
            if (mIconSelector != null)
            {
                RemoveCallbacks(mIconSelector);
            }
            mIconSelector = () =>
            {
                var scrollPos = iconView.Left - (Width - iconView.Width) / 2;
                SmoothScrollTo(scrollPos, 0);
                mIconSelector = null;
            };
            Post(mIconSelector);
            //            Post ( () => {
            //                var scrollPos = iconView.Left - ( Width - iconView.Width ) / 2;
            //                SmoothScrollTo ( scrollPos , 0 );
            //            } );
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            if (mIconSelector != null)
            {
                Post(mIconSelector);
            }
        }

        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            if (mIconSelector != null)
            {
                RemoveCallbacks(mIconSelector);
            }
        }

        public void OnPageScrollStateChanged(int state)
        {
            if (mListener != null)
            {
                mListener.OnPageScrollStateChanged(state);
            }
        }

        public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
        {
            if (mListener != null)
            {
                mListener.OnPageScrolled(position, positionOffset, positionOffsetPixels);
            }
        }

        public void OnPageSelected(int position)
        {
            SetCurrentItem(position);
            if (mListener != null)
            {
                mListener.OnPageSelected(position);
            }
        }

        public void SetViewPager(ViewPager view)
        {
            var adapter = view.Adapter;
            if (adapter == null)
            {
                throw new IllegalStateException("ViewPager does not have adapter instance. ");
            }
            if (!(adapter is IIconPagerAdapter))
            {
                throw new IllegalStateException("ViewPager adapter must implement IIconPagerAdapter to be used with IconPageIndicator.");
            }
            //            if ( mViewPager == view ) {
            //                return;
            //            }
            //            if ( mViewPager != null ) {
            //                mViewPager.SetOnPageChangeListener ( null );
            //            }
            mViewPager = view;
            view.SetOnPageChangeListener(this);
            NotifyDataSetChanged();
        }

        public void SetViewPager(ViewPager view, int initialPosition)
        {
            SetViewPager(view);
            SetCurrentItem(initialPosition);
        }

        public void SetCurrentItem(int item)
        {
            if (mViewPager == null)
            {
                throw new IllegalStateException("Viewpager has not been bound.");
            }
            mSelectedIndex = item;
            mViewPager.CurrentItem = item;

            var tabCount = mIconsLayout.ChildCount;
            for (var i = 0; i < tabCount; i++)
            {
                var child = mIconsLayout.GetChildAt(i);
                var isSelected = (i == item);
                child.Selected = isSelected;
                if (isSelected)
                {
                    AnimateToIcon(item);
                }
            }
        }

        public void SetOnPageChangeListener(ViewPager.IOnPageChangeListener pageChangeListener)
        {
            mListener = pageChangeListener;
        }

        public void NotifyDataSetChanged()
        {
            mIconsLayout.RemoveAllViews();
            var iconAdapter = (IIconPagerAdapter)mViewPager.Adapter;
            if (iconAdapter == null)
            {
                throw new NullReferenceException("ViewPager adapter is not an Icon Pager Adapter");
            }
            var count = iconAdapter.Count;
            for (var i = 0; i < count; i++)
            {
                var view = new ImageView(Context, null, Resource.Attribute.vpiIconPageIndicatorStyle);
                view.SetImageResource(iconAdapter.GetIconResId(i));
                mIconsLayout.AddView(view);
            }
            if (mSelectedIndex < count)
            {
                mSelectedIndex = count - 1;
            }
            SetCurrentItem(mSelectedIndex);
            RequestLayout();
        }
    }
}