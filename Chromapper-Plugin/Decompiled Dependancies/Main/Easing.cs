using System;
using System.Collections.Generic;
using UnityEngine;

public class Easing
{
	public class Quadratic
	{
		public static float In(float k)
		{
			return k * k;
		}

		public static float Out(float k)
		{
			return k * (2f - k);
		}

		public static float InOut(float k)
		{
			if (!((k *= 2f) < 1f))
			{
				return -0.5f * ((k -= 1f) * (k - 2f) - 1f);
			}
			return 0.5f * k * k;
		}
	}

	public class Cubic
	{
		public static float In(float k)
		{
			return k * k * k;
		}

		public static float Out(float k)
		{
			return 1f + (k -= 1f) * k * k;
		}

		public static float InOut(float k)
		{
			if (!((k *= 2f) < 1f))
			{
				return 0.5f * ((k -= 2f) * k * k + 2f);
			}
			return 0.5f * k * k * k;
		}
	}

	public class Quartic
	{
		public static float In(float k)
		{
			return k * k * k * k;
		}

		public static float Out(float k)
		{
			return 1f - (k -= 1f) * k * k * k;
		}

		public static float InOut(float k)
		{
			if (!((k *= 2f) < 1f))
			{
				return -0.5f * ((k -= 2f) * k * k * k - 2f);
			}
			return 0.5f * k * k * k * k;
		}
	}

	public class Quintic
	{
		public static float In(float k)
		{
			return k * k * k * k * k;
		}

		public static float Out(float k)
		{
			return 1f + (k -= 1f) * k * k * k * k;
		}

		public static float InOut(float k)
		{
			if (!((k *= 2f) < 1f))
			{
				return 0.5f * ((k -= 2f) * k * k * k * k + 2f);
			}
			return 0.5f * k * k * k * k * k;
		}
	}

	public class Sinusoidal
	{
		public static float In(float k)
		{
			return 1f - Mathf.Cos(k * MathF.PI / 2f);
		}

		public static float Out(float k)
		{
			return Mathf.Sin(k * MathF.PI / 2f);
		}

		public static float InOut(float k)
		{
			return 0.5f * (1f - Mathf.Cos(MathF.PI * k));
		}
	}

	public class Exponential
	{
		public static float In(float k)
		{
			if (k != 0f)
			{
				return Mathf.Pow(1024f, k - 1f);
			}
			return 0f;
		}

		public static float Out(float k)
		{
			if (k != 1f)
			{
				return 1f - Mathf.Pow(2f, -10f * k);
			}
			return 1f;
		}

		public static float InOut(float k)
		{
			if (k == 0f)
			{
				return 0f;
			}
			if (k == 1f)
			{
				return 1f;
			}
			if ((k *= 2f) < 1f)
			{
				return 0.5f * Mathf.Pow(1024f, k - 1f);
			}
			return 0.5f * (0f - Mathf.Pow(2f, -10f * (k - 1f)) + 2f);
		}
	}

	public class Circular
	{
		public static float In(float k)
		{
			return 1f - Mathf.Sqrt(1f - k * k);
		}

		public static float Out(float k)
		{
			return Mathf.Sqrt(1f - (k -= 1f) * k);
		}

		public static float InOut(float k)
		{
			if (!((k *= 2f) < 1f))
			{
				return 0.5f * (Mathf.Sqrt(1f - (k -= 2f) * k) + 1f);
			}
			return -0.5f * (Mathf.Sqrt(1f - k * k) - 1f);
		}
	}

	public class Elastic
	{
		public static float In(float k)
		{
			if (k == 0f)
			{
				return 0f;
			}
			if (k == 1f)
			{
				return 1f;
			}
			return (0f - Mathf.Pow(2f, 10f * (k -= 1f))) * Mathf.Sin((k - 0.1f) * (MathF.PI * 2f) / 0.4f);
		}

		public static float Out(float k)
		{
			if (k == 0f)
			{
				return 0f;
			}
			if (k == 1f)
			{
				return 1f;
			}
			return Mathf.Pow(2f, -10f * k) * Mathf.Sin((k - 0.1f) * (MathF.PI * 2f) / 0.4f) + 1f;
		}

