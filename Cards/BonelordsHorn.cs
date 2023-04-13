using DiskCardGame;
using UnityEngine;
namespace GrimoraMod;

public partial class GrimoraPlugin
{
	public const string NameBoneLordsHorn = $"{GUID}_BonelordsHorn";

	private void Add_Card_BonelordsHorn()
	{

		Sprite pixelSprite = "BonelordHorn".GetCardInfo().pixelPortrait;

		CardBuilder.Builder
			.SetAsRareCard()
			.SetAbilities(Ability.IceCube, Ability.QuadrupleBones)
			.SetBaseAttackAndHealth(0, 1)
			.SetBoneCost(3)
			.SetDescription("THE HORN OF THE BONELORD, YOU DO NOT WANT TO FIND OUT WHAT'S INSIDE.")
			.SetEnergyCost(2)
			.SetIceCube(NameBonePrince)
			.SetNames(NameBoneLordsHorn, "Bonelord's Horn")
			.Build().pixelPortrait = pixelSprite;
	}
}
