using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Haru.Kei.SureyomiChan.Helpers;

internal class StartupSequence {
	private const string MutexName = "sureyomichan-mutex";
	private const string FileMappingName = "sureyomichan-mapping";

	private nint hMapObj;
	private nint mutex;

	public (bool IsFirstStart, nint Window) Begin() {
		mutex = Interop.CreateMutex(0, false, MutexName);
		Interop.WaitForSingleObject(mutex, -1);

		hMapObj = Interop.CreateFileMapping(
			-1, 0, Interop.PAGE_READWRITE,
			0, 8,
			FileMappingName);
		if(Marshal.GetLastWin32Error() == Interop.ERROR_ALREADY_EXISTS) {
			var ptr = Interop.MapViewOfFile(hMapObj, Interop.FILE_MAP_READ, 0, 0, IntPtr.Zero);
			var hwnd = Marshal.ReadIntPtr(ptr, 0);
			Interop.UnmapViewOfFile(ptr);

			return (false, hwnd);
		} else {
			return (true, 0);
		}
	}

	public void End(nint hwnd) {
		if(hwnd != 0) {
			var ptr = Interop.MapViewOfFile(hMapObj, Interop.FILE_MAP_WRITE, 0, 0, IntPtr.Zero);
			Marshal.WriteIntPtr(ptr, hwnd);
			Interop.UnmapViewOfFile(ptr);
		}
		if(mutex != 0) {
			Interop.ReleaseMutex(mutex);
		}

		if(hwnd == 0) {
			if(hMapObj != 0) {
				Interop.CloseHandle(hMapObj);
			}
			if(mutex != 0) {
				Interop.CloseHandle(mutex);
			}
			hMapObj = 0;
			mutex = 0;
		}
	}
}
