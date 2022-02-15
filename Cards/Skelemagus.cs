﻿using APIPlugin;
using DiskCardGame;

namespace GrimoraMod;

public partial class GrimoraPlugin
{
	public const string NameSkelemagus = "GrimoraMod_Skelemagus";

	private void Add_Skelemagus()
	{
		NewCard.Add(CardBuilder.Builder
			.SetAsNormalCard()
			.SetAbilities(Ability.Brittle)
			.SetBaseAttackAndHealth(4, 1)
			.SetEnergyCost(5)
			.SetDescription("THEY HAVE LEARNED THE ANCIENT SPELL OF DEATH.")
			.SetNames(NameSkelemagus, "Skelemagus")
			.Build()
		);
	}
}
