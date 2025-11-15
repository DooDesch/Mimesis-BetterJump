using System;
using System.Collections.Generic;
using System.Reflection;
using BetterJump.Config;
using HarmonyLib;
using Mimic.Actors;
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

		private static readonly FieldInfo GroundedField = AccessTools.Field(typeof(ProtoActor), "<grounded>k__BackingField")
			?? throw new MissingMemberException("ProtoActor.<grounded>k__BackingField");

		private static readonly FieldInfo FallingField = AccessTools.Field(typeof(ProtoActor), "falling")
			?? throw new MissingMemberException("ProtoActor.falling");

		private static readonly FieldInfo FakeJumperOwnerField = AccessTools.Field(typeof(ProtoActor.FakeJumper), "owner")
			?? throw new MissingMemberException("ProtoActor.FakeJumper.owner");

		internal static void OnJumpStarted(ProtoActor.FakeJumper fakeJumper)
		{
			if (!BetterJumpPreferences.Enabled || fakeJumper == null)
			{
				return;
			}

			if (FakeJumperOwnerField.GetValue(fakeJumper) is not ProtoActor owner)
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

		private static bool GetGrounded(ProtoActor actor) => (bool)GroundedField.GetValue(actor)!;

		private static void SetGrounded(ProtoActor actor, bool value) => GroundedField.SetValue(actor, value);

		private static void SetFalling(ProtoActor actor, float value) => FallingField.SetValue(actor, value);
	}
}

