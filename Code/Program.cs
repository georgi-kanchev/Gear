public class Program : Gear.Instance
{
	public override Program Create() => this;
	public override string[] LoadingScreenPrepare()
	{
		Gear.Canvas.SetPixelSizeWH(4, 4);
		for (int i = 0; i < 3; i++)
		{
			var body = new Gear.Body($"loading-{i}");
			body.SetPositionXY(- 30 + i * 20, 0);
		}

		var numbers = new string[11];
		for (int i = 0; i < 10; i++)
		{
			numbers[i] = $"number ({i}).png";
		}
		numbers[10] = "empty.png";
		return numbers;
	}
	public override void EachLoadingScreenUpdate(int percentLoaded)
	{
		DisplayNumber($"loading-", percentLoaded);
	}
	public override void EachTick(int tickCount)
	{
		if (tickCount == 40)
		{
			RemoveLoadingPercents();
			CreateBackground();
			StartMenu();
		}
		else if (tickCount > 40)
		{
			var play = Gear.Body.GetByUniqueName("play");
			if (play.HitboxOverlapsPoint(Gear.Input.GetMouseCursorPosition()))
			{
				play.SetSizeWH(85, 95);
				if (Gear.Input.LeftMouseButtonIsPressed())
				{
					play.SetSizeWH(75, 85);
				}
			}
			else
			{
				play.SetSizeWH(80, 90);
			}
		}
	}

	public void RemoveLoadingPercents()
	{
		for (int i = 0; i < 3; i++)
		{
			var body = Gear.Body.GetByUniqueName($"loading-{i}");
			body.Delete();
		}
	}
	public void CreateBackground()
	{
		var backgroundSprite = $"background ({Gear.Number.GetRandomized(1, 4, 0)})";
		var starsSprite = $"stars ({Gear.Number.GetRandomized(1, 2, 0)})";
		var background = new Gear.Body("background");
		var stars = new Gear.Body("stars");

		background.DisplaySprite(backgroundSprite, width: 480, height: 270, originX: 240, originY: 135);
		stars.DisplaySprite(starsSprite, width: 480, height: 270, originX: 240, originY: 135, o: 100);
	}
	public void StartMenu()
	{
		var play = new Gear.Body("play");
		var playBorder = new Gear.Body("play-border");
		play.DisplaySprite("play", width: 71, height: 78, originX: 36, originY: 39);

		play.AddHitboxLine("up", new Gear.Line(new Gear.Point(-25, -30), new Gear.Point(25, -30)));
		play.AddHitboxLine("down", new Gear.Line(new Gear.Point(-25, 25), new Gear.Point(25, 25)));
		play.AddHitboxLine("left", new Gear.Line(new Gear.Point(-25, -30), new Gear.Point(-25, 25)));
		play.AddHitboxLine("right", new Gear.Line(new Gear.Point(25, -30), new Gear.Point(25, 25)));

		playBorder.DisplaySprite("play-border", width: 71, height: 78, originX: 36, originY: 39);
	}
	public void DisplayNumber(string uniqueName, int number)
	{
		var percentStr = number.ToString();

		for (int i = 0; i < percentStr.Length; i++)
		{
			var digit = percentStr[i];
			var body = Gear.Body.GetByUniqueName($"{uniqueName}{i}");
			var sprite = $"number ({digit})";

			body.DisplaySprite(sprite, width: 19, height: 23);
		}
	}
}