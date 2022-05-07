#### 2.0.4:
* Added ConfigOptions
* Added SoftDependencyManager
* Added Risk of Options soft dependency
* Converted BaseInteractable.AddDirectorCardTo to static
* Added BaseInteractable.RemoveDirectorCardFrom
#### 2.0.3:
* Recompiled against the 1.2.3 version of the game to update certain references
* Removed BaseAssetTypes.BaseAchievement
	* Dependent mods should migrate to the RoR2.RegisterAchievement attribute
* Added BrotherInfection static class
* Added BaseItem.SetItemTierWhenAvailable method
* Fixed GenericGameEvents.OnPopulateScene crashing the game
#### 2.0.2:
* Standard CameraTargetParams and PlainHologram are no longer loaded on plugin Awake
	* This fixes an issue with the game hanging up for a few seconds before the rest of the game content starts loading
* Added MysticsRisky2UtilsObjectTransformCurveLoop MonoBehaviour
* Removed StateSerializerFix as it's no longer needed
#### 2.0.1:
* Updated project references to work with the 1st March 2022 version of the game
* Removed the mr2u_notification console command
#### 2.0.0:
* Added XML documentation for fields that can be used by mods that soft-depend on this mod
* Removed CharacterStats class in favour of R2API's RecalculateStatsAPI
* Added RetrieveCharacterBodyList to MysticsRisky2UtilsColliderTriggerList component
* Added a static loadedDictionary (string -> BaseItem/BaseEquipment) for BaseItem and BaseEquipment
* BaseItemLike now has a followerModels dictionary that can be used to store and retrieve multiple item display objects
	* The old followerModel field is now a property that points to followerModels["default"]
* Added OnPlayerCharacterDeath generic game event hook
* Added Utils.AddItemIconBackgroundToSprite
* Added Utils.FormatStringByDict
* Added CharacterModelMaterialOverrides
* Removed BaseLoadableAsset.TokenPrefix and ContentLoadHelper.AddPrefixToAssets
* Removed several methods from BaseItemLike:
	* IsDisabledByConfig
	* LoadModel
	* FollowerModelExists
	* LoadFollowerModel
	* LoadIconSprite
	* SetAssets
	* SetIcon
	* AfterTokensPopulated
	* PreLoad
* Removed BaseItem.ModifierTimesFunction
