/***************************************************************************************************************
   * IcsLinearLayout.cs
   * 
   * Copyright (c) 2015, Shahman Teh Sharifuddin
   * All rights reserved.     
   * 
**************************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace TabCarouselPage.Droid.Widget
{
    public class IcsLinearLayout : LinearLayout
    {
        public static readonly int[] LinearLayoutResources = new[] {
                /* 0 */ global::Android.Resource.Attribute.Divider ,
                /* 1 */ global::Android.Resource.Attribute.ShowDividers ,
                /* 2 */ global::Android.Resource.Attribute.DividerPadding
        };

        private enum EAttributeIndex
        {
            Divider = 0,
            ShowDivider = 1,
            DividerPadding = 2
        }

        //        private const int LinearLayoutDivider = 0;
        //        private const int LinearLayoutShowDivider = 1;
        //        private const int LinearLayoutDividerPadding = 2;

        private global::Android.Graphics.Drawables.Drawable divider;
        private int dividerWidth;
        private int dividerHeight;
        private readonly int showDividers;
        private readonly int dividerPadding;

        protected IcsLinearLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }
        public IcsLinearLayout(Context context) : base(context) { }
        public IcsLinearLayout(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle) { }

        public IcsLinearLayout(Context context, int themeAttr)
            : base(context)
        {
            var a = context.ObtainStyledAttributes(null, LinearLayoutResources, themeAttr, 0);
            SetDividerDrawable(a.GetDrawable((int)EAttributeIndex.Divider));
            dividerPadding = a.GetDimensionPixelSize((int)EAttributeIndex.DividerPadding, 0);
            showDividers = a.GetInteger((int)EAttributeIndex.ShowDivider, (int)ShowDividers.None);
            a.Recycle();
        }

        public IcsLinearLayout(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            var a = context.ObtainStyledAttributes(null, LinearLayoutResources, attrs.StyleAttribute, 0);
            SetDividerDrawable(a.GetDrawable((int)EAttributeIndex.Divider));
            dividerPadding = a.GetDimensionPixelSize((int)EAttributeIndex.DividerPadding, 0);
            showDividers = a.GetInteger((int)EAttributeIndex.ShowDivider, (int)ShowDividers.None);
            a.Recycle();
        }

        public override sealed void SetDividerDrawable(global::Android.Graphics.Drawables.Drawable divider)
        {
            if (divider == this.divider)
            {
                return;
            }

            this.divider = divider;
            if (divider != null)
            {
                dividerWidth = divider.IntrinsicWidth;
                dividerHeight = divider.IntrinsicHeight;
            }
            else
            {
                dividerWidth = 0;
                dividerHeight = 0;
            }
            SetWillNotDraw(divider == null);
            RequestLayout();
        }

        protected override void MeasureChildWithMargins(View child, int parentWidthMeasureSpec, int widthUsed, int parentHeightMeasureSpec, int heightUsed)
        {
            var index = IndexOfChild(child);
            var orientation = Orientation;
            var layoutParams = ((LayoutParams)(child.LayoutParameters));
            if (HasDividerBeforeChildAt(index))
            {
                if (orientation == global::Android.Widget.Orientation.Vertical)
                {
                    //Account for the divider by pushing everything up
                    layoutParams.TopMargin = dividerHeight;
                }
                else
                {
                    //Account for the divider by pushing everything left
                    layoutParams.LeftMargin = dividerWidth;
                }
            }
            var count = ChildCount;
            if (index == count - 1)
            {
                if (HasDividerBeforeChildAt(count))
                {
                    if (orientation == global::Android.Widget.Orientation.Vertical)
                    {
                        layoutParams.BottomMargin = dividerHeight;
                    }
                    else
                    {
                        layoutParams.RightMargin = dividerWidth;
                    }
                }
            }
            base.MeasureChildWithMargins(child, parentWidthMeasureSpec, widthUsed, parentHeightMeasureSpec, heightUsed);
        }

        protected override void OnDraw(Canvas canvas)
        {
            if (divider != null)
            {
                if (Orientation == global::Android.Widget.Orientation.Vertical)
                {
                    DrawDividerVertical(canvas);
                }
                else
                {
                    DrawDividersHorizontal(canvas);
                }
            }
            base.OnDraw(canvas);
        }


        private void DrawDividerVertical(Canvas canvas)
        {
            var count = ChildCount;
            for (var i = 0; i < count; i++)
            {
                var child = GetChildAt(i);
                if (child != null && child.Visibility != ViewStates.Gone)
                {
                    if (HasDividerBeforeChildAt(i))
                    {
                        var layoutParams = ((LayoutParams)(child.LayoutParameters));
                        var top = child.Top = layoutParams.TopMargin /*- mDividerHeight*/;
                        DrawHorizontalDivider(canvas, top);
                    }
                }
            }

            if (HasDividerBeforeChildAt(count))
            {
                var child = GetChildAt(count - 1);
                var bottom = 0;
                if (child == null)
                {
                    bottom = Height - PaddingBottom - dividerHeight;
                }
                else
                {
                    //LayoutParams layoutParams = ( ( LayoutParams ) ( child.LayoutParameters ) );
                    bottom = child.Bottom /* + layoutParams.BottomMargin*/;
                }
                DrawHorizontalDivider(canvas, bottom);
            }
        }

        private void DrawDividersHorizontal(Canvas canvas)
        {
            var count = ChildCount;
            for (var i = 0; i < count; i++)
            {
                var child = GetChildAt(i);

                if (child != null && child.Visibility != ViewStates.Gone)
                {
                    if (HasDividerBeforeChildAt(i))
                    {
                        var layoutParams = ((LayoutParams)(child.LayoutParameters));
                        var left = child.Left - layoutParams.LeftMargin /* - mDividerWidth */;
                        DrawVerticalDivider(canvas, left);
                    }
                }
            }

            if (HasDividerBeforeChildAt(count))
            {
                var child = GetChildAt(count - 1);
                var right = 0;
                if (child == null)
                {
                    right = Width - PaddingRight - dividerWidth;
                }
                else
                {
                    //LayoutParams layoutParams = ( ( LayoutParams ) ( child.LayoutParameters ) );
                    right = child.Right /* + layoutParams.RightMargin*/;
                }
                DrawVerticalDivider(canvas, right);
            }
        }

        private void DrawHorizontalDivider(Canvas canvas, int top)
        {
            divider.SetBounds(PaddingLeft + dividerPadding, top, Width - PaddingRight - dividerPadding, top + dividerHeight);
            divider.Draw(canvas);
        }

        private void DrawVerticalDivider(Canvas canvas, int left)
        {
            divider.SetBounds(left, PaddingTop + dividerPadding, left + dividerWidth, Height - PaddingBottom - dividerPadding);
            divider.Draw(canvas);
        }

        private bool HasDividerBeforeChildAt(int childIndex)
        {
            if (childIndex == 0 || childIndex == ChildCount)
            {
                return false;
            }
            if ((showDividers & (int)ShowDividers.Middle) != 0)
            {
                var hasVisibleViewBefore = false;
                for (var i = childIndex - 1; i >= 0; i--)
                {
                    if (GetChildAt(i).Visibility == ViewStates.Gone)
                    {
                        continue;
                    }
                    hasVisibleViewBefore = true;
                    break;
                }
                return hasVisibleViewBefore;
            }
            return false;
        }
    }
}