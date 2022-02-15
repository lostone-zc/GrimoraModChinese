﻿using APIPlugin;
using DiskCardGame;

namespace GrimoraMod;

public partial class GrimoraPlugin
{
	public const string NameNecromancer = "GrimoraMod_Necromancer";

	private void Add_Necromancer()
	{
		NewCard.Add(CardBuilder.Builder
			.SetAsRareCard()
			.SetAbilities(Ability.DoubleDeath)
			.SetBaseAttackAndHealth(1, 2)
			.SetBoneCost(3)
			.SetDescription("NOTHING DIES ONCE.")
			.SetNames(NameNecromancer, "Necromancer")
			.Build()
		);
	}
}
