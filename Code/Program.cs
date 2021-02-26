public class Program : Gear.Instance
{
	public override Program Create() => this;

	public override void EachTick(int tickCount)
	{
		if (tickCount == 1)
		{
			Gear.Canvas.SetPixelSize(5, 5);

			var ball = new Gear.Body("ball");
			ball.SetSprite("ball", width: 16, height: 16, originX: 8, originY: 8);
			ball.SetPositionXY(x: 100, y: 100);

			var line = new Gear.Body("paddle");
			line.SetSprite("ball", width: 180, height: 16);
			line.SetSizeWH(64, 1);

			var line3 = new Gear.Body("hello");
			line3.SetSprite("ball", width: 180, height: 16, r: 0);
			line3.SetSizeWH(200, 1);
			line3.SetPositionXY(150, 1);
			line3.SetAngleA(90);

			var line4 = line3.Duplicate("test");
			line4.SetSizeWH(0, 0);
		}

		var mousePos = Gear.Input.MouseCursorPositionWorldGet();
		var lineAngle = new Gear.Angle();
		var line2 = Gear.Body.GetByUniqueName("paddle");
		var ball2 = Gear.Body.GetByUniqueName("ball");
		var dist = line2.GetPosition().GetDistanceToPoint(mousePos);

		lineAngle.SetFromBetweenPoints(line2.GetPosition(), mousePos);
		line2.SetAngle(lineAngle);
		line2.SetSizeWH(dist, 1);

		var ballCircle = new Gear.Circle(ball2.GetPosition(), ball2.GetSize().GetW() / 2);
		var orangeLine = new Gear.Line(new Gear.Point(), mousePos);
		var greenLine = new Gear.Line(new Gear.Point(150, 1), new Gear.Point(150, 200));
		var crossPointsCircle = orangeLine.GetCrossPointsWithCircle(ballCircle);
		var orangeXGreen = orangeLine.GetCrossPointWithLine(greenLine);
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

		var asd = new Gear.Storage<string, string>();
		asd.Expand(10, "key", "value");
		asd.Expand(10, "key2", "value2");
		asd.ReplaceAt(10, "test");
		var index = asd.IndexExists(9);
	}
}