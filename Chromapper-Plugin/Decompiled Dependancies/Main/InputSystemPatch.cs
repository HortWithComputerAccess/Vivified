using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputSystemPatch : MonoBehaviour
{
	private const string inputPatchID = "com.caeden117.chromapper.inputpatch";

	private static readonly MethodInfo returnFromFunctionInfo = SymbolExtensions.GetMethodInfo(() => WillReturnFromFunction(null));

	private static IEnumerable<InputAction> allInputActions;

	private static Dictionary<InputAction, IEnumerable<string>> allInputBindingNames = new Dictionary<InputAction, IEnumerable<string>>();

	private static IEnumerable<InputControl> allControls;

	private static readonly ConcurrentDictionary<InputAction, List<InputAction>> inputActionBlockMap = new ConcurrentDictionary<InputAction, List<InputAction>>();

	private Harmony inputPatchHarmony;

	private void Start()
	{
		allInputActions = CMInputCallbackInstaller.InputInstance.asset.actionMaps.SelectMany((InputActionMap x) => x.actions).ToList();
		allInputBindingNames = allInputActions.ToDictionary((InputAction x) => x, (InputAction x) => from y in x.bindings
			where !y.isComposite
			select y.path);
		allControls = InputSystem.devices.SelectMany((InputDevice d) => d.allControls.Where((InputControl c) => c is KeyControl || c is ButtonControl));
		Parallel.ForEach(allInputActions, delegate(InputAction action)
		{
			if (action != null)
			{
				ConcurrentBag<InputAction> map = new ConcurrentBag<InputAction>();
				Parallel.ForEach(allInputActions, delegate(InputAction other)
				{
					if (other != null && WillBeBlockedByAction(action, other))
					{
						map.Add(other);
					}
				});
				inputActionBlockMap.TryAdd(action, map.ToList());
			}
		});
		MethodInfo method = Assembly.GetAssembly(typeof(InputSystem)).GetTypes().First((Type x) => x.Name == "InputActionState")
			.GetMethod("ChangePhaseOfActionInternal", BindingFlags.Instance | BindingFlags.NonPublic);
		inputPatchHarmony = new Harmony("com.caeden117.chromapper.inputpatch");
		inputPatchHarmony.Patch(method, null, null, new HarmonyMethod(GetType(), "Transpiler"));
	}

	private void OnDestroy()
	{
		inputPatchHarmony?.UnpatchSelf();
	}

	private static IEnumerable<CodeInstruction> Transpiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
	{
		List<CodeInstruction> list = instructions.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].opcode == OpCodes.Switch)
			{
				Label label = generator.DefineLabel();
				list[^1] = list[^1].WithLabels(label);
				list.InsertRange(i - 3, new List<CodeInstruction>
				{
					new CodeInstruction(OpCodes.Ldloc_2),
					new CodeInstruction(OpCodes.Call, returnFromFunctionInfo),
					new CodeInstruction(OpCodes.Brtrue_S, label)
				});
				break;
			}
		}
		return list;
	}

	public static bool WillReturnFromFunction(InputAction action)
	{
		if (!inputActionBlockMap.TryGetValue(action, out var value))
		{
			return false;
		}
		foreach (InputAction item in value)
		{
			if (!CMInputCallbackInstaller.IsActionMapDisabled(item.GetType()) && item.controls.All((InputControl x) => x.IsPressed() || x.IsActuated()))
			{
				return true;
			}
		}
		return false;
	}

	private static bool CheckEqualPaths(string pathA, string pathB)
	{
		return InputSystem.FindControl(pathA).GetHashCode() == InputSystem.FindControl(pathB).GetHashCode();
	}

	private static string StripString(string source, params char[] toRemove)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(source.Where((char ch) => !Enumerable.Contains(toRemove, ch)));
		return stringBuilder.ToString();
	}

	private static bool WillBeBlockedByAction(InputAction action, InputAction otherAction)
	{
		if (!action.actionMap.controlSchemes.Any((InputControlScheme c) => c.name.Contains("ChroMapper")))
		{
			return false;
		}
		if (action.bindings.Any((InputBinding b) => b.action.StartsWith(KeybindsController.PersistentKeybindIdentifier)))
		{
			return false;
		}
		if (action.id == otherAction.id || !allInputBindingNames.TryGetValue(action, out var value) || !allInputBindingNames.TryGetValue(otherAction, out var otherPaths))
		{
			return false;
		}
		bool num = otherPaths.Count() > value.Count();
		bool flag = value.All((string p1) => otherPaths.Any((string pathB) => CheckEqualPaths(p1, pathB)));
		return num && flag;
	}
}
