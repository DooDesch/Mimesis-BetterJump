using System;
using MelonLoader;
using UnityEngine;

#nullable enable

namespace BetterJump.Config
{
	internal static class BetterJumpPreferences
	{
		private const string CategoryId = "BetterJump";

		private static MelonPreferences_Category? _category;
		private static MelonPreferences_Entry<bool>? _enabled;
		private static MelonPreferences_Entry<float>? _jumpVelocity;
		private static MelonPreferences_Entry<float>? _forceUngroundTime;

		internal static void Initialize()
		{
			if (_category != null)
			{
				return;
			}

			_category = MelonPreferences.CreateCategory(CategoryId, "BetterJump");
			_enabled = CreateEntry("Enabled", true, "Enable BetterJump functionality");
			_jumpVelocity = CreateEntry("JumpVelocity", 5.2f, "Jump velocity", "Upward speed applied when a jump starts (units/second).");
			_forceUngroundTime = CreateEntry("ForceUngroundTime", 0.08f, "Force unground time", "Seconds to keep avatar airborne before the next ground check can succeed.");
		}

		private static MelonPreferences_Entry<T> CreateEntry<T>(string identifier, T defaultValue, string displayName, string? description = null)
		{
			if (_category == null)
			{
				throw new InvalidOperationException("Preference category not initialized.");
			}

			return _category.CreateEntry(identifier, defaultValue, displayName, description);
		}

		internal static bool Enabled => _enabled?.Value ?? true;

		internal static float JumpVelocity => Mathf.Max(0f, _jumpVelocity?.Value ?? 6.5f);

		internal static float ForceUngroundTime => Mathf.Max(0.01f, _forceUngroundTime?.Value ?? 0.12f);
	}
}

