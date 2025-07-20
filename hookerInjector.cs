using System;

public class HookerInjector
{
	public static void Main()
	{
		Hooker.Hook();
	}

	public void Inject()
	{
		// 1. write hooker.dll to target process's memory 
		// 2. write "hooker.dll" string to target process memory
		// 3. call target's kernerl32!LoadLibrary with argument "hooker.dll"
		// 4. call hooker's Hook() function
	}
}
