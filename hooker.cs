using System;
using System.Runtime.InteropServices;
using Vanara.PInvoke;
using static Vanara.PInvoke.Kernel32;
//using static Vanara.PInvoke.User32;

public class Hooker
{
	[DllImport("detours64.dll", SetLastError = true)]
	public static extern uint DetourRestoreAfterWith();
	[DllImport("detours64.dll", SetLastError = true)]
	public static extern uint DetourTransactionBegin();
	[DllImport("detours64.dll", SetLastError = true)]
	public static extern uint DetourUpdateThread(nint hThread);
	[DllImport("detours64.dll", SetLastError = true)]
	public static extern uint DetourAttach(nint ppPointer, nint pDetour);
	[DllImport("detours64.dll", SetLastError = true)]
	public static extern uint DetourTransactionCommit();

	public static void HandleError(uint code)
	{
		if (code != 0) throw new Exception($"win32 error: {code}");
	}

	public delegate int MessageBox_Delegate(nint hWnd, nint lpText, nint lpCaption, uint uType);
	public static unsafe int MessageBox_Hook(nint hWnd, nint lpText, nint lpCaption, uint uType)
	{
		Console.WriteLine("No message box for you !");
		return 0;
	}

	// where the target function is hooked
	public static unsafe void Hook()
	{
		nint hookFnPtr = Marshal.GetFunctionPointerForDelegate<MessageBox_Delegate>(MessageBox_Hook);

		// Find the target function (pointer) through some means eg GetProcAdress() (exported functions) or IDA Pro
		nint dllBase = (nint)LoadLibrary("user32.dll");
		nint targetFnPtr = GetProcAddress(dllBase, "MessageBoxA");

		// init detour and hook
		HandleError(DetourRestoreAfterWith());
		HandleError(DetourTransactionBegin());
		HandleError(DetourUpdateThread((nint)GetCurrentThread()));
		HandleError(DetourAttach((nint)(&targetFnPtr), hookFnPtr));
		HandleError(DetourTransactionCommit());
	}

	// for calling the target function to see if our hook captures it
	[DllImport("user32.dll")]
	public static extern int MessageBoxA(nint hWnd, string text, string caption, uint type);

	// comment out when building dll
	public static unsafe void Main()
	{
		Hook();
		MessageBoxA(0, "hello", "message", (uint)0x00000000L);
	}
}
