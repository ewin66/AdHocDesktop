#region license

/*
DirectShowLib - Provide access to DirectShow interfaces via .NET
Copyright (C) 2005
http://sourceforge.net/projects/directshownet/

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Microsoft.DirectX.DirectShow
{

	#region Declarations

#if ALLOW_UNTESTED_INTERFACES

	/// <summary>
	/// From MIXER_DATA_* defines
	/// </summary>
	[Flags]
	public enum MixerData
	{
		AspectRatio = 0x00000001, // picture aspect ratio changed
		NativeSize = 0x00000002, // native size of video changed
		Palette = 0x00000004 // palette of video changed
	}

	/// <summary>
	/// #define MIXER_STATE_* defines
	/// </summary>
	public enum MixerState
	{
		Mask = 0x00000003, // use this mask with state status bits
		Unconnected = 0x00000000, // mixer is unconnected and stopped
		ConnectedStopped = 0x00000001, // mixer is connected and stopped
		ConnectedPaused = 0x00000002, // mixer is connected and paused
		ConnectedPlaying = 0x00000003 // mixer is connected and playing
	}

#endif

	#endregion

	#region Interfaces

#if ALLOW_UNTESTED_INTERFACES

	[Guid("81A3BD31-DEE1-11d1-8508-00A0C91F9CA0"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMixerOCXNotify
	{
		[PreserveSig]
		int OnInvalidateRect([In] Rectangle lpcRect);

		[PreserveSig]
		int OnStatusChange([In] int ulStatusFlags);

		[PreserveSig]
		int OnDataChange([In] int ulDataFlags);
	}

	[Guid("81A3BD32-DEE1-11d1-8508-00A0C91F9CA0"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMixerOCX
	{
		[PreserveSig]
		int OnDisplayChange(
			[In] int ulBitsPerPixel,
			[In] int ulScreenWidth,
			[In] int ulScreenHeight
			);

		[PreserveSig]
		int GetAspectRatio(
			[Out] out int pdwPictAspectRatioX,
			[Out] out int pdwPictAspectRatioY
			);

		[PreserveSig]
		int GetVideoSize(
			[Out] out int pdwVideoWidth,
			[Out] out int pdwVideoHeight
			);

		[PreserveSig]
		int GetStatus([Out] out int pdwStatus);

		[PreserveSig]
		int OnDraw(
			[In] IntPtr hdcDraw, // HDC
			[In] Rectangle prcDraw
			);

		[PreserveSig]
		int SetDrawRegion(
			[In] Point lpptTopLeftSC,
			[In] Rectangle prcDrawCC,
			[In] Rectangle lprcClip
			);

		[PreserveSig]
		int Advise([In] IMixerOCXNotify pmdns);

		[PreserveSig]
		int UnAdvise();
	}
#endif

	#endregion
}