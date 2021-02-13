public class Program : Gear.Instance
{
	public override Program Created_Get() => this;
	public override string[] Loading_Screen_Prepare() => new string[] { "font.spritefont" };
	public override void Each_Loading_Screen_Update(int percent_loaded) { }
	public override void Each_Tick(int tick_count)
	{
		if (tick_count == 1)
		{
			//Gear.Window.Show(false);
			//Gear.Network.Server_Start();
			//Gear.Network.Client_Connect("test", "asfagf");
			var body = Gear.Body.Create_Get();
			body.Sprite_Set("red");
			body.Unique_Name_Set("f");
			body.Position_Set(50, 50);
			body.Angle_Set(45);
			body.Boundaries_Show(true, color_red: 0, color_green: 0, color_blue: 255);
			body.Angle_Show(true, color_red: 0, color_green: 255, color_blue: 0);
			body.Origin_Show(true, color_red: 255, color_green: 255, color_blue: 0);
		}
		var f = Gear.Body.Pick_By_Name_Get("f");



		var gp = Gear.Point_Grid.Created_Get(x: 666, y: 420, grid_width: 100, grid_height: 100);
		f.Position_Set(gp.Get().X_Get(), gp.Get().Y_Get());
		Gear.Text.Display("font", gp, overwrite: true);
		//Gear.Network.Clinet_Message_Send_To_All(Gear.Network.Console_Read());
	}
}