/***************************************************************************************************************
	* ObjectJavaBox.cs
	* 
	* Copyright (c) 2015, Shahman Teh Sharifuddin
	* All rights reserved.     
	* 
**************************************************************************************************************/
namespace TabCarouselPage.Droid.Renderers
{
	internal sealed class ObjectJavaBox <T> : Java.Lang.Object
	{
		public T Instance { get; set; }

		public ObjectJavaBox ( T instance ) {
			Instance = instance;
		}
	}
}