using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;
using NetCoreServer;
using TcpClient = NetCoreServer.TcpClient;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Mono.Nat;
using System.Threading.Tasks;

public static class Gear
{
	///<summary>
	///text, <paramref name="param"/>, <see cref="char"/>, <typeparamref name="Type"/>
	///</summary>
	private static void Summary_Example() { }

	#region Sleep Prevention
	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
	[Flags]
	private enum EXECUTION_STATE : uint
	{
		ES_AWAYMODE_REQUIRED = 0x00000040,
		ES_CONTINUOUS = 0x80000000,
		ES_DISPLAY_REQUIRED = 0x00000002,
		ES_SYSTEM_REQUIRED = 0x00000001
		// Legacy flag, should not be used.
		// ES_USER_PRESENT = 0x00000004
	}
	#endregion
	#region Window Maximization
	[DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void SDL_MaximizeWindow(IntPtr window);
	#endregion
	#region Show Console
	private static void Form1_Load(object sender, EventArgs e)
	{
		AllocConsole();
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	static extern bool AllocConsole();
	#endregion
	#region Data
	private static Game game;
	private static Program program;
	private static GraphicsDeviceManager graphics;
	private static SpriteBatch sprite_batch;
	private static RenderTarget2D render_target;
	private static SamplerState render_sampler_state;
	private static Canvas.Pixel_Filter render_pixel_filter;
	private static Server server;
	private static Client client;

	private enum Message_Type
	{
		Connection, Unique_Name_Change, Client_Connected, Client_Disconnected, Client_Online, Message_To_All, Message_To_Client
	}

	private static PerformanceCounter ram_available = new PerformanceCounter("Memory", "Available MBytes");
	private static PerformanceCounter ram_used_percent = new PerformanceCounter("Memory", "% Committed Bytes In Use");

	private static Dictionary<string, SpriteFont> fonts = new Dictionary<string, SpriteFont>();
	private static Dictionary<string, Texture2D> sprites = new Dictionary<string, Texture2D>(), sprite_outlines = new Dictionary<string, Texture2D>(), sprite_fills = new Dictionary<string, Texture2D>();
	private static Dictionary<string, SoundEffectInstance> sounds = new Dictionary<string, SoundEffectInstance>();
	private static Dictionary<string, SoundEffect> sounds_raw = new Dictionary<string, SoundEffect>();
	private static Dictionary<string, Song> melodies = new Dictionary<string, Song>();
	private static Dictionary<string, List<string>> last_messages = new Dictionary<string, List<string>>();
	private static Dictionary<string, bool> gates = new Dictionary<string, bool>(), signal_pauses = new Dictionary<string, bool>();
	private static Dictionary<string, int> gate_entries_count = new Dictionary<string, int>();
	private static Dictionary<string, string> client_ids = new Dictionary<string, string>();
	private static Dictionary<string, float> signal_end_times = new Dictionary<string, float>(), signal_start_times = new Dictionary<string, float>(), signal_delays = new Dictionary<string, float>();

	private static List<Input.Keys> last_frame_keys_pressed = new List<Input.Keys>(), keys_just_pressed = new List<Input.Keys>(), keys_just_released = new List<Input.Keys>();
	private static List<Body> bodies_all = new List<Body>();
	private static List<float> tps_averages = new List<float>(), fps_averages = new List<float>();
	private static List<string> client_unique_names = new List<string>();

	private static int tick, frame, frame_rendered, tps_average_index, fps_average_index, loading_percent, loading_screen_update_per_files = 10, loaded_files, content_file_count, pixel_width, pixel_height, server_port = 1234;
	private static bool text_display_draw, loading = true, pause_unfocus, render, sleep_prevented, console_shown, client_is_connected, server_is_running;
	private static float text_display_scale, tps, tps_average, fps, fps_average, ticks_delta_time, frames_delta_time, time;
	private static string text_display_font, text_display_message, main_dir = AppDomain.CurrentDomain.BaseDirectory, console_log, connect_to_server_info, client_unique_name;

	private static DateTime last_tick_time, last_frame_time;
	private static Color background_color = Color.Black;
	private static Microsoft.Xna.Framework.Point canvas_size = new Microsoft.Xna.Framework.Point(1920, 1080), screen_size = new Microsoft.Xna.Framework.Point(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
	private static Vector2 camera_position;
	#endregion

	public abstract class Instance : Game
	{
		[STAThread]
		public static void Main()
		{
			using (var game = new Program())
			{
				program = game;
				game.Run();
			}
		}
		public Instance()
		{
			graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
				PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
			};
			Content.RootDirectory = "Content";
			game = Created_Get();
		}

		/// <summary>
		/// - Example code setup:<br></br>
		/// <paramref name="public"/> <paramref name="override"/> <see cref="Program"/> <typeparamref name="Create"/>() => <paramref name="this"/>;<br></br>
		/// </summary>
		public abstract Program Created_Get();
		/// <summary>
		/// - Has to return a <see cref="string"/>[] containing <paramref name="folder"/>/<paramref name="name"/>.<paramref name="extension"/> for the small amount of content files that need to be loaded before the <typeparamref name="Loading"/> <typeparamref name="Screen"/> so they can be used during <see cref="Each_Loading_Screen_Update"/> while the rest of the content files are being loaded.<br></br>
		/// - The <paramref name="folder"/> part of the path is skipped if the file is directly inside the Content folder.<br></br><br></br>
		/// - Example code setup:<br></br>
		/// // the following code pre-loads two files<br></br>
		/// // the first with path: Content/folder/name.extension<br></br>
		/// // the second with path: Content/name.extension<br></br>
		/// <paramref name="public"/> <paramref name="override"/> <see cref="string"/>[] <typeparamref name="Loading_Screen_Prepare"/>() => <paramref name="new"/> <see cref="string"/>[] { "<typeparamref name="folder"/>/<typeparamref name="name"/>.<typeparamref name="extension"/>", "<typeparamref name="name"/>.<typeparamref name="extension"/>" };<br></br>
		/// </summary>
		public abstract string[] Loading_Screen_Prepare();
		/// <summary>
		/// - The place for the code that displays and updates the visuals of the <typeparamref name="Loading"/> <typeparamref name="Screen"/> with the small amount of content files loaded through <see cref="Loading_Screen_Prepare"/>. The pre-loaded files can be used by each <see cref="Body"/>.<br></br>
		/// - The <see cref="int"/> <paramref name="parameter"/> contains the loading %.
		/// </summary>
		public abstract void Each_Loading_Screen_Update(int percent_loaded);
		/// <summary>
		/// - The place for all program code.<br></br>
		/// - The <see cref="int"/> <paramref name="parameter"/> contains the tick count.<br></br><br></br>
		/// - The tick count and information about ticks/frames can be checked through <see cref="Performance.Ticks_Count_Get"/>.
		/// </summary>
		public abstract void Each_Tick(int tick_count);

		protected override void Initialize()
		{
			sprite_batch = new SpriteBatch(game.GraphicsDevice);

			graphics.PreferredBackBufferWidth = screen_size.X;
			graphics.PreferredBackBufferHeight = screen_size.Y;
			graphics.HardwareModeSwitch = false;
			graphics.IsFullScreen = true;
			game.Window.Position = new Microsoft.Xna.Framework.Point(0, 0);

			render_sampler_state = SamplerState.PointWrap;

			render_target = new RenderTarget2D(game.GraphicsDevice, screen_size.X, screen_size.Y, false, game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
			Gear.Canvas.Pixel_Size_Set(1, 1);

			graphics.ApplyChanges();
			game.Window.Title = "Gear";
			game.IsMouseVisible = true;

			// start maximized
			//var form = (Form)Control.FromHandle(Window.Handle);
			//form.WindowState = FormWindowState.Maximized;

			//anti pc sleep
			SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED);

			var content = program.Loading_Screen_Prepare();
			foreach (var file in content)
			{
				Load_File(file);
			}
			Count_Content_Files();
			Load_All_Content();

			base.Initialize();
		}
		protected override void Update(GameTime gameTime)
		{
			if (pause_unfocus && game.IsActive == false)
			{
				return;
			}
			if (loading)
			{
				Load_All_Content();
				program.Each_Loading_Screen_Update(loading_percent);
			}
			else
			{
				tick++;
				Advance_Tick_Time();
				Update_On_Keys();
				program.Each_Tick(tick);
			}
			base.Update(gameTime);
		}
		private static void Advance_Tick_Time()
		{
			var delta = last_tick_time == default ? default : DateTime.Now - last_tick_time;
			ticks_delta_time = (float)delta.TotalSeconds;
			tps = 60 / ((float)delta.TotalSeconds * 60);
			tps = double.IsInfinity(tps) ? tps_average : tps;
			Advance_TPS_Average();
			time += ticks_delta_time;
			last_tick_time = DateTime.Now;
		}
		private static void Advance_TPS_Average()
		{
			if (tps_average_index == 60)
			{
				tps_average_index = 0;
			}
			if (tps_averages.Contains(tps))
			{
				return;
			}
			if (tps_average_index == tps_averages.Count)
			{
				tps_averages.Add(tps);
			}
			tps_averages[tps_average_index] = tps;
			tps_average = tps_averages.Average();
			tps_average_index++;
		}

		private static void Update_On_Keys()
		{
			var keysPressed = Input.Keys_Pressed_Get();

			keys_just_pressed.Clear();
			keys_just_released.Clear();
			foreach (var key in keysPressed)
			{
				if (last_frame_keys_pressed.Contains(key) == false)
				{
					keys_just_pressed.Add(key);
				}
			}
			foreach (var key in last_frame_keys_pressed)
			{
				if (keysPressed.Contains(key) == false)
				{
					keys_just_released.Add(key);
				}
			}

			last_frame_keys_pressed = Input.Keys_Pressed_Get();
		}

		protected override void Draw(GameTime gameTime)
		{
			frame++;
			if (render == false) return;
			frame_rendered++;

			sprite_batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, render_sampler_state, DepthStencilState.Default, RasterizerState.CullNone);
			GraphicsDevice.SetRenderTarget(render_target);
			GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };

			GraphicsDevice.Clear(background_color);

			// draw =======================================================
			Advance_Frame_Time();
			Draw_All_Bodies();
			Draw_Text_Display();
			// draw =======================================================

			GraphicsDevice.SetRenderTarget(null);
			var scale = new Vector2(pixel_width, pixel_height);
			sprite_batch.Draw(render_target, Vector2.Zero, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
			render = false;
			sprite_batch.End();
			base.Draw(gameTime);
		}
		private static void Advance_Frame_Time()
		{
			var delta = last_frame_time == default ? default : DateTime.Now - last_frame_time;
			frames_delta_time = (float)delta.TotalSeconds;
			fps = 60 / ((float)delta.TotalSeconds * 60);
			fps = double.IsInfinity(fps) ? fps_average : fps;
			Advance_FPS_Average();
			last_frame_time = DateTime.Now;
		}
		private static void Advance_FPS_Average()
		{
			if (fps_average_index == 60)
			{
				fps_average_index = 0;
			}
			if (fps_averages.Contains(fps))
			{
				return;
			}
			if (fps_average_index == fps_averages.Count)
			{
				fps_averages.Add(fps);
			}
			fps_averages[fps_average_index] = fps;
			fps_average = fps_averages.Average();
			fps_average_index++;
		}

		private static void Draw_All_Bodies()
		{
			foreach (var body in bodies_all)
			{
				var sprite = body.Sprite_Name_Get();
				if (sprite == null)
				{
					continue;
				}
				var sprite_shown = body.Sprite_Is_Shown_Check();
				var tile_index = new Microsoft.Xna.Framework.Point(body.Sprite_Index_Horizontal_Get(), body.Sprite_Index_Vertical_Get());
				var pos = new Vector2(body.Position_X_Get(), body.Position_Y_Get()) + camera_position;
				var size = new Vector2(body.Size_Width_Get(), body.Size_Height_Get()).ToPoint();
				var sprite_size = new Vector2(body.Sprite_Width_Get(), body.Sprite_Height_Get());
				var scale = size.ToVector2() / sprite_size;
				var origin = new Vector2(body.Sprite_Origin_X_Get(), body.Sprite_Origin_Y_Get());
				var color = new Color(body.Sprite_Red_Get(), body.Sprite_Green_Get(), body.Sprite_Blue_Get(), body.Sprite_Opacity_Get());
				var boundaries_sprite = new Texture2D(graphics.GraphicsDevice, 1, 1);
				var origin_sprite = new Texture2D(graphics.GraphicsDevice, 1, 1);
				var angle_sprite = new Texture2D(graphics.GraphicsDevice, 1, 1);
				var data = new Color[1] { Color.White };
				boundaries_sprite.SetData(data);
				origin_sprite.SetData(data);
				angle_sprite.SetData(data);

				if (sprite_shown)
				{
					_Draw_Tile(sprites[sprite], pos - origin, tile_index, body.Sprite_Grid_Size_Get(), (size.ToVector2() / scale).ToPoint(), Vector2.Zero, scale, color, body.Angle_Get(), SpriteEffects.None);
				}

				var boundaries_color = new Color(body.Boundaries_Red_Get(), body.Boundaries_Green_Get(), body.Boundaries_Blue_Get());
				if (boundaries_sprite != null && body.Boundaries_Are_Shown_Check())
				{
					_Draw_Tile(boundaries_sprite, pos - origin, Microsoft.Xna.Framework.Point.Zero, 0, new Microsoft.Xna.Framework.Point(size.X, 1), Vector2.Zero, Vector2.One, boundaries_color, body.Angle_Get(), SpriteEffects.None);
					_Draw_Tile(boundaries_sprite, pos - origin, Microsoft.Xna.Framework.Point.Zero, 0, new Microsoft.Xna.Framework.Point(1, size.Y), Vector2.Zero, Vector2.One, boundaries_color, body.Angle_Get(), SpriteEffects.None);
				}

				var angle_color = new Color(body.Angle_Red_Get(), body.Angle_Green_Get(), body.Angle_Blue_Get());
				if (angle_sprite != null && body.Angle_Is_Shown_Check())
					_Draw_Tile(angle_sprite, pos, Microsoft.Xna.Framework.Point.Zero, 0, new Microsoft.Xna.Framework.Point((int)(size.X * 1.1f), 1), Vector2.Zero, Vector2.One, angle_color, body.Angle_Get(), SpriteEffects.None);

				var origin_color = new Color(body.Origin_Red_Get(), body.Origin_Green_Get(), body.Origin_Blue_Get());
				if (origin_sprite != null && body.Origin_Is_Shown_Check())
					_Draw_Tile(origin_sprite, pos, Microsoft.Xna.Framework.Point.Zero, 0, new Microsoft.Xna.Framework.Point(1, 1), Vector2.Zero, Vector2.One, origin_color, body.Angle_Get(), SpriteEffects.None);

				boundaries_sprite.Dispose();
				angle_sprite.Dispose();
				origin_sprite.Dispose();
			}
		}
		private static void Draw_Text_Display()
		{
			if (text_display_font != null && text_display_draw && fonts.ContainsKey(text_display_font) && string.IsNullOrWhiteSpace(text_display_message) == false)
			{
				var font_size = fonts[text_display_font].MeasureString("a") / 18 * text_display_scale;
				sprite_batch.DrawString(fonts[text_display_font], text_display_message, new Vector2(font_size.Y, font_size.Y), Color.Black, 0, Vector2.Zero, text_display_scale, SpriteEffects.None, 0);
				sprite_batch.DrawString(fonts[text_display_font], text_display_message, new Vector2(0, 0), Color.White, 0, Vector2.Zero, text_display_scale, SpriteEffects.None, 0);
			}
		}

		private static void Count_Content_Files()
		{
			var directories = Directory.GetDirectories($"{main_dir}\\Content").ToList();
			for (int i = 0; i < directories.Count; i++)
			{
				Count_Folder(directories[i]);
			}
			Count_Folder($"{main_dir}\\Content");
			loading_screen_update_per_files = (int)Math.Ceiling(content_file_count / 10d);
		}
		private static void Count_Folder(string folder)
		{
			if (Directory.Exists(folder) == false)
			{
				return;
			}

			var files = Directory.GetFiles(folder);
			var data_files = new List<string>();

			for (int i = 0; i < files.Length; i++)
			{
				if (files[i].Contains(".png") || files[i].Contains(".spritefont"))
				{
					data_files.Add(files[i]);
				}
			}

			content_file_count += data_files.Count;

			var currentDirectories = Directory.GetDirectories(folder).ToList();
			while (currentDirectories.Count > 0)
			{
				Count_Folder(currentDirectories[0]);
				currentDirectories.RemoveAt(0);
			}
		}

		private static void Load_All_Content()
		{
			var directories = Directory.GetDirectories($"{main_dir}\\Content").ToList();
			for (int i = 0; i < directories.Count; i++)
			{
				Load_Folder(directories[i]);
			}
			Load_Folder($"{main_dir}\\Content");
			if (loaded_files >= content_file_count)
			{
				loading = false;
			}
		}
		private static void Load_Folder(string folder)
		{
			if (Directory.Exists(folder) == false)
			{
				return;
			}

			var result = "";
			var path = folder.Split('\\');
			var adding = false;
			for (int i = 0; i < path.Length; i++)
			{
				if (adding)
				{
					result = result.Insert(result.Length, path[i]);
					if (i != path.Length - 1)
					{
						result = result.Insert(result.Length, "\\");
					}
				}
				if (path[i] == "Content")
				{
					adding = true;
				}
			}
			var files = Directory.GetFiles(folder);
			for (int i = 0; i < files.Length; i++)
			{
				var split = files[i].Split('\\');
				try
				{
					var loaded = Load_File($"{result}\\{split[split.Length - 1]}");
					if (loaded == false)
					{
						continue;
					}
				}
				catch (Exception)
				{
					continue;
				}
				if (loaded_files % loading_screen_update_per_files == 0)
				{
					goto end;
				}
			}
			var currentDirectories = Directory.GetDirectories(folder).ToList();
			while (currentDirectories.Count > 0)
			{
				Load_Folder(currentDirectories[0]);
				currentDirectories.RemoveAt(0);
			}
		end:;
		}
		private static bool Load_File(string name)
		{
			var split = name.Split('.');
			var key = split[0];
			key = key.Replace('\\', '/');
			if (key[0] == '/')
			{
				key = key.Remove(0, 1);
			}
			if (sprites.ContainsKey(key) || fonts.ContainsKey(key) || sounds.ContainsKey(key) || melodies.ContainsKey(key))
			{
				return false;
			}
			switch (split[1])
			{
				case "png":
					{
						var sprite = game.Content.Load<Texture2D>(key);
						sprites[key] = sprite;

						/*
						var filled = new Texture2D(graphics.GraphicsDevice, sprites[key].Width, sprites[key].Height);
						var outline = new Texture2D(graphics.GraphicsDevice, sprites[key].Width, sprites[key].Height);
						var outlineData = new Color[sprite.Width * sprite.Height];
						var filledData = new Color[sprite.Width * sprite.Height];
						var pixels = new Color[sprite.Width * sprite.Height];

						sprite.GetData(pixels, 0, sprite.Width * sprite.Height);

						for (int i = 0; i < sprite.Height * sprite.Width; i++)
						{
							if (TransparentPixelHasNeighbourOpaquePixel(pixels, i, sprite.Width, sprite.Height))
							{
								outlineData[i] = Color.White;
							}
							if (pixels[i] != Color.Transparent)
							{
								filledData[i] = Color.White;
							}
						}
						outline.SetData(outlineData);
						filled.SetData(filledData);

						spriteOutlines[key] = outline;
						spriteFills[key] = filled;
						*/
						break;
					}
				case "spritefont": fonts[key] = game.Content.Load<SpriteFont>(key); break;
				case "wav":
					{
						sounds_raw[key] = game.Content.Load<SoundEffect>(key);
						sounds[key] = sounds_raw[key].CreateInstance();
						break;
					}
				case "mp3": melodies[key] = game.Content.Load<Song>(key); break;
				default: return false;
			}
			loaded_files++;
			loading_percent = (int)((float)loaded_files / content_file_count * 100);
			loading_percent = loading_percent > 100 ? 100 : loading_percent;
			return true;
			/*
			bool TransparentPixelHasNeighbourOpaquePixel(Color[] pixels, int i, int width, int height)
			{
				var y = i / height;
				var x = i % width;
				var xLeft = i - 1;
				var xRight = i + 1;
				var yUp = i - height;
				var yDown = i + height;

				if (pixels[i] == Color.Transparent)
				{
					if ((IndexIsOutOfBounds(x + 1, y - 1) == false && (pixels[xRight] != Color.Transparent || pixels[yUp] != Color.Transparent)) ||
						(IndexIsOutOfBounds(x - 1, y + 1) == false && (pixels[xLeft] != Color.Transparent || pixels[yDown] != Color.Transparent)) ||
						(IndexIsOutOfBounds(x + 1, y + 1) == false && (pixels[xRight] != Color.Transparent || pixels[yDown] != Color.Transparent)) ||
						(IndexIsOutOfBounds(x - 1, y - 1) == false && (pixels[xLeft] != Color.Transparent || pixels[yUp] != Color.Transparent)))
					{
						return true;
					}
				}
				return false;

				bool IndexIsOutOfBounds(int k, int l)
				{
					if (k < 0 || k > width - 1 || l < 0 || l > height - 1)
					{
						return true;
					}
					return false;
				}
			}
			*/
		}
	}

	public static class Canvas
	{
		public enum Pixel_Filter
		{
			Lowest,
			Medium,
			Highest
		}
		/// <summary>
		/// - Smooths out the edges of the pixels according to the <paramref name="pixel_filter"/>. Higher filters apply better image quality but cost more performance.<br></br><br></br>- Pixel art projects go best with <see cref="Pixel_Filter.Lowest"/>.<br></br>- High resolution projects go best with the rest. <br></br><br></br>
		/// - The current filter can be checked with <see cref="Canvas_Pixel_Filter_Get"/>.
		/// </summary>
		/// <param name="pixel_filter"></param>
		public static void Pixel_Filter_Set(Pixel_Filter pixel_filter)
		{
			render_pixel_filter = pixel_filter;
			switch (render_pixel_filter)
			{
				case Pixel_Filter.Lowest: render_sampler_state = SamplerState.PointWrap; break;
				case Pixel_Filter.Medium: render_sampler_state = SamplerState.LinearWrap; break;
				case Pixel_Filter.Highest: render_sampler_state = SamplerState.AnisotropicWrap; break;
			}
		}
		/// <summary>
		/// - Gets the current pixel filter and returns it.<br></br><br></br>
		/// - Pixel filters can be changed and researched through <see cref="Canvas_Pixel_Filter_Get"/>.
		/// </summary>
		public static Pixel_Filter Canvas_Pixel_Filter_Get() => render_pixel_filter;
		/// <summary>
		/// - Sets the size of the displayed pixel relative to the user's monitor resolution. Each displayed pixel is equal to <paramref name="width"/> and <paramref name="height"/> of screen pixels.<br></br><br></br> - The canvas size can be checked with <see cref="Size_Width_Get"/> and <see cref="Size_Height_Get"/>.<br></br> - The user's screen size can be checked with <see cref="User.Screen_Size_Width_Get"/> and <see cref="User.Screen_Size_Width_Get"/>.
		/// </summary>
		public static void Pixel_Size_Set(int width, int height)
		{
			width = (int)Number.Limited_Get(width, 1, screen_size.X);
			height = (int)Number.Limited_Get(height, 1, screen_size.Y);
			pixel_width = width;
			pixel_height = height;
			canvas_size = screen_size / new Microsoft.Xna.Framework.Point(width, height);
			var gd = game.GraphicsDevice;
			render_target = new RenderTarget2D(gd, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, false, gd.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
			graphics.ApplyChanges();
		}
		/// <summary>
		/// - Gets the current pixel width according to the user's monitor resolution and returns it.<br></br><br></br>
		/// - The height of the current pixel can be received from <see cref="Pixel_Size_Height_Get"/>.<br></br><br></br>
		/// - Pixel size can be changed through <see cref="Pixel_Size_Set"/>.
		/// </summary>
		public static int Pixel_Size_Width_Get() => pixel_width;
		/// <summary>
		/// - Gets the current pixel height according to the user's monitor resolution and returns it.<br></br><br></br>
		/// - The width of the current pixel can be received from <see cref="Pixel_Size_Width_Get"/>.<br></br><br></br>
		/// - Pixel size can be changed through <see cref="Pixel_Size_Set"/>.
		/// </summary>
		public static int Pixel_Size_Height_Get() => pixel_height;
		/// <summary>
		/// - Gets the current width of the canvas in pixels. <br></br><br></br>
		/// - This value can be changed through <see cref="Pixel_Size_Set"/>.<br></br><br></br>
		/// - The height of the canvas size can be checked with <see cref="Size_Height_Get"/>.
		/// </summary>
		public static int Size_Width_Get() => canvas_size.X;
		/// <summary>
		/// - Gets the current height of the canvas in pixels. <br></br><br></br>
		/// - This value can be changed through <see cref="Pixel_Size_Set"/>.<br></br><br></br>
		/// - The width of the canvas size can be checked with <see cref="Size_Width_Get"/>.
		/// </summary>
		public static int Size_Height_Get() => canvas_size.Y;
		/// <summary>
		/// - Sets the background color's hues to <paramref name="red"/>, <paramref name="green"/>, <paramref name="blue"/>.<br></br><br></br>
		/// - Those values must be between 0 and 255 inclusively.<br></br><br></br>
		/// - Those hues can be checked through<br></br>
		/// <see cref="Background_Color_Red_Get"/><br></br>
		/// <see cref="Background_Color_Green_Get"/><br></br>
		/// <see cref="Background_Color_Blue_Get"/><br></br>
		/// </summary>
		public static void Background_Color_Set(byte red, byte green, byte blue) => background_color = new Color(red, green, blue);
		/// <summary>
		/// - Gets the red hue in the background color and returns it.<br></br><br></br>
		/// - The background color can be changed through <see cref="Background_Color_Set"/>.<br></br><br></br>
		/// - The other two hues can be checked with <br></br>
		/// <see cref="Background_Color_Green_Get"/><br></br>
		/// <see cref="Background_Color_Blue_Get"/>
		/// </summary>
		public static byte Background_Color_Red_Get() => background_color.R;
		/// <summary>
		/// - Gets the green hue in the background color and returns it.<br></br><br></br>
		/// - The background color can be changed through <see cref="Background_Color_Set"/>.<br></br><br></br>
		/// - The other two hues can be checked with <br></br>
		/// <see cref="Background_Color_Red_Get"/><br></br>
		/// <see cref="Background_Color_Blue_Get"/>
		/// </summary>
		public static byte Background_Color_Green_Get() => background_color.G;
		/// <summary>
		/// - Gets the blue hue in the background color and returns it.<br></br><br></br>
		/// - The background color can be changed through <see cref="Background_Color_Set"/>.<br></br><br></br>
		/// - The other two hues can be checked with <br></br>
		/// <see cref="Background_Color_Red_Get"/><br></br>
		/// <see cref="Background_Color_Green_Get"/>
		/// </summary>
		public static byte Background_Color_Blue_Get() => background_color.B;
	}
	public static class Window
	{
		/// <summary>
		/// - Checks wether the window is currently focused by the user and returns the result.
		/// </summary>
		public static bool Focused_Check() => game.IsActive;
		/// <summary>
		/// - Pause is <paramref name="activated"/> when the user has the window unfocused or minimized.<br></br><br></br>
		/// - A check wether this pause is activated can be done through <see cref="Window_Unfocused_Pause_Is_Activated_Check"/>.<br></br><br></br>
		/// - A check wether the user has focused the window can be done through <see cref="Window_Is_Focused_Check"/>.
		/// </summary>
		public static void Unfocused_Pause_Activate(bool activated) => pause_unfocus = activated;
		/// <summary>
		/// - Checks if the window pause when unfocusing or minimizing the window is activated and returns the result.<br></br><br></br>
		/// - The window pause can be activated or deactivated through <see cref="Window_Unfocused_Pause_Activate"/>.<br></br><br></br>
		/// - A check wether the user has focused the window can be done through <see cref="Window_Is_Focused_Check"/>.
		/// </summary>
		public static bool Unfocused_Pause_Is_Activated_Check() => pause_unfocus;

		public static void Show(bool show)
		{
			var form = Control.FromHandle(game.Window.Handle) as Form;
			if (show) form.Show();
			else form.Hide();
		}

		/// <summary>
		/// - Sets the <paramref name="title"/> of the window.<br></br><br></br>
		/// - The title can be received with <see cref="Window_Title_Get"/>.
		/// </summary>
		public static void Title_Set(string title) => game.Window.Title = title;
		/// <summary>
		/// - Gets the title of the window and returns it.<br></br><br></br>
		/// - The title can be changed through <see cref="Window_Title_Set"/>.
		/// </summary>
		public static string Title_Get() => game.Window.Title;

		/// <summary>
		/// - Sets the Alt+F4 functionality to <paramref name="activated"/>. When <paramref name="activated"/> pressing Alt+F4 closes the window.<br></br><br></br>
		/// - A check wether the Alt+F4 functionality is activated can be done through <see cref="Window_Close_Hotkeys_Is_Activated_Check"/>.<br></br><br></br>
		/// - The window can be also closed through <see cref="Window_Close"/>.
		/// </summary>
		public static void Close_Hotkeys_Activate(bool activated) => game.Window.AllowAltF4 = activated;
		/// <summary>
		/// - Checks wether the Alt+F4 functionality is activated and returns the result.<br></br><br></br>
		/// - The Alt+F4 functionality can be activated or deactivated with <see cref="Window_Close_Hotkeys_Activate(bool)"/>.
		/// </summary>
		public static bool Close_Hotkeys_Is_Activated_Check() => game.Window.AllowAltF4;

		/// <summary>
		/// - Ends the runtime of the program and closes the window.
		/// </summary>
		public static void Close() => game.Exit();
	}
	/// <summary>
	/// A main object for the program that can contain different data for it to be displayed on the screen and interacted with.
	/// </summary>
	public class Body
	{
		private static int ID;
		private static Dictionary<string, Body> body_unique_names = new Dictionary<string, Body>();

		public enum Pick_Number_Comparison
		{
			Lowest, Less, Equals, Greater, Highest
		}

		public static List<Body> Bodies_All_Get() => new List<Body>(bodies_all);
		public static Body Pick_By_Name_Get(string unique_name)
		{
			if (unique_name == null || body_unique_names.ContainsKey(unique_name) == false)
			{
				return default;
			}
			return body_unique_names[unique_name];
		}
		public static Body Create_Get() => new Body();

		[JsonProperty]
		private Color sprite_color, boundaries_color, origin_color, angle_color;
		[JsonProperty]
		private Microsoft.Xna.Framework.Point sprite_index, sprite_size;
		[JsonProperty]
		private Vector2 position, sprite_origin, size;
		[JsonProperty]
		private int uid, sprite_grid_size;
		[JsonProperty]
		private float angle;
		[JsonProperty]
		private string unique_name, sprite_name;
		[JsonProperty]
		private bool boundaries_shown, origin_shown, angle_shown, sprite_shown;

		public Body()
		{
			if (unique_name == null) unique_name = $"{ID}";
			bodies_all.Add(this);
			body_unique_names.Add(unique_name, this);
			uid = ID;
			ID++;
		}

		public override string ToString() => $"[{uid}] {unique_name}";

		public int Unique_ID_Get() => uid;
		public string Unique_Name_Get() => unique_name;
		public void Unique_Name_Set(string name)
		{
			if (name == unique_name)
			{
				return;
			}
			if (name == null || body_unique_names.ContainsKey(name))
			{
				throw new ArgumentException($"Another Body with unique name '{name}' already exists. Make sure you are not creating multiple times or each tick.");
			}
			unique_name = name;
			body_unique_names.Add(unique_name, this);
		}

		public float Position_X_Get() => position.X;
		public float Position_Y_Get() => position.Y;
		public void Position_Set(float x, float y)
		{
			position.X = x;
			position.Y = y;
			render = true;
		}

		public void Angle_Set(float angle)
		{
			this.angle = angle;
			render = true;
		}
		public float Angle_Get() => angle;

		public void Size_Set(float width, float height)
		{
			size.X = width;
			size.Y = height;
			render = true;
		}
		public float Size_Width_Get() => size.X;
		public float Size_Height_Get() => size.Y;

		#region Display Angle
		public void Angle_Show(bool show = true, byte color_red = 255, byte color_green = 255, byte color_blue = 255, byte opacity = 255)
		{
			angle_shown = show;
			angle_color = new Color(color_red, color_green, color_blue, opacity);
			render = true;
		}
		public bool Angle_Is_Shown_Check() => angle_shown;
		public int Angle_Red_Get() => angle_color.R;
		public int Angle_Green_Get() => angle_color.G;
		public int Angle_Blue_Get() => angle_color.B;
		public int Angle_Opacity_Get() => angle_color.A;
		#endregion
		#region Display Origin
		public void Origin_Show(bool show = true, byte color_red = 255, byte color_green = 255, byte color_blue = 255, byte opacity = 255)
		{
			origin_shown = show;
			origin_color = new Color(color_red, color_green, color_blue, opacity);
			render = true;
		}
		public bool Origin_Is_Shown_Check() => origin_shown;
		public int Origin_Red_Get() => origin_color.R;
		public int Origin_Green_Get() => origin_color.G;
		public int Origin_Blue_Get() => origin_color.B;
		public int Origin_Opacity_Get() => origin_color.A;
		#endregion
		#region Display Boundaries
		public void Boundaries_Show(bool show = true, byte color_red = 255, byte color_green = 255, byte color_blue = 255, byte opacity = 255)
		{
			boundaries_shown = show;
			boundaries_color = new Color(color_red, color_green, color_blue, opacity);
			render = true;
		}
		public bool Boundaries_Are_Shown_Check() => boundaries_shown;
		public int Boundaries_Red_Get() => boundaries_color.R;
		public int Boundaries_Green_Get() => boundaries_color.G;
		public int Boundaries_Blue_Get() => boundaries_color.B;
		public int Boundaries_Opacity_Get() => boundaries_color.A;
		#endregion
		#region Display Sprite
		public void Sprite_Set(string name, bool show = true, int width = 64, int height = 64, byte red = 255, byte green = 255, byte blue = 255, byte opacity = 255, int origin_x = 0, int origin_y = 0, int grid_size = 0, int index_h = 0, int index_v = 0)
		{
			if (sprites.ContainsKey(name) == false)
			{
				throw new ArgumentException($"No sprite with name '{name}' was found. In order to load a sprite:\n1. Add it to the 'Content' folder.\n2. Add it to the Solution Explorer's 'Content' folder.\n3. In its properties select Copy to Output Directory: 'Copy Always'.\n4. Open 'Content.mgcb' with the MonoGame Content Pipeline Tool.\n5. Add it to the Content and build/rebuild it.");
			}
			sprite_name = name;
			size = new Vector2(sprites[name].Width, sprites[name].Height);
			sprite_size = new Microsoft.Xna.Framework.Point(width, height);
			sprite_color = new Color(red, green, blue, opacity);
			sprite_origin = new Vector2(origin_x, origin_y);
			sprite_grid_size = grid_size;
			sprite_index = new Microsoft.Xna.Framework.Point(index_h, index_v);
			sprite_shown = show;
			render = true;
		}
		public string Sprite_Name_Get() => sprite_name;

		public bool Sprite_Is_Shown_Check() => sprite_shown;

		public int Sprite_Grid_Size_Get() => sprite_grid_size;

		public int Sprite_Index_Horizontal_Get() => sprite_index.X;
		public int Sprite_Index_Vertical_Get() => sprite_index.Y;

		public float Sprite_Origin_X_Get() => sprite_origin.X;
		public float Sprite_Origin_Y_Get() => sprite_origin.Y;

		public float Sprite_Width_Get() => sprite_size.X;
		public float Sprite_Height_Get() => sprite_size.Y;

		public int Sprite_Red_Get() => sprite_color.R;
		public int Sprite_Green_Get() => sprite_color.G;
		public int Sprite_Blue_Get() => sprite_color.B;
		public int Sprite_Opacity_Get() => sprite_color.A;
		#endregion
	}
	/// <summary>
	/// Controls <see cref="float"/> in different ways.
	/// </summary>
	public static class Number
	{
		public enum Round_Type
		{
			Closest, Up, Down
		}
		public enum Time_Convert_Type
		{
			Milliseconds_To_Seconds,
			Seconds_To_Milliseconds, Seconds_To_Minutes, Seconds_To_Hours,
			Minutes_To_Milliseconds, Minutes_To_Seconds, Minutes_To_Hours, Minutes_To_Days,
			Hours_To_Seconds, Hours_To_Minutes, Hours_To_Days, Hours_To_Weeks,
			Days_To_Minutes, Days_To_Hours, Days_To_Weeks,
			Weeks_To_Hours, Weeks_To_Days
		}
		public static float Unsigned_Get(float number)
		{
			return Math.Abs(number);
		}
		public static float Averaged_Get(float number_a, float number_b)
		{
			return (number_a + number_b) / 2;
		}
		public static float Randomized_Get(float lower_bound, float upper_bound, int precision)
		{
			precision = (int)Limited_Get(precision, 0, 5);
			if (lower_bound > upper_bound)
			{
				var swap = lower_bound;
				lower_bound = upper_bound;
				upper_bound = swap;
			}
			var lowerInt = Convert.ToInt32(lower_bound * Math.Pow(10, Precision_Get(lower_bound)));
			var upperInt = Convert.ToInt32(upper_bound * Math.Pow(10, Precision_Get(upper_bound)));
			var randInt = new Random(Guid.NewGuid().GetHashCode()).Next(lowerInt, upperInt + 1);

			return randInt / (float)Math.Pow(10, precision);
		}
		public static float Rounded_Get(float number, int precision, Round_Type number_round_type)
		{
			precision = (int)Limited_Get(precision, 0, 5);
			var a = (float)Math.Pow(10, Precision_Get(number));
			var b = (float)Math.Pow(10, precision);
			var c = number * a;
			switch (number_round_type)
			{
				case Round_Type.Closest:
					return Convert.ToInt32(c) / b;
				case Round_Type.Up:
					return (float)Math.Ceiling(c) / b;
				default:
					return (float)Math.Floor(c) / b;
			}
		}
		public static float Limited_Get(float number, float minimum, float maximum)
		{
			if (minimum > maximum)
			{
				var swap = minimum;
				minimum = maximum;
				maximum = swap;
			}
			if (number < minimum)
			{
				return minimum;
			}
			else if (number > maximum)
			{
				return maximum;
			}
			return number;
		}
		public static float Percented_Towards_Target_Get(float number, float target_number, float percent)
		{
			var vec = new Vector2(number, 0);
			var targetVec = new Vector2(target_number, 0);
			var result = Vector2.Lerp(vec, targetVec, percent / 100);

			return result.X;
		}
		public static float Changed_Get(float number, float numbers_per_second)
		{
			return number + (numbers_per_second * ticks_delta_time);
		}
		public static float Towards_Target_Get(float number, float target_number, float numbers_per_second)
		{
			if (number <= target_number && target_number * ticks_delta_time < 0)
			{
				return target_number;
			}
			else if (number >= target_number && target_number * ticks_delta_time > 0)
			{
				return target_number;
			}
			return Changed_Get(number, numbers_per_second);
		}
		public static float Time_Converted_Get(float time, Time_Convert_Type time_convert_type)
		{
			switch (time_convert_type)
			{
				case Time_Convert_Type.Milliseconds_To_Seconds: return time / 1_000;
				case Time_Convert_Type.Seconds_To_Milliseconds: return time * 1_000;
				case Time_Convert_Type.Seconds_To_Minutes: return time / 60;
				case Time_Convert_Type.Seconds_To_Hours: return time / 3_600;
				case Time_Convert_Type.Minutes_To_Milliseconds: return time * 60_000;
				case Time_Convert_Type.Minutes_To_Seconds: return time * 60;
				case Time_Convert_Type.Minutes_To_Hours: return time / 60;
				case Time_Convert_Type.Minutes_To_Days: return time / 1_440;
				case Time_Convert_Type.Hours_To_Seconds: return time * 3_600;
				case Time_Convert_Type.Hours_To_Minutes: return time * 60;
				case Time_Convert_Type.Hours_To_Days: return time / 24;
				case Time_Convert_Type.Hours_To_Weeks: return time / 168;
				case Time_Convert_Type.Days_To_Minutes: return time * 1_440;
				case Time_Convert_Type.Days_To_Hours: return time * 24;
				case Time_Convert_Type.Days_To_Weeks: return time / 7;
				case Time_Convert_Type.Weeks_To_Hours: return time * 168;
				case Time_Convert_Type.Weeks_To_Days: return time * 7;
			}
			return 0;
		}
		public static bool Chance_Check(float percent)
		{
			percent = Limited_Get(percent, 0, 100);
			var n = Randomized_Get(1, 100, 0);
			return n <= percent;
		}
		public static int Precision_Get(float number)
		{
			var result = 0;
			var number_str = number.ToString();
			if (number_str.Contains('.')) result = number.ToString().Split('.')[1].Length;
			return result;
		}
	}
	/// <summary>
	/// Controls <see cref="string"/> in different ways.
	/// </summary>
	public static class Text
	{
		/// <summary>
		/// Converts a <paramref name="list"/> into a <see cref="string"/>. The elements are separated by a <paramref name="separator"/>. Then the <see cref="string"/> is returned.
		/// </summary>
		public static string From_List_Get<T>(List<T> list, string separator = ", ")
		{
			var result = "";
			if (list == null || list.Count == 0)
			{
				return result;
			}
			for (int i = 0; i < list.Count; i++)
			{
				result = result.Insert(result.Length, $"{list[i]}");
				if (i == list.Count - 1)
				{
					break;
				}
				result = result.Insert(result.Length, $"{separator}");
			}
			return result;
		}
		/// <summary>
		/// Converts an <paramref name="array"/> into a <see cref="string"/>. The elements are separated by a <paramref name="separator"/>. Then the <see cref="string"/> is returned.
		/// </summary>
		public static string From_Array_Get<T>(T[] array, string separator = ", ")
		{
			return From_List_Get(array.ToList(), separator);
		}

		/// <summary>
		/// Adds <paramref name="text"/> to the clipboard (copies it). It can be accessed later via <typeparamref name="Ctrl"/> + <typeparamref name="V"/> or <see cref="Clipboard_Get"/>.
		/// </summary>
		public static void Clipboard_Copy(string text)
		{
			Clipboard.SetText(text);
		}
		/// <summary>
		/// Gets the clipboard data and returns it if it is a <see cref="string"/>, otherwise returns <paramref name="null"/>.‪‪
		/// </summary>
		public static string Clipboard_Get()
		{
			var result = Clipboard.GetText();
			return result == string.Empty ? null : result;
		}

		/// <summary>
		/// Returns a new <see cref="string"/> after a simple encryption on <paramref name="text"/> with a <paramref name="key"/> that can be <paramref name="performed_twice"/>.‪‪ The encryption can be decrypred later and the text can be retrieved back with <see cref="Decrypted_Get"/>.
		/// </summary>
		public static string Encrypted_Get(string text, char key, bool performed_twice = false)
		{
			var result = text;
			var times = performed_twice ? 2 : 1;
			for (int i = 0; i < times; i++)
			{
				result = Encrypt(result, key);
			}
			return result;
		}
		private static string Encrypt(string text, char key)
		{
			var amplifier = Convert.ToByte(key);
			byte[] data = Encoding.UTF8.GetBytes(text);
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(data[i] ^ amplifier);
			}
			return Convert.ToBase64String(data);
		}
		/// <summary>
		/// Returns the decrypted version of an encrypted <paramref name="text"/> with a <paramref name="key"/> that could have been <paramref name="performed_twice"/> with <see cref="Encrypted_Get"/>.‪‪
		/// </summary>
		public static string Decrypted_Get(string encrypted_text, char key, bool performed_twice = false)
		{
			var result = encrypted_text;
			var times = performed_twice ? 2 : 1;
			for (int i = 0; i < times; i++)
			{
				result = Decrypt(result, key);
			}
			return result;
		}
		private static string Decrypt(string encrypted_text, char key)
		{
			var amplifier = Convert.ToByte(key);
			byte[] data = Convert.FromBase64String(encrypted_text);
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(data[i] ^ amplifier);
			}
			return Encoding.UTF8.GetString(data);
		}

		/// <summary>
		/// Converts <typeparamref name="T"/> <paramref name="data"/> into a <see cref="string"/> (<paramref name="JSON"/>) and returns it. It can be converted and retrieved back to <typeparamref name="T"/> <paramref name="data"/> later with <see cref="To_Data_Convert"/>.‪‪
		/// </summary>
		public static string From_Data_Convert<T>(T data)
		{
			return JsonConvert.SerializeObject(data);
		}
		/// <summary>
		/// Converts an already formatted <paramref name="text"/> (<paramref name="JSON"/>) into <typeparamref name="T"/> <paramref name="data"/> and returns it if the <paramref name="text"/> is in the correct format. Otherwise returns <paramref name="default"/>(<typeparamref name="T"/>).
		/// </summary>
		public static T To_Data_Convert<T>(string text)
		{
			try
			{
				return JsonConvert.DeserializeObject<T>(text);
			}
			catch (Exception)
			{
				return default(T);
			}
		}

		/// <summary>
		/// Creates or overwrites a file on <paramref name="file_path"/> with <paramref name="file_name"/> and <paramref name="file_extension"/> then fills it with <paramref name="text"/>. This <paramref name="text"/> can be accessed later with <see cref="Load"/>.<br></br><br></br>
		/// This is a slow operation - do not call frequently.
		/// </summary>
		public static void Save(string text, string file_path = "", string file_name = "data", string file_extension = "data")
		{
			try
			{
				File.WriteAllText($"{main_dir}\\{file_path}\\{file_name}.{file_extension}", text);
			}
			catch (Exception)
			{
				return;
			}
		}
		/// <summary>
		/// Reads the text from the file at <paramref name="file_path"/> with <paramref name="file_name"/> and <paramref name="file_extension"/> and returns it as a <see cref="string"/> if successful. Returns <paramref name="null"/> otherwise. A text can be saved to a file with <see cref="Save"/>.<br></br><br></br>
		/// This is a slow operation - do not call frequently.
		/// </summary>
		public static string Load(string file_path = "", string file_name = "data", string file_extension = "data")
		{
			try
			{
				return File.ReadAllText($"{main_dir}\\{file_path}\\{file_name}.{file_extension}");
			}
			catch (Exception)
			{
				return default;
			}
		}

		/// <summary>
		/// - Displays a <paramref name="message"/> on the screen with a <paramref name="font"/> that has a <paramref name="scale"/>. The <paramref name="message"/> may <paramref name="overwrite"/> what is already displayed instead of appending it.<br></br><br></br>
		/// - The displayed text can be cleared with <see cref="Display_Clear"/>.
		/// </summary>
		public static void Display(string font, object message, float scale = 1, bool overwrite = false)
		{
			if (fonts.ContainsKey(font) == false)
			{
				return;
			}
			text_display_draw = true;
			text_display_font = font;
			if (overwrite) text_display_message = "";
			text_display_message = $"{text_display_message}{message}";
			scale = Number.Limited_Get(scale, 0.001f, 5000);
			text_display_scale = scale;

			var sample_size = fonts[text_display_font].MeasureString("a");
			var sample_size_scaled = sample_size * text_display_scale;
			var visible_lines = (int)(canvas_size.Y / sample_size_scaled.Y);
			var size = fonts[text_display_font].MeasureString(text_display_message) * text_display_scale;
			var lines = text_display_message.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			if (size.Y > canvas_size.Y + sample_size_scaled.Y && lines.Count > 2 && visible_lines < lines.Count)
			{
				text_display_message = "";
				lines[lines.Count - visible_lines] = "...";
				for (int i = lines.Count - visible_lines; i < lines.Count; i++)
				{
					text_display_message = $"{text_display_message}{lines[i]}\n";
				}
			}
			render = true;
		}
		/// <summary>
		/// - Clears all the text on screen that was displayed through <see cref="Display"/>.
		/// </summary>
		public static void Display_Clear()
		{
			text_display_message = null;
			render = true;
		}

		public static string Time_Formatted_Get(float seconds, string separator = ":", bool ms_show = false, string ms_format = "ms", bool sec_show = true, string sec_format = "s", bool min_show = true, string min_format = "m", bool hr_show = true, string hr_format = "h")
		{
			seconds = Number.Unsigned_Get(seconds);
			var seconds_str = seconds.ToString();
			var ms = 0;
			if (seconds_str.Contains('.'))
			{
				var spl = seconds_str.Split('.');
				ms = int.Parse(spl[1]) * 100;
				seconds = Number.Rounded_Get(seconds, 0, Number.Round_Type.Down);
			}
			var sec = seconds % 60;
			var min = Number.Rounded_Get(seconds / 60 % 60, 0, Number.Round_Type.Down);
			var hr = Number.Rounded_Get(seconds / 3_600, 0, Number.Round_Type.Down);
			var ms_str = ms_show ? $"{ms}" : "";
			var sec_str = sec_show ? $"{sec}" : "";
			var min_str = min_show ? $"{min}" : "";
			var hr_str = hr_show ? $"{hr}" : "";
			var ms_f = ms_show ? $"{ms_format}" : "";
			var sec_f = sec_show ? $"{sec_format}" : "";
			var min_f = min_show ? $"{min_format}" : "";
			var hr_f = hr_show ? $"{hr_format}" : "";
			var sec_ms_sep = ms_show && (sec_show || min_show || hr_show) ? $"{separator}" : "";
			var min_sec_sep = sec_show && (min_show || hr_show) ? $"{separator}" : "";
			var hr_min_sep = min_show && hr_show ? $"{separator}" : "";

			return $"{hr_str}{hr_f}{hr_min_sep}{min_str}{min_f}{min_sec_sep}{sec_str}{sec_f}{sec_ms_sep}{ms_str}{ms_f}";
		}
	}
	public static class Performance
	{
		/// <summary>
		/// - Sets the target <paramref name="tps"/> that can be between 2 and 1000 inclusively if <paramref name="limited"/>. The ticks per second may go bellow but not above the targeted speed (depending on performance), otherwise multiple ticks (but not frames) at the same time will occur in order to keep up. <br></br>- The tick rate is also capped to the user's monitor refresh rate if <paramref name="v_synced"/> (vertical synchronization removes scanlines and tearing artifacts). <br></br>- Not <paramref name="limited"/> and not <paramref name="v_synced"/> tick rate uncaps both the frame rate and tick rate, therefore running as fast as possible. <br></br><br></br>- The current tick rate can be checked with <see cref="Ticks_Per_Second_Get"/>.<br></br>- The current target tick rate can be checked with <see cref="Ticks_Per_Second_Target_Get"/>.<br></br>- A check wether the tick rate is <paramref name="limited"/> can be received from <see cref="Ticks_Per_Second_Are_Limited_Check"/>.<br></br>- And check wether they are vertically synchronized from <see cref="Ticks_Per_Second_Are_V_Synced_Check"/>.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. The current frame rate can be checked with <see cref="Frames_Total_Per_Second_Get"/>.<br></br>
		/// </summary>
		public static void Ticks_Per_Second_Target_Set(float tps, bool limited, bool v_synced)
		{
			tps = tps < 2 ? 2 : tps;
			tps = tps > 1000 ? 1000 : tps;
			game.TargetElapsedTime = TimeSpan.FromSeconds(1d / tps);
			game.IsFixedTimeStep = limited;
			graphics.SynchronizeWithVerticalRetrace = v_synced;
			graphics.ApplyChanges();
		}
		/// <summary>
		/// - Gets the current tick rate that can be an <paramref name="average"/> of the previous 60 ticks and returns it. <br></br><br></br>- The target tick speed can be changed via <see cref="Ticks_Per_Second_Target_Set"/>.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. The current frames per second can be checked with <see cref="Frames_Total_Per_Second_Get"/>.
		/// </summary>
		public static float Ticks_Per_Second_Get(bool average = false)
		{
			return average ? tps_average : tps;
		}
		/// <summary>
		/// - Gets the current target tick rate and returns it. <br></br><br></br>
		/// - The target tick speed can be changed via <see cref="Ticks_Per_Second_Target_Set"/>. Also contains information about ticks/frames.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. The current frames per second can be checked with <see cref="Frames_Total_Per_Second_Get"/>.<br></br><br></br>
		/// </summary>
		public static float Ticks_Per_Second_Target_Get() => 60 / ((float)game.TargetElapsedTime.TotalSeconds * 60);
		/// <summary>
		/// - Checks wether the tick rate is limited to the target tick rate and returns the result.<br></br><br></br>- The limitation of the tick speed and other related changes can be set through<br></br> <see cref="Ticks_Per_Second_Target_Set"/>. Also contains information about ticks/frames.
		/// </summary>
		public static bool Ticks_Per_Second_Are_Limited_Check() => game.IsFixedTimeStep;
		/// <summary>
		/// - Checks wether the tick rate is limited by the user's monitor refresh rate and returns the result.<br></br><br></br>
		/// - The vertical synchronization and other related changes can set through<br></br> <see cref="Ticks_Per_Second_Target_Set"/>. Also contains information about ticks/frames.<br></br><br></br>
		/// </summary>
		public static bool Ticks_Per_Second_Are_V_Synced_Check() => graphics.SynchronizeWithVerticalRetrace;
		/// <summary>
		/// - Gets the number of ticks that have passed since the start and returns them.<br></br><br></br>
		/// - The tick count is also provided as an <see cref="int"/> parameter with <see cref="Program.Each_Tick(int)"/>.<br></br><br></br>
		/// - Changing the tick speed and receiving information about ticks/frames may be done through <see cref="Ticks_Per_Second_Target_Set"/>.
		/// </summary>
		public static int Ticks_Count_Get() => tick;

