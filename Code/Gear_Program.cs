public static class Gear_Program
{
	public static string[] Loading_Screen_Prepare() => new string[] { "font.spritefont" };
	public static void Each_Loading_Screen_Update(int percent_loaded) { }
	public static void Each_Tick()
	{
		if (Gear_System.System.Ticks_Count_Get() == 1)
		{
			Gear_System.System.Window_Show(false);
			Gear_System.Multiplayer.Server_Start();
			Gear_System.Multiplayer.Client_Connect("test");
			/*
			Gear_System.System.Canvas_Pixel_Size_Set(10, 10);
			var body = Gear_System.Body.Create();
			body.Sprite_Set("test", width: 538, height: 592, origin_x: 0, origin_y: 0, grid_size: 2, index_h: 0);
			body.Size_Set(128, 128);
			body.Angle_Show(true, false, 0, 255, 0);
			body.Boundaries_Show(true, false, 255, 0, 0);
			body.Origin_Show(true, false, 0, 0, 255);
			body.Position_Set(0, 0);
			Gear_System.System.Console_Write("font", $"{Gear_System.System.Canvas_Background_Blue_Get()}\n", 3);
			*/
		}
		if (Gear_System.System.Ticks_Count_Get() == 20)
		{
			//Gear_System.Multiplayer.Client_Disconnect();
		}
	}
}