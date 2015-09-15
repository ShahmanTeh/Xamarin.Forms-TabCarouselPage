/***************************************************************************************************************
   * CirclePageIndicator.cs
   * 
   * Copyright (c) 2015, Shahman Teh Sharifuddin
   * All rights reserved.     
   * 
**************************************************************************************************************/
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Math = System.Math;

namespace TabCarouselPage.Droid.Widget
{
    public class CirclePageIndicator : View, IPageIndicator
    {
        const int Horizontal = 0;
        const int Vertical = 1;
        private float radius;
        private readonly Paint paintPageFill;
        private readonly Paint paintStroke;
        private readonly Paint paintFill;
        private ViewPager viewPager;
        private ViewPager.IOnPageChangeListener listener;
        private int currentPage;
        private int snapPage;
        private int currentOffset;
        private int scrollState;
        private int pageSize;
        private int orientation;
        public bool IsCentered { get; set; }
        private bool snap;
        private const int InvalidPointer = -1;
        private readonly int touchSlop;
        private float lastMotionX = -1;
        private int activePointerId = InvalidPointer;
        private bool isDragging;

        public CirclePageIndicator(Context context) : this(context, null) { }

        public CirclePageIndicator(Context context, IAttributeSet attrs) : this(context, attrs, Resource.Attribute.vpiCirclePageIndicatorStyle) { }

        public CirclePageIndicator(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {

            var defaultPageColor = Resources.GetColor(Resource.Color.default_circle_indicator_page_color);
            var defaultFillColor = Resources.GetColor(Resource.Color.default_circle_indicator_fill_color);
            var defaultOrientation = Resources.GetInteger(Resource.Integer.default_circle_indicator_orientation);
            var defaultStrokeColor = Resources.GetColor(Resource.Color.default_circle_indicator_stroke_color);
            var defaultStrokeWidth = Resources.GetDimension(Resource.Dimension.default_circle_indicator_stroke_width);
            var defaultRadius = Resources.GetDimension(Resource.Dimension.default_circle_indicator_radius);
            var defaultCentered = Resources.GetBoolean(Resource.Boolean.default_circle_indicator_centered);
            var defaultSnap = Resources.GetBoolean(Resource.Boolean.default_circle_indicator_snap);

            //Retrieve styles attributes
            var styledAttributes = context.ObtainStyledAttributes(attrs, Resource.Styleable.CirclePageIndicator, defStyle, Resource.Style.Widget_CirclePageIndicator);

            IsCentered = styledAttributes.GetBoolean(Resource.Styleable.CirclePageIndicator_centered, defaultCentered);
            Orientation = styledAttributes.GetInt(Resource.Styleable.CirclePageIndicator_orientation, defaultOrientation);

            paintPageFill = new Paint(PaintFlags.AntiAlias);
            paintPageFill.SetStyle(Paint.Style.Fill);
            paintPageFill.Color = styledAttributes.GetColor(Resource.Styleable.CirclePageIndicator_pageColor, defaultPageColor);

            paintStroke = new Paint(PaintFlags.AntiAlias);
            paintStroke.SetStyle(Paint.Style.Stroke);
            paintStroke.Color = styledAttributes.GetColor(Resource.Styleable.CirclePageIndicator_strokeColor, defaultStrokeColor);
            paintStroke.StrokeWidth = styledAttributes.GetDimension(Resource.Styleable.CirclePageIndicator_strokeWidth, defaultStrokeWidth);

            paintFill = new Paint(PaintFlags.AntiAlias);
            paintFill.SetStyle(Paint.Style.Fill);
            paintFill.Color = styledAttributes.GetColor(Resource.Styleable.CirclePageIndicator_fillColor, defaultFillColor);

            radius = styledAttributes.GetDimension(Resource.Styleable.CirclePageIndicator_radius, defaultRadius);
            snap = styledAttributes.GetBoolean(Resource.Styleable.CirclePageIndicator_snap, defaultSnap);

            styledAttributes.Recycle();

            var configuration = ViewConfiguration.Get(context);
            touchSlop = ViewConfigurationCompat.GetScaledPagingTouchSlop(configuration);
        }

        public Color PageColor
        {
            get
            {
                return paintPageFill.Color;
            }
            set
            {
                paintPageFill.Color = value;
                Invalidate();
            }
        }

        public Color FillColor
        {
            get
            {
                return paintFill.Color;
            }
            set
            {
                paintFill.Color = value;
                Invalidate();
            }
        }

        public int Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                switch (value)
                {
                    case Horizontal:
                    case Vertical:
                        orientation = value;
                        UpdatePageSize();
                        RequestLayout();
                        break;
                    default:
                        throw new IllegalArgumentException("Orientation must be either HORIZONTAL or VERTICAL.");
                }
            }
        }