		public static float RAM_GB_Available_Get() => ram_available.NextValue() / 1000;
		public static float RAM_Percent_Used_Get() => ram_used_percent.NextValue();

		/// <summary>
		/// - Gets the current frame rate that can be an <paramref name="average"/> of the previous 60 ticks and returns it.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. A lower frame rate will be present with slow tick rate and vice versa. The targeted tick rate can be changed or uncapped with <see cref="Ticks_Per_Second_Target_Set"/>. Also contains information about ticks/frames.
		/// </summary>
		public static float Frames_Total_Per_Second_Get(bool average = false)
		{
			return average ? fps_average : fps;
		}
		/// <summary>
		/// - Gets the number of frames that have passed since the start and returns them. This counter is not affected by rendering. Therefore a frame might be skipped and the counter will still increment. <br></br><br></br>- Rendered frames counter can be checked with <see cref="Frames_Rendered_Count_Get"/>.<br></br><br></br>
		/// - Changing or uncapping the tick rate and receiving information about ticks/frames can be done through <see cref="Ticks_Per_Second_Target_Set"/>. This affects the frame rate.
		/// </summary>
		public static int Frames_Total_Count_Get() => frame;
		/// <summary>
		/// - Gets the number of rendered frames that have passed since the start and returns them. Rendered frames happen only when the current frame is different than the last frame. Therefore frames are skipped when the screen is static.<br></br><br></br>
		/// - A check for the total frames counter can be done through <see cref="Frames_Total_Count_Get"/>.<br></br><br></br>
		/// - Changing or uncapping the tick rate and receiving information about ticks/frames can be done through <see cref="Ticks_Per_Second_Target_Set"/>. This affects the frame rate.
		/// </summary>
		public static int Frames_Rendered_Count_Get() => frame_rendered;

