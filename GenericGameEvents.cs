using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine;
using static MysticsRisky2Utils.MysticsRisky2UtilsPlugin;

namespace MysticsRisky2Utils
{
    public class GenericGameEvents
    {
        public struct OnTakeDamageEventInfo
        {
            public float damageTaken;
        }

        public delegate void DamageAttackerVictimEventHandler(DamageInfo damageInfo, GenericCharacterInfo attackerInfo, GenericCharacterInfo victimInfo);
        public delegate void DamageAttackerEventHandler(DamageInfo damageInfo, GenericCharacterInfo attackerInfo);
        public delegate void DamageModifierEventHandler(DamageInfo damageInfo, GenericCharacterInfo attackerInfo, GenericCharacterInfo victimInfo, ref float damage);
        public delegate void DamageVictimEventHandler(DamageInfo damageInfo, GenericCharacterInfo victimInfo);
        public delegate void DamageReportEventHandler(DamageReport damageReport);
        public delegate void SceneRNGEventHandler(Xoroshiro128Plus rng);
        public delegate void InteractionEventHandler(Interactor interactor, IInteractable interactable, GameObject interactableObject, bool canProc);

        public static event DamageAttackerVictimEventHandler OnHitEnemy;
        public static event DamageAttackerEventHandler OnHitAll;
        public static event DamageAttackerVictimEventHandler BeforeTakeDamage;
        public static event DamageModifierEventHandler OnApplyDamageIncreaseModifiers;
        public static event DamageModifierEventHandler OnApplyDamageReductionModifiers;
        public static event DamageModifierEventHandler OnApplyDamageCapModifiers;
        public static event DamageReportEventHandler OnTakeDamage;
        public static event SceneRNGEventHandler OnPopulateScene;
        public static event InteractionEventHandler OnInteractionBegin;

        internal static void ErrorHookFailed(string name)
        {
            logger.LogError("generic game event '" + name + "' hook failed");
        }
        internal static void Init()
        {
            On.RoR2.HealthComponent.Awake += (orig, self) =>
            {
                self.gameObject.AddComponent<MysticsRisky2UtilsDamageEvents>();
                orig(self);
            };

            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
            {
                orig(self, damageInfo, victim);
                if (damageInfo.attacker)
                {
                    CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                    CharacterBody victimBody = victim ? victim.GetComponent<CharacterBody>() : null;
                    GenericCharacterInfo attackerInfo = new GenericCharacterInfo(attackerBody);
                    GenericCharacterInfo victimInfo = new GenericCharacterInfo(victimBody);
                    if (OnHitEnemy != null) OnHitEnemy(damageInfo, attackerInfo, victimInfo);
                }
            };

            On.RoR2.GlobalEventManager.OnHitAll += (orig, self, damageInfo, hitObject) =>
            {
                orig(self, damageInfo, hitObject);
                if (damageInfo.attacker)
                {
                    CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                    GenericCharacterInfo attackerInfo = new GenericCharacterInfo(attackerBody);
                    if (OnHitAll != null) OnHitAll(damageInfo, attackerInfo);
                }
            };

            IL.RoR2.HealthComponent.TakeDamage += (il) =>
            {
                ILCursor c = new ILCursor(il);
                int bypassArmorFlagPosition = 5;
                int damagePosition = 6;
                if (c.TryGotoNext(
                    MoveType.AfterLabel,
                    x => x.MatchLdarg(1),
                    x => x.MatchLdfld<DamageInfo>("damageType"),
                    x => x.MatchLdcI4(2),
                    x => x.MatchAnd(),
                    x => x.MatchLdcI4(0),
                    x => x.MatchCgtUn(),
                    x => x.MatchStloc(out bypassArmorFlagPosition)
                ) && c.TryGotoNext(
                    MoveType.AfterLabel,
                    x => x.MatchLdarg(1),
                    x => x.MatchLdfld<DamageInfo>("damage"),
                    x => x.MatchStloc(out damagePosition)
                ))
                {
                    if (c.TryGotoNext(
                        MoveType.AfterLabel,
                        x => x.MatchLdarg(1),
                        x => x.MatchLdfld<DamageInfo>("crit"),
                        x => x.MatchBrfalse(out _)
                    ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_1);
                        c.Emit(OpCodes.Ldloc_1);
                        c.Emit(OpCodes.Ldloc, damagePosition);
                        c.EmitDelegate<System.Func<HealthComponent, DamageInfo, CharacterBody, float, float>>((healthComponent, damageInfo, attackerBody, damage) =>
                        {
                            CharacterBody victimBody = healthComponent.body;
                            GenericCharacterInfo attackerInfo = new GenericCharacterInfo(attackerBody);
                            GenericCharacterInfo victimInfo = new GenericCharacterInfo(victimBody);
                            if (OnApplyDamageIncreaseModifiers != null) OnApplyDamageIncreaseModifiers(damageInfo, attackerInfo, victimInfo, ref damage);
                            return damage;
                        });
                        c.Emit(OpCodes.Stloc, damagePosition);
                    }
                    else ErrorHookFailed("on apply damage increase modifiers");
                    if (c.TryGotoNext(
                        MoveType.After,
                        x => x.MatchLdloc(bypassArmorFlagPosition),
                        x => x.MatchBrtrue(out _)
                    ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_1);
                        c.Emit(OpCodes.Ldloc_1);
                        c.Emit(OpCodes.Ldloc, damagePosition);
                        c.EmitDelegate<System.Func<HealthComponent, DamageInfo, CharacterBody, float, float>>((healthComponent, damageInfo, attackerBody, damage) =>
                        {
                            CharacterBody victimBody = healthComponent.body;
                            GenericCharacterInfo attackerInfo = new GenericCharacterInfo(attackerBody);
                            GenericCharacterInfo victimInfo = new GenericCharacterInfo(victimBody);
                            if (OnApplyDamageReductionModifiers != null) OnApplyDamageReductionModifiers(damageInfo, attackerInfo, victimInfo, ref damage);
                            return damage;
                        });
                        c.Emit(OpCodes.Stloc, damagePosition);
                    }
                    else ErrorHookFailed("on apply damage reduction modifiers");
                    if (c.TryGotoNext(
                        MoveType.After,
                        x => x.MatchLdarg(0),
                        x => x.MatchCallOrCallvirt<HealthComponent>("get_fullHealth"),
                        x => x.MatchLdcR4(0.1f),
                        x => x.MatchMul(),
                        x => x.MatchStloc(damagePosition)
                    ) && c.TryGotoPrev(
                        MoveType.AfterLabel,
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<HealthComponent>("body"),
                        x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "LunarShell")
                    ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_1);
                        c.Emit(OpCodes.Ldloc_1);
                        c.Emit(OpCodes.Ldloc, damagePosition);
                        c.EmitDelegate<System.Func<HealthComponent, DamageInfo, CharacterBody, float, float>>((healthComponent, damageInfo, attackerBody, damage) =>
                        {
                            CharacterBody victimBody = healthComponent.body;
                            GenericCharacterInfo attackerInfo = new GenericCharacterInfo(attackerBody);
                            GenericCharacterInfo victimInfo = new GenericCharacterInfo(victimBody);
                            if (OnApplyDamageCapModifiers != null) OnApplyDamageCapModifiers(damageInfo, attackerInfo, victimInfo, ref damage);
                            return damage;
                        });
                        c.Emit(OpCodes.Stloc, damagePosition);
                    }
                    else ErrorHookFailed("on apply damage cap modifiers");
                }
                else ErrorHookFailed("get HealthComponent local variable positions");
            };

