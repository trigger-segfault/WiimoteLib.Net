using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WiimoteController.Util;
using WiimoteLib.Geometry;
using WindowsInput;

namespace WiimoteController.Input {
	public interface IInputAnalog : IInputType {
		//string Name { get; }
		//string Type { get; }
		//bool IsDown { get; }
		Point2F Position { get; }
		float Distance { get; }
		float DirectionRad { get; }
		float DirectionDeg { get; }
		float Deadzone { get; set; }
		//bool IsInitialized { get; }
		//void Initialize(InputSimulator sim);
		void Update(PointF position);
		void Update();
		//void Reset();
	}
	public abstract class InputAnalogBase : IInputAnalog {
		private IInputSimulator sim;
		private bool initialized;
		private bool down;
		private Point2F position;
		public Point2F Position => position;
		private float deadzone;

		public float Deadzone {
			get => deadzone;
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException(nameof(Deadzone));
				deadzone = value;
			}
		}
		public float Distance {
			get => position.Length;
		}
		public float DirectionRad {
			get => position.DirectionRad;
		}
		public float DirectionDeg {
			get => position.DirectionDeg;
		}

		public InputAnalogBase() {
			deadzone = 0.085f;
		}

		protected IInputSimulator Sim => sim;

		public bool IsDown => down;
		public bool IsInitialized => initialized;

		public abstract string Name { get; }
		public abstract string Type { get; }

		public override string ToString() => $"{Type}: {Name}";

		public void Initialize(IInputSimulator sim) {
			Dispose();
			this.sim = sim ?? throw new ArgumentNullException(nameof(sim));
			OnInitialize();
			initialized = true;
		}
		public void Dispose() {
			if (initialized) {
				Reset();
				OnDispose();
				sim = null;
				down = false;
				initialized = false;
			}
		}
		public void Update(PointF position) {
			if (initialized) {
				this.position = position;
				Update();
			}
		}
		public void Update() {
			if (initialized) {
				if (Distance >= deadzone) {
					if (!down) {
						down = true;
						Trace.WriteLine($"{this}: START");
						OnStart();
					}
					OnUpdate();
				}
				else if (down) {
					down = false;
					Trace.WriteLine($"{this}: STOP");
					OnStop();
				}
			}
		}

		public void Reset() {
			if (down) {
				down = false;
				OnStop();
			}
			position = PointF.Empty;
		}

		protected virtual void OnInitialize() { }
		protected virtual void OnDispose() { }
		protected virtual void OnStart() { }
		protected virtual void OnStop() { }
		protected virtual void OnUpdate() { }
	}


	public class AnalogMouse : InputAnalogBase {

		private float speed;
		private float power;

		public float Speed {
			get => speed;
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException(nameof(Speed));
				speed = value;
			}
		}
		public float Power {
			get => speed;
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException(nameof(Power));
				power = value;
			}
		}

		private const int UpdateSpeed = 4;

		private PointF leftover;

		private object lockObj;

		private CancellationTokenSource token;
		private Task moveTask;

		private Stopwatch watch;
		private long lastMove;

		public AnalogMouse() {
			lockObj = new object();
			speed = 6.5f;
			power = 3f;
			leftover = PointF.Empty;
		}

		public override string Name => "Mouse Move";
		public override string Type => "Analog";
		
		protected override void OnStart() {
			lock (lockObj) {
				token?.Cancel();
				token = new CancellationTokenSource();
				watch?.Stop();
				watch = Stopwatch.StartNew();
				moveTask = Task.Run(() => MouseMoveTask(token.Token), token.Token);
			}
		}

		protected override void OnStop() {
			lock (lockObj) {
				watch?.Stop();
				watch = null;
				token?.Cancel();
				token = null;
				moveTask = null;
			}
		}

		private void MouseMoveTask(CancellationToken token) {
			int min = int.MaxValue;
			int max = int.MinValue;

			PointF leftover = PointF.Empty;
			Stopwatch watch = Stopwatch.StartNew();
			long lastMove = 0;
			while (!token.IsCancellationRequested) {
				Point2F position = Position;

				float dir = position.DirectionRad;
				float dist = position.Length;
				long newTime = watch.ElapsedMilliseconds;
				int ellapsed = (int) (newTime - lastMove);
				if (min == int.MaxValue)
					ellapsed = 12;
				float scale = Speed / 5f * Math.Min(1.5f, ellapsed / 18f);
				dist = scale * (float) Math.Pow(dist * 5f, power);

				Point2F scaled = Point2F.FromPolarRad(dist, dir) + leftover;
				Point2I move = new Point2I((int) Math.Truncate(scaled.X), (int) Math.Truncate(scaled.Y));
				leftover = scaled - move;

				if (!move.IsZero) {
					Sim.Mouse.MoveMouseBy(move.X, -move.Y); // Y axis is inverted
				}
				if (lastMove != 0)
					min = Math.Min(min, ellapsed);
				max = Math.Max(max, ellapsed);
				//Trace.WriteLine($"Move: {ellapsed}ms / {UpdateSpeed}ms");
				lastMove = newTime;
				token.Sleep(UpdateSpeed);
			}
			Trace.WriteLine($"Min={min}ms Max={max}ms");
		}
	}
}
