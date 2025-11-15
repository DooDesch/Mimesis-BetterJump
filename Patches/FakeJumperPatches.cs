using BetterJump.Runtime;
using HarmonyLib;
using Mimic.Actors;

namespace BetterJump.Patches
{
	[HarmonyPatch(typeof(ProtoActor.FakeJumper), "StartJump")]
	internal static class FakeJumperPatches
	{
		private static void Postfix(ProtoActor.FakeJumper __instance)
		{
			JumpRuntimeState.OnJumpStarted(__instance);
		}
	}
}

