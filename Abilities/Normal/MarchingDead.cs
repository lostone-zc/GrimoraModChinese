﻿using System.Collections;
using DiskCardGame;
using InscryptionAPI.Card;
using static GrimoraMod.GrimoraPlugin;

namespace GrimoraMod;

public class MarchingDead : AbilityBehaviour
{
	public static Ability ability;

	public override Ability Ability => ability;

	public PlayableCard leftAdjCard = null;
	public PlayableCard rightAdjCard = null;

	public override bool RespondsToResolveOnBoard()
	{
		return leftAdjCard || rightAdjCard;
	}

	public override IEnumerator OnResolveOnBoard()
	{
		CardSlot leftAdjSlot = BoardManager.Instance.GetAdjacent(Card.Slot, true);
		CardSlot rightAdjSlot = BoardManager.Instance.GetAdjacent(Card.Slot, false);
		if (leftAdjCard && leftAdjSlot && leftAdjSlot.Card.IsNull())
		{
			yield return PlayerHand.Instance.PlayCardOnSlot(leftAdjCard, leftAdjSlot);
		}

		if (rightAdjCard && rightAdjSlot && rightAdjSlot.Card.IsNull())
		{
			yield return PlayerHand.Instance.PlayCardOnSlot(rightAdjCard, rightAdjSlot);
		}
	}

	public void SetAdjCardsToPlay(List<PlayableCard> cardsInHand)
	{
		Log.LogDebug($"[MarchingDead] Setting adj cards to play");
		int indexOfCard = cardsInHand.IndexOf(Card);
		for (var i = 0; i < cardsInHand.Count; i++)
		{
			if (i == indexOfCard - 1)
			{
				leftAdjCard = cardsInHand[i];
			}
			else if (i == indexOfCard + 1)
			{
				rightAdjCard = cardsInHand[i];
			}
		}
	}
}

public partial class GrimoraPlugin
{
	public void Add_Ability_MarchingDead()
	{
		const string rulebookDescription
			= "When [creature] is played, also play the cards in your hand that were adjacent to this card for free.";

		ApiUtils.CreateAbility<MarchingDead>(rulebookDescription);
	}
}
