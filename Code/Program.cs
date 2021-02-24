public class Program : Gear.Instance
{
	public override Program CreatedGet() => this;

	public override void EachTick(int tickCount)
	{
		if (tickCount == 1)
		{
			Gear.Canvas.PixelSizeSet(5, 5);

			var ball = new Gear.Body();
			ball.SpriteSet("ball", width: 16, height: 16, originX: 8, originY: 8);
			ball.PositionSet(x: 100, y: 100);
			ball.UniqueNameSet("ball");

			var line = new Gear.Body();
			line.SpriteSet("ball", width: 180, height: 16);
			line.SizeSet(64, 1);
			line.UniqueNameSet("paddle");

			var line3 = new Gear.Body();
			line3.SpriteSet("ball", width: 180, height: 16, red: 0);
			line3.SizeSet(200, 1);
			line3.PositionSet(150, 1);
			line3.AngleSet(90);
		}

		var mousePos = Gear.Input.MouseCursorPositionWorldGet();
		var lineAngle = new Gear.Angle();
		var line2 = Gear.Body.PickByUniqueNameGet("paddle");
		var ball2 = Gear.Body.PickByUniqueNameGet("ball");
		var dist = line2.PositionGet().DistanceToPointGet(mousePos);

		lineAngle.SetFromBetweenPoints(line2.PositionGet(), mousePos);
		line2.AngleSet(lineAngle.Get());
		line2.SizeSet(dist, 1);

		if (Gear.Input.KeyIsPressedCheck(Gear.InputKeys.A))
		{
			var asd = 0;
		}

		var ballCircle = new Gear.Circle(ball2.PositionGet(), ball2.SizeGet().WidthGet() / 2);
		var orangeLine = new Gear.Line(new Gear.Point(), mousePos);
		var greenLine = new Gear.Line(new Gear.Point(150, 1), new Gear.Point(150, 200));
		var crossPointsCircle = orangeLine.CrossPointsWithCircleGet(ballCircle);
		var orangeXGreen = orangeLine.CrossPointWithLine(greenLine);
		var lineXCircle = "";
		var lineXLine = "";
		foreach (var point in orangeXGreen)
		{
			lineXLine = $"{lineXLine}\n{point}";
		}
		foreach (var point in crossPointsCircle)
		{
			lineXCircle = $"{lineXCircle}\n{point}";
		}

		Gear.Text.Display("font",
			$"{lineXLine}\n" +
			$"{lineXCircle}"
			, scale: 0.4f, overwrite:true);
	}
}