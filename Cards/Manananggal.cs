﻿using DiskCardGame;

namespace GrimoraMod;

public partial class GrimoraPlugin
{
	public const string NameManananggal = "GrimoraMod_Manananggal";

	private void Add_Manananggal()
	{
		CardBuilder.Builder
			.SetAsNormalCard()
			.SetAbilities(Ability.Flying)
			.SetBaseAttackAndHealth(3, 3)
			.SetBoneCost(8)
			.SetDescription("THIS MONSTROUS CREATURE ABANDONED ITS LEGS TO HUNT FOR ITS NEXT VICTIM.")
			.SetNames(NameManananggal, "Manananggal")
			.Build();
	}
}

