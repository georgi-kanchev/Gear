public class Program : Gear.Instance
{
	public override Program Create() => this;

	public override void EachTick(int tickCount)
	{
		if (tickCount == 1)
		{
			Gear.Canvas.SetPixelSize(5, 5);

			var a = new Gear.Hitbox<int>();

			a.AddCircle(5, new Gear.Circle());
		}
	}
}