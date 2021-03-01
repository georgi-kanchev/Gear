public class Program : Gear.Instance
{
	public override Program Create() => this;

	public override void EachTick(int tickCount)
	{
		if (tickCount == 1)
		{
			Gear.Canvas.SetPixelSize(5, 5);

			var lineA = new Gear.Line(new Gear.Point(50, 50), new Gear.Point(50, 100));
			var lineB = new Gear.Line(new Gear.Point(50, 100), new Gear.Point(80, 60));
			var lineC = new Gear.Line(new Gear.Point(80, 60), new Gear.Point(50, 50));
			var line1 = new Gear.Line(new Gear.Point(25, 75), new Gear.Point(100, 75));
			var line2 = new Gear.Line(new Gear.Point(100, 75), new Gear.Point(100, 125));
			var line3 = new Gear.Line(new Gear.Point(100, 125), new Gear.Point(25, 75));
			var a = new Gear.Body("test");
			var b = new Gear.Body("test2");
			a.AddHitboxLine("line1", lineA);
			a.AddHitboxLine("line2", lineB);
			a.AddHitboxLine("line3", lineC);

			b.AddHitboxLine("line1", line1);
			b.AddHitboxLine("line2", line2);
			b.AddHitboxLine("line3", line3);

			a.AddHitboxObstacle(b);
			b.AddHitboxObstacle(a);

			a.DisplayHitbox();
			b.DisplayHitbox();

			b.DisplayHitboxCrossPoints(g: 0, b: 0);

			var collision = b.HitboxOverlapsObstacle(a);
			var collisionPoint = b.GetHitboxCrossPointsWithObstacle(a);
		}

		Gear.Text.Display("font", Gear.Input.GetCursorPosition(), scale: 0.5f, overwrite: true);
	}
}