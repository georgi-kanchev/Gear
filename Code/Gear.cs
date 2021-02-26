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
using System.Globalization;
using System.Collections;

public static class Gear
{
	///<summary>
	///text, <paramref name="param"/>, <see cref="char"/>, <typeparamref name="Type"/>
	///</summary>
	private static void SummaryExample() { }

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
	private static SpriteBatch spriteBatch;
	private static RenderTarget2D renderTarget;
	private static SamplerState renderSamplerState;
	private static CanvasPixelFilter renderPixelFilter;
	private static Server server;
	private static Client client;

	private enum MessageType
	{
		Connection, UniqueNameChange, ClientConnected, ClientDisconnected, ClientOnline, ClientMessageToAll, ClientMessageToClient, ClientMessageToServer, ServerMessageToAll, ServerMessageToClient, ClientMessageToAllAndServer
	}
	public enum RotationSamples
	{
		Left, Right, Up, Down, UpLeft, UpRight, DownLeft, DownRight
	}
	public enum CanvasPixelFilter
	{
		Lowest, Medium, Highest
	}
	public enum InputKeys
	{
		None = 0, BackSpace = 8, Tab = 9, Enter = 13, Pause = 19, CapsLock = 20, Kana = 21, Kanji = 25, Escape = 27, ImeConvert = 28, ImeNoConvert = 29, Space = 32, PageUp = 33, PageDown = 34, End = 35, Home = 36, Left = 37, Up = 38, Right = 39, Down = 40, Select = 41, Print = 42, Execute = 43, PrintScreen = 44, Insert = 45, Delete = 46, Help = 47, _0 = 48, _1 = 49, _2 = 50, _3 = 51, _4 = 52, _5 = 53, _6 = 54, _7 = 55, _8 = 56, _9 = 57, A = 65, B = 66, C = 67, D = 68, E = 69, F = 70, G = 71, H = 72, I = 73, J = 74, K = 75, L = 76, M = 77, N = 78, O = 79, P = 80, Q = 81, R = 82, S = 83, T = 84, U = 85, V = 86, W = 87, X = 88, Y = 89, Z = 90, LeftWindows = 91, RightWindows = 92, Apps = 93, Sleep = 95, Num0 = 96, Num1 = 97, Num2 = 98, Num3 = 99, Num4 = 100, Num5 = 101, Num6 = 102, Num7 = 103, Num8 = 104, Num9 = 105, NumMultiply = 106, NumAdd = 107, Separator = 108, NumSubtract = 109, NumDecimal = 110, NumDivide = 111, F1 = 112, F2 = 113, F3 = 114, F4 = 115, F5 = 116, F6 = 117, F7 = 118, F8 = 119, F9 = 120, F10 = 121, F11 = 122, F12 = 123, F13 = 124, F14 = 125, F15 = 126, F16 = 127, F17 = 128, F18 = 129, F19 = 130, F20 = 131, F21 = 132, F22 = 133, F23 = 134, F24 = 135, NumLock = 144, Scroll = 145, ShiftLeft = 160, ShiftRight = 161, ControlLeft = 162, ControlRight = 163, AltLeft = 164, AltRight = 165, BrowserBack = 166, BrowserForward = 167, BrowserRefresh = 168, BrowserStop = 169, BrowserSearch = 170, BrowserFavorites = 171, BrowserHome = 172, VolumeMute = 173, VolumeDown = 174, VolumeUp = 175, MediaNextTrack = 176, MediaPreviousTrack = 177, MediaStop = 178, MediaPlayPause = 179, LaunchMail = 180, SelectMedia = 181, LaunchApplication1 = 182, LaunchApplication2 = 183, Semicolon = 186, Equals = 187, Comma = 188, MinusDash = 189, Dot = 190, Slash = 191, GraveAccent = 192, ChatPadGreen = 202, ChatPadOrange = 203, SquareBracketOpen = 219, Backslash = 220, SquareBracketClose = 221, Quote = 222, Oem8 = 223, OemBackslash = 226, ProcessKey = 229, OemCopy = 242, OemAuto = 243, OemEnlW = 244, Attn = 246, Crsel = 247, Exsel = 248, EraseEof = 249, Play = 250, Zoom = 251, Pa1 = 253, OemClear = 254
	}
	public enum NumberRoundType
	{
		Closest, Up, Down
	}
	public enum NumberTimeConvertType
	{
		MillisecondsToSeconds,
		SecondsToMilliseconds, SecondsToMinutes, SecondsToHours,
		MinutesToMilliseconds, MinutesToSeconds, MinutesToHours, MinutesToDays,
		HoursToSeconds, HoursToMinutes, HoursToDays, HoursToWeeks,
		DaysToMinutes, DaysToHours, DaysToWeeks,
		WeeksToHours, WeeksToDays
	}

	private static PerformanceCounter ramAvailable = new PerformanceCounter("Memory", "Available MBytes");
	private static PerformanceCounter ramUsedPercent = new PerformanceCounter("Memory", "% Committed Bytes In Use");

	private static Dictionary<string, SpriteFont> fonts = new Dictionary<string, SpriteFont>();
	private static Dictionary<string, Texture2D> sprites = new Dictionary<string, Texture2D>();//, spriteOutlines = new Dictionary<string, Texture2D>(), spriteFills = new Dictionary<string, Texture2D>();
	private static Dictionary<string, SoundEffectInstance> sounds = new Dictionary<string, SoundEffectInstance>();
	private static Dictionary<string, SoundEffect> soundsRaw = new Dictionary<string, SoundEffect>();
	private static Dictionary<string, Song> melodies = new Dictionary<string, Song>();
	private static Dictionary<string, bool> gates = new Dictionary<string, bool>(), signalpauses = new Dictionary<string, bool>();
	private static Dictionary<string, int> gateEntriesCount = new Dictionary<string, int>();
	private static Dictionary<string, string> clientIDs = new Dictionary<string, string>();
	private static Dictionary<string, float> signalEndTimes = new Dictionary<string, float>(), signalstarttimes = new Dictionary<string, float>(), signalDelays = new Dictionary<string, float>();

	private static List<InputKeys> lastFrameKeysPressed = new List<InputKeys>(), keysJustPressed = new List<InputKeys>(), keysJustReleased = new List<InputKeys>();
	private static List<Body> bodiesAll = new List<Body>();
	private static List<float> tpsAverages = new List<float>(), fpsAverages = new List<float>();
	private static List<string> clientUniqueNames = new List<string>();

	private static int tick, frame, frameRendered, tpsAverageIndex, fpsAverageIndex, loadingPercent, loadingScreenUpdatePerFiles = 10, loadedFiles, contentFileCount, serverPort = 1234;
	private static bool textDisplayDraw, loading = true, pauseUnfocus, render, sleepPrevented, consoleShown, clientIsConnected, serverIsRunning, networkLogMessagesToConsole;
	private static float textDisplayScale, tps, tpsAverage, fps, fpsAverage, ticksDeltaTime, framesDeltaTime, time;
	private static string textDisplayFont, textDisplayMessage, mainDir = AppDomain.CurrentDomain.BaseDirectory, consoleLog, connectToServerInfo, clientUniqueName;

