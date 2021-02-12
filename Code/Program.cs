public class Program : Gear.Instance
{
	public override Program Create() => this;
	public override string[] Loading_Screen_Prepare() => new string[] { "font.spritefont" };
	public override void Each_Loading_Screen_Update(int percent_loaded) { }
	public override void Each_Tick(int tick_count)
	{
		if (tick_count == 1)
		{
			//Gear.Window.Show(false);
			//Gear.Network.Server_Start();
			//Gear.Network.Client_Connect("test", "asfagf");
			Gear.Canvas.Pixel_Size_Set(10, 10);
			var body = Gear.Body.Create();
			body.Sprite_Set("red", origin_x: 16, origin_y: 16);
			body.Unique_Name_Set("f");
			body.Position_Set(50, 50);
			body.Angle_Set(45);
			body.Boundaries_Show(true, color_red: 0, color_green: 0, color_blue: 255);
			body.Angle_Show(true, color_red: 0, color_green: 255, color_blue: 0);
			body.Origin_Show(true, color_red: 255, color_green: 255, color_blue: 0);
		}
		//Gear.Text.Display("font", $"{Gear.Text.Time_Formatted_Get(5.5f, ms_show: true)}\n", overwrite: false);
		//Gear.Network.Clinet_Message_Send_To_All(Gear.Network.Console_Read());
	}
}