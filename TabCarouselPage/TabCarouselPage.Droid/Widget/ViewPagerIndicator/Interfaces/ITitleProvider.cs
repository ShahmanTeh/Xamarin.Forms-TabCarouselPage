/***************************************************************************************************************
   * ITitleProvider.cs
   * 
   * Copyright (c) 2015, Shahman Teh Sharifuddin
   * All rights reserved.     
   * 
**************************************************************************************************************/
using System;

namespace TabCarouselPage.Droid.Widget
{
    /// <summary>
    /// A TitleProvider provides the title to display according to a view.
    /// </summary>
    public interface ITitleProvider
    {

        /// <summary>
        /// Returns the title of the view at position
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        String GetTitle(int position);
    }
}