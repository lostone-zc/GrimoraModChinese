﻿using System.Collections;
using DiskCardGame;
using UnityEngine;
using static GrimoraMod.GrimoraPlugin;

namespace GrimoraMod;

public abstract class BaseBossExt : Part1BossOpponent
{
	public static readonly Dictionary<string, Tuple<Opponent.Type, System.Type, EncounterBlueprintData>>
		OpponentTupleBySpecialId = new()
		{
			{
				"KayceeBoss", new Tuple<Type, System.Type, EncounterBlueprintData>(
					KayceeOpponent, typeof(GrimoraModKayceeBossSequencer), BlueprintUtils.BuildKayceeBossInitialBlueprint()
				)
			},
			{
				"SawyerBoss", new Tuple<Type, System.Type, EncounterBlueprintData>(
					SawyerOpponent, typeof(GrimoraModSawyerBossSequencer), BlueprintUtils.BuildSawyerBossInitialBlueprint()
				)
			},
			{
				"RoyalBoss", new Tuple<Type, System.Type, EncounterBlueprintData>(
					RoyalOpponent, typeof(GrimoraModRoyalBossSequencer), BlueprintUtils.BuildRoyalBossInitialBlueprint()
				)
			},
			{
				"GrimoraBoss", new Tuple<Type, System.Type, EncounterBlueprintData>(
					GrimoraOpponent, typeof(GrimoraModGrimoraBossSequencer), BlueprintUtils.BuildGrimoraBossInitialBlueprint()
				)
			},
			{
				"GrimoraModBattleSequencer",
				new Tuple<Type, System.Type, EncounterBlueprintData>(0, typeof(GrimoraModBattleSequencer), null)
			}
		};

	public const string PrefabPathMasks = "Prefabs/Opponents/Leshy/Masks";
	public const string PrefabPathRoyalBossSkull = "Prefabs/Opponents/Grimora/RoyalBossSkull";

	public static GameObject RoyalBossSkull => GrimoraRightWrist.transform.GetChild(6).gameObject;
	public static GameObject GrimoraBossSkull => GrimoraAnimationController.Instance.bossSkull;
	public static GameObject GrimoraRightWrist { get; } = GameObject.Find("Grimora_RightWrist");

	public const Type KayceeOpponent = (Type)1001;
	public const Type SawyerOpponent = (Type)1002;
	public const Type RoyalOpponent = (Type)1003;
	public const Type GrimoraOpponent = (Type)1004;

	public static readonly Dictionary<Type, GameObject> BossMasksByType = new()
	{
		{ SawyerOpponent, ResourceBank.Get<GameObject>($"{PrefabPathMasks}/MaskTrader") },
		{ KayceeOpponent, AssetUtils.GetPrefab<GameObject>("KayceeBossSkull") },
		// { RoyalOpponent, PrefabPathRoyalBossSkull }
	};

	private static readonly int ShowSkull = Animator.StringToHash("show_skull");
	private static readonly int HideSkull = Animator.StringToHash("hide_skull");


	public abstract StoryEvent EventForDefeat { get; }

	public abstract Type Opponent { get; }

	public abstract string SpecialEncounterId { get; }

