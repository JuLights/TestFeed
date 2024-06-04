using Microsoft.Maui.Handlers;


#if __ANDROID__
using PlatformView = TestFeed.Platforms.Android.MauiCameraView;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace TestFeed
{
    internal partial class CameraViewHandler : ViewHandler<CameraView, PlatformView>
    {
        public static IPropertyMapper<CameraView, CameraViewHandler> PropertyMapper = new PropertyMapper<CameraView, CameraViewHandler>(ViewMapper)
        {
        };

        public static CommandMapper<CameraView, CameraViewHandler> CommandMapper = new(ViewCommandMapper)
        {
        };
        public CameraViewHandler() : base(PropertyMapper, CommandMapper)
        {
        }

        public CameraViewHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null) : base(mapper ?? PropertyMapper, commandMapper ?? CommandMapper)
        {

        }

#if __ANDROID__
        //protected override PlatformView CreatePlatformView() => new(Context, VirtualView);
        protected override PlatformView CreatePlatformView() => new();
#endif

        protected override void DisconnectHandler(PlatformView platformView)
        {
#if ANDROID
            //platformView.DisposeControl();
#endif
            base.DisconnectHandler(platformView);
        }

        protected override void ConnectHandler(PlatformView platformView)
        {

            base.ConnectHandler(platformView);

            //platformView.DisposeControl();
        }

    }
}
