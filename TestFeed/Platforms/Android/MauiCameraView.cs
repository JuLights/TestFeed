using Android.Content;
using Android.Graphics;
using Android.Views;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.CardView.Widget;
using AndroidX.Core.Content;
using Google.Common.Util.Concurrent;
using Java.Util.Concurrent;
using Java.Lang;
using Size = Android.Util.Size;
using AndroidX.Lifecycle;
using Android.OS;
using Android.Provider;
using Java.IO;
using Java.Net;
using Environment = Android.OS.Environment;
using static Android.Graphics.Bitmap;
using System.Reflection.PortableExecutable;

namespace TestFeed.Platforms.Android
{
    internal class MauiCameraView : CardView, ImageAnalysis.IAnalyzer, IDisposable
    {
        private static readonly Context _context = Platform.AppContext;
        private PreviewView? _viewFinder;
        private Preview? _preview;
        private ICamera? _camera;
        private ProcessCameraProvider? _cameraProvider;
        private ImageAnalysis? _imageAnalysis;
        private ImageAnalysis.IAnalyzer analyzer;

        public Size DefaultTargetResolution => new Size(200, 200);

        public MauiCameraView() : base(_context)
        {
            StartCameraPreview();
        }

        public static int counter = 0;
        public async void Analyze(IImageProxy image)
        {
            var img = image;
            // Process the image frame here
            // we can save bitmap, but i can't :D
            try
            {
                var buffer = image.GetPlanes()[0].Buffer;
                var data = new byte[buffer.Capacity()];
                buffer.Get(data);
                var bitmap = await BitmapFactory.DecodeByteArrayAsync(data, 0, data.Length);
                // process the image
                var path = System.IO.Path.Combine(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures).AbsolutePath,$"newimg{counter}.jpg");
                //OutputStream fOut = null;
                var fs = new FileStream(path, FileMode.Create);

                if (fs != null)
                {
                    await bitmap?.CompressAsync(CompressFormat.Jpeg, 90, fs); // not working idk why!
                    fs.Flush();
                    fs.Close();
                }


                counter++;
                // must!!!
                image.Close();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Analyze exception: {ex.Message}");
                image.Close();
            }
        }

        private void StartCameraPreview()
        {
            try
            {
                IListenableFuture? cameraProviderFuture = ProcessCameraProvider.GetInstance(_context);

                cameraProviderFuture.AddListener(new Runnable(() =>
                {
                    try
                    {
                        _cameraProvider = (ProcessCameraProvider?)cameraProviderFuture.Get();
                        if (_cameraProvider is not null)
                        {
                            _cameraProvider.UnbindAll();


                            _viewFinder = new PreviewView(_context)
                            {
                                LayoutParameters = new LayoutParams(Width, Height),
                            };
                            _viewFinder.SetScaleType(PreviewView.ScaleType.FillCenter);
                            AddView(_viewFinder);

                            //Refer to Android documentation to retrieve cameras that are not the default back camera
                            if (_cameraProvider.HasCamera(CameraSelector.DefaultBackCamera) is true && _viewFinder is not null)
                            {


                                _preview = new Preview.Builder()
                                            .SetTargetResolution(new Size(1080, 1920))
                                            .SetCameraSelector(CameraSelector.DefaultBackCamera)
                                            .Build();


                                _preview.SetSurfaceProvider(_viewFinder.SurfaceProvider);

                                _imageAnalysis = new ImageAnalysis.Builder()
                                .SetTargetResolution(new Size(1280, 720))
                                .SetBackpressureStrategy(ImageAnalysis.StrategyKeepOnlyLatest)
                                .Build();

                                _imageAnalysis.SetAnalyzer(ContextCompat.GetMainExecutor(_context), this);

                                ILifecycleOwner? owner = Platform.CurrentActivity as ILifecycleOwner;

                                //initialize back camera
                                _camera = _cameraProvider.BindToLifecycle(owner, CameraSelector.DefaultBackCamera, _preview, _imageAnalysis);


                                //start the camera with AutoFocus
                                MeteringPoint point = new SurfaceOrientedMeteringPointFactory(1f, 1f).CreatePoint(0, 5f, 0.5f);
                                FocusMeteringAction action = new FocusMeteringAction.Builder(point, FocusMeteringAction.FlagAf + FocusMeteringAction.FlagAe + FocusMeteringAction.FlagAwb)
                                                                                    .SetAutoCancelDuration(1, TimeUnit.Seconds!)
                                                                                    .Build();
                                _camera.CameraControl.StartFocusAndMetering(action);
                            }
                        }

                    }
                    catch (System.Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine($"{e.GetType()}, {e.Message}");
                    }
                }), ContextCompat.GetMainExecutor(_context));
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"{e.GetType()}, {e.Message}");
            }
        }

        public new void Dispose()
        {
            _cameraProvider?.UnbindAll();
            _viewFinder?.Dispose();
            _imageAnalysis?.ClearAnalyzer();
            base.Dispose();
        }

    }
}
