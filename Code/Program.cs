public class Program : Gear.Instance
{
	public override Program Created_Get() => this;
	public override string[] Loading_Screen_Prepare() => new string[] { "font.spritefont" };
	public override void Each_Loading_Screen_Update(int percent_loaded) { }

	public override void Each_Tick(int tick_count)
	{
		if (tick_count == 1)
		{
			var angle = new Gear.Angle();
			angle.Set_From_Rotation_Sample(Gear.Rotation_Samples.Down_Right);
			Gear.Text.Display("font", $"{angle}", overwrite: true);
		}
	}
}