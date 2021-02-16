public class Program : Gear.Instance
{
	public override Program Created_Get() => this;
	public override string[] Loading_Screen_Prepare() => new string[] { "font.spritefont" };
	public override void Each_Loading_Screen_Update(int percent_loaded) { }

	Gear.Angle angle = new Gear.Angle(0);
	public override void Each_Tick(int tick_count)
	{
		if (tick_count == 1)
		{
			var target = new Gear.Angle(50);
			angle.Percent_Towards_Target(target, 10);
			Gear.Text.Display("font", angle, overwrite: true);
		}

    }
}