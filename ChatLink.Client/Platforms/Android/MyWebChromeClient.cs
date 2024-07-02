using Android.Webkit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace ChatLink.Client.Platforms.Android
{
    internal class MyWebChromeClient : MauiWebChromeClient
    {
        public MyWebChromeClient(IWebViewHandler handler) : base(handler)
        {
        }

        public override void OnPermissionRequest(PermissionRequest request)
        {
            var resources = request.GetResources();
            bool videoPermissionGranted = true;
            bool audioPermissionGranted = true;

            foreach (var resource in resources)
            {
                if (resource.Equals(PermissionRequest.ResourceVideoCapture, StringComparison.OrdinalIgnoreCase))
                {
                    // Get the status of the .NET MAUI app's access to the camera
                    PermissionStatus status = Permissions.CheckStatusAsync<Permissions.Camera>().Result;

                    // Deny the web page's request if the app's access to the camera is not "Granted"
                    if (status != PermissionStatus.Granted)
                    {
                        videoPermissionGranted = false;
                    }
                }

                if (resource.Equals(PermissionRequest.ResourceAudioCapture, StringComparison.OrdinalIgnoreCase))
                {
                    // Get the status of the .NET MAUI app's access to the microphone
                    PermissionStatus status = Permissions.CheckStatusAsync<Permissions.Microphone>().Result;

                    // Deny the web page's request if the app's access to the microphone is not "Granted"
                    if (status != PermissionStatus.Granted)
                    {
                        audioPermissionGranted = false;
                    }
                }

            }

            if (videoPermissionGranted && audioPermissionGranted)
            {
                request.Grant(resources);
                return;
            }
            else
            {
                request.Deny();
            }

            base.OnPermissionRequest(request);
        }
    }


}
