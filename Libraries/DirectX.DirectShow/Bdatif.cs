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

using System.Runtime.InteropServices;

namespace Microsoft.DirectX.DirectShow.BDA
{

	#region Declarations

#if ALLOW_UNTESTED_INTERFACES
	[ComImport, Guid("14EB8748-1753-4393-95AE-4F7E7A87AAD6")]
	public class TIFLoad
	{
	}
#endif

	#endregion

	#region Interfaces

#if ALLOW_UNTESTED_INTERFACES

	[Guid("DFEF4A68-EE61-415f-9CCB-CD95F2F98A3A"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IBDA_TIF_REGISTRATION
	{
		[PreserveSig]
		int RegisterTIFEx(
			[In] IPin pTIFInputPin,
			[In, Out] ref int ppvRegistrationContext,
			[In, Out, MarshalAs(UnmanagedType.Interface)] ref object ppMpeg2DataControl
			);

		[PreserveSig]
		int UnregisterTIF([In] int pvRegistrationContext);
	}

	[Guid("F9BAC2F9-4149-4916-B2EF-FAA202326862"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMPEG2_TIF_CONTROL
	{
		[PreserveSig]
		int RegisterTIF(
			[In, MarshalAs(UnmanagedType.Interface)] object pUnkTIF,
			[In, Out] ref int ppvRegistrationContext
			);

		[PreserveSig]
		int UnregisterTIF([In] int pvRegistrationContext);

		[PreserveSig]
		int AddPIDs(
			[In] int ulcPIDs,
			[In] ref int pulPIDs
			);

		[PreserveSig]
		int DeletePIDs(
			[In] int ulcPIDs,
			[In] ref int pulPIDs
			);

		[PreserveSig]
		int GetPIDCount([Out] out int pulcPIDs);

		[PreserveSig]
		int GetPIDs(
			[Out] out int pulcPIDs,
			[Out] out int pulPIDs
			);
	}

	[Guid("A3B152DF-7A90-4218-AC54-9830BEE8C0B6"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ITuneRequestInfo
	{
		[PreserveSig]
		int GetLocatorData([In] ITuneRequest Request);

		[PreserveSig]
		int GetComponentData([In] ITuneRequest CurrentRequest);

		[PreserveSig]
		int CreateComponentList([In] ITuneRequest CurrentRequest);

		[PreserveSig]
		int GetNextProgram(
			[In] ITuneRequest CurrentRequest,
			[Out] out ITuneRequest TuneRequest
			);

		[PreserveSig]
		int GetPreviousProgram(
			[In] ITuneRequest CurrentRequest,
			[Out] out ITuneRequest TuneRequest
			);

		[PreserveSig]
		int GetNextLocator(
			[In] ITuneRequest CurrentRequest,
			[Out] out ITuneRequest TuneRequest
			);

		[PreserveSig]
		int GetPreviousLocator(
			[In] ITuneRequest CurrentRequest,
			[Out] out ITuneRequest TuneRequest
			);
	}

	[Guid("EFDA0C80-F395-42c3-9B3C-56B37DEC7BB7"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IGuideDataEvent
	{
		[PreserveSig]
		int GuideDataAcquired();

		[PreserveSig]
		int ProgramChanged([In] object varProgramDescriptionID);

		[PreserveSig]
		int ServiceChanged([In] object varProgramDescriptionID);

		[PreserveSig]
		int ScheduleEntryChanged([In] object varProgramDescriptionID);

		[PreserveSig]
		int ProgramDeleted([In] object varProgramDescriptionID);

		[PreserveSig]
		int ServiceDeleted([In] object varProgramDescriptionID);

		[PreserveSig]
		int ScheduleDeleted([In] object varProgramDescriptionID);
	}

	[Guid("88EC5E58-BB73-41d6-99CE-66C524B8B591"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IGuideDataProperty
	{
		[PreserveSig]
		int get_Name([Out, MarshalAs(UnmanagedType.BStr)] out string pbstrName);

		[PreserveSig]
		int get_Language([Out] out int idLang);

		[PreserveSig]
		int get_Value([Out] out object pvar);
	}

	[Guid("AE44423B-4571-475c-AD2C-F40A771D80EF"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEnumGuideDataProperties
	{
		[PreserveSig]
		int Next(
			[In] int celt,
			[Out] out IGuideDataProperty ppprop,
			[Out] out int pcelt
			);

		[PreserveSig]
		int Skip([In] int celt);

		[PreserveSig]
		int Reset();

		[PreserveSig]
		int Clone([Out] out IEnumGuideDataProperties ppenum);
	}

	[Guid("1993299C-CED6-4788-87A3-420067DCE0C7"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEnumTuneRequests
	{
		[PreserveSig]
		int Next(
			[In] int celt,
			[Out] out ITuneRequest ppprop,
			[Out] out int pcelt
			);

		[PreserveSig]
		int Skip([In] int celt);

		[PreserveSig]
		int Reset();

		[PreserveSig]
		int Clone([Out] out IEnumTuneRequests ppenum);
	}

	[Guid("61571138-5B01-43cd-AEAF-60B784A0BF93"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IGuideData
	{
		[PreserveSig]
		int GetServices([Out] out IEnumTuneRequests ppEnumTuneRequests);

		[PreserveSig]
		int GetServiceProperties(
			[In] ITuneRequest pTuneRequest,
			[Out] out IEnumGuideDataProperties ppEnumProperties
			);

		[PreserveSig]
		int GetGuideProgramIDs([Out] out UCOMIEnumVARIANT pEnumPrograms);

		[PreserveSig]
		int GetProgramProperties(
			[In] object varProgramDescriptionID,
			[Out] out IEnumGuideDataProperties ppEnumProperties
			);

		[PreserveSig]
		int GetScheduleEntryIDs([Out] out UCOMIEnumVARIANT pEnumScheduleEntries);

		[PreserveSig]
		int GetScheduleEntryProperties(
			[In] object varScheduleEntryDescriptionID,
			[Out] out IEnumGuideDataProperties ppEnumProperties
			);
	}

	[Guid("4764ff7c-fa95-4525-af4d-d32236db9e38"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IGuideDataLoader
	{
		[PreserveSig]
		int Init([In] IGuideData pGuideStore);

		[PreserveSig]
		int Terminate();
	}

#endif

	#endregion
}