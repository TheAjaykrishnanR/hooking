using System;
using System.Runtime.InteropServices;

public class HookerInjector
{
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern nint OpenProcess(uint dwDesiredAccess, bool bInheritHandles, uint dwProcessId);
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern int CloseHandle(nint handle);

	public static unsafe void Main()
	{
		Console.Write("processId: ");
		uint processId = Convert.ToUInt32(Console.ReadLine());
		const uint PROCESS_ALL_ACCESS = 0x1FFFFF;
		nint hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
		Console.WriteLine($"hProcess: {hProcess}");
		GetModulesInProcess(hProcess);
		CloseHandle(hProcess);
	}

	[DllImport("psapi.dll", SetLastError = true)]
	public static extern int EnumProcessModules(nint hProcess, [Out] nint ptrToModuleArray, int moduleArrayLength, [Out] nint sizeNeeded);

	public static unsafe void GetModulesInProcess(nint hProcess)
	{
		nint[] modules = new nint[100];
		uint sizeNeeded = 0;
		fixed (nint ptr = (nint)modules)
		{
			if (EnumProcessModules(hProcess, ptr, modules.Length * sizeof(nint), (nint)(&sizeNeeded)) == 0)
			{
				Console.WriteLine($"EnumProcessModules() failed, win32: {Marshal.GetLastWin32Error()}");
				return;
			}
		}

		Console.WriteLine("processes...");
		foreach (nint module in modules) Console.WriteLine($"m: {module}");
	}

	public static void Inject()
	{
		// 1. write hooker.dll to target process's memory 
		// 2. write "hooker.dll" string to target process memory
		// 3. call target's kernerl32!LoadLibrary with argument "hooker.dll"
		// 4. call hooker's Hook() function
	}
}
