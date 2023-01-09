﻿using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using static GrimoraMod.GrimoraPlugin;

namespace GrimoraMod;

[HarmonyPatch]
public class CardAppearanceBehaviourPatch
{
	public static readonly Material GravestoneGold = AssetUtils.GetPrefab<Material>("GravestoneCardBack_Rare");

	[HarmonyPrefix, HarmonyPatch(typeof(RareCardBackground), nameof(RareCardBackground.ApplyAppearance))]
	public static bool CorrectBehaviourForGrimora(ref RareCardBackground __instance)
	{
		if (GrimoraSaveUtil.IsNotGrimoraModRun)
		{
			return true;
		}

		var renderer = __instance.Card.GetComponentInChildren<GravestoneRenderStatsLayer>();
		if (renderer != null)
		{
			renderer.Material.SetAlbedoTexture(GravestoneGold.mainTexture);
		}

		return false;
	}
}