		/// <summary>
		/// - Gets the time that has passed since the start and returns it.
		/// </summary>
		public static float Time_Get() => time;
		/// <summary>
		/// - Gets the time that has passed since the last tick and returns it. <br></br><br></br>- The target tick rate can be changed via <see cref="Ticks_Per_Second_Target_Set"/><br></br>- The current target tick rate can be checked with <see cref="Time_Since_Last_Tick_Target_Get"/>.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. The time since last frame can be checked with <see cref="Time_Since_Last_Frame_Get"/>.
		/// </summary>
		public static float Time_Since_Last_Tick_Get() => ticks_delta_time;
		/// <summary>
		/// - Gets the target time between ticks and returns it.<br></br><br></br>- The target tick rate can be changed via <see cref="Ticks_Per_Second_Target_Set"/>.<br></br>- The current tick rate can be checked with <see cref="Time_Since_Last_Tick_Get"/>.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. The time since last frame can be checked with <see cref="Time_Since_Last_Frame_Get"/>.
		/// </summary>
		public static float Time_Since_Last_Tick_Target_Get() => (float)game.TargetElapsedTime.TotalSeconds;
		/// <summary>
		/// - Gets the time that has passed since the last frame and returns it.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. The time since last tick can be checked with <see cref="Time_Since_Last_Tick_Get"/>.
		/// </summary>
		public static float Time_Since_Last_Frame_Get() => frames_delta_time;
	}
	public static class Hardware
	{
		/// <summary>
		/// - Gets the user's screen width in pixels.<br></br><br></br>
		/// - The user's screen height can be checked with <see cref="Screen_Size_Height_Get"/>.
		/// </summary>
		public static int Screen_Size_Width_Get() => screen_size.X;
		/// <summary>
		/// - Gets the user's screen height in pixels.<br></br><br></br>
		/// - The user's screen width can be checked with <see cref="Screen_Size_Width_Get"/>.
		/// </summary>
		public static int Screen_Size_Height_Get() => screen_size.Y;
		/// <summary>
		/// - When <paramref name="activated"/> the user's computer will stay active at all times, even when left idle.<br></br><br></br>
		/// - A check wether sleep prevention is activated can be done via <see cref="Computer_Sleep_Prevention_Is_Activated_Check"/>.
		/// </summary>
		public static void Computer_Sleep_Prevention_Activate(bool activated)
		{
			sleep_prevented = activated;
			if (sleep_prevented)
			{
				SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED);
			}
			else
			{
				SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
			}
		}
		/// <summary>
		/// - Checks wether the user's computer is prevented from sleeping and returns the result.<br></br><br></br>
		/// - Sleep prevention can be activated or deactivated through <see cref="Computer_Sleep_Prevention_Activate"/>.
		/// </summary>
		public static bool Computer_Sleep_Prevention_Is_Activated_Check() => sleep_prevented;
	}
	public static class Network
	{
		public static void Server_Start()
		{
			try
			{
				if (server_is_running)
				{
					console_log = $"{console_log}\nServer_Start(): Server is already starting/started.";
					_Console_Update();
					return;
				}
				if (client_is_connected)
				{
					console_log = $"{console_log}\nServer_Start(): Cannot start a server while connected to one.";
					_Console_Update();
					return;
				}
				server = new Server(IPAddress.Any, server_port);
				console_log = $"{console_log}\nServer_Start(): Starging a LAN Server on port {server_port}...";
				server.Start();
				console_log = $"{console_log}\nServer_Start(): Done!";

				var host_name = Dns.GetHostName();
				var host_entry = Dns.GetHostEntry(host_name);
				connect_to_server_info = "Clients can connect through those IPs if they are in the same network\n(device / router / Virtual Private Network programs like Hamachi or Radmin): \nSame device: 127.0.0.1";
				foreach (var ip in host_entry.AddressList)
				{
					if (ip.AddressFamily == AddressFamily.InterNetwork)
					{
						var ip_parts = ip.ToString().Split('.');
						var ip_type = ip_parts[0] == "192" && ip_parts[1] == "168" ? "Same router: " : "Same VPN: ";
						connect_to_server_info = $"{connect_to_server_info}\n{ip_type}{ip}";
					}
				}

				server_is_running = true;
				NatUtility.DeviceFound += DeviceFound;
				NatUtility.StartDiscovery();
				_Console_Update();
			}
			catch (Exception ex)
			{
				server_is_running = false;
				console_log = $"{console_log}\nServer_Start() Error: {ex.Message}";
				_Console_Update();
				return;
			}
		}
		public static void Client_Connect(string unique_name, string ip)
		{
			var func_name = $"Client_Connect(\"{unique_name}\", \"{ip}\")";
			if (client_is_connected)
			{
				console_log = $"{console_log}\n{func_name}: Already connecting/connected.";
				_Console_Update();
				return;
			}
			if (server_is_running)
			{
				console_log = $"{console_log}\n{func_name}: Cannot connect as a client while a server.";
				_Console_Update();
				return;
			}
			NatUtility.DeviceFound += DeviceFound;
			NatUtility.StartDiscovery();
			client_is_connected = true;

			try
			{
				client = new Client(ip, server_port);
			}
			catch (Exception)
			{
				client_is_connected = false;
				console_log = $"{console_log}{func_name}: {ip} is an invalid IP.";
				return;
			}
			client_unique_name = unique_name;
			console_log = $"{console_log}\n{func_name}: Connecting to {ip}:{server_port}...";
			_Console_Update();
			client.ConnectAsync();

		}
		public static void Client_Disconnect()
		{
			if (client_is_connected == false)
			{
				console_log = $"{console_log}\nClient_Disconnect(): Cannot disconnect when not connected.";
				_Console_Update();
				return;
			}
			client.DisconnectAndStop();
		}
		public static void Clinet_Message_Send_To_All(string message)
		{
			if (client_is_connected == false && server_is_running == false)
			{
				console_log = $"{console_log}\nClinet_Message_Send_To_All(): Cannot send a message while disconnected.";
				_Console_Update();
				return;
			}
			else if (server_is_running)
			{
				console_log = $"{console_log}\nClinet_Message_Send_To_All(): Cannot send a message while a server. Only clients can send messages.";
				_Console_Update();
				return;
			}
			console_log = $"{console_log}\nSending a message to all clients: {message}";
			_Console_Update();
			client.SendAsync($"{(int)Message_Type.Message_To_All}|{client_unique_name}|{message}");
		}
		public static void Clinet_Message_Send_To_Client(string receiver_unique_name, string message)
		{
			if (client_unique_name == receiver_unique_name)
			{
				return;
			}
			if (client_is_connected == false && server_is_running == false)
			{
				console_log = $"{console_log}\nClinet_Message_Send_To_Client(): Cannot send a message while disconnected.";
				_Console_Update();
				return;
			}
			else if (server_is_running)
			{
				console_log = $"{console_log}\nClinet_Message_Send_To_Client(): Cannot send a message while a server. Only clients can send messages.";
				_Console_Update();
				return;
			}
			console_log = $"{console_log}\nSending a message to client [{receiver_unique_name}]: {message}";
			_Console_Update();
			client.SendAsync($"{(int)Message_Type.Message_To_Client}|{client_unique_name}|{receiver_unique_name}|{message}");
		}

		public static string Console_Read() => System.Console.ReadLine();
		private static void DeviceFound(object sender, DeviceEventArgs args) => args.Device.CreatePortMap(new Mapping(Protocol.Tcp, server_port, server_port));
	}
	public static class Camera
	{
		/// <summary>
		/// - Creates a screenshot in <paramref name="path"/>/<paramref name="name"/>.png that contains what is currently visible in the window and saves it as a sprite. The result can be <paramref name="scaled"/> to the user's screen resolution, otherwise takes the canvas resolution.<br></br><br></br>
		/// - The canvas resolution can be received from <see cref="Canvas.Size_Width_Get"/> and <see cref="Canvas.Size_Height_Get"/>.<br></br>
		/// - The user's screen resolution can be received from <see cref="Hardware.Screen_Size_Width_Get"/> and <see cref="Hardware.Screen_Size_Height_Get"/>.<br></br>
		/// </summary>
		public static void Screenshot(string path, string name, bool scaled)
		{
			var size = new Microsoft.Xna.Framework.Point(
				scaled ? game.GraphicsDevice.PresentationParameters.BackBufferWidth : canvas_size.X,
				scaled ? game.GraphicsDevice.PresentationParameters.BackBufferHeight : canvas_size.Y);
			var buffer = new int[size.X * size.Y];
			var texture = new Texture2D(game.GraphicsDevice, size.X, size.Y);
			var final_path = $"{main_dir}{path}";

			if (Directory.Exists(final_path) == false) Directory.CreateDirectory(final_path);

			if (scaled) game.GraphicsDevice.GetBackBufferData(buffer);
			else render_target.GetData(0, new Rectangle(0, 0, size.X, size.Y), buffer, 0, size.X * size.Y);

			texture.SetData(buffer);
			using (Stream stream = File.Create($"{final_path}\\{name}.png"))
			{
				texture.SaveAsPng(stream, size.X, size.Y);
			}
			sprites[name] = texture;
		}

		public static void Position_Set(float x, float y) => camera_position = new Vector2(x, y);
		public static float Position_X_Get() => camera_position.X;
		public static float Position_Y_Get() => camera_position.Y;
	}
	/// <summary>
	/// - Holds information about the current input of the user.
	/// </summary>
	public static class Input
	{
		public enum Keys
		{
			None = 0, BackSpace = 8, Tab = 9, Enter = 13, Pause = 19, CapsLock = 20, Kana = 21, Kanji = 25, Escape = 27, ImeConvert = 28, ImeNoConvert = 29, Space = 32, PageUp = 33, PageDown = 34, End = 35, Home = 36, Left = 37, Up = 38, Right = 39, Down = 40, Select = 41, Print = 42, Execute = 43, PrintScreen = 44, Insert = 45, Delete = 46, Help = 47, _0 = 48, _1 = 49, _2 = 50, _3 = 51, _4 = 52, _5 = 53, _6 = 54, _7 = 55, _8 = 56, _9 = 57, A = 65, B = 66, C = 67, D = 68, E = 69, F = 70, G = 71, H = 72, I = 73, J = 74, K = 75, L = 76, M = 77, N = 78, O = 79, P = 80, Q = 81, R = 82, S = 83, T = 84, U = 85, V = 86, W = 87, X = 88, Y = 89, Z = 90, LeftWindows = 91, RightWindows = 92, Apps = 93, Sleep = 95, Num0 = 96, Num1 = 97, Num2 = 98, Num3 = 99, Num4 = 100, Num5 = 101, Num6 = 102, Num7 = 103, Num8 = 104, Num9 = 105, NumMultiply = 106, NumAdd = 107, Separator = 108, NumSubtract = 109, NumDecimal = 110, NumDivide = 111, F1 = 112, F2 = 113, F3 = 114, F4 = 115, F5 = 116, F6 = 117, F7 = 118, F8 = 119, F9 = 120, F10 = 121, F11 = 122, F12 = 123, F13 = 124, F14 = 125, F15 = 126, F16 = 127, F17 = 128, F18 = 129, F19 = 130, F20 = 131, F21 = 132, F22 = 133, F23 = 134, F24 = 135, NumLock = 144, Scroll = 145, ShiftLeft = 160, ShiftRight = 161, ControlLeft = 162, ControlRight = 163, AltLeft = 164, AltRight = 165, BrowserBack = 166, BrowserForward = 167, BrowserRefresh = 168, BrowserStop = 169, BrowserSearch = 170, BrowserFavorites = 171, BrowserHome = 172, VolumeMute = 173, VolumeDown = 174, VolumeUp = 175, MediaNextTrack = 176, MediaPreviousTrack = 177, MediaStop = 178, MediaPlayPause = 179, LaunchMail = 180, SelectMedia = 181, LaunchApplication1 = 182, LaunchApplication2 = 183, Semicolon = 186, Equals = 187, Comma = 188, Minus_Dash = 189, Dot = 190, Slash = 191, GraveAccent = 192, ChatPadGreen = 202, ChatPadOrange = 203, SquareBracketOpen = 219, Backslash = 220, SquareBracketClose = 221, Quote = 222, Oem8 = 223, OemBackslash = 226, ProcessKey = 229, OemCopy = 242, OemAuto = 243, OemEnlW = 244, Attn = 246, Crsel = 247, Exsel = 248, EraseEof = 249, Play = 250, Zoom = 251, Pa1 = 253, OemClear = 254
		}
		public static string Key_To_Text_Get(Keys key)
		{
			var shift = Key_Is_Pressed_Check(Keys.ShiftLeft) || Key_Is_Pressed_Check(Keys.ShiftRight);
			var result = "";
			switch (key)
			{
				case Keys.Space: result = " "; break;
				case Keys._0: result = shift ? ")" : "0"; break;
				case Keys._1: result = shift ? "!" : "1"; break;
				case Keys._2: result = shift ? "@" : "2"; break;
				case Keys._3: result = shift ? "#" : "3"; break;
				case Keys._4: result = shift ? "$" : "4"; break;
				case Keys._5: result = shift ? "%" : "5"; break;
				case Keys._6: result = shift ? "^" : "6"; break;
				case Keys._7: result = shift ? "&" : "7"; break;
				case Keys._8: result = shift ? "*" : "8"; break;
				case Keys._9: result = shift ? "(" : "9"; break;
				case Keys.A: result = "a"; break;
				case Keys.B: result = "b"; break;
				case Keys.C: result = "c"; break;
				case Keys.D: result = "d"; break;
				case Keys.E: result = "e"; break;
				case Keys.F: result = "f"; break;
				case Keys.G: result = "g"; break;
				case Keys.H: result = "h"; break;
				case Keys.I: result = "i"; break;
				case Keys.J: result = "j"; break;
				case Keys.K: result = "k"; break;
				case Keys.L: result = "l"; break;
				case Keys.M: result = "m"; break;
				case Keys.N: result = "n"; break;
				case Keys.O: result = "o"; break;
				case Keys.P: result = "p"; break;
				case Keys.Q: result = "q"; break;
				case Keys.R: result = "r"; break;
				case Keys.S: result = "s"; break;
				case Keys.T: result = "t"; break;
				case Keys.U: result = "u"; break;
				case Keys.V: result = "v"; break;
				case Keys.W: result = "w"; break;
				case Keys.X: result = "x"; break;
				case Keys.Y: result = "y"; break;
				case Keys.Z: result = "z"; break;
				case Keys.Num0: result = "0"; break;
				case Keys.Num1: result = "1"; break;
				case Keys.Num2: result = "2"; break;
				case Keys.Num3: result = "3"; break;
				case Keys.Num4: result = "4"; break;
				case Keys.Num5: result = "5"; break;
				case Keys.Num6: result = "6"; break;
				case Keys.Num7: result = "7"; break;
				case Keys.Num8: result = "8"; break;
				case Keys.Num9: result = "9"; break;
				case Keys.NumMultiply: result = "*"; break;
				case Keys.NumAdd: result = "+"; break;
				case Keys.NumSubtract: result = "-"; break;
				case Keys.NumDecimal: result = "."; break;
				case Keys.NumDivide: result = "/"; break;
				case Keys.Semicolon: result = shift ? ":" : ";"; break;
				case Keys.Equals: result = shift ? "+" : "="; break;
				case Keys.Comma: result = shift ? "<" : ","; break;
				case Keys.Minus_Dash: result = shift ? "_" : "-"; break;
				case Keys.Dot: result = shift ? ">" : "."; break;
				case Keys.Slash: result = shift ? "?" : "/"; break;
				case Keys.GraveAccent: result = shift ? "~" : "`"; break;
				case Keys.SquareBracketOpen: result = shift ? "{" : "["; break;
				case Keys.Backslash: result = shift ? "|" : "\\"; break;
				case Keys.SquareBracketClose: result = shift ? "}" : "]"; break;
				case Keys.Quote: result = shift ? "\"" : "'"; break;
				default: result = null; break;
			}
			result = shift && result != null ? result.ToUpper() : result;
			return result;
		}
		public static List<Keys> Keys_Pressed_Get()
		{
			var result = new List<Keys>();
			var keysPressed = Keyboard.GetState().GetPressedKeys();
			for (int i = 0; i < keysPressed.Length; i++)
			{
				result.Add((Keys)(int)keysPressed[i]);
			}
			return result;
		}
		public static List<Keys> Keys_Just_Pressed_Get() => new List<Keys>(keys_just_pressed);
		public static List<Keys> Keys_Just_Released_Get() => new List<Keys>(keys_just_released);
		public static bool Key_Is_Pressed_Check(Keys key) => Keyboard.GetState().IsKeyDown((Microsoft.Xna.Framework.Input.Keys)(int)key);

		public static Vector2 Mouse_Cursor_Position_World_Get()
		{
			var scale = new Vector2((float)canvas_size.X / screen_size.X, (float)canvas_size.Y / screen_size.Y);
			var pos = new Vector2(Mouse.GetState().Position.X, Mouse.GetState().Position.Y) * scale;
			return pos;
		}
		public static Vector2 Mouse_Cursor_Position_Window_Get() => Mouse_Cursor_Position_World_Get() + new Vector2(Camera.Position_X_Get(), Camera.Position_Y_Get());
		public static void Mouse_Cursor_Show(bool shown) => game.IsMouseVisible = shown;
		public static bool Mouse_Cursor_Is_Shown_Check() => game.IsMouseVisible == false;
		public static bool Mouse_Button_Is_Pressed_Left_Check() => Mouse.GetState().LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
		public static bool Mouse_Button_Is_Pressed_Middle_Check() => Mouse.GetState().MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
		public static bool Mouse_Button_Is_Pressed_Right_Check() => Mouse.GetState().RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
		public static void Mouse_Cursor_From_Sprite_Set(string sprite_path, int origin_x, int origin_y)
		{
			if (sprite_path == null || sprites.ContainsKey(sprite_path) == false) return;

			Mouse.SetCursor(MouseCursor.FromTexture2D(sprites[sprite_path], origin_x, origin_y));
		}

		public static bool Press_Into_Hold_Check(string name, bool condition, float seconds_delay = 0.5f, float updates_per_second = 0.1f)
		{
			if (Gate.Opened_Check($"{name}-gate", condition))
			{
				Signal.Create(name, seconds_delay);
				return true;
			}
			else if (Timer.Occurance_Check(name, updates_per_second))
			{
				return condition;
			}
			return false;
		}
	}
	public static class Gate
	{
		public static int Entries_Count_Get(string name) => name != null && gate_entries_count.ContainsKey(name) ? gate_entries_count[name] : 0;
		public static void Entries_Remove(string name)
		{
			if (name == null || gate_entries_count.ContainsKey(name) == false) return;
			gate_entries_count[name] = default;
		}
		public static void Close(string name)
		{
			if (name == null || gates.ContainsKey(name) == false) return;
			gates.Remove(name);
		}
		public static bool Opened_Check(string name, bool condition, int maximum_entries = int.MaxValue)
		{
			if (name == null || (gates.ContainsKey(name) == false && condition == false)) return false;
			else if (gates.ContainsKey(name) == false && condition == true)
			{
				gates[name] = true;
				gate_entries_count[name] = 1;
				return true;
			}
			else
			{
				if (gates[name] == true && condition == true) return false;
				else if (gates[name] == false && condition == true)
				{
					gates[name] = true;
					gate_entries_count[name]++;
					return true;
				}
				else if (gate_entries_count[name] < maximum_entries) gates[name] = false;
			}
			return false;
		}
	}
	public static class Signal
	{
		public static void Create(string name, float seconds_delay)
		{
			if (name == null) return;
			seconds_delay = Number.Limited_Get(seconds_delay, 0, float.MaxValue);
			signal_pauses[name] = false;
			signal_start_times[name] = Performance.Time_Get();
			signal_delays[name] = seconds_delay;
			signal_end_times[name] = Performance.Time_Get() + seconds_delay;
		}
		public static bool Exists_Check(string name) => name != null && signal_start_times.ContainsKey(name);
		public static float Seconds_Delay_Get(string name) => name != null && signal_delays.ContainsKey(name) ? signal_delays[name] : 0;
		public static void Pause(string name, bool paused)
		{
			if (name == null || signal_pauses.ContainsKey(name) == false) return;
			signal_pauses[name] = paused;
		}
		public static float Seconds_Left_Get(string name)
		{
			if (name == null || signal_end_times.ContainsKey(name) == false) return 0;
			var result = signal_end_times[name] - Performance.Time_Get();
			return result < 0 ? 0 : result;
		}
		public static float Time_Start_Get(string name) => name != null && signal_start_times.ContainsKey(name) ? signal_start_times[name] : 0;
		public static float Time_Occur_Get(string name) => name != null && signal_end_times.ContainsKey(name) ? signal_end_times[name] : 0;
		public static bool Occurance_Check(string name, bool delete)
		{
			if (name == null) return false;
			if (signal_delays.ContainsKey(name) == false) return false;
			if (signal_pauses.ContainsKey(name) == true && signal_pauses[name])
			{
				signal_end_times[name] += ticks_delta_time;
				return false;
			}
			if (time >= signal_end_times[name])
			{
				if (delete) Delete(name);
				return true;
			}
			return false;
		}
		public static void Delete(string name)
		{
			if (name == null) return;
			if (signal_end_times.ContainsKey(name)) signal_end_times.Remove(name);
			if (signal_pauses.ContainsKey(name)) signal_pauses.Remove(name);
			if (signal_start_times.ContainsKey(name)) signal_start_times.Remove(name);
			if (signal_delays.ContainsKey(name)) signal_delays.Remove(name);
		}
	}
	public static class Timer
	{
		private static Dictionary<string, int> repeats = new Dictionary<string, int>();

		public static bool Occurance_Check(string name, float intervals_in_seconds, int repeats = 1_000_000)
		{
			intervals_in_seconds = Number.Limited_Get(intervals_in_seconds, 0.1f, 100_000);
			if (Gate.Opened_Check(name, Signal.Occurance_Check(name, false), repeats))
			{
				Signal.Create(name, intervals_in_seconds);
				Timer.repeats[name] = repeats;
				return true;
			}
			return false;
		}
		public static float Seconds_Get(string name) => Repeat_Count_Get(name) * Signal.Seconds_Delay_Get(name);
		public static float Seconds_Left_Get(string name)
		{
			var repeats = Repeats_Get(name);
			var delay = Signal.Seconds_Delay_Get(name);
			var seconds = Seconds_Get(name);
			return repeats * delay - seconds;
		}
		public static int Repeat_Count_Get(string name) => Gate.Entries_Count_Get(name);
		public static int Repeats_Get(string name) => name != null && repeats.ContainsKey(name) ? repeats[name] : 0;
		public static void Restart(string name) => Gate.Entries_Remove(name);
	}
	public static class Console
	{
		public static void Show()
		{
			console_shown = true;
			AllocConsole();
			_Console_Update();
		}
		public static bool Is_Shown() => console_shown;
		public static string Input_Get() => System.Console.ReadLine();
		public static void Log(string message)
		{
			console_log = $"{console_log}{message}";
			_Console_Update();
		}
	}

	private class Pair
	{
		private object first;
		private object second;

		public Pair() { }
		public void Set(object first, object second)
		{
			this.first = first;
			this.second = second;
		}
		public T First_Get<T>() => (T)first;
		public T Second_Get<T>() => (T)second;
		public override string ToString() => $"pair[first:{first}][second:{second}]";
	}

	public class Pair_Texts
	{
		private Pair pair = new Pair();

		public static Pair_Texts Created_Get() => new Pair_Texts();
		public Pair_Texts() { }
		public void Set(string first, string second) => pair.Set(first, second);
		public string First_Get() => pair.First_Get<string>();
		public string Second_Get() => pair.Second_Get<string>();
		public override string ToString() => $"pair_texts[first:{First_Get()}][second:{Second_Get()}]";
	}

	public class Pair_Numbers
	{
		private Pair pair = new Pair();

		public static Pair_Numbers Created_Get(float first = 0, float second = 0) => new Pair_Numbers(first, second);
		public Pair_Numbers(float first = 0, float second = 0) => pair.Set(first, second);
		public void Set(float first, float second) => pair.Set(first, second);
		public float First_Get() => pair.First_Get<float>();
		public float Second_Get() => pair.Second_Get<float>(); 
		public override string ToString() => $"pair_numbers[first:{First_Get()}][second:{Second_Get()}]";
	}
	public class Point
	{
		private Pair_Numbers point = new Pair_Numbers();

		public static Point Created_Get(float x = 0, float y = 0) => new Point(x, y);
		public Point(float x = 0, float y = 0) => point.Set(x, y);
		public virtual void Set(float x, float y) => point.Set(x, y);
		public float X_Get() => point.First_Get();
		public float Y_Get() => point.Second_Get();
		public override string ToString() => $"point[x:{X_Get()}][y:{Y_Get()}]";
	}
	public class Size
	{
		private Pair_Numbers size = new Pair_Numbers();

		public static Size Created_Get(float width = 1, float height = 1) => new Size();
		public Size(float width = 1, float height = 1) => size.Set(width, height);
		public virtual void Set(float width, float height) => size.Set(width, height);
		public float Width_Get() => size.First_Get();
		public float Height_Get() => size.Second_Get();
		public override string ToString() => $"size[width:{Width_Get()}][height:{Height_Get()}]";
	}

	public class Point_Grid
	{
		private Point original_point = new Point();
		private Point point = new Point();
		private Size grid_size = new Size();

		public static Point_Grid Created_Get(float x = 0, float y = 0, float grid_width = 1, float grid_height = 1) => new Point_Grid(x, y, grid_width, grid_height);
		public Point_Grid(float x = 0, float y = 0, float grid_width = 1, float grid_height = 1) => Set(new Point(x, y), new Size(grid_width, grid_height));
		public void Set(Point point, Size grid_size)
		{
			this.grid_size = grid_size;
			original_point = point;
			var x = grid_size.Width_Get() * (int)Math.Round((float)point.X_Get() / grid_size.Width_Get());
			var y = grid_size.Height_Get() * (int)Math.Round((float)point.Y_Get() / grid_size.Height_Get());
			this.point.Set(x, y);
		}
		public Point Original_Get() => original_point;
		public Point Get() => point;
		public Size Size_Get() => grid_size;
		public override string ToString() => $"point_grid(original_{original_point})(grid_{point})(grid_{grid_size})";
	}

	private class Session : TcpSession
	{
		public Session(TcpServer server) : base(server) { }

		protected override void OnConnected()
		{
			// Send invite message
			//string message = "Hello from TCP! Please send a message!";
			//SendAsync(message);
		}
		protected override void OnDisconnected()
		{
			var disconnected_client = client_ids[Id.ToString()];
			client_unique_names.Remove(disconnected_client);
			server.Multicast($"~{(int)Message_Type.Client_Disconnected}|{disconnected_client}");
			console_log = $"{console_log}\nClient [{disconnected_client}] just disconnected.";
			_Console_Update();
		}
		protected override void OnReceived(byte[] buffer, long offset, long size)
		{
			var raw_messages = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
			var messages = raw_messages.Split('~', StringSplitOptions.RemoveEmptyEntries);
			var message_back = "";
			foreach (var message in messages)
			{
				var components = message.Split('|');
				var message_type = (Message_Type)int.Parse(components[0]);
				switch (message_type)
				{
					case Message_Type.Connection: // A client just connected and sent his ID & unique name
						{
							var id = components[1];
							var unique_name = components[2];
							if (client_unique_names.Contains(unique_name)) // Is the unique name free?
							{
								unique_name = ChangeUniqueName(unique_name);
								message_back = $"~{(int)Message_Type.Unique_Name_Change}|{id}|{unique_name}"; // Send a message back with a free one towards the same ID so the client can recognize it's for him
							}
							client_ids[Id.ToString()] = unique_name;
							client_unique_names.Add(unique_name);
							message_back = $"{message_back}~{(int)Message_Type.Client_Online}|{unique_name}"; // Sticking another message to update the newcoming client about online clients
							foreach (var client in client_unique_names)
							{
								message_back = $"{message_back}|{client}";
							}
							message_back = $"{message_back}~{(int)Message_Type.Client_Connected}|{unique_name}"; // Sticking a third message to update online clients about the newcomer.
							console_log = $"{console_log}\nClient [{unique_name}] just connected.";
							_Console_Update();
							break;
						}
					case Message_Type.Message_To_All: // A client wants to send a message to everyone
						{
							message_back = $"{message_back}~{message}";
							break;
						}
					case Message_Type.Message_To_Client: // A client wants to send a message to another client
						{
							message_back = $"{message_back}~{message}";
							break;
						}
				}
			}
			if (message_back != "")
			{
				server.Multicast(message_back);
			}
		}
		protected override void OnError(SocketError error)
		{
			console_log = $"{console_log}\nServer Error: {error}";
			_Console_Update();
		}
		private string ChangeUniqueName(string unique_name)
		{
			var i = 0;
			while (true)
			{
				i++;
				if (client_unique_names.Contains($"{unique_name}{i}") == false)
				{
					break;
				}
			}
			return $"{unique_name}{i}";
		}
	}
	private class Server : TcpServer
	{
		public Server(IPAddress address, int port) : base(address, port) { }
		protected override TcpSession CreateSession() { return new Session(this); }
		protected override void OnError(SocketError error)
		{
			server_is_running = false;
			console_log = $"{console_log}\nServer Error: {error}";
			_Console_Update();
		}
	}
	private class Client : TcpClient
	{
		private bool stop;

		public Client(string address, int port) : base(address, port) { }

		public void DisconnectAndStop()
		{
			stop = true;
			DisconnectAsync();
			while (IsConnected) Thread.Yield();
		}
		protected override void OnConnected()
		{
			client_is_connected = true;
			client_unique_names.Add(client_unique_name);
			console_log = $"{console_log}\nConnected as [{client_unique_name}] to {client.Socket.RemoteEndPoint}.";
			_Console_Update();
			client.SendAsync($"~{(int)Message_Type.Connection}|{client.Id}|{client_unique_name}");
		}
		protected override void OnDisconnected()
		{
			if (client_is_connected)
			{
				client_is_connected = false;
				console_log = $"Disconnected.";
				client_unique_names.Clear();
			}

			// Wait for a while...
			Thread.Sleep(1000);

			// Try to connect again
			console_log = $"{console_log}\nTrying to reconnect...";
			if (stop == false) ConnectAsync();
			_Console_Update();
		}
		protected override void OnReceived(byte[] buffer, long offset, long size)
		{
			var raw_messages = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
			var messages = raw_messages.Split('~', StringSplitOptions.RemoveEmptyEntries);
			var message_back = "";
			foreach (var message in messages)
			{
				var components = message.Split('|');
				var message_type = (Message_Type)int.Parse(components[0]);
				switch (message_type)
				{
					case Message_Type.Unique_Name_Change: // Server said someone's unique name is taken and sent a free one
						{
							if (components[1] == client.Id.ToString()) // Is this for me?
							{
								console_log = $"{console_log}\nMy Unique Name [{client_unique_name}] is taken so my new Unique Name is [{components[2]}].";
								client_unique_names.Remove(client_unique_name);
								client_unique_name = components[2];
								client_unique_names.Add(client_unique_name);
							}
							break;
						}
					case Message_Type.Client_Connected: // Server said some client connected
						{
							if (components[1] != client_unique_name) // If not me
							{
								client_unique_names.Add(components[1]);
								console_log = $"{console_log}\nClient [{components[1]}] just connected.";
							}
							break;
						}
					case Message_Type.Client_Disconnected: // Server said some client disconnected
						{
							client_unique_names.Remove(components[1]);
							console_log = $"{console_log}\nClient [{components[1]}] just disconnected.";
							break;
						}
					case Message_Type.Client_Online: // Someone just connected and is getting updated on who is already online
						{
							if (components[1] == client_unique_name) // For me?
							{
								for (int i = 2; i < components.Length; i++)
								{
									if (client_unique_names.Contains(components[i]) == false)
									{
										client_unique_names.Add(components[i]);
									}
								}
							}
							break;
						}
					case Message_Type.Message_To_All: // A client is sending a message to everybody
						{
							if (components[1] == client_unique_name) // Is this my message coming back to me?
							{
								break;
							}
							_Add_Message(components[1], components[2]);
							console_log = $"{console_log}\nClient [{components[1]}] sent everyone a message: {components[2]}";
							break;
						}
					case Message_Type.Message_To_Client: // A client is sending a message to another client
						{
							if (components[1] == client_unique_name) // Is this my message coming back to me?
							{
								break;
							}
							if (components[2] == client_unique_name) // Is it for me?
							{
								_Add_Message(components[1], components[3]);
								console_log = $"{console_log}\nClient [{components[1]}] sent me a message: {components[3]}";
							}
							break;
						}
				}
			}
			_Console_Update();
			if (message_back != "")
			{
				client.SendAsync(message_back);
			}
		}
		protected override void OnError(SocketError error)
		{
			client_is_connected = false;
			console_log = $"{console_log}\nClient Error: {error}";
			_Console_Update();
		}
	}

	private static string _Clients_Online_Get()
	{
		var result = "";
		for (int i = 0; i < client_unique_names.Count; i++)
		{
			var separator = i == client_unique_names.Count - 1 ? "" : ", ";
			result = $"{result}[{client_unique_names[i]}]{separator}";
		}
		return result;
	}
	private static void _Add_Message(string from, string message)
	{
		if (last_messages.ContainsKey(from) == false)
		{
			last_messages[from] = new List<string>();
		}
		last_messages[from].Add(message);
	}
	private static void _Console_Update()
	{
		if (console_shown == false) return;
		System.Console.Clear();
		var clients_connected = server_is_running || client_is_connected ? $"Clients Connected ({client_unique_names.Count}): {_Clients_Online_Get()}\n\n" : "";
		var connect_info = server_is_running || client_is_connected ? connect_to_server_info + "\n\n" : "";

		System.Console.Title = $"Console | {Window.Title_Get()}";
		System.Console.WriteLine($"{connect_info}{clients_connected}{console_log}");
	}
	private static void _Draw_Tile(Texture2D texture, Vector2 position, Microsoft.Xna.Framework.Point tile_index, int grid_size, Microsoft.Xna.Framework.Point size, Vector2 origin, Vector2 scale, Color color, float angle, SpriteEffects spriteEffects)
	{
		var texture_start_position = new Microsoft.Xna.Framework.Point(tile_index.X * size.X + (grid_size * tile_index.X), tile_index.Y * size.Y + (grid_size * tile_index.Y));
		sprite_batch.Draw(texture, position, new Rectangle(texture_start_position.X, texture_start_position.Y, size.X, size.Y), color, (float)Math.PI / 180 * angle, origin, scale, spriteEffects, 0);
	}
	private static bool _Rectangle_Contains_Point(Vector2 rectA, Vector2 rectB, Vector2 rectC, Vector2 point)
	{
		// AB, AP, BC, BP = vector
		//P of coordinates (x, y) is inside the rectangle if 0 <= dot(AB, AP) <= dot(AB, AB) && 0 <= dot(BC, BP) <= dot(BC, BC)

		var ab = rectB - rectA;
		var ap = point - rectA;
		var bc = rectC - rectB;
		var bp = point - rectB;

		var dot1 = Vector2.Dot(ab, ap);
		var dot2 = Vector2.Dot(ab, ab);
		var dot3 = Vector2.Dot(bc, bp);
		var dot4 = Vector2.Dot(bc, bc);

		return (0 <= dot1 && dot1 < dot2) && (0 <= dot3 && dot3 < dot4);
	}
	private static bool _Line_Crosses_Line(Vector2 startA, Vector2 endA, Vector2 startB, Vector2 endB)
	{
		return _ccw(startA, startB, endB) != _ccw(endA, startB, endB) && _ccw(startA, endA, startB) != _ccw(startA, endA, endB);
	}
	private static bool _ccw(Vector2 a, Vector2 b, Vector2 c)
	{
		return (c.Y - a.Y) * (b.X - a.X) > (b.Y - a.Y) * (c.X - a.X);
	}
	private static Vector2 _Get_Cross_Point_Of_Two_Lines(Vector2 startA, Vector2 endA, Vector2 startB, Vector2 endB)
	{
		var p1 = startA;
		var p2 = startB;
		var n1 = endA - startA;
		var n2 = endB - startB;

		Vector2 p1End = p1 + n1; // another point in line p1->n1
		Vector2 p2End = p2 + n2; // another point in line p2->n2

		float m1 = (p1End.Y - p1.Y) / (p1End.X - p1.X); // slope of line p1->n1
		float m2 = (p2End.Y - p2.Y) / (p2End.X - p2.X); // slope of line p2->n2

		float b1 = p1.Y - m1 * p1.X; // y-intercept of line p1->n1
		float b2 = p2.Y - m2 * p2.X; // y-intercept of line p2->n2

		float px = (b2 - b1) / (m1 - m2); // collision x
		float py = m1 * px + b1; // collision y

		return new Vector2(px, py); // return statement
	}
}