﻿using System.Collections;
using APIPlugin;
using DiskCardGame;
using static GrimoraMod.GrimoraPlugin;

namespace GrimoraMod;

public class FlameStrafe : Strafe
{
	public static Ability ability;

	public override Ability Ability => ability;

	public override IEnumerator PostSuccessfulMoveSequence(CardSlot cardSlot)
	{
		if (cardSlot.Card.IsNull())
		{
			yield return BoardManager.Instance.CreateCardInSlot(NameFlames.GetCardInfo(), cardSlot);
		}
	}

	public static NewAbility Create()
	{
		const string rulebookDescription =
			"Whenever [creature] moves, it leaves a trail of Embers. " +
			"The warmth of the Embers shall enlighten nearby cards.";

		return ApiUtils.CreateAbility<FlameStrafe>(rulebookDescription);
	}
}
