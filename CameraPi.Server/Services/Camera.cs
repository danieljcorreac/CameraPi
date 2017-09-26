using System;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Buffer = Windows.Storage.Streams.Buffer;

namespace CameraPi.Server.Services
{
	public class Camera
	{
		private IBuffer _buffer;
		private MediaCapture _mediaCapture;
		private VideoFrame _previewFrame;
		private StreamSocketClient _streamSocketClient;
		private StreamSocketListenerServer _streamSocketServer;

		private DispatcherTimer _timer;

		private int _timerTickCompleteFlag;
		private VideoFrame _videoFrame;

		public void Emit()
		{
			InitializeCamera();
			InitSocket();
		}

		private async void InitializeCamera()
		{
			var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
			var cameraDevice = allVideoDevices.FirstOrDefault();
			var mediaInitSettings = new MediaCaptureInitializationSettings {VideoDeviceId = cameraDevice.Id};
			_mediaCapture = new MediaCapture();

			try
			{
				await _mediaCapture.InitializeAsync(mediaInitSettings);
			}
			catch (UnauthorizedAccessException)
			{

			}

			var captureElementDummy = new CaptureElement {Source = _mediaCapture};

			await _mediaCapture.StartPreviewAsync();

			_videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, 240, 180, 0);
			_buffer = new Buffer(240 * 180 * 8);
		}

		private async void InitSocket()
		{
			_streamSocketServer = new StreamSocketListenerServer();
			await _streamSocketServer.Start("22333");

			_streamSocketClient = new StreamSocketClient();

			_timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(100)
			};
			_timer.Tick += TimerTick;
			_timer.Start();
		}

		private async void TimerTick(object sender, object e)
		{
			if (_timerTickCompleteFlag == 1)
				return;
			_timerTickCompleteFlag = 1;

			/*  stream client */
			try
			{
				if (_streamSocketClient.FlagClientStart == 0)
				{
					if (_streamSocketServer.ReceiveClientIp == 1)
						await _streamSocketClient.Start(_streamSocketServer.StringTemp, "22343");
				}
				else
				{
					if (_mediaCapture.CameraStreamState == CameraStreamState.Streaming)
					{
						_previewFrame = await _mediaCapture.GetPreviewFrameAsync(_videoFrame);
						_previewFrame.SoftwareBitmap.CopyToBuffer(_buffer);
						await _streamSocketClient.SendBuffer(_buffer);
					}
				}
			}
			catch (Exception)
			{
			}

			_timerTickCompleteFlag = 0;
		}
	}
}