        public Color StrokeColor
        {
            get
            {
                return paintStroke.Color;
            }
            set
            {
                paintStroke.Color = value;
                Invalidate();
            }
        }

        public float StrokeWidth
        {
            get
            {
                return paintStroke.StrokeWidth;
            }
            set
            {
                paintStroke.StrokeWidth = value;
                Invalidate();
            }
        }

        public float Radius
        {
            get
            {
                return radius;
            }
            set
            {
                radius = value;
                Invalidate();
            }
        }

        public bool IsSnap
        {
            get
            {
                return snap;
            }
            set
            {
                snap = value;
                Invalidate();
            }
        }

        protected override void OnDraw(Canvas canvas)
        {

            base.OnDraw(canvas);

            int longSize;
            int longPaddingBefore;
            int longPaddingAfter;
            int shortPaddingBefore;

            float dX;
            float dY;

            if (viewPager == null)
            {
                return;
            }

            var count = viewPager.Adapter.Count;
            if (count == 0)
            {
                return;
            }

            if (currentPage >= count)
            {
                SetCurrentItem(count - 1);
                return;
            }

            if (Orientation == Horizontal)
            {
                longSize = Width;
                longPaddingBefore = PaddingLeft;
                longPaddingAfter = PaddingRight;
                shortPaddingBefore = PaddingTop;
            }
            else
            {
                longSize = Height;
                longPaddingBefore = PaddingTop;
                longPaddingAfter = PaddingBottom;
                shortPaddingBefore = PaddingLeft;
            }

            var threeRadius = Radius * 3;
            var shortOffset = shortPaddingBefore + Radius;
            var longOffset = longPaddingBefore + Radius;
            if (IsCentered)
            {
                longOffset += ((longSize - longPaddingBefore - longPaddingAfter) / 2.0f) - ((count * threeRadius) / 2.0f);
            }


            var pageFillRadius = radius;
            if (paintStroke.StrokeWidth > 0)
            {
                pageFillRadius -= paintStroke.StrokeWidth / 2.0f;
            }

            //Draw stroked circles
            for (var iLoop = 0; iLoop < count; iLoop++)
            {

                var drawLong = longOffset + (iLoop * threeRadius);
                if (Orientation == Horizontal)
                {
                    dX = drawLong;
                    dY = shortOffset;
                }
                else
                {
                    dX = shortOffset;
                    dY = drawLong;
                }

                // Only paint fill if not completely transparent
                if (paintPageFill.Alpha > 0)
                {
                    canvas.DrawCircle(dX, dY, pageFillRadius, paintPageFill);
                }

                // Only paint stroke if a stroke width was non-zero
                if (!Equals(pageFillRadius, Radius))
                {
                    canvas.DrawCircle(dX, dY, Radius, paintStroke);
                }
            }

            //Draw the filled circle according to the current scroll
            var cx = (IsSnap ? snapPage : currentPage) * threeRadius;

            if (!IsSnap && (pageSize != 0))
            {
                cx += (currentOffset * 1.0f / pageSize) * threeRadius;
            }

            if (Orientation == Horizontal)
            {
                dX = longOffset + cx;
                dY = shortOffset;
            }
            else
            {
                dX = shortOffset;
                dY = longOffset + cx;
            }
            canvas.DrawCircle(dX, dY, Radius, paintFill);
        }

