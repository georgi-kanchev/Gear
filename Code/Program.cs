public class Program : Gear.Instance
{
	public override Program Create() => this;

	public override void EachTick(int tickCount)
	{
		if (tickCount == 1)
		{
			Gear.Canvas.SetPixelSize(4, 4);

			var lineA = new Gear.Line(new Gear.Point(50, 100), new Gear.Point(66, 100));
			var lineB = new Gear.Line(new Gear.Point(66, 100), new Gear.Point(66, 116));
			var lineC = new Gear.Line(new Gear.Point(66, 116), new Gear.Point(50, 116));
			var lineD = new Gear.Line(new Gear.Point(50, 116), new Gear.Point(50, 100));

			var line1 = new Gear.Line(new Gear.Point(100, 50), new Gear.Point(116, 50));
			var line2 = new Gear.Line(new Gear.Point(116, 50), new Gear.Point(116, 66));
			var line3 = new Gear.Line(new Gear.Point(116, 66), new Gear.Point(100, 66));
			var line4 = new Gear.Line(new Gear.Point(100, 66), new Gear.Point(100, 50));
			var a = new Gear.Body("test");
			var b = new Gear.Body("test2");

			a.SetPosition(new Gear.Point(50, 100));
			b.SetPosition(new Gear.Point(100, 50));
			a.SetSprite("ball", width: 16, height: 16);
			b.SetSprite("ball", width: 16, height: 16, originX: 8, originY: 8);
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
			b.SetSizeWH(32, 64);

			a.AddHitboxObstacle(b);
			b.AddHitboxObstacle(a);

			a.DisplayHitbox();
			b.DisplayHitbox();

			b.DisplayHitboxCrossPoints(g: 0, b: 0);
		}

		var a2 = Gear.Body.GetByUniqueName("test");
		var b2 = Gear.Body.GetByUniqueName("test2");

		var pos = Gear.Input.GetCursorPosition();
		var angA = a2.GetAngle();
		var angB = b2.GetAngle();
		angA.Rotate(50);
		angB.Rotate(-10);

		a2.SetAngle(angA);
		a2.SetPositionXY(200, 100);
		b2.SetAngle(angB);
		b2.SetPosition(pos);

		//var lineA1 = b2.GetHitboxLine("line2");
		//var lineB1 = b2.GetHitboxLine("line3");
		//b2.SetHitboxLine("line2", new Gear.Line(lineA1.GetStartPoint(), Gear.Input.GetCursorPosition()));
		//b2.SetHitboxLine("line3", new Gear.Line(Gear.Input.GetCursorPosition(), lineB1.GetEndPoint()));

		//b2.GetHitboxCrossPointsWithObstacle(a2);

		//Gear.Text.Display("font", b2.GetAngle(), scale: 0.5f, overwrite: true);
	}
}