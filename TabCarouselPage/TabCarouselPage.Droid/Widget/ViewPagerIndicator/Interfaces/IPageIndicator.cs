/***************************************************************************************************************
   * IPageIndicator.cs
   * 
   * Copyright (c) 2015, Shahman Teh Sharifuddin
   * All rights reserved.     
   * 
**************************************************************************************************************/
using Android.Support.V4.View;

namespace TabCarouselPage.Droid.Widget
{
    public interface IPageIndicator : ViewPager.IOnPageChangeListener
    {
        /// <summary>
        /// Bind the indicator to a ViewPager.
        /// </summary>
        /// <param name="view">View.</param>
        void SetViewPager(ViewPager view);

        /// <summary>
        /// Bind the indicator to a ViewPager.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="initialPosition">The initial position.</param>
        void SetViewPager(ViewPager view, int initialPosition);

        /// <summary>
        /// <para>Set the current page of both the ViewPager and indicator.</para>
        /// <para>This must be used if you need to set the page before the views are drawn on screen (e.g., default start page).</para>
        /// </summary>
        /// <param name="item">The item.</param>
        void SetCurrentItem(int item);

        /// <summary>
        /// Set a page change listener which will receive forwarded events.
        /// </summary>
        /// <param name="pageChangeListener">The listener.</param>
        void SetOnPageChangeListener(ViewPager.IOnPageChangeListener pageChangeListener);

        /// <summary>
        /// Notifies the indicator that the fragment list has changed.
        /// </summary>
        void NotifyDataSetChanged();
    }
}