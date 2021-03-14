public class Program : Gear.Instance
{
	int highscore, currScore, shipFrame = 1, shotFrame = 1;
	bool soundOn = true, paused = true;
	float scrollSpeed = 1, dodgeSpeed = 200, shotSpeed = 300;
	float[] bgScrollX = new float[3];

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
			InitializeSounds();
			RemoveLoadingPercents();
			CreateBackground();
			CreateMenu();
			CreateGame();
			ShowMenu();
			Gear.Signal.Create("ship-animation", 0);
			Gear.Signal.Create("shot-animation", 0);
			Gear.Signal.Create("asteroid-spawn", 0);
		}
		else if (paused == false && tickCount > 40)
		{
			ScrollBackgrounds();
			UpdateShip();
			UpdateShot();
			UpdateAsteroids();
		}

		if (Gear.Timer.IsIntervalOccuring("ship-animation", 0.05f))
		{
			AnimateShip();
		}
		if (Gear.Timer.IsIntervalOccuring("shot-animation", 0.02f))
		{
			AnimateShotCast();
		}
		if (Gear.Timer.IsIntervalOccuring("asteroid-spawn", Gear.Number.GetRandomized(1, 5)))
		{
			SpawnAsteroid();
		}
		AnimateShotFly();
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
						StartGame();
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
		if (body.GetUniqueName() == "pause" && body.SpriteIsDisplayed())
		{
			var size = 24;
			var shade = 200;
			var sprite = body.GetSpriteName();
			switch (interaction)
			{
				case Gear.MouseHitboxInteraction.Hovered:
					{
						size = 26;
						shade = 255;
						break;
					}
				case Gear.MouseHitboxInteraction.Unhovered:
					{
						size = 24;
						shade = 200;
						break;
					}
				case Gear.MouseHitboxInteraction.Clicked:
					{
						size = 22;
						shade = 150;
						break;
					}
				case Gear.MouseHitboxInteraction.ClickReleased:
					{
						size = 26;
						shade = 255;
						sprite = sprite == "pause-on" ? "pause-off" : "pause-on";
						paused = sprite == "pause-off";

						var pausedBody = Gear.Body.GetByUniqueName("paused");
						pausedBody.DisplaySprite("paused", displayed: paused, width: 87, height: 14, originX: 44, originY: 7);
						if (soundOn)
						{
							Gear.Sound.Play("button");
						}
						break;
					}
				case Gear.MouseHitboxInteraction.Released:
					{
						size = 26;
						shade = 255;
						break;
					}
			}
			body.DisplaySprite(sprite, width: 24, height: 24, originX: 12, originY: 12, r: shade, g: shade, b: shade);
			body.SetSizeWH(size, size);
		}
	}
	public override void UserJustInteractedWithMouseButton(Gear.MouseButton button, Gear.Interaction interaction)
	{
		var hoverCount = 0;
		var hovered = Gear.Body.GetAllHovered();
		foreach (var body in hovered)
		{
			if (body.SpriteIsDisplayed())
			{
				hoverCount++;
			}
		}
		if (interaction == Gear.Interaction.Released || paused || hoverCount > 0 || button != Gear.MouseButton.Left) return;

		var ship = Gear.Body.GetByUniqueName("ship");
		var shot = Gear.Body.GetByUniqueName("shot");
		var shotCreated = shot != null;
		shot = shotCreated == false ? new Gear.Body("shot") : shot;

		shotFrame = 1;
		shot.SetPosition(ship.GetPosition());
		shot.SetAngle(ship.GetAngle());
		shot.DisplaySprite("shot (1)", width: 64, height: 64, originX: 32, originY: 32);
		if (shotCreated == false)
		{
			shot.AddTag("shot");
			shot.AddHitboxLine("up", new Gear.Line(new Gear.Point(0, -10), new Gear.Point(30, -10)));
			shot.AddHitboxLine("down", new Gear.Line(new Gear.Point(0, 10), new Gear.Point(30, 10)));
			shot.AddHitboxLine("left", new Gear.Line(new Gear.Point(0, -10), new Gear.Point(0, 10)));
			shot.AddHitboxLine("right", new Gear.Line(new Gear.Point(30, -10), new Gear.Point(30, 10)));
		}

		if (soundOn)
		{
			Gear.Sound.PlayFromCollection("shot", pitchPercent: 75);
		}
	}

	void InitializeSounds()
	{
		Gear.Sound.CreateCollection("shot");
		Gear.Sound.AddToCollection("laser (1)", "shot");
		Gear.Sound.AddToCollection("laser (2)", "shot");
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
		var meteors = new Gear.Body("meteors");
	}
	void ShowBackground()
	{
		var background = Gear.Body.GetByUniqueName("background");
		var stars = Gear.Body.GetByUniqueName("stars");
		var meteors = Gear.Body.GetByUniqueName("meteors");
		var backgroundSprite = $"background ({Gear.Number.GetRandomized(1, 4, 0)})";
		var starsSprite = $"stars ({Gear.Number.GetRandomized(1, 2, 0)})";
		var meteorsSprite = $"meteors ({Gear.Number.GetRandomized(1, 3, 0)})";

		background.DisplaySprite(backgroundSprite, width: 480, height: 270, originX: 240, originY: 135);
		stars.DisplaySprite(starsSprite, width: 480, height: 270, originX: 240, originY: 135, o: 100);
		meteors.DisplaySprite(meteorsSprite, width: 480, height: 270, originX: 240, originY: 135);
	}

	void CreateMenu()
	{
		Gear.Input.SetMouseCursorFromSprite("aim", 36, 36);

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
		var menu = Gear.Body.GetAllByTag("menu");
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

		var pause = new Gear.Body("pause");
		pause.DisplaySprite("pause-on", displayed: false, width: 24, height: 24, originX: 12, originY: 12);
		pause.SetPositionXY(canvasSize.GetW() / 2 - 45, -canvasSize.GetH() / 2 + 16);
		pause.AddHitboxLine("up", new Gear.Line(new Gear.Point(-12, -12), new Gear.Point(12, -12)));
		pause.AddHitboxLine("down", new Gear.Line(new Gear.Point(-12, 12), new Gear.Point(12, 12)));
		pause.AddHitboxLine("left", new Gear.Line(new Gear.Point(-12, -12), new Gear.Point(-12, 12)));
		pause.AddHitboxLine("right", new Gear.Line(new Gear.Point(12, -12), new Gear.Point(12, 12)));

		var ship = new Gear.Body("ship");
		ship.SetPositionXY(-canvasSize.GetW() / 2 + 50, canvasSize.GetH() / 2 + 50);

		new Gear.Body("paused");
		var score = new Gear.Body("score");
		score.SetPositionXY(-canvasSize.GetW() / 2 + 5, -canvasSize.GetH() / 2 + 5);

		for (int i = 0; i < 5; i++)
		{
			var scoreNumb = new Gear.Body($"score-{i}");
			scoreNumb.SetPositionXY(-canvasSize.GetW() / 2 + 35 + i * 25, -canvasSize.GetH() / 2 + 8);
		}
	}
	void StartGame()
	{
		paused = false;

		var pause = Gear.Body.GetByUniqueName("pause");
		pause.DisplaySprite("pause-on", displayed: true, width: 24, height: 24, originX: 12, originY: 12);

		var score = Gear.Body.GetByUniqueName("score");
		score.DisplaySprite("highscore", width: 57, height: 53);
		score.SetSizeWH(25, 25);

		DisplayNumber("score-", currScore);
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

	void ScrollBackgrounds()
	{
		var background = Gear.Body.GetByUniqueName("background");
		var stars = Gear.Body.GetByUniqueName("stars");
		var meteors = Gear.Body.GetByUniqueName("meteors");

		bgScrollX[0] += scrollSpeed * 0.01f;
		//bgScrollX[1] += 0.01f;
		bgScrollX[2] += scrollSpeed * 0.1f;

		ScrollBody(background, (int)bgScrollX[0], 255);
		//ScrollBody(stars, (int)bgScrollX[1], 100);
		ScrollBody(meteors, (int)bgScrollX[2], 255);

		void ScrollBody(Gear.Body body, int speed, int opacity)
		{
			body.DisplaySprite(body.GetSpriteName(), width: 480, height: 270, originX: 240, originY: 135, gridSize: 1, indexH: speed, o: opacity);
		}
	}
	void AnimateShip()
	{
		shipFrame++;
		shipFrame = shipFrame > 4 ? 1 : shipFrame;

		var ship = Gear.Body.GetByUniqueName("ship");
		ship.DisplaySprite($"ship ({shipFrame})", width: 128, height: 128, originX: 60, originY: 62);
	}
	void AnimateShotCast()
	{
		var shot = Gear.Body.GetAllByTag("shot");
		if (shot.Length == 0) return;

		var ship = Gear.Body.GetByUniqueName("ship");
		var shipPos = ship.GetPosition();
		var shipAng = ship.GetAngle();

		if (shotFrame < 7)
		{
			shotFrame++;
			shot[0].DisplaySprite($"shot ({shotFrame})", width: 64, height: 64, originX: 32, originY: 32);
			shot[0].SetPosition(shipPos);
			shot[0].SetAngle(shipAng);
			return;
		}
	}
	void UpdateShip()
	{
		var canvasSize = Gear.Canvas.GetSize();
		var ship = Gear.Body.GetByUniqueName("ship");
		var shipAngle = ship.GetAngle();
		var targetAngle = new Gear.Angle();
		var shipPos = ship.GetPosition();
		var mousePos = Gear.Input.GetMouseCursorPosition();
		mousePos.SetX(canvasSize.GetW() / 2);
		mousePos.SetY(Gear.Number.GetLimited(mousePos.GetY(), -canvasSize.GetH() / 2 + 50, canvasSize.GetH() / 2 - 50));

		targetAngle.SetFromBetweenPoints(ship.GetPosition(), mousePos);
		shipAngle.RotateTowardAngle(targetAngle, dodgeSpeed / 2);
		shipPos.MoveTowardPoint(mousePos, dodgeSpeed);

		ship.SetAngle(shipAngle);
		ship.SetPositionY(shipPos.GetY());
	}
	void UpdateShot()
	{
		var shot = Gear.Body.GetAllByTag("shot");
		if (shot.Length == 0 || shotFrame != 7) return;

		var shotPos = shot[0].GetPosition();

		shotPos.MoveAtAngle(shot[0].GetAngle(), shotSpeed);
		shot[0].SetPosition(shotPos);
	}
	void AnimateShotFly()
	{
		var shot = Gear.Body.GetAllByTag("shot");
		if (shot.Length == 0 || shotFrame != 7 || Gear.Performance.GetTickCount() % 5 != 0) return;

		shot[0].SetSizeH(-shot[0].GetSize().GetH());
	}

	void SpawnAsteroid()
	{
		var canvasSize = Gear.Canvas.GetSize();
		var ast = new Gear.Body($"ast-{Gear.Performance.GetTickCount()}");
		var sprite = $"asteroid ({Gear.Number.GetRandomized(1, 3)})";

		ast.SetPositionXY(canvasSize.GetW(), Gear.Number.GetRandomized(-canvasSize.GetH() / 2, canvasSize.GetH() / 2));
		ast.DisplaySprite(sprite, originX: 32, originY: 32);
		//ast.SetSizeWH(Gear.Number.GetRandomized(56, 72), Gear.Number.GetRandomized(56, 72));
		ast.AddTag("asteroid");
	}
	void UpdateAsteroids()
	{
		var asteroids = Gear.Body.GetAllByTag("asteroid");
		foreach (var asteroid in asteroids)
		{
			var pos = asteroid.GetPosition();
			pos.MoveAtAngle(new Gear.Angle(Gear.Number.GetRandomized(170, 190)), scrollSpeed * 100);

			asteroid.SetPosition(pos);
		}
	}
}