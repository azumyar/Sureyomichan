using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;

namespace Haru.Kei.SureyomiChan; 
internal static class Interop {
	[StructLayout(LayoutKind.Sequential)]
	public struct COPYDATASTRUCT {
		public nint dwData;
		public int cbData;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpData;
	}
	public const int WM_COPYDATA = 0x4A;
	public const int WM_USER = 0x400;


	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	public static extern int SendMessage(nint hWnd, int Msg, nint wParam, nint lParam);
	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	public static extern int SendMessage(nint hWnd, int Msg, nint wParam, in COPYDATASTRUCT lParam);


	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]

	public static extern nint CreateMutex(nint lpMutexAttributes, bool bInitialOwner, string lpName);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern bool ReleaseMutex(nint hMutex);
	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern bool CloseHandle(nint hObject);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern int WaitForSingleObject(nint hHandle, int dwMilliseconds);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern nint CreateFileMapping(nint hFile, nint lpFileMappingAttributes, int flProtect, int dwMaximumSizeHigh, int dwMaximumSizeLow, string lpName);
	[DllImport("kernel32.dll")]
	public static extern nint MapViewOfFile(nint hFileMappingObject, int dwDesiredAccess, int dwFileOffsetHigh, int dwFileOffsetLow, nint dwNumberOfBytesToMap);
	[DllImport("kernel32.dll")]
	public static extern nint UnmapViewOfFile(nint hFileMappingObject);

	public const int PAGE_READWRITE = 0x04;
	public const int FILE_MAP_WRITE = 0x00000002;
	public const int FILE_MAP_READ = 0x00000004;
	public const int ERROR_ALREADY_EXISTS = 183;
}
