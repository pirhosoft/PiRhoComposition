using PiRhoSoft.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PiRhoSoft.Bindings
{
	public static class BindingAnimation
	{
		private static HierarchyPool<IBindingTransition> _pool = new HierarchyPool<IBindingTransition>();

		#region Registration

		public static void RegisterDefaults()
		{
			Register<sbyte, SByteBindingAnimation>();
			Register<short, ShortBindingAnimation>();
			Register<int, IntBindingAnimation>();
			Register<long, LongBindingAnimation>();
			Register<byte, ByteBindingAnimation>();
			Register<ushort, UShortBindingAnimation>();
			Register<uint, UIntBindingAnimation>();
			Register<ulong, ULongBindingAnimation>();
			Register<float, FloatBindingAnimation>();
			Register<double, DoubleBindingAnimation>();
			Register<decimal, DecimalBindingAnimation>();
			Register<char, CharBindingAnimation>();
			Register<string, StringBindingAnimation>();
			Register<Vector2, Vector2BindingAnimation>();
			Register<Vector3, Vector3BindingAnimation>();
			Register<Vector4, Vector4BindingAnimation>();
			Register<Quaternion, QuaternionBindingAnimation>();
			Register<Rect, RectBindingAnimation>();
			Register<Bounds, BoundsBindingAnimation>();
			Register<Vector2Int, Vector2IntBindingAnimation>();
			Register<Vector3Int, Vector3IntBindingAnimation>();
			Register<RectInt, RectIntBindingAnimation>();
			Register<BoundsInt, BoundsIntBindingAnimation>();
			Register<Color, ColorBindingAnimation>();
			Register<bool, BoolBindingAnimation>();
		}

		public static void Register<Type, TransitionType>(int capacity = ClassPool.DefaultCapacity, int growth = ClassPool.DefaultGrowth)
			where TransitionType : class, IBindingTransition<Type>, new()
		{
			_pool.Register<Type, TransitionType>(() => new TransitionType(), capacity, growth);
		}

		#endregion

		#region Creation

		public static IBindingTransition Create<Type>() => _pool.Reserve(typeof(Type));
		public static IBindingTransition Create(Type type) => _pool.Reserve(type);
		internal static void Release(IBindingTransition transition) => _pool.Release(transition);

		#endregion

		#region Built In Type Implementations

		#region Integral

		private class SByteBindingAnimation : BindingAnimation<sbyte>
		{
			protected override float GetDistance(sbyte from, sbyte to) => Math.Abs(to - from);
			protected override sbyte Interpolate(sbyte from, sbyte to, float t) => (sbyte)Math.Round(from + (to - from) * t);
		}

		private class ShortBindingAnimation : BindingAnimation<short>
		{
			protected override float GetDistance(short from, short to) => Math.Abs(to - from);
			protected override short Interpolate(short from, short to, float t) => (short)Math.Round(from + (to - from) * t);
		}

		private class IntBindingAnimation : BindingAnimation<int>
		{
			protected override float GetDistance(int from, int to) => Math.Abs(to - from);
			protected override int Interpolate(int from, int to, float t) => (int)Math.Round(from + (to - from) * t);
		}

		private class LongBindingAnimation : BindingAnimation<long>
		{
			protected override float GetDistance(long from, long to) => Math.Abs(to - from);
			protected override long Interpolate(long from, long to, float t) => (long)Math.Round(from + (to - from) * t);
		}

		#endregion

		#region Unsigned

		private class ByteBindingAnimation : BindingAnimation<byte>
		{
			protected override float GetDistance(byte from, byte to) => (to > from ? to - from : from - to);
			protected override byte Interpolate(byte from, byte to, float t) => (byte)Math.Round(from + ((int)to - from) * t);
		}

		private class UShortBindingAnimation : BindingAnimation<ushort>
		{
			protected override float GetDistance(ushort from, ushort to) => (to > from ? to - from : from - to);
			protected override ushort Interpolate(ushort from, ushort to, float t) => (ushort)Math.Round(from + ((int)to - from) * t);
		}

		private class UIntBindingAnimation : BindingAnimation<uint>
		{
			protected override float GetDistance(uint from, uint to) => (to > from ? to - from : from - to);
			protected override uint Interpolate(uint from, uint to, float t) => (uint)Math.Round(from + ((long)to - from) * t);
		}

		private class ULongBindingAnimation : BindingAnimation<ulong>
		{
			protected override float GetDistance(ulong from, ulong to) => (to > from ? to - from : from - to);
			protected override ulong Interpolate(ulong from, ulong to, float t) => (ulong)Math.Round(from + ((long)to - (long)from) * t);
		}

		#endregion

		#region Floating Point

		private class FloatBindingAnimation : BindingAnimation<float>
		{
			protected override float GetDistance(float from, float to) => Math.Abs(to - from);
			protected override float Interpolate(float from, float to, float t) => from + (to - from) * t;
		}

		private class DoubleBindingAnimation : BindingAnimation<double>
		{
			protected override float GetDistance(double from, double to) => (float)Math.Abs(to - from);
			protected override double Interpolate(double from, double to, float t) => from + (to - from) * t;
		}

		private class DecimalBindingAnimation : BindingAnimation<decimal>
		{
			protected override float GetDistance(decimal from, decimal to) => (float)Math.Abs(to - from);
			protected override decimal Interpolate(decimal from, decimal to, float t) => from + (to - from) * (decimal)t;
		}

		#endregion

		#region Text

		private class CharBindingAnimation : BindingAnimation<char>
		{
			protected override float GetDistance(char from, char to) => Math.Abs(to - from);
			protected override char Interpolate(char from, char to, float t) => (char)Math.Round(from + ((int)to - from) * t);
		}

		private class StringBindingAnimation : BindingAnimation<string>
		{
			// This does a typewriter effect.

			protected override float GetDistance(string from, string to)
			{
				from = from ?? string.Empty;
				to = to ?? string.Empty;

				return Math.Abs(to.Length - from.Length);
			}

			protected override string Interpolate(string from, string to, float t)
			{
				from = from ?? string.Empty;
				to = to ?? string.Empty;

				var length = Mathf.RoundToInt(from.Length + (to.Length - from.Length) * t);

				return from.Length > to.Length
					? from.Substring(0, length)
					: to.Substring(0, length);
			}
		}

		#endregion

		#region Unity

		public class Vector2BindingAnimation : BindingAnimation<Vector2>
		{
			protected override float GetDistance(Vector2 from, Vector2 to) => Vector2.Distance(from, to);
			protected override Vector2 Interpolate(Vector2 from, Vector2 to, float t) => Vector2.Lerp(from, to, t);
		}

		public class Vector3BindingAnimation : BindingAnimation<Vector3>
		{
			protected override float GetDistance(Vector3 from, Vector3 to) => Vector3.Distance(from, to);
			protected override Vector3 Interpolate(Vector3 from, Vector3 to, float t) => Vector3.Lerp(from, to, t);
		}

		public class Vector4BindingAnimation : BindingAnimation<Vector4>
		{
			protected override float GetDistance(Vector4 from, Vector4 to) => Vector4.Distance(from, to);
			protected override Vector4 Interpolate(Vector4 from, Vector4 to, float t) => Vector4.Lerp(from, to, t);
		}

		public class QuaternionBindingAnimation : BindingAnimation<Quaternion>
		{
			protected override float GetDistance(Quaternion from, Quaternion to) => Quaternion.Angle(from, to);
			protected override Quaternion Interpolate(Quaternion from, Quaternion to, float t) => Quaternion.Slerp(from, to, t);
		}

		public class RectBindingAnimation : BindingAnimation<Rect>
		{
			protected override float GetDistance(Rect from, Rect to)
			{
				var xMin = Math.Abs(from.xMin - to.xMin);
				var yMin = Math.Abs(from.yMin - to.yMin);
				var xMax = Math.Abs(from.xMax - to.xMax);
				var yMax = Math.Abs(from.yMax - to.yMax);

				return Math.Max(Math.Max(xMin, xMax), Math.Max(yMin, yMax));
			}

			protected override Rect Interpolate(Rect from, Rect to, float t)
			{
				var xMin = from.xMin + (to.xMin - from.xMin) * t;
				var yMin = from.yMin + (to.yMin - from.yMin) * t;
				var xMax = from.xMax + (to.xMax - from.xMax) * t;
				var yMax = from.yMax + (to.yMax - from.yMax) * t;

				return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
			}
		}

		public class BoundsBindingAnimation : BindingAnimation<Bounds>
		{
			protected override float GetDistance(Bounds from, Bounds to)
			{
				var xMin = Math.Abs(from.min.x - to.min.x);
				var yMin = Math.Abs(from.min.y - to.min.y);
				var zMin = Math.Abs(from.min.z - to.min.z);
				var xMax = Math.Abs(from.max.x - to.max.x);
				var yMax = Math.Abs(from.max.y - to.max.y);
				var zMax = Math.Abs(from.max.z - to.max.z);

				return Math.Max(Math.Max(Math.Max(xMin, xMax), Math.Max(yMin, yMax)), Math.Max(zMin, zMax));
			}

			protected override Bounds Interpolate(Bounds from, Bounds to, float t)
			{
				var xMin = from.min.x + (to.min.x - from.min.x) * t;
				var yMin = from.min.y + (to.min.y - from.min.y) * t;
				var zMin = from.min.z + (to.min.z - from.min.z) * t;
				var xMax = from.max.x + (to.max.x - from.max.x) * t;
				var yMax = from.max.y + (to.max.y - from.max.y) * t;
				var zMax = from.max.z + (to.max.z - from.max.z) * t;

				return new Bounds
				{
					min = new Vector3(xMin, yMin, zMin),
					max = new Vector3(xMax, yMax, zMax)
				};
			}
		}

		public class Vector2IntBindingAnimation : BindingAnimation<Vector2Int>
		{
			protected override float GetDistance(Vector2Int from, Vector2Int to) => Vector2Int.Distance(from, to);

			protected override Vector2Int Interpolate(Vector2Int from, Vector2Int to, float t)
			{
				var x = (int)Math.Round(from.x + (to.x - from.x) * t);
				var y = (int)Math.Round(from.y + (to.y - from.y) * t);

				return new Vector2Int(x, y);
			}
		}

		public class Vector3IntBindingAnimation : BindingAnimation<Vector3Int>
		{
			protected override float GetDistance(Vector3Int from, Vector3Int to) => Vector3Int.Distance(from, to);

			protected override Vector3Int Interpolate(Vector3Int from, Vector3Int to, float t)
			{
				var x = (int)Math.Round(from.x + (to.x - from.x) * t);
				var y = (int)Math.Round(from.y + (to.y - from.y) * t);
				var z = (int)Math.Round(from.z + (to.z - from.z) * t);

				return new Vector3Int(x, y, z);
			}
		}

		public class RectIntBindingAnimation : BindingAnimation<RectInt>
		{
			protected override float GetDistance(RectInt from, RectInt to)
			{
				var xMin = Math.Abs(from.xMin - to.xMin);
				var yMin = Math.Abs(from.yMin - to.yMin);
				var xMax = Math.Abs(from.xMax - to.xMax);
				var yMax = Math.Abs(from.yMax - to.yMax);

				return Math.Max(Math.Max(xMin, xMax), Math.Max(yMin, yMax));
			}

			protected override RectInt Interpolate(RectInt from, RectInt to, float t)
			{
				var xMin = (int)Math.Round(from.xMin + (to.xMin - from.xMin) * t);
				var yMin = (int)Math.Round(from.yMin + (to.yMin - from.yMin) * t);
				var xMax = (int)Math.Round(from.xMax + (to.xMax - from.xMax) * t);
				var yMax = (int)Math.Round(from.yMax + (to.yMax - from.yMax) * t);

				return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
			}
		}

		public class BoundsIntBindingAnimation : BindingAnimation<BoundsInt>
		{
			protected override float GetDistance(BoundsInt from, BoundsInt to)
			{
				var xMin = Math.Abs(from.min.x - to.min.x);
				var yMin = Math.Abs(from.min.y - to.min.y);
				var zMin = Math.Abs(from.min.z - to.min.z);
				var xMax = Math.Abs(from.max.x - to.max.x);
				var yMax = Math.Abs(from.max.y - to.max.y);
				var zMax = Math.Abs(from.max.z - to.max.z);

				return Math.Max(Math.Max(Math.Max(xMin, xMax), Math.Max(yMin, yMax)), Math.Max(zMin, zMax));
			}

			protected override BoundsInt Interpolate(BoundsInt from, BoundsInt to, float t)
			{
				var xMin = (int)Math.Round(from.min.x + (to.min.x - from.min.x) * t);
				var yMin = (int)Math.Round(from.min.y + (to.min.y - from.min.y) * t);
				var zMin = (int)Math.Round(from.min.z + (to.min.z - from.min.z) * t);
				var xMax = (int)Math.Round(from.max.x + (to.max.x - from.max.x) * t);
				var yMax = (int)Math.Round(from.max.y + (to.max.y - from.max.y) * t);
				var zMax = (int)Math.Round(from.max.z + (to.max.z - from.max.z) * t);

				return new BoundsInt
				{
					min = new Vector3Int(xMin, yMin, zMin),
					max = new Vector3Int(xMax, yMax, zMax)
				};
			}
		}

		public class ColorBindingAnimation : BindingAnimation<Color>
		{
			protected override float GetDistance(Color from, Color to) => Vector4.Distance(from, to);
			protected override Color Interpolate(Color from, Color to, float t) => Color.Lerp(from, to, t);
		}

		#endregion

		#region Other

		private class BoolBindingAnimation : BindingAnimation<bool>
		{
			protected override float GetDistance(bool from, bool to) => from == to ? 0 : 1;
			protected override bool Interpolate(bool from, bool to, float t) => t < 0.5f ? from : to;
		}

		#endregion

		#endregion

		#region Tools Interface

		public static List<ClassPoolInfo> Pool => _pool.GetPoolInfo();

		#endregion
	}

	public abstract class BindingAnimation<Type> : IBindingTransition<Type>
	{
		System.Type IBindingTransition.TransitionType => typeof(Type);
		void IBindingTransition.Release() => BindingAnimation.Release(this);

		public bool IsInProgress => _elapsed >= 0.0f && _elapsed < _duration;
		public Type CurrentValue { get; private set; }

		private float _speed = 0.0f;
		private float _duration = 2.0f;
		private float _elapsed = -1.0f;

		public float Speed
		{
			get => _speed;
			set => SetSpeed(value);
		}

		public float Duration
		{
			get => _duration;
			set => SetDuration(value);
		}

		public virtual void Start(Type from, Type to)
		{
			if (_speed > 0.0f)
				_duration = GetDistance(from, to) / _speed;

			_elapsed = 0.0f;
			CurrentValue = from;
		}

		public virtual void Update(float elapsed, Type from, Type to)
		{
			_elapsed += elapsed;
			CurrentValue = Interpolate(from, to, _elapsed / _duration);
		}

		private void SetDuration(float seconds)
		{
			_duration = seconds;
			_speed = 0.0f;
		}

		private void SetSpeed(float perSecond)
		{
			_duration = 0.0f;
			_speed = perSecond;
		}

		protected abstract float GetDistance(Type from, Type to);
		protected abstract Type Interpolate(Type from, Type to, float t);
	}
}
