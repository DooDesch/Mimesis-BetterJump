using BetterJump.Config;
using MelonLoader;

#nullable enable

[assembly: MelonInfo(typeof(BetterJump.Core), "BetterJump", "1.0.0", "DooDesch", null)]
[assembly: MelonGame("ReLUGames", "MIMESIS")]
[assembly: MelonOptionalDependencies("MimicAPI")]

namespace BetterJump
{
	public sealed class Core : MelonMod
	{
		public override void OnInitializeMelon()
		{
			BetterJumpPreferences.Initialize();
			HarmonyInstance.PatchAll();
			MelonLogger.Msg("BetterJump initialized. JumpVelocity={0:F2}, ForceUngroundTime={1:F2}", BetterJumpPreferences.JumpVelocity, BetterJumpPreferences.ForceUngroundTime);
		}
	}
}