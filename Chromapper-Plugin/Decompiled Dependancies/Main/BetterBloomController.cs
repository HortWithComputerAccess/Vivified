using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal.Internal;

public class BetterBloomController : MonoBehaviour
{
	private const string betterBloomID = "com.caeden117.chromapper.betterbloom";

	private Harmony betterBloomHarmony;

	private void Start()
	{
		betterBloomHarmony = new Harmony("com.caeden117.chromapper.betterbloom");
		if (Settings.Instance.HighQualityBloom)
		{
			MethodBase method = typeof(PostProcessPass).GetMethod("SetupBloom", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.DefaultBinder, new Type[3]
			{
				typeof(CommandBuffer),
				typeof(RenderTargetIdentifier),
				typeof(Material)
			}, new ParameterModifier[0]);
			HarmonyMethod transpiler = new HarmonyMethod(typeof(BetterBloomController), "PatchSetupBloom");
			betterBloomHarmony.Patch(method, null, null, transpiler);
		}
	}

	private void OnDestroy()
	{
		betterBloomHarmony.UnpatchSelf();
	}

	private static IEnumerable<CodeInstruction> PatchSetupBloom(IEnumerable<CodeInstruction> insns)
	{
		List<CodeInstruction> list = new List<CodeInstruction>();
		int num = 0;
		bool flag = false;
		foreach (CodeInstruction insn in insns)
		{
			if (num < 2)
			{
				if (!flag && insn.opcode == OpCodes.Ldc_I4_1)
				{
					flag = true;
				}
				else if (flag && insn.opcode == OpCodes.Shr)
				{
					flag = false;
					num++;
					continue;
				}
			}
			if (!flag)
			{
				list.Add(insn);
			}
		}
		return list;
	}
}
