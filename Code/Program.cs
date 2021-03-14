public class Program : Gear.Instance
{
	int highscore, currScore, shipFrame = 1, shotFrame = 1, astSpawnChance = 80;
	bool soundOn = true, paused = true;
	float scrollSpeed = 1, dodgeSpeed = 600, shotSpeed = 300, energy, maxEnergy = 480 - 14, energyRegen = 4;
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
			UpdateEnergy();
		}

		if (Gear.Timer.IsIntervalOccuring("ship-animation", 0.05f))
		{
			AnimateShip();
		}
		if (Gear.Timer.IsIntervalOccuring("shot-animation", 0.02f))
		{
			AnimateShotCast();
		}
		if (Gear.Timer.IsIntervalOccuring("asteroid-spawn", 3) && Gear.Number.HasChance(astSpawnChance))
		{
			SpawnAsteroid();
		}
		AnimateShotFly();
	}

	public override void MouseJustInteractedWithHitbox(Gear.Body body, Gear.MouseHitboxInteraction interaction)
	{
		if (body.SpriteIsDisplayed() == false) return;
		var sprite = body.GetSpriteName();
		var name = body.GetUniqueName();
		switch (name)
		{
			case "play":
				{
					AnimateButton(sprite, interaction != Gear.MouseHitboxInteraction.ClickReleased, 80, 71, 78, 36, 39);
					if (interaction == Gear.MouseHitboxInteraction.ClickReleased)
					{
						HideMenu();
						StartGame();
					}
					break;
				}
			case "sound":
				{
					AnimateButton(sprite, true, 25, width: 24, height: 25, originX: 12, originY: 12);
					if (interaction == Gear.MouseHitboxInteraction.ClickReleased)
					{
						sprite = sprite == "sound-on" ? "sound-off" : "sound-on";
						AnimateButton(sprite, true, 25, width: 24, height: 25, originX: 12, originY: 12);
						soundOn = sprite == "sound-off";
					}
					break;
				}
			case "exit":
				{
					AnimateButton(sprite, true, 24, width: 24, height: 24, originX: 12, originY: 12);
					if (interaction == Gear.MouseHitboxInteraction.ClickReleased)
					{
						Gear.Window.Close();
					}
					break;
				}
			case "pause":
				{
					AnimateButton(sprite, true, 24, width: 24, height: 24, originX: 12, originY: 12);
					sprite = body.GetSpriteName();
					if (interaction == Gear.MouseHitboxInteraction.ClickReleased)
					{
						paused = !paused;
						AnimateButton(paused ? "pause-off" : "pause-on", true, 24, width: 24, height: 24, originX: 12, originY: 12);

						var pausedBody = Gear.Body.GetByUniqueName("paused");
						pausedBody.DisplaySprite("paused", displayed: paused, width: 87, height: 14, originX: 44, originY: 7);
					}
					break;
				}
		}

		void AnimateButton(string sprite, bool displayed, int size, int width, int height, int originX, int originY)
		{
			var shade = 200;
			switch (interaction)
			{
				case Gear.MouseHitboxInteraction.Hovered:
					{
						size = size + 2;
						shade = 255;
						break;
					}
				case Gear.MouseHitboxInteraction.Unhovered:
					{
						shade = 200;
						break;
					}
				case Gear.MouseHitboxInteraction.Clicked:
					{
						size = size - 2;
						shade = 150;
						break;
					}
				case Gear.MouseHitboxInteraction.ClickReleased:
					{
						size = size + 2;
						shade = 255;
						if (soundOn)
						{
							Gear.Sound.Play("button");
						}
						break;
					}
				case Gear.MouseHitboxInteraction.Released:
					{
						size = size + 2;
						shade = 255;
						break;
					}
			}
			body.DisplaySprite(sprite, displayed: displayed, width: width, height: height, originX: originX, originY: originY, r: shade, g: shade, b: shade);
			body.SetSizeWH(size, size);
		}
	}
	public override void UserJustInteractedWithMouseButton(Gear.MouseButton button, Gear.Interaction interaction)
	{
		if (CanShoot(interaction) == false) return;

		Shoot();
	}
	public override void UserJustInteractedWithKey(Gear.Key key, Gear.Interaction interaction)
	{
		if (CanShoot(interaction) == false) return;

		Shoot();
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

		var exit = new Gear.Body("exit");
		exit.SetPositionXY(canvasSize.GetW() / 2 - 18, -canvasSize.GetH() / 2 + 16);
		exit.DisplaySprite("exit", width: 24, height: 24, originX: 12, originY: 12, r: 200, g: 200, b: 200);
		exit.AddHitboxLine("up", new Gear.Line(new Gear.Point(-12, -12), new Gear.Point(12, -12)));
		exit.AddHitboxLine("down", new Gear.Line(new Gear.Point(-12, 12), new Gear.Point(12, 12)));
		exit.AddHitboxLine("left", new Gear.Line(new Gear.Point(-12, -12), new Gear.Point(-12, 12)));
		exit.AddHitboxLine("right", new Gear.Line(new Gear.Point(12, -12), new Gear.Point(12, 12)));

		var sound = new Gear.Body("sound");
		sound.SetPositionXY(canvasSize.GetW() / 2 - 46, -canvasSize.GetH() / 2 + 16);
		sound.DisplaySprite("sound-off", width: 24, height: 25, originX: 12, originY: 12);
		sound.AddHitboxLine("up", new Gear.Line(new Gear.Point(-12, -12), new Gear.Point(12, -12)));
		sound.AddHitboxLine("down", new Gear.Line(new Gear.Point(-12, 12), new Gear.Point(12, 12)));
		sound.AddHitboxLine("left", new Gear.Line(new Gear.Point(-12, -12), new Gear.Point(-12, 12)));
		sound.AddHitboxLine("right", new Gear.Line(new Gear.Point(12, -12), new Gear.Point(12, 12)));

		var pause = new Gear.Body("pause");
		pause.DisplaySprite("pause-on", displayed: false, width: 24, height: 24, originX: 12, originY: 12);
		pause.SetPositionXY(canvasSize.GetW() / 2 - 72, -canvasSize.GetH() / 2 + 16);
		pause.AddHitboxLine("up", new Gear.Line(new Gear.Point(-12, -12), new Gear.Point(12, -12)));
		pause.AddHitboxLine("down", new Gear.Line(new Gear.Point(-12, 12), new Gear.Point(12, 12)));
		pause.AddHitboxLine("left", new Gear.Line(new Gear.Point(-12, -12), new Gear.Point(-12, 12)));
		pause.AddHitboxLine("right", new Gear.Line(new Gear.Point(12, -12), new Gear.Point(12, 12)));

		var ship = new Gear.Body("ship");
		ship.SetPositionXY(-canvasSize.GetW() / 2 + 50, canvasSize.GetH() / 2 + 50);
		ship.DisplaySprite("ship (1)", width: 128, height: 128, originX: 64, originY: 64);
		ship.AddHitboxLine("up", new Gear.Line(new Gear.Point(0, -10), new Gear.Point(30, -10)));
		ship.AddHitboxLine("down", new Gear.Line(new Gear.Point(0, 10), new Gear.Point(30, 10)));
		ship.AddHitboxLine("left", new Gear.Line(new Gear.Point(0, -10), new Gear.Point(0, 10)));
		ship.AddHitboxLine("right", new Gear.Line(new Gear.Point(30, -10), new Gear.Point(30, 10)));
		ship.DisplayHitbox(r: 0, b: 0);

		new Gear.Body("paused");
		var score = new Gear.Body("score");
		score.SetPositionXY(-canvasSize.GetW() / 2 + 5, -canvasSize.GetH() / 2 + 5);

		for (int i = 0; i < 5; i++)
		{
			var scoreNumb = new Gear.Body($"score-{i}");
			scoreNumb.SetPositionXY(-canvasSize.GetW() / 2 + 35 + i * 25, -canvasSize.GetH() / 2 + 8);
		}

		var cd = new Gear.Body("cd");
		var cdBorder = new Gear.Body("cd-border");
		cd.SetPositionXY(-canvasSize.GetW() / 2 + 7, canvasSize.GetH() / 2 - 14);
		cd.DisplaySprite("cd", false, width: 1, height: 9);
		cd.SetSizeWH(canvasSize.GetW() - 14, 9);
		cdBorder.SetPositionXY(-canvasSize.GetW() / 2, canvasSize.GetH() / 2 - 19);
		cdBorder.DisplaySprite("cd-border", false, width: 480, height: 19);
	}
	void StartGame()
	{
		paused = false;

		var pause = Gear.Body.GetByUniqueName("pause");
		pause.DisplaySprite("pause-on", displayed: true, width: 24, height: 24, originX: 12, originY: 12);

		var score = Gear.Body.GetByUniqueName("score");
		score.DisplaySprite("highscore", width: 57, height: 53);
		score.SetSizeWH(25, 25);

		var cdBorder = Gear.Body.GetByUniqueName("cd-border");
		cdBorder.DisplaySprite("cd-border", true, width: 480, height: 19);
		var cd = Gear.Body.GetByUniqueName("cd");
		cd.DisplaySprite("cd", true, width: 1, height: 9);

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
		var rightScreenEdge = Gear.Canvas.GetSize().GetW() / 2;

		shotPos.MoveAtAngle(shot[0].GetAngle(), shotSpeed);
		if (shotPos.GetX() > rightScreenEdge)
		{
			shotPos.SetY(1000);
		}
		shot[0].SetPosition(shotPos);
	}
	void Shoot()
	{
		var ship = Gear.Body.GetByUniqueName("ship");
		var shot = Gear.Body.GetByUniqueName("shot");
		var shotCreated = shot != null;
		shot = shotCreated == false ? new Gear.Body("shot") : shot;

		shotFrame = 1;
		energy = 0;
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
			shot.DisplayHitbox();
		}

		if (soundOn)
		{
			Gear.Sound.PlayFromCollection("shot", pitchPercent: 75);
		}
	}
	bool CanShoot(Gear.Interaction interaction)
	{
		return interaction == Gear.Interaction.Pressed && paused == false &&
			AnythingIsHovered() == false && energy == maxEnergy;
	}

	void UpdateEnergy()
	{
		energy = energy < maxEnergy ? energy + energyRegen : maxEnergy;
		var cd = Gear.Body.GetByUniqueName("cd");
		cd.SetSizeW(energy);
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
		ast.AddTag("asteroid");
		ast.SetAngleA(Gear.Number.GetRandomized(0, 360));

		ast.AddHitboxLine("up", new Gear.Line(new Gear.Point(-24, -24), new Gear.Point(24, -24)));
		ast.AddHitboxLine("down", new Gear.Line(new Gear.Point(-24, 24), new Gear.Point(24, 24)));
		ast.AddHitboxLine("left", new Gear.Line(new Gear.Point(-24, -24), new Gear.Point(-24, 24)));
		ast.AddHitboxLine("right", new Gear.Line(new Gear.Point(24, -24), new Gear.Point(24, 24)));
		ast.DisplayHitbox(g: 0, b: 0);

		var size = Gear.Number.GetRandomized(16, 72);
		ast.SetSizeWH(size, size);
	}
	void UpdateAsteroids()
	{
		var canvasSize = Gear.Canvas.GetSize();
		var asteroids = Gear.Body.GetAllByTag("asteroid");
		foreach (var asteroid in asteroids)
		{
			var pos = asteroid.GetPosition();
			var ang = asteroid.GetAngle();
			pos.MoveAtAngle(new Gear.Angle(180), scrollSpeed * 100);
			if (pos.GetX() < -canvasSize.GetW() / 2 - 100)
			{
				pos.SetXY(canvasSize.GetW() / 2 + Gear.Number.GetRandomized(50, 200),
					Gear.Number.GetRandomized(-canvasSize.GetH() / 2, canvasSize.GetH() / 2));
			}
			ang.Rotate(10);
			asteroid.SetPosition(pos);
			asteroid.SetAngle(ang);
		}
	}

	bool AnythingIsHovered()
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

		return hoverCount > 0;
	}
}