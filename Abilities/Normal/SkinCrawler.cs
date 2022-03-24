﻿using System.Collections;
using APIPlugin;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using Pixelplacement;
using Pixelplacement.TweenSystem;
using UnityEngine;
using static GrimoraMod.GrimoraPlugin;

namespace GrimoraMod;

public class SkinCrawler : AbilityBehaviour
{
	internal static List<CardSlot> SlotsThatHaveCrawlersHidingUnderCards = new();

	public static Ability ability;

	public override Ability Ability => ability;

	private CardSlot _slotHidingUnderCard = null;

	public static SkinCrawler GetSkinCrawlerFromCard(PlayableCard playableCard)
	{
		for (int i = 0; i < playableCard.transform.childCount; i++)
		{
			SkinCrawler skinCrawler = playableCard.transform.GetChild(i).GetComponent<SkinCrawler>();
			if (skinCrawler)
			{
				// now we get the card itself to add to the list
				return skinCrawler;
			}
		}

		return null;
	}

	public static SkinCrawler GetSkinCrawlerFromSlot(CardSlot slot)
	{
		if (slot && slot.Card)
		{
			return GetSkinCrawlerFromCard(slot.Card);
		}

		return null;
	}

	private CardSlot FindValidHostSlot()
	{
		Log.LogDebug($"[SkinCrawler] Checking if adj slots from [{Card.Slot}] are not null");
		CardSlot slotToPick = null;

		Log.LogDebug($"[SkinCrawler] Slot for SkinCrawler [{Card.Slot}]");
		CardSlot toLeftSlot = BoardManager.Instance.GetAdjacent(Card.Slot, true);
		CardSlot toRightSlot = BoardManager.Instance.GetAdjacent(Card.Slot, false);

		if (toLeftSlot && toLeftSlot.Card && GetSkinCrawlerFromCard(toLeftSlot.Card).IsNull())
		{
			slotToPick = toLeftSlot;
			Log.LogDebug($"[SkinCrawler] LeftSlot, has card [{slotToPick.Card.GetNameAndSlot()}]");
		}
		else if (toRightSlot
		         && toRightSlot.Card
		         && GetSkinCrawlerFromCard(toRightSlot.Card).IsNull())
		{
			slotToPick = toRightSlot;
			Log.LogDebug($"[SkinCrawler] RightSlot, has card [{slotToPick.Card.GetNameAndSlot()}]");
		}

		if (GetSkinCrawlerFromSlot(slotToPick))
		{
			return null;
		}

		return slotToPick;
	}

	public IEnumerator AssignSkinCrawlerCardToHost(CardSlot slotToPick)
	{
		if (slotToPick)
		{
			PlayableCard cardToPick = slotToPick.Card;
			CardSlot cardSlotToPick = slotToPick.Card.Slot;

			ViewManager.Instance.SwitchToView(View.Board, lockAfter: true);

			yield return new WaitForSeconds(0.4f);
			// to the left and up, like something is being added under it
			Vector3 vector = new Vector3(0f, 0.25f, 0f);
			// do to card that will be hiding Boo Hag
			Tween.Position(cardToPick.transform, cardSlotToPick.transform.position + vector, 0.1f, 0f, Tween.EaseInOut);

			Vector3 cardRot = cardToPick.transform.rotation.eulerAngles;

			// rotate on z-axis, as if you rotated your hand holding the card counter-clockwise
			// Tween.Rotate(toRightTransform, new Vector3(0f, 0f, 25f), Space.World, 0.1f, 0f, Tween.EaseInOut);

			// Vector3 positionFurtherAwayFromBaseCard = toRightSlotTransform.position + Vector3.forward * 8f;
			// set starting position 
			// base.Card.transform.position = positionFurtherAwayFromBaseCard;

			// move pack from current position to the baseCardSlotPosition
			// Log.LogDebug($"[SkinCrawler] moving BooHag to [{toRightSlotTransform.position}]");
			TweenBase tweenMoveIntoCardSlot = Tween.Position(
				Card.transform,
				cardSlotToPick.transform.position,
				0.4f,
				0f,
				Tween.EaseOut
			);

			while (tweenMoveIntoCardSlot.Status is not Tween.TweenStatus.Finished)
			{
				// Log.LogDebug($"[SkinCrawler] playing negation effect in loop");
				cardToPick.Anim.StrongNegationEffect();
				yield return new WaitForSeconds(0.1f);
			}

			TweenBase tweenMoveUpward = Tween.Position(
				Card.transform,
				cardSlotToPick.transform.position + new Vector3(0f, 0f, 0.31f),
				0.2f,
				0f,
				Tween.EaseOut
			);
			yield return new WaitForSeconds(0.1f);

			// rotate base card with its original rotation values so that it lays flat on the board again
			// Log.LogDebug($"[SkinCrawler] rotating [{toRightSlot.Card.Info.name}]");
			Tween.Rotation(cardToPick.transform, cardRot, 0.1f, 0f, Tween.EaseInOut);

			// offset the card to be a little higher
			// Log.LogDebug($"[SkinCrawler] Setting height offset");
			cardToPick.SlotHeightOffset = 0.13f;

			// reassign the card to the slot
			Log.LogDebug($"[SkinCrawler] Assigning [{cardToPick.Info.name}] to slot");
			yield return BoardManager.Instance.AssignCardToSlot(cardToPick, cardSlotToPick);

			cardToPick.AddTemporaryMod(new CardModificationInfo() { attackAdjustment = 1, healthAdjustment = 1 });

			yield return new WaitUntil(
				() => !Tween.activeTweens.Exists(t => t.targetInstanceID == cardToPick.transform.GetInstanceID())
			);

			Log.LogDebug($"[SkinCrawler] Setting Boo Hag as child of card");
			transform.SetParent(cardToPick.transform);
			Log.LogDebug($"[SkinCrawler] Setting Boo Hag slot [{Card.Slot.Index}] to null");
			// Card.UnassignFromSlot();
			BoardManager.Instance.playerSlots[Card.Slot.Index].Card = null;
			Log.LogDebug($"[SkinCrawler] Setting Boo Hag slot to [{cardSlotToPick}]");
			SlotsThatHaveCrawlersHidingUnderCards.Add(cardSlotToPick);
			_slotHidingUnderCard = cardSlotToPick;
		}

		yield return new WaitForSeconds(0.25f);
		ViewManager.Instance.SwitchToView(View.Default);
		ViewManager.Instance.SetViewUnlocked();
	}


