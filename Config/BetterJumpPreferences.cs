using System;
using MelonLoader;
using UnityEngine;

namespace BetterJump.Config
{
	internal static class BetterJumpPreferences
	{
		private const string CategoryId = "BetterJump";

		private static MelonPreferences_Category _category;
		private static MelonPreferences_Entry<bool> _enabled;
		private static MelonPreferences_Entry<float> _jumpVelocity;
		private static MelonPreferences_Entry<float> _forceUngroundTime;
		private static MelonPreferences_Entry<float> _airGravityScale;
		private static MelonPreferences_Entry<bool> _debugLogs;

		internal static void Initialize()
		{
			if (_category != null)
			{
				return;
			}

			_category = MelonPreferences.CreateCategory(CategoryId, "BetterJump");
			_enabled = CreateEntry("Enabled", true, "Enabled", "Enable BetterJump functionality. When disabled, the mod will not modify jump behavior.");
			_jumpVelocity = CreateEntry("JumpVelocity", 4.8f, "Jump Velocity", "Upward speed applied when a jump starts (units/second). Higher values result in stronger jumps. Default: 4.8");
			_forceUngroundTime = CreateEntry("ForceUngroundTime", 0.08f, "Force Unground Time", "Seconds to keep avatar airborne before the next ground check can succeed. This prevents the game from immediately detecting the ground after a jump, allowing for better jump responsiveness. Default: 0.08");
			_airGravityScale = CreateEntry("AirGravityScale", 0.7f, "Air Gravity Scale", "Gravity multiplier applied during the fast part of the RISE (lower = floatier, higher arc). Near the apex it blends back to normal gravity so there is no hovering 'platform' at the top, and the descent always uses normal gravity for crisp landings. 1.0 disables the effect. Default: 0.7");
			_debugLogs = CreateEntry("DebugLogs", false, "Debug Logs", "Enable detailed debug logging for troubleshooting jump behavior.");
		}

		private static MelonPreferences_Entry<T> CreateEntry<T>(string identifier, T defaultValue, string displayName, string description = null)
		{
			if (_category == null)
			{
				throw new InvalidOperationException("Preference category not initialized.");
			}

			return _category.CreateEntry(identifier, defaultValue, displayName, description);
		}

		internal static bool Enabled => _enabled.Value;

		internal static float JumpVelocity => Mathf.Max(0f, _jumpVelocity.Value);

		internal static float ForceUngroundTime => Mathf.Max(0.01f, _forceUngroundTime.Value);

		internal static float AirGravityScale => Mathf.Clamp(_airGravityScale.Value, 0.1f, 1f);

		internal static bool DebugLogs => _debugLogs.Value;
	}
}

