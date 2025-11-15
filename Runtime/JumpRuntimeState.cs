using System;
using System.Collections.Generic;
using BetterJump.Config;
using MelonLoader;
using Mimic.Actors;
using MimicAPI.GameAPI;
using UnityEngine;

namespace BetterJump.Runtime
{
	internal static class JumpRuntimeState
	{
		private sealed class ActorState
		{
			public float ForceUngroundUntil;
		}

		private static readonly Dictionary<ProtoActor, ActorState> States = new();

		internal static void OnJumpStarted(ProtoActor.FakeJumper fakeJumper)
		{
			if (!BetterJumpPreferences.Enabled || fakeJumper == null)
			{
				return;
			}

			ProtoActor owner = ReflectionHelper.GetFieldValue<ProtoActor>(fakeJumper, "owner");
			if (owner == null)
			{
				return;
			}

			if (!owner.AmIAvatar() || !GetGrounded(owner))
			{
				return;
			}

			SetFalling(owner, BetterJumpPreferences.JumpVelocity);
			SetGrounded(owner, false);

			if (!States.TryGetValue(owner, out var state) || state == null)
			{
				state = new ActorState();
				States[owner] = state;
			}

			state.ForceUngroundUntil = Time.time + Mathf.Max(0.02f, BetterJumpPreferences.ForceUngroundTime);
		}

		internal static void OnGroundCheck(ProtoActor actor)
		{
			if (!BetterJumpPreferences.Enabled)
			{
				return;
			}

			if (!States.TryGetValue(actor, out var state))
			{
				return;
			}

			if (Time.time < state.ForceUngroundUntil)
			{
				SetGrounded(actor, false);
				return;
			}

			States.Remove(actor);
		}

		internal static void OnActorDestroyed(ProtoActor actor)
		{
			States.Remove(actor);
		}

		private static bool GetGrounded(ProtoActor actor)
		{
			object value = ReflectionHelper.GetPropertyValue(actor, "grounded");
			if (value != null && value is bool boolValue)
			{
				return boolValue;
			}
			value = ReflectionHelper.GetFieldValue(actor, "<grounded>k__BackingField");
			if (value != null && value is bool boolValue2)
			{
				return boolValue2;
			}
			return false;
		}

		private static void SetGrounded(ProtoActor actor, bool value)
		{
			ReflectionHelper.SetFieldValue(actor, "<grounded>k__BackingField", value);
		}

		private static void SetFalling(ProtoActor actor, float value) => ReflectionHelper.SetFieldValue(actor, "falling", value);
	}
}
