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

		// Sicherheits-Backstop: Selbst wenn der Actor nie landet (z. B. Sturz von einer Klippe
		// in eine Kill-Zone), wird die reduzierte Flug-Schwerkraft spätestens nach dieser Zeit
		// wieder freigegeben, damit kein dauerhaftes Schweben/State-Leak entsteht.
		private const float MaxAirGravitySeconds = 8f;

		// Der Boost wird erst nach dieser Verweildauer am Boden wieder freigegeben. Grund:
		// CheckGrounded meldet grounded bereits ~0,1-0,15 m über dem Boden (großzügige SphereCast-
		// Zone, checkGroundedDistance = 0.5), und im langsamen Flug-Sinkflug verweilt der Avatar
		// dort mehrere Frames. Ohne diese Verriegelung könnte man genau dort in der Luft erneut
		// "nachspringen" und sich hochstapeln. 0,15 s deckt selbst die floatigsten Settings ab.
		private const float LandSettleSeconds = 0.15f;

		// Ab dieser Steiggeschwindigkeit (m/s) wirkt die reduzierte Schwerkraft voll; darunter wird
		// linear auf normale Schwerkraft zurückgeblendet. Verhindert das lange Verharren am Scheitel
		// ("unsichtbare Plattform"), ohne den floatigen Aufstieg zu verlieren.
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

			// Läuft bereits ein durch BetterJump geboosteter Sprung (Avatar noch nicht sauber
			// gelandet)? Dann keinen weiteren Boost geben. Verhindert Mid-Air-"Nachspringen".
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
			// Game-Update 0.3.0: UpdateControl() setzt beim Übergang grounded->airborne
			// (wasGrounded == true && grounded == false) "falling = 0f" und löscht damit den
			// Sprung-Impuls. wasGrounded muss daher mit-zurückgesetzt werden, sonst hat der
			// Sprung keine vertikale Wirkung mehr.
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

			// Phase 1: kurzes erzwungenes Unground-Fenster direkt nach dem Absprung, damit die
			// grounded-Branch in UpdateControl den Steig-Impuls nicht abwürgt (deckt die ~0,4 m
			// Steighöhe ab, in denen der Avatar die Boden-Erkennungszone noch nicht verlassen hat).
			if (Time.time < state.ForceUngroundUntil)
			{
				SetGrounded(actor, false);
				// Während des erzwungenen Unground-Fensters konsistent halten, damit die neue
				// wasGrounded-Logik in UpdateControl() den Steig-Impuls nicht doch noch nullt.
				SetWasGrounded(actor, false);
				state.GroundedSince = -1f;
				ApplyAirGravity(actor, state);
				return;
			}

			// Phase 2: Fenster vorbei.
			if (GetGrounded(actor))
			{
				// Bodenkontakt erkannt - aber erst nach LandSettleSeconds durchgehend am Boden als
				// "gelandet" werten und den Boost wieder freigeben (siehe LandSettleSeconds).
				if (state.GroundedSince < 0f)
				{
					state.GroundedSince = Time.time;
				}
				if (Time.time - state.GroundedSince >= LandSettleSeconds)
				{
					States.Remove(actor);
				}
				// grounded == true -> das Spiel steuert die Vertikalbewegung; nichts weiter tun.
				return;
			}

			// In der Luft: Verweildauer zurücksetzen und (nur im Steigflug) den Flugbogen abbremsen.
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

		// Diagnose: misst per Raycast die tatsächliche Distanz Füße->nächste Geometrie nach unten
		// und nennt das getroffene Objekt. Zeigt, ob ein mid-air grounded auf echte Geometrie trifft.
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

		// Verlängert die Airtime durch eine reduzierte effektive Schwerkraft - aber NUR im Steigflug.
		// UpdateControl() addiert im selben Frame NACH diesem Postfix-Hook (CheckGrounded läuft
		// laut Update() vor UpdateControl) "falling += Physics.gravity.y * deltaTime". Wir addieren
		// vorab den fehlenden Anteil, sodass die Netto-Schwerkraft im Steigflug = scale * Original ergibt.
		private static void ApplyAirGravity(ProtoActor actor, ActorState state)
		{
			float scale = BetterJumpPreferences.AirGravityScale;
			if (scale >= 1f)
			{
				return;
			}

			// Nur den Steigflug abbremsen (falling > 0). Der Sinkflug bleibt bei normaler Schwerkraft:
			// Sonst nähert man sich dem Boden so langsam, dass die grounded-Branch des Spiels die
			// Sinkgeschwindigkeit abrupt auf ~0 nullt und man kurz auf einer "unsichtbaren Plattform"
			// stehen bleibt. "Float up, fall normal" ist die saubere, ruckelfreie Variante.
			float current = GetFalling(actor);
			if (current <= 0f)
			{
				return;
			}

			// Exakt einmal pro Frame. In diesem Spiel feuert CheckGrounded (und damit dieser Hook)
			// zweimal pro Frame; ohne die Sperre würde der Gegenimpuls doppelt greifen und der
			// Sprung wäre floatiger als eingestellt.
			if (state.LastGravityTime == Time.time)
			{
				return;
			}
			state.LastGravityTime = Time.time;

			// Float nur solange der Avatar zügig steigt; zum Scheitel hin (kleine Steiggeschwindigkeit)
			// zurück auf normale Schwerkraft blenden. Sonst geht falling sehr langsam durch 0 und der
			// Avatar verharrt kurz am Apex = die "unsichtbare Plattform".
			float blend = Mathf.Clamp01(current / ApexBlendSpeed);
			float effectiveScale = Mathf.Lerp(1f, scale, blend);
			float counter = Physics.gravity.y * Time.deltaTime * (effectiveScale - 1f);
			SetFalling(actor, current + counter);
		}
	}
}
