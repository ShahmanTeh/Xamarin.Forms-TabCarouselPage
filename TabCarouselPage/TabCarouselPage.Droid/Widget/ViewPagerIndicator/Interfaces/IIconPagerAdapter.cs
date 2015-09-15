/***************************************************************************************************************
   * IIconPagerAdapter.cs
   * 
   * Copyright (c) 2015, Shahman Teh Sharifuddin
   * All rights reserved.     
   * 
**************************************************************************************************************/
namespace TabCarouselPage.Droid.Widget
{
    public interface IIconPagerAdapter
    {
        int GetIconResId ( int index );

        int Count { get; }
    }
}