		public static float InOut(float k)
		{
			if (!((k *= 2f) < 1f))
			{
				return Mathf.Pow(2f, -10f * (k -= 1f)) * Mathf.Sin((k - 0.1f) * (MathF.PI * 2f) / 0.4f) * 0.5f + 1f;
			}
			return -0.5f * Mathf.Pow(2f, 10f * (k -= 1f)) * Mathf.Sin((k - 0.1f) * (MathF.PI * 2f) / 0.4f);
		}
	}

	public class Back
	{
		private static readonly float s = 1.70158f;

		private static readonly float s2 = 2.5949094f;

		public static float In(float k)
		{
			return k * k * ((s + 1f) * k - s);
		}

		public static float Out(float k)
		{
			return (k -= 1f) * k * ((s + 1f) * k + s) + 1f;
		}

		public static float InOut(float k)
		{
			if ((k *= 2f) < 1f)
			{
				return 0.5f * (k * k * ((s2 + 1f) * k - s2));
			}
			return 0.5f * ((k -= 2f) * k * ((s2 + 1f) * k + s2) + 2f);
		}
	}

	public class Bounce
	{
		public static float In(float k)
		{
			return 1f - Out(1f - k);
		}

		public static float Out(float k)
		{
			if (k < 0.36363637f)
			{
				return 7.5625f * k * k;
			}
			if (k < 0.72727275f)
			{
				return 7.5625f * (k -= 0.54545456f) * k + 0.75f;
			}
			if (k < 0.90909094f)
			{
				return 7.5625f * (k -= 0.8181818f) * k + 0.9375f;
			}
			return 7.5625f * (k -= 21f / 22f) * k + 63f / 64f;
		}

		public static float InOut(float k)
		{
			if (k < 0.5f)
			{
				return In(k * 2f) * 0.5f;
			}
			return Out(k * 2f - 1f) * 0.5f + 0.5f;
		}
	}

	public class BeatSaber
	{
		public static float EaseVNJS(int? easingType, float k)
		{
			switch (easingType)
			{
			case 4:
			case 5:
			case 6:
			case 7:
			case 8:
			case 9:
			case 10:
			case 11:
			case 12:
			case 13:
			case 14:
			case 15:
			case 16:
			case 17:
			case 18:
				return 0f;
			default:
				return Ease(easingType, k);
			}
		}

		public static float Ease(int? easingType, float k)
		{
			switch (easingType)
			{
			case -1:
				return 0f;
			case 0:
				return Linear(k);
			case 1:
				return Quadratic.In(k);
			case 2:
				return Quadratic.Out(k);
			case 3:
				return Quadratic.InOut(k);
			case 4:
				return Sinusoidal.In(k);
			case 5:
				return Sinusoidal.Out(k);
			case 6:
				return Sinusoidal.InOut(k);
			case 7:
				return Cubic.In(k);
			case 8:
				return Cubic.Out(k);
			case 9:
				return Cubic.InOut(k);
			case 10:
				return Quartic.In(k);
			case 11:
				return Quartic.Out(k);
			case 12:
				return Quartic.InOut(k);
			case 13:
				return Quintic.In(k);
			case 14:
				return Quintic.Out(k);
			case 15:
				return Quintic.InOut(k);
			case 16:
				return Exponential.In(k);
			case 17:
				return Exponential.Out(k);
			case 18:
				return Exponential.InOut(k);
			case 19:
				return Circular.In(k);
			case 20:
				return Circular.Out(k);
			case 21:
				return Circular.InOut(k);
			case 22:
				return Back.In(k);
			case 23:
				return Back.Out(k);
			case 24:
				return Back.InOut(k);
			case 25:
				return Elastic.In(k);
			case 26:
				return Elastic.Out(k);
			case 27:
				return Elastic.InOut(k);
			case 28:
				return Bounce.In(k);
			case 29:
				return Bounce.Out(k);
			case 30:
				return Bounce.InOut(k);
			case 100:
			case 101:
			case 102:
				return 0f;
			default:
				return 0f;
			}
		}
	}

