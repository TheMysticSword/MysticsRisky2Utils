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
