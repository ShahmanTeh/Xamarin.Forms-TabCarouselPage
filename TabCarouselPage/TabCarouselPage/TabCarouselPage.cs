/***************************************************************************************************************
	* TabCarouselPage.cs
	* 
	* Copyright (c) 2015, Shahman Teh Sharifuddin
	* All rights reserved.     
	* 
**************************************************************************************************************/

using Xamarin.Forms;

namespace TabCarouselPage.Core
{
	public class TabCarouselPage : CarouselPage
	{
		public TabType TabType { get; protected set; }

	    public TabCarouselPage ( TabType tabType = TabType.TitleWithIcon ) {
	        TabType = tabType;
	    }
	}
}
