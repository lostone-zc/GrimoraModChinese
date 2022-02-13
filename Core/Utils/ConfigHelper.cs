﻿using APIPlugin;
using BepInEx;
using BepInEx.Configuration;
using DiskCardGame;
using Sirenix.Utilities;
using static GrimoraMod.GrimoraPlugin;

namespace GrimoraMod;

public class ConfigHelper
{
	private static readonly List<StoryEvent> StoryEventsToBeCompleteBeforeStarting = new()
	{
		StoryEvent.BasicTutorialCompleted, StoryEvent.TutorialRunCompleted, StoryEvent.BonesTutorialCompleted,
		StoryEvent.TutorialRun2Completed, StoryEvent.TutorialRun3Completed
	};

	private static ConfigHelper m_Instance;
	public static ConfigHelper Instance => m_Instance ??= new ConfigHelper();

	private readonly ConfigFile GrimoraConfigFile = new(
		Path.Combine(Paths.ConfigPath, "grimora_mod_config.cfg"),
		true
	);

	public const string DefaultRemovedPieces =
		"BossFigurine," +
		"ChessboardChestPiece," +
		"EnemyPiece_Skelemagus,EnemyPiece_Gravedigger," +
		"Tombstone_North1," +
		"Tombstone_Wall1,Tombstone_Wall2,Tombstone_Wall3,Tombstone_Wall4,Tombstone_Wall5," +
		"Tombstone_South1,Tombstone_South2,Tombstone_South3,";

	private ConfigEntry<bool> _configCardsLeftInDeck;
	
	public bool EnableCardsLeftInDeckView => _configCardsLeftInDeck.Value;

	private ConfigEntry<int> _configCurrentChessboardIndex;

	public int CurrentChessboardIndex
	{
		get => _configCurrentChessboardIndex.Value;
		set => _configCurrentChessboardIndex.Value = value;
	}

	private ConfigEntry<int> _configBossesDefeated;
	public int BossesDefeated => _configBossesDefeated.Value;

	public bool isKayceeDead => BossesDefeated == 1;

	public bool isSawyerDead => BossesDefeated == 2;

	public bool isRoyalDead => BossesDefeated == 3;

	public bool isGrimoraDead => BossesDefeated == 4;

	private ConfigEntry<bool> _configDeveloperMode;

	public bool isDevModeEnabled => _configDeveloperMode.Value;

	private ConfigEntry<bool> _configHotReloadEnabled;

	public bool isHotReloadEnabled => _configHotReloadEnabled.Value;

	private ConfigEntry<string> _configCurrentRemovedPieces;

	public List<string> RemovedPieces
	{
		get => _configCurrentRemovedPieces.Value.Split(',').Distinct().ToList();
		set => _configCurrentRemovedPieces.Value = string.Join(",", value);
	}

	internal void BindConfig()
	{
		Log.LogDebug($"Binding config");

		_configCurrentChessboardIndex
			= GrimoraConfigFile.Bind(Name, "Current chessboard layout index", 0);

		_configBossesDefeated
			= GrimoraConfigFile.Bind(Name, "Number of bosses defeated", 0);

		_configCurrentRemovedPieces = GrimoraConfigFile.Bind(
			Name,
			"Current Removed Pieces",
			DefaultRemovedPieces,
			new ConfigDescription("Contains all the current removed pieces." +
			                      "\nDo not alter this list unless you know what you are doing!")
		);

		_configCardsLeftInDeck = GrimoraConfigFile.Bind(
			Name,
			"Enable showing list of cards left in deck during battles",
			true,
			new ConfigDescription("This option will allow you to see what cards are left in your deck.")
		);
		
		_configDeveloperMode = GrimoraConfigFile.Bind(
			Name,
			"Enable Developer Mode",
			false,
			new ConfigDescription("Does not generate blocker pieces. Chests fill first row, enemy pieces fill first column.")
		);

		_configHotReloadEnabled = GrimoraConfigFile.Bind(
			Name,
			"Enable Hot Reload",
			false,
			new ConfigDescription(
				"If the dll is placed in BepInEx/scripts, this will allow running certain commands that should only ever be ran to re-add abilities/cards back in the game correctly.")
		);

		var list = _configCurrentRemovedPieces.Value.Split(',').ToList();
		// this is so that for whatever reason the game map gets added to the removal list,
		//	this will automatically remove those entries
		if (list.Contains("ChessboardGameMap"))
		{
			list.RemoveAll(piece => piece.Equals("ChessboardGameMap"));
		}

		_configCurrentRemovedPieces.Value = string.Join(",", list.Distinct());

		GrimoraConfigFile.SaveOnConfigSet = true;

		ResetConfigDataIfGrimoraHasNotReachedTable();

		HandleHotReloadBefore();

		UnlockAllNecessaryEventsToPlay();
	}

