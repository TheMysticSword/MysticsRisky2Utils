using RoR2;
using UnityEngine;
using System.Collections.Generic;
using MysticsRisky2Utils.ContentManagement;
using static MysticsRisky2Utils.MysticsRisky2UtilsPlugin;

namespace MysticsRisky2Utils.BaseAssetTypes
{
    public abstract class BaseBuff : BaseLoadableAsset
    {
        public BuffDef buffDef;
        public static List<BaseBuff> loadedBuffs = new List<BaseBuff>();

        public abstract Sprite LoadSprite(string assetPath);

        public override void Load()
        {
            buffDef = ScriptableObject.CreateInstance<BuffDef>();
            OnLoad();
            buffDef.iconSprite = LoadSprite(buffDef.name);
            loadedBuffs.Add(this);
            asset = buffDef;
        }

        private float StatModifierTimes(GenericCharacterInfo genericCharacterInfo)
        {
            return genericCharacterInfo.body.HasBuff(buffDef) ? genericCharacterInfo.body.GetBuffCount(buffDef) : 0f;
        }
        private float StatModifierTimesNoStack(GenericCharacterInfo genericCharacterInfo)
        {
            return genericCharacterInfo.body.HasBuff(buffDef) ? 1f : 0f;
        }
        private bool StatModifierShouldApply(GenericCharacterInfo genericCharacterInfo)
        {
            return genericCharacterInfo.body.HasBuff(buffDef);
        }
        public void AddModifier(List<CharacterStats.StatModifier> list, float multiplier, float flat, bool stacks)
        {
            CharacterStats.StatModifier statModifier = new CharacterStats.StatModifier
            {
                multiplier = multiplier,
                flat = flat,
                times = StatModifierTimes
            };
            if (!stacks) statModifier.times = StatModifierTimesNoStack;
            list.Add(statModifier);
        }
        public void AddModifier(List<CharacterStats.FlatStatModifier> list, float amount, bool stacks)
        {
            CharacterStats.FlatStatModifier statModifier = new CharacterStats.FlatStatModifier
            {
                amount = amount,
                times = StatModifierTimes
            };
            if (!stacks) statModifier.times = StatModifierTimesNoStack;
            list.Add(statModifier);
        }
        public void AddModifier(List<CharacterStats.BoolStatModifier> list)
        {
            CharacterStats.BoolStatModifier statModifier = new CharacterStats.BoolStatModifier
            {
                shouldApply = StatModifierShouldApply
            };
            list.Add(statModifier);
        }
        public void AddLevelModifier(int amount, bool stacks = true)
        {
            AddModifier(CharacterStats.levelModifiers, amount, stacks);
        }
        public void AddHealthModifier(float multiplier = 0f, float flat = 0f, bool stacks = true)
        {
            AddModifier(CharacterStats.healthModifiers, multiplier, flat, stacks);
        }
        public void AddShieldModifier(float multiplier = 0f, float flat = 0f, bool stacks = true)
        {
            AddModifier(CharacterStats.shieldModifiers, multiplier, flat, stacks);
        }
        public void AddRegenModifier(float amount, bool stacks = true)
        {
            AddModifier(CharacterStats.regenModifiers, amount, stacks);
        }
        public void AddMoveSpeedModifier(float multiplier = 0f, float flat = 0f, bool stacks = true)
        {
            AddModifier(CharacterStats.moveSpeedModifiers, multiplier, flat, stacks);
        }
        public void AddRootMovementModifier()
        {
            AddModifier(CharacterStats.rootMovementModifiers);
        }
        public void AddDamageModifier(float multiplier = 0f, float flat = 0f, bool stacks = true)
        {
            AddModifier(CharacterStats.damageModifiers, multiplier, flat, stacks);
        }
        public void AddAttackSpeedModifier(float multiplier = 0f, float flat = 0f, bool stacks = true)
        {
            AddModifier(CharacterStats.attackSpeedModifiers, multiplier, flat, stacks);
        }
        public void AddCritModifier(float amount, bool stacks = true)
        {
            AddModifier(CharacterStats.critModifiers, amount, stacks);
        }
        public void AddArmorModifier(float amount, bool stacks = true)
        {
            AddModifier(CharacterStats.armorModifiers, amount, stacks);
        }
        public void AddCooldownModifier(float multiplier = 0f, float flat = 0f, bool stacks = true)
        {
            AddModifier(CharacterStats.cooldownModifiers, multiplier, flat, stacks);
        }
    }
}
