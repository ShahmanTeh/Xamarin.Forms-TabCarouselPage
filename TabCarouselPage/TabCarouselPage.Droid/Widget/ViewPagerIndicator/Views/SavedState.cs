/***************************************************************************************************************
   * SavedState.cs
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
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Interop;

namespace TabCarouselPage.Droid.Widget
{
    public class SavedState : View.BaseSavedState
    {
        public int CurrentPage { get; set; }

        public SavedState(IParcelable superState) : base(superState) { }

        public SavedState(Parcel parcel)
            : base(parcel)
        {
            CurrentPage = parcel.ReadInt();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteInt(CurrentPage);
        }

        [ExportField("CREATOR")]
        static SavedStateCreator InitializeCreator()
        {
            return new SavedStateCreator();
        }

        class SavedStateCreator : Java.Lang.Object, IParcelableCreator
        {
            public Java.Lang.Object CreateFromParcel(Parcel source)
            {
                return new SavedState(source);
            }

            public Java.Lang.Object[] NewArray(int size)
            {
                return new SavedState[size];
            }
        }
    }
}