using CoreGraphics;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Platform;
using WebKit;

[assembly: ExportRenderer(typeof(MyWebView), typeof(MyWebViewRenderer))]
namespace ChatLink.Client.Platforms.iOS
{
    public class MyWebViewRenderer : WkWebViewRenderer
    {
        public MyWebViewRenderer(CGRect frame, WKWebViewConfiguration config) : base(frame, config)
        {
        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
            if (Configuration != null)
            {
                Configuration.MediaTypesRequiringUserActionForPlayback = WKAudiovisualMediaTypes.None;
            }
        }
    }
}
