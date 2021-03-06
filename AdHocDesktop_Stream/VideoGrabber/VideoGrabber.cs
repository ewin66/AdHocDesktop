/* <file>
 * <copyright see="prj:///doc/copyright.rtf"/>
 * <license see="prj:///doc/license.rtf"/>
 * <owner name="可愛龍" email="cute.ofdragon@gmail.com"/>
 * <date>2005/12/25</date>
 * <version value="$version"/>
 * <comment>  
 * </comment>
 * </file>
 */
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Threading;

using Microsoft.DirectX.DirectShow;

namespace Microsoft.DirectX.VideoGrabber
{
	internal class VideoGrabber : Control, ISampleGrabberCB
	{
		Thread grabberThread;
		int frame = 2;
		int interval = (1000 / 2);

		/// <summary>
		/// 每秒幾個框頁。
		/// </summary>
		[Description("每秒幾個框頁。")]
		public int Frame
		{
			get
			{
				return frame;
			}
			set
			{
				frame = value;
				interval = (1000 / frame);
			}
		}

		public VideoGrabber()
		{
		}

		public VideoGrabber(DsDevice device)
		{
			this.capDevice = device;
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				EndGrabber();
			}
			base.Dispose (disposing);
		}

		void Initialize()
		{
			if( firstActive )
			{
				throw new VideoGrabberException("已經呼叫過 BeginGrabber() 方法了！");
			}
			firstActive = true;

			if( ! DsUtils.IsCorrectDirectXVersion() )
			{
				EndGrabber();
				throw new VideoGrabberException("系統未安裝 DirectX 8.1 以後的版本！");
			}

			if( capDevice == null )
			{
                if ((capDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice)) == null)
                {
                    EndGrabber();
                    throw new VideoGrabberException("未偵測到視訊裝置！");
                }

                if (capDevices.Length >= 1)
                {
                    capDevice = capDevices[0] as DsDevice;
                }
                else
                {
                    DeviceSelector selector = new DeviceSelector();
                    selector.ShowDialog(this);
                    capDevice = selector.SelectedDevice;
                }
			}			

			if(capDevice == null)
			{
				EndGrabber();
				throw new VideoGrabberException("無法取得視訊裝置！");				
			}

			if( ! StartupVideo( capDevice.Mon ) )				
			{
				EndGrabber();
				throw new VideoGrabberException("無法初始化設定視訊裝置！");
			}

