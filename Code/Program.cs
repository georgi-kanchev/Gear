public class Program : Gear.Instance
{
	public override Program Create() => this;

	public override void EachTick(int tickCount)
	{
		if (tickCount == 1)
		{
			Gear.Canvas.SetPixelSize(5, 5);

			//var a = new Gear.Body(null);
			var b = new Gear.Body("test");
			b.SetSprite("test");
			//a.Tag("test-tag");
			b.Tag("test-tag");

			var bodies = Gear.Body.GetByTag("test-tag");
		}
	}
}