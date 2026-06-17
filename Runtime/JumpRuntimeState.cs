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
			public float AirGravityUntil;
			public float GroundedSince;
			public float LastGravityTime;
		}

		// Safety backstop: even if the actor never lands (e.g. falling off a cliff into a kill
		// zone), the reduced air gravity is released after this time at the latest, so there is
		// no permanent floating / state leak.
		private const float MaxAirGravitySeconds = 8f;

		// The boost is only re-armed after this much continuous ground contact. Reason:
		// CheckGrounded already reports grounded ~0.1-0.15 m above the ground (generous SphereCast
		// zone, checkGroundedDistance = 0.5), and during a slow floaty descent the avatar lingers
		// there for several frames. Without this latch you could re-jump in mid-air right there
		// and climb upwards. 0.15 s covers even the floatiest settings.
		private const float LandSettleSeconds = 0.15f;

		// At or above this rising speed (m/s) the reduced gravity applies fully; below it, gravity
		// is blended linearly back to normal. Prevents the long lingering at the apex
		// ("invisible platform") without losing the floaty rise.
		private const float ApexBlendSpeed = 2.5f;

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

			bool avatar = owner.AmIAvatar();
			bool grounded = GetGrounded(owner);
			bool inProgress = States.ContainsKey(owner);

			if (BetterJumpPreferences.DebugLogs)
			{
				MelonLogger.Msg("[DBG] StartJump trigger: avatar={0} grounded={1} inProgress={2} y={3:F2} falling={4:F2}", avatar, grounded, inProgress, GetHeight(owner), GetFalling(owner));
			}

			if (!avatar || !grounded)
			{
				return;
			}

			// Is a BetterJump-boosted jump already in progress (avatar has not cleanly landed yet)?
			// Then do not apply another boost. Prevents mid-air re-jumping.
			if (inProgress)
			{
				return;
			}

			if (BetterJumpPreferences.DebugLogs)
			{
				MelonLogger.Msg("[DBG]   -> BOOST falling={0:F2}", BetterJumpPreferences.JumpVelocity);
			}

			SetFalling(owner, BetterJumpPreferences.JumpVelocity);
			SetGrounded(owner, false);
			// Game update 0.3.0: UpdateControl() sets "falling = 0f" on the grounded->airborne
			// transition (wasGrounded == true && grounded == false), which wipes the jump impulse.
			// wasGrounded must therefore be reset as well, otherwise the jump has no vertical
			// effect anymore.
			SetWasGrounded(owner, false);

			var state = new ActorState
			{
				ForceUngroundUntil = Time.time + Mathf.Max(0.02f, BetterJumpPreferences.ForceUngroundTime),
				AirGravityUntil = Time.time + MaxAirGravitySeconds,
				GroundedSince = -1f,
				LastGravityTime = -1f
			};
			States[owner] = state;
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

			if (BetterJumpPreferences.DebugLogs)
			{
				string phase = Time.time < state.ForceUngroundUntil ? "FORCE" : (GetGrounded(actor) ? "GROUND" : "AIR");
				MelonLogger.Msg("[DBG] tick {0}: y={1:F2} falling={2:F2} grounded={3} wasGrounded={4} ground={5} t={6:F2}", phase, GetHeight(actor), GetFalling(actor), GetGrounded(actor), GetWasGrounded(actor), GroundProbe(actor), Time.time);
			}

			// Phase 1: short forced-unground window right after takeoff so the grounded branch in
			// UpdateControl does not kill the upward impulse (covers the ~0.4 m of rise during which
			// the avatar has not yet left the ground-detection zone).
			if (Time.time < state.ForceUngroundUntil)
			{
				SetGrounded(actor, false);
				// Keep it consistent during the forced-unground window so the new wasGrounded logic
				// in UpdateControl() does not zero the upward impulse after all.
				SetWasGrounded(actor, false);
				state.GroundedSince = -1f;
				ApplyAirGravity(actor, state);
				return;
			}

			// Phase 2: window is over.
			if (GetGrounded(actor))
			{
				// Ground contact detected - but only treat it as "landed" (and re-arm the boost)
				// after LandSettleSeconds of continuous ground contact (see LandSettleSeconds).
				if (state.GroundedSince < 0f)
				{
					state.GroundedSince = Time.time;
				}
				if (Time.time - state.GroundedSince >= LandSettleSeconds)
				{
					States.Remove(actor);
				}
				// grounded == true -> the game drives the vertical movement; nothing else to do.
				return;
			}

			// Airborne: reset the dwell timer and (only while rising) slow down the arc.
			state.GroundedSince = -1f;
			if (Time.time >= state.AirGravityUntil)
			{
				States.Remove(actor);
				return;
			}

			ApplyAirGravity(actor, state);
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

		private static void SetWasGrounded(ProtoActor actor, bool value) => ReflectionHelper.SetFieldValue(actor, "wasGrounded", value);

		private static float GetFalling(ProtoActor actor) => ReflectionHelper.GetFieldValue<float>(actor, "falling");

		private static bool GetWasGrounded(ProtoActor actor) => ReflectionHelper.GetFieldValue<bool>(actor, "wasGrounded");

		private static float GetHeight(ProtoActor actor) => actor != null ? actor.transform.position.y : 0f;

		// Diagnostics: raycasts downward to measure the actual distance feet->nearest geometry and
		// names the hit object. Shows whether a mid-air grounded hits real geometry.
		private static string GroundProbe(ProtoActor actor)
		{
			if (actor == null)
			{
				return "n/a";
			}
			Vector3 origin = actor.transform.position + Vector3.up * 0.1f;
			if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 20f, ~0, QueryTriggerInteraction.Ignore))
			{
				string layer = LayerMask.LayerToName(hit.collider.gameObject.layer);
				return string.Format("{0:F2}m@{1}[{2}]", hit.distance - 0.1f, hit.collider.name, layer);
			}
			return "none";
		}

		// Extends airtime via a reduced effective gravity - but ONLY while rising. UpdateControl()
		// adds "falling += Physics.gravity.y * deltaTime" in the same frame AFTER this postfix hook
		// (CheckGrounded runs before UpdateControl per Update()). We pre-add the missing part so the
		// net gravity while rising equals scale * original.
		private static void ApplyAirGravity(ProtoActor actor, ActorState state)
		{
			float scale = BetterJumpPreferences.AirGravityScale;
			if (scale >= 1f)
			{
				return;
			}

			// Only slow down the rise (falling > 0). The descent keeps normal gravity: otherwise you
			// approach the ground so slowly that the game's grounded branch abruptly zeroes the
			// descent speed and you briefly stand on an "invisible platform". "Float up, fall normal"
			// is the clean, judder-free variant.
			float current = GetFalling(actor);
			if (current <= 0f)
			{
				return;
			}

			// Exactly once per frame. In this game CheckGrounded (and therefore this hook) fires
			// twice per frame; without this guard the counter-impulse would apply twice and the jump
			// would be floatier than configured.
			if (state.LastGravityTime == Time.time)
			{
				return;
			}
			state.LastGravityTime = Time.time;

			// Float only while the avatar is rising quickly; blend back to normal gravity towards the
			// apex (small rising speed). Otherwise falling crosses 0 very slowly and the avatar
			// lingers briefly at the apex = the "invisible platform".
			float blend = Mathf.Clamp01(current / ApexBlendSpeed);
			float effectiveScale = Mathf.Lerp(1f, scale, blend);
			float counter = Physics.gravity.y * Time.deltaTime * (effectiveScale - 1f);
			SetFalling(actor, current + counter);
		}
	}
}
