public static class Gear_Program
{
	public static string[] Loading_Screen_Prepare() => new string[] { "font.spritefont" };
	public static void Each_Loading_Screen_Update(int percent_loaded) { }
	public static void Each_Tick()
	{
		if (Gear_System.System.Ticks_Count_Get() == 1)
		{
			Gear_System.System.Window_Show(false);
			//Gear_System.Network.Server_Start();
			Gear_System.Network.Client_Connect("test");
		}
		Gear_System.Network.Clinet_Message_Send_To_All(Gear_System.Network.Console_Read());
	}
}