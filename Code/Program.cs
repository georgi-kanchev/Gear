public class Program : Gear.Instance
{
	// game creation
	public override Program CreatedGet() => this;

	// some values we need to keep track of throughout the game
	Gear.Direction ballMovDir;
	float ballSpeed = 50;
	Gear.PairNumbers score;

	// updating the game
	public override void EachTick(int tickCount)
	{
		// is this the very first game tick?
		if (tickCount == 1)
		{
			// some initial game settings
			Gear.Canvas.PixelSizeSet(width: 5, height: 5);
			Gear.Window.Show(false);
			Gear.Console.Show();

			// console input starting server/client game
			Gear.Console.Log("type 'server' to start a server or 'client' to connect to a local server: ");
			var input = Gear.Console.InputGet();
			Gear.Console.Clear();
			if (input == "server")
				Gear.Network.ServerStart();
			else if (input == "client")
			{
				Gear.Console.Log("type the IP: ");
				var ip = Gear.Console.InputGet();
				Gear.Network.ClientConnect(uniqueName: "client", ip);
				Gear.Window.Show(true);

				Gear.Text.Display("font", $"0:0", overwrite: true);
			}
			// create all objects
			var canvasWidth = Gear.Canvas.SizeGet().WidthGet();
			var paddleClient1 = new Gear.Body();
			var paddleClient2 = new Gear.Body();
			var ball = new Gear.Body();

			// populate all objects with data
			paddleClient1.SpriteSet(name: "paddle", width: 22, height: 64, originX: 11, originY: 16);
			paddleClient1.PositionSet(x: 11, y: 16);
			paddleClient1.SizeSet(width: 11, height: 32);
			paddleClient1.UniqueNameSet(name: "client");

			paddleClient2.SpriteSet(name: "paddle", width: 22, height: 64, red: 0, green: 255, blue: 0, originY: 16);
			paddleClient2.SizeSet(width: 11, height: 32);
			paddleClient2.PositionSet(x: canvasWidth - 11, y: 16);
			paddleClient2.UniqueNameSet(name: "client1");

			ball.SpriteSet(name: "ball", width: 16, height: 16, originX: 8, originY: 8);
			ball.PositionSet(x: canvasWidth / 2, y: Gear.Canvas.SizeGet().HeightGet() / 2);
			ball.UniqueNameSet("ball");
			ballMovDir = new Gear.Direction();
			ballMovDir.SetToRotationSample(Gear.RotationSamples.Left);
		}

		// is my game connected to the server?
		if (Gear.Network.ClientUniqueNameGet() != default)
		{
			// take the user's input
			var upArrowIsPressed = Gear.Input.KeysPressedGet().Contains(Gear.InputKeys.Up);
			var downArrowIsPressed = Gear.Input.KeysPressedGet().Contains(Gear.InputKeys.Down);

			var myUniqueName = Gear.Network.ClientUniqueNameGet();

			var myPaddle = Gear.Body.PickByUniqueNameGet(myUniqueName);
			var myPaddleSpeed = 50;
			var myPaddleNewPos = myPaddle.PositionGet();
			var myPaddleDir = new Gear.Direction();
			var myPaddleY = myPaddle.PositionGet().YGet();
			var myPaddleHeight = myPaddle.SizeGet().HeightGet();
			var myPaddleUpY = myPaddleY - myPaddleHeight / 2;
			var myPaddleDownY = myPaddleY + myPaddleHeight / 2;

			var canvasheight = Gear.Canvas.SizeGet().HeightGet();

			// update my paddle according to the input
			if (upArrowIsPressed && myPaddleUpY > 0)
				myPaddleDir.SetToRotationSample(Gear.RotationSamples.Up);

			if (downArrowIsPressed && myPaddleDownY < canvasheight)
				myPaddleDir.SetToRotationSample(Gear.RotationSamples.Down);

			myPaddleNewPos.MoveInDirection(myPaddleDir, pixelsPerSecond: myPaddleSpeed);
			myPaddle.PositionSet(myPaddleNewPos.XGet(), myPaddleNewPos.YGet());

			// every third tick...
			if (tickCount % 3 == 0)
			{
				// update everyone including the server about my paddle's position
				var message = $"{myPaddle.PositionGet().XGet()} {myPaddle.PositionGet().YGet()}";
				Gear.Network.ClientMessageSendToServerAndAllClients(message);
			}
		}

		// not connected as a client? => i'm a server, are 2 clients connected to me?
		else if (Gear.Network.ClientsConnectedCountGet() > 1)
		{
			var ball = Gear.Body.PickByUniqueNameGet("ball");
			var ballNewPos = ball.PositionGet();

			// moving the ball
			ballNewPos.MoveInDirection(ballMovDir, pixelsPerSecond: ballSpeed);
			ball.PositionSet(ballNewPos.XGet(), ballNewPos.YGet());

			// some data needed to check collision
			var canvasWidth = Gear.Canvas.SizeGet().WidthGet();
			var canvasHeight = Gear.Canvas.SizeGet().HeightGet();

			var paddleL = Gear.Body.PickByUniqueNameGet("client");
			var paddleR = Gear.Body.PickByUniqueNameGet("client1");

			var randomRot = new float[] { -500, -250, 0, 250, 500 };
			var ballNewRot = randomRot[(int)Gear.Number.RandomizedGet(0, randomRot.Length - 1, 0)];

			var ballDistPaddleL = ball.PositionGet().DistanceToPointGet(paddleL.PositionGet());
			var ballDistPaddleR = ball.PositionGet().DistanceToPointGet(paddleR.PositionGet());

			var ballIsMovR = ballMovDir.EndPointGet().XGet() > 0;
			var ballIsMovL = ballMovDir.EndPointGet().XGet() < 0;
			var ballIsMovU = ballMovDir.EndPointGet().YGet() < 0;
			var ballIsMovD = ballMovDir.EndPointGet().YGet() > 0;

			var ballHeight = ball.SizeGet().HeightGet();
			var ballNewX = ball.PositionGet().XGet();
			var ballNewY = ball.PositionGet().YGet();
			var ballDownY = ballNewY - ballHeight / 2;
			var ballUpY = ballNewY + ballHeight / 2;

			// is the ball colliding with any of the paddles?
			if ((ballDistPaddleR < 20 && ballIsMovR) ||
				(ballDistPaddleL < 20 && ballIsMovL))
			{
				ballMovDir.ReverseHorizontally();
				ballMovDir.Rotate(ballNewRot);
				ballSpeed += 5;
			}
			// is the ball going off-secreen upwards or downwards?
			if ((ballDownY < 0 && ballIsMovU) ||
				(ballUpY > canvasHeight && ballIsMovD))
				ballMovDir.ReverseVertically();

			// is the ball going off-screen to the left?
			if (ballNewX < 0)
				score.Set(score.FirstGet(), score.SecondGet() + 1);
			// or to the right?
			else if (ballNewX > canvasWidth)
				score.Set(score.FirstGet() + 1, score.SecondGet());

			// is the ball going off-screen to the left or right?
			if (ballNewX < 0 || ballNewX > canvasWidth)
			{
				// restart the round
				ball.PositionSet(canvasWidth / 2, canvasHeight / 2);
				ballMovDir.SetToRotationSample(
					ballNewX < 0 ? Gear.RotationSamples.Right : Gear.RotationSamples.Left);
				ballMovDir.Rotate(ballNewRot);
				ballSpeed = 50;

				// update all clients with the current score
				var message = $"score {score.FirstGet()} {score.SecondGet()}";
				Gear.Network.ServerMessageSendToAllClients(message);
			}

			// every third tick...
			if (tickCount % 3 == 0)
			{
				// update all clients with the current ball position
				var message = $"ball {ballNewX} {ballNewY}";
				Gear.Network.ServerMessageSendToAllClients(message);
			}
		}
	}

	// in case a network message get received and has to be catched
	public override void NetworkMessageJustReceived(string sender, string message)
	{
		var packetHasSender = sender != null;

		// did a client send that message?
		if (packetHasSender)
		{
			// the only kind of client message is about his paddle position so get the data from it
			var messageParts = message.Split();
			var senderPaddle = Gear.Body.PickByUniqueNameGet(sender);
			var senderPaddleX = Gear.Number.FromTextGet(messageParts[0]);
			var senderPaddleY = Gear.Number.FromTextGet(messageParts[1]);

			// update the paddle position
			senderPaddle.PositionSet(senderPaddleX, senderPaddleY);
		}

		// did the server sent that message?
		else
		{
			// the server can send us different kinds of messages
			var messageParts = message.Split();

			// is the message about the ball's position?
			if (messageParts[0] == "ball")
			{
				var ballX = Gear.Number.FromTextGet(messageParts[1]);
				var ballY = Gear.Number.FromTextGet(messageParts[2]);
				var ball = Gear.Body.PickByUniqueNameGet("ball");

				// update the ball's position
				ball.PositionSet(ballX, ballY);
			}

			// or the score?
			else if (messageParts[0] == "score")
			{
				var scoreL = Gear.Number.FromTextGet(messageParts[1]);
				var scoreR = Gear.Number.FromTextGet(messageParts[2]);

				// update the score
				score.Set(scoreL, scoreR);
				// and the text
				var scoreText = $"{scoreL}:{scoreR}";
				Gear.Text.Display("font", scoreText, overwrite: true);
			}
		}
	}
}