	public int BonesToAdd => BossesDefeated * 2;

	public void HandleHotReloadBefore()
	{
		if (!_configHotReloadEnabled.Value)
		{
			return;
		}

		if (!CardLoader.allData.IsNullOrEmpty())
		{
			int removedNewCards = NewCard.cards.RemoveAll(card => card.name.StartsWith("GrimoraMod_"));
			int removedCardLoader = CardLoader.allData.RemoveAll(info => info.name.StartsWith("GrimoraMod_"));
			Log.LogDebug($"All data is not null. Removed [{removedNewCards}] NewCards, [{removedCardLoader}] CardLoader");
		}

		if (!AbilitiesUtil.allData.IsNullOrEmpty())
		{
			int removed = NewAbility.abilities.RemoveAll(ab => ab.id.ToString().StartsWith(GUID));
			AbilitiesUtil.allData.RemoveAll(info =>
				NewAbility.abilities.Exists(na => na.id.ToString().StartsWith(GUID) && na.ability == info.ability));
			Log.LogDebug($"All data is not null, removed [{removed}] abilities");
		}

		// TODO: I'd prefer not to do this but I'm not sure how to filter out the emissions without literally
		//	making a giant list of all the card names.
		NewCard.emissions.Clear();
	}

	public void HandleHotReloadAfter()
	{
		if (!_configHotReloadEnabled.Value)
		{
			return;
		}

		if (!CardLoader.allData.IsNullOrEmpty())
		{
			CardLoader.allData = CardLoader.allData.Concat(
					NewCard.cards.Where(card => card.name.StartsWith("GrimoraMod_"))
				)
				.Distinct()
				.ToList();
		}

		if (!AbilitiesUtil.allData.IsNullOrEmpty())
		{
			Log.LogDebug($"All data is not null, concatting GrimoraMod abilities");
			AbilitiesUtil.allData = AbilitiesUtil.allData
				.Concat(
					NewAbility.abilities.Where(ab => ab.id.ToString().StartsWith(GUID)).Select(_ => _.info)
				)
				.ToList();
		}
	}

	public void ResetRun()
	{
		Log.LogDebug($"[ResetRun] Resetting run");

		ResetConfig();
		ResetDeck();
		StoryEventsData.EraseEvent(StoryEvent.GrimoraReachedTable);
		SaveManager.SaveToFile();

		LoadingScreenManager.LoadScene("finale_grimora");
	}

	public static void ResetDeck()
	{
		Log.LogWarning($"Resetting Grimora Deck Data");
		GrimoraSaveData.Data.Initialize();
	}

	internal void ResetConfig()
	{
		Log.LogWarning($"Resetting Grimora Mod config");
		_configBossesDefeated.Value = 0;
		_configCurrentChessboardIndex.Value = 0;
		ResetRemovedPieces();
	}

	private void ResetConfigDataIfGrimoraHasNotReachedTable()
	{
		if (!StoryEventsData.EventCompleted(StoryEvent.GrimoraReachedTable))
		{
			Log.LogWarning($"Grimora has not reached the table yet, resetting values to false again.");
			ResetConfig();
		}
	}

	private static void UnlockAllNecessaryEventsToPlay()
	{
		if (!StoryEventsToBeCompleteBeforeStarting.TrueForAll(StoryEventsData.EventCompleted))
		{
			Log.LogWarning($"You haven't completed a required event... Starting unlock process");
			StoryEventsToBeCompleteBeforeStarting.ForEach(evt => StoryEventsData.SetEventCompleted(evt));
			ProgressionData.UnlockAll();
			SaveManager.SaveToFile();
		}
	}

	public void AddPieceToRemovedPiecesConfig(string pieceName)
	{
		_configCurrentRemovedPieces.Value += "," + pieceName + ",";
	}

	public void ResetRemovedPieces()
	{
		_configCurrentRemovedPieces.Value = DefaultRemovedPieces;
	}

	public void SetBossDefeatedInConfig(BaseBossExt boss)
	{
		_configBossesDefeated.Value = boss switch
		{
			KayceeBossOpponent => 1,
			SawyerBossOpponent => 2,
			RoyalBossOpponentExt => 3,
			GrimoraBossOpponentExt => 4
		};

		var bossPiece = ChessboardMapExt.Instance.BossPiece;
		ChessboardMapExt.Instance.BossDefeated = true;
		AddPieceToRemovedPiecesConfig(bossPiece.name);
		Log.LogDebug($"[SetBossDefeatedInConfig] Boss {bossPiece} defeated.");
	}
}
