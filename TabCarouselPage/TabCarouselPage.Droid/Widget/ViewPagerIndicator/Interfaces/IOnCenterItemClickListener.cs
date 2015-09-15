/***************************************************************************************************************
   * IOnCenterItemClickListener.cs
   * 
   * Copyright (c) 2015, Shahman Teh Sharifuddin
   * All rights reserved.     
   * 
**************************************************************************************************************/
namespace TabCarouselPage.Droid.Widget
{
    /// <summary>
    /// Interface for a callback when the center item has been clicked.
    /// </summary>
    public interface IOnCenterItemClickListener
    {
        /// <summary>
        /// Callback when the center item has been clicked.
        /// </summary>
        /// <param name="position">Position of the current center item..</param>
        void OnCenterItemClick ( int position );
    }
}