			grabberThread = new Thread(new ThreadStart(Processing));
			
		}

		/// <summary> handler for toolbar button clicks. </summary>
		private void Processing()
		{		
			do
			{
				int hr;
				if( sampGrabber == null )
					return;

				if( savedArray == null )
				{
					int size = videoInfoHeader.BmiHeader.ImageSize;
					if( (size < 1000) || (size > 16000000) )
						return;
					//savedArray = new byte[ size + 64000 ];
					savedArray = new byte[ size ];
				}

				captured = false;
				hr = sampGrabber.SetCallback( this, 1 );

				Thread.Sleep(interval);
			}while(true);
		}

		public void EndGrabber()
		{
			try
			{
				grabberThread.Abort();
			}
			catch {}
			CloseInterfaces();
		}		

		/// <summary> detect first form appearance, start grabber. </summary>
		public void BeginGrabber()
		{
			if(!isInit)
			{
				Initialize();
				isInit = true;
			}
			grabberThread.Start();
		}

		void OnBufferData(object sender, VideoGrabberBufferDataEventArgs e)
		{
			if(BufferData != null)
			{
				BufferData(sender, e);
			}
		}
		

		/// <summary> capture event, triggered by buffer callback. </summary>
		void OnCaptureDone()
		{
			Trace.WriteLine( "!!DLG: OnCaptureDone" );
			try 
			{
				int hr;
				if( sampGrabber == null )
				{
					return;
				}
				hr = sampGrabber.SetCallback( null, 0 );

				int w = videoInfoHeader.BmiHeader.Width;
				int h = videoInfoHeader.BmiHeader.Height;
				if( ((w & 0x03) != 0) || (w < 32) || (w > 4096) || (h < 32) || (h > 4096) )
				{
					return;
				}
				
				//get Image
				int stride = w * 3;
				GCHandle handle = GCHandle.Alloc( savedArray, GCHandleType.Pinned );
				int scan0 = (int) handle.AddrOfPinnedObject();
				scan0 += (h - 1) * stride;
				Bitmap b = new Bitmap( w, h, -stride, PixelFormat.Format24bppRgb, (IntPtr) scan0 );
				handle.Free();
                OnBufferData(this, new VideoGrabberBufferDataEventArgs(b));

                /*
				//savedArray = WMAsfBitmapFilter.Reverse(savedArray, 0, savedArray.Length, w);
				OnBufferData(this, new VideoGrabberBufferDataEventArgs(savedArray, w, h));				
			    */
				savedArray = null;			
			}
			catch (Exception)
			{
				//MessageBox.Show(e.Message);
			}
		}


		/// <summary> start all the interfaces, graphs and preview window. </summary>
		bool StartupVideo( UCOMIMoniker mon )
		{
			int hr;
			try 
			{
				if( ! CreateCaptureDevice( mon ) )
					return false;

				if( ! GetInterfaces() )
					return false;

				if( ! SetupGraph() )
					return false;

				if( ! SetupVideoWindow() )
					return false;

#if DEBUG
				//DsROT.AddGraphToRot( graphBuilder, out rotCookie );		// graphBuilder capGraph
				rot = new DsROTEntry( graphBuilder );
#endif
			
				hr = mediaCtrl.Run();
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );

				bool hasTuner = DsUtils.ShowTunerPinDialog( capGraph, capFilter, this.Handle );
				//tuneBtn.Enabled = hasTuner;

				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary> make the video preview window to show in videoPanel. </summary>
		bool SetupVideoWindow()
		{
			int hr;
			try 
			{
				// Set the video window to be a child of the main window
				hr = videoWin.put_Owner( this.Handle );
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );

				// Set video window style
				hr = videoWin.put_WindowStyle( WindowStyle.Child | WindowStyle.ClipChildren );
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );

				// Use helper function to position video window in client rect of owner window
				ResizeVideoWindow();

				// Make the video window visible, now that it is properly positioned
				hr = videoWin.put_Visible( OABool.True );
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );

				hr = mediaEvt.SetNotifyWindow( this.Handle, WM_GRAPHNOTIFY, IntPtr.Zero );
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );
				return true;
			}
			catch
			{
				return false;
			}
		}


		/// <summary> build the capture graph for grabber. </summary>
		bool SetupGraph()
		{
			int hr;
			try 
			{
				hr = capGraph.SetFiltergraph( graphBuilder );
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );

				hr = graphBuilder.AddFilter( capFilter, "Ds.NET Video Capture Device" );
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );

				//DsUtils.ShowCapPinDialog( capGraph, capFilter, this.Handle );

				AMMediaType media = new AMMediaType();
				media.majorType	= MediaType.Video;
				media.subType	= MediaSubType.RGB24;
				media.formatType = FormatType.VideoInfo;		// ???
				hr = sampGrabber.SetMediaType( media );
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );

				hr = graphBuilder.AddFilter( baseGrabFlt, "Ds.NET Grabber" );
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );

				Guid cat = PinCategory.Preview;
				Guid med = MediaType.Video;
				hr = capGraph.RenderStream( cat, med, capFilter, null, null ); // baseGrabFlt 
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );

				cat = PinCategory.Capture;
				med = MediaType.Video;
				hr = capGraph.RenderStream( cat, med, capFilter, null, baseGrabFlt ); // baseGrabFlt 
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );

				media = new AMMediaType();
				hr = sampGrabber.GetConnectedMediaType( media );
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );
				if( (media.formatType != FormatType.VideoInfo) || (media.formatPtr == IntPtr.Zero) )
					throw new NotSupportedException( "Unknown Grabber Media Format" );

				videoInfoHeader = (VideoInfoHeader) Marshal.PtrToStructure( media.formatPtr, typeof(VideoInfoHeader) );
				Marshal.FreeCoTaskMem( media.formatPtr ); media.formatPtr = IntPtr.Zero;

				hr = sampGrabber.SetBufferSamples( false );
				if( hr == 0 )
					hr = sampGrabber.SetOneShot( false );
				if( hr == 0 )
					hr = sampGrabber.SetCallback( null, 0 );
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );

				return true;
			}
			catch
			{
				return false;
			}
		}


		/// <summary> create the used COM components and get the interfaces. </summary>
		bool GetInterfaces()
		{			
			try 
			{
				graphBuilder = (IGraphBuilder) new FilterGraph();
				if(graphBuilder == null)
				{
					throw new COMException("Can't initialize FilterGraph instance.");
				}

				capGraph = (ICaptureGraphBuilder2) new CaptureGraphBuilder2();
				if(capGraph == null)
				{
					throw new COMException("Can't initialize CaptureGraphBuilder2 instance.");
				}

				sampGrabber = (ISampleGrabber) new SampleGrabber();
				if(sampGrabber == null)
				{
					throw new COMException("Can't initialize SampleGrabber instance.");
				}

				mediaCtrl	= (IMediaControl)	graphBuilder;
				videoWin	= (IVideoWindow)	graphBuilder;
				mediaEvt	= (IMediaEventEx)	graphBuilder;
				baseGrabFlt	= (IBaseFilter)		sampGrabber;
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
			}
		}

		/// <summary> create the user selected capture device. </summary>
		bool CreateCaptureDevice( UCOMIMoniker mon )
		{
			object capObj = null;
			try 
			{
				Guid gbf = typeof( IBaseFilter ).GUID;
				mon.BindToObject( null, null, ref gbf, out capObj );
				capFilter = (IBaseFilter) capObj; capObj = null;
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				if( capObj != null )
					Marshal.ReleaseComObject( capObj ); capObj = null;
			}
		}

		/// <summary> do cleanup and release DirectShow. </summary>
		void CloseInterfaces()
		{
			int hr;
			try 
			{
#if DEBUG
				//if( rotCookie != 0 )
				//	DsROT.RemoveGraphFromRot( ref rotCookie );
				rot.Dispose();
#endif

				if( mediaCtrl != null )
				{
					hr = mediaCtrl.Stop();
					mediaCtrl = null;
				}

				if( mediaEvt != null )
				{
					hr = mediaEvt.SetNotifyWindow( IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero );
					mediaEvt = null;
				}

				if( videoWin != null )
				{
					hr = videoWin.put_Visible( OABool.False );
					hr = videoWin.put_Owner( IntPtr.Zero );
					videoWin = null;
				}

				baseGrabFlt = null;
				if( sampGrabber != null )
					Marshal.ReleaseComObject( sampGrabber ); sampGrabber = null;

				if( capGraph != null )
					Marshal.ReleaseComObject( capGraph ); capGraph = null;

				if( graphBuilder != null )
					Marshal.ReleaseComObject( graphBuilder ); graphBuilder = null;

				if( capFilter != null )
					Marshal.ReleaseComObject( capFilter ); capFilter = null;
			
				if( capDevices != null )
				{
					foreach( DsDevice d in capDevices )
						d.Dispose();
					capDevices = null;
				}
			}
			catch
			{}
		}

		/// <summary> resize preview video window to fill client area. </summary>
		public void ResizeVideoWindow()
		{
			if( videoWin != null )
			{
				Rectangle rc = this.ClientRectangle;
				videoWin.SetWindowPosition( 0, 0, rc.Right, rc.Bottom );
			}
		}

		/// <summary> sample callback, NOT USED. </summary>
		int ISampleGrabberCB.SampleCB( double SampleTime, IMediaSample pSample )
		{
			Trace.WriteLine( "!!CB: ISampleGrabberCB.SampleCB" );
			return 0;
		}

		/// <summary> buffer callback, COULD BE FROM FOREIGN THREAD. </summary>
		int ISampleGrabberCB.BufferCB( double SampleTime, IntPtr pBuffer, int BufferLen )
		{
			if( captured || (savedArray == null) )
			{
				Trace.WriteLine( "!!CB: ISampleGrabberCB.BufferCB" );
				return 0;
			}

			captured = true;
			bufferedSize = BufferLen;
			Trace.WriteLine( "!!CB: ISampleGrabberCB.BufferCB  !GRAB! size = " + BufferLen.ToString() );
			if( (pBuffer != IntPtr.Zero) && (BufferLen > 1000) && (BufferLen <= savedArray.Length) )
				Marshal.Copy( pBuffer, savedArray, 0, BufferLen );
			else
				Trace.WriteLine( "    !!!GRAB! failed " );
			//this.BeginInvoke( new CaptureDone( this.OnCaptureDone ) );
			OnCaptureDone();
			return 0;
		}


		/// <summary> flag to detect first Form appearance </summary>
		private bool					firstActive;
		
		private bool					isInit;

		/// <summary> base filter of the actually used video devices. </summary>
		private IBaseFilter				capFilter;

		/// <summary> graph builder interface. </summary>
		private IGraphBuilder			graphBuilder;

		/// <summary> capture graph builder interface. </summary>
		private ICaptureGraphBuilder2	capGraph;
		private ISampleGrabber			sampGrabber;

		/// <summary> control interface. </summary>
		private IMediaControl			mediaCtrl;

		/// <summary> event interface. </summary>
		private IMediaEventEx			mediaEvt;

		/// <summary> video window interface. </summary>
		private IVideoWindow			videoWin;

		/// <summary> grabber filter interface. </summary>
		private IBaseFilter				baseGrabFlt;

		/// <summary> structure describing the bitmap to grab. </summary>
		private	VideoInfoHeader			videoInfoHeader;
		private	bool					captured = true;
		private	int						bufferedSize;

		/// <summary> buffer for bitmap data. </summary>
		private	byte[]					savedArray;

		/// <summary> list of installed video devices. </summary>
		private DsDevice[]				capDevices;		

		private DsDevice				capDevice;

		private const int WM_GRAPHNOTIFY	= 0x00008001;	// message from graph

		private const int WS_CHILD			= 0x40000000;	// attributes for video window
		private const int WS_CLIPCHILDREN	= 0x02000000;
		private const int WS_CLIPSIBLINGS	= 0x04000000;		
		
		public event VideoGrabberBufferDataEventHandler BufferData;

#if DEBUG
		//private int		rotCookie = 0;
		private DsROTEntry				rot;
#endif		

	}
}
