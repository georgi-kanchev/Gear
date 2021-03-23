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
using System.Runtime.CompilerServices;
using System.ComponentModel;

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
	public enum Key
	{
		None = 0, BackSpace = 8, Tab = 9, Enter = 13, Pause = 19, CapsLock = 20, Kana = 21, Kanji = 25, Escape = 27, ImeConvert = 28, ImeNoConvert = 29, Space = 32, PageUp = 33, PageDown = 34, End = 35, Home = 36, LeftArrow = 37, UpArrow = 38, RightArrow = 39, DownArrow = 40, Select = 41, Print = 42, Execute = 43, PrintScreen = 44, Insert = 45, Delete = 46, Help = 47, _0 = 48, _1 = 49, _2 = 50, _3 = 51, _4 = 52, _5 = 53, _6 = 54, _7 = 55, _8 = 56, _9 = 57, A = 65, B = 66, C = 67, D = 68, E = 69, F = 70, G = 71, H = 72, I = 73, J = 74, K = 75, L = 76, M = 77, N = 78, O = 79, P = 80, Q = 81, R = 82, S = 83, T = 84, U = 85, V = 86, W = 87, X = 88, Y = 89, Z = 90, LeftWindows = 91, RightWindows = 92, Apps = 93, Sleep = 95, Num0 = 96, Num1 = 97, Num2 = 98, Num3 = 99, Num4 = 100, Num5 = 101, Num6 = 102, Num7 = 103, Num8 = 104, Num9 = 105, NumMultiply = 106, NumAdd = 107, Separator = 108, NumSubtract = 109, NumDecimal = 110, NumDivide = 111, F1 = 112, F2 = 113, F3 = 114, F4 = 115, F5 = 116, F6 = 117, F7 = 118, F8 = 119, F9 = 120, F10 = 121, F11 = 122, F12 = 123, F13 = 124, F14 = 125, F15 = 126, F16 = 127, F17 = 128, F18 = 129, F19 = 130, F20 = 131, F21 = 132, F22 = 133, F23 = 134, F24 = 135, NumLock = 144, Scroll = 145, ShiftLeft = 160, ShiftRight = 161, ControlLeft = 162, ControlRight = 163, AltLeft = 164, AltRight = 165, BrowserBack = 166, BrowserForward = 167, BrowserRefresh = 168, BrowserStop = 169, BrowserSearch = 170, BrowserFavorites = 171, BrowserHome = 172, VolumeMute = 173, VolumeDown = 174, VolumeUp = 175, MediaNextTrack = 176, MediaPreviousTrack = 177, MediaStop = 178, MediaPlayPause = 179, LaunchMail = 180, SelectMedia = 181, LaunchApplication1 = 182, LaunchApplication2 = 183, Semicolon = 186, Equals = 187, Comma = 188, MinusDash = 189, Dot = 190, Slash = 191, GraveAccent = 192, ChatPadGreen = 202, ChatPadOrange = 203, SquareBracketOpen = 219, Backslash = 220, SquareBracketClose = 221, Quote = 222, Oem8 = 223, OemBackslash = 226, ProcessKey = 229, OemCopy = 242, OemAuto = 243, OemEnlW = 244, Attn = 246, Crsel = 247, Exsel = 248, EraseEof = 249, Play = 250, Zoom = 251, Pa1 = 253, OemClear = 254
	}
	public enum RoundNumberToward
	{
		Closest, Up, Down
	}
	public enum RoundNumberPrio
	{
		TowardEven, AwayFromZero, TowardZero, TowardNegativeInfinity, TowardPositiveInfinity
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
	public enum PopUpIcon
	{
		None, Info, Error, Warning
	}
	public enum Motion
	{
		PerSecond, PerTick
	}
	public enum MouseHitboxInteraction
	{
		Hovered, Unhovered, Clicked, ClickReleased, Released
	}
	public enum Interaction
	{
		Pressed, Released
	}
	public enum MouseButton
	{
		Left, Middle, Right
	}

	private static PerformanceCounter ramAvailable = new PerformanceCounter("Memory", "Available MBytes");
	private static PerformanceCounter ramUsedPercent = new PerformanceCounter("Memory", "% Committed Bytes In Use");

	private static Dictionary<string, SpriteFont> fonts = new Dictionary<string, SpriteFont>();
	private static Dictionary<string, Texture2D> sprites = new Dictionary<string, Texture2D>();//, spriteOutlines = new Dictionary<string, Texture2D>(), spriteFills = new Dictionary<string, Texture2D>();
	private static Dictionary<string, SoundEffectInstance> sounds = new Dictionary<string, SoundEffectInstance>();
	private static Dictionary<string, SoundEffect> soundsRaw = new Dictionary<string, SoundEffect>();
	private static Dictionary<string, Song> melodies = new Dictionary<string, Song>();
	private static Dictionary<Song, string> melodyUniqueNames = new Dictionary<Song, string>();
	private static Dictionary<string, bool> gates = new Dictionary<string, bool>();
	private static Dictionary<string, int> gateEntriesCount = new Dictionary<string, int>(), timerRepeats = new Dictionary<string, int>();
	private static Dictionary<string, string> clientIDs = new Dictionary<string, string>();
	private static Dictionary<string, List<Body>> tagBodies = new Dictionary<string, List<Body>>();
	private static Dictionary<string, float> signalEndTimes = new Dictionary<string, float>(), signalStartTimes = new Dictionary<string, float>(), signalDelays = new Dictionary<string, float>(), timerTickSeconds = new Dictionary<string, float>();
	private static Dictionary<Body, float> bodyCameraDistances = new Dictionary<Body, float>(), bodyCameraAngle = new Dictionary<Body, float>(), bodyCameraAngleDifferences = new Dictionary<Body, float>();
	private static Dictionary<string, List<string>> soundCollections = new Dictionary<string, List<string>>();
	private static Dictionary<Body, bool> bodiesLastTickHovered = new Dictionary<Body, bool>(), bodiesClicked = new Dictionary<Body, bool>();

	private static bool[] mouseButtonsLastFramePressed = new bool[3];
	private static List<Key> keysPressed = new List<Key>(), lastFrameKeysPressed = new List<Key>();
	private static List<Body> bodiesAll = new List<Body>();
	private static List<float> tpsAverages = new List<float>(), fpsAverages = new List<float>();
	private static List<string> clientUniqueNames = new List<string>();
	private static int tick, frame, frameRendered, tpsAverageIndex, fpsAverageIndex, loadingPercent, loadingScreenUpdatePerFiles = 10, loadedFiles, contentFileCount, serverPort = 1234, eachTickLineCall;
	private static bool textDisplayDraw, loading = true, pauseUnfocus, render, sleepPrevented, consoleShown, clientIsConnected, serverIsRunning, networkLogMessagesToConsole, windowIsDisplayed = true, muteMelody, muteSound;
	private static float textDisplayScale, tps, tpsAverage, fps, fpsAverage, ticksDeltaTime, framesDeltaTime, time, cameraAngle;
	private static string textDisplayFont, textDisplayMessage, mainDir = AppDomain.CurrentDomain.BaseDirectory, consoleLog, connectToServerInfo, clientUniqueName, userErrorMessage, melodyOverKey = ";;'gosak";

	private static string contentLoadingInfo =
		"1. In File Explorer: Add it to the 'Content' folder/sub-folder inside it.\n" +
		"2. In Visual Studio's Solution Explorer: Add it to the according folder chosen above.\n" +
		"3. In Visual Studio's Solution Explorer: Right click file -> Properties -> Copy to Output Directory='CopyAlways'.\n" +
		"4. Open 'Content.mgcb' with the MonoGame Content Pipeline Tool.\n" +
		"5. In MonoGame Content Pipeline Tool: Add the file/folder and build/rebuild it.\n\n" +
		"Notes:\n" +
		"The .mgcb project has to look like the 'Content' folder.";

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
		public virtual void MouseJustInteractedWithHitbox(Body body, MouseHitboxInteraction interaction) { }
		public virtual void UserJustInteractedWithKey(Key key, Interaction interaction) { }
		public virtual void UserJustInteractedWithMouseButton(MouseButton button, Interaction interaction) { }
		public virtual void BodyJustCollidedWithBody(Body bodyA, Body bodyB) { }
		public virtual void MelodyJustEnded(string uniqueName) { }
		public virtual void SignalJustOccurred(Signal signal) { }
		public virtual void TimerTickJustOccurred(Timer timer) { }

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
			Canvas.SetPixelSizeWH(1, 1);

			graphics.ApplyChanges();
			game.Window.Title = "Gear";
			game.IsMouseVisible = true;

			Debug.SetUserErrorMessage($"{Gear.Window.GetTitle()} encountered an error and will now exit. Please contact the developer to get the issue fixed.");

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
				UpdateKeys();
				UpdateMouseButtons();
				UpdateBodies();
				UpdateSignalsAndTimers();

				eachTickLineCall = Debug.GetCodeLine() + 1;
				program.EachTick(tick);
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
		private static void UpdateKeys()
		{
			keysPressed.Clear();

			var keyPresses = Keyboard.GetState().GetPressedKeys();

			foreach (var key in keyPresses)
			{
				var gearKey = (Key)(int)key;
				keysPressed.Add(gearKey);
				if (lastFrameKeysPressed.Contains(gearKey) == false)
				{
					program.UserJustInteractedWithKey(gearKey, Interaction.Pressed);
				}
			}
			foreach (var key in lastFrameKeysPressed)
			{
				if (keysPressed.Contains(key) == false)
				{
					program.UserJustInteractedWithKey(key, Interaction.Released);
				}
			}

			lastFrameKeysPressed = new List<Key>(keysPressed);
		}
		private static void UpdateMouseButtons()
		{
			var pressed = new bool[3]
			{
				Input.LeftMouseButtonIsPressed(),
				Input.MiddleMouseButtonIsPressed(),
				Input.RightMouseButtonIsPressed()
			};
			for (int i = 0; i < 3; i++)
			{
				if (mouseButtonsLastFramePressed[i] == false && pressed[i])
				{
					program.UserJustInteractedWithMouseButton((MouseButton)i, Interaction.Pressed);
				}
				if (mouseButtonsLastFramePressed[i] && pressed[i] == false)
				{
					program.UserJustInteractedWithMouseButton((MouseButton)i, Interaction.Released);
				}
			}
			for (int i = 0; i < 3; i++)
			{
				mouseButtonsLastFramePressed[i] = false;
			}
			mouseButtonsLastFramePressed = pressed;
		}
		private static void UpdateBodies()
		{
			var leftClick = Input.LeftMouseButtonIsPressed();
			var justLeftClick = Gate.IsOpened("just-left-click", leftClick);
			foreach (var body in bodiesAll)
			{
				var isHovered = body.IsIgnoringAllObstacles() == false && body.GetAllHitboxLines().Length > 2 &&
					body.HitboxOverlapsPoint(Input.GetMouseCursorPosition());
				var obstacles = body.GetObstacles();

				foreach (var obstacle in obstacles)
				{
					if (Gate.IsOpened($"{body}-{obstacle}-collision", body.HitboxOverlapsObstacle(obstacle)))
					{
						program.BodyJustCollidedWithBody(body, obstacle);
					}
				}

				if (bodiesLastTickHovered.ContainsKey(body) == false)
				{
					bodiesLastTickHovered.Add(body, false);
				}
				if (bodiesClicked.ContainsKey(body) == false)
				{
					bodiesClicked.Add(body, false);
				}

				if (Gate.IsOpened($"{body}-hover", isHovered))
				{
					program.MouseJustInteractedWithHitbox(body, MouseHitboxInteraction.Hovered);
				}
				if (Gate.IsOpened($"{body}-unhover", isHovered == false && bodiesLastTickHovered[body]))
				{
					program.MouseJustInteractedWithHitbox(body, MouseHitboxInteraction.Unhovered);
				}
				if (Gate.IsOpened($"{body}-click", isHovered && justLeftClick))
				{
					bodiesClicked[body] = true;
					program.MouseJustInteractedWithHitbox(body, MouseHitboxInteraction.Clicked);
				}
				if (Gate.IsOpened($"{body}-click-released", isHovered && leftClick == false && bodiesClicked[body]))
				{
					program.MouseJustInteractedWithHitbox(body, MouseHitboxInteraction.ClickReleased);
				}

				bodiesLastTickHovered[body] = false;

				if (isHovered)
				{
					bodiesLastTickHovered[body] = true;
				}
			}
			if (leftClick == false)
			{
				bodiesClicked.Clear();
			}
		}
		private static void UpdateSignalsAndTimers()
		{
			foreach (var kvp in signalDelays)
			{
				var signal = kvp.Key;
				if (timerRepeats.ContainsKey(signal)) continue;

				if (SignalIsOccurring(signal, true))
				{
					if (signal == melodyOverKey)
					{
						program.MelodyJustEnded(melodyUniqueNames[MediaPlayer.Queue.ActiveSong]);
					}
					else
					{
						program.SignalJustOccurred(signal);
					}
				}
			}
			foreach (var kvp in timerRepeats)
			{
				var timer = kvp.Key;
				if (TimerTickIsOccurring(timer, timerTickSeconds[timer], timerRepeats[timer]))
				{
					program.TimerTickJustOccurred(timer);
				}
			}
		}

		protected override void Draw(GameTime gameTime)
		{
			if (Gear.Window.IsDisplayed() == false) return;
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
				var spriteShown = body.SpriteIsDisplayed();
				var tileIndex = body.GetSpriteGridIndexes();
				var cameraOffset = new Point(canvasSize.GetW() / 2, canvasSize.GetH() / 2);
				var pos = GetCameraBodyPosition(body) + cameraOffset - Camera.GetPosition();
				var size = body.GetSize();
				var spritesize = body.GetSpriteSize();
				var angle = Camera.GetAngle().GetA() + body.GetAngle().GetA();
				var scale = size / spritesize;
				var origin = body.GetSpriteOrigin();
				var color = body.GetSpriteColor();
				var boundariesSprite = new Texture2D(graphics.GraphicsDevice, 1, 1);
				var originSprite = new Texture2D(graphics.GraphicsDevice, 1, 1);
				var angleSprite = new Texture2D(graphics.GraphicsDevice, 1, 1);
				var hitboxSprite = new Texture2D(graphics.GraphicsDevice, 1, 1);
				var hitboxCrossPointsSprite = new Texture2D(graphics.GraphicsDevice, 1, 1);
				var hitboxMiddlePointSprite = new Texture2D(graphics.GraphicsDevice, 1, 1);
				var data = new Microsoft.Xna.Framework.Color[1] { Microsoft.Xna.Framework.Color.White };
				boundariesSprite.SetData(data);
				originSprite.SetData(data);
				angleSprite.SetData(data);
				hitboxSprite.SetData(data);
				hitboxCrossPointsSprite.SetData(data);
				hitboxMiddlePointSprite.SetData(data);

				if (sprite != null && spriteShown)
				{
					DrawTile(sprites[sprite], pos, tileIndex, body.GetSpriteGridSize(), size / scale, origin, scale, color, angle, SpriteEffects.None);
				}

				var boundariesColor = body.GetSizeColor();
				if (boundariesSprite != null && body.SizeIsDisplayed())
				{
					DrawTile(boundariesSprite, pos - origin, new Point(), 0, new Size(size.GetW(), 1), new Point(), new Size(), boundariesColor, angle, SpriteEffects.None);
					DrawTile(boundariesSprite, pos - origin, new Point(), 0, new Size(1, size.GetH()), new Point(), new Size(), boundariesColor, angle, SpriteEffects.None);
				}

				var angleColor = body.GetAngleColor();
				var angleWidth = body.GetAngleW();
				if (angleSprite != null && body.AngleIsDisplayed())
				{
					DrawTile(angleSprite, pos, new Point(), 0, new Size(size.GetW() * 1.1f, angleWidth), new Point(), new Size(1, 1), angleColor, angle, SpriteEffects.None);
				}

				var originColor = body.GetOriginColor();
				var originSize = body.GetOriginSize();
				if (originSprite != null && body.OriginIsDisplayed())
				{
					DrawTile(originSprite, pos - new Point(originSize.GetW() / 2, originSize.GetH() / 2), new Point(), 0, originSize, new Point(), new Size(1, 1), originColor, 0, SpriteEffects.None);
				}

				var hitboxColor = body.GetHitboxColor();
				var hitboxWidth = body.GetHitboxW();
				var hitboxLinesCamera = new List<Line>();
				if (hitboxSprite != null && body.HitboxIsDisplayed())
				{
					var lines = body.GetAllHitboxLines();
					foreach (var line in lines)
					{
						var lineAngle = new Angle();
						var dist = body.GetPosition().GetDistanceToPoint(line.GetStartPoint());
						var dir = new Direction();
						var camAng = Camera.GetAngle();
						var ang = new Angle();
						ang.SetFromBetweenPoints(body.GetPosition(), line.GetStartPoint());
						ang += camAng;
						dir.SetFromAngle(ang);
						var linePos = GetCameraBodyPosition(body) + cameraOffset + dir.GetEndPoint() * dist - Camera.GetPosition();
						lineAngle.SetFromBetweenPoints(line.GetStartPoint(), line.GetEndPoint());
						var lineDir = new Direction();
						lineDir.SetFromAngle(camAng + lineAngle);
						var endPos = linePos + lineDir.GetEndPoint() * line.GetLength() - Camera.GetPosition();

						hitboxLinesCamera.Add(new Line(linePos, endPos));
						DrawTile(hitboxSprite, linePos - new Point(0, hitboxWidth / 2), new Point(), 0, new Size(1, 1), new Point(), new Size(line.GetLength(), hitboxWidth), hitboxColor, camAng.GetA() + lineAngle.GetA(), SpriteEffects.None);
					}
				}

				var hitboxCrossPointsColor = body.GetHitboxCrossPointsColor();
				var hitboxCrossPointsSize = body.GetHitboxCrossPointsSize();
				if (hitboxCrossPointsSprite != null && body.HitboxCrossPointsAreDisplayed())
				{
					var crossPoints = body.GetAllHitboxCrossPoints();
					foreach (var point in crossPoints)
					{
						DrawTile(hitboxCrossPointsSprite, point + cameraOffset - new Point(hitboxCrossPointsSize.GetW() / 2, hitboxCrossPointsSize.GetH() / 2), new Point(), 0, hitboxCrossPointsSize, new Point(), new Size(1, 1), hitboxCrossPointsColor, 0, SpriteEffects.None);
					}
				}

				var hitboxMiddlePointColor = body.GetHitboxMiddlePointColor();
				var hitboxMiddlePointSize = body.GetHitboxMiddlePointSize();
				if (hitboxMiddlePointSprite != null && body.HitboxMiddlePointIsDisplayed())
				{
					var cameraMiddlePoint = new Point();
					var middleDir = new Direction();
					var middleAng = new Angle();
					var middlePoint = body.GetHitboxMiddlePoint() + cameraOffset;
					var bodyPos = body.GetPosition() + cameraOffset;
					var dist = bodyPos.GetDistanceToPoint(middlePoint);

					middleAng.SetFromBetweenPoints(bodyPos, middlePoint);
					middleDir.SetFromAngle(middleAng + Camera.GetAngle());

					cameraMiddlePoint = GetCameraBodyPosition(body) + cameraOffset + middleDir.GetEndPoint() * dist - Camera.GetPosition();

					DrawTile(hitboxMiddlePointSprite, cameraMiddlePoint - new Point(hitboxMiddlePointSize.GetW() / 2, hitboxMiddlePointSize.GetH() / 2), new Point(), 0, hitboxMiddlePointSize, new Point(), new Size(1, 1), hitboxMiddlePointColor, 0, SpriteEffects.None);
				}

				boundariesSprite.Dispose();
				angleSprite.Dispose();
				originSprite.Dispose();
				hitboxSprite.Dispose();
				hitboxCrossPointsSprite.Dispose();
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
			if (CountFolder($"{mainDir}\\Content")) return;

			loadingScreenUpdatePerFiles = (int)Math.Ceiling(contentFileCount / 10d);
		}
		private static bool CountFolder(string folder)
		{
			if (Directory.Exists(folder) == false)
			{
				return true;
			}

			var files = Directory.GetFiles(folder);
			var datafiles = new List<string>();
			var names = new List<string>();

			for (int i = 0; i < files.Length; i++)
			{
				var path = files[i].Split('\\');
				var name = path[path.Length - 1];
				var pathIndex = path.Length - 1;
				var directory = "";
				while (true)
				{
					pathIndex--;
					directory = $"{directory}{path[pathIndex]}";
					if (path[pathIndex] == "Content") break;
					directory = $"{directory}/";
				}
				name = name.Replace(".png", "");
				name = name.Replace(".spritefont", "");
				name = name.Replace(".wav", "");
				name = name.Replace(".mp3", "");

				if (names.Contains(name))
				{
					Error($"Description:\nAnother content file with the name '{name}' already exists in the '{directory}' directory.\n\n" +
						$"Tip:\nMake sure that each file in the same directory has a unique name.", 5);
					return true;
				}
				names.Add(name);

				if (files[i].Contains(".png") || files[i].Contains(".spritefont") || files[i].Contains(".wav") || files[i].Contains(".mp3"))
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
			return false;
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
				case "mp3":
					{
						melodies[key] = game.Content.Load<Song>(key);
						melodyUniqueNames[melodies[key]] = key;
						break;
					}
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
		private static void _SetPixelSize(Size size, int index, bool invalidSizeError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(size)} = {size}\n" +
				$"{nameof(invalidSizeError)} = {invalidSizeError.ToString().ToLower()}";

			if (size.GetW() < 1 || size.GetW() > screenSize.GetW() ||
				size.GetH() < 1 || size.GetH() > screenSize.GetH())
			{
				if (invalidSizeError)
				{
					Error($"{parameters}\n\n{GetInvalidValueError("pixel size", $"{size}")}\n\nTip: Make sure the pixel size is between '{new Size(1, 1)}' and '{screenSize}'.", index + 1);
				}
				return;
			}

			size.SetWH(Number.GetLimited(size.GetW(), 1, screenSize.GetW()),
				Number.GetLimited(size.GetH(), 1, (int)screenSize.GetH()));
			pixelSize = size;
			canvasSize = screenSize / size;
			var gd = game.GraphicsDevice;
			renderTarget = new RenderTarget2D(gd, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, false, gd.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
			graphics.ApplyChanges();
		}
		public static void SetPixelSize(Size size, bool invalidSizeError = true)
		{
			_SetPixelSize(size, 1, invalidSizeError);
		}
		public static void SetPixelSizeWH(float w, float h, bool invalidSizeError = true)
		{
			_SetPixelSize(new Size(w, h), 1, invalidSizeError);
		}
		public static void SetPixelSizeW(float w, bool invalidSizeError = true)
		{
			_SetPixelSize(new Size(w, pixelSize.GetH()), 1, invalidSizeError);
		}
		public static void SetPixelSizeH(float h, bool invalidSizeError = true)
		{
			_SetPixelSize(new Size(pixelSize.GetW(), h), 1, invalidSizeError);
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
		public static bool IsFocused()
		{
			return game.IsActive;
		}
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

		public static void Display(bool displayed)
		{
			windowIsDisplayed = displayed;
			var form = Control.FromHandle(game.Window.Handle) as Form;
			if (displayed) form.Show();
			else form.Hide();
		}
		public static bool IsDisplayed()
		{
			return windowIsDisplayed;
		}

		/// <summary>
		/// - Sets the <paramref name="title"/> of the window.<br></br><br></br>
		/// - The title can be received with <see cref="WindowTitleGet"/>.
		/// </summary>
		public static void SetTitle(string title, bool titleIsNullError = true)
		{
			var titleStr = title == null ? "null" : $"\"{title}\"";
			var parameters = $"Parameters:\n" +
				$"{nameof(title)} = {titleStr}\n" +
				$"{nameof(titleIsNullError)} = {titleIsNullError.ToString().ToLower()}";

			if (IsNullError(parameters, nameof(title), title, titleIsNullError, 1)) return;

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

		public static void PopUp(string message, string title, PopUpIcon icon = PopUpIcon.None)
		{
			var msgIcon = MessageBoxIcon.None;
			switch (icon)
			{
				case PopUpIcon.Info: msgIcon = MessageBoxIcon.Information; break;
				case PopUpIcon.Error: msgIcon = MessageBoxIcon.Error; break;
				case PopUpIcon.Warning: msgIcon = MessageBoxIcon.Warning; break;
			}
			System.Windows.Forms.MessageBox.Show(message, title, MessageBoxButtons.OK, msgIcon);
		}

		/// <summary>
		/// - Ends the runtime of the program and closes the window.
		/// </summary>
		public static void Close()
		{
			game.Exit();
		}
	}
	/// <summary>
	/// A main object for the program that can contain different data for it to be displayed on the screen and interacted with.
	/// </summary>
	public class Body
	{
		#region Data
		private static int ID;
		private static Dictionary<string, Body> bodyUniqueNames = new Dictionary<string, Body>();

		[JsonProperty]
		private Color spriteColor, boundariesColor, originColor, angleColor, hitboxColor, hitboxCrossPointsColor, hitboxMiddlePointColor;
		[JsonProperty]
		private Size size, spriteSize, originSize, hitboxCrossPointsSize, hitboxMiddlePointSize;
		[JsonProperty]
		private Point position, spriteOrigin, spriteIndex;
		[JsonProperty]
		private int UID, spriteGridSize, createdAtTick;
		[JsonProperty]
		private Angle angle;
		[JsonProperty]
		private string uniqueName, spriteName;
		[JsonProperty]
		private float hitboxWidth, angleWidth, createdAtTime;
		[JsonProperty]
		private bool boundariesShown, originShown, angleShown, spriteShown, hitboxShown, hitboxCrossPointsShown, hitboxMiddlePointShown, positionLocked, angleLocked, sizeLocked, ignoreCollisions;
		[JsonProperty]
		private List<string> tags = new List<string>();
		[JsonProperty]
		private List<Body> hitboxObstacles = new List<Body>(), hitboxExceptions = new List<Body>();
		[JsonProperty]
		private Dictionary<string, float[]> hitboxLineDistances = new Dictionary<string, float[]>(), hitboxLineAngles = new Dictionary<string, float[]>();
		[JsonProperty]
		private Dictionary<string, Size> hitboxLineSizes = new Dictionary<string, Size>();
		[JsonProperty]
		private Dictionary<string, Line> hitboxLines = new Dictionary<string, Line>();
		#endregion
		#region Creation
		public static Body[] GetAll()
		{
			return bodiesAll.ToArray();
		}
		public static Body GetByUniqueName(string uniqueName)
		{
			if (uniqueName == null || bodyUniqueNames.ContainsKey(uniqueName) == false)
			{
				return default;
			}
			return bodyUniqueNames[uniqueName];
		}
		public static Body[] GetAllByTag(string tag)
		{
			if (tag == null || tagBodies.ContainsKey(tag) == false)
			{
				return new Body[0];
			}
			return tagBodies[tag].ToArray();
		}
		public static Body[] GetAllHovered()
		{
			var hovered = new List<Body>();
			foreach (var kvp in bodiesLastTickHovered)
			{
				if (kvp.Value)
				{
					hovered.Add(kvp.Key);
				}
			}
			return hovered.ToArray();
		}

		public Body(string uniqueName, bool nameIsNullError = true, bool nameExistsError = true)
		{
			Instantiate();
			_SetUniqueName(uniqueName, 1, nameIsNullError, nameExistsError);
		}
		private void Instantiate()
		{
			bodiesAll.Add(this);
			UID = ID;
			ID++;
			size.SetWH(1, 1);
			createdAtTick = tick;
			createdAtTime = time;
			UpdateCameraBodyTransform(this);
		}
		public void Delete()
		{
			bodyCameraAngle.Remove(this);
			bodyCameraAngleDifferences.Remove(this);
			bodyCameraDistances.Remove(this);
			bodyUniqueNames.Remove(uniqueName);
			bodiesAll.Remove(this);
			bodiesClicked.Remove(this);
			bodiesLastTickHovered.Remove(this);
			RemoveAllTags();
			foreach (var body in bodiesAll)
			{
				body.hitboxExceptions.Remove(this);
				body.hitboxObstacles.Remove(this);
			}
			render = true;
		}
		public int GetTickOfCreation()
		{
			return createdAtTick;
		}
		public float GetTimeOfCreation()
		{
			return createdAtTime;
		}

		// DUPLICATION - UPDATE FREQUENTLY
		//public Body Duplicate(string uniqueName)
		//{
		//	var dup = new Body(uniqueName);
		//	dup.spriteColor = spriteColor;
		//	dup.boundariesColor = boundariesColor;
		//	dup.originColor = originColor;
		//	dup.angleColor = angleColor;
		//	dup.size = size;
		//	dup.spriteSize = spriteSize;
		//	dup.position = position;
		//	dup.spriteOrigin = spriteOrigin;
		//	dup.spriteIndex = spriteIndex;
		//	dup.spriteGridSize = spriteGridSize;
		//	dup.angle = angle;
		//	dup.spriteName = spriteName;
		//	dup.boundariesShown = boundariesShown;
		//	dup.originShown = originShown;
		//	dup.angleShown = angleShown;
		//	dup.spriteShown = spriteShown;
		//	return dup;
		//}
		#endregion
		#region Identity
		public int GetUniqueID()
		{
			return UID;
		}
		public string GetUniqueName()
		{
			return uniqueName;
		}
		private void _SetUniqueName(string uniqueName, int methodIndex, bool nameIsNullError = true, bool nameExistsError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(uniqueName)} = \"{uniqueName}\"\n" +
				$"{nameof(nameIsNullError)} = {nameIsNullError.ToString().ToLower()}\n" +
				$"{nameof(nameExistsError)} = {nameExistsError.ToString().ToLower()}";
			if (uniqueName == null)
			{
				if (nameIsNullError)
				{
					Error(GetCannotBeNullError($"{nameof(Body)}'s {nameof(uniqueName)}"), methodIndex + 1);
				}
				return;
			}
			if (bodyUniqueNames.ContainsKey(uniqueName))
			{
				if (nameExistsError)
				{
					Error($"{parameters}\n\n{GetAlreadyExistsError($"{nameof(Body)}'s {nameof(uniqueName)}", uniqueName)}\n\n" +
						$"Tip:\n" +
						$"Make sure you are not creating the {nameof(Body)} multiple times or each tick.", methodIndex + 1);
				}
				return;
			}

			this.uniqueName = uniqueName;
			bodyUniqueNames.Add(uniqueName, this);
		}
		public void SetUniqueName(string uniqueName, bool nameIsNullError = true, bool nameExistsError = true)
		{
			_SetUniqueName(uniqueName, 1, nameIsNullError, nameExistsError);
		}

		public void AddTag(string tag, bool tagIsNullError = true, bool tagAlreadyAddedError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(tag)} = \"{tag}\"\n" +
				$"{nameof(tagIsNullError)} = {tagIsNullError.ToString().ToLower()}\n" +
				$"{nameof(tagAlreadyAddedError)} = {tagAlreadyAddedError.ToString().ToLower()}";
			if (IsNullError(parameters, nameof(tag), tag, tagIsNullError, 1)) return;

			if (ValueAlreadyAddedError(parameters, tags, $"{nameof(Body)}'s {nameof(tag)}", tag, tagAlreadyAddedError, 1)) return;

			tags.Add(tag);
			if (tagBodies.ContainsKey(tag))
			{
				tagBodies[tag].Add(this);
			}
			else
			{
				tagBodies.Add(tag, new List<Body>() { this });
			}
		}
		public void RemoveTag(string tag, bool tagIsNullError = true, bool tagNotFoundError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(tag)} = \"{tag}\"\n" +
				$"{nameof(tagIsNullError)} = {tagIsNullError.ToString().ToLower()}\n" +
				$"{nameof(tagNotFoundError)} = {tagNotFoundError.ToString().ToLower()}";
			if (IsNullError(parameters, nameof(tag), tag, tagIsNullError, 1)) return;
			if (ValueNotFoundError(parameters, tags, nameof(tag), tag, tagNotFoundError, 1)) return;

			tags.Remove(tag);

			tagBodies[tag].Remove(this);
			if (tagBodies[tag].Count == 0)
			{
				tagBodies.Remove(tag);
			}
		}
		public void RemoveAllTags()
		{
			foreach (var tag in tags)
			{
				tagBodies[tag].Remove(this);
				if (tagBodies[tag].Count == 0)
				{
					tagBodies.Remove(tag);
				}
			}

			tags.Clear();
		}
		public string[] GetTags()
		{
			return tags.ToArray();
		}
		public bool HasTag(string tag, bool tagIsNullError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(tag)} = \"{tag}\"\n" +
				$"{nameof(tagIsNullError)} = {tagIsNullError.ToString().ToLower()}";
			if (IsNullError(parameters, nameof(tag), tag, tagIsNullError, 1)) return false;

			return tags.Contains(tag);
		}

		public override string ToString()
		{
			return $"[{UID}] {uniqueName}";
		}
		#endregion
		#region Transform
		#region Position
		private void _SetPosition(Point pos)
		{
			position = pos;
			render = true;
			UpdateCollisions();
			UpdateCameraBodyTransform(this);
		}
		public void SetPositionXY(float x, float y, bool positionLockedError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(x)} = {x}, {nameof(y)} = {y}\n" +
				$"{nameof(positionLockedError)} = {positionLockedError.ToString().ToLower()}";
			if (BodyTransformLockedError(parameters, "position", positionLocked, positionLockedError, 1)) return;

			_SetPosition(new Point(x, y));
		}
		public void SetPositionX(float x, bool positionLockedError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(x)} = {x}\n" +
				$"{nameof(positionLockedError)} = {positionLockedError.ToString().ToLower()}";
			if (BodyTransformLockedError(parameters, "position", positionLocked, positionLockedError, 1)) return;

			_SetPosition(new Point(x, position.GetY()));
		}
		public void SetPositionY(float y, bool positionLockedError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(y)} = {y}\n" +
				$"{nameof(positionLockedError)} = {positionLockedError.ToString().ToLower()}";
			if (BodyTransformLockedError(parameters, "position", positionLocked, positionLockedError, 1)) return;

			_SetPosition(new Point(position.GetX(), y));
		}
		public void SetPosition(Point position, bool positionLockedError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(position)} = {position}\n" +
				$"{nameof(positionLockedError)} = {positionLockedError.ToString().ToLower()}";
			if (BodyTransformLockedError(parameters, "position", positionLocked, positionLockedError, 1)) return;

			_SetPosition(position);
		}
		public Point GetPosition()
		{
			return position;
		}
		public void LockPosition(bool locked)
		{
			positionLocked = locked;
		}
		public bool PositionIsLocked()
		{
			return positionLocked;
		}
		public void DisplayOrigin(bool display = true, float r = 255, float g = 255, float b = 255, float o = 255, float w = 4, float h = 4)
		{
			originShown = display;
			originColor.SetRGBO(r, g, b, o);
			originSize.SetWH(w, h);
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
		public Size GetOriginSize()
		{
			return originSize;
		}
		#endregion
		#region Angle
		private void _SetAngle(Angle angle)
		{
			this.angle = angle;
			render = true;
			UpdateCollisions();
		}
		public void SetAngleA(float a, bool angleLockedError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(a)} = {a}\n" +
				$"{nameof(angleLockedError)} = {angleLockedError.ToString().ToLower()}";
			if (BodyTransformLockedError(parameters, "angle", angleLocked, angleLockedError, 1)) return;

			_SetAngle(new Angle(a));
		}
		public void SetAngle(Angle angle, bool angleLockedError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(angle)} = {angle}\n" +
				$"{nameof(angleLockedError)} = {angleLockedError.ToString().ToLower()}";
			if (BodyTransformLockedError(parameters, "angle", angleLocked, angleLockedError, 1)) return;
			_SetAngle(angle);
		}
		public Angle GetAngle()
		{
			return angle;
		}
		public void LockAngle(bool locked)
		{
			angleLocked = locked;
		}
		public bool AngleIsLocked()
		{
			return angleLocked;
		}
		public void DisplayAngle(bool display = true, float r = 255, float g = 255, float b = 255, float o = 255, float w = 2)
		{
			angleShown = display;
			angleColor.SetRGBO(r, g, b, o);
			angleWidth = w;
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
		public float GetAngleW()
		{
			return angleWidth;
		}
		#endregion
		#region Size
		private void _SetSize(Size size)
		{
			this.size = size;
			render = true;
			UpdateCollisions();
		}
		public void SetSize(Size size, bool sizeLockedError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(size)} = {size}\n" +
				$"{nameof(sizeLockedError)} = {sizeLockedError.ToString().ToLower()}";
			if (BodyTransformLockedError(parameters, "size", sizeLocked, sizeLockedError, 1)) return;

			_SetSize(size);
		}
		public void SetSizeWH(float w, float h, bool sizeLockedError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(w)} = {w}, {nameof(h)} = {h}\n" +
				$"{nameof(sizeLockedError)} = {sizeLockedError.ToString().ToLower()}";
			if (BodyTransformLockedError(parameters, "size", sizeLocked, sizeLockedError, 1)) return;

			_SetSize(new Size(w, h));
		}
		public void SetSizeW(float w, bool sizeLockedError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(w)} = {w}\n" +
				$"{nameof(sizeLockedError)} = {sizeLockedError.ToString().ToLower()}";
			if (BodyTransformLockedError(parameters, "size", sizeLocked, sizeLockedError, 1)) return;

			_SetSize(new Size(w, size.GetH()));
		}
		public void SetSizeH(float h, bool sizeLockedError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(h)} = {h}\n" +
				$"{nameof(sizeLockedError)} = {sizeLockedError.ToString().ToLower()}";
			if (BodyTransformLockedError(parameters, "size", sizeLocked, sizeLockedError, 1)) return;

			_SetSize(new Size(size.GetW(), h));
		}
		public Size GetSize()
		{
			return size;
		}
		public void LockSize(bool locked)
		{
			sizeLocked = locked;
		}
		public bool SizeIsLocked()
		{
			return sizeLocked;
		}
		public void DisplaySize(bool display = true, float r = 255, float g = 255, float b = 255, float o = 255)
		{
			boundariesShown = display;
			boundariesColor.SetRGBO(r, g, b, o);
			render = true;
		}
		public bool SizeIsDisplayed()
		{
			return boundariesShown;
		}
		public Color GetSizeColor()
		{
			return boundariesColor;
		}
		#endregion

		private bool BodyTransformLockedError(string parameters, string component, bool condition, bool error, int index)
		{
			if (condition)
			{
				if (error)
				{
					Error($"{parameters}\n\nCannot change {nameof(Body)}'s {component} due to it being locked.", index + 1);
				}
				return true;
			}
			return false;
		}
		#endregion
		#region Sprite
		public void DisplaySprite(string name, bool displayed = true, int width = 64, int height = 64, float r = 255, float g = 255, float b = 255, float o = 255, int originX = 0, int originY = 0, int gridSize = 0, int indexH = 0, int indexV = 0, bool nameNotFound = true)
		{
			if (sprites.ContainsKey(name) == false)
			{
				var parameters = $"Parameters:\n" +
					$"{nameof(name)} = \"{name}\"\n" +
					$"{nameof(displayed)} = {displayed.ToString().ToLower()}\n" +
					$"{nameof(width)} = {width}, {nameof(height)} = {height}\n" +
					$"{nameof(r)} = {r}, {nameof(g)} = {g}, {nameof(b)} = {b}, {nameof(o)} = {o}\n" +
					$"{nameof(originX)} = {originX}, {nameof(originY)} = {originY}\n" +
					$"{nameof(gridSize)} = {gridSize}\n" +
					$"{nameof(indexH)} = {indexH}, {nameof(indexV)} = {indexV}\n";
				if (nameNotFound)
				{
					Error($"{parameters}\n\n{GetContentNotFoundError("sprite", name)}", 1);
				}
				return;
			}
			spriteName = name;
			size = new Size(sprites[name].Width, sprites[name].Height);
			spriteSize = new Size(width, height);
			spriteColor = new Color(r, g, b, o);
			spriteOrigin = new Point(originX, originY);
			spriteGridSize = gridSize;
			spriteIndex = new Point(indexH, indexV);
			spriteShown = displayed;
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
		#endregion
		#region Hitbox
		#region Display
		public void DisplayHitbox(bool display = true, float r = 255, float g = 255, float b = 255, float o = 255, float w = 2)
		{
			hitboxShown = display;
			hitboxColor.SetRGBO(r, g, b, o);
			hitboxWidth = w;
			render = true;
		}
		public Color GetHitboxColor()
		{
			return hitboxColor;
		}
		public bool HitboxIsDisplayed()
		{
			return hitboxShown;
		}
		public float GetHitboxW()
		{
			return hitboxWidth;
		}
		#endregion
		#region Cross Points
		public void DisplayHitboxCrossPoints(bool display = true, float r = 255, float g = 255, float b = 255, float o = 255, float w = 4, float h = 4)
		{
			hitboxCrossPointsShown = display;
			hitboxCrossPointsColor.SetRGBO(r, g, b, o);
			hitboxCrossPointsSize.SetWH(w, h);
			render = true;
		}
		public Color GetHitboxCrossPointsColor()
		{
			return hitboxCrossPointsColor;
		}
		public Size GetHitboxCrossPointsSize()
		{
			return hitboxCrossPointsSize;
		}
		public bool HitboxCrossPointsAreDisplayed()
		{
			return hitboxCrossPointsShown;
		}

		#endregion
		#region Creation
		private void _SetHitboxLine(string uniqueKey, Line line)
		{
			var position = this.position - spriteOrigin;
			line = new Line(position + line.GetStartPoint(), position + line.GetEndPoint());

			hitboxLines[uniqueKey] = line;

			var startAngle = new Angle();
			var endAngle = new Angle();
			var startDist = position.GetDistanceToPoint(line.GetStartPoint());
			var endDist = position.GetDistanceToPoint(line.GetEndPoint());
			startAngle.SetFromBetweenPoints(position, line.GetStartPoint());
			endAngle.SetFromBetweenPoints(position, line.GetEndPoint());
			hitboxLineAngles[uniqueKey] = new float[] { startAngle.GetA(), endAngle.GetA() };
			hitboxLineDistances[uniqueKey] = new float[] { startDist, endDist };
			hitboxLineSizes[uniqueKey] = size;
			UpdateCollisions();
		}
		public void SetHitboxLine(string uniqueKey, Line line, bool keyNotFoundError = true, bool keyIsNullError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(uniqueName)} = \"{uniqueName}\"\n" +
				$"{nameof(line)} = {line}\n" +
				$"{nameof(keyNotFoundError)} = {keyNotFoundError.ToString().ToLower()}\n" +
				$"{nameof(keyIsNullError)} = {keyIsNullError.ToString().ToLower()}";
			if (KeyNotFoundError(parameters, hitboxLines, nameof(uniqueKey), uniqueName, keyNotFoundError, 1)) return;

			_SetHitboxLine(uniqueName, line);
		}
		public void AddHitboxLine(string uniqueKey, Line line, bool keyExistsError = true, bool keyIsNullError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(uniqueKey)} = \"{uniqueKey}\"\n" +
				$"{nameof(line)} = {line}\n" +
				$"{nameof(keyExistsError)} = {keyExistsError.ToString().ToLower()}\n" +
				$"{nameof(keyIsNullError)} = {keyIsNullError.ToString().ToLower()}";
			if (hitboxLines == null) hitboxLines = new Dictionary<string, Line>();
			if (IsNullError(parameters, $"{nameof(line)}'s {nameof(uniqueKey)}", uniqueKey, keyIsNullError, 1)) return;
			if (KeyExistsError(parameters, hitboxLines, nameof(uniqueKey), uniqueKey, keyExistsError, 1)) return;

			_SetHitboxLine(uniqueKey, line);
		}
		public Line GetHitboxLine(string uniqueKey, bool keyNotFoundError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(uniqueName)} = \"{uniqueName}\"\n" +
				$"{nameof(keyNotFoundError)} = {keyNotFoundError.ToString().ToLower()})";
			if (KeyNotFoundError(parameters, hitboxLines, nameof(uniqueKey), uniqueName, keyNotFoundError, 1)) return default;

			return hitboxLines[uniqueName];
		}
		public Line[] GetAllHitboxLines()
		{
			return hitboxLines.Values.ToArray();
		}
		public string[] GetAllHitboxLineUniqueNames()
		{
			return hitboxLines.Keys.ToArray();
		}
		#endregion
		#region Collision Cases
		public void IgnoreAllObstacles(bool ignored)
		{
			ignoreCollisions = ignored;
		}
		public bool IsIgnoringAllObstacles()
		{
			return ignoreCollisions;
		}
		public void AddHitboxObstacle(Body body, bool bodyAlreadyAddedError = true)
		{
			AddHitboxX(hitboxObstacles, body, bodyAlreadyAddedError);
		}
		public void SetHitboxObstacles(Body[] bodies)
		{
			hitboxObstacles = new List<Body>(bodies.ToList());
		}
		public void RemoveHitboxObstacle(Body body, bool bodyNotFoundError = true)
		{
			RemoveHitboxX(hitboxObstacles, body, bodyNotFoundError);
		}
		public void RemoveAllHitboxObstacles()
		{
			hitboxObstacles.Clear();
		}
		public Body[] GetObstacles()
		{
			return hitboxObstacles.ToArray();
		}
		public bool HitboxOverlapsPoint(Point point)
		{
			var ray = new Line(point, new Point(99_999, 99_999));
			var crossSum = 0;
			foreach (var kvp in hitboxLines)
			{
				var line = kvp.Value;
				if (ray.IsCrossingLine(line))
				{
					crossSum += ray.GetCrossPointWithLine(line).Length;
				}
			}
			return crossSum % 2 != 0 && ignoreCollisions == false;
		}
		public bool HitboxOverlapsObstacleLine(Body body)
		{
			return ignoreCollisions == false && hitboxExceptions.Contains(body) == false && hitboxObstacles.Contains(body) &&
				GetHitboxCrossPointsWithObstacle(body).Length > 0;
		}
		public bool HitboxOverlapsObstacle(Body body)
		{
			return HitboxMiddlePointOverlapsObstacle(body) || HitboxOverlapsObstacleLine(body);
		}
		public Point[] GetHitboxCrossPointsWithObstacle(Body body)
		{
			var result = new List<Point>();
			var bodyLines = body.GetAllHitboxLines();
			var myLines = hitboxLines.Values;
			foreach (var myLine in myLines)
			{
				foreach (var bodyLine in bodyLines)
				{
					result.AddRange(myLine.GetCrossPointWithLine(bodyLine));
				}
			}
			return result.ToArray();
		}
		public Point[] GetAllHitboxCrossPoints()
		{
			var result = new List<Point>();

			foreach (var obstacle in hitboxObstacles)
			{
				result.AddRange(GetHitboxCrossPointsWithObstacle(obstacle));
			}
			return result.ToArray();
		}
		public bool HasHitboxObstacle(Body body)
		{
			return hitboxObstacles.Contains(body);
		}
		public bool HasHitboxException(Body body)
		{
			return hitboxExceptions.Contains(body);
		}

		public void AddHitboxException(Body body, bool bodyAlreadyAddedError = true)
		{
			AddHitboxX(hitboxExceptions, body, bodyAlreadyAddedError);
		}
		public void SetHitboxExceptions(Body[] bodies)
		{
			hitboxExceptions = new List<Body>(bodies.ToList());
		}
		public void RemoveHitboxException(Body body, bool bodyNotFoundError = true)
		{
			RemoveHitboxX(hitboxExceptions, body, bodyNotFoundError);
		}
		public void RemoveAllHitboxExceptions()
		{
			hitboxExceptions.Clear();
		}
		public Body[] GetExceptions()
		{
			return hitboxExceptions.ToArray();
		}

		private void AddHitboxX(List<Body> list, Body body, bool bodyAlreadyAddedError = true)
		{
			if (list.Contains(body))
			{
				if (bodyAlreadyAddedError)
				{
					var parameters = $"Parameters:\n" +
						$"{nameof(body)} = {body}\n" +
						$"{nameof(bodyAlreadyAddedError)} = {bodyAlreadyAddedError.ToString().ToLower()})";
					Error(GetAlreadyExistsError(nameof(body), $"{body}"), 2);
				}
				return;
			}
			list.Add(body);
		}
		private void RemoveHitboxX(List<Body> list, Body body, bool bodyNotFoundError = true)
		{
			if (list.Contains(body) == false)
			{
				if (bodyNotFoundError)
				{
					var parameters = $"Parameters:\n" +
						$"{nameof(body)} = {body}\n" +
						$"{nameof(bodyNotFoundError)} = {bodyNotFoundError}";
					Error($"{parameters}\n\n{GetNotFoundError(nameof(Body), $"{body}")}", 1);
				}
				return;
			}
			list.Remove(body);
		}
		private void UpdateCollisions()
		{
			foreach (var kvp in hitboxLines)
			{
				var key = kvp.Key;
				var start = new Point();
				var end = new Point();
				var dirStart = new Direction();
				var dirEnd = new Direction();

				var baseSize = hitboxLineSizes[key];
				var ratio = new Size(size.GetW() < size.GetH() ? size.GetW() / size.GetH() : 1, size.GetH() < size.GetW() ? size.GetH() / size.GetW() : 1);
				baseSize *= ratio;
				var scale = new Point(size.GetW() / baseSize.GetW(), size.GetH() / baseSize.GetH());

				dirStart.SetFromAngle((angle + new Angle(hitboxLineAngles[key][0])));
				dirEnd.SetFromAngle((angle + new Angle(hitboxLineAngles[key][1])));

				start = position + dirStart.GetEndPoint() * hitboxLineDistances[key][0] * scale;
				end = position + dirEnd.GetEndPoint() * hitboxLineDistances[key][1] * scale;

				hitboxLines[key] = new Line(start, end);
			}
		}
		#endregion
		#region Middle Point
		public Point GetHitboxMiddlePoint()
		{
			var mostLeftPoint = new Point(float.PositiveInfinity, float.PositiveInfinity);
			var mostRightPoint = new Point(float.NegativeInfinity, float.NegativeInfinity);

			foreach (var kvp in hitboxLines)
			{
				var line = kvp.Value;

				if (line.GetStartPoint().GetX() < mostLeftPoint.GetX()) mostLeftPoint = line.GetStartPoint();
				if (line.GetEndPoint().GetX() < mostLeftPoint.GetX()) mostLeftPoint = line.GetEndPoint();
				if (line.GetStartPoint().GetX() > mostRightPoint.GetX()) mostRightPoint = line.GetStartPoint();
				if (line.GetEndPoint().GetX() > mostRightPoint.GetX()) mostRightPoint = line.GetEndPoint();

				if (line.GetStartPoint().GetY() < mostLeftPoint.GetY()) mostLeftPoint.SetY(line.GetStartPoint().GetY());
				if (line.GetEndPoint().GetY() < mostLeftPoint.GetY()) mostLeftPoint.SetY(line.GetEndPoint().GetY());
				if (line.GetStartPoint().GetY() > mostRightPoint.GetY()) mostRightPoint.SetY(line.GetStartPoint().GetY());
				if (line.GetEndPoint().GetY() > mostRightPoint.GetY()) mostRightPoint.SetY(line.GetEndPoint().GetY());
			}
			var result = mostLeftPoint;
			result.SetToPercentTowardPoint(mostRightPoint, 50);
			return result;
		}
		public void DisplayHitboxMiddlePoint(bool display = true, float r = 255, float g = 255, float b = 255, float o = 255, float w = 4, float h = 4)
		{
			hitboxMiddlePointShown = display;
			hitboxMiddlePointColor.SetRGBO(r, g, b, o);
			hitboxMiddlePointSize.SetWH(w, h);
			render = true;
		}
		public Color GetHitboxMiddlePointColor()
		{
			return hitboxMiddlePointColor;
		}
		public Size GetHitboxMiddlePointSize()
		{
			return hitboxMiddlePointSize;
		}
		public bool HitboxMiddlePointIsDisplayed()
		{
			return hitboxMiddlePointShown;
		}
		public bool HitboxMiddlePointOverlapsObstacle(Body body)
		{
			if (hitboxObstacles.Contains(body) == false) return false;
			var ray = new Line(body.GetHitboxMiddlePoint(), new Point(99_999, -99_999));
			var crossSum = 0;
			var lines = GetAllHitboxLines();
			foreach (var line in lines)
			{
				crossSum += ray.GetCrossPointWithLine(line).Length;
			}
			return crossSum == 1 && hitboxExceptions.Contains(body) == false && ignoreCollisions == false;
		}
		#endregion
		#endregion
	}

	public class Signal
	{
		private string uniqueName;
		private float delayInSeconds, startTime, endTime;
		private bool isPaused;

		public Signal(string uniqueName, float delayInSeconds)
		{
			this.uniqueName = uniqueName;
			this.delayInSeconds = Number.GetUnsigned(delayInSeconds);
			startTime = Performance.GetSecondsSinceStart();
			endTime = startTime + delayInSeconds;
		}
		public float GetDelayInSeconds()
		{
			return delayInSeconds;
		}
		public void Pause(bool paused)
		{
			isPaused = paused;
		}
		public float GetSecondsLeft()
		{
			var result = endTime - Performance.GetSecondsSinceStart();
			return result < 0 ? 0 : result;
		}
		public float GetStartTime()
		{
			return startTime;
		}
		public float GetOccurTime()
		{
			return endTime;
		}
		public void Delete()
		{

		}
	}
	public class Timer
	{
		private Signal signal;
		private string uniqueName;
		private int repeats;
		private float tickSeconds;

		public Timer(string uniqueName, float tickTimeInSeconds = 1, int repeats = 1000000)
		{
			signal = new Signal(uniqueName, 0);
			this.uniqueName = uniqueName;
			this.repeats = repeats;
			tickSeconds = tickTimeInSeconds;
		}
		public void SetTickTime(float tickTimeInSeconds)
		{
			tickSeconds = tickTimeInSeconds;
		}
		public void SetRepeats(int repeats)
		{
			this.repeats = repeats;
		}
		public float GetSeconds()
		{
			return GetRepeatCount() * signal.GetSecondsDelay();
		}
		public float GetSecondsLeft()
		{
			var repeats = GetRepeats();
			var delay = signal.GetSecondsDelay();
			var seconds = GetSeconds();
			return repeats * delay - seconds;
		}
		public int GetRepeatCount()
		{
			return Gate.GetEntriesCount(uniqueName);
		}
		public int GetRepeats()
		{
			return repeats;
		}
		public void Restart()
		{
			Gate.RemoveEntries(uniqueName);
		}
		public void Delete()
		{

		}
	}
	/// <summary>
	/// Controls <see cref="float"/> in different ways.
	/// </summary>
	public static class Number
	{
		public static float GetUnsigned(float number)
		{
			return Math.Abs(number);
		}
		public static float GetAveraged(float[] numbers, bool arrayIsNullError = true, bool arrayIsEmptyError = true)
		{
			var numbersStr = numbers == null ? "null" : $"float[{numbers.Length}]";
			var parameters = $"Parameters:\n" +
				$"{nameof(numbers)} = {numbersStr}\n" +
				$"{nameof(arrayIsNullError)} = {arrayIsNullError.ToString().ToLower()}\n" +
				$"{nameof(arrayIsEmptyError)} = {arrayIsEmptyError.ToString().ToLower()}";
			if (IsNullError(parameters, $"{nameof(numbers)} array", numbers, arrayIsNullError, 1)) return default;
			if (ArrayIsEmptyError(parameters, numbers, "float array", $"{nameof(numbers)}", arrayIsEmptyError, 1)) return default;

			return numbers.Sum() / numbers.Length;
		}
		public static float GetRandomized(float lowerBound, float upperBound, int precision = 0)
		{
			precision = (int)GetLimited(precision, 0, 5);
			if (lowerBound > upperBound)
			{
				var swap = lowerBound;
				lowerBound = upperBound;
				upperBound = swap;
			}
			var precisionValue = (float)Math.Pow(10, precision);
			var lowerInt = Convert.ToInt32(lowerBound * Math.Pow(10, GetPrecision(lowerBound)));
			var upperInt = Convert.ToInt32(upperBound * Math.Pow(10, GetPrecision(upperBound)));
			var randInt = new Random(Guid.NewGuid().GetHashCode()).Next((int)(lowerInt * precisionValue), (int)(upperInt * precisionValue) + 1);
			var result = randInt / precisionValue;

			return result;
		}
		public static float GetRounded(float number, int precision = 0, RoundNumberToward roundToward = RoundNumberToward.Closest, RoundNumberPrio roundPriority = RoundNumberPrio.TowardEven)
		{
			var midpoint = (MidpointRounding)roundPriority;
			precision = (int)GetLimited(precision, 0, 5);

			if (roundToward == RoundNumberToward.Down || roundToward == RoundNumberToward.Up)
			{
				var numStr = number.ToString();
				var prec = GetPrecision(number);
				if (prec > 0 && prec > precision)
				{
					var digit = roundToward == RoundNumberToward.Down ? "1" : "9";
					numStr = numStr.Remove(numStr.Length - 1);
					numStr = $"{numStr}{digit}";
					number = float.Parse(numStr);
				}
			}

			return MathF.Round(number, precision, midpoint);
		}
		public static float GetLimited(float number, float minimum, float maximum)
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
		public static float GetPercentedTowardTarget(float number, float targetNumber, float percent)
		{
			var vec = new Vector2(number, 0);
			var targetVec = new Vector2(targetNumber, 0);
			var result = Vector2.Lerp(vec, targetVec, percent / 100);

			return result.X;
		}
		public static float GetChanged(float number, float speed, Motion motion = Motion.PerSecond)
		{
			if (motion == Motion.PerSecond)
			{
				speed *= ticksDeltaTime;
			}
			return number + speed;
		}
		public static float GetTowardTarget(float number, float targetNumber, float numbersPerSecond)
		{
			if (number <= targetNumber && targetNumber * ticksDeltaTime < 0) return targetNumber;
			else if (number >= targetNumber && targetNumber * ticksDeltaTime > 0) return targetNumber;
			return GetChanged(number, numbersPerSecond);
		}
		public static float GetTimeConverted(float time, NumberTimeConvertType convertType)
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
		public static float[] GetFromText(string text, bool invalidTextError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(text)} = {text}\n" +
				$"{nameof(invalidTextError)} = {invalidTextError.ToString().ToLower()}";
			var result = 0f;
			text = text.Replace(',', '.');
			var parsed = float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			if (parsed)
			{
				return new float[] { result };
			}
			if (invalidTextError)
			{
				Error($"{parameters}\n\n{GetInvalidValueError(nameof(text), text)}\n\nTip:\nMake sure it's a number.", 1);
			}
			return new float[0];
		}
		public static int GetPrecision(float number)
		{
			var result = 0;
			var numberStr = number.ToString();
			var hasDot = numberStr.Contains('.');
			var hasComma = numberStr.Contains(',');

			if (hasDot || hasComma) result = number.ToString().Split(hasDot ? '.' : ',')[1].Length;
			return result;
		}
		public static bool HasChance(float percent)
		{
			percent = GetLimited(percent, 0, 100);
			var n = GetRandomized(1, 100, 0);
			return n <= percent;
		}
		public static bool IsBetween(float lowerBound, float number, float upperBound,
			bool inclusiveLower = true, bool inclusiveUpper = true)
		{
			var lower = false;
			var upper = false;
			if (inclusiveLower) lower = lowerBound <= number;
			else lower = lowerBound < number;
			if (inclusiveUpper) upper = upperBound >= number;
			else upper = upperBound > number;

			return lower && upper;
		}
	}
	/// <summary>
	/// Controls <see cref="string"/> in different ways.
	/// </summary>
	public static class Text
	{
		/// <summary>
		/// Converts an <paramref name="array"/> into a <see cref="string"/>. The elements are separated by a <paramref name="separator"/>. Then the <see cref="string"/> is returned.
		/// </summary>
		public static string GetFromArray<T>(T[] array, string separator = ", ", bool arrayIsNullError = true, bool arrayIsEmptyError = true)
		{
			var arrayStr = array == null ? "null" : $"{array}";
			var parameters = $"Parameters:\n" +
				$"{nameof(array)} = {arrayStr}\n" +
				$"{nameof(separator)} = \"{separator}\"\n" +
				$"{nameof(arrayIsNullError)} = {arrayIsNullError.ToString().ToLower()}\n" +
				$"{nameof(arrayIsEmptyError)} = {arrayIsEmptyError.ToString().ToLower()}";
			var result = "";

			if (IsNullError(parameters, "array", array, arrayIsNullError, 1)) return default;
			if (ArrayIsEmptyError(parameters, array, "array", $"{array}", arrayIsEmptyError, 1)) return default;

			for (int i = 0; i < array.Length; i++)
			{
				result = result.Insert(result.Length, $"{array[i]}");
				if (i == array.Length - 1) break;
				result = result.Insert(result.Length, $"{separator}");
			}
			return result;
		}

		/// <summary>
		/// Adds <paramref name="text"/> to the clipboard (copies it). It can be accessed later via <typeparamref name="Ctrl"/> + <typeparamref name="V"/> or <see cref="ClipboardGet"/>.
		/// </summary>
		public static void Copy(string text, bool textIsNullError = true)
		{
			var textStr = text == null ? "null" : $"\"{text}\"";
			var parameters = $"Parameters:\n" +
				$"{nameof(text)} = {textStr}\n" +
				$"{nameof(textIsNullError)} = {textIsNullError.ToString().ToLower()}";

			if (IsNullError(parameters, nameof(text), text, textIsNullError, 1)) return;

			Clipboard.SetText(text);
		}
		/// <summary>
		/// Gets the clipboard data and returns it if it is a <see cref="string"/>, otherwise returns <paramref name="null"/>.‪‪
		/// </summary>
		public static string GetClipboard()
		{
			var result = Clipboard.GetText();
			return result == string.Empty ? null : result;
		}

		/// <summary>
		/// Returns a new <see cref="string"/> after a simple encryption on <paramref name="text"/> with a <paramref name="key"/> that can be <paramref name="performedtwice"/>.‪‪ The encryption can be decrypred later and the text can be retrieved back with <see cref="DecryptedGet"/>.
		/// </summary>
		public static string GetEncrypted(string text, char key, bool performedTwice = false)
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
		public static string GetDecrypted(string encryptedText, char key, bool performedTwice = false)
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
		/// Converts <typeparamref name="T"/> <paramref name="data"/> into a <see cref="string"/> (<paramref name="JSON"/>) and returns it. It can be converted and retrieved back to <typeparamref name="T"/> <paramref name="data"/> later with <see cref="GetFromData"/>.‪‪
		/// </summary>
		public static string GetFromData<T>(T data)
		{
			return JsonConvert.SerializeObject(data);
		}
		/// <summary>
		/// Converts an already formatted <paramref name="text"/> (<paramref name="JSON"/>) into <typeparamref name="T"/> <paramref name="data"/> and returns it if the <paramref name="text"/> is in the correct format. Otherwise returns <paramref name="default"/>(<typeparamref name="T"/>).
		/// </summary>
		public static T GetData<T>(string text)
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
		/// Reads the text from the file at <paramref name="filePath"/> with <paramref name="fileName"/> and <paramref name="fileExtension"/> and returns it as a <see cref="string"/> if successful. Returns <paramref name="null"/> otherwise. A text can be saved to a file with <see cref="Save"/>.<br></br><br></br>
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
		public static void Display(string font, object message, float scale = 1, bool overwrite = true)
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
			scale = Number.GetLimited(scale, 0.001f, 5000);
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
		public static void ClearDisplay()
		{
			textDisplayMessage = null;
			render = true;
		}

		public static string GetFormattedTime(float seconds, string separator = ":", bool msShow = false, string msFormat = "ms", bool secShow = true, string secFormat = "s", bool minShow = true, string minFormat = "m", bool hrShow = true, string hrFormat = "h")
		{
			seconds = Number.GetUnsigned(seconds);
			var secondsStr = seconds.ToString();
			var ms = 0;
			if (secondsStr.Contains('.'))
			{
				var spl = secondsStr.Split('.');
				ms = int.Parse(spl[1]) * 100;
				seconds = Number.GetRounded(seconds, roundToward: RoundNumberToward.Down);
			}
			var sec = seconds % 60;
			var min = Number.GetRounded(seconds / 60 % 60, roundToward: RoundNumberToward.Down);
			var hr = Number.GetRounded(seconds / 3600, roundToward: RoundNumberToward.Down);
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
		public static void SetTargetTicksPerSecond(float tps, bool limited, bool vSynced)
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
		public static float GetTicksPerSecond(bool average = false)
		{
			return average ? tpsAverage : tps;
		}
		/// <summary>
		/// - Gets the current target tick rate and returns it. <br></br><br></br>
		/// - The target tick speed can be changed via <see cref="TicksPerSecondTargetSet"/>. Also contains information about ticks/frames.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. The current frames per second can be checked with <see cref="FramesTotalPerSecondGet"/>.<br></br><br></br>
		/// </summary>
		public static float GetTargetTicksPerSecond()
		{
			return 60 / ((float)game.TargetElapsedTime.TotalSeconds * 60);
		}
		/// <summary>
		/// - Checks wether the tick rate is limited to the target tick rate and returns the result.<br></br><br></br>- The limitation of the tick speed and other related changes can be set through<br></br> <see cref="TicksPerSecondTargetSet"/>. Also contains information about ticks/frames.
		/// </summary>
		public static bool TicksPerSecondAreLimited()
		{
			return game.IsFixedTimeStep;
		}
		/// <summary>
		/// - Checks wether the tick rate is limited by the user's monitor refresh rate and returns the result.<br></br><br></br>
		/// - The vertical synchronization and other related changes can set through<br></br> <see cref="TicksPerSecondTargetSet"/>. Also contains information about ticks/frames.<br></br><br></br>
		/// </summary>
		public static bool TicksPerSecondAreVSynced()
		{
			return graphics.SynchronizeWithVerticalRetrace;
		}
		/// <summary>
		/// - Gets the number of ticks that have passed since the start and returns them.<br></br><br></br>
		/// - The tick count is also provided as an <see cref="int"/> parameter with <see cref="Program.EachTick(int)"/>.<br></br><br></br>
		/// - Changing the tick speed and receiving information about ticks/frames may be done through <see cref="TicksPerSecondTargetSet"/>.
		/// </summary>
		public static int GetTickCount()
		{
			return tick;
		}

		public static float GetGigaBytesAvailableRAM()
		{
			return ramAvailable.NextValue() / 1000;
		}
		public static float GetPercentUsedRAM()
		{
			return ramUsedPercent.NextValue();
		}

		/// <summary>
		/// - Gets the current frame rate that can be an <paramref name="average"/> of the previous 60 ticks and returns it.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. A lower frame rate will be present with slow tick rate and vice versa. The targeted tick rate can be changed or uncapped with <see cref="TicksPerSecondTargetSet"/>. Also contains information about ticks/frames.
		/// </summary>
		public static float GetTotalFramesPerSecond(bool average = false)
		{
			return average ? fpsAverage : fps;
		}
		/// <summary>
		/// - Gets the number of frames that have passed since the start and returns them. This counter is not affected by rendering. Therefore a frame might be skipped and the counter will still increment. <br></br><br></br>- Rendered frames counter can be checked with <see cref="FramesRenderedCountGet"/>.<br></br><br></br>
		/// - Changing or uncapping the tick rate and receiving information about ticks/frames can be done through <see cref="TicksPerSecondTargetSet"/>. This affects the frame rate.
		/// </summary>
		public static int GetTotalFrameCount()
		{
			return frame;
		}
		/// <summary>
		/// - Gets the number of rendered frames that have passed since the start and returns them. Rendered frames happen only when the current frame is different than the last frame. Therefore frames are skipped when the screen is static.<br></br><br></br>
		/// - A check for the total frames counter can be done through <see cref="FramesTotalCountGet"/>.<br></br><br></br>
		/// - Changing or uncapping the tick rate and receiving information about ticks/frames can be done through <see cref="TicksPerSecondTargetSet"/>. This affects the frame rate.
		/// </summary>
		public static int GetRenderedFrameCount()
		{
			return frameRendered;
		}

		/// <summary>
		/// - Gets the time that has passed since the start and returns it.
		/// </summary>
		public static float GetSecondsSinceStart()
		{
			return time;
		}
		/// <summary>
		/// - Gets the time that has passed since the last tick and returns it. <br></br><br></br>- The target tick rate can be changed via <see cref="TicksPerSecondTargetSet"/><br></br>- The current target tick rate can be checked with <see cref="TimeSinceLastTickTargetGet"/>.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. The time since last frame can be checked with <see cref="TimeSinceLastFrameGet"/>.
		/// </summary>
		public static float GetSecondsSinceLastTick()
		{
			return ticksDeltaTime;
		}
		/// <summary>
		/// - Gets the target time between ticks and returns it.<br></br><br></br>- The target tick rate can be changed via <see cref="TicksPerSecondTargetSet"/>.<br></br>- The current tick rate can be checked with <see cref="TimeSinceLastTickGet"/>.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. The time since last frame can be checked with <see cref="TimeSinceLastFrameGet"/>.
		/// </summary>
		public static float GetTargetSecondsSinceLastTick()
		{
			return (float)game.TargetElapsedTime.TotalSeconds;
		}
		/// <summary>
		/// - Gets the time that has passed since the last frame and returns it.<br></br><br></br>
		/// - The frame rate is tied to the tick rate but they are not the same. The time since last tick can be checked with <see cref="TimeSinceLastTickGet"/>.
		/// </summary>
		public static float GetSecondsSinceLastFrame()
		{
			return framesDeltaTime;
		}
	}
	public static class Hardware
	{
		public static Size GetScreenSize()
		{
			return screenSize;
		}
		/// <summary>
		/// - When <paramref name="activated"/> the user's computer will stay active at all times, even when left idle.<br></br><br></br>
		/// - A check wether sleep prevention is activated can be done via <see cref="ComputerSleepPreventionIsActivatedCheck"/>.
		/// </summary>
		public static void PreventComputerSleep(bool prevented)
		{
			sleepPrevented = prevented;
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
		public static bool ComputerSleepIsPrevented()
		{
			return sleepPrevented;
		}
	}
	public static class Network
	{
		public static void LogMessagesToConsole(bool log)
		{
			networkLogMessagesToConsole = log;
		}
		public static bool MessagesAreLoggedToConsole()
		{
			return networkLogMessagesToConsole;
		}

		public static int GetClientCount()
		{
			return clientUniqueNames.Count;
		}

		public static string GetSameDeviceIP()
		{
			return "127.0.0.1";
		}
		public static void StartServer()
		{
			var method = Debug.GetCodeMethodName();
			try
			{
				if (serverIsRunning)
				{
					consoleLog = $"{consoleLog}\n{method}: Server is already starting/started.";
					ConsoleUpdate();
					return;
				}
				if (clientIsConnected)
				{
					consoleLog = $"{consoleLog}\n{method}: Cannot start a server while a client.";
					ConsoleUpdate();
					return;
				}
				server = new Server(IPAddress.Any, serverPort);
				server.Start();
				consoleLog = $"{consoleLog}\n{method}: Started a LAN Server on port {serverPort}.";

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
				consoleLog = $"{consoleLog}\n{method} Error: {ex.Message}";
				ConsoleUpdate();
				return;
			}
		}
		public static void StopServer()
		{
			var method = Debug.GetCodeMethodName();
			try
			{
				if (serverIsRunning == false)
				{
					consoleLog = $"{consoleLog}\n{method}: Server is not running.";
					ConsoleUpdate();
					return;
				}
				if (clientIsConnected)
				{
					consoleLog = $"{consoleLog}\n{method}: Cannot stop a server while a client.";
					ConsoleUpdate();
					return;
				}
				serverIsRunning = false;
				server.Stop();
				consoleLog = $"{consoleLog}\n{method}: The LAN Server on port {serverPort} was stopped.";
				ConsoleUpdate();
			}
			catch (Exception ex)
			{
				serverIsRunning = false;
				consoleLog = $"{consoleLog}\n{method} Error: {ex.Message}";
				ConsoleUpdate();
				return;
			}
		}
		public static void SendServerMessageToAllClients(string message)
		{
			var method = Debug.GetCodeMethodName();
			if (ServerCannotSendMessage(method)) return;

			ServerMessageSent(method);
			server.Multicast($"~{(int)MessageType.ServerMessageToAll}|{message}");
		}
		public static void SendServerMessageToClient(string receiverUniqueName, string message)
		{
			var method = Debug.GetCodeMethodName();
			if (ServerCannotSendMessage(method)) return;

			ServerMessageSent(method);
			server.Multicast($"~{(int)MessageType.ServerMessageToClient}|{receiverUniqueName}|{message}");
		}
		public static bool ServerIsRunning()
		{
			return serverIsRunning;
		}

		public static void ConnectClient(string uniqueName, string ip)
		{
			uniqueName = uniqueName.Trim();
			var method = Debug.GetCodeMethodName();
			var parameters = $"Parameters:\n" +
				$"{nameof(uniqueName)} = \"{uniqueName}\"\n" +
				$"{nameof(ip)} = \"{ip}\"";
			if (clientIsConnected)
			{
				consoleLog = $"{consoleLog}\n{method}: Already connecting/connected.";
				ConsoleUpdate();
				return;
			}
			if (serverIsRunning)
			{
				consoleLog = $"{consoleLog}\n{method}: Cannot connect as a client while a server.";
				ConsoleUpdate();
				return;
			}
			if (uniqueName == null || uniqueName == "")
			{
				consoleLog = $"{consoleLog}\n{method}: Clients' unique names cannot be null or empty.";
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
				Error($"{method}\n\n{GetInvalidValueError(nameof(ip), ip)}", 1);
				return;
			}
			clientUniqueName = uniqueName;
			consoleLog = $"{consoleLog}\n{parameters}: Connecting to {ip}:{serverPort}...";
			ConsoleUpdate();
			client.ConnectAsync();

		}
		public static void DisconnectClient()
		{
			var method = Debug.GetCodeMethodName();
			if (clientIsConnected == false)
			{
				consoleLog = $"{consoleLog}\n{method}: Cannot disconnect when not connected.";
				ConsoleUpdate();
				return;
			}
			client.DisconnectAndStop();
		}
		public static bool ClientIsConnected()
		{
			return clientIsConnected;
		}
		public static string GetClientUniqueName()
		{
			return clientIsConnected ? clientUniqueName : default;
		}
		public static void SendClinetMessageToAllClients(string message)
		{
			var method = Debug.GetCodeMethodName();
			if (ClientCannotSendMessage(method)) return;

			ClientMessageSent(method);
			client.SendAsync($"~{(int)MessageType.ClientMessageToAll}|{clientUniqueName}|{message}");
		}
		public static void SendClinetMessageToClient(string receiverUniqueName, string message)
		{
			var method = Debug.GetCodeMethodName();
			if (clientUniqueName == receiverUniqueName) return;
			if (ClientCannotSendMessage(method)) return;

			ClientMessageSent(method);
			client.SendAsync($"~{(int)MessageType.ClientMessageToClient}|{clientUniqueName}|{receiverUniqueName}|{message}");
		}
		public static void SendClientMessageToServer(string message)
		{
			var method = Debug.GetCodeMethodName();
			if (ClientCannotSendMessage(method)) return;

			ClientMessageSent(method);
			client.SendAsync($"~{(int)MessageType.ClientMessageToServer}|{clientUniqueName}|{message}");
		}
		public static void SendClientMessageToServerAndAllClients(string message)
		{
			var method = Debug.GetCodeMethodName();
			if (ClientCannotSendMessage(method)) return;

			ClientMessageSent(method);
			client.SendAsync($"~{(int)MessageType.ClientMessageToAllAndServer}|{clientUniqueName}|{message}");
		}

		private static bool ClientCannotSendMessage(string method)
		{
			if (MessageDisconnected(method)) return true;
			else if (serverIsRunning)
			{
				if (networkLogMessagesToConsole == false) return true;
				consoleLog = $"{consoleLog}\n{method}: Cannot send a client message while a server.";
				ConsoleUpdate();
				return true;
			}
			return false;
		}
		private static void ClientMessageSent(string method)
		{
			if (networkLogMessagesToConsole == false) return;
			consoleLog = $"{consoleLog}\n{method}: Sent.";
			ConsoleUpdate();
		}
		private static bool ServerCannotSendMessage(string method)
		{
			if (MessageDisconnected(method)) return true;
			else if (clientIsConnected)
			{
				if (networkLogMessagesToConsole == false) return true;
				consoleLog = $"{consoleLog}\n{method}: Cannot send a server message while a client.";
				ConsoleUpdate();
				return true;
			}
			return false;
		}
		private static void ServerMessageSent(string method)
		{
			if (networkLogMessagesToConsole == false) return;
			consoleLog = $"{consoleLog}\n{method}: Sent.";
			ConsoleUpdate();
		}
		private static bool MessageDisconnected(string method)
		{
			if (clientIsConnected == false && serverIsRunning == false)
			{
				if (networkLogMessagesToConsole == false) return true;
				consoleLog = $"{consoleLog}\n{method}: Cannot send a message while disconnected.";
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

		private static void _SetPosition(Point position)
		{
			cameraPosition = position;
			render = true;

			foreach (var body in bodiesAll)
			{
				UpdateCameraBodyTransform(body);
			}
		}
		public static void SetPosition(Point position)
		{
			_SetPosition(position);
		}
		public static void SetPositionXY(float x, float y)
		{
			_SetPosition(new Point(x, y));
		}
		public static void SetPositionX(float x)
		{
			_SetPosition(new Point(x, cameraPosition.GetY()));
		}
		public static void SetPositionY(float y)
		{
			_SetPosition(new Point(cameraPosition.GetX(), y));
		}
		public static Point GetPosition()
		{
			return cameraPosition;
		}

		public static void _SetAngle(Angle angle)
		{
			cameraAngle = angle.GetA();
			render = true;

			foreach (var body in bodiesAll)
			{
				UpdateCameraBodyTransform(body);
			}
		}
		public static void SetAngle(Angle angle)
		{
			_SetAngle(angle);
		}
		public static void SetAngleA(float a)
		{
			_SetAngle(new Angle(a));
		}
		public static Angle GetAngle()
		{
			return new Angle(cameraAngle);
		}
	}
	/// <summary>
	/// - Holds information about the current input of the user.
	/// </summary>
	public static class Input
	{
		public static Point GetMouseCursorPosition(bool canvas = true)
		{
			var result = new Point();
			if (canvas)
			{
				var scale = new Point(canvasSize.GetW() / screenSize.GetW(), canvasSize.GetH() / screenSize.GetH());
				var cameraOffset = new Point(canvasSize.GetW() / 2, canvasSize.GetH() / 2);

				result = new Point(Mouse.GetState().Position.X, Mouse.GetState().Position.Y) * scale;
				return result - cameraOffset + Camera.GetPosition();
			}
			return result + Camera.GetPosition();
		}
		public static void DisplayMouseCursor(bool displayed)
		{
			game.IsMouseVisible = displayed;
		}
		public static bool MouseCursorIsDisplayed()
		{
			return game.IsMouseVisible;
		}
		public static bool LeftMouseButtonIsPressed()
		{
			return Mouse.GetState().LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
		}
		public static bool MiddleMouseButtonIsPressed()
		{
			return Mouse.GetState().MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
		}
		public static bool RightMouseButtonIsPressed()
		{
			return Mouse.GetState().RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
		}
		public static void SetMouseCursorFromSprite(string spritePath, int originX, int originY)
		{
			if (spritePath == null || sprites.ContainsKey(spritePath) == false) return;

			Microsoft.Xna.Framework.Input.Mouse.SetCursor(
				MouseCursor.FromTexture2D(sprites[spritePath], originX, originY));
		}

		public static string GetTextFromKey(Key key)
		{
			var shift = KeyIsPressed(Key.ShiftLeft) || KeyIsPressed(Key.ShiftRight);
			var result = "";
			switch (key)
			{
				case Key.Space: result = " "; break;
				case Key._0: result = shift ? ")" : "0"; break;
				case Key._1: result = shift ? "!" : "1"; break;
				case Key._2: result = shift ? "@" : "2"; break;
				case Key._3: result = shift ? "#" : "3"; break;
				case Key._4: result = shift ? "$" : "4"; break;
				case Key._5: result = shift ? "%" : "5"; break;
				case Key._6: result = shift ? "^" : "6"; break;
				case Key._7: result = shift ? "&" : "7"; break;
				case Key._8: result = shift ? "*" : "8"; break;
				case Key._9: result = shift ? "(" : "9"; break;
				case Key.A: result = "a"; break;
				case Key.B: result = "b"; break;
				case Key.C: result = "c"; break;
				case Key.D: result = "d"; break;
				case Key.E: result = "e"; break;
				case Key.F: result = "f"; break;
				case Key.G: result = "g"; break;
				case Key.H: result = "h"; break;
				case Key.I: result = "i"; break;
				case Key.J: result = "j"; break;
				case Key.K: result = "k"; break;
				case Key.L: result = "l"; break;
				case Key.M: result = "m"; break;
				case Key.N: result = "n"; break;
				case Key.O: result = "o"; break;
				case Key.P: result = "p"; break;
				case Key.Q: result = "q"; break;
				case Key.R: result = "r"; break;
				case Key.S: result = "s"; break;
				case Key.T: result = "t"; break;
				case Key.U: result = "u"; break;
				case Key.V: result = "v"; break;
				case Key.W: result = "w"; break;
				case Key.X: result = "x"; break;
				case Key.Y: result = "y"; break;
				case Key.Z: result = "z"; break;
				case Key.Num0: result = "0"; break;
				case Key.Num1: result = "1"; break;
				case Key.Num2: result = "2"; break;
				case Key.Num3: result = "3"; break;
				case Key.Num4: result = "4"; break;
				case Key.Num5: result = "5"; break;
				case Key.Num6: result = "6"; break;
				case Key.Num7: result = "7"; break;
				case Key.Num8: result = "8"; break;
				case Key.Num9: result = "9"; break;
				case Key.NumMultiply: result = "*"; break;
				case Key.NumAdd: result = "+"; break;
				case Key.NumSubtract: result = "-"; break;
				case Key.NumDecimal: result = "."; break;
				case Key.NumDivide: result = "/"; break;
				case Key.Semicolon: result = shift ? ":" : ";"; break;
				case Key.Equals: result = shift ? "+" : "="; break;
				case Key.Comma: result = shift ? "<" : ","; break;
				case Key.MinusDash: result = shift ? "" : "-"; break;
				case Key.Dot: result = shift ? ">" : "."; break;
				case Key.Slash: result = shift ? "?" : "/"; break;
				case Key.GraveAccent: result = shift ? "~" : "`"; break;
				case Key.SquareBracketOpen: result = shift ? "{" : "["; break;
				case Key.Backslash: result = shift ? "|" : "\\"; break;
				case Key.SquareBracketClose: result = shift ? "}" : "]"; break;
				case Key.Quote: result = shift ? "\"" : "'"; break;
				default: result = null; break;
			}
			result = shift && result != null ? result.ToUpper() : result;
			return result;
		}
		public static Key[] GetPressedKeys()
		{
			return keysPressed.ToArray();
		}
		public static bool KeyIsPressed(Key key)
		{
			return keysPressed.Contains(key);
		}

		/*
		public static bool IsPressHolding(string name, bool condition, float secondsDelay = 0.5f, float updatesPerSecond = 0.1f)
		{
			if (Gate.IsOpened($"{name}-gate", condition))
			{
				Signal.Set(name, secondsDelay);
				return true;
			}
			else if (Timer.IsIntervalOccuring(name, updatesPerSecond)) return condition;
			return false;
		}
		*/
	}
	public static class Gate
	{
		public static int GetEntriesCount(string name)
		{
			return name != null && gateEntriesCount.ContainsKey(name) ? gateEntriesCount[name] : 0;
		}
		public static void RemoveEntries(string name)
		{
			if (name == null || gateEntriesCount.ContainsKey(name) == false) return;
			gateEntriesCount[name] = default;
		}
		public static void Close(string name)
		{
			if (name == null || gates.ContainsKey(name) == false) return;
			gates.Remove(name);
		}
		public static bool IsOpened(string name, bool condition, int maxEntries = int.MaxValue)
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
	/*
	public static class Signal
	{
		public static void Set(string name, float secondsDelay)
		{
			if (name == null) return;
			secondsDelay = Number.GetLimited(secondsDelay, 0, float.MaxValue);
			signalpauses[name] = false;
			signalStartTimes[name] = Performance.GetTime();
			signalDelays[name] = secondsDelay;
			signalEndTimes[name] = Performance.GetTime() + secondsDelay;
		}
		public static bool Exists(string name)
		{
			return name != null && signalStartTimes.ContainsKey(name);
		}
		public static float GetSecondsDelay(string name)
		{
			return name != null && signalDelays.ContainsKey(name) ? signalDelays[name] : 0;
		}
		public static void Pause(string name, bool paused)
		{
			if (name == null || signalpauses.ContainsKey(name) == false) return;
			signalpauses[name] = paused;
		}
		public static float GetSecondsLeft(string name)
		{
			if (name == null || signalEndTimes.ContainsKey(name) == false) return 0;
			var result = signalEndTimes[name] - Performance.GetTime();
			return result < 0 ? 0 : result;
		}
		public static float GetTimeStart(string name)
		{
			return name != null && signalStartTimes.ContainsKey(name) ? signalStartTimes[name] : 0;
		}
		public static float GetTimeOccur(string name)
		{
			return name != null && signalEndTimes.ContainsKey(name) ? signalEndTimes[name] : 0;
		}
		public static void Delete(string name)
		{
			if (name == null) return;
			if (signalEndTimes.ContainsKey(name)) signalEndTimes.Remove(name);
			if (signalpauses.ContainsKey(name)) signalpauses.Remove(name);
			if (signalStartTimes.ContainsKey(name)) signalStartTimes.Remove(name);
			if (signalDelays.ContainsKey(name)) signalDelays.Remove(name);
		}
	}
	*/
	/*
	public static class Timer
	{
		public static void Set(string name, float intervalsInSeconds = 1, int repeats = 1000000)
		{
			Signal.Set(name, 0);
			timerRepeats[name] = repeats;
			timerTickSeconds[name] = intervalsInSeconds;
		}
		public static void SetTickTime(string name, float intervalsInSeconds = 1)
		{
			timerTickSeconds[name] = intervalsInSeconds;
		}
		public static void SetRepeats(string signalName, int repeats = 1000000)
		{
			timerRepeats[signalName] = repeats;
		}
		public static float GetSeconds(string name)
		{
			return GetRepeatCount(name) * Signal.GetSecondsDelay(name);
		}
		public static float GetSecondsLeft(string name)
		{
			var repeats = GetRepeats(name);
			var delay = Signal.GetSecondsDelay(name);
			var seconds = GetSeconds(name);
			return repeats * delay - seconds;
		}
		public static int GetRepeatCount(string name)
		{
			return Gate.GetEntriesCount(name);
		}
		public static int GetRepeats(string name)
		{
			return name != null && timerRepeats.ContainsKey(name) ? timerRepeats[name] : 0;
		}
		public static void Restart(string name)
		{
			Gate.RemoveEntries(name);
		}
	}
	*/
	public static class Console
	{
		public static void Display()
		{
			consoleShown = true;
			AllocConsole();
			ConsoleUpdate();
		}
		public static bool IsDisplayed()
		{
			return consoleShown;
		}
		public static string GetInput()
		{
			return System.Console.ReadLine();
		}
		public static void Log(string message)
		{
			consoleLog = $"{consoleLog}{message}";
			ConsoleUpdate();
		}
		public static void Clear()
		{
			consoleLog = "";
			ConsoleUpdate();
		}
	}
	public static class Debug
	{
		public static void SetUserErrorMessage(string message, bool messageIsNullError = true)
		{
			var messageStr = message == null ? "null" : $"\"{message}\"";
			var parameters = $"Parameters:\n" +
				$"{nameof(message)} = {messageStr}\n" +
				$"{nameof(messageIsNullError)} = {messageIsNullError.ToString().ToLower()}";

			if (IsNullError(parameters, nameof(message), message, messageIsNullError, 1)) return;

			userErrorMessage = message;
		}
		public static string GetUserErrorMessage()
		{
			return userErrorMessage;
		}

		public static bool IsActivated()
		{
			return Debugger.IsAttached;
		}

		public static int GetCodeLine(int index = 0)
		{
			var info = new StackFrame(index + 1, true);
			var a = info.GetFileLineNumber();
			return a == eachTickLineCall ? 0 : a;
		}
		public static string GetCodeFilePath(int index = 0)
		{
			var info = new StackFrame(index + 1, true);
			var a = info.GetFileName();
			return a;
		}
		public static string GetCodeFileName(int index = 0)
		{
			var pathRaw = GetCodeFilePath(index + 1);
			if (pathRaw == null) return null;
			var path = pathRaw.Split('\\');
			var name = path[path.Length - 1].Split('.');
			return name[0] == "Gear" ? null : name[0];
		}
		public static string GetCodeMethodName(int index = 0)
		{
			var info = new StackFrame(index + 1, true);
			var ignoredCases = new List<string>()
			{
				"Update", "DoUpdate", "Tick", "TickOnIdle", "Interop.Mso.IMsoComponent.FDoIdle", "Interop.Mso.IMsoComponentManager.FPushMessageLoop", "RunMessageLoopInner", "RunMessageLoop", "Run", "RunLoop", "Main"
			};
			var method = info.GetMethod();
			var fullName = method.ReflectedType.FullName.Replace('+', '.');
			if (method == null) return null;
			return ignoredCases.Contains(method.Name) ? null : $"{fullName}.{method.Name}";
		}
	}
	/// <summary>
	/// Controls Sounds and holds information about them.
	/// </summary>
	public static class Sound
	{
		public static string[] GetAllUniqueNames()
		{
			return sounds.Keys.ToArray();
		}

		public static void Play(string uniqueName, float volumePercent = 50, float pitchPercent = 50,
			float centerPercent = 50, bool loop = false, bool ableToPlayOverSelf = false, bool soundNameIsNullError = true, bool soundNotFoundError = true)
		{
			var nameStr = uniqueName == null ? "null" : $"\"{uniqueName}\"";
			var parameters = $"Parameters:\n" +
				$"{nameof(uniqueName)} = {nameStr}\n" +
				$"{nameof(volumePercent)} = {volumePercent}\n" +
				$"{nameof(pitchPercent)} = {pitchPercent}\n" +
				$"{nameof(centerPercent)} = {centerPercent}\n" +
				$"{nameof(loop)} = {loop.ToString().ToLower()}\n" +
				$"{nameof(ableToPlayOverSelf)} = {ableToPlayOverSelf.ToString().ToLower()}\n" +
				$"{nameof(soundNotFoundError)} = {soundNotFoundError.ToString().ToLower()}";
			if (IsNullError(parameters, nameof(uniqueName), uniqueName, soundNameIsNullError, 1)) return;
			if (sounds.ContainsKey(uniqueName) == false)
			{
				if (soundNotFoundError)
				{
					Error($"{parameters}\n\n{GetContentNotFoundError("sound", uniqueName)}", 1);
				}
				return;
			}

			if (muteSound) return;

			volumePercent = Number.GetLimited(volumePercent, 0, 100);
			pitchPercent = Number.GetLimited(pitchPercent, 0, 100);
			centerPercent = Number.GetLimited(centerPercent, 0, 100);
			if (ableToPlayOverSelf)
			{
				sounds[uniqueName] = soundsRaw[uniqueName].CreateInstance();
			}
			sounds[uniqueName].Pan = ((float)centerPercent * 2 - 100) / 100;
			sounds[uniqueName].IsLooped = loop;
			sounds[uniqueName].Pitch = ((float)pitchPercent * 2 - 100) / 100;
			sounds[uniqueName].Volume = (float)volumePercent / 100;
			sounds[uniqueName].Play();
		}
		public static void PauseAll(bool paused)
		{
			foreach (var kvp in sounds)
			{
				if (paused)
				{
					kvp.Value.Pause();
					continue;
				}
				kvp.Value.Resume();
			}
		}
		public static void Pause(string uniqueName, bool paused)
		{
			if (sounds.ContainsKey(uniqueName) == false)
			{
				return;
			}
			if (paused)
			{
				sounds[uniqueName].Pause();
				return;
			}
			sounds[uniqueName].Resume();
		}
		public static void StopAll()
		{
			foreach (var kvp in sounds)
			{
				kvp.Value.Stop();
			}
		}
		public static void Stop(string uniqueName)
		{
			if (sounds.ContainsKey(uniqueName) == false)
			{
				return;
			}
			sounds[uniqueName].Stop();
		}
		public static void Mute(bool muted)
		{
			muteSound = muted;
		}

		public static void PlayFromCollection(string collectionUniqueName, float volumePercent = 50, float pitchPercent = 50, float centerPercent = 50, bool loop = false, bool ableToPlayOverSelf = false, bool soundNameIsNullError = true, bool collectionNotFoundError = true, bool collectionIsEmptyError = true)
		{
			var collectionNameStr = collectionUniqueName == null ? "null" : $"\"{collectionUniqueName}\"";
			var parameters = $"Parameters:\n" +
				$"{nameof(collectionUniqueName)} = {collectionNameStr}\n" +
				$"{nameof(volumePercent)} = {volumePercent}\n" +
				$"{nameof(pitchPercent)} = {pitchPercent}\n" +
				$"{nameof(centerPercent)} = {centerPercent}\n" +
				$"{nameof(loop)} = {loop.ToString().ToLower()}\n" +
				$"{nameof(ableToPlayOverSelf)} = {ableToPlayOverSelf.ToString().ToLower()}\n" +
				$"{nameof(collectionNotFoundError)} = {collectionNotFoundError.ToString().ToLower()}\n" +
				$"{nameof(collectionIsEmptyError)} = {collectionIsEmptyError.ToString().ToLower()}";
			if (IsNullError(parameters, nameof(collectionUniqueName), collectionUniqueName, soundNameIsNullError, 1)) return;
			if (KeyNotFoundError(parameters, soundCollections, nameof(collectionUniqueName), collectionUniqueName, collectionNotFoundError, 1)) return;
			var collection = soundCollections[collectionUniqueName];
			if (ArrayIsEmptyError(parameters, collection.ToArray(), "sound collection", collectionUniqueName, collectionIsEmptyError, 1)) return;

			var randomSound = collection[(int)Number.GetRandomized(0, collection.Count - 1)];
			Play(randomSound, volumePercent, pitchPercent, centerPercent, loop, ableToPlayOverSelf);
		}
		public static void CreateCollection(string uniqueName, bool nameIsNullError = true, bool nameExistsError = true)
		{
			var uniqueNameStr = uniqueName == null ? "null" : $"\"{uniqueName}\"";
			var parameters = $"Parameters:\n" +
				$"{nameof(uniqueName)} = {uniqueNameStr}\n" +
				$"{nameof(nameExistsError)} = {nameExistsError.ToString().ToLower()}";
			if (IsNullError(parameters, $"sound collection's {nameof(uniqueName)}", uniqueName, nameIsNullError, 1)) return;
			if (KeyExistsError(parameters, soundCollections, $"sound collection's {nameof(uniqueName)}", uniqueName, nameExistsError, 1)) return;

			soundCollections[uniqueName] = new List<string>();
		}
		public static void AddToCollection(string soundUniqueName, string collectionUniqueName, bool soundNameIsNullError = true, bool collectionNameIsNullError = true, bool soundNotFoundError = true, bool collectionNotFoundError = true, bool soundAlreadyAddedError = true)
		{
			var nameStr = soundUniqueName == null ? "null" : $"\"{soundUniqueName}\"";
			var collNameStr = collectionUniqueName == null ? "null" : $"\"{collectionUniqueName}\"";
			var parameters = $"Parameters:\n" +
				$"{nameof(soundUniqueName)} = {nameStr}\n" +
				$"{nameof(collectionUniqueName)} = {collNameStr}\n" +
				$"{nameof(soundNameIsNullError)} = {soundNameIsNullError.ToString().ToLower()}\n" +
				$"{nameof(collectionNameIsNullError)} = {collectionNameIsNullError.ToString().ToLower()}\n" +
				$"{nameof(collectionNotFoundError)} = {collectionNotFoundError.ToString().ToLower()}\n" +
				$"{nameof(soundNotFoundError)} = {soundNotFoundError.ToString().ToLower()}\n" +
				$"{nameof(soundAlreadyAddedError)} = {soundAlreadyAddedError.ToString().ToLower()}";

			if (XCollectionErrorChecking(parameters, soundUniqueName, collectionUniqueName, soundNameIsNullError, collectionNameIsNullError, soundNotFoundError, collectionNotFoundError)) return;
			if (ValueAlreadyAddedError(parameters, soundCollections[collectionUniqueName], "sound", soundUniqueName, soundNotFoundError, 1, $" in the '{collectionUniqueName}' sound collection")) return;

			soundCollections[collectionUniqueName].Add(soundUniqueName);
		}
		public static void RemoveFromCollection(string soundUniqueName, string collectionUniqueName, bool soundNameIsNullError = true, bool collectionNameIsNullError = true, bool soundNotFoundError = true, bool collectionNotFoundError = true)
		{
			var nameStr = soundUniqueName == null ? "null" : $"\"{soundUniqueName}\"";
			var collNameStr = collectionUniqueName == null ? "null" : $"\"{collectionUniqueName}\"";
			var parameters = $"Parameters:\n" +
				$"{nameof(soundUniqueName)} = {nameStr}\n" +
				$"{nameof(collectionUniqueName)} = {collNameStr}\n" +
				$"{nameof(soundNameIsNullError)} = {soundNameIsNullError.ToString().ToLower()}\n" +
				$"{nameof(collectionNameIsNullError)} = {collectionNameIsNullError.ToString().ToLower()}\n" +
				$"{nameof(collectionNotFoundError)} = {collectionNotFoundError.ToString().ToLower()}\n" +
				$"{nameof(soundNotFoundError)} = {soundNotFoundError.ToString().ToLower()}";
			if (XCollectionErrorChecking(parameters, soundUniqueName, collectionUniqueName, soundNameIsNullError, collectionNameIsNullError, soundNotFoundError, collectionNotFoundError)) return;
			if (ValueNotFoundError(parameters, soundCollections[collectionUniqueName], "sound", soundUniqueName, soundNotFoundError, 1, $" in the '{collectionUniqueName}' sound collection")) return;

			soundCollections[collectionUniqueName].Remove(soundUniqueName);
		}
		public static void RemoveCollection(string uniqueName, bool nameIsNullError = true, bool nameNotFoundError = true)
		{
			var collNameStr = uniqueName == null ? "null" : $"\"{uniqueName}\"";
			var parameters = $"Parameters:\n" +
				$"{nameof(uniqueName)} = {collNameStr}\n" +
				$"{nameof(nameIsNullError)} = {nameIsNullError.ToString().ToLower()}\n" +
				$"{nameof(nameNotFoundError)} = {nameNotFoundError.ToString().ToLower()}";
			if (IsNullError(parameters, $"sound collection's {nameof(uniqueName)}", uniqueName, nameIsNullError, 1)) return;
			if (KeyNotFoundError(parameters, soundCollections, $"sound collection's {nameof(uniqueName)}", uniqueName, nameNotFoundError, 1)) return;

			soundCollections.Remove(uniqueName);
		}
		public static void RemoveAllFromCollection(string uniqueName, bool nameIsNullError = true, bool nameNotFoundError = true)
		{
			var collNameStr = uniqueName == null ? "null" : $"\"{uniqueName}\"";
			var parameters = $"Parameters:\n" +
				$"{nameof(uniqueName)} = {collNameStr}\n" +
				$"{nameof(nameIsNullError)} = {nameIsNullError.ToString().ToLower()}\n" +
				$"{nameof(nameNotFoundError)} = {nameNotFoundError.ToString().ToLower()}";
			if (IsNullError(parameters, $"sound collection's {nameof(uniqueName)}", uniqueName, nameIsNullError, 1)) return;
			if (KeyNotFoundError(parameters, soundCollections, $"sound collection's {nameof(uniqueName)}", uniqueName, nameNotFoundError, 1)) return;

			soundCollections.Clear();
		}
		public static void RemoveAllCollections()
		{
			soundCollections.Clear();
		}
		public static string[] GetAllFromCollection(string uniqueName, bool nameIsNullError = true, bool nameNotFoundError = true)
		{
			var collNameStr = uniqueName == null ? "null" : $"\"{uniqueName}\"";
			var parameters = $"Parameters:\n" +
				$"{nameof(uniqueName)} = {collNameStr}\n" +
				$"{nameof(nameIsNullError)} = {nameIsNullError.ToString().ToLower()}\n" +
				$"{nameof(nameNotFoundError)} = {nameNotFoundError.ToString().ToLower()}";
			if (IsNullError(parameters, $"sound collection's {nameof(uniqueName)}", uniqueName, nameIsNullError, 1)) return new string[0];
			if (KeyNotFoundError(parameters, soundCollections, $"sound collection's {nameof(uniqueName)}", uniqueName, nameNotFoundError, 1)) return new string[0];

			return soundCollections[uniqueName].ToArray();
		}
		public static string[] GetAllCollectionUniqueNames()
		{
			return soundCollections.Keys.ToArray();
		}

		private static bool XCollectionErrorChecking(string parameters, string soundUniqueName, string collectionUniqueName, bool soundNameIsNullError = true, bool collectionNameIsNullError = true, bool soundNotFoundError = true, bool collectionNotFoundError = true)
		{
			if (IsNullError(parameters, nameof(soundUniqueName), soundUniqueName, soundNameIsNullError, 2)) return true;
			if (IsNullError(parameters, nameof(collectionUniqueName), collectionUniqueName, collectionNameIsNullError, 2)) return true;
			if (sounds.ContainsKey(soundUniqueName) == false)
			{
				if (soundNotFoundError)
				{
					Error($"{parameters}\n\n{GetContentNotFoundError("sound", soundUniqueName)}", 2);
				}
				return true;
			}
			if (KeyNotFoundError(parameters, soundCollections, nameof(collectionUniqueName), collectionUniqueName, soundNotFoundError, 2)) return true;

			return false;
		}
	}
	/// <summary>
	/// Controls Music and holds information about them.
	/// </summary>
	public static class Music
	{
		public static string[] GetAllUniqueNames()
		{
			return melodies.Keys.ToArray();
		}

		public static void Play(string uniqueName, float volumePercent = 50)
		{
			if (melodies.ContainsKey(uniqueName) == false) return;

			if (muteMelody) return;

			volumePercent = Number.GetLimited(volumePercent, 0, 100);
			MediaPlayer.Volume = volumePercent / 100;
			MediaPlayer.Play(melodies[uniqueName]);
			Signal.Set(melodyOverKey, SongDurationInSec(MediaPlayer.Queue.ActiveSong));
		}
		public static float GetDurationInSeconds(string name)
		{
			return name == null || melodies.ContainsKey(name) == false ? default : SongDurationInSec(melodies[name]);
		}
		public static string GetUniqueName()
		{
			return melodyUniqueNames[MediaPlayer.Queue.ActiveSong];
		}
		public static float GetProgressInSeconds()
		{
			return GetDurationInSeconds(melodyUniqueNames[MediaPlayer.Queue.ActiveSong]) - Signal.GetSecondsLeft(melodyOverKey);
		}
		public static float GetProgressInPercent()
		{
			var dur = GetDurationInSeconds(melodyUniqueNames[MediaPlayer.Queue.ActiveSong]);
			return dur == 0 ? 0 : (dur - Signal.GetSecondsLeft(melodyOverKey)) / dur * 100;
		}
		public static void Pause(bool paused)
		{
			Signal.Pause(melodyOverKey, paused);
			if (paused)
			{
				MediaPlayer.Pause();
				return;
			}
			MediaPlayer.Resume();
		}
		public static void Stop()
		{
			MediaPlayer.Stop();
		}
		public static void Mute(bool muted)
		{
			muteMelody = muted;
		}

		public static bool IsLooping()
		{
			return MediaPlayer.IsRepeating;
		}
		public static void Loop(bool looping)
		{
			MediaPlayer.IsRepeating = looping;
		}

		public static float GetVolume()
		{
			return MediaPlayer.Volume;
		}

		private static float SongDurationInSec(Song song)
		{
			var value = song.Duration.Hours * 3600 + song.Duration.Minutes * 60 +
				song.Duration.Seconds + song.Duration.Milliseconds / 1000;
			return value;
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
			var parameters = $"{nameof(Expand)}({nameof(index)}: {index}, " +
				$"{nameof(uniqueKey)}: {uniqueKey}, {nameof(value)}: {value}, " +
				$"{nameof(invalidIndexError)}: {invalidIndexError}, {nameof(keyExistsError)}: {keyExistsError})";
			if (keys == null) keys = new List<UniqueKeyT>();
			if (indexes == null) indexes = new List<int>();

			if (values == null) values = new List<ValueT>();
			if (index < 0)
			{
				if (invalidIndexError)
				{
					Error($"{parameters}\n\n{GetInvalidValueError(nameof(index), $"{index}")}\n\nTip:\nMake sure it's not < 0.", 1);
				}
				return;
			}
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
			if (KeyExistsError(parameters, dict, nameof(uniqueKey), uniqueKey, keyExistsError, 1)) return;

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
		public void ShrinkAt(int index, bool indexNotFoundError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(index)} = {index}\n" +
				$"{nameof(indexNotFoundError)} = {indexNotFoundError.ToString().ToLower()}";
			if (IndexNotFoundError(parameters, index, indexNotFoundError)) return;

			indexes.Remove(index);
			values.RemoveAt(index);
			dict.Remove(keys[index]);
			keys.RemoveAt(index);
		}
		public void ShrinkIn(UniqueKeyT uniqueKey, bool keyNotFoundError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(uniqueKey)} = {uniqueKey}\n" +
				$"{nameof(keyNotFoundError)} = {keyNotFoundError}";
			if (KeyNotFoundError(parameters, dict, nameof(uniqueKey), uniqueKey, keyNotFoundError, 1)) return;

			indexes.Remove(keys.IndexOf(uniqueKey));
			values.RemoveAt(keys.IndexOf(uniqueKey));
			dict.Remove(uniqueKey);
			keys.Remove(uniqueKey);
		}
		public void ReplaceAt(int index, ValueT value, bool indexNotFoundError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(index)} = {index}\n" +
				$"{nameof(value)} = {value}\n" +
				$"{nameof(indexNotFoundError)} = {indexNotFoundError.ToString().ToLower()}";
			if (IndexNotFoundError(parameters, index, indexNotFoundError)) return;

			values[index] = value;
			dict[keys[index]] = value;
		}
		public void ReplaceIn(UniqueKeyT uniqueKey, ValueT value, bool keyNotFoundError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(uniqueKey)} = {uniqueKey}\n" +
				$"{nameof(value)} = {value}\n" +
				$"{nameof(keyNotFoundError)} = {keyNotFoundError.ToString().ToLower()}";
			if (KeyNotFoundError(parameters, dict, nameof(uniqueKey), uniqueKey, keyNotFoundError, 1)) return;

			dict[uniqueKey] = value;
			values[keys.IndexOf(uniqueKey)] = value;
		}
		public void Free()
		{
			if (indexes == null || keys == null || values == null || dict == null || indexes.Count == 0) return;
			indexes.Clear();
			keys.Clear();
			values.Clear();
			dict.Clear();
		}
		public void Shuffle()
		{
			for (int i = 0; i < indexes.Count - 1; i++)
			{
				var j = (int)Number.GetRandomized(i, indexes.Count - 1);
				var tempIndex = indexes[i];
				//var tempKey = keys[tempIndex];
				var tempValue = values[tempIndex];

				dict[keys[indexes[i]]] = values[indexes[j]];
				//keys[indexes[i]] = keys[indexes[j]];
				values[indexes[i]] = values[indexes[j]];
				indexes[i] = indexes[j];

				dict[keys[indexes[j]]] = tempValue;
				//keys[indexes[j]] = tempKey;
				values[indexes[j]] = tempValue;
				indexes[j] = tempIndex;
			}
		}

		public int GetDataAmount()
		{
			return indexes == null ? 0 : indexes.Count;
		}
		public ValueT GetValueIn(UniqueKeyT uniqueKey, bool keyNotFoundError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(uniqueKey)} = {uniqueKey}\n" +
				$"{nameof(keyNotFoundError)} = {keyNotFoundError.ToString().ToLower()}";
			if (KeyNotFoundError(parameters, dict, nameof(uniqueKey), uniqueKey, keyNotFoundError, 1)) return default;

			return dict[uniqueKey];
		}
		public ValueT GetValueAt(int index, bool indexNotFoundError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(index)} = {index}\n" +
				$"{nameof(indexNotFoundError)} = {indexNotFoundError.ToString().ToLower()}";
			if (IndexNotFoundError(parameters, index, indexNotFoundError)) return default;

			return values[index];
		}
		public UniqueKeyT GetUniqueKeyAt(int index, bool indexNotFoundError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(index)} = {index}\n" +
				$"{nameof(indexNotFoundError)} = {indexNotFoundError.ToString().ToLower()}";
			if (IndexNotFoundError(parameters, index, indexNotFoundError)) return default;

			return keys[index];
		}
		public int GetIndexIn(UniqueKeyT uniqueKey, bool keyNotFoundError = true)
		{
			var parameters = $"Parameters:\n" +
				$"{nameof(uniqueKey)} = {uniqueKey}\n" +
				$"{nameof(keyNotFoundError)} = {keyNotFoundError.ToString().ToLower()}";
			if (KeyNotFoundError(parameters, dict, nameof(uniqueKey), uniqueKey, keyNotFoundError, 1)) return default;

			return keys.IndexOf(uniqueKey);
		}
		public int[] GetIndexes()
		{
			return indexes == null ? new int[0] : indexes.ToArray();
		}
		public UniqueKeyT[] GetUniqueKeys()
		{
			var result = new List<UniqueKeyT>();
			for (int i = 0; i < indexes.Count; i++)
			{
				result.Add(keys[indexes[i]]);
			}
			return result.ToArray();
		}
		public ValueT[] GetValues()
		{
			var result = new List<ValueT>();
			for (int i = 0; i < indexes.Count; i++)
			{
				result.Add(values[indexes[i]]);
			}
			return result.ToArray();
		}

		public bool HasIndex(int index)
		{
			return indexes != null && indexes.Contains(index);
		}
		public bool HasUniqueKey(UniqueKeyT uniqueKey)
		{
			return keys != null && keys.Contains(uniqueKey);
		}
		public bool HasValue(ValueT value)
		{
			return values != null && values.Contains(value);
		}

		private bool IndexNotFoundError(string parameters, int index, bool indexNotFoundError)
		{
			if (indexes.Contains(index) == false)
			{
				if (indexNotFoundError)
				{
					Error($"{parameters}\n\n{GetNotFoundError(nameof(index), $"{index}")}", index + 1);
				}
				return true;
			}
			return false;
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
		public void SetToPercentTowardAngle(Angle targetAngle, float percent)
		{
			To360();
			targetAngle.To360();
			a = Number.GetPercentedTowardTarget(a, targetAngle.GetA(), percent);
		}
		public void Rotate(float speed, Motion motion = Motion.PerSecond)
		{
			a = Number.GetChanged(a, speed, motion);
			To360();
		}
		public void RotateTowardAngle(Angle targetAngle, float speed, Motion motion = Motion.PerSecond)
		{
			To360();
			targetAngle.To360();
			speed = Math.Abs(speed);
			var difference = a - targetAngle.GetA();

			// stops the rotation with an else when close enough
			// prevents the rotation from staying behind after the stop
			var checkedSpeed = speed;
			if (motion == Motion.PerSecond)
			{
				checkedSpeed *= ticksDeltaTime;
			}
			if (Math.Abs(difference) < checkedSpeed) a = targetAngle.GetA();
			else if (difference > 0 && difference < 180) Rotate(-speed);
			else if (difference > -180 && difference < 0) Rotate(speed);
			else if (difference > -360 && difference < -180) Rotate(-speed);
			else if (difference > 180 && difference < 360) Rotate(speed);

			// detects speed greater than possible
			// prevents jiggle when passing 0-360 & 360-0 | simple to fix yet took me half a day
			if (Math.Abs(difference) > 360 - checkedSpeed) a = targetAngle.GetA();
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

		public static Angle operator +(Angle a, Angle b)
		{
			return new Angle(a.GetA() + b.GetA());
		}
		public static Angle operator -(Angle a, Angle b)
		{
			return new Angle(a.GetA() - b.GetA());
		}
		public static Angle operator *(Angle a, Angle b)
		{
			return new Angle(a.GetA() * b.GetA());
		}
		public static Angle operator /(Angle a, Angle b)
		{
			return new Angle(a.GetA() / b.GetA());
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
		public void Scale(float speed, Motion motion = Motion.PerSecond)
		{
			if (motion == Motion.PerSecond)
			{
				speed *= ticksDeltaTime;
			}
			w += speed;
			h += speed;
		}
		public void ScaleTowardTarget(Size targetSize, float speed, Motion motion = Motion.PerSecond)
		{
			Scale(speed, motion);
			if (motion == Motion.PerSecond)
			{
				speed *= ticksDeltaTime;
			}
			var dist = Vector2.Distance(new Vector2(GetW(), GetH()), new Vector2(targetSize.GetW(), targetSize.GetH()));
			if (dist < speed * 2)
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
		public void MoveInDirection(Direction direction, float speed, Motion motion = Motion.PerSecond)
		{
			if (motion == Motion.PerSecond)
			{
				speed *= ticksDeltaTime;
			}
			direction.Normalize();
			x += direction.GetEndPoint().GetX() * speed;
			y += direction.GetEndPoint().GetY() * speed;
		}
		public void MoveAtAngle(Angle angle, float speed, Motion motion = Motion.PerSecond)
		{
			var dir = new Direction();
			dir.SetFromAngle(angle);
			MoveInDirection(dir, speed, motion);
		}
		public void MoveTowardPoint(Point targetPoint, float speed, Motion motion = Motion.PerSecond)
		{
			var dir = new Direction(targetPoint - this);
			MoveInDirection(dir, speed, motion);

			if (motion == Motion.PerSecond)
			{
				speed *= ticksDeltaTime;
			}
			var dist = Vector2.Distance(new Vector2(x, y), new Vector2(targetPoint.GetX(), targetPoint.GetY()));
			if (dist < speed * 2)
			{
				x = targetPoint.GetX();
				y = targetPoint.GetY();
			}
		}
		public void SetToPercentTowardPoint(Point targetPoint, float percent)
		{
			var vec = Vector2.Lerp(new Vector2(GetX(), GetY()), new Vector2(targetPoint.GetX(), targetPoint.GetY()), (float)percent / 100);
			x = vec.X;
			y = vec.Y;
		}
		public void SetToGrid(Size gridSize)
		{
			var grid_width = Number.GetLimited(gridSize.GetW(), 1, screenSize.GetW());
			var grid_height = Number.GetLimited(gridSize.GetH(), 1, screenSize.GetH());
			if (gridSize.GetW() > 0)
			{
				SetXY(grid_width * (float)Math.Round(x / grid_width), y);
			}
			if (gridSize.GetH() > 0)
			{
				SetXY(x, grid_height * (float)Math.Round(y / grid_height));
			}
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
		public void SetToPercentTowardDirection(Direction targetDirection, float percent)
		{
			Normalize();
			targetDirection.Normalize();
			var angle = new Angle();
			var targetAngle = new Angle();
			angle.SetFromDirection(this);
			targetAngle.SetFromDirection(targetDirection);
			angle.SetToPercentTowardAngle(targetAngle, percent);
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
		public void RotateTowardDirection(Direction targetDirection, float degreesPerSecond)
		{
			Normalize();
			targetDirection.Normalize();
			var angle = new Angle();
			var targetAngle = new Angle();
			angle.SetFromDirection(this);
			targetAngle.SetFromDirection(targetDirection);
			angle.RotateTowardAngle(targetAngle, degreesPerSecond);
			SetFromAngle(angle);
		}

		public static Direction operator +(Direction a, Direction b)
		{
			var dir = new Direction(a.endPoint + b.endPoint);
			dir.Normalize();
			return dir;
		}
		public static Direction operator -(Direction a, Direction b)
		{
			var dir = new Direction(a.endPoint - b.endPoint);
			dir.Normalize();
			return dir;
		}
		public static Direction operator *(Direction a, Direction b)
		{
			var dir = new Direction(a.endPoint * b.endPoint);
			dir.Normalize();
			return dir;
		}
		public static Direction operator /(Direction a, Direction b)
		{
			var dir = new Direction(a.endPoint / b.endPoint);
			dir.Normalize();
			return dir;
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
		private void _Set(Color color)
		{
			r = color.r;
			g = color.g;
			b = color.b;
			o = color.o;
			To255();
		}
		public void SetRGBO(float r, float g, float b, float o = 255)
		{
			_Set(new Color(r, g, b, o));
		}
		public void SetR(float r)
		{
			_Set(new Color(r, g, b, o));
		}
		public void SetG(float g)
		{
			_Set(new Color(r, g, b, o));
		}
		public void SetB(float b)
		{
			_Set(new Color(r, g, b, o));
		}
		public void SetO(float o)
		{
			_Set(new Color(r, g, b, o));
		}
		public void Set(Color color)
		{
			_Set(color);
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
		public void TintTowardR(float targetRed, float shadesPerSecond)
		{
			TintR(r < targetRed ? shadesPerSecond : -shadesPerSecond);
			var dist = Math.Abs(r - targetRed);
			if (dist < shadesPerSecond * ticksDeltaTime * 2) r = targetRed;
		}
		public void TintTowardG(float targetGreen, float shadesPerSecond)
		{
			TintG(r < targetGreen ? shadesPerSecond : -shadesPerSecond);
			var dist = Math.Abs(g - targetGreen);
			if (dist < shadesPerSecond * ticksDeltaTime * 2) g = targetGreen;
		}
		public void TintTowardB(float targetBlue, float shadesPerSecond)
		{
			TintB(b < targetBlue ? shadesPerSecond : -shadesPerSecond);
			var dist = Math.Abs(b - targetBlue);
			if (dist < shadesPerSecond * ticksDeltaTime * 2) b = targetBlue;
		}
		public void AppearTowardO(float targeto, float shadesPerSecond)
		{
			Appear(o < targeto ? shadesPerSecond : -shadesPerSecond);
			var dist = Math.Abs(o - targeto);
			if (dist < shadesPerSecond * ticksDeltaTime * 2) o = targeto;
		}
		public void TintTowardColor(Color targetColor, float shadesPerSecond)
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
			r = Number.GetLimited(r, 0, 255);
			g = Number.GetLimited(g, 0, 255);
			b = Number.GetLimited(b, 0, 255);
			o = Number.GetLimited(o, 0, 255);
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
			radius = Number.GetLimited(radius, 2, 1_000_000);
			this.position = position;
			this.radius = radius;
		}
		public void Set(Point position, float radius)
		{
			radius = Number.GetLimited(radius, 2, 1_000_000);
			this.position = position;
			this.radius = radius;
		}

		public Point GetPosition()
		{
			return position;
		}
		public float GetRadius()
		{
			return radius;
		}

		public Point[] GetCrossPointsWithLine(Line line)
		{
			return GetLineCircleCrossPoints(position, radius, line.GetStartPoint(), line.GetEndPoint());
		}
		public bool IsCrossedByLine(Line line)
		{
			return GetLineCircleCrossPoints(position, radius, line.GetStartPoint(), line.GetEndPoint()).Length > 0;
		}
		public bool IsOverlappingCircle(Circle circle)
		{
			var sum = radius + circle.radius;
			var dist = position.GetDistanceToPoint(circle.position);
			return sum >= dist;
		}
		public Point[] GetCrossPointsWithCircle(Circle circle)
		{
			return GetCircleCircleCrossPoints(position.GetX(), position.GetY(), radius, circle.position.GetX(), circle.position.GetY(), circle.radius);
		}

		public override string ToString()
		{
			return $"{nameof(Circle)}[{nameof(position)}:{position}][{nameof(radius)}:{radius}]";
		}
	}
	public struct Line
	{
		private Point startPoint;
		private Point endPoint;

		public Line(Point startPoint, Point endPoint)
		{
			this.startPoint = startPoint;
			this.endPoint = endPoint;
		}
		public void Set(Point startPoint, Point endPoint)
		{
			this.startPoint = startPoint;
			this.endPoint = endPoint;
		}

		public Point GetStartPoint()
		{
			return startPoint;
		}
		public Point GetEndPoint()
		{
			return endPoint;
		}
		public float GetLength()
		{
			return startPoint.GetDistanceToPoint(endPoint);
		}

		public Point[] GetCrossPointsWithCircle(Circle circle)
		{
			return circle.GetCrossPointsWithLine(this);
		}
		public bool IsCrossingCircle(Circle circle)
		{
			return circle.IsCrossedByLine(this);
		}
		public Point[] GetCrossPointWithLine(Line line)
		{
			var segmentsCross = false;
			var linesCross = false;
			var intersection = new Point[1];
			var closestCrossPointToMe = new Point();
			var closestCrossPointToLine = new Point();

			GetCrossPointOfTwoLines(startPoint, endPoint, line.startPoint, line.endPoint, out linesCross, out segmentsCross, out intersection, out closestCrossPointToMe, out closestCrossPointToLine);
			return segmentsCross ? intersection : new Point[0];
		}
		public bool IsCrossingLine(Line line)
		{
			return GetCrossPointWithLine(line).Length == 1;
		}
		public bool ContainsPoint(Point point)
		{
			var AB = GetLength();
			var AP = startPoint.GetDistanceToPoint(point);
			var PB = endPoint.GetDistanceToPoint(point);
			var sum = AP + PB;
			return Number.IsBetween(AB - 0.01f, sum, AB + 0.01f);
		}

		public override string ToString()
		{
			return $"{nameof(Line)}[{nameof(startPoint)}:{startPoint}][{nameof(endPoint)}:{endPoint}]";
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
								messageBack = $"~{(int)MessageType.UniqueNameChange}|{id}|{uniqueName}"; // Send a message back with a free one toward the same ID so the client can recognize it's for him
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
		var feed = $"{connectInfo}{clientsConnected}{consoleLog}";
		var newLine = feed.Length > 0 ? "\n" : "";
		System.Console.Write($"{feed}{newLine}");
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
	private static void GetCrossPointOfTwoLines(Point startA, Point endA, Point startB, Point endB,
		 out bool lines_intersect, out bool segments_intersect,
		 out Point[] intersection,
		 out Point close_p1, out Point close_p2)
	{
		// Find the point of intersection between
		// the lines p1 --> p2 and p3 --> p4.

		intersection = new Point[0];
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
		intersection = new Point[1] { new Point(startA.GetX() + dx12 * t1, startA.GetY() + dy12 * t1) };

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
	private static Point[] GetLineCircleCrossPoints(Point circlePosition, float circleRadius, Point pointA, Point pointB)
	{
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
			return new Point[0];
		}
		else if (det == 0)
		{
			// one solution
			t = -B / (2 * A);
			var point = new Point(pointA.GetX() + t * dx, pointA.GetY() + t * dy);
			if (point.GetDistanceToPoint(pointA) >= lineLength) return new Point[1] { point };
		}
		else
		{
			var result = new Point[2];
			// two solutions
			t = (float)((-B + Math.Sqrt(det)) / (2 * A));
			var point1 = new Point(pointA.GetX() + t * dx, pointA.GetY() + t * dy);
			if (point1.GetDistanceToPoint(pointA) <= lineLength) result[0] = point1;

			t = (float)((-B - Math.Sqrt(det)) / (2 * A));
			var point2 = new Point(pointA.GetX() + t * dx, pointA.GetY() + t * dy);
			if (point2.GetDistanceToPoint(pointA) <= lineLength) result[1] = point2;
			return result;
		}
		return new Point[0];
	}
	private static Point[] GetCircleCircleCrossPoints(float cx0, float cy0, float radius0, float cx1, float cy1, float radius1)
	{
		// Find the distance between the centers.
		float dx = cx0 - cx1;
		float dy = cy0 - cy1;
		double dist = Math.Sqrt(dx * dx + dy * dy);

		// See how many solutions there are.
		if (dist > radius0 + radius1)
		{
			// No solutions, the circles are too far apart.
			return new Point[0];
		}
		else if (dist < Math.Abs(radius0 - radius1))
		{
			// No solutions, one circle contains the other.
			return new Point[0];
		}
		else if ((dist == 0) && (radius0 == radius1))
		{
			// No solutions, the circles coincide.
			return new Point[0];
		}
		else
		{
			var resultA = new Point[1];
			var resultA2 = new Point[1];
			var resultB = new Point[2];
			// Find a and h.
			double a = (radius0 * radius0 -
				 radius1 * radius1 + dist * dist) / (2 * dist);
			double h = Math.Sqrt(radius0 * radius0 - a * a);

			// Find P2.
			double cx2 = cx0 + a * (cx1 - cx0) / dist;
			double cy2 = cy0 + a * (cy1 - cy0) / dist;

			// Get the points P3.
			var point1 = new Point((float)(cx2 + h * (cy1 - cy0) / dist), (float)(cy2 - h * (cx1 - cx0) / dist));
			var point2 = new Point((float)(cx2 - h * (cy1 - cy0) / dist), (float)(cy2 + h * (cx1 - cx0) / dist));
			resultA[0] = point1;
			resultA2[0] = point2;
			resultB[0] = point1;
			resultB[1] = point2;

			// See if we have 1 or 2 solutions.
			if (dist == radius0 + radius1) return resultA[0] == default ? resultA2 : resultA;
			return resultB;
		}
	}

	private static void Error(string message, int index)
	{
		var resultMessage = Debug.IsActivated() ? $"File:\n{Debug.GetCodeFileName(index + 1)}\n\nLine:\n{Debug.GetCodeLine(index + 1)}\n\nMethod:\n{Debug.GetCodeMethodName(index)}\n\n{message}" : userErrorMessage;

		Window.PopUp(resultMessage, $"Error | {Window.GetTitle()}", PopUpIcon.Error);
		Window.Close();
	}
	private static string GetNotFoundError(string type, string name, string inside = "")
	{
		return $"Description:\nThe {type} '{name}' was not found{inside}.";
	}
	private static string GetAlreadyExistsError(string type, string name, string inside = "")
	{
		return $"Description:\nThe {type} '{name}' already exists{inside}.";
	}
	private static string GetAlreadyAddedError(string type, string name, string inside = "")
	{
		return $"Description:\nThe {type} '{name}' is already added{inside}.";
	}
	private static string GetContentNotFoundError(string type, string name)
	{
		var extension = "";
		switch (type)
		{
			case "sound": extension = "wav"; break;
			case "melody": extension = "mp3"; break;
			case "sprite": extension = "png"; break;
		}
		return $"Description:\nThe {type} '{name}' was not found.\n\nTip:\nIn order to load a {type}...\n{contentLoadingInfo}\nAll {type} files should be of .{extension} format.";
	}
	private static string GetCannotBeNullError(string type)
	{
		return $"Description:\nThe {type} cannot be 'null'.";
	}
	private static string GetBodyTransformLockError(Body body, string component)
	{
		return $"Description:\nThe component of {nameof(Body)} '{body}' cannot be affected due to it being locked.";
	}
	private static string GetInvalidValueError(string type, string name)
	{
		return $"Description:\nThe {type} '{name}' is invalid.";
	}
	private static string GetIsEmptyError(string type, string name)
	{
		return $"The {type} '{name}' is empty.";
	}

	private static bool KeyNotFoundError<UniqueKeyT, ValueT>(string parameters, Dictionary<UniqueKeyT, ValueT> dict, string type, UniqueKeyT uniqueKey, bool keyNotFoundError, int index, string inside = "")
	{
		if (dict == null || dict.ContainsKey(uniqueKey) == false)
		{
			if (keyNotFoundError)
			{
				Error($"{parameters}\n\n{GetNotFoundError(type, $"{uniqueKey}", inside)}", index + 1);
			}
			return true;
		}
		return false;
	}
	private static bool KeyExistsError<UniqueKeyT, ValueT>(string parameters, Dictionary<UniqueKeyT, ValueT> dict, string type, UniqueKeyT uniqueKey, bool keyExistsError, int index, string inside = "")
	{
		if (dict != null && dict.ContainsKey(uniqueKey))
		{
			if (keyExistsError)
			{
				Error($"{parameters}\n\n{GetAlreadyExistsError(type, $"{uniqueKey}", inside)}", index + 1);
			}
			return true;
		}
		return false;
	}
	private static bool IsNullError<T>(string parameters, string type, T value, bool valueIsNullError, int index)
	{
		if (value == null)
		{
			if (valueIsNullError)
			{
				Error($"{parameters}\n\n{GetCannotBeNullError(type)}", index + 1);
			}
			return true;
		}
		return false;
	}
	private static bool ValueAlreadyAddedError<T>(string parameters, List<T> list, string type, T value, bool valueExistsError, int index, string inside = "")
	{
		if (list != null && list.Contains(value))
		{
			if (valueExistsError)
			{
				Error($"{parameters}\n\n{GetAlreadyAddedError(type, $"{value}", inside)}", index + 1);
			}
			return true;
		}
		return false;
	}
	private static bool ValueNotFoundError<T>(string parameters, List<T> list, string type, T value, bool valueNotFoundError, int index, string inside = "")
	{
		if (list != null && list.Contains(value) == false)
		{
			if (valueNotFoundError)
			{
				Error($"{parameters}\n\n{GetNotFoundError(type, $"{value}", inside)}", index + 1);
			}
			return true;
		}
		return false;
	}
	private static bool ArrayIsEmptyError<T>(string parameters, T[] array, string type, string name, bool arrayIsEmptyError, int index)
	{
		if (array != null && array.Length == 0)
		{
			if (arrayIsEmptyError)
			{
				Error($"{parameters}\n\n{GetIsEmptyError(type, name)}", index + 1);
			}
			return true;
		}
		return false;
	}

	private static void UpdateCameraBodyTransform(Body body)
	{
		var angle = new Angle();
		var camPos = Camera.GetPosition();
		var dist = camPos.GetDistanceToPoint(body.GetPosition());
		angle.SetFromBetweenPoints(Camera.GetPosition(), body.GetPosition());

		bodyCameraAngle[body] = angle.GetA();
		bodyCameraDistances[body] = dist;
		bodyCameraAngleDifferences[body] = cameraAngle - body.GetAngle().GetA();
	}
	private static Point GetCameraBodyPosition(Body body)
	{
		var pos = new Point();
		var dir = new Direction();
		dir.SetFromAngle(Camera.GetAngle() + new Angle(bodyCameraAngle[body]));
		pos = Camera.GetPosition() + dir.GetEndPoint() * bodyCameraDistances[body];

		return pos;
	}

	private static bool TimerTickIsOccurring(string name, float intervalsInSeconds, int repeats = 1000000)
	{
		intervalsInSeconds = Number.GetLimited(intervalsInSeconds, 0.01f, 100000);
		if (Gate.IsOpened(name, SignalIsOccurring(name, false), repeats))
		{
			Signal.Set(name, intervalsInSeconds);
			timerRepeats[name] = repeats;
			return true;
		}
		return false;
	}
	private static bool SignalIsOccurring(string name, bool delete)
	{
		if (name == null) return false;
		if (signalDelays.ContainsKey(name) == false) return false;
		if (signalpauses.ContainsKey(name) == true && signalpauses[name])
		{
			signalEndTimes[name] += ticksDeltaTime;
			return false;
		}
		if (time - ticksDeltaTime >= signalEndTimes[name])
		{
			if (delete) Signal.Delete(name);
			return true;
		}
		return false;
	}
}