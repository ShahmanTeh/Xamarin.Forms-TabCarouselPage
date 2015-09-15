/***************************************************************************************************************
   * TabCarouselPage.cs
   * 
   * Copyright (c) 2015, Shahman Teh Sharifuddin
   * All rights reserved.     
   * 
**************************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace TabCarouselPage.Core
{
    public class TabCarouselPage : CarouselPage
    {
        public ETabType TabType { get; protected set; }

        public TabCarouselPage(ETabType tabType = ETabType.TitleWithIcon)
        {
            TabType = tabType;
        }
    }
}
