public class Program : Gear.Executable
{
	public override Program Create() => this;
	public override string[] Loading_Screen_Prepare() => new string[] { "font.spritefont" };
	public override void Each_Loading_Screen_Update(int percent_loaded) { }
	public override void Each_Tick(int tick_count)
	{
		if (tick_count == 1)
		{
			Gear.System.Window_Show(false);
			//Gear_System.Network.Server_Start();
			Gear.Network.Client_Connect("test");
		}
		Gear.Network.Clinet_Message_Send_To_All(Gear.Network.Console_Read());
	}
}