	public override IEnumerator IntroSequence(EncounterData encounter)
	{
		yield return base.IntroSequence(encounter);

		// Royal boss has a specific sequence to follow so that it flows easier
		if (BossMasksByType.TryGetValue(Opponent, out GameObject mask))
		{
			Log.LogDebug($"[{GetType()}] Setting royal skull inactive");

			Log.LogDebug($"[{GetType()}] Creating skull");
			GrimoraAnimationController.Instance.bossSkull = Instantiate(mask, GrimoraRightWrist.transform);
			var bossSkullTransform = GrimoraBossSkull.transform;
			if (Opponent is SawyerOpponent)
			{
				bossSkullTransform.localPosition = new Vector3(0.02f, 0.18f, 0.07f);
				bossSkullTransform.localRotation = Quaternion.Euler(0, 0, 270);
				bossSkullTransform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
			}
			else if (Opponent is KayceeOpponent)
			{
				bossSkullTransform.localPosition = new Vector3(-0.0044f, 0.18f, -0.042f);
				bossSkullTransform.localRotation = Quaternion.Euler(85.85f, 227.76f, 262.77f);
				bossSkullTransform.localScale = new Vector3(0.14f, 0.14f, 0.14f);
			}

			try
			{
				RoyalBossSkull.SetActive(false);
			}
			catch (Exception e)
			{
				Log.LogError("Was unable to find Royal's skull or set it as inactive?");
				throw;
			}

			yield return ShowBossSkull();

			AudioController.Instance.FadeOutLoop(0.75f);
			RunState.CurrentMapRegion.FadeOutAmbientAudio();
		}
	}

	public override IEnumerator OutroSequence(bool wasDefeated)
	{
		if (wasDefeated)
		{
			ConfigHelper.Instance.SetBossDefeatedInConfig(this);

			Log.LogDebug($"[{GetType()}] SaveFile is Grimora");

			AudioController.Instance.PlaySound2D("glitch_error", MixerGroup.TableObjectsSFX);

			if (GrimoraBossSkull is not null)
			{
				Log.LogDebug($"[{GetType()}] Glitching mask");
				yield return HideBossSkull();
			}

			Log.LogDebug($"[{GetType()}] hiding skull");
			GrimoraAnimationController.Instance.SetHeadTrigger("hide_skull");

			Log.LogDebug($"[{GetType()}] Destroying scenery");
			DestroyScenery();

			Log.LogDebug($"[{GetType()}] Set Scene Effects");
			SetSceneEffectsShown(false);

			Log.LogDebug($"[{GetType()}] Stopping audio");
			AudioController.Instance.StopAllLoops();

			yield return new WaitForSeconds(0.75f);

			Log.LogDebug($"[{GetType()}] CleanUpBossBehaviours");
			CleanUpBossBehaviours();

			ViewManager.Instance.SwitchToView(View.Default, false, true);

			Log.LogDebug($"[{GetType()}] Resetting table colors");
			TableVisualEffectsManager.Instance.ResetTableColors();
			yield return new WaitForSeconds(0.25f);

			Log.LogDebug($"Setting post battle special node to a rare code node data");
			TurnManager.Instance.PostBattleSpecialNode = new ChooseRareCardNodeData();
		}
		else
		{
			yield return base.OutroSequence(false);
		}
	}

	public IEnumerator ShowBossSkull()
	{
		yield return new WaitForSeconds(0.1f);
		Log.LogDebug($"[{GetType()}] Calling ShowBossSkull");
		GrimoraAnimationController.Instance.ShowBossSkull();
		Log.LogDebug($"[{GetType()}] Setting Head Trigger");
		GrimoraAnimationController.Instance.headAnim.ResetTrigger(HideSkull);
		GrimoraAnimationController.Instance.SetHeadTrigger("show_skull");
		yield return new WaitForSeconds(0.05f);

		ViewManager.Instance.SwitchToView(View.BossCloseup, false, true);

		yield return new WaitForSeconds(1f);
	}

	public IEnumerator HideBossSkull()
	{
		// Log.LogDebug($"[{GetType()}] Calling GlitchOutBossSkull");
		GrimoraAnimationController.Instance.GlitchOutBossSkull();

		GrimoraAnimationController.Instance.headAnim.ResetTrigger(ShowSkull);
		GrimoraAnimationController.Instance.SetHeadTrigger("hide_skull");

		yield break;
	}

	public virtual IEnumerator ReplaceBlueprintCustom(EncounterBlueprintData blueprintData)
	{
		Blueprint = blueprintData;
		List<List<CardInfo>> plan = EncounterBuilder.BuildOpponentTurnPlan(Blueprint, 0, false);
		ReplaceAndAppendTurnPlan(plan);
		yield return QueueNewCards();
	}
}