	public static Dictionary<string, Func<float, float>> ByName = new Dictionary<string, Func<float, float>>
	{
		{ "easeLinear", Linear },
		{
			"easeInQuad",
			Quadratic.In
		},
		{
			"easeOutQuad",
			Quadratic.Out
		},
		{
			"easeInOutQuad",
			Quadratic.InOut
		},
		{
			"easeInCubic",
			Cubic.In
		},
		{
			"easeOutCubic",
			Cubic.Out
		},
		{
			"easeInOutCubic",
			Cubic.InOut
		},
		{
			"easeInQuart",
			Quartic.In
		},
		{
			"easeOutQuart",
			Quartic.Out
		},
		{
			"easeInOutQuart",
			Quartic.InOut
		},
		{
			"easeInQuint",
			Quintic.In
		},
		{
			"easeOutQuint",
			Quintic.Out
		},
		{
			"easeInOutQuint",
			Quintic.InOut
		},
		{
			"easeInSine",
			Sinusoidal.In
		},
		{
			"easeOutSine",
			Sinusoidal.Out
		},
		{
			"easeInOutSine",
			Sinusoidal.InOut
		},
		{
			"easeInExpo",
			Exponential.In
		},
		{
			"easeOutExpo",
			Exponential.Out
		},
		{
			"easeInOutExpo",
			Exponential.InOut
		},
		{
			"easeInCirc",
			Circular.In
		},
		{
			"easeOutCirc",
			Circular.Out
		},
		{
			"easeInOutCirc",
			Circular.InOut
		},
		{
			"easeInBack",
			Back.In
		},
		{
			"easeOutBack",
			Back.Out
		},
		{
			"easeInOutBack",
			Back.InOut
		},
		{
			"easeInElastic",
			Elastic.In
		},
		{
			"easeOutElastic",
			Elastic.Out
		},
		{
			"easeInOutElastic",
			Elastic.InOut
		},
		{
			"easeInBounce",
			Bounce.In
		},
		{
			"easeOutBounce",
			Bounce.Out
		},
		{
			"easeInOutBounce",
			Bounce.InOut
		},
		{ "easeStep", Step }
	};

	public static Dictionary<string, string> DisplayNameToInternalName = new Dictionary<string, string>
	{
		{ "Linear", "easeLinear" },
		{ "Quadratic In", "easeInQuad" },
		{ "Quadratic Out", "easeOutQuad" },
		{ "Quadratic In/Out", "easeInOutQuad" },
		{ "Cubic In", "easeInCubic" },
		{ "Cubic Out", "easeOutCubic" },
		{ "Cubic In/Out", "easeInOutCubic" },
		{ "Quartic In", "easeInQuart" },
		{ "Quartic Out", "easeOutQuart" },
		{ "Quartic In/Out", "easeInOutQuart" },
		{ "Quintic In", "easeInQuint" },
		{ "Quintic Out", "easeOutQuint" },
		{ "Quintic In/Out", "easeInOutQuint" },
		{ "Sine In", "easeInSine" },
		{ "Sine Out", "easeOutSine" },
		{ "Sine In/Out", "easeInOutSine" },
		{ "Exponential In", "easeInExpo" },
		{ "Exponential Out", "easeOutExpo" },
		{ "Exponential In/Out", "easeInOutExpo" },
		{ "Circular In", "easeInCirc" },
		{ "Circular Out", "easeOutCirc" },
		{ "Circular In/Out", "easeInOutCirc" },
		{ "Back In", "easeInBack" },
		{ "Back Out", "easeOutBack" },
		{ "Back In/Out", "easeInOutBack" },
		{ "Elastic In", "easeInElastic" },
		{ "Elastic Out", "easeOutElastic" },
		{ "Elastic In/Out", "easeInOutElastic" },
		{ "Bounce In", "easeInBounce" },
		{ "Bounce Out", "easeOutBounce" },
		{ "Bounce In/Out", "easeInOutBounce" },
		{ "Step", "easeStep" }
	};

	public static Func<float, float> Named(string name)
	{
		if (ByName.TryGetValue(name, out var value))
		{
			return value;
		}
		return Linear;
	}

	public static int EasingShaderId(string easingId)
	{
		int num = 0;
		foreach (string key in ByName.Keys)
		{
			if (key == easingId)
			{
				return num;
			}
			num++;
		}
		return 0;
	}

	public static float Linear(float k)
	{
		return k;
	}

	public static float Step(float k)
	{
		return Mathf.Floor(k);
	}
}
