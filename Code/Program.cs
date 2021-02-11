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
			//Gear.Network.Client_Connect("test");
			Gear.Signal.Create("test", 0);
		}
		if (Gear.Timer.Occurance_Check("test", 0.1f))
		{
			Gear.Text.Display("font", Gear.Timer.Repeat_Count_Get("test"), overwrite: true);
		}

		//Gear.Network.Clinet_Message_Send_To_All(Gear.Network.Console_Read());
	}
}