public class Program : Gear.Instance
{
	int highscore = 0;
	bool soundOn = true;

	public override Program Create() => this;
	public override string[] LoadingScreenPrepare()
	{
		Gear.Canvas.SetPixelSizeWH(4, 4);

		for (int i = 0; i < 3; i++)
		{
			var body = new Gear.Body($"loading-{i}");
			body.SetPositionXY(-30 + i * 20, 0);
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
			Gear.Input.SetMouseCursorFromSprite("aim", 24, 24);
			RemoveLoadingPercents();
			CreateBackground();
			CreateMenu();
			CreateGame();
			ShowMenu();
		}
		else if (tickCount > 40)
		{

		}
	}

	public override void MouseJustInteractedWithHitbox(Gear.Body body, Gear.MouseHitboxInteraction interaction)
	{
		if (body.GetUniqueName() == "play" && body.SpriteIsDisplayed())
		{
			var size = 80;
			var shade = 200;
			var displayed = true;
			switch (interaction)
			{
				case Gear.MouseHitboxInteraction.Hovered:
					{
						size = 82;
						shade = 255;
						break;
					}
				case Gear.MouseHitboxInteraction.Unhovered:
					{
						size = 80;
						shade = 200;
						break;
					}
				case Gear.MouseHitboxInteraction.Clicked:
					{
						size = 78;
						shade = 150;
						break;
					}
				case Gear.MouseHitboxInteraction.ClickReleased:
					{
						size = 82;
						shade = 255;
						displayed = false;
						if (soundOn)
						{
							Gear.Sound.Play("button");
						}
						HideMenu();
						ShowGame();
						break;
					}
				case Gear.MouseHitboxInteraction.Released:
					{
						size = 82;
						shade = 255;
						break;
					}
			}
			body.DisplaySprite("play", displayed: displayed, width: 71, height: 78, originX: 36, originY: 39, r: shade, g: shade, b: shade);
			body.SetSizeWH(size, size);
		}
		if (body.GetUniqueName() == "sound" && body.SpriteIsDisplayed())
		{
			var size = 25;
			var shade = 200;
			var sprite = body.GetSpriteName();
			switch (interaction)
			{
				case Gear.MouseHitboxInteraction.Hovered:
					{
						size = 27;
						shade = 255;
						break;
					}
				case Gear.MouseHitboxInteraction.Unhovered:
					{
						size = 25;
						shade = 200;
						break;
					}
				case Gear.MouseHitboxInteraction.Clicked:
					{
						size = 23;
						shade = 150;
						break;
					}
				case Gear.MouseHitboxInteraction.ClickReleased:
					{
						size = 27;
						shade = 255;
						sprite = sprite == "sound-on" ? "sound-off" : "sound-on";
						soundOn = sprite == "sound-off";
						if (soundOn)
						{
							Gear.Sound.Play("button");
						}
						break;
					}
				case Gear.MouseHitboxInteraction.Released:
					{
						size = 27;
						shade = 255;
						break;
					}
			}
			body.DisplaySprite(sprite, width: 24, height: 25, originX: 12, originY: 12, r: shade, g: shade, b: shade);
			body.SetSizeWH(size, size);
		}
	}

	void RemoveLoadingPercents()
	{
		for (int i = 0; i < 3; i++)
		{
			var body = Gear.Body.GetByUniqueName($"loading-{i}");
			body.Delete();
		}
	}
	void CreateBackground()
	{
		var background = new Gear.Body("background");
		var stars = new Gear.Body("stars");
	}
	void ShowBackground()
	{
		var background = Gear.Body.GetByUniqueName("background");
		var stars = Gear.Body.GetByUniqueName("stars");
		var backgroundSprite = $"background ({Gear.Number.GetRandomized(1, 4, 0)})";
		var starsSprite = $"stars ({Gear.Number.GetRandomized(1, 2, 0)})";

		background.DisplaySprite(backgroundSprite, width: 480, height: 270, originX: 240, originY: 135);
		stars.DisplaySprite(starsSprite, width: 480, height: 270, originX: 240, originY: 135, o: 100);
	}

	void CreateMenu()
	{
		for (int i = 0; i < 5; i++)
		{
			var highscore = new Gear.Body($"menu-highscore-{i}");
			highscore.AddTag("menu");
			highscore.SetPositionXY(i * 19 + 65, -12);
		}
		var star = new Gear.Body($"menu-highscore-star");
		star.SetPositionY(-30);
		star.DisplaySprite("highscore", width: 57, height: 53);
		star.AddTag("menu");

		var play = new Gear.Body("play");
		play.SetPositionX(-40);
		play.SetSizeWH(80, 80);
		play.AddHitboxLine("up", new Gear.Line(new Gear.Point(-30, -30), new Gear.Point(30, -30)));
		play.AddHitboxLine("down", new Gear.Line(new Gear.Point(-30, 30), new Gear.Point(30, 30)));
		play.AddHitboxLine("left", new Gear.Line(new Gear.Point(-30, -30), new Gear.Point(-30, 30)));
		play.AddHitboxLine("right", new Gear.Line(new Gear.Point(30, -30), new Gear.Point(30, 30)));
		play.AddTag("menu");

		var playBorder = new Gear.Body("play-border");
		playBorder.SetPositionX(-40);
		playBorder.AddTag("menu");
	}
	void HideMenu()
	{
		var menu = Gear.Body.GetByTag("menu");
		foreach (var body in menu)
		{
			body.DisplaySprite("empty", displayed: false);
		}
	}
	void ShowMenu()
	{
		var play = Gear.Body.GetByUniqueName("play");
		var playBorder = Gear.Body.GetByUniqueName("play-border");

		DisplayNumber("menu-highscore-", highscore);
		play.DisplaySprite("play", width: 71, height: 78, originX: 36, originY: 39, r: 200, g: 200, b: 200);
		playBorder.DisplaySprite("play-border", width: 71, height: 78, originX: 36, originY: 39);
		ShowBackground();
	}

	void CreateGame()
	{
		var canvasSize = Gear.Canvas.GetSize();
		var sound = new Gear.Body("sound");
		sound.SetPositionXY(canvasSize.GetW() / 2 - 18, -canvasSize.GetH() / 2 + 16);
		sound.DisplaySprite("sound-off", width: 24, height: 25, originX: 12, originY: 12);
		sound.AddHitboxLine("up", new Gear.Line(new Gear.Point(-12, -12), new Gear.Point(12, -12)));
		sound.AddHitboxLine("down", new Gear.Line(new Gear.Point(-12, 12), new Gear.Point(12, 12)));
		sound.AddHitboxLine("left", new Gear.Line(new Gear.Point(-12, -12), new Gear.Point(-12, 12)));
		sound.AddHitboxLine("right", new Gear.Line(new Gear.Point(12, -12), new Gear.Point(12, 12)));
	}
	void ShowGame()
	{

	}
	void DisplayNumber(string uniqueName, int number)
	{
		var percentStr = number.ToString();

		for (int i = 0; i < percentStr.Length; i++)
		{
			var digit = percentStr[i];
			var body = Gear.Body.GetByUniqueName($"{uniqueName}{i}");
			var sprite = $"number ({digit})";

			if (body == null) return;
			body.DisplaySprite(sprite, width: 19, height: 23);
		}
	}
}