            IL.RoR2.SceneDirector.PopulateScene += (il) =>
            {
                ILCursor c = new ILCursor(il);

                ILLabel label = null;

                if (c.TryGotoNext(
                    x => x.MatchCallOrCallvirt<SceneInfo>("get_instance"),
                    x => x.MatchCallOrCallvirt<SceneInfo>("get_countsAsStage"),
                    x => x.MatchBrfalse(out label)
                ))
                {
                    c.GotoLabel(label);
                    c.Emit(OpCodes.Ldloc, 11);
                    c.EmitDelegate<System.Action<Xoroshiro128Plus>>((xoroshiro128Plus) =>
                    {
                        if (SceneInfo.instance.countsAsStage)
                        {
                            if (OnPopulateScene != null) OnPopulateScene(xoroshiro128Plus);
                        }
                    });
                }
                else ErrorHookFailed("on populate scene");
            };

            GlobalEventManager.OnInteractionsGlobal += (interactor, interactable, interactableObject) =>
            {
                MonoBehaviour interactableAsMonoBehaviour = (MonoBehaviour)interactable;
                bool canProc = !interactableAsMonoBehaviour.GetComponent<GenericPickupController>() && !interactableAsMonoBehaviour.GetComponent<VehicleSeat>() && !interactableAsMonoBehaviour.GetComponent<NetworkUIPromptController>();
                InteractionProcFilter interactionProcFilter = interactableObject.GetComponent<InteractionProcFilter>();
                if (interactionProcFilter) canProc = interactionProcFilter.shouldAllowOnInteractionBeginProc;
                if (OnInteractionBegin != null) OnInteractionBegin(interactor, interactable, interactableObject, canProc);
            };
        }

        public class MysticsRisky2UtilsDamageEvents : MonoBehaviour, IOnIncomingDamageServerReceiver, IOnTakeDamageServerReceiver
        {
            public HealthComponent healthComponent;
            public CharacterBody victimBody;

            public void Start()
            {
                healthComponent = GetComponent<HealthComponent>();
                if (!healthComponent) {
                    Object.Destroy(this);
                    return;
                }
                victimBody = healthComponent.body;
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                GenericCharacterInfo attackerInfo = new GenericCharacterInfo();
                if (damageInfo.attacker) attackerInfo = new GenericCharacterInfo(damageInfo.attacker.GetComponent<CharacterBody>());
                GenericCharacterInfo victimInfo = new GenericCharacterInfo(victimBody);
                if (BeforeTakeDamage != null) BeforeTakeDamage(damageInfo, attackerInfo, victimInfo);
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (victimBody && OnTakeDamage != null) OnTakeDamage(damageReport);
            }
        }
    }
}