using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinApp {
  [ContentProperty("Text")]
  public class TranslateExtension : IMarkupExtension {
    public string Text { get; set; }

    public string StringFormat { get; set; }

    public object ProvideValue(IServiceProvider serviceProvider) {
      if( string.IsNullOrEmpty(Text) ) return null;

      if( !string.IsNullOrEmpty(StringFormat) )
        return string.Format(StringFormat, Resource.ResourceManager.GetString(Text));

      return Resource.ResourceManager.GetString(Text);
    }
  }

  [ContentProperty(nameof(Source))]
  public class ImgResExtension : IMarkupExtension {
    public string Source { get; set; }

    public object ProvideValue(IServiceProvider serviceProvider) {
      if( Source == null ) return null;

      ImageSource imgsrc = ImageSource.FromResource($"XamarinApp.Images.{Source}", typeof(ImgResExtension).GetTypeInfo().Assembly);
      return imgsrc;
    }
  }
}
