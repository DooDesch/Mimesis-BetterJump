using BetterJump.Config;
using MelonLoader;

#nullable enable

[assembly: MelonInfo(typeof(BetterJump.Core), "BetterJump", "1.5.2", "DooDesch", null)]
[assembly: MelonGame("ReLUGames", "MIMESIS")]
[assembly: MelonOptionalDependencies("MimicAPI")]

namespace BetterJump
{
	public sealed class Core : MelonMod
	{
		public override void OnInitializeMelon()
		{
			BetterJumpPreferences.Initialize();
			// MelonLoader auto-applies this assembly's Harmony patches via HarmonyInit(); calling PatchAll()
			// here too would double-apply every patch (each prefix/postfix runs twice). Do NOT add it back.
			// (See FakePlayers/Core.cs.)
			MelonLogger.Msg("BetterJump initialized. JumpVelocity={0:F2}, ForceUngroundTime={1:F2}, AirGravityScale={2:F2}", BetterJumpPreferences.JumpVelocity, BetterJumpPreferences.ForceUngroundTime, BetterJumpPreferences.AirGravityScale);
		}
	}
}