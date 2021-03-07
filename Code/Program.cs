public class Program : Gear.Instance
{
	public override Program Create() => this;

	public override void EachTick(int tickCount)
	{
		if (tickCount == 1)
		{
			Gear.Canvas.SetPixelSize(4, 4);
			var lineA = new Gear.Line(new Gear.Point(0, 0), new Gear.Point(16, 0));
			var lineB = new Gear.Line(new Gear.Point(16, 0), new Gear.Point(16, 16));
			var lineC = new Gear.Line(new Gear.Point(16, 16), new Gear.Point(0, 16));
			var lineD = new Gear.Line(new Gear.Point(0, 16), new Gear.Point(0, 0));

			var line1 = new Gear.Line(new Gear.Point(0, 0), new Gear.Point(16, 0));
			var line2 = new Gear.Line(new Gear.Point(16, 0), new Gear.Point(16, 16));
			var line3 = new Gear.Line(new Gear.Point(16, 16), new Gear.Point(0, 16));
			var line4 = new Gear.Line(new Gear.Point(0, 16), new Gear.Point(0, 0));
			var a = new Gear.Body("test");
			var b = new Gear.Body("test2");

			a.SetPosition(new Gear.Point(0, 0));
			b.SetPosition(new Gear.Point(100, 0));
			a.DisplaySprite("ball", width: 16, height: 16);
			b.DisplaySprite("ball", width: 16, height: 16, originX: 8, originY: 8);
			a.DisplayOrigin(true, b: 0);
			b.DisplayOrigin(true, b: 0);
			a.DisplayAngle(true, g: 0);
			b.DisplayAngle(true, g: 0);

			a.AddHitboxLine("line1", lineA);
			a.AddHitboxLine("line2", lineB);
			a.AddHitboxLine("line3", lineC);
			a.AddHitboxLine("line4", lineD);
			a.SetSizeWH(64, 64);

			b.AddHitboxLine("line1", line1);
			b.AddHitboxLine("line2", line2);
			b.AddHitboxLine("line3", line3);
			b.AddHitboxLine("line4", line4);
			b.SetSizeWH(16, 32);

			b.AddHitboxObstacle(a);

			b.DisplayHitbox();

			b.DisplayHitboxMiddlePoint(true, r: 100, b: 0);

			b.DisplayHitboxCrossPoints(g: 0, b: 0);
		}

		var ang = Gear.Camera.GetAngle();
		ang.Rotate(10);
		Gear.Camera.SetAngle(ang);

		Gear.Text.Display("font", Gear.Camera.GetPosition(), scale: 0.5f, overwrite: true);
	}
}