/***************************************************************************************************************
   * TabView.cs
   * 
   * Copyright (c) 2015, Shahman Teh Sharifuddin
   * All rights reserved.     
   * 
**************************************************************************************************************/
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace TabCarouselPage.Droid.Widget
{
    [Register("tabcarouselpage.droid.widget.TabView")]
    public class TabView : LinearLayout
    {
        private TabPageIndicator mParent;
        private int mIndex;

        public TabView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public TextView TextView { get; set; }
        public ImageView ImageView { get; set; }

        public void Init(TabPageIndicator parent, string text, int index)
        {
            mParent = parent;
            mIndex = index;

            TextView = FindViewById<TextView>(global::Android.Resource.Id.Text1);
            ImageView = FindViewById<ImageView>(global::Android.Resource.Id.Icon1);
            TextView.Text = text;
        }

        public void InitForm ( TabPageIndicator parent , string text , int index ) {
            mParent = parent;
            mIndex = index;

            TextView = FindViewById<TextView>(global::Android.Resource.Id.Text1);
            ImageView = FindViewById<ImageView>(global::Android.Resource.Id.Icon1);
            TextView.Text = text;
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

            // Re-measure if we went beyond our maximum size.
            if (mParent.mMaxTabWidth > 0 && MeasuredWidth > mParent.mMaxTabWidth)
            {
                base.OnMeasure(MeasureSpec.MakeMeasureSpec(mParent.mMaxTabWidth, MeasureSpecMode.Exactly), heightMeasureSpec);
            }

        }

        public int GetIndex()
        {
            return mIndex;
        }
    }
}