	private bool CardIsAdjacent(PlayableCard playableCard)
	{
		return BoardManager.Instance.GetAdjacentSlots(Card.Slot)
			.Exists(slot => slot && slot.Card == playableCard);
	}

	public override bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard)
	{
		Log.LogDebug(
			$"[Crawler.RespondsToOtherCardAssignedToSlot]"
			+ $" This {Card.GetNameAndSlot()} OtherCard {otherCard.GetNameAndSlot()} "
			+ $"_slotHidingUnderCard [{_slotHidingUnderCard}] "
			+ $"otherCard.Slot != Card.Slot [{otherCard.Slot != Card.Slot}]"
		);

		return _slotHidingUnderCard.IsNull()
		       && !SlotsThatHaveCrawlersHidingUnderCards.Contains(otherCard.Slot)
		       && otherCard.Slot != Card.Slot
		       && CardIsAdjacent(otherCard);
	}

	public override IEnumerator OnOtherCardAssignedToSlot(PlayableCard otherCard)
	{
		Log.LogDebug(
			$"[Crawler.OnOtherCardAssignedToSlot] [{Card.GetNameAndSlot()}] Will now buff [{otherCard.GetNameAndSlot()}]"
		);
		yield return AssignSkinCrawlerCardToHost(FindValidHostSlot());
	}


	public override bool RespondsToResolveOnBoard()
	{
		return true;
	}

	public override IEnumerator OnResolveOnBoard()
	{
		yield return AssignSkinCrawlerCardToHost(FindValidHostSlot());
	}

	public override bool RespondsToOtherCardDie(
		PlayableCard card,
		CardSlot deathSlot,
		bool fromCombat,
		PlayableCard killer
	)
	{
		Log.LogDebug(
			$"[Crawler.RespondsToOtherCardDie] "
			+ $"Crawler [{Card.GetNameAndSlot()}] Card [{card.GetNameAndSlot()}] deathSlot [{deathSlot.name}] "
			+ $"_slotHidingUnderCard [{_slotHidingUnderCard}] is card.Slot? [{_slotHidingUnderCard == card.Slot}]"
			+ $"Exists in list? [{SlotsThatHaveCrawlersHidingUnderCards.Contains(deathSlot)}]"
		);
		return _slotHidingUnderCard
		       && _slotHidingUnderCard == card.Slot
		       && SlotsThatHaveCrawlersHidingUnderCards.Contains(deathSlot);
	}

	public override IEnumerator OnOtherCardDie(
		PlayableCard card,
		CardSlot deathSlot,
		bool fromCombat,
		PlayableCard killer
	)
	{
		Log.LogDebug($"[Crawler.OnOtherCardDie] Resolving [{Card.GetNameAndSlot()}] to deathSlot [{deathSlot.Index}]");
		CardInfo infoCopy = Card.Info;
		UnityObject.Destroy(Card.gameObject);
		yield return BoardManager.Instance.CreateCardInSlot(infoCopy, deathSlot, 0);
		_slotHidingUnderCard = null;
	}

	public static AbilityManager.FullAbility Create()
	{
		const string rulebookDescription =
			"[creature] will attempt to find a host in an adjacent friendly slot, hiding under it providing a +1/+1 buff."
			+ "Cards on the left take priority.";

		return ApiUtils.CreateAbility<SkinCrawler>(rulebookDescription);
	}
}

[HarmonyPatch(typeof(BoardManager))]
public class SkinCrawlerPatches
{
	[HarmonyPostfix, HarmonyPatch(nameof(BoardManager.CardsOnBoard), MethodType.Getter)]
	public static void Postfix(ref List<PlayableCard> __result)
	{
		List<PlayableCard> newList = new List<PlayableCard>(__result);
		foreach (var card in __result)
		{
			SkinCrawler skinCrawler = SkinCrawler.GetSkinCrawlerFromSlot(card.Slot);
			if (skinCrawler)
			{
				// now we get the card itself to add to the list
				Log.LogDebug(
					$"[CardsOnBoard] Card [{card.GetNameAndSlot()}] has a child with skin crawler"
				);
				newList.Add(skinCrawler.GetComponent<PlayableCard>());
			}
		}

		__result = newList;
	}
}
