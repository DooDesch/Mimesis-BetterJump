using System.Collections.Generic;
using System.Reflection.Emit;
using BetterJump.Runtime;
using HarmonyLib;
using Mimic.Actors;

namespace BetterJump.Patches
{
	[HarmonyPatch(typeof(ProtoActor), "CheckGrounded")]
	internal static class ProtoActorGroundedPatch
	{
		private static void Postfix(ProtoActor __instance)
		{
			JumpRuntimeState.OnGroundCheck(__instance);
		}
	}

	[HarmonyPatch(typeof(ProtoActor), "OnDestroy")]
	internal static class ProtoActorDestroyPatch
	{
		private static void Postfix(ProtoActor __instance)
		{
			JumpRuntimeState.OnActorDestroyed(__instance);
		}
	}
}
