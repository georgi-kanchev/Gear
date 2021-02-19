public class Program : Gear.Instance
{
	public override Program Created_Get() => this;
	public override string[] Loading_Screen_Prepare() => new string[] { };
	public override void Each_Loading_Screen_Update(int percent_loaded) { }

	Gear.Direction ball_movement_direction;
	public override void Each_Tick(int tick_count)
	{
		if (tick_count == 1)
		{
			Gear.Canvas.Pixel_Size_Set(5, 5);
			//Gear.Window.Show(false);
			Gear.Console.Show();

			Gear.Console.Log("type 'server' to start a server or 'client' to connect to a local server: ");
			var input = Gear.Console.Input_Get();
			Gear.Console.Clear();

			if (input == "server") Gear.Network.Server_Start();
			else if (input == "client")
			{
				Gear.Network.Client_Connect(unique_name: "client", ip: Gear.Network.Server_IP_Same_Device_Get());
			}
			Gear.Window.Show(true);
			var paddle_client_1 = new Gear.Body();
			paddle_client_1.Sprite_Set(name: "paddle", width: 22, height: 64, red: 255, green: 255, blue: 255);
			paddle_client_1.Size_Set(width: 11, height: 32);
			paddle_client_1.Unique_Name_Set(name: "client");

			var paddle_client_2 = new Gear.Body();
			paddle_client_2.Sprite_Set(name: "paddle", width: 22, height: 64, red: 0, green: 255, blue: 0);
			paddle_client_2.Size_Set(width: 11, height: 32);
			paddle_client_2.Position_Set(x: Gear.Canvas.Size_Get().Width_Get() - 11, y: 0);
			paddle_client_2.Unique_Name_Set(name: "client1");

			var ball = new Gear.Body();
			ball.Sprite_Set(name: "ball", width: 16, height: 16, origin_x: 8, origin_y: 8);
			ball.Position_Set(x: Gear.Canvas.Size_Get().Width_Get() / 2, y: Gear.Canvas.Size_Get().Height_Get() / 2);
			ball.Unique_Name_Set("ball");
			ball_movement_direction = new Gear.Direction();
			ball_movement_direction.Set_To_Rotation_Sample(Gear.Rotation_Samples.Right);
		}

		if (Gear.Network.Client_Unique_Name_Get() != default)
		{
			var my_unique_name = Gear.Network.Client_Unique_Name_Get();
			var my_paddle = Gear.Body.Pick_By_Unique_Name_Get(my_unique_name);
			var new_position = my_paddle.Position_Get();
			var direction = new Gear.Direction();
			var speed = 50;

			if (Gear.Input.Keys_Pressed_Get().Contains(Gear.Input_Keys.Up) &&
				my_paddle.Position_Get().Y_Get() > 0)
			{
				direction.Set_To_Rotation_Sample(Gear.Rotation_Samples.Up);
			}
			if (Gear.Input.Keys_Pressed_Get().Contains(Gear.Input_Keys.Down) &&
				my_paddle.Position_Get().Y_Get() < Gear.Canvas.Size_Get().Height_Get() - my_paddle.Size_Get().Height_Get())
			{
				direction.Set_To_Rotation_Sample(Gear.Rotation_Samples.Down);
			}
			new_position.Move_In_Direction(direction, pixels_per_second: speed);
			my_paddle.Position_Set(new_position.X_Get(), new_position.Y_Get());

			Gear.Network.Client_Message_Send_To_Server_And_All_Clients($"{my_paddle.Position_Get().X_Get()} {my_paddle.Position_Get().Y_Get()}");
		}
		else if (Gear.Network.Clients_Connected_Count_Get() > 1)
		{
			var ball = Gear.Body.Pick_By_Unique_Name_Get("ball");
			var ball_new_position = ball.Position_Get();
			var ball_speed = 20;
			ball_new_position.Move_In_Direction(ball_movement_direction, pixels_per_second: ball_speed);

			ball.Position_Set(ball_new_position.X_Get(), ball_new_position.Y_Get());

			var left_paddle = Gear.Body.Pick_By_Unique_Name_Get("client");
			var right_paddle = Gear.Body.Pick_By_Unique_Name_Get("client1");

			var distance_to_right_paddle = ball.Position_Get().Distance_To_Point_Get(right_paddle.Position_Get());
			var distance_to_left_paddle = ball.Position_Get().Distance_To_Point_Get(left_paddle.Position_Get());

			if (distance_to_right_paddle < 12 || distance_to_left_paddle < 12) ball_movement_direction.Reverse_Horizontally();

			Gear.Network.Server_Message_Send_To_All_Clients($"{ball.Position_Get().X_Get()} {ball.Position_Get().Y_Get()}");
		}
	}
	public override void Event_Just_Occured(Gear.Event_Type event_type, object parameter)
	{
		if (event_type == Gear.Event_Type.Network_Packet_Received_From_Client)
		{
			var packet = (Gear.Network_Packet) parameter;
			var sender_unique_name = packet.Sender_Get();
			var sender_paddle = Gear.Body.Pick_By_Unique_Name_Get(sender_unique_name);
			var message = packet.Message_Get().Split();
			var sender_paddle_x = Gear.Number.From_Text_Get(message[0]);
			var sender_paddle_y = Gear.Number.From_Text_Get(message[1]);

			sender_paddle.Position_Set(sender_paddle_x, sender_paddle_y);
		}

		if (event_type == Gear.Event_Type.Network_Packet_Received_From_Server)
		{
			var packet = (Gear.Network_Packet) parameter;
			var message = packet.Message_Get().Split();
			var ball_x = Gear.Number.From_Text_Get(message[0]);
			var ball_y = Gear.Number.From_Text_Get(message[1]);
			var ball = Gear.Body.Pick_By_Unique_Name_Get("ball");

			ball.Position_Set(ball_x, ball_y);
		}
	}
}