        public override bool OnTouchEvent(MotionEvent ev)
        {

            float x;

            if (base.OnTouchEvent(ev))
            {
                return true;
            }
            if ((viewPager == null) || (viewPager.Adapter.Count == 0))
            {
                return false;
            }

            var action = ev.Action;
            switch (action & MotionEventActions.Mask)
            {
                case MotionEventActions.Down:
                    activePointerId = MotionEventCompat.GetPointerId(ev, 0);
                    lastMotionX = ev.GetX();
                    break;
                case MotionEventActions.Mask:
                    break;
                case MotionEventActions.Move:
                    var activePointerIndex = MotionEventCompat.FindPointerIndex(ev, activePointerId);
                    x = MotionEventCompat.GetX(ev, activePointerIndex);
                    var deltaX = x - lastMotionX;

                    if (!isDragging)
                    {
                        if (Math.Abs(deltaX) > touchSlop)
                        {
                            isDragging = true;
                        }
                    }

                    if (isDragging)
                    {
                        if (!viewPager.IsFakeDragging)
                        {
                            viewPager.BeginFakeDrag();
                        }
                        lastMotionX = x;
                        viewPager.FakeDragBy(deltaX);
                    }
                    break;
                case MotionEventActions.Pointer1Down:
                    int index = MotionEventCompat.GetActionIndex(ev);
                    x = MotionEventCompat.GetX(ev, index);
                    lastMotionX = x;
                    activePointerId = MotionEventCompat.GetPointerId(ev, index);
                    break;
                case MotionEventActions.Pointer1Up:
                    var pointerIndex = MotionEventCompat.GetActionIndex(ev);
                    var pointerId = MotionEventCompat.GetPointerId(ev, pointerIndex);
                    if (pointerId == activePointerId)
                    {
                        var newPointerIndex = pointerIndex == 0 ? 1 : 0;
                        activePointerId = MotionEventCompat.GetPointerId(ev, newPointerIndex);
                    }
                    lastMotionX = MotionEventCompat.GetX(ev, MotionEventCompat.FindPointerIndex(ev, activePointerId));
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    if (!isDragging)
                    {
                        var count = viewPager.Adapter.Count;
                        var width = Width;
                        var halfWidth = width / 2.0f;
                        var sixthWidth = width / 6.0f;

                        if ((currentPage > 0) && (ev.GetX() < halfWidth - sixthWidth))
                        {
                            viewPager.CurrentItem = currentPage - 1;
                            return true;
                        }
                        if ((currentPage < count - 1) && (ev.GetX() > halfWidth + sixthWidth))
                        {
                            viewPager.CurrentItem = currentPage + 1;
                            return true;
                        }
                    }

                    isDragging = false;
                    activePointerId = InvalidPointer;
                    if (viewPager.IsFakeDragging)
                    {
                        viewPager.EndFakeDrag();
                    }
                    break;
            }

            #region compat version
            //            switch ((int)action & MotionEventCompat.ActionMask)
            //            {
            //                case (int)MotionEventActions.Down:
            //                    activePointerId = MotionEventCompat.GetPointerId(ev, 0);
            //                    lastMotionX = ev.GetX();
            //                    break;
            //
            //                case (int)MotionEventActions.Move:
            //                    {
            //                        int activePointerIndex = MotionEventCompat.FindPointerIndex(ev, activePointerId);
            //                        x = MotionEventCompat.GetX(ev, activePointerIndex);
            //                        float deltaX = x - lastMotionX;
            //
            //                        if (!isDragging)
            //                        {
            //                            if (Java.Lang.Math.Abs(deltaX) > touchSlop)
            //                            {
            //                                isDragging = true;
            //                            }
            //                        }
            //
            //                        if (isDragging)
            //                        {
            //                            if (!viewPager.IsFakeDragging)
            //                            {
            //                                viewPager.BeginFakeDrag();
            //                            }
            //
            //                            lastMotionX = x;
            //
            //                            viewPager.FakeDragBy(deltaX);
            //                        }
            //
            //                        break;
            //                    }
            //
            //                case (int)MotionEventActions.Cancel:
            //                case (int)MotionEventActions.Up:
            //                    if (!isDragging)
            //                    {
            //                        int count = viewPager.Adapter.Count;
            //                        int width = Width;
            //                        float halfWidth = width / 2f;
            //                        float sixthWidth = width / 6f;
            //
            //                        if ((currentPage > 0) && (ev.GetX() < halfWidth - sixthWidth))
            //                        {
            //                            viewPager.CurrentItem = currentPage - 1;
            //                            return true;
            //                        }
            //                        else if ((currentPage < count - 1) && (ev.GetX() > halfWidth + sixthWidth))
            //                        {
            //                            viewPager.CurrentItem = currentPage + 1;
            //                            return true;
            //                        }
            //                    }
            //
            //                    isDragging = false;
            //                    activePointerId = InvalidPointer;
            //                    if (viewPager.IsFakeDragging)
            //                        viewPager.EndFakeDrag();
            //                    break;
            //
            //                case MotionEventCompat.ActionPointerDown:
            //                    {
            //                        int index = MotionEventCompat.GetActionIndex(ev);
            //                        x = MotionEventCompat.GetX(ev, index);
            //                        lastMotionX = x;
            //                        activePointerId = MotionEventCompat.GetPointerId(ev, index);
            //                        break;
            //                    }
            //
            //                case MotionEventCompat.ActionPointerUp:
            //                    int pointerIndex = MotionEventCompat.GetActionIndex(ev);
            //                    int pointerId = MotionEventCompat.GetPointerId(ev, pointerIndex);
            //                    if (pointerId == activePointerId)
            //                    {
            //                        int newPointerIndex = pointerIndex == 0 ? 1 : 0;
            //                        activePointerId = MotionEventCompat.GetPointerId(ev, newPointerIndex);
            //                    }
            //                    lastMotionX = MotionEventCompat.GetX(ev, MotionEventCompat.FindPointerIndex(ev, activePointerId));
            //                    break;
            //            }
            #endregion

            return true;
        }