	private static DateTime lastTickTime, lastFrameTime;
	private static Color backgroundColor = new Color(0, 0, 0);
	private static Size canvasSize = new Size(1920, 1080), screenSize = new Size(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height), pixelSize;
	private static Point cameraPosition;
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
			game = Create();
		}

		/// <summary>
		/// - Example code setup:<br></br>
		/// <paramref name="public"/> <paramref name="override"/> <see cref="Program"/> <typeparamref name="Create"/>() => <paramref name="this"/>;<br></br>
		/// </summary>
		public abstract Program Create();
		/// <summary>
		/// - Has to return a <see cref="string"/>[] containing <paramref name="folder"/>/<paramref name="name"/>.<paramref name="extension"/> for the small amount of content files that need to be loaded before the <typeparamref name="Loading"/> <typeparamref name="Screen"/> so they can be used during <see cref="EachLoadingScreenUpdate"/> while the rest of the content files are being loaded.<br></br>
		/// - The <paramref name="folder"/> part of the path is skipped if the file is directly inside the Content folder.<br></br><br></br>
		/// - Example code setup:<br></br>
		/// // the following code pre-loads two files<br></br>
		/// // the first with path: Content/folder/name.extension<br></br>
		/// // the second with path: Content/name.extension<br></br>
		/// <paramref name="public"/> <paramref name="override"/> <see cref="string"/>[] <typeparamref name="LoadingScreenPrepare"/>() => <paramref name="new"/> <see cref="string"/>[] { "<typeparamref name="folder"/>/<typeparamref name="name"/>.<typeparamref name="extension"/>", "<typeparamref name="name"/>.<typeparamref name="extension"/>" };<br></br>
		/// </summary>
		public virtual string[] LoadingScreenPrepare() => new string[0];
		/// <summary>
		/// - The place for the code that displays and updates the visuals of the <typeparamref name="Loading"/> <typeparamref name="Screen"/> with the small amount of content files loaded through <see cref="LoadingScreenPrepare"/>. The pre-loaded files can be used by each <see cref="Body"/>.<br></br>
		/// - The <see cref="int"/> <paramref name="parameter"/> contains the loading %.
		/// </summary>
		public virtual void EachLoadingScreenUpdate(int percentLoaded) { }
		/// <summary>
		/// - The place for all program code.<br></br>
		/// - The <see cref="int"/> <paramref name="parameter"/> contains the tick count.<br></br><br></br>
		/// - The tick count and information about ticks/frames can be checked through <see cref="Performance.GetTickCount"/>.
		/// </summary>
		public abstract void EachTick(int tickCount);

		public virtual void NetworkMessageJustReceived(string sender, string message) { }

		protected override void Initialize()
		{
			spriteBatch = new SpriteBatch(game.GraphicsDevice);

			graphics.PreferredBackBufferWidth = (int)screenSize.GetW();
			graphics.PreferredBackBufferHeight = (int)screenSize.GetH();
			graphics.HardwareModeSwitch = false;
			graphics.IsFullScreen = true;
			game.Window.Position = new Microsoft.Xna.Framework.Point(0, 0);

			renderSamplerState = SamplerState.PointWrap;

			renderTarget = new RenderTarget2D(game.GraphicsDevice, (int)screenSize.GetW(), (int)screenSize.GetH(), false, game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
			Canvas.SetPixelSize(1, 1);

			graphics.ApplyChanges();
			game.Window.Title = "Gear";
			game.IsMouseVisible = true;

			// start maximized
			//var form = (Form)Control.FromHandle(Window.Handle);
			//form.WindowState = FormWindowState.Maximized;

			//anti pc sleep
			SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED);

			var content = program.LoadingScreenPrepare();
			foreach (var file in content)
			{
				LoadFile(file);
			}
			CountContentFiles();
			LoadAllContent();

			base.Initialize();
		}
		protected override void Update(GameTime gameTime)
		{
			if (pauseUnfocus && game.IsActive == false)
			{
				return;
			}
			if (loading)
			{
				LoadAllContent();
				program.EachLoadingScreenUpdate(loadingPercent);
			}
			else
			{
				tick++;
				AdvanceTickTime();
				UpdateOnKeys();

				try
				{
					program.EachTick(tick);
				}
				catch (Exception ex)
				{
					AllocConsole();
					System.Console.WriteLine($"{ex.Source}: {ex.Message}");
					System.Console.ReadLine();
					throw;
				}

			}
			base.Update(gameTime);
		}

		private static void AdvanceTickTime()
		{
			var delta = lastTickTime == default ? default : DateTime.Now - lastTickTime;
			ticksDeltaTime = (float)delta.TotalSeconds;
			tps = 60 / ((float)delta.TotalSeconds * 60);
			tps = double.IsInfinity(tps) ? tpsAverage : tps;
			AdvanceTPSAverage();
			time += ticksDeltaTime;
			lastTickTime = DateTime.Now;
		}
		private static void AdvanceTPSAverage()
		{
			if (tpsAverageIndex == 60)
			{
				tpsAverageIndex = 0;
			}
			if (tpsAverages.Contains(tps))
			{
				return;
			}
			if (tpsAverageIndex == tpsAverages.Count)
			{
				tpsAverages.Add(tps);
			}
			tpsAverages[tpsAverageIndex] = tps;
			tpsAverage = tpsAverages.Average();
			tpsAverageIndex++;
		}

		private static void UpdateOnKeys()
		{
			var keysPressed = Input.GetKeysPressed();

			keysJustPressed.Clear();
			keysJustReleased.Clear();
			foreach (var key in keysPressed)
			{
				if (lastFrameKeysPressed.Contains(key) == false)
				{
					keysJustPressed.Add(key);
				}
			}
			foreach (var key in lastFrameKeysPressed)
			{
				if (keysPressed.Contains(key) == false)
				{
					keysJustReleased.Add(key);
				}
			}

			lastFrameKeysPressed = Input.GetKeysPressed();
		}

		protected override void Draw(GameTime gameTime)
		{
			frame++;
			if (render == false) return;
			frameRendered++;

			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, renderSamplerState, DepthStencilState.Default, RasterizerState.CullNone);
			GraphicsDevice.SetRenderTarget(renderTarget);
			GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };

			GraphicsDevice.Clear(new Microsoft.Xna.Framework.Color((int)backgroundColor.GetR(), (int)backgroundColor.GetG(), (int)backgroundColor.GetB(), 255));

			// draw =======================================================
			AdvanceFrameTime();
			DrawAllBodies();
			DrawTextDisplay();
			// draw =======================================================

			GraphicsDevice.SetRenderTarget(null);
			var scale = new Vector2(pixelSize.GetW(), pixelSize.GetH());
			spriteBatch.Draw(renderTarget, Vector2.Zero, null, Microsoft.Xna.Framework.Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
			render = false;
			spriteBatch.End();
			base.Draw(gameTime);
		}
		private static void AdvanceFrameTime()
		{
			var delta = lastFrameTime == default ? default : DateTime.Now - lastFrameTime;
			framesDeltaTime = (float)delta.TotalSeconds;
			fps = 60 / ((float)delta.TotalSeconds * 60);
			fps = double.IsInfinity(fps) ? fpsAverage : fps;
			AdvanceFPSAverage();
			lastFrameTime = DateTime.Now;
		}
		private static void AdvanceFPSAverage()
		{
			if (fpsAverageIndex == 60)
			{
				fpsAverageIndex = 0;
			}
			if (fpsAverages.Contains(fps))
			{
				return;
			}
			if (fpsAverageIndex == fpsAverages.Count)
			{
				fpsAverages.Add(fps);
			}
			fpsAverages[fpsAverageIndex] = fps;
			fpsAverage = fpsAverages.Average();
			fpsAverageIndex++;
		}

		private static void DrawAllBodies()
		{
			foreach (var body in bodiesAll)
			{
				var sprite = body.GetSpriteName();
				if (sprite == null)
				{
					continue;
				}
				var spriteShown = body.SpriteIsDisplayed();
				var tileIndex = body.GetSpriteGridIndexes();
				var pos = body.GetPosition() + cameraPosition;
				var size = body.GetSize();
				var spritesize = body.GetSpriteSize();
				var scale = size / spritesize;
				var origin = body.GetSpriteOrigin();
				var color = body.GetSpriteColor();
				var boundariesSprite = new Texture2D(graphics.GraphicsDevice, 1, 1);
				var originSprite = new Texture2D(graphics.GraphicsDevice, 1, 1);
				var angleSprite = new Texture2D(graphics.GraphicsDevice, 1, 1);
				var data = new Microsoft.Xna.Framework.Color[1] { Microsoft.Xna.Framework.Color.White };
				boundariesSprite.SetData(data);
				originSprite.SetData(data);
				angleSprite.SetData(data);

				if (spriteShown)
					DrawTile(sprites[sprite], pos - origin, tileIndex, body.GetSpriteGridSize(), size / scale, new Point(), scale, color, body.GetAngleA(), SpriteEffects.None);

				var boundariescolor = body.GetBoundariesColor();
				if (boundariesSprite != null && body.BoundariesAreDisplayed())
				{
					DrawTile(boundariesSprite, pos - origin, new Point(), 0, new Size(size.GetW(), 1), new Point(), new Size(), boundariescolor, body.GetAngleA(), SpriteEffects.None);
					DrawTile(boundariesSprite, pos - origin, new Point(), 0, new Size(1, size.GetH()), new Point(), new Size(), boundariescolor, body.GetAngleA(), SpriteEffects.None);
				}

				var anglecolor = body.GetAngleColor();
				if (angleSprite != null && body.AngleIsDisplayed())
					DrawTile(angleSprite, pos, new Point(), 0, new Size(size.GetW() * 1.1f, 1), new Point(), new Size(1, 1), anglecolor, body.GetAngleA(), SpriteEffects.None);

				var origincolor = body.GetOriginColor();
				if (originSprite != null && body.OriginIsDisplayed())
					DrawTile(originSprite, pos, new Point(), 0, new Size(1, 1), new Point(), new Size(), origincolor, body.GetAngleA(), SpriteEffects.None);

				boundariesSprite.Dispose();
				angleSprite.Dispose();
				originSprite.Dispose();
			}
		}
		private static void DrawTextDisplay()
		{
			if (textDisplayFont != null && textDisplayDraw && fonts.ContainsKey(textDisplayFont) && string.IsNullOrWhiteSpace(textDisplayMessage) == false)
			{
				var fontsize = fonts[textDisplayFont].MeasureString("a") / 18 * textDisplayScale;
				spriteBatch.DrawString(fonts[textDisplayFont], textDisplayMessage, new Vector2(fontsize.Y, fontsize.Y), Microsoft.Xna.Framework.Color.Black, 0, Vector2.Zero, textDisplayScale, SpriteEffects.None, 0);
				spriteBatch.DrawString(fonts[textDisplayFont], textDisplayMessage, new Vector2(0, 0), Microsoft.Xna.Framework.Color.White, 0, Vector2.Zero, textDisplayScale, SpriteEffects.None, 0);
			}
		}

		private static void CountContentFiles()
		{
			var directories = Directory.GetDirectories($"{mainDir}\\Content").ToList();
			for (int i = 0; i < directories.Count; i++)
			{
				CountFolder(directories[i]);
			}
			CountFolder($"{mainDir}\\Content");
			loadingScreenUpdatePerFiles = (int)Math.Ceiling(contentFileCount / 10d);
		}
		private static void CountFolder(string folder)
		{
			if (Directory.Exists(folder) == false)
			{
				return;
			}

			var files = Directory.GetFiles(folder);
			var datafiles = new List<string>();

			for (int i = 0; i < files.Length; i++)
			{
				if (files[i].Contains(".png") || files[i].Contains(".spritefont"))
				{
					datafiles.Add(files[i]);
				}
			}

			contentFileCount += datafiles.Count;

			var currentDirectories = Directory.GetDirectories(folder).ToList();
			while (currentDirectories.Count > 0)
			{
				CountFolder(currentDirectories[0]);
				currentDirectories.RemoveAt(0);
			}
		}

		private static void LoadAllContent()
		{
			var directories = Directory.GetDirectories($"{mainDir}\\Content").ToList();
			for (int i = 0; i < directories.Count; i++)
			{
				LoadFolder(directories[i]);
			}
			LoadFolder($"{mainDir}\\Content");
			if (loadedFiles >= contentFileCount)
			{
				loading = false;
			}
		}
		private static void LoadFolder(string folder)
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
					var loaded = LoadFile($"{result}\\{split[split.Length - 1]}");
					if (loaded == false)
					{
						continue;
					}
				}
				catch (Exception)
				{
					continue;
				}
				if (loadedFiles % loadingScreenUpdatePerFiles == 0)
				{
					goto end;
				}
			}
			var currentDirectories = Directory.GetDirectories(folder).ToList();
			while (currentDirectories.Count > 0)
			{
				LoadFolder(currentDirectories[0]);
				currentDirectories.RemoveAt(0);
			}
		end:;
		}
		private static bool LoadFile(string name)
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
						soundsRaw[key] = game.Content.Load<SoundEffect>(key);
						sounds[key] = soundsRaw[key].CreateInstance();
						break;
					}
				case "mp3": melodies[key] = game.Content.Load<Song>(key); break;
				default: return false;
			}
			loadedFiles++;
			loadingPercent = (int)((float)loadedFiles / contentFileCount * 100);
			loadingPercent = loadingPercent > 100 ? 100 : loadingPercent;
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

		/// <summary>
		/// - Smooths out the edges of the pixels according to the <paramref name="pixelfilter"/>. Higher filters apply better image quality but cost more performance.<br></br><br></br>- Pixel art projects go best with <see cref="PixelFilter.Lowest"/>.<br></br>- High resolution projects go best with the rest. <br></br><br></br>
		/// - The current filter can be checked with <see cref="CanvasPixelFilterGet"/>.
		/// </summary>
		/// <param name="pixelfilter"></param>
		public static void SetPixelFilter(CanvasPixelFilter pixelFilter)
		{
			renderPixelFilter = pixelFilter;
			switch (renderPixelFilter)
			{
				case CanvasPixelFilter.Lowest: renderSamplerState = SamplerState.PointWrap; break;
				case CanvasPixelFilter.Medium: renderSamplerState = SamplerState.LinearWrap; break;
				case CanvasPixelFilter.Highest: renderSamplerState = SamplerState.AnisotropicWrap; break;
			}
		}
		/// <summary>
		/// - Gets the current pixel filter and returns it.<br></br><br></br>
		/// - Pixel filters can be changed and researched through <see cref="CanvasPixelFilterGet"/>.
		/// </summary>
		public static CanvasPixelFilter GetPixelFilter()
		{
			return renderPixelFilter;
		}
		/// <summary>
		/// - Sets the size of the displayed pixel relative to the user's monitor resolution. Each displayed pixel is equal to <paramref name="width"/> and <paramref name="height"/> of screen pixels.<br></br><br></br> - The canvas size can be checked with <see cref="SizeGetW"/> and <see cref="SizeGetH"/>.<br></br> - The user's screen size can be checked with <see cref="User.ScreenSizeGetW"/> and <see cref="User.ScreenSizeGetW"/>.
		/// </summary>
		public static void SetPixelSize(int width, int height)
		{
			width = (int)Number.LimitedGet(width, 1, screenSize.GetW());
			height = (int)Number.LimitedGet(height, 1, (int)screenSize.GetH());
			pixelSize = new Size(width, height);
			canvasSize = screenSize / new Size(width, height);
			var gd = game.GraphicsDevice;
			renderTarget = new RenderTarget2D(gd, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, false, gd.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
			graphics.ApplyChanges();
		}
		public static Size GetPixelSize()
		{
			return pixelSize;
		}
		public static Size GetSize()
		{
			return canvasSize;
		}
		/// <summary>
		/// - Sets the background color's hues to <paramref name="r"/>, <paramref name="g"/>, <paramref name="b"/>.<br></br><br></br>
		/// - Those values must be between 0 and 255 inclusively.<br></br><br></br>
		/// - Those hues can be checked through<br></br>
		/// <see cref="BackgroundColorGetRed"/><br></br>
		/// <see cref="BackgroundColorGetGren"/><br></br>
		/// <see cref="BackgroundColorGetBlue"/><br></br>
		/// </summary>
		public static void SetBackgroundColor(float r, float g, float b)
		{
			backgroundColor = new Color(r, g, b);
		}
		public static Color GetBackgroundColor()
		{
			return backgroundColor;
		}
	}
	public static class Window
	{
		/// <summary>
		/// - Checks wether the window is currently focused by the user and returns the result.
		/// </summary>
		public static bool IsFocused() => game.IsActive;
		/// <summary>
		/// - Pause is <paramref name="activated"/> when the user has the window unfocused or minimized.<br></br><br></br>
		/// - A check wether this pause is activated can be done through <see cref="WindowUnfocusedPauseIsActivatedCheck"/>.<br></br><br></br>
		/// - A check wether the user has focused the window can be done through <see cref="WindowIsFocusedCheck"/>.
		/// </summary>
		public static void PauseWhenUnfocused(bool pause)
		{
			pauseUnfocus = pause;
		}
		/// <summary>
		/// - Checks if the window pause when unfocusing or minimizing the window is activated and returns the result.<br></br><br></br>
		/// - The window pause can be activated or deactivated through <see cref="WindowUnfocusedPauseActivate"/>.<br></br><br></br>
		/// - A check wether the user has focused the window can be done through <see cref="WindowIsFocusedCheck"/>.
		/// </summary>
		public static bool IsPausingWhenUnfocused()
		{
			return pauseUnfocus;
		}

		public static void Display(bool display)
		{
			var form = Control.FromHandle(game.Window.Handle) as Form;
			if (display) form.Show();
			else form.Hide();
		}

		/// <summary>
		/// - Sets the <paramref name="title"/> of the window.<br></br><br></br>
		/// - The title can be received with <see cref="WindowTitleGet"/>.
		/// </summary>
		public static void SetTitle(string title)
		{
			game.Window.Title = title;
		}
		/// <summary>
		/// - Gets the title of the window and returns it.<br></br><br></br>
		/// - The title can be changed through <see cref="WindowTitleSet"/>.
		/// </summary>
		public static string GetTitle()
		{
			return game.Window.Title;
		}

		/// <summary>
		/// - Sets the Alt+F4 functionality to <paramref name="activated"/>. When <paramref name="activated"/> pressing Alt+F4 closes the window.<br></br><br></br>
		/// - A check wether the Alt+F4 functionality is activated can be done through <see cref="WindowCloseHotkeysIsActivatedCheck"/>.<br></br><br></br>
		/// - The window can be also closed through <see cref="WindowClose"/>.
		/// </summary>
		public static void ActivateCloseHotkey(bool activated)
		{
			game.Window.AllowAltF4 = activated;
		}
		/// <summary>
		/// - Checks wether the Alt+F4 functionality is activated and returns the result.<br></br><br></br>
		/// - The Alt+F4 functionality can be activated or deactivated with <see cref="WindowCloseHotkeysActivate(bool)"/>.
		/// </summary>
		public static bool CloseHotkeyIsActivated()
		{
			return game.Window.AllowAltF4;
		}

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
		private static Dictionary<string, Body> bodyUniqueNames = new Dictionary<string, Body>();

		public static List<Body> GetAllBodies()
		{
			return new List<Body>(bodiesAll);
		}
		public static Body GetByUniqueName(string uniquename)
		{
			if (uniquename == null || bodyUniqueNames.ContainsKey(uniquename) == false)
			{
				return default;
			}
			return bodyUniqueNames[uniquename];
		}

		[JsonProperty]
		private Color spriteColor, boundariesColor, originColor, angleColor;
		[JsonProperty]
		private Size size, spriteSize;
		[JsonProperty]
		private Point position, spriteOrigin, spriteIndex;
		[JsonProperty]
		private int UID, spriteGridSize;
		[JsonProperty]
		private Angle angle;
		[JsonProperty]
		private string uniqueName, spriteName;
		[JsonProperty]
		private bool boundariesShown, originShown, angleShown, spriteShown;

		public Body(string uniqueName)
		{
			Instantiate();
			SetUniqueName(uniqueName);
		}
		private void Instantiate()
		{
			bodiesAll.Add(this);
			UID = ID;
			ID++;
		}
		public Body Duplicate(string uniqueName)
		{
			var dup = new Body(uniqueName);
			dup.spriteColor = spriteColor;
			dup.boundariesColor = boundariesColor;
			dup.originColor = originColor;
			dup.angleColor = angleColor;
			dup.size = size;
			dup.spriteSize = spriteSize;
			dup.position = position;
			dup.spriteOrigin = spriteOrigin;
			dup.spriteIndex = spriteIndex;
			dup.spriteGridSize = spriteGridSize;
			dup.angle = angle;
			dup.spriteName = spriteName;
			dup.boundariesShown = boundariesShown;
			dup.originShown = originShown;
			dup.angleShown = angleShown;
			dup.spriteShown = spriteShown;
			return dup;
		}

		public int GetUniqueID()
		{
			return UID;
		}
		public string GetUniqueName()
		{
			return uniqueName;
		}
		public void SetUniqueName(string uniqueName)
		{
			if (uniqueName == this.uniqueName) return;
			else if (uniqueName == null)
			{
				Console.LogError($"{nameof(Body)}'s {nameof(uniqueName)} cannot be null.");
			}
			if (bodyUniqueNames.ContainsKey(uniqueName))
			{
				Console.LogError($"Another {nameof(Body)} with {nameof(uniqueName)} '{uniqueName}' already exists." +
					$"Make sure you are not creating it multiple times or each tick.");
			}

			this.uniqueName = uniqueName;
			bodyUniqueNames.Add(uniqueName, this);
		}

		private void _SetPosition(Point pos)
		{
			position = pos;
			render = true;
		}
		public void SetPositionXY(float x, float y)
		{
			_SetPosition(new Point(x, y));
		}
		public void SetPositionX(float x)
		{
			_SetPosition(new Point(x, position.GetY()));
		}
		public void SetPositionY(float y)
		{
			_SetPosition(new Point(position.GetX(), y));
		}
		public void SetPosition(Point position)
		{
			_SetPosition(position);
		}
		public Point GetPosition()
		{
			return position;
		}
		public float GetPositionX()
		{
			return position.GetX();
		}
		public float GetPositionY()
		{
			return position.GetY();
		}

		private void _SetAngle(Angle angle)
		{
			this.angle = angle;
			render = true;
		}
		public void SetAngleA(float a)
		{
			_SetAngle(new Angle(a));
		}
		public void SetAngle(Angle angle)
		{
			_SetAngle(angle);
		}
		public float GetAngleA()
		{
			return angle.GetA();
		}
		public Angle GetAngle()
		{
			return angle;
		}

		public void _SetSize(Size size)
		{
			this.size = size;
			render = true;
		}
		public void SetSize(Size size)
		{
			_SetSize(size);
		}
		public void SetSizeWH(float w, float h)
		{
			_SetSize(new Size(w, h));
		}
		public void SetSizeW(float w)
		{
			_SetSize(new Size(w, size.GetH()));
		}
		public void SetSizeH(float h)
		{
			_SetSize(new Size(size.GetW(), h));
		}
		public Size GetSize()
		{
			return size;
		}
		public float GetSizeW()
		{
			return size.GetW();
		}
		public float GetSizeH()
		{
			return size.GetH();
		}

		public void DisplayAngle(bool show = true, float r = 255, float g = 255, float b = 255, float o = 255)
		{
			angleShown = show;
			angleColor.Set(r, g, b, o);
			render = true;
		}
		public bool AngleIsDisplayed()
		{
			return angleShown;
		}
		public Color GetAngleColor()
		{
			return angleColor;
		}

		public void DisplayOrigin(bool show = true, float r = 255, float g = 255, float b = 255, float o = 255)
		{
			originShown = show;
			originColor.Set(r, g, b, o);
			render = true;
		}
		public bool OriginIsDisplayed()
		{
			return originShown;
		}
		public Color GetOriginColor()
		{
			return originColor;
		}

		public void DisplayBoundaries(bool show = true, float colorRed = 255, float colorGreen = 255, float colorBlue = 255, float o = 255)
		{
			boundariesShown = show;
			boundariesColor.Set(colorRed, colorGreen, colorBlue, o);
			render = true;
		}
		public bool BoundariesAreDisplayed()
		{
			return boundariesShown;
		}
		public Color GetBoundariesColor()
		{
			return boundariesColor;
		}

		public void SetSprite(string name, bool show = true, int width = 64, int height = 64, float r = 255, float g = 255, float b = 255, float o = 255, int originX = 0, int originY = 0, int gridSize = 0, int indexH = 0, int indexV = 0)
		{
			if (sprites.ContainsKey(name) == false)
			{
				throw new ArgumentException($"No sprite with name '{name}' was found. In order to load a sprite:\n1. Add it to the 'Content' folder.\n2. Add it to the Solution Explorer's 'Content' folder.\n3. In its properties select Copy to Output Directory: 'Copy Always'.\n4. Open 'Content.mgcb' with the MonoGame Content Pipeline Tool.\n5. Add it to the Content and build/rebuild it.");
			}
			spriteName = name;
			size = new Size(sprites[name].Width, sprites[name].Height);
			spriteSize = new Size(width, height);
			spriteColor = new Color(r, g, b, o);
			spriteOrigin = new Point(originX, originY);
			spriteGridSize = gridSize;
			spriteIndex = new Point(indexH, indexV);
			spriteShown = show;
			render = true;
		}
		public string GetSpriteName()
		{
			return spriteName;
		}

		public bool SpriteIsDisplayed()
		{
			return spriteShown;
		}
		public int GetSpriteGridSize()
		{
			return spriteGridSize;
		}
		public Point GetSpriteGridIndexes()
		{
			return spriteIndex;
		}
		public Point GetSpriteOrigin()
		{
			return spriteOrigin;
		}
		public Size GetSpriteSize()
		{
			return spriteSize;
		}
		public Color GetSpriteColor()
		{
			return spriteColor;
		}

		public override string ToString()
		{
			return $"[{UID}] {uniqueName}";
		}
	}
	/// <summary>
	/// Controls <see cref="float"/> in different ways.
	/// </summary>
	public static class Number
	{
		public static float UnsignedGet(float number) => Math.Abs(number);
		public static float AveragedGet(float numberA, float numberB) => (numberA + numberB) / 2;
		public static float RandomizedGet(float lowerBound, float upperBound, int precision)
		{
			precision = (int)LimitedGet(precision, 0, 5);
			if (lowerBound > upperBound)
			{
				var swap = lowerBound;
				lowerBound = upperBound;
				upperBound = swap;
			}
			var precisionValue = (float)Math.Pow(10, precision);
			var lowerInt = Convert.ToInt32(lowerBound * Math.Pow(10, PrecisionGet(lowerBound)));
			var upperInt = Convert.ToInt32(upperBound * Math.Pow(10, PrecisionGet(upperBound)));
			var randInt = new Random(Guid.NewGuid().GetHashCode()).Next((int)(lowerInt * precisionValue), (int)(upperInt * precisionValue) + 1);
			var result = randInt / precisionValue;

			return result;
		}
		//public static float RoundedGet(float number, int precision, NumberRoundType numberroundtype)
		public static float RoundedGet(float number, NumberRoundType RoundType)
		{
			// doesn't work with values like 0.00300007 or 0.1234567
			var precision = 0; //(int)LimitedGet(precision, 0, 5);
			var a = (float)Math.Pow(10, PrecisionGet(number));
			var b = (float)Math.Pow(10, precision);
			var c = number * a;
			switch (RoundType)
			{
				case NumberRoundType.Closest: return Convert.ToInt32(c) / b;
				case NumberRoundType.Up: return (float)Math.Ceiling(c) / b;
				default: return (float)Math.Floor(c) / b;
			}
		}
		public static float LimitedGet(float number, float minimum, float maximum)
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
		public static float PercentedTowardsTargetGet(float number, float targetNumber, float percent)
		{
			var vec = new Vector2(number, 0);
			var targetVec = new Vector2(targetNumber, 0);
			var result = Vector2.Lerp(vec, targetVec, percent / 100);

			return result.X;
		}
		public static float ChangedGet(float number, float numbersPerSecond) => number + (numbersPerSecond * ticksDeltaTime);
		public static float TowardsTargetGet(float number, float targetNumber, float numbersPerSecond)
		{
			if (number <= targetNumber && targetNumber * ticksDeltaTime < 0) return targetNumber;
			else if (number >= targetNumber && targetNumber * ticksDeltaTime > 0) return targetNumber;
			return ChangedGet(number, numbersPerSecond);
		}
		public static float TimeConvertedGet(float time, NumberTimeConvertType convertType)
		{
			switch (convertType)
			{
				case NumberTimeConvertType.MillisecondsToSeconds: return time / 1000;
				case NumberTimeConvertType.SecondsToMilliseconds: return time * 1000;
				case NumberTimeConvertType.SecondsToMinutes: return time / 60;
				case NumberTimeConvertType.SecondsToHours: return time / 3600;
				case NumberTimeConvertType.MinutesToMilliseconds: return time * 60000;
				case NumberTimeConvertType.MinutesToSeconds: return time * 60;
				case NumberTimeConvertType.MinutesToHours: return time / 60;
				case NumberTimeConvertType.MinutesToDays: return time / 1440;
				case NumberTimeConvertType.HoursToSeconds: return time * 3600;
				case NumberTimeConvertType.HoursToMinutes: return time * 60;
				case NumberTimeConvertType.HoursToDays: return time / 24;
				case NumberTimeConvertType.HoursToWeeks: return time / 168;
				case NumberTimeConvertType.DaysToMinutes: return time * 1440;
				case NumberTimeConvertType.DaysToHours: return time * 24;
				case NumberTimeConvertType.DaysToWeeks: return time / 7;
				case NumberTimeConvertType.WeeksToHours: return time * 168;
				case NumberTimeConvertType.WeeksToDays: return time * 7;
			}
			return 0;
		}
		public static bool ChanceCheck(float percent)
		{
			percent = LimitedGet(percent, 0, 100);
			var n = RandomizedGet(1, 100, 0);
			return n <= percent;
		}
		public static float FromTextGet(string text)
		{
			var result = 0f;
			text = text.Replace(',', '.');
			var parsed = float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			if (parsed) return result;
			else Console.LogError($"{nameof(FromTextGet)}(\"{text}\"): The provided text is not a number.");
			return result;
		}
		public static int PrecisionGet(float number)
		{
			var result = 0;
			var numberstr = number.ToString();
			if (numberstr.Contains('.')) result = number.ToString().Split('.')[1].Length;
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
		public static string FromListGet<T>(List<T> list, string separator = ", ")
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
		public static string FromArrayGet<T>(T[] array, string separator = ", ")
		{
			return FromListGet(array.ToList(), separator);
		}

		/// <summary>
		/// Adds <paramref name="text"/> to the clipboard (copies it). It can be accessed later via <typeparamref name="Ctrl"/> + <typeparamref name="V"/> or <see cref="ClipboardGet"/>.
		/// </summary>
		public static void ClipboardCopy(string text)
		{
			Clipboard.SetText(text);
		}
		/// <summary>
		/// Gets the clipboard data and returns it if it is a <see cref="string"/>, otherwise returns <paramref name="null"/>.‪‪
		/// </summary>
		public static string ClipboardGet()
		{
			var result = Clipboard.GetText();
			return result == string.Empty ? null : result;
		}

		/// <summary>
		/// Returns a new <see cref="string"/> after a simple encryption on <paramref name="text"/> with a <paramref name="key"/> that can be <paramref name="performedtwice"/>.‪‪ The encryption can be decrypred later and the text can be retrieved back with <see cref="DecryptedGet"/>.
		/// </summary>
		public static string EncryptedGet(string text, char key, bool performedTwice = false)
		{
			var result = text;
			var times = performedTwice ? 2 : 1;
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
		/// Returns the decrypted version of an encrypted <paramref name="text"/> with a <paramref name="key"/> that could have been <paramref name="performedtwice"/> with <see cref="EncryptedGet"/>.‪‪
		/// </summary>
		public static string DecryptedGet(string encryptedText, char key, bool performedTwice = false)
		{
			var result = encryptedText;
			var times = performedTwice ? 2 : 1;
			for (int i = 0; i < times; i++)
			{
				result = Decrypt(result, key);
			}
			return result;
		}
		private static string Decrypt(string encryptedText, char key)
		{
			var amplifier = Convert.ToByte(key);
			byte[] data = Convert.FromBase64String(encryptedText);
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(data[i] ^ amplifier);
			}
			return Encoding.UTF8.GetString(data);
		}

		/// <summary>
		/// Converts <typeparamref name="T"/> <paramref name="data"/> into a <see cref="string"/> (<paramref name="JSON"/>) and returns it. It can be converted and retrieved back to <typeparamref name="T"/> <paramref name="data"/> later with <see cref="ToDataConvert"/>.‪‪
		/// </summary>
		public static string FromDataConvert<T>(T data)
		{
			return JsonConvert.SerializeObject(data);
		}
		/// <summary>
		/// Converts an already formatted <paramref name="text"/> (<paramref name="JSON"/>) into <typeparamref name="T"/> <paramref name="data"/> and returns it if the <paramref name="text"/> is in the correct format. Otherwise returns <paramref name="default"/>(<typeparamref name="T"/>).
		/// </summary>
		public static T ToDataConvert<T>(string text)
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
		/// Creates or overwrites a file on <paramref name="filepath"/> with <paramref name="filename"/> and <paramref name="fileextension"/> then fills it with <paramref name="text"/>. This <paramref name="text"/> can be accessed later with <see cref="Load"/>.<br></br><br></br>
		/// This is a slow operation - do not call frequently.
		/// </summary>
		public static void Save(string text, string filePath = "", string fileName = "data", string fileExtension = "data")
		{
			try
			{
				File.WriteAllText($"{mainDir}\\{filePath}\\{fileName}.{fileExtension}", text);
			}
			catch (Exception)
			{
				return;
			}
		}
		/// <summary>
		/// Reads the text from the file at <paramref name="filepath"/> with <paramref name="filename"/> and <paramref name="fileextension"/> and returns it as a <see cref="string"/> if successful. Returns <paramref name="null"/> otherwise. A text can be saved to a file with <see cref="Save"/>.<br></br><br></br>
		/// This is a slow operation - do not call frequently.
		/// </summary>
		public static string Load(string filePath = "", string fileName = "data", string fileExtension = "data")
		{
			try
			{
				return File.ReadAllText($"{mainDir}\\{filePath}\\{fileName}.{fileExtension}");
			}
			catch (Exception)
			{
				return default;
			}
		}

		/// <summary>
		/// - Displays a <paramref name="message"/> on the screen with a <paramref name="font"/> that has a <paramref name="scale"/>. The <paramref name="message"/> may <paramref name="overwrite"/> what is already displayed instead of appending it.<br></br><br></br>
		/// - The displayed text can be cleared with <see cref="DisplayClear"/>.
		/// </summary>
		public static void Display(string font, object message, float scale = 1, bool overwrite = false)
		{
			if (fonts.ContainsKey(font) == false)
			{
				return;
			}
			textDisplayDraw = true;
			textDisplayFont = font;
			if (overwrite) textDisplayMessage = "";
			textDisplayMessage = $"{textDisplayMessage}{message}";
			textDisplayMessage = textDisplayMessage.Replace("∞", "Infinity");
			scale = Number.LimitedGet(scale, 0.001f, 5000);
			textDisplayScale = scale;

			var sampleSize = fonts[textDisplayFont].MeasureString("a");
			var sampleSizeScaled = sampleSize * textDisplayScale;
			var visibleLines = (int)(canvasSize.GetH() / sampleSizeScaled.Y);
			var size = fonts[textDisplayFont].MeasureString(textDisplayMessage) * textDisplayScale;
			var lines = textDisplayMessage.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			if (size.Y > canvasSize.GetH() + sampleSizeScaled.Y && lines.Count > 2 && visibleLines < lines.Count)
			{
				textDisplayMessage = "";
				lines[lines.Count - visibleLines] = "...";
				for (int i = lines.Count - visibleLines; i < lines.Count; i++)
				{
					textDisplayMessage = $"{textDisplayMessage}{lines[i]}\n";
				}
			}
			render = true;
		}
		/// <summary>
		/// - Clears all the text on screen that was displayed through <see cref="Display"/>.
		/// </summary>
		public static void DisplayClear()
		{
			textDisplayMessage = null;
			render = true;
		}

		public static string TimeFormattedGet(float seconds, string separator = ":", bool msShow = false, string msFormat = "ms", bool secShow = true, string secFormat = "s", bool minShow = true, string minFormat = "m", bool hrShow = true, string hrFormat = "h")
		{
			seconds = Number.UnsignedGet(seconds);
			var secondsStr = seconds.ToString();
			var ms = 0;
			if (secondsStr.Contains('.'))
			{
				var spl = secondsStr.Split('.');
				ms = int.Parse(spl[1]) * 100;
				seconds = Number.RoundedGet(seconds, NumberRoundType.Down);
			}
			var sec = seconds % 60;
			var min = Number.RoundedGet(seconds / 60 % 60, NumberRoundType.Down);
			var hr = Number.RoundedGet(seconds / 3600, NumberRoundType.Down);
			var msStr = msShow ? $"{ms}" : "";
			var secStr = secShow ? $"{sec}" : "";
			var minStr = minShow ? $"{min}" : "";
			var hrStr = hrShow ? $"{hr}" : "";
			var msF = msShow ? $"{msFormat}" : "";
			var secF = secShow ? $"{secFormat}" : "";
			var minF = minShow ? $"{minFormat}" : "";
			var hrF = hrShow ? $"{hrFormat}" : "";
			var secMsSep = msShow && (secShow || minShow || hrShow) ? $"{separator}" : "";
			var minSecSep = secShow && (minShow || hrShow) ? $"{separator}" : "";
			var hrMinSep = minShow && hrShow ? $"{separator}" : "";

			return $"{hrStr}{hrF}{hrMinSep}{minStr}{minF}{minSecSep}{secStr}{secF}{secMsSep}{msStr}{msF}";
		}
	}
	public static class Performance
	{
		/// <summary>
		/// - Sets the target <paramref name="tps"/> that can be between 2 and 1000 inclusively if <paramref name="limited"/>. The ticks per second may go bellow but not above the targeted speed (depending on performance), otherwise multiple ticks (but not frames) at the same time will occur in order to keep up. <br></br>- The tick rate is also capped to the user's monitor refresh rate if <paramref name="vsynced"/> (vertical synchronization removes scanlines and tearing artifacts). <br></br>- Not <paramref name="limited"/> and not <paramref name="vsynced"/> tick rate uncaps both the frame rate and tick rate, therefore running as fast as possible. <br></br><br></br>- The current tick rate can be checked with <see cref="TicksPerSecondGet"/>.<br></br>- The current target tick rate can be checked with <see cref="TicksPerSecondTargetGet"/>.<br></br>- A check wether the tick rate is <paramref name="limited"/> can be received from <see cref="TicksPerSecondAreLimitedCheck"/>.<br></br>- And check wether they are vertically synchronized from <see cref="TicksPerSecondAreVSyncedCheck"/>.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. The current frame rate can be checked with <see cref="FramesTotalPerSecondGet"/>.<br></br>
		/// </summary>
		public static void TicksPerSecondTargetSet(float tps, bool limited, bool vSynced)
		{
			tps = tps < 2 ? 2 : tps;
			tps = tps > 1000 ? 1000 : tps;
			game.TargetElapsedTime = TimeSpan.FromSeconds(1d / tps);
			game.IsFixedTimeStep = limited;
			graphics.SynchronizeWithVerticalRetrace = vSynced;
			graphics.ApplyChanges();
		}
		/// <summary>
		/// - Gets the current tick rate that can be an <paramref name="average"/> of the previous 60 ticks and returns it. <br></br><br></br>- The target tick speed can be changed via <see cref="TicksPerSecondTargetSet"/>.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. The current frames per second can be checked with <see cref="FramesTotalPerSecondGet"/>.
		/// </summary>
		public static float TicksPerSecondGet(bool average = false)
		{
			return average ? tpsAverage : tps;
		}
		/// <summary>
		/// - Gets the current target tick rate and returns it. <br></br><br></br>
		/// - The target tick speed can be changed via <see cref="TicksPerSecondTargetSet"/>. Also contains information about ticks/frames.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. The current frames per second can be checked with <see cref="FramesTotalPerSecondGet"/>.<br></br><br></br>
		/// </summary>
		public static float TicksPerSecondTargetGet() => 60 / ((float)game.TargetElapsedTime.TotalSeconds * 60);
		/// <summary>
		/// - Checks wether the tick rate is limited to the target tick rate and returns the result.<br></br><br></br>- The limitation of the tick speed and other related changes can be set through<br></br> <see cref="TicksPerSecondTargetSet"/>. Also contains information about ticks/frames.
		/// </summary>
		public static bool TicksPerSecondAreLimitedCheck() => game.IsFixedTimeStep;
		/// <summary>
		/// - Checks wether the tick rate is limited by the user's monitor refresh rate and returns the result.<br></br><br></br>
		/// - The vertical synchronization and other related changes can set through<br></br> <see cref="TicksPerSecondTargetSet"/>. Also contains information about ticks/frames.<br></br><br></br>
		/// </summary>
		public static bool TicksPerSecondAreVSyncedCheck() => graphics.SynchronizeWithVerticalRetrace;
		/// <summary>
		/// - Gets the number of ticks that have passed since the start and returns them.<br></br><br></br>
		/// - The tick count is also provided as an <see cref="int"/> parameter with <see cref="Program.EachTick(int)"/>.<br></br><br></br>
		/// - Changing the tick speed and receiving information about ticks/frames may be done through <see cref="TicksPerSecondTargetSet"/>.
		/// </summary>
		public static int GetTickCount() => tick;

		public static float RAMGBAvailableGet() => ramAvailable.NextValue() / 1000;
		public static float RAMPercentUsedGet() => ramUsedPercent.NextValue();

		/// <summary>
		/// - Gets the current frame rate that can be an <paramref name="average"/> of the previous 60 ticks and returns it.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. A lower frame rate will be present with slow tick rate and vice versa. The targeted tick rate can be changed or uncapped with <see cref="TicksPerSecondTargetSet"/>. Also contains information about ticks/frames.
		/// </summary>
		public static float FramesTotalPerSecondGet(bool average = false)
		{
			return average ? fpsAverage : fps;
		}
		/// <summary>
		/// - Gets the number of frames that have passed since the start and returns them. This counter is not affected by rendering. Therefore a frame might be skipped and the counter will still increment. <br></br><br></br>- Rendered frames counter can be checked with <see cref="FramesRenderedCountGet"/>.<br></br><br></br>
		/// - Changing or uncapping the tick rate and receiving information about ticks/frames can be done through <see cref="TicksPerSecondTargetSet"/>. This affects the frame rate.
		/// </summary>
		public static int FramesTotalCountGet() => frame;
		/// <summary>
		/// - Gets the number of rendered frames that have passed since the start and returns them. Rendered frames happen only when the current frame is different than the last frame. Therefore frames are skipped when the screen is static.<br></br><br></br>
		/// - A check for the total frames counter can be done through <see cref="FramesTotalCountGet"/>.<br></br><br></br>
		/// - Changing or uncapping the tick rate and receiving information about ticks/frames can be done through <see cref="TicksPerSecondTargetSet"/>. This affects the frame rate.
		/// </summary>
		public static int FramesRenderedCountGet() => frameRendered;

		/// <summary>
		/// - Gets the time that has passed since the start and returns it.
		/// </summary>
		public static float TimeGet() => time;
		/// <summary>
		/// - Gets the time that has passed since the last tick and returns it. <br></br><br></br>- The target tick rate can be changed via <see cref="TicksPerSecondTargetSet"/><br></br>- The current target tick rate can be checked with <see cref="TimeSinceLastTickTargetGet"/>.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. The time since last frame can be checked with <see cref="TimeSinceLastFrameGet"/>.
		/// </summary>
		public static float TimeSinceLastTickGet() => ticksDeltaTime;
		/// <summary>
		/// - Gets the target time between ticks and returns it.<br></br><br></br>- The target tick rate can be changed via <see cref="TicksPerSecondTargetSet"/>.<br></br>- The current tick rate can be checked with <see cref="TimeSinceLastTickGet"/>.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. The time since last frame can be checked with <see cref="TimeSinceLastFrameGet"/>.
		/// </summary>
		public static float TimeSinceLastTickTargetGet() => (float)game.TargetElapsedTime.TotalSeconds;
		/// <summary>
		/// - Gets the time that has passed since the last frame and returns it.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. The time since last tick can be checked with <see cref="TimeSinceLastTickGet"/>.
		/// </summary>
		public static float TimeSinceLastFrameGet() => framesDeltaTime;
	}
	public static class Hardware
	{
		public static Size ScreenSizeGet() => screenSize;
		/// <summary>
		/// - When <paramref name="activated"/> the user's computer will stay active at all times, even when left idle.<br></br><br></br>
		/// - A check wether sleep prevention is activated can be done via <see cref="ComputerSleepPreventionIsActivatedCheck"/>.
		/// </summary>
		public static void ComputerSleepPreventionActivate(bool activated)
		{
			sleepPrevented = activated;
			if (sleepPrevented)
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
		/// - Sleep prevention can be activated or deactivated through <see cref="ComputerSleepPreventionActivate"/>.
		/// </summary>
		public static bool ComputerSleepPreventionIsActivatedCheck() => sleepPrevented;
	}
	public static class Network
	{
		public static void PacketsToConsoleLog(bool log) => networkLogMessagesToConsole = log;

		public static int ClientsConnectedCountGet() => clientUniqueNames.Count;

		public static string ServerIPSameDeviceGet() => "127.0.0.1";
		public static void ServerStart()
		{
			var funcName = $"{nameof(ServerStart)}()";
			try
			{
				if (serverIsRunning)
				{
					consoleLog = $"{consoleLog}\n{funcName}: Server is already starting/started.";
					ConsoleUpdate();
					return;
				}
				if (clientIsConnected)
				{
					consoleLog = $"{consoleLog}\n{funcName}: Cannot start a server while a client.";
					ConsoleUpdate();
					return;
				}
				server = new Server(IPAddress.Any, serverPort);
				server.Start();
				consoleLog = $"{consoleLog}\n{funcName}: Started a LAN Server on port {serverPort}.";

				var hostName = Dns.GetHostName();
				var hostEntry = Dns.GetHostEntry(hostName);
				connectToServerInfo = "Clients can connect through those IPs if they are in the same network\n(device / router / Virtual Private Network programs like Hamachi or Radmin): \nSame device: 127.0.0.1";
				foreach (var ip in hostEntry.AddressList)
				{
					if (ip.AddressFamily == AddressFamily.InterNetwork)
					{
						var ipParts = ip.ToString().Split('.');
						var ipType = ipParts[0] == "192" && ipParts[1] == "168" ? "Same router: " : "Same VPN: ";
						connectToServerInfo = $"{connectToServerInfo}\n{ipType}{ip}";
					}
				}

				serverIsRunning = true;
				//NatUtility.DeviceFound += DeviceFound;
				//NatUtility.StartDiscovery();
				ConsoleUpdate();
			}
			catch (Exception ex)
			{
				serverIsRunning = false;
				consoleLog = $"{consoleLog}\n{funcName} Error: {ex.Message}";
				ConsoleUpdate();
				return;
			}
		}
		public static void ServerStop()
		{
			var funcName = $"{nameof(ServerStop)}()";
			try
			{
				if (serverIsRunning == false)
				{
					consoleLog = $"{consoleLog}\n{funcName}: Server is not running.";
					ConsoleUpdate();
					return;
				}
				if (clientIsConnected)
				{
					consoleLog = $"{consoleLog}\n{funcName}: Cannot stop a server while a client.";
					ConsoleUpdate();
					return;
				}
				serverIsRunning = false;
				server.Stop();
				consoleLog = $"{consoleLog}\n{funcName}: The LAN Server on port {serverPort} was stopped.";
				ConsoleUpdate();
			}
			catch (Exception ex)
			{
				serverIsRunning = false;
				consoleLog = $"{consoleLog}\n{funcName} Error: {ex.Message}";
				ConsoleUpdate();
				return;
			}
		}
		public static void ServerMessageSendToAllClients(string message)
		{
			var funcName = $"{nameof(ServerMessageSendToAllClients)}(\"{message}\")";
			if (ServerCannotSendMessage(funcName)) return;

			ServerMessageSent(funcName);
			server.Multicast($"~{(int)MessageType.ServerMessageToAll}|{message}");
		}
		public static void ServerMessageSendToClient(string receiverUniqueName, string message)
		{
			var funcName = $"{nameof(ServerMessageSendToClient)}(\"{receiverUniqueName}\", \"{message}\")";
			if (ServerCannotSendMessage(funcName)) return;

			ServerMessageSent(funcName);
			server.Multicast($"~{(int)MessageType.ServerMessageToClient}|{receiverUniqueName}|{message}");
		}

		public static void ClientConnect(string uniqueName, string ip)
		{
			var funcName = $"{nameof(ClientConnect)}(\"{uniqueName}\", \"{ip}\")";
			if (clientIsConnected)
			{
				consoleLog = $"{consoleLog}\n{funcName}: Already connecting/connected.";
				ConsoleUpdate();
				return;
			}
			if (serverIsRunning)
			{
				consoleLog = $"{consoleLog}\n{funcName}: Cannot connect as a client while a server.";
				ConsoleUpdate();
				return;
			}
			if (uniqueName == null)
			{
				consoleLog = $"{consoleLog}\n{funcName}: Client's unique names cannot be null.";
				ConsoleUpdate();
				return;
			}
			NatUtility.DeviceFound += DeviceFound;
			NatUtility.StartDiscovery();
			clientIsConnected = true;

			try
			{
				client = new Client(ip, serverPort);
			}
			catch (Exception)
			{
				clientIsConnected = false;
				Console.LogError($"{funcName}: {ip} is an invalid IP.");
				return;
			}
			clientUniqueName = uniqueName;
			consoleLog = $"{consoleLog}\n{funcName}: Connecting to {ip}:{serverPort}...";
			ConsoleUpdate();
			client.ConnectAsync();

		}
		public static void ClientDisconnect()
		{
			if (clientIsConnected == false)
			{
				consoleLog = $"{consoleLog}\nClientDisconnect(): Cannot disconnect when not connected.";
				ConsoleUpdate();
				return;
			}
			client.DisconnectAndStop();
		}
		public static string ClientUniqueNameGet() => clientIsConnected ? clientUniqueName : default;
		public static void ClinetMessageSendToAllClients(string message)
		{
			var funcName = $"{nameof(ClinetMessageSendToAllClients)}(\"{message}\")";
			if (ClientCannotSendMessage(funcName)) return;

			ClientMessageSent(funcName);
			client.SendAsync($"~{(int)MessageType.ClientMessageToAll}|{clientUniqueName}|{message}");
		}
		public static void ClinetMessageSendToClient(string receiverUniqueName, string message)
		{
			var funcName = $"{nameof(ClinetMessageSendToClient)}(\"{receiverUniqueName}\", \"{message}\")";
			if (clientUniqueName == receiverUniqueName) return;
			if (ClientCannotSendMessage(funcName)) return;

			ClientMessageSent(funcName);
			client.SendAsync($"~{(int)MessageType.ClientMessageToClient}|{clientUniqueName}|{receiverUniqueName}|{message}");
		}
		public static void ClientMessageSendToServer(string message)
		{
			var funcName = $"{nameof(ClientMessageSendToServer)}(\"{message}\")";
			if (ClientCannotSendMessage(funcName)) return;

			ClientMessageSent(funcName);
			client.SendAsync($"~{(int)MessageType.ClientMessageToServer}|{clientUniqueName}|{message}");
		}
		public static void ClientMessageSendToServerAndAllClients(string message)
		{
			var funcName = $"{nameof(ClientMessageSendToServerAndAllClients)}(\"{message}\")";
			if (ClientCannotSendMessage(funcName)) return;

			ClientMessageSent(funcName);
			client.SendAsync($"~{(int)MessageType.ClientMessageToAllAndServer}|{clientUniqueName}|{message}");
		}

		private static bool ClientCannotSendMessage(string funcName)
		{
			if (MessageDisconnected(funcName)) return true;
			else if (serverIsRunning)
			{
				if (networkLogMessagesToConsole == false) return true;
				consoleLog = $"{consoleLog}\n{funcName}: Cannot send a client message while a server.";
				ConsoleUpdate();
				return true;
			}
			return false;
		}
		private static void ClientMessageSent(string funcName)
		{
			if (networkLogMessagesToConsole == false) return;
			consoleLog = $"{consoleLog}\n{funcName}: Sent.";
			ConsoleUpdate();
		}
		private static bool ServerCannotSendMessage(string funcName)
		{
			if (MessageDisconnected(funcName)) return true;
			else if (clientIsConnected)
			{
				if (networkLogMessagesToConsole == false) return true;
				consoleLog = $"{consoleLog}\n{funcName}: Cannot send a server message while a client.";
				ConsoleUpdate();
				return true;
			}
			return false;
		}
		private static void ServerMessageSent(string funcName)
		{
			if (networkLogMessagesToConsole == false) return;
			consoleLog = $"{consoleLog}\n{funcName}: Sent.";
			ConsoleUpdate();
		}
		private static bool MessageDisconnected(string funcName)
		{
			if (clientIsConnected == false && serverIsRunning == false)
			{
				if (networkLogMessagesToConsole == false) return true;
				consoleLog = $"{consoleLog}\n{funcName}: Cannot send a message while disconnected.";
				ConsoleUpdate();
				return true;
			}
			return false;
		}

		private static void DeviceFound(object sender, DeviceEventArgs args) => args.Device.CreatePortMap(new Mapping(Protocol.Tcp, serverPort, serverPort));
	}
	public static class Camera
	{
		/// <summary>
		/// - Creates a screenshot in <paramref name="path"/>/<paramref name="name"/>.png that contains what is currently visible in the window and saves it as a sprite. The result can be <paramref name="scaled"/> to the user's screen resolution, otherwise takes the canvas resolution.<br></br><br></br>
		/// - The canvas resolution can be received from <see cref="Canvas.SizeGetW"/> and <see cref="Canvas.SizeGetH"/>.<br></br>
		/// - The user's screen resolution can be received from <see cref="Hardware.ScreenSizeGetW"/> and <see cref="Hardware.ScreenSizeGetH"/>.<br></br>
		/// </summary>
		public static void Screenshot(string path, string name, bool scaled)
		{
			var size = new Size(
				scaled ? game.GraphicsDevice.PresentationParameters.BackBufferWidth : canvasSize.GetW(),
				scaled ? game.GraphicsDevice.PresentationParameters.BackBufferHeight : canvasSize.GetH());
			var buffer = new int[(int)(size.GetW() * size.GetH())];
			var texture = new Texture2D(game.GraphicsDevice, (int)size.GetW(), (int)size.GetH());
			var finalPath = $"{mainDir}{path}";

			if (Directory.Exists(finalPath) == false) Directory.CreateDirectory(finalPath);

			if (scaled) game.GraphicsDevice.GetBackBufferData(buffer);
			else renderTarget.GetData(0, new Rectangle(0, 0, (int)size.GetW(), (int)size.GetH()), buffer, 0, (int)(size.GetW() * size.GetH()));

			texture.SetData(buffer);
			using (Stream stream = File.Create($"{finalPath}\\{name}.png"))
			{
				texture.SaveAsPng(stream, (int)size.GetW(), (int)size.GetH());
			}
			sprites[name] = texture;
		}

		public static void PositionSet(float x, float y) => cameraPosition = new Point(x, y);
		public static Point PositionGet() => cameraPosition;
	}
	/// <summary>
	/// - Holds information about the current input of the user.
	/// </summary>
	public static class Input
	{
		public static string KeyToTextGet(InputKeys key)
		{
			var shift = KeyIsPressedCheck(InputKeys.ShiftLeft) || KeyIsPressedCheck(InputKeys.ShiftRight);
			var result = "";
			switch (key)
			{
				case InputKeys.Space: result = " "; break;
				case InputKeys._0: result = shift ? ")" : "0"; break;
				case InputKeys._1: result = shift ? "!" : "1"; break;
				case InputKeys._2: result = shift ? "@" : "2"; break;
				case InputKeys._3: result = shift ? "#" : "3"; break;
				case InputKeys._4: result = shift ? "$" : "4"; break;
				case InputKeys._5: result = shift ? "%" : "5"; break;
				case InputKeys._6: result = shift ? "^" : "6"; break;
				case InputKeys._7: result = shift ? "&" : "7"; break;
				case InputKeys._8: result = shift ? "*" : "8"; break;
				case InputKeys._9: result = shift ? "(" : "9"; break;
				case InputKeys.A: result = "a"; break;
				case InputKeys.B: result = "b"; break;
				case InputKeys.C: result = "c"; break;
				case InputKeys.D: result = "d"; break;
				case InputKeys.E: result = "e"; break;
				case InputKeys.F: result = "f"; break;
				case InputKeys.G: result = "g"; break;
				case InputKeys.H: result = "h"; break;
				case InputKeys.I: result = "i"; break;
				case InputKeys.J: result = "j"; break;
				case InputKeys.K: result = "k"; break;
				case InputKeys.L: result = "l"; break;
				case InputKeys.M: result = "m"; break;
				case InputKeys.N: result = "n"; break;
				case InputKeys.O: result = "o"; break;
				case InputKeys.P: result = "p"; break;
				case InputKeys.Q: result = "q"; break;
				case InputKeys.R: result = "r"; break;
				case InputKeys.S: result = "s"; break;
				case InputKeys.T: result = "t"; break;
				case InputKeys.U: result = "u"; break;
				case InputKeys.V: result = "v"; break;
				case InputKeys.W: result = "w"; break;
				case InputKeys.X: result = "x"; break;
				case InputKeys.Y: result = "y"; break;
				case InputKeys.Z: result = "z"; break;
				case InputKeys.Num0: result = "0"; break;
				case InputKeys.Num1: result = "1"; break;
				case InputKeys.Num2: result = "2"; break;
				case InputKeys.Num3: result = "3"; break;
				case InputKeys.Num4: result = "4"; break;
				case InputKeys.Num5: result = "5"; break;
				case InputKeys.Num6: result = "6"; break;
				case InputKeys.Num7: result = "7"; break;
				case InputKeys.Num8: result = "8"; break;
				case InputKeys.Num9: result = "9"; break;
				case InputKeys.NumMultiply: result = "*"; break;
				case InputKeys.NumAdd: result = "+"; break;
				case InputKeys.NumSubtract: result = "-"; break;
				case InputKeys.NumDecimal: result = "."; break;
				case InputKeys.NumDivide: result = "/"; break;
				case InputKeys.Semicolon: result = shift ? ":" : ";"; break;
				case InputKeys.Equals: result = shift ? "+" : "="; break;
				case InputKeys.Comma: result = shift ? "<" : ","; break;
				case InputKeys.MinusDash: result = shift ? "" : "-"; break;
				case InputKeys.Dot: result = shift ? ">" : "."; break;
				case InputKeys.Slash: result = shift ? "?" : "/"; break;
				case InputKeys.GraveAccent: result = shift ? "~" : "`"; break;
				case InputKeys.SquareBracketOpen: result = shift ? "{" : "["; break;
				case InputKeys.Backslash: result = shift ? "|" : "\\"; break;
				case InputKeys.SquareBracketClose: result = shift ? "}" : "]"; break;
				case InputKeys.Quote: result = shift ? "\"" : "'"; break;
				default: result = null; break;
			}
			result = shift && result != null ? result.ToUpper() : result;
			return result;
		}
		public static List<InputKeys> GetKeysPressed()
		{
			var result = new List<InputKeys>();
			var keysPressed = Keyboard.GetState().GetPressedKeys();
			for (int i = 0; i < keysPressed.Length; i++)
			{
				result.Add((InputKeys)(int)keysPressed[i]);
			}
			return result;
		}
		public static List<InputKeys> KeysJustPressedGet() => new List<InputKeys>(keysJustPressed);
		public static List<InputKeys> KeysJustReleasedGet() => new List<InputKeys>(keysJustReleased);
		public static bool KeyIsPressedCheck(InputKeys key) => Keyboard.GetState().IsKeyDown((Microsoft.Xna.Framework.Input.Keys)(int)key);

		public static Point MouseCursorPositionWorldGet()
		{
			var scale = new Point(canvasSize.GetW() / screenSize.GetW(), canvasSize.GetH() / screenSize.GetH());
			var pos = new Point(Mouse.GetState().Position.X, Mouse.GetState().Position.Y) * scale;
			return pos;
		}
		public static Point MouseCursorPositionWindowGet() => MouseCursorPositionWorldGet() + cameraPosition;
		public static void MouseCursorShow(bool shown) => game.IsMouseVisible = shown;
		public static bool MouseCursorIsShownCheck() => game.IsMouseVisible == false;
		public static bool MouseButtonIsPressedLeftCheck() => Mouse.GetState().LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
		public static bool MouseButtonIsPressedMiddleCheck() => Mouse.GetState().MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
		public static bool MouseButtonIsPressedRightCheck() => Mouse.GetState().RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
		public static void MouseCursorFromSpriteSet(string spritePath, int originX, int originY)
		{
			if (spritePath == null || sprites.ContainsKey(spritePath) == false) return;

			Mouse.SetCursor(MouseCursor.FromTexture2D(sprites[spritePath], originX, originY));
		}

		public static bool PressIntoHoldCheck(string name, bool condition, float secondsDelay = 0.5f, float updatesPerSecond = 0.1f)
		{
			if (Gate.OpenedCheck($"{name}-gate", condition))
			{
				Signal.Create(name, secondsDelay);
				return true;
			}
			else if (Timer.OccuranceCheck(name, updatesPerSecond)) return condition;
			return false;
		}
	}
	public static class Gate
	{
		public static int EntriesCountGet(string name) => name != null && gateEntriesCount.ContainsKey(name) ? gateEntriesCount[name] : 0;
		public static void EntriesRemove(string name)
		{
			if (name == null || gateEntriesCount.ContainsKey(name) == false) return;
			gateEntriesCount[name] = default;
		}
		public static void Close(string name)
		{
			if (name == null || gates.ContainsKey(name) == false) return;
			gates.Remove(name);
		}
		public static bool OpenedCheck(string name, bool condition, int maxEntries = int.MaxValue)
		{
			if (name == null || (gates.ContainsKey(name) == false && condition == false)) return false;
			else if (gates.ContainsKey(name) == false && condition == true)
			{
				gates[name] = true;
				gateEntriesCount[name] = 1;
				return true;
			}
			else
			{
				if (gates[name] == true && condition == true) return false;
				else if (gates[name] == false && condition == true)
				{
					gates[name] = true;
					gateEntriesCount[name]++;
					return true;
				}
				else if (gateEntriesCount[name] < maxEntries) gates[name] = false;
			}
			return false;
		}
	}
	public static class Signal
	{
		public static void Create(string name, float secondsDelay)
		{
			if (name == null) return;
			secondsDelay = Number.LimitedGet(secondsDelay, 0, float.MaxValue);
			signalpauses[name] = false;
			signalstarttimes[name] = Performance.TimeGet();
			signalDelays[name] = secondsDelay;
			signalEndTimes[name] = Performance.TimeGet() + secondsDelay;
		}
		public static bool ExistsCheck(string name) => name != null && signalstarttimes.ContainsKey(name);
		public static float SecondsDelayGet(string name) => name != null && signalDelays.ContainsKey(name) ? signalDelays[name] : 0;
		public static void Pause(string name, bool paused)
		{
			if (name == null || signalpauses.ContainsKey(name) == false) return;
			signalpauses[name] = paused;
		}
		public static float SecondsLeftGet(string name)
		{
			if (name == null || signalEndTimes.ContainsKey(name) == false) return 0;
			var result = signalEndTimes[name] - Performance.TimeGet();
			return result < 0 ? 0 : result;
		}
		public static float TimeStartGet(string name) => name != null && signalstarttimes.ContainsKey(name) ? signalstarttimes[name] : 0;
		public static float TimeOccurGet(string name) => name != null && signalEndTimes.ContainsKey(name) ? signalEndTimes[name] : 0;
		public static bool OccuranceCheck(string name, bool delete)
		{
			if (name == null) return false;
			if (signalDelays.ContainsKey(name) == false) return false;
			if (signalpauses.ContainsKey(name) == true && signalpauses[name])
			{
				signalEndTimes[name] += ticksDeltaTime;
				return false;
			}
			if (time >= signalEndTimes[name])
			{
				if (delete) Delete(name);
				return true;
			}
			return false;
		}
		public static void Delete(string name)
		{
			if (name == null) return;
			if (signalEndTimes.ContainsKey(name)) signalEndTimes.Remove(name);
			if (signalpauses.ContainsKey(name)) signalpauses.Remove(name);
			if (signalstarttimes.ContainsKey(name)) signalstarttimes.Remove(name);
			if (signalDelays.ContainsKey(name)) signalDelays.Remove(name);
		}
	}
	public static class Timer
	{
		private static Dictionary<string, int> repeats = new Dictionary<string, int>();

		public static bool OccuranceCheck(string name, float intervalsInSeconds, int repeats = 1000000)
		{
			intervalsInSeconds = Number.LimitedGet(intervalsInSeconds, 0.1f, 100000);
			if (Gate.OpenedCheck(name, Signal.OccuranceCheck(name, false), repeats))
			{
				Signal.Create(name, intervalsInSeconds);
				Timer.repeats[name] = repeats;
				return true;
			}
			return false;
		}
		public static float SecondsGet(string name) => RepeatCountGet(name) * Signal.SecondsDelayGet(name);
		public static float SecondsLeftGet(string name)
		{
			var repeats = RepeatsGet(name);
			var delay = Signal.SecondsDelayGet(name);
			var seconds = SecondsGet(name);
			return repeats * delay - seconds;
		}
		public static int RepeatCountGet(string name) => Gate.EntriesCountGet(name);
		public static int RepeatsGet(string name) => name != null && repeats.ContainsKey(name) ? repeats[name] : 0;
		public static void Restart(string name) => Gate.EntriesRemove(name);
	}
	public static class Console
	{
		public static void Show()
		{
			consoleShown = true;
			AllocConsole();
			ConsoleUpdate();
		}
		public static bool IsShown() => consoleShown;
		public static string InputGet() => System.Console.ReadLine();
		public static void Log(string message)
		{
			consoleLog = $"{consoleLog}{message}";
			ConsoleUpdate();
		}
		public static void LogError(string message)
		{
			AllocConsole();
			System.Console.Clear();
			throw new Exception(message);
		}
		public static void Clear()
		{
			consoleLog = "";
			ConsoleUpdate();
		}
	}

	public struct Pair<T>
	{
		private T f, s;

		public Pair(T f, T s)
		{
			this.f = f;
			this.s = s;
		}
		public void SetFS(T f, T s)
		{
			this.f = f;
			this.s = s;
		}
		public void SetF(T f)
		{
			this.f = f;
		}
		public void SetS(T s)
		{
			this.s = s;
		}
		public T GetF()
		{
			return f;
		}
		public T GetS()
		{
			return s;
		}

		public override string ToString()
		{
			return $"{nameof(Pair<T>)}[f:{f}][s:{s}]";
		}
		/// <summary>
		/// This default <see cref="object"/> method is not implemented.
		/// </summary>
		public override bool Equals(object obj)
		{
			return default;
		}
		/// <summary>
		/// This default <see cref="object"/> method is not implemented.
		/// </summary>
		public override int GetHashCode()
		{
			return default;
		}
	}
	public struct Storage<UniqueKeyT, ValueT>
	{
		private List<int> indexes;
		private List<UniqueKeyT> keys;
		private List<ValueT> values;
		private Dictionary<UniqueKeyT, ValueT> dict;

		public void Expand(int index, UniqueKeyT uniqueKey, ValueT value,
			bool invalidIndexError = true, bool keyExistsError = true)
		{
			if (keys == null) keys = new List<UniqueKeyT>();
			if (indexes == null) indexes = new List<int>();

			if (values == null) values = new List<ValueT>();
			if (invalidIndexError && index < 0)
			{
				Console.LogError($"The index of [{nameof(uniqueKey)}:{uniqueKey}][{nameof(value)}:{value}] cannot be < 0.");
			}
			else if (index < 0) return;
			if (index >= values.Count)
			{
				var oldListK = new List<UniqueKeyT>(keys);
				var oldListV = new List<ValueT>(values);
				values = new List<ValueT>();
				keys = new List<UniqueKeyT>();
				for (int i = 0; i < index; i++)
				{
					values.Add(default);
					keys.Add(default);
				}
				for (int i = 0; i < oldListV.Count; i++)
				{
					values[i] = oldListV[i];
					keys[i] = oldListK[i];
				}
			}

			if (dict == null) dict = new Dictionary<UniqueKeyT, ValueT>();
			if (keyExistsError && dict.ContainsKey(uniqueKey))
			{
				Console.LogError($"Unique key '{uniqueKey}' already exists.");
			}
			else if (dict.ContainsKey(uniqueKey)) return;

			dict.Add(uniqueKey, value);
			values.Insert(index, value);
			keys.Insert(index, uniqueKey);
			indexes.Add(index);

			var sameIndexMet = false;
			for (int i = 0; i < indexes.Count; i++)
			{
				if (sameIndexMet && indexes[i] == index)
				{
					indexes[i]++;
					continue;
				}
				if (indexes[i] > index)
				{
					indexes[i]++;
				}
				else if (sameIndexMet == false && indexes[i] == index)
				{
					sameIndexMet = true;
				}
			}
		}
		public void ReplaceAt(int index, ValueT value, bool indexNotFoundError = true)
		{
			if (indexNotFoundError && indexes.Contains(index) == false)
			{
				Console.LogError($"The {this}'s index '{index}' was not found.");
			}
			else if (indexes.Contains(index) == false) return;

			values[index] = value;
			dict[keys[index]] = value;
		}
		public void ReplaceIn(UniqueKeyT uniqueKey, ValueT value, bool keyNotFoundError = true)
		{
			if (keyNotFoundError && dict.ContainsKey(uniqueKey) == false)
			{
				Console.LogError($"The {nameof(uniqueKey)} '{uniqueKey}' was not found.");
			}
			else if (dict.ContainsKey(uniqueKey) == false) return;

			dict[uniqueKey] = value;
			values[keys.IndexOf(uniqueKey)] = value;
		}
		public int GetDataAmount()
		{
			return values == null ? 0 : values.Count;
		}
		public ValueT GetValueIn(UniqueKeyT uniqueKey, bool keyNotFoundError = true)
		{
			if (keyNotFoundError && dict.ContainsKey(uniqueKey) == false)
			{
				Console.LogError($"The {nameof(uniqueKey)} '{uniqueKey}' was not found.");
			}
			else if (dict.ContainsKey(uniqueKey) == false) return default;

			return dict[uniqueKey];
		}
		public ValueT GetValueAt(int index, bool indexNotFoundError = true)
		{
			if (indexNotFoundError && indexes.Contains(index) == false)
			{
				Console.LogError($"The {this}'s index '{index}' was not found.");
			}
			else if (indexes.Contains(index) == false) return default;

			return values[index];
		}
		public UniqueKeyT GetUniqueKeyAt(int index, bool indexNotFoundError = true)
		{
			if (indexNotFoundError && indexes.Contains(index) == false)
			{
				Console.LogError($"The {this}'s index '{index}' was not found.");
			}
			else if (indexes.Contains(index) == false) return default;

			return keys[index];
		}
		public int GetIndexIn(UniqueKeyT uniqueKey, bool keyNotFoundError = true)
		{
			if (keyNotFoundError && dict.ContainsKey(uniqueKey) == false)
			{
				Console.LogError($"The {nameof(uniqueKey)} '{uniqueKey}' was not found.");
			}
			else if (dict.ContainsKey(uniqueKey) == false) return default;

			return keys.IndexOf(uniqueKey);
		}
		public bool IndexExists(int index)
		{
			return indexes.Contains(index);
		}
		public bool UniqueKeyExists(UniqueKeyT uniqueKey)
		{
			return keys.Contains(uniqueKey);
		}
	}

	public struct Angle
	{
		private float a;

		public Angle(float a)
		{
			this.a = a;
			To360();
		}
		public float GetA()
		{
			return a;
		}
		public void SetA(float a)
		{
			this.a = a;
			To360();
		}
		public void SetFromDirection(Direction direction)
		{
			//Vector2 to Radians: atan2(Vector2.y, Vector2.x)
			//Radians to Angle: radians * (180 / Math.PI)
			if (direction != new Direction()) direction.Normalize();
			var rad = (double)Math.Atan2(direction.GetEndPoint().GetY(), direction.GetEndPoint().GetX());
			a = (float)(rad * (180 / Math.PI));
			To360();
		}
		public void SetFromBetweenPoints(Point point, Point targetPoint)
		{
			var dir = new Direction();
			dir.SetFromBetweenPoints(point, targetPoint);
			SetFromDirection(dir);
		}
		public void SetToRotationSample(RotationSamples sample)
		{
			switch (sample)
			{
				case RotationSamples.Up: this = new Angle(270); break;
				case RotationSamples.Left: this = new Angle(180); break;
				case RotationSamples.Right: this = new Angle(0); break;
				case RotationSamples.Down: this = new Angle(90); break;
				case RotationSamples.UpLeft: this = new Angle(225); break;
				case RotationSamples.UpRight: this = new Angle(315); break;
				case RotationSamples.DownLeft: this = new Angle(135); break;
				case RotationSamples.DownRight: this = new Angle(45); break;
			}
		}
		public void SetToPercentTowardsAngle(Angle targetAngle, float percent)
		{
			To360();
			targetAngle.To360();
			a = Number.PercentedTowardsTargetGet(a, targetAngle.GetA(), percent);
		}
		public void Rotate(float degreesPerSecond)
		{
			a = Number.ChangedGet(a, degreesPerSecond);
			To360();
		}
		public void RotateTowardsAngle(Angle targetAngle, float degreesPerSecond)
		{
			To360();
			targetAngle.To360();
			degreesPerSecond = Math.Abs(degreesPerSecond);
			var difference = a - targetAngle.GetA();

			// stops the rotation with an else when close enough
			// prevents the rotation from staying behind after the stop
			if (Math.Abs(difference) < degreesPerSecond * ticksDeltaTime) a = targetAngle.GetA();
			else if (difference > 0 && difference < 180) Rotate(-degreesPerSecond);
			else if (difference > -180 && difference < 0) Rotate(degreesPerSecond);
			else if (difference > -360 && difference < -180) Rotate(-degreesPerSecond);
			else if (difference > 180 && difference < 360) Rotate(degreesPerSecond);

			// detects speed greater than possible
			// prevents jiggle when passing 0-360 & 360-0 | simple to fix yet took me half a day
			if (Math.Abs(difference) > 360 - degreesPerSecond * ticksDeltaTime) a = targetAngle.GetA();
		}

		public override string ToString()
		{
			return $"{nameof(Angle)}[a:{a:F2}]";
		}
		/// <summary>
		/// This default <see cref="object"/> method is not implemented.
		/// </summary>
		public override bool Equals(object obj)
		{
			return default;
		}
		/// <summary>
		/// This default <see cref="object"/> method is not implemented.
		/// </summary>
		public override int GetHashCode()
		{
			return default;
		}

		private void To360()
		{
			a = ((a % 360) + 360) % 360;
		}
	}
	public struct Size
	{
		private float w;
		private float h;

		public Size(float w, float h)
		{
			this.w = w;
			this.h = h;
		}
		public void SetWH(float w, float h)
		{
			this.w = w;
			this.h = h;
		}
		public void SetW(float w)
		{
			this.w = w;
		}
		public void SetH(float h)
		{
			this.h = h;
		}
		public float GetW()
		{
			return w;
		}
		public float GetH()
		{
			return h;
		}
		public void Scale(float pixelsPerSecond)
		{
			pixelsPerSecond *= ticksDeltaTime;
			w += pixelsPerSecond;
			h += pixelsPerSecond;
		}
		public void ScaleTowardsTarget(Size targetSize, float pixelsPerSecond)
		{
			Scale(pixelsPerSecond);
			var dist = Vector2.Distance(new Vector2(GetW(), GetH()), new Vector2(targetSize.GetW(), targetSize.GetH()));
			if (dist < pixelsPerSecond * ticksDeltaTime * 2)
			{
				w = targetSize.GetW();
				h = targetSize.GetH();
			}
		}

		public override string ToString()
		{
			return $"{nameof(Size)}[w:{w:F2}][h:{h:F2}]";
		}
		/// <summary>
		/// This default <see cref="object"/> method is not implemented.
		/// </summary>
		public override bool Equals(object obj)
		{
			return default;
		}
		/// <summary>
		/// This default <see cref="object"/> method is not implemented.
		/// </summary>
		public override int GetHashCode()
		{
			return default;
		}

		public static Size operator +(Size a, Size b)
		{
			return new Size(a.GetW() + b.GetW(), a.GetH() + b.GetH());
		}
		public static Size operator -(Size a, Size b)
		{
			return new Size(a.GetW() - b.GetW(), a.GetH() - b.GetH());
		}
		public static Size operator *(Size a, Size b)
		{
			return new Size(a.GetW() * b.GetW(), a.GetH() * b.GetH());
		}
		public static Size operator *(Size a, float b)
		{
			return new Size(a.GetW() * b, a.GetH() * b);
		}
		public static Size operator /(Size a, Size b)
		{
			return new Size(a.GetW() / b.GetW(), a.GetH() / b.GetH());
		}
		public static Size operator /(Size a, float b)
		{
			return new Size(a.GetW() / b, a.GetH() / b);
		}
	}
	public struct Point
	{
		float x;
		float y;

		public Point(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
		public void SetXY(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
		public float GetX()
		{
			return x;
		}
		public float GetY()
		{
			return y;
		}
		public void SetX(float x)
		{
			this.x = x;
		}
		public void SetY(float y)
		{
			this.y = y;
		}
		public float GetDistanceToPoint(Point point)
		{
			return Vector2.Distance(new Vector2(x, y), new Vector2(point.GetX(), point.GetY()));
		}
		public void MoveInDirection(Direction direction, float pixelsPerSecond)
		{
			pixelsPerSecond *= ticksDeltaTime;
			direction.Normalize();
			x += direction.GetEndPoint().GetX() * pixelsPerSecond;
			y += direction.GetEndPoint().GetY() * pixelsPerSecond;
		}
		public void MoveAtAngle(Angle angle, float pixelsPerSecond)
		{
			var dir = new Direction();
			dir.SetFromAngle(angle);
			MoveInDirection(dir, pixelsPerSecond);
		}
		public void MoveTowardsPoint(Point targetPoint, float pixelsPerSecond)
		{
			var dir = new Direction(targetPoint - this);
			MoveInDirection(dir, pixelsPerSecond);
			var dist = Vector2.Distance(new Vector2(x, y), new Vector2(targetPoint.GetX(), targetPoint.GetY()));
			if (dist < pixelsPerSecond * ticksDeltaTime * 2)
			{
				x = targetPoint.GetX();
				y = targetPoint.GetY();
			}
		}
		public void SetToPercentTowardsPoint(Point targetPoint, float percent)
		{
			var vec = Vector2.Lerp(new Vector2(GetX(), GetY()), new Vector2(targetPoint.GetX(), targetPoint.GetY()), (float)percent / 100);
			x = vec.X;
			y = vec.Y;
		}

		public override string ToString()
		{
			return $"{nameof(Point)}[x:{GetX():F2}][y:{GetY():F2}]";
		}
		/// <summary>
		/// This default <see cref="object"/> method is not implemented.
		/// </summary>
		public override bool Equals(object obj)
		{
			return default;
		}
		/// <summary>
		/// This default <see cref="object"/> method is not implemented.
		/// </summary>
		public override int GetHashCode()
		{
			return default;
		}

		public static Point operator +(Point a, Point b)
		{
			return new Point(a.GetX() + b.GetX(), a.GetY() + b.GetY());
		}
		public static Point operator -(Point a, Point b)
		{
			return new Point(a.GetX() - b.GetX(), a.GetY() - b.GetY());
		}
		public static Point operator *(Point a, Point b)
		{
			return new Point(a.GetX() * b.GetX(), a.GetY() * b.GetY());
		}
		public static Point operator /(Point a, Point b)
		{
			return new Point(a.GetX() / b.GetX(), a.GetY() / b.GetY());
		}
		public static Point operator /(Point a, float b)
		{
			return new Point(a.GetX() / b, a.GetY() / b);
		}
		public static Point operator *(Point a, float b)
		{
			return new Point(a.GetX() * b, a.GetY() * b);
		}
		public static bool operator ==(Point a, Point b)
		{
			return a.GetX() == b.GetX() && a.GetY() == b.GetY();
		}
		public static bool operator !=(Point a, Point b)
		{
			return a.GetX() != b.GetX() && a.GetY() != b.GetY();
		}
	}
	public struct Direction
	{
		private Point endPoint;

		public Direction(Point endPoint)
		{
			this.endPoint = endPoint; Normalize();
		}
		public Point GetEndPoint()
		{
			return endPoint;
		}
		public void Normalize()
		{
			var vec = new Vector2(endPoint.GetX(), endPoint.GetY());
			if (vec != Vector2.Zero) vec.Normalize();
			endPoint = new Point(vec.X, vec.Y);
		}
		public void Reverse()
		{
			endPoint = new Point(-endPoint.GetX(), -endPoint.GetY()); 
			Normalize();
		}
		public void ReverseHorizontally()
		{
			endPoint = new Point(-endPoint.GetX(), endPoint.GetY()); 
			Normalize();
		}
		public void ReverseVertically()
		{
			endPoint = new Point(endPoint.GetX(), -endPoint.GetY()); 
			Normalize();
		}
		public void Set(Point endPoint)
		{
			this.endPoint = endPoint;
			Normalize();
		}
		public void SetFromAngle(Angle angle)
		{
			//Angle to Radians : (Math.PI / 180) * angle
			//Radians to Vector2 : Vector2.x = cos(angle) | Vector2.y = sin(angle)

			var rad = Math.PI / 180 * angle.GetA();
			var dir = new Vector2((float)Math.Cos(rad), (float)Math.Sin(rad));
			dir.Normalize();
			endPoint = new Point(dir.X, dir.Y);
		}
		public void SetFromBetweenPoints(Point point, Point targetPoint)
		{
			endPoint = targetPoint - point;
			Normalize();
		}
		public void SetToRotationSample(RotationSamples direction)
		{
			switch (direction)
			{
				case RotationSamples.Up: this = new Direction(new Point(0, -1)); break;
				case RotationSamples.Left: this = new Direction(new Point(-1, 0)); break;
				case RotationSamples.Right: this = new Direction(new Point(1, 0)); break;
				case RotationSamples.Down: this = new Direction(new Point(0, 1)); break;
				case RotationSamples.UpLeft: this = new Direction(new Point(-1, -1)); break;
				case RotationSamples.UpRight: this = new Direction(new Point(1, -1)); break;
				case RotationSamples.DownLeft: this = new Direction(new Point(-1, 1)); break;
				case RotationSamples.DownRight: this = new Direction(new Point(1, 1)); break;
			}
			Normalize();
		}
		public void SetToPercentTowardsDirection(Direction targetDirection, float percent)
		{
			Normalize();
			targetDirection.Normalize();
			var angle = new Angle();
			var targetAngle = new Angle();
			angle.SetFromDirection(this);
			targetAngle.SetFromDirection(targetDirection);
			angle.SetToPercentTowardsAngle(targetAngle, percent);
			SetFromAngle(angle);
		}
		public void Rotate(float degreesPerSecond)
		{
			Normalize();
			var angle = new Angle();
			angle.SetFromDirection(this);
			angle.Rotate(degreesPerSecond);
			SetFromAngle(angle);
		}
		public void RotateTowardsDirection(Direction targetDirection, float degreesPerSecond)
		{
			Normalize();
			targetDirection.Normalize();
			var angle = new Angle();
			var targetAngle = new Angle();
			angle.SetFromDirection(this);
			targetAngle.SetFromDirection(targetDirection);
			angle.RotateTowardsAngle(targetAngle, degreesPerSecond);
			SetFromAngle(angle);
		}

		public static Direction operator +(Direction a, Direction b)
		{
			return new Direction(a.endPoint + b.endPoint);
		}
		public static Direction operator -(Direction a, Direction b)
		{
			return new Direction(a.endPoint - b.endPoint);
		}
		public static Direction operator *(Direction a, Direction b)
		{
			return new Direction(a.endPoint * b.endPoint);
		}
		public static Direction operator /(Direction a, Direction b)
		{
			return new Direction(a.endPoint / b.endPoint);
		}
		public static bool operator ==(Direction a, Direction b)
		{
			return a.endPoint == b.endPoint;
		}
		public static bool operator !=(Direction a, Direction b)
		{
			return a.endPoint != b.endPoint;
		}

		public override string ToString()
		{
			return $"{nameof(Direction)}[endpoint:{endPoint}]";
		}
		/// <summary>
		/// A default <see cref="object"/> method. Not implemented.
		/// </summary>
		public override bool Equals(object obj)
		{
			return default;
		}
		/// <summary>
		/// A default <see cref="object"/> method. Not implemented.
		/// </summary>
		public override int GetHashCode()
		{
			return default;
		}
	}
	public struct Color
	{
		private float r, g, b, o;

		public Color(float r, float g, float b, float o = 255)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.o = o;
			To255();
		}
		public void Set(float r, float g, float b, float o = 255)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.o = o;
			To255();
		}
		public void Lighten(float shadesPerSecond)
		{
			shadesPerSecond *= ticksDeltaTime;
			r += shadesPerSecond;
			g += shadesPerSecond;
			b += shadesPerSecond;
			To255();
		}
		public void TintR(float shadesPerSecond)
		{
			r += shadesPerSecond * ticksDeltaTime;
			To255();
		}
		public void TintG(float shadesPerSecond)
		{
			g += shadesPerSecond * ticksDeltaTime;
			To255();
		}
		public void TintB(float shadesPerSecond)
		{
			b += shadesPerSecond * ticksDeltaTime;
			To255();
		}
		public void Appear(float shadesPerSecond)
		{
			o += shadesPerSecond * ticksDeltaTime;
			To255();
		}
		public void TintTowardsR(float targetRed, float shadesPerSecond)
		{
			TintR(r < targetRed ? shadesPerSecond : -shadesPerSecond);
			var dist = Math.Abs(r - targetRed);
			if (dist < shadesPerSecond * ticksDeltaTime * 2) r = targetRed;
		}
		public void TintTowardsG(float targetGreen, float shadesPerSecond)
		{
			TintG(r < targetGreen ? shadesPerSecond : -shadesPerSecond);
			var dist = Math.Abs(g - targetGreen);
			if (dist < shadesPerSecond * ticksDeltaTime * 2) g = targetGreen;
		}
		public void TintTowardsB(float targetBlue, float shadesPerSecond)
		{
			TintB(b < targetBlue ? shadesPerSecond : -shadesPerSecond);
			var dist = Math.Abs(b - targetBlue);
			if (dist < shadesPerSecond * ticksDeltaTime * 2) b = targetBlue;
		}
		public void AppearTowardsO(float targeto, float shadesPerSecond)
		{
			Appear(o < targeto ? shadesPerSecond : -shadesPerSecond);
			var dist = Math.Abs(o - targeto);
			if (dist < shadesPerSecond * ticksDeltaTime * 2) o = targeto;
		}
		public void TintTowardsColor(Color targetColor, float shadesPerSecond)
		{
			TintR(targetColor.r > r ? shadesPerSecond : -shadesPerSecond);
			TintG(targetColor.g > g ? shadesPerSecond : -shadesPerSecond);
			TintB(targetColor.b > b ? shadesPerSecond : -shadesPerSecond);

			var rDist = Math.Abs(targetColor.r - r);
			var gDist = Math.Abs(targetColor.g - g);
			var bDist = Math.Abs(targetColor.b - b);

			shadesPerSecond *= ticksDeltaTime;
			if (rDist < shadesPerSecond * 2) r = targetColor.r;
			if (gDist < shadesPerSecond * 2) g = targetColor.g;
			if (bDist < shadesPerSecond * 2) b = targetColor.b;

			To255();
		}
		public float GetR()
		{
			return r;
		}
		public float GetG()
		{
			return g;
		}
		public float GetB()
		{
			return b;
		}
		public float GetO()
		{
			return o;
		}

		public override string ToString()
		{
			return $"{nameof(Color)}[r:{r:F2}][g:{g:F2}][b:{b:F2}][o:{o:F2}]";
		}
		/// <summary>
		/// This default <see cref="object"/> method is not implemented.
		/// </summary>
		public override bool Equals(object obj)
		{
			return default;
		}
		/// <summary>
		/// This default <see cref="object"/> method is not implemented.
		/// </summary>
		public override int GetHashCode()
		{
			return default;
		}

		private void To255()
		{
			r = Number.LimitedGet(r, 0, 255);
			g = Number.LimitedGet(g, 0, 255);
			b = Number.LimitedGet(b, 0, 255);
			o = Number.LimitedGet(o, 0, 255);
		}

		public static Color operator +(Color a, Color b)
		{
			return new Color((a.r + b.r), (a.g + b.g), (a.b + b.b));
		}
		public static Color operator -(Color a, Color b)
		{
			return new Color((a.r - b.r), (a.g - b.g), (a.b - b.b));
		}
	}
	public struct Circle
	{
		private Point position;
		private float radius;

		public Circle(Point position, float radius)
		{
			radius = Number.LimitedGet(radius, 2, 1_000_000);
			this.position = position;
			this.radius = radius;
		}
		public void Set(Point position, float radius)
		{
			radius = Number.LimitedGet(radius, 2, 1_000_000);
			this.position = position;
			this.radius = radius;
		}

		public Point GetPosition() => position;
		public float GetRadius() => radius;

		public List<Point> GetCrossPointsWithLine(Line line)
		{
			return GetLineCircleCrossPoints(position, radius, line.GetStartPoint(), line.GetEndPoint());
		}
		public bool IsCrossedByLine(Line line)
		{
			return GetLineCircleCrossPoints(position, radius, line.GetStartPoint(), line.GetEndPoint()).Count > 0;
		}
	}
	public struct Line
	{
		private Point pointStart;
		private Point pointEnd;

		public Line(Point pointStart, Point pointEnd)
		{
			this.pointStart = pointStart;
			this.pointEnd = pointEnd;
		}
		public void Set(Point pointStart, Point pointEnd)
		{
			this.pointStart = pointStart;
			this.pointEnd = pointEnd;
		}

		public Point GetStartPoint() => pointStart;
		public Point GetEndPoint() => pointEnd;
		public float GetLength() => pointStart.GetDistanceToPoint(pointEnd);

		public List<Point> GetCrossPointsWithCircle(Circle circle)
		{
			return circle.GetCrossPointsWithLine(this);
		}
		public bool IsCrossingCircle(Circle circle)
		{
			return circle.IsCrossedByLine(this);
		}
		public List<Point> GetCrossPointWithLine(Line line)
		{
			var segmentsCross = false;
			var linesCross = false;
			var intersection = new List<Point>();
			var closestCrossPointToMe = new Point();
			var closestCrossPointToLine = new Point();

			GetCrossPointOfTwoLines(pointStart, pointEnd, line.pointStart, line.pointEnd, out linesCross, out segmentsCross, out intersection, out closestCrossPointToMe, out closestCrossPointToLine);
			return intersection;
		}
		public bool IsCrossingLine(Line line)
		{
			return LineCrossesLine(pointStart, pointEnd, line.pointStart, line.pointEnd);
		}
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
			var disconnectedClient = clientIDs[Id.ToString()];
			clientUniqueNames.Remove(disconnectedClient);
			server.Multicast($"~{(int)MessageType.ClientDisconnected}|{disconnectedClient}");
			consoleLog = $"{consoleLog}\nClient [{disconnectedClient}] just disconnected.";
			ConsoleUpdate();
		}
		protected override void OnReceived(byte[] buffer, long offset, long size)
		{
			var rawMessages = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
			var messages = rawMessages.Split('~', StringSplitOptions.RemoveEmptyEntries);
			var messageBack = "";
			foreach (var message in messages)
			{
				var components = message.Split('|');
				var messageType = (MessageType)int.Parse(components[0]);
				switch (messageType)
				{
					case MessageType.Connection: // A client just connected and sent his ID & unique name
						{
							var id = components[1];
							var uniqueName = components[2];
							if (clientUniqueNames.Contains(uniqueName)) // Is the unique name free?
							{
								uniqueName = ChangeUniqueName(uniqueName);
								messageBack = $"~{(int)MessageType.UniqueNameChange}|{id}|{uniqueName}"; // Send a message back with a free one towards the same ID so the client can recognize it's for him
							}
							clientIDs[Id.ToString()] = uniqueName;
							clientUniqueNames.Add(uniqueName);
							messageBack = $"{messageBack}~{(int)MessageType.ClientOnline}|{uniqueName}"; // Sticking another message to update the newcoming client about online clients
							foreach (var client in clientUniqueNames)
							{
								messageBack = $"{messageBack}|{client}";
							}
							messageBack = $"{messageBack}~{(int)MessageType.ClientConnected}|{uniqueName}"; // Sticking a third message to update online clients about the newcomer.
							consoleLog = $"{consoleLog}\nClient [{uniqueName}] just connected.";
							ConsoleUpdate();
							break;
						}
					case MessageType.ClientMessageToAll: // A client wants to send a message to everyone
						{
							messageBack = $"{messageBack}~{message}";
							break;
						}
					case MessageType.ClientMessageToClient: // A client wants to send a message to another client
						{
							messageBack = $"{messageBack}~{message}";
							break;
						}
					case MessageType.ClientMessageToServer: // A client sent me (the server) a message
						{
							program.NetworkMessageJustReceived(components[1], components[2]);
							if (networkLogMessagesToConsole) consoleLog = $"{consoleLog}\nMessage received from Client [{components[1]}]: {components[2]}";
							break;
						}
					case MessageType.ClientMessageToAllAndServer: // A client is sending me (the server) and all other clients a message
						{
							program.NetworkMessageJustReceived(components[1], components[2]);
							if (networkLogMessagesToConsole) consoleLog = $"{consoleLog}\nMessage received from Client [{components[1]}]: {components[2]}";
							messageBack = $"{messageBack}~{message}";
							break;
						}
				}
			}
			if (networkLogMessagesToConsole) ConsoleUpdate();
			if (messageBack != "") server.Multicast(messageBack);
		}
		protected override void OnError(SocketError error)
		{
			consoleLog = $"{consoleLog}\nServer Error: {error}";
			ConsoleUpdate();
		}
		private string ChangeUniqueName(string uniqueName)
		{
			var i = 0;
			while (true)
			{
				i++;
				if (clientUniqueNames.Contains($"{uniqueName}{i}") == false)
				{
					break;
				}
			}
			return $"{uniqueName}{i}";
		}
	}
	private class Server : TcpServer
	{
		public Server(IPAddress address, int port) : base(address, port) { }
		protected override TcpSession CreateSession() { return new Session(this); }
		protected override void OnError(SocketError error)
		{
			serverIsRunning = false;
			consoleLog = $"{consoleLog}\nServer Error: {error}";
			ConsoleUpdate();
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
			clientIsConnected = true;
			clientUniqueNames.Add(clientUniqueName);
			consoleLog = $"{consoleLog}\nConnected as [{clientUniqueName}] to {client.Socket.RemoteEndPoint}.";
			ConsoleUpdate();
			client.SendAsync($"~{(int)MessageType.Connection}|{client.Id}|{clientUniqueName}");
		}
		protected override void OnDisconnected()
		{
			if (clientIsConnected)
			{
				clientIsConnected = false;
				consoleLog = $"Disconnected.";
				clientUniqueNames.Clear();
			}

			// Wait for a while...
			Thread.Sleep(1000);

			// Try to connect again
			consoleLog = $"{consoleLog}\nTrying to reconnect...";
			if (stop == false) ConnectAsync();
			ConsoleUpdate();
		}
		protected override void OnReceived(byte[] buffer, long offset, long size)
		{
			var rawMessages = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
			var messages = rawMessages.Split('~', StringSplitOptions.RemoveEmptyEntries);
			var messageBack = "";
			foreach (var message in messages)
			{
				var components = message.Split('|');
				var messageType = (MessageType)int.Parse(components[0]);
				switch (messageType)
				{
					case MessageType.UniqueNameChange: // Server said someone's unique name is taken and sent a free one
						{
							if (components[1] == client.Id.ToString()) // Is this for me?
							{
								consoleLog = $"{consoleLog}\nMy Unique Name [{clientUniqueName}] is taken so my new Unique Name is [{components[2]}].";
								clientUniqueNames.Remove(clientUniqueName);
								clientUniqueName = components[2];
								clientUniqueNames.Add(clientUniqueName);
								ConsoleUpdate();
							}
							break;
						}
					case MessageType.ClientConnected: // Server said some client connected
						{
							if (components[1] != clientUniqueName) // If not me
							{
								clientUniqueNames.Add(components[1]);
								consoleLog = $"{consoleLog}\nClient [{components[1]}] just connected.";
								ConsoleUpdate();
							}
							break;
						}
					case MessageType.ClientDisconnected: // Server said some client disconnected
						{
							clientUniqueNames.Remove(components[1]);
							consoleLog = $"{consoleLog}\nClient [{components[1]}] just disconnected.";
							break;
						}
					case MessageType.ClientOnline: // Someone just connected and is getting updated on who is already online
						{
							if (components[1] == clientUniqueName) // For me?
								for (int i = 2; i < components.Length; i++)
									if (clientUniqueNames.Contains(components[i]) == false)
										clientUniqueNames.Add(components[i]);
							ConsoleUpdate();
							break;
						}
					case MessageType.ClientMessageToAll: // A client is sending a message to all clients
						{
							if (components[1] == clientUniqueName) break; // Is this my message coming back to me?
							program.NetworkMessageJustReceived(components[1], components[2]);
							if (networkLogMessagesToConsole) consoleLog = $"{consoleLog}\nMessage received from Client [{components[1]}]: {components[2]}";
							break;
						}
					case MessageType.ClientMessageToAllAndServer: // A client is sending a message to the server and all clients
						{
							if (components[1] == clientUniqueName) break; // Is this my message coming back to me?
							program.NetworkMessageJustReceived(components[1], components[2]);
							if (networkLogMessagesToConsole) consoleLog = $"{consoleLog}\nMessage received from Client [{components[1]}]: {components[2]}";
							break;
						}
					case MessageType.ClientMessageToClient: // A client is sending a message to another client
						{
							if (components[1] == clientUniqueName) return; // Is this my message coming back to me? (unlikely)
							if (components[2] != clientUniqueName) return; // Not for me?

							program.NetworkMessageJustReceived(components[1], components[3]);
							if (networkLogMessagesToConsole) consoleLog = $"{consoleLog}\nMessage received from Client [{components[1]}]: {components[3]}";
							break;
						}
					case MessageType.ServerMessageToAll: // The server sent everyone a message
						{
							program.NetworkMessageJustReceived(null, components[1]);
							if (networkLogMessagesToConsole) consoleLog = $"{consoleLog}\nMessage received from Server: {components[1]}";
							break;
						}
					case MessageType.ServerMessageToClient: // The server sent some client a message
						{
							if (components[1] != clientUniqueName) return; // Not for me?

							program.NetworkMessageJustReceived(null, components[1]);
							if (networkLogMessagesToConsole) consoleLog = $"{consoleLog}\nMessage received from Server: {components[1]}";
							break;
						}
				}
			}
			if (networkLogMessagesToConsole) ConsoleUpdate();
			if (messageBack != "") client.SendAsync(messageBack);
		}
		protected override void OnError(SocketError error)
		{
			clientIsConnected = false;
			consoleLog = $"{consoleLog}\nClient Error: {error}";
			ConsoleUpdate();
		}
	}

	private static string ClientsOnlineGet()
	{
		var result = "";
		for (int i = 0; i < clientUniqueNames.Count; i++)
		{
			var separator = i == clientUniqueNames.Count - 1 ? "" : ", ";
			result = $"{result}[{clientUniqueNames[i]}]{separator}";
		}
		return result;
	}
	private static void ConsoleUpdate()
	{
		if (consoleShown == false) return;
		System.Console.Clear();
		var clientsConnected = serverIsRunning || clientIsConnected ? $"Clients Connected ({clientUniqueNames.Count}): {ClientsOnlineGet()}\n\n" : "";
		var connectInfo = serverIsRunning || clientIsConnected ? connectToServerInfo + "\n\n" : "";

		System.Console.Title = $"Console | {Window.GetTitle()}";
		System.Console.WriteLine($"{connectInfo}{clientsConnected}{consoleLog}");
	}
	private static void DrawTile(Texture2D texture, Point position, Point tileIndex, int gridSize, Size size, Point origin, Size scale, Color color, float angle, SpriteEffects spriteEffects)
	{
		var textureStartPosition = new Point(
			tileIndex.GetX() * size.GetW() + (gridSize * tileIndex.GetX()),
			tileIndex.GetY() * size.GetH() + (gridSize * tileIndex.GetY()));

		spriteBatch.Draw(
			texture,
			new Vector2(position.GetX(), position.GetY()),
			new Rectangle((int)textureStartPosition.GetX(),
			(int)textureStartPosition.GetY(),
			(int)size.GetW(),
			(int)size.GetH()),
			new Microsoft.Xna.Framework.Color((int)color.GetR(), (int)color.GetG(), (int)color.GetB(), (int)color.GetO()),
			(float)Math.PI / 180 * angle, new Vector2(origin.GetX(), origin.GetY()),
			new Vector2(scale.GetW(), scale.GetH()),
			spriteEffects,
			0);
	}
	private static bool RectangleContainsPoint(Vector2 rectA, Vector2 rectB, Vector2 rectC, Vector2 point)
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
	private static bool LineCrossesLine(Point startA, Point endA, Point startB, Point endB)
	{
		return ccw(startA, startB, endB) != ccw(endA, startB, endB) && ccw(startA, endA, startB) != ccw(startA, endA, endB);
		
		static bool ccw(Point a, Point b, Point c) => (c.GetY() - a.GetY()) * (b.GetX() - a.GetX()) > (b.GetY() - a.GetY()) * (c.GetX() - a.GetX());
	}
	// Find the point of intersection between
	// the lines p1 --> p2 and p3 --> p4.
	private static void GetCrossPointOfTwoLines(Point startA, Point endA, Point startB, Point endB,
		 out bool lines_intersect, out bool segments_intersect,
		 out List<Point> intersection,
		 out Point close_p1, out Point close_p2)
	{
		var lineLength = startA.GetDistanceToPoint(endA);
		intersection = new List<Point>();

		// Get the segments' parameters.
		float dx12 = endA.GetX() - startA.GetX();
		float dy12 = endA.GetY() - startA.GetY();
		float dx34 = endB.GetX() - startB.GetX();
		float dy34 = endB.GetY() - startB.GetY();

		// Solve for t1 and t2
		float denominator = (dy12 * dx34 - dx12 * dy34);

		float t1 = ((startA.GetX() - startB.GetX()) * dy34 + (startB.GetY() - startA.GetY()) * dx34) / denominator;
		if (float.IsInfinity(t1))
		{
			// The lines are parallel (or close enough to it).
			lines_intersect = false;
			segments_intersect = false;
			close_p1 = new Point(float.NaN, float.NaN);
			close_p2 = new Point(float.NaN, float.NaN);
			return;
		}
		lines_intersect = true;

		float t2 = ((startB.GetX() - startA.GetX()) * dy12 + (startA.GetY() - startB.GetY()) * dx12) / -denominator;

		// Find the point of intersection.
		var point = new Point(startA.GetX() + dx12 * t1, startA.GetY() + dy12 * t1);
		if (point.GetDistanceToPoint(startA) <= lineLength) intersection.Add(point);

		// The segments intersect if t1 and t2 are between 0 and 1.
		segments_intersect = ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));

		// Find the closest points on the segments.
		if (t1 < 0) t1 = 0;
		else if (t1 > 1) t1 = 1;

		if (t2 < 0) t2 = 0;
		else if (t2 > 1) t2 = 1;

		close_p1 = new Point(startA.GetX() + dx12 * t1, startA.GetY() + dy12 * t1);
		close_p2 = new Point(startB.GetX() + dx34 * t2, startB.GetY() + dy34 * t2);
	}
	private static List<Point> GetLineCircleCrossPoints(Point circlePosition, float circleRadius, Point pointA, Point pointB)
	{
		var result = new List<Point>();
		var t = 0f;
		var dx = pointB.GetX() - pointA.GetX();
		var dy = pointB.GetY() - pointA.GetY();
		var cx = circlePosition.GetX();
		var cy = circlePosition.GetY();
		var r = circleRadius;
		var A = dx * dx + dy * dy;
		var B = 2 * (dx * (pointA.GetX() - cx) + dy * (pointA.GetY() - cy));
		var C = (pointA.GetX() - cx) * (pointA.GetX() - cx) + (pointA.GetY() - cy) * (pointA.GetY() - cy) - r * r;
		var det = B * B - 4 * A * C;
		var lineLength = pointA.GetDistanceToPoint(pointB);

		if ((A <= 0.0000001) || (det < 0))
		{
			// no real solutions
			return result;
		}
		else if (det == 0)
		{
			// one solution
			t = -B / (2 * A);
			var point = new Point(pointA.GetX() + t * dx, pointA.GetY() + t * dy);
			if (point.GetDistanceToPoint(pointA) >= lineLength) result.Add(point);
		}
		else
		{
			// two solutions
			t = (float)((-B + Math.Sqrt(det)) / (2 * A));
			var point1 = new Point(pointA.GetX() + t * dx, pointA.GetY() + t * dy);
			if (point1.GetDistanceToPoint(pointA) <= lineLength) result.Add(point1);

			t = (float)((-B - Math.Sqrt(det)) / (2 * A));
			var point2 = new Point(pointA.GetX() + t * dx, pointA.GetY() + t * dy);
			if (point2.GetDistanceToPoint(pointA) <= lineLength) result.Add(point2);
		}
		return result;
	}
}