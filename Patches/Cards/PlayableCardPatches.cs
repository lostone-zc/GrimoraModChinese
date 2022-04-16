﻿using System.Collections;
using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;
using static GrimoraMod.GrimoraPlugin;

namespace GrimoraMod;

[HarmonyPatch(typeof(PlayableCard))]
public class PlayableCardPatches
{
	[HarmonyPostfix, HarmonyPatch(nameof(PlayableCard.Die))]
	public static IEnumerator ExtendDieMethod(
		IEnumerator enumerator,
		PlayableCard __instance,
		bool wasSacrifice,
		PlayableCard killer = null,
		bool playSound = true
	)
	{
		yield return __instance.DieCustom(wasSacrifice, killer, playSound);
	}

	[HarmonyPostfix, HarmonyPatch(nameof(PlayableCard.GetOpposingSlots))]
	public static void PossessiveGetOpposingSlotsPatch(PlayableCard __instance, ref List<CardSlot> __result)
	{
		if (__instance.Slot.opposingSlot.CardIsNotNullAndHasAbility(Possessive.ability))
		{
			var adjSlots = BoardManager.Instance
			 .GetAdjacentSlots(__instance.Slot)
			 .Where(_ => _.Card)
			 .ToList();

			__result = new List<CardSlot>();
			if (adjSlots.Any())
			{
				CardSlot slotToTarget = adjSlots.GetRandomItem();
				// Log.LogDebug($"[OpposingPatches.Possessive] Slot targeted for attack [{slotToTarget.Index}]");
				__result.Add(slotToTarget);
			}
		}
	}

	[HarmonyPostfix, HarmonyPatch(nameof(PlayableCard.CanAttackDirectly))]
	public static void PossessiveCanAttackDirectlyPatch(PlayableCard __instance, CardSlot opposingSlot, ref bool __result)
	{
		Log.LogDebug($"[Possessive.CanAttackDirectly] Result before [{__result}]");
		bool oppositeSlotHasPossessive = __instance.Slot.opposingSlot.CardIsNotNullAndHasAbility(Possessive.ability);
		__result &= !oppositeSlotHasPossessive;
		Log.LogDebug($"[Possessive.CanAttackDirectly] Result after [{__result}]");
	}

	[HarmonyPrefix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveAttackBuffs))]
	public static bool CorrectBuffsAndDebuffsForGrimoraGiants(PlayableCard __instance, ref int __result)
	{
		bool isGrimoraGiant = __instance.Info.HasTrait(Trait.Giant) && __instance.HasSpecialAbility(GrimoraGiant.FullSpecial.Id);
		if (__instance.OnBoard && isGrimoraGiant)
		{
			int finalAttackNum = 0;
			List<CardSlot> opposingSlots = BoardManager.Instance.GetSlots(__instance.OpponentCard).Where(slot => slot.Card).ToList();
			foreach (var opposingSlot in opposingSlots)
			{
				if (opposingSlot.Card.HasAbility(Ability.BuffEnemy))
				{
					finalAttackNum++;
				}

				if (!__instance.HasAbility(Ability.MadeOfStone) && opposingSlot.Card.HasAbility(Ability.DebuffEnemy))
				{
					finalAttackNum--;
				}
			}

			List<CardSlot> slotsWithGiants = BoardManager.Instance.GetSlots(!__instance.OpponentCard).Where(slot => slot.Card == __instance).ToList();
			foreach (var giant in slotsWithGiants)
			{
				List<CardSlot> adjSlotsWithCards = BoardManager.Instance.GetAdjacentSlots(giant).Where(slot => slot && slot.Card && slot.Card != __instance).ToList();
				if (adjSlotsWithCards.Exists(slot => slot.Card.HasAbility(Ability.BuffNeighbours)))
				{
					finalAttackNum++;
				}
			}

			__result = finalAttackNum;
			return false;
		}

		return true;
	}
}