        public void SetViewPager(ViewPager view, int initialPosition)
        {
            SetViewPager(view);
            SetCurrentItem(initialPosition);
        }

        public void SetViewPager(ViewPager view)
        {
            if (view.Adapter == null)
            {
                throw new IllegalStateException("ViewPager does not have adapter instance.");
            }
            viewPager = view;
            viewPager.SetOnPageChangeListener(this);
            UpdatePageSize();
            Invalidate();
        }

        public void SetCurrentItem(int item)
        {
            if (viewPager == null)
            {
                throw new IllegalStateException("ViewPager has not been bound.");
            }
            viewPager.CurrentItem = item;
            currentPage = item;
            Invalidate();
        }

        private void UpdatePageSize()
        {
            if (viewPager != null)
            {
                pageSize = (Orientation == Horizontal) ? viewPager.Width : viewPager.Height;
            }
        }

        public void NotifyDataSetChanged()
        {
            Invalidate();
        }

        public void OnPageScrollStateChanged(int state)
        {
            scrollState = state;

            if (listener != null)
            {
                listener.OnPageScrollStateChanged(state);
            }
        }

        public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
        {
            currentPage = position;
            currentOffset = positionOffsetPixels;
            UpdatePageSize();
            Invalidate();

            if (listener != null)
            {
                listener.OnPageScrolled(position, positionOffset, positionOffsetPixels);
            }
        }

        public void OnPageSelected(int position)
        {
            if (IsSnap || scrollState == ViewPager.ScrollStateIdle)
            {
                currentPage = position;
                snapPage = position;
                Invalidate();
            }

            if (listener != null)
            {
                listener.OnPageSelected(position);
            }
        }

        public void SetOnPageChangeListener(ViewPager.IOnPageChangeListener pageChangeListener)
        {
            listener = pageChangeListener;
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if (Orientation == Horizontal)
            {
                SetMeasuredDimension(MeasureLong(widthMeasureSpec), MeasureShort(heightMeasureSpec));
            }
            else
            {
                SetMeasuredDimension(MeasureShort(widthMeasureSpec), MeasureLong(heightMeasureSpec));
            }
        }

        /// <summary>
        /// Determines the width of this view
        /// </summary>
        /// <param name="measureSpec">The measure spec packed into an int.</param>
        /// <returns>The width of the view, honoring constraints from measureSpec</returns>
        private int MeasureLong(int measureSpec)
        {

            int result;
            var specMode = MeasureSpec.GetMode(measureSpec);
            var specSize = MeasureSpec.GetSize(measureSpec);

            if ((specMode == MeasureSpecMode.Exactly) || (viewPager == null))
            {
                result = specSize;
            }
            else
            {
                var count = viewPager.Adapter.Count;
                result = (int)(PaddingLeft + PaddingRight + (count * 2 * radius) + (count - 1) * radius + 1);
                //Respect AT_MOST value if that was what is called for by measureSpec
                if (specMode == MeasureSpecMode.AtMost)
                {
                    result = Math.Min(result, specSize);
                }
            }
            return result;
        }

        /// <summary>
        /// Determines the height of this view
        /// </summary>
        /// <param name="measureSpec">The measure spec packed into an int.</param>
        /// <returns>The height of the view, honoring constraints from measureSpec</returns>
        private int MeasureShort(int measureSpec)
        {
            int result;
            var specMode = MeasureSpec.GetMode(measureSpec);
            var specSize = MeasureSpec.GetSize(measureSpec);

            if (specMode == MeasureSpecMode.Exactly)
            {
                result = specSize;
            }
            else
            {
                result = (int)(2 * radius + PaddingTop + PaddingBottom + 1);
                //Respect AT_MOST value if that was what is called for by measureSpec
                if (specMode == MeasureSpecMode.AtMost)
                {
                    result = Math.Min(result, specSize);
                }
            }
            return result;
        }

        protected override void OnRestoreInstanceState(IParcelable state)
        {

            try
            {
                var savedState = (SavedState)state;
                base.OnRestoreInstanceState(savedState.SuperState);
                currentPage = savedState.CurrentPage;
                snapPage = savedState.CurrentPage;
            }
            catch
            {
                base.OnRestoreInstanceState(state);
            }
            RequestLayout();
        }

        protected override IParcelable OnSaveInstanceState()
        {
            var superState = base.OnSaveInstanceState();
            var savedState = new SavedState(superState)
            {
                CurrentPage = currentPage
            };
            return savedState;
        }
    }
}