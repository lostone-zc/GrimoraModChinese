﻿using APIPlugin;
using DiskCardGame;

namespace GrimoraMod;

public partial class GrimoraPlugin
{
	public const string NameDanseMacabre = "ara_DanseMacabre";
	
	private void AddAra_DanseMacabre()
	{
		NewCard.Add(CardBuilder.Builder
			.SetAsNormalCard()
			.SetAbilities(AlternatingStrike.ability, Erratic.ability)
			.SetBaseAttackAndHealth(3, 3)
			.SetBoneCost(8)
			.SetNames(NameDanseMacabre, "Danse Macabre")
			// .SetDescription("A vicious pile of bones. You can have it...")
			.Build()
		);
	}
}
