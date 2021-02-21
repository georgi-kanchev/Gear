public class Program : Gear.Instance
{
	// game creation and loading screen which we are not interested in
	#region boring
	public override Program Created_Get() => this;
	public override string[] Loading_Screen_Prepare() => new string[] { };
	public override void Each_Loading_Screen_Update(int percent_loaded) { }
	#endregion

	// some values we need to keep track of throughout the game
	Gear.Direction ball_movement_direction;
	float ball_speed = 50;
	Gear.Pair_Numbers score;

	// updating the game
	public override void Each_Tick(int tick_count)
	{
		// is this the very first game tick?
		if (tick_count == 1)
		{
			// some initial game settings
			Gear.Canvas.Pixel_Size_Set(width: 5, height: 5);
			Gear.Window.Show(false);
			Gear.Console.Show();

			// console input starting server/client game
			Gear.Console.Log("type 'server' to start a server or 'client' to connect to a local server: ");
			var input = Gear.Console.Input_Get();
			Gear.Console.Clear();
			if (input == "server")
				Gear.Network.Server_Start();
			else if (input == "client")
			{
				Gear.Console.Log("type the IP: ");
				var ip = Gear.Console.Input_Get();
				Gear.Network.Client_Connect(unique_name: "client", ip);
				Gear.Window.Show(true);

				Gear.Text.Display("font", $"0:0", overwrite: true);
			}
			// create all objects
			var canvas_width = Gear.Canvas.Size_Get().Width_Get();
			var paddle_client_1 = new Gear.Body();
			var paddle_client_2 = new Gear.Body();
			var ball = new Gear.Body();

			// populate all objects with data
			paddle_client_1.Sprite_Set(name: "paddle", width: 22, height: 64, origin_x: 11, origin_y: 16);
			paddle_client_1.Position_Set(x: 11, y: 16);
			paddle_client_1.Size_Set(width: 11, height: 32);
			paddle_client_1.Unique_Name_Set(name: "client");

			paddle_client_2.Sprite_Set(name: "paddle", width: 22, height: 64, red: 0, green: 255, blue: 0, origin_y: 16);
			paddle_client_2.Size_Set(width: 11, height: 32);
			paddle_client_2.Position_Set(x: canvas_width - 11, y: 16);
			paddle_client_2.Unique_Name_Set(name: "client1");

			ball.Sprite_Set(name: "ball", width: 16, height: 16, origin_x: 8, origin_y: 8);
			ball.Position_Set(x: canvas_width / 2, y: Gear.Canvas.Size_Get().Height_Get() / 2);
			ball.Unique_Name_Set("ball");
			ball_movement_direction = new Gear.Direction();
			ball_movement_direction.Set_To_Rotation_Sample(Gear.Rotation_Samples.Left);
		}

		// is my game connected to the server?
		if (Gear.Network.Client_Unique_Name_Get() != default)
		{
			var up_arrow_pressed = Gear.Input.Keys_Pressed_Get().Contains(Gear.Input_Keys.Up);
			var down_arrow_pressed = Gear.Input.Keys_Pressed_Get().Contains(Gear.Input_Keys.Down);

			var my_unique_name = Gear.Network.Client_Unique_Name_Get();

			var my_paddle = Gear.Body.Pick_By_Unique_Name_Get(my_unique_name);
			var my_paddle_speed = 50;
			var my_paddle_new_position = my_paddle.Position_Get();
			var my_paddle_direction = new Gear.Direction();
			var my_paddle_y = my_paddle.Position_Get().Y_Get();
			var my_paddle_height = my_paddle.Size_Get().Height_Get();
			var my_paddle_upper_side_y = my_paddle_y - my_paddle_height / 2;
			var my_paddle_down_side_y = my_paddle_y + my_paddle_height / 2;

			var canvas_height = Gear.Canvas.Size_Get().Height_Get();

			if (up_arrow_pressed && my_paddle_upper_side_y > 0)
				my_paddle_direction.Set_To_Rotation_Sample(Gear.Rotation_Samples.Up);

			if (down_arrow_pressed && my_paddle_down_side_y < canvas_height)
				my_paddle_direction.Set_To_Rotation_Sample(Gear.Rotation_Samples.Down);

			my_paddle_new_position.Move_In_Direction(my_paddle_direction, pixels_per_second: my_paddle_speed);
			my_paddle.Position_Set(my_paddle_new_position.X_Get(), my_paddle_new_position.Y_Get());

			//

			if (tick_count % 3 == 0)
			{
				var message = $"{my_paddle.Position_Get().X_Get()} {my_paddle.Position_Get().Y_Get()}";
				Gear.Network.Client_Message_Send_To_Server_And_All_Clients(message);
			}
		}

		// not connected as a client? => i'm a server, are 2 clients connected to me?
		else if (Gear.Network.Clients_Connected_Count_Get() > 1)
		{
			var ball = Gear.Body.Pick_By_Unique_Name_Get("ball");
			var ball_new_position = ball.Position_Get();

			// moving the ball
			ball_new_position.Move_In_Direction(ball_movement_direction, pixels_per_second: ball_speed);
			ball.Position_Set(ball_new_position.X_Get(), ball_new_position.Y_Get());

			// some data needed to check collision
			var canvas_width = Gear.Canvas.Size_Get().Width_Get();
			var canvas_height = Gear.Canvas.Size_Get().Height_Get();

			var left_paddle = Gear.Body.Pick_By_Unique_Name_Get("client");
			var right_paddle = Gear.Body.Pick_By_Unique_Name_Get("client1");

			var random_rotations = new float[] { -500, -250, 0, 250, 500 };
			var ball_new_rotation = random_rotations[(int)Gear.Number.Randomized_Get(0, random_rotations.Length - 1, 0)];

			var ball_distance_to_right_paddle = ball.Position_Get().Distance_To_Point_Get(right_paddle.Position_Get());
			var ball_distance_to_left_paddle = ball.Position_Get().Distance_To_Point_Get(left_paddle.Position_Get());

			var ball_is_moving_right = ball_movement_direction.End_Point_Get().X_Get() > 0;
			var ball_is_moving_left = ball_movement_direction.End_Point_Get().X_Get() < 0;
			var ball_is_moving_up = ball_movement_direction.End_Point_Get().Y_Get() < 0;
			var ball_is_moving_down = ball_movement_direction.End_Point_Get().Y_Get() > 0;

			var ball_height = ball.Size_Get().Height_Get();
			var ball_new_x = ball.Position_Get().X_Get();
			var ball_new_y = ball.Position_Get().Y_Get();
			var ball_down_side_y = ball_new_y - ball_height / 2;
			var ball_upper_side_y = ball_new_y + ball_height / 2;

			// is the ball colliding with any of the paddles?
			if ((ball_distance_to_right_paddle < 20 && ball_is_moving_right) ||
				(ball_distance_to_left_paddle < 20 && ball_is_moving_left))
			{
				ball_movement_direction.Reverse_Horizontally();
				ball_movement_direction.Rotate(ball_new_rotation);
				ball_speed += 5;
			}
			// is the ball going off-secreen upwards or downwards?
			if ((ball_down_side_y < 0 && ball_is_moving_up) ||
				(ball_upper_side_y > canvas_height && ball_is_moving_down))
					ball_movement_direction.Reverse_Vertically();

			// is the ball going off-screen to the left?
			if (ball_new_x < 0)
				score.Set(score.First_Get(), score.Second_Get() + 1);
			// or to the right?
			else if (ball_new_y > canvas_width)
				score.Set(score.First_Get() + 1, score.Second_Get());

			// is the ball going off-screen to the left or right?
			if (ball_new_x < 0 || ball_new_x > canvas_width)
			{
				// restart the round
				ball.Position_Set(canvas_width / 2, canvas_height / 2);
				ball_movement_direction.Set_To_Rotation_Sample(
					ball.Position_Get().X_Get() < 0 ? Gear.Rotation_Samples.Right : Gear.Rotation_Samples.Left);
				ball_movement_direction.Rotate(ball_new_rotation);
				ball_speed = 50;

				// update all clients with the current score
				var message = $"score {score.First_Get()} {score.Second_Get()}";
				Gear.Network.Server_Message_Send_To_All_Clients(message);
			}

			// every third tick...
			if (tick_count % 3 == 0)
			{
				// update all clients with the current ball position
				var message = $"ball {ball_new_x} {ball_new_y}";
				Gear.Network.Server_Message_Send_To_All_Clients(message);
			}
		}
	}

	// something interesting happened, but what?
	public override void Event_Just_Occured(Gear.Event_Type event_type, object parameter)
	{
		// did a client sent us a message?
		if (event_type == Gear.Event_Type.Network_Packet_Received_From_Client)
		{
			// the only kind of client message is about his paddle position so get the data from it
			var packet = (Gear.Network_Packet) parameter;
			var sender_unique_name = packet.Sender_Get();
			var sender_paddle = Gear.Body.Pick_By_Unique_Name_Get(sender_unique_name);
			var message = packet.Message_Get().Split();
			var sender_paddle_x = Gear.Number.From_Text_Get(message[0]);
			var sender_paddle_y = Gear.Number.From_Text_Get(message[1]);

			// update the paddle position
			sender_paddle.Position_Set(sender_paddle_x, sender_paddle_y);
		}

		// did the server sent us a message?
		if (event_type == Gear.Event_Type.Network_Packet_Received_From_Server)
		{
			// the server can send us different kinds of messages
			var packet = (Gear.Network_Packet) parameter;
			var message = packet.Message_Get().Split();

			// is the message about the ball's position?
			if (message[0] == "ball")
			{
				var ball_x = Gear.Number.From_Text_Get(message[1]);
				var ball_y = Gear.Number.From_Text_Get(message[2]);
				var ball = Gear.Body.Pick_By_Unique_Name_Get("ball");

				// update the ball's position
				ball.Position_Set(ball_x, ball_y);
			}

			// or the score?
			else if (message[0] == "score")
			{
				var score_left = Gear.Number.From_Text_Get(message[1]);
				var score_right = Gear.Number.From_Text_Get(message[2]);

				// update the score
				score.Set(score_left, score_right);
				// and the text
				var score_text = $"{score.First_Get()}:{score.Second_Get()}";
				Gear.Text.Display("font", score_text, overwrite: true);
			}
		}
	}
}