public class Program : Gear.Instance
{
	int highscore, currScore, shipFrame = 1, shotFrame = 1, astSpawnChance = 80;
	bool soundOn = true, paused = true, shipExplosion;
	float scrollSpeed = 1, dodgeSpeed = 600, shotSpeed = 300, energy, maxEnergy = 480 - 14, energyRegen = 4;
	float[] bgScrollX = new float[3];
	Gear.Storage<Gear.Body, int> astFrames = new Gear.Storage<Gear.Body, int>();

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
			Gear.Signal.Create("ast-animation", 0);
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

		if (Gear.Timer.IsIntervalOccuring("ship-animation", shipExplosion ? 0.3f : 0.05f) && paused == false)
		{
			AnimateShip();
		}
		if (Gear.Timer.IsIntervalOccuring("shot-animation", 0.02f) && paused == false)
		{
			AnimateShotCast();
		}
		if (Gear.Timer.IsIntervalOccuring("ast-animation", shipExplosion ? 0.8f : 0.1f) && paused == false)
		{
			AnimateAsteroids();
		}
		if (Gear.Timer.IsIntervalOccuring("asteroid-spawn", 3) && paused == false && Gear.Number.HasChance(astSpawnChance))
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
						soundOn = sprite == "sound-off";
						AnimateButton(sprite, true, 25, width: 24, height: 25, originX: 12, originY: 12);
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
		if (CanShoot(interaction) == false || HudIsHovered()) return;

		Shoot();
	}
	public override void UserJustInteractedWithKey(Gear.Key key, Gear.Interaction interaction)
	{
		if (CanShoot(interaction) == false) return;

		Shoot();
	}
	public override void BodyJustCollidedWithBody(Gear.Body bodyA, Gear.Body bodyB)
	{
		var nameA = bodyA.GetUniqueName();

		if (nameA == "ship")
		{
			shipExplosion = true;
			scrollSpeed *= 0.2f;
			if (astFrames.HasUniqueKey(bodyB) == false)
			{
				astFrames.Expand(0, bodyB, 1);
			}
			if (soundOn)
			{
				Gear.Sound.PlayFromCollection("explosion");
			}
		}
		else if (nameA == "shot")
		{
			if (astFrames.HasUniqueKey(bodyB) == false)
			{
				astFrames.Expand(0, bodyB, 1);
			}
		}
	}

	void InitializeSounds()
	{
		Gear.Sound.CreateCollection("shot");
		Gear.Sound.AddToCollection("laser (1)", "shot");
		Gear.Sound.AddToCollection("laser (2)", "shot");

		Gear.Sound.CreateCollection("explosion");
		Gear.Sound.AddToCollection("explosion (1)", "explosion");
		Gear.Sound.AddToCollection("explosion (2)", "explosion");
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
		var star = Gear.Body.GetByUniqueName("menu-highscore-star");

		star.DisplaySprite("highscore", width: 57, height: 53);
		play.DisplaySprite("play", width: 71, height: 78, originX: 36, originY: 39, r: 200, g: 200, b: 200);
		playBorder.DisplaySprite("play-border", width: 71, height: 78, originX: 36, originY: 39);
		DisplayNumber("menu-highscore-", highscore);
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
		pause.AddTag("game");

		var ship = new Gear.Body("ship");
		ship.SetPositionXY(-canvasSize.GetW() / 2 + 50, canvasSize.GetH() / 2 + 50);
		ship.DisplaySprite("ship (1)", width: 128, height: 128, originX: 64, originY: 64);
		ship.AddHitboxLine("up", new Gear.Line(new Gear.Point(0, -10), new Gear.Point(30, -10)));
		ship.AddHitboxLine("down", new Gear.Line(new Gear.Point(0, 10), new Gear.Point(30, 10)));
		ship.AddHitboxLine("left", new Gear.Line(new Gear.Point(0, -10), new Gear.Point(0, 10)));
		ship.AddHitboxLine("right", new Gear.Line(new Gear.Point(30, -10), new Gear.Point(30, 10)));
		ship.DisplayHitbox();
		ship.AddTag("game");
		ship.DisplayHitboxMiddlePoint();

		var paused = new Gear.Body("paused");
		paused.AddTag("game");

		var score = new Gear.Body("score");
		score.SetPositionXY(-canvasSize.GetW() / 2 + 5, -canvasSize.GetH() / 2 + 5);
		score.AddTag("game");

		for (int i = 0; i < 5; i++)
		{
			var scoreNumb = new Gear.Body($"score-{i}");
			scoreNumb.SetPositionXY(-canvasSize.GetW() / 2 + 35 + i * 25, -canvasSize.GetH() / 2 + 8);
			scoreNumb.AddTag("game");
		}

		var cd = new Gear.Body("cd");
		var cdBorder = new Gear.Body("cd-border");
		cd.SetPositionXY(-canvasSize.GetW() / 2 + 7, canvasSize.GetH() / 2 - 14);
		cd.DisplaySprite("cd", false, width: 1, height: 9);
		cd.SetSizeWH(canvasSize.GetW() - 14, 9);
		cd.AddTag("game");
		cdBorder.SetPositionXY(-canvasSize.GetW() / 2, canvasSize.GetH() / 2 - 19);
		cdBorder.DisplaySprite("cd-border", false, width: 480, height: 19);
		cdBorder.AddTag("game");
	}
	void StartGame()
	{
		var canvasSize = Gear.Canvas.GetSize();

		paused = false;
		shipExplosion = false;

		currScore = 0;
		shipFrame = 1;
		shotFrame = 1;
		astSpawnChance = 80;
		scrollSpeed = 1;
		dodgeSpeed = 600;
		shotSpeed = 300;
		energy = 0;
		energyRegen = 4;

		var ship = Gear.Body.GetByUniqueName("ship");
		ship.RemoveAllHitboxObstacles();
		ship.SetPositionXY(-canvasSize.GetW() / 2 + 50, canvasSize.GetH() / 2 + 50);

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
	void HideGame()
	{
		energy = 0;
		var gameBodies = Gear.Body.GetAllByTag("game");
		var asts = Gear.Body.GetAllByTag("asteroid");
		var shot = Gear.Body.GetByUniqueName("shot");
		foreach (var body in gameBodies)
		{
			if (body.HasTag("asteroid")) continue;

			body.DisplaySprite("empty", displayed: false);
		}
		foreach (var ast in asts)
		{
			ast.Delete();
		}
		if (shot != null)
		{
			shot.Delete();
		}
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

	bool CanShoot(Gear.Interaction interaction)
	{
		return interaction == Gear.Interaction.Pressed && paused == false && energy == maxEnergy && shipExplosion == false;
	}
	void Shoot()
	{
		var ship = Gear.Body.GetByUniqueName("ship");
		var shot = Gear.Body.GetByUniqueName("shot");
		var shotCreated = shot != null;
		shot = shotCreated == false ? new Gear.Body("shot") : shot;

		shotFrame = 1;
		energy = 0;
		shot.RemoveAllHitboxObstacles();
		shot.SetPosition(ship.GetPosition());
		shot.SetAngle(ship.GetAngle());
		shot.DisplaySprite("shot (1)", width: 64, height: 64, originX: 32, originY: 32);
		if (shotCreated == false)
		{
			shot.AddTag("game");
			shot.AddHitboxLine("up", new Gear.Line(new Gear.Point(0, -10), new Gear.Point(30, -10)));
			shot.AddHitboxLine("down", new Gear.Line(new Gear.Point(0, 10), new Gear.Point(30, 10)));
			shot.AddHitboxLine("left", new Gear.Line(new Gear.Point(0, -10), new Gear.Point(0, 10)));
			shot.AddHitboxLine("right", new Gear.Line(new Gear.Point(30, -10), new Gear.Point(30, 10)));
			shot.DisplayHitbox();
		}
		var asts = Gear.Body.GetAllByTag("asteroid");
		foreach (var ast in asts)
		{
			if (ast.HasHitboxObstacle(shot) == false)
			{
				ast.AddHitboxObstacle(shot);
			}
			if (shot.HasHitboxObstacle(ast) == false)
			{
				shot.AddHitboxObstacle(ast);
			}
		}

		if (soundOn)
		{
			Gear.Sound.PlayFromCollection("shot", pitchPercent: 75);
		}
	}
	void UpdateShot()
	{
		var shot = Gear.Body.GetByUniqueName("shot");
		if (shot == null || shotFrame != 7) return;

		var shotPos = shot.GetPosition();
		var rightScreenEdge = Gear.Canvas.GetSize().GetW() / 2;

		shotPos.MoveAtAngle(shot.GetAngle(), shotSpeed);
		if (shotPos.GetX() > rightScreenEdge)
		{
			shotPos.SetY(1000);
		}
		shot.SetPosition(shotPos);
	}
	void AnimateShotCast()
	{
		var shot = Gear.Body.GetByUniqueName("shot");
		var ship = Gear.Body.GetByUniqueName("ship");
		if (shot == null || ship == null || shotFrame > 6) return;

		var shipAng = ship.GetAngle();
		var shipPos = ship.GetPosition();

		shotFrame++;
		shot.DisplaySprite($"shot ({shotFrame})", width: 64, height: 64, originX: 32, originY: 32);
		shot.SetPosition(shipPos);
		shot.SetAngle(shipAng);
	}
	void AnimateShotFly()
	{
		var shot = Gear.Body.GetByUniqueName("shot");
		if (shot == null || shotFrame != 7 || Gear.Performance.GetTickCount() % 5 != 0) return;

		shot.SetSizeH(-shot.GetSize().GetH());
	}

	void AnimateShip()
	{
		var ship = Gear.Body.GetByUniqueName("ship");
		if (ship == null) return;

		shipFrame++;
		if ((shipExplosion == false && shipFrame > 4))
		{
			shipFrame = 1;
		}
		else if (shipExplosion && shipFrame == 12)
		{
			paused = true;
			scrollSpeed = 0;
			HideGame();
			ShowMenu();
			return;
		}
		if (shipFrame > 11) return;

		ship.DisplaySprite(shipExplosion ? $"ship-explosion ({shipFrame})" : $"ship ({shipFrame})", width: 128, height: 128, originX: 60, originY: 62);
	}
	void UpdateShip()
	{
		var ship = Gear.Body.GetByUniqueName("ship");
		if (ship == null || shipExplosion) return;
		var canvasSize = Gear.Canvas.GetSize();
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

	void UpdateEnergy()
	{
		energy = energy < maxEnergy ? energy + energyRegen : maxEnergy;
		var cd = Gear.Body.GetByUniqueName("cd");
		cd.SetSizeW(energy);
	}

	void SpawnAsteroid()
	{
		var canvasSize = Gear.Canvas.GetSize();
		var ast = new Gear.Body($"ast-{Gear.Performance.GetTickCount()}");
		var sprite = $"asteroid ({Gear.Number.GetRandomized(1, 3)})";
		var ship = Gear.Body.GetByUniqueName("ship");
		var shot = Gear.Body.GetByUniqueName("shot");

		ast.SetPositionXY(canvasSize.GetW(), Gear.Number.GetRandomized(-canvasSize.GetH() / 2, canvasSize.GetH() / 2));
		ast.DisplaySprite(sprite, originX: 32, originY: 32);
		ast.AddTag("asteroid");
		ast.SetAngleA(Gear.Number.GetRandomized(0, 360));

		ast.AddHitboxLine("up", new Gear.Line(new Gear.Point(-24, -24), new Gear.Point(24, -24)));
		ast.AddHitboxLine("down", new Gear.Line(new Gear.Point(-24, 24), new Gear.Point(24, 24)));
		ast.AddHitboxLine("left", new Gear.Line(new Gear.Point(-24, -24), new Gear.Point(-24, 24)));
		ast.AddHitboxLine("right", new Gear.Line(new Gear.Point(24, -24), new Gear.Point(24, 24)));
		ast.DisplayHitbox();
		ast.AddTag("game");
		ast.AddHitboxObstacle(ship);
		ast.DisplayHitboxMiddlePoint();
		ship.AddHitboxObstacle(ast);

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
	void AnimateAsteroids()
	{
		var indexes = astFrames.GetIndexes();
		for (int i = 0; i < indexes.Length; i++)
		{
			var index = indexes[i];
			var frame = astFrames.GetValueAt(index);
			var ast = astFrames.GetUniqueKeyAt(index);
			if (frame > 4)
			{
				if (ast != null)
				{
					ast.Delete();
				}
				continue;
			}
			var size = ast.GetSize();
			ast.SetAngleA(0);
			ast.DisplaySprite($"asteroid-explosion ({frame})", originX: 32, originY: 32);
			ast.SetSize(size);
			astFrames.ReplaceAt(index, frame + 1);
		}
	}

	bool HudIsHovered()
	{
		var hoverCount = 0;
		var hovered = Gear.Body.GetAllHovered();
		foreach (var body in hovered)
		{
			if (body.GetUniqueName() != "ship" && body.HasTag("asteroid") == false && body.SpriteIsDisplayed())
			{
				hoverCount++;
			}
		}

		return hoverCount > 0;
	}
}