using System;
using System.Runtime.InteropServices;
using Vanara.PInvoke;
using static Vanara.PInvoke.Kernel32;
//using static Vanara.PInvoke.User32;

public class Hooker
{
	[DllImport("detours64.dll", SetLastError = true)]
	public static extern int DetourRestoreAfterWith();
	[DllImport("detours64.dll", SetLastError = true)]
	public static extern uint DetourTransactionBegin();
	[DllImport("detours64.dll", SetLastError = true)]
	public static extern uint DetourUpdateThread(nint hThread);
	[DllImport("detours64.dll", SetLastError = true)]
	public static extern uint DetourAttach(nint ppPointer, nint pDetour);
	[DllImport("detours64.dll", SetLastError = true)]
	public static extern uint DetourTransactionCommit();

	//public delegate int GetCursorPos_Delegate(nint cursorPos);
	//public static unsafe int GetCursorPos_Hook(nint cursorPos)
	//{
	//	Console.WriteLine("fuck you calling ? this is a hook");
	//	return 0;
	//}
	public delegate int MessageBox_Delegate(nint hWnd, nint lpText, nint lpCaption, uint uType);
	public static unsafe int MessageBox_Hook(nint hWnd, nint lpText, nint lpCaption, uint uType)
	{
		Console.WriteLine("No message box for you !");
		return 0;
	}

	// where the target function is hooked
	public static unsafe void Hook()
	{
		Console.WriteLine("Hooking");

		// 1. Find the target function (pointer) through some means eg GetProcAdress() (exported functions) or IDA Pro
		nint dllBase = (nint)LoadLibrary("user32.dll");
		nint targetFnPtr = GetProcAddress(dllBase, "MessageBoxA");
		Console.WriteLine($"targetFnPtr: {targetFnPtr}");

		// 2. init detour 
		Console.WriteLine(DetourRestoreAfterWith());
		Console.WriteLine($"win32error: {Marshal.GetLastWin32Error()}");
		Console.WriteLine(DetourTransactionBegin());
		Console.WriteLine($"win32error: {Marshal.GetLastWin32Error()}");
		nint hThread = (nint)GetCurrentThread();
		Console.WriteLine(DetourUpdateThread(hThread));
		Console.WriteLine($"win32error: {Marshal.GetLastWin32Error()}, hThread: {hThread}");
		nint hookFnPtr = Marshal.GetFunctionPointerForDelegate<MessageBox_Delegate>(MessageBox_Hook);
		Console.WriteLine(DetourAttach((nint)(&targetFnPtr), hookFnPtr));
		Console.WriteLine($"win32error: {Marshal.GetLastWin32Error()}, hookFnPtr: {hookFnPtr}");
		Console.WriteLine(DetourTransactionCommit());
		Console.WriteLine($"win32error: {Marshal.GetLastWin32Error()}");
	}

	[DllImport("user32.dll")]
	public static extern int MessageBoxA(nint hWnd, string text, string caption, uint type);

	// comment out when building dll
	public static unsafe void Main()
	{
		Hook();
		//GetCursorPos(out POINT cursorPos);
		MessageBoxA(0, "hello", "message", (uint)0x00000000L);
		//Console.ReadKey();
	}
}
