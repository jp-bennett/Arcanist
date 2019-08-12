using ArcaneTide.Arcanist;
using ArcaneTide.Utils;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Risia {
    static public class RisiaAddBrain {
        static internal BlueprintCharacterClass arcanist => ArcanistClass.arcanist;
        static internal LibraryScriptableObject library => Main.library;
        static internal GlobalConstants consts => Main.constsManager;
        static internal BlueprintUnit risiaBoss;
        static private BlueprintAiCastSpell CreateCastSpell(string actionName, string actionId, BlueprintAbility spell, double baseScore = 20.0, int actorConsiderationCnt = 0, int targetConsiderationCnt = 0, int CombatCnt = 10, DiceFormula CooldownRoundsDice = new DiceFormula(), int StartCooldownRounds = 0, BlueprintAbility spellVariant = null, params Consideration[] considerations) {
            if (actorConsiderationCnt + targetConsiderationCnt > considerations.Length) {
                throw new Exception($"Fuck, wrong actor considerations {actorConsiderationCnt} and target considerations {targetConsiderationCnt} ! consideration params have {considerations.Length}");
            }
            var ans = Helpers.Create<BlueprintAiCastSpell>();
            ans.Ability = spell;
            ans.name = actionName;
            List<Consideration> actorConsList = new List<Consideration>();
            List<Consideration> targetConsList = new List<Consideration>();
            for (int i = 0; i < actorConsiderationCnt; i++) {
                actorConsList.Add(considerations[i]);
            }
            for (int i = actorConsiderationCnt; i < actorConsiderationCnt + targetConsiderationCnt; i++) {
                targetConsList.Add(considerations[i]);
            }
            ans.ActorConsiderations = actorConsList.ToArray();
            ans.TargetConsiderations = targetConsList.ToArray();
            ans.Variant = spellVariant;
            ans.CombatCount = CombatCnt;
            ans.CooldownDice = CooldownRoundsDice;
            ans.StartCooldownRounds = StartCooldownRounds;
            library.AddAsset(ans, actionId);
            return ans;
        }
        static private Consideration getConsideration(string s) {
            return library.Get<Consideration>(s);
        }
        static private BlueprintAbility getSpell(string s) {
            return library.Get<BlueprintAbility>(s);
        }
        static private DiceFormula getConstant(int c) {
            return new DiceFormula(c, DiceType.One);
        }
        static private DiceFormula getDice(int k, DiceType f = DiceType.One) {
            return new DiceFormula(k, f);
        }
        static private HasSpellPerDayRemainedConsideration considerSpellSlot(int x) {
            var ans = Helpers.Create<HasSpellPerDayRemainedConsideration>(a => {
                a.spellbookBlue = ArcanistClass.arcanist.Spellbook;
                a.level = x;
            });
            return ans;
        }
        static private BuffConsideration considerNoBuff(string name, BlueprintBuff[] buffs, float hasBuff = 0f, float noBuff = 1f) {
            var ans = Helpers.Create<BuffConsideration>();
            ans.BaseScoreModifier = 1.0f;
            ans.HasBuffScore = hasBuff ;
            ans.NoBuffScore = noBuff;
            ans.Buffs = buffs;
            library.AddAsset(ans, OtherUtils.GetMd5(name));
            return ans;
        }
        
        static private BuffConsideration considerNoBuff(string name, BlueprintBuff buff, float hasBuff = 0f, float noBuff = 1f) {
            return considerNoBuff(name, new BlueprintBuff[] { buff }, hasBuff, noBuff);
        }
        static private BuffConsideration considerHasBuff(string name, BlueprintBuff buff, float hasBuff = 1f, float noBuff = 0f) {
            return considerNoBuff(name, new BlueprintBuff[] { buff }, hasBuff, noBuff);
        }
        static private BuffsAroundConsideration considerBuffsAround(string name, TargetType filter, BlueprintBuff[] buffs, int maxCount = 4, float maxScore = 1f, int minCount = 1, float minScore = 0f) {
            var ans = Helpers.Create<BuffsAroundConsideration>();
            ans.Buffs = buffs;
            ans.Filter = filter;
            ans.MaxCount = maxCount;
            ans.MaxScore = maxScore;
            ans.MinCount = minCount;
            ans.MinScore = minScore;
            ans.BelowMinScore = minScore;
            library.AddAsset(ans, OtherUtils.GetMd5(name));
            return ans;
        }
        static public void Load() {
            risiaBoss = library.Get<BlueprintUnit>("95fb27a5b8ae40099bd727ea93de5b9b");
            BlueprintBrain brain = Helpers.Create<BlueprintBrain>();

            BlueprintBuff holyAuraBuff = library.Get<BlueprintBuff>("a33bf327207a5904d9e38d6a80eb09e2");
            BlueprintBuff unholyAuraBuff = library.Get<BlueprintBuff>("9eda82a1f78558747a03c17e0e9a1a68");

            Consideration hasSwift = getConsideration("c2b7d2f9a5cb8d04d9e1aa4bf3d3c598");
            Consideration hasStandard = getConsideration("a82d061edd18ce748a1a7f97e7e6d9d2");

            Consideration targetSelf = getConsideration("83e2dd97b82d769498394c3edf0d260e");

            Consideration attackTargetPriority = getConsideration("7a2b25dcc09cd244db261ce0a70cca84");

            Consideration noShieldBuff = getConsideration("a3ffff7b93017744ea88433311569cec");
            Consideration noMirrorBuff = getConsideration("db074912aa8072c469b527f6c111e82c");
            Consideration noInvisibilityGreaterBuff = getConsideration("2fc05579e43f56146a1cdaaa82e5119c");
            Consideration noFieryBodyBuff = considerNoBuff("RisiaNoFieryBodyBuffConsideration", library.Get<BlueprintBuff>("b574e1583768798468335d8cdb77e94c"));
            Consideration aroundHasHolyAuraBuff = considerBuffsAround(
                "RisiaEnemyHasHolyAuraBuffConsideration", TargetType.Enemy,
                new BlueprintBuff[] { holyAuraBuff }
                );
            Consideration aroundHasUnholyAuraBuff = considerBuffsAround(
                "RisiaEnemyHasUnholyAuraBuffConsideration", TargetType.Enemy,
                new BlueprintBuff[] { unholyAuraBuff }
                );
            Consideration selfHasARBoostDCBuff = considerHasBuff(
                "RisiaHasARBoostDCBuff", ArcaneReservoir.AR_AddDCBuff
                );
            Consideration selfHasNoARBoostDCBuff = considerNoBuff(
                "RisiaHasNoARBoostDCBuff", ArcaneReservoir.AR_AddDCBuff
                );
            BlueprintAbility shield_swift = getSpell("3c1b92a0a3ce0754a889fb0d7b2c23a4");
            BlueprintAbility mirror_swift = getSpell(OtherUtils.GetMd5("Risia.MirrorImageSwift"));
            BlueprintAbility invisiblity_greater_swift = getSpell(OtherUtils.GetMd5("Risia.GreaterInvisibility.Swift"));
            BlueprintAbility fiery_body = getSpell("08ccad78cac525040919d51963f9ac39");
            BlueprintAbility overwhelmingPresence = getSpell("41cf93453b027b94886901dbfc680cb9");
            BlueprintAbility weird = getSpell("870af83be6572594d84d276d7fc583e0");
            BlueprintAbility summon7Base = getSpell("ab167fd8203c1314bac6568932f1752f");
            BlueprintAbility summon7_1d3 = getSpell("43f763d347eb2744caed9c656ba89531");
            BlueprintAbility summon8Base = getSpell("d3ac756a229830243a72e84f3ab050d0");
            BlueprintAbility summon8Single = getSpell("eb6df7ddfc0669d4fb3fc9af4bd34bca");
            BlueprintAbility DMHolyAura = getSpell(OtherUtils.GetMd5("Risia.DispelMagicArea.HolyAura"));
            BlueprintAbility DMUnholyAura = getSpell(OtherUtils.GetMd5("Risia.DispelMagicArea.UnholyAura"));

            BlueprintAbility seamantle_preBuff = getSpell(consts.GUIDs["RisiaSeamantleFree_N"]);
            BlueprintAbility angelicAspectGreater_preBuff = getSpell(consts.GUIDs["RisiaAngelicAspectFree_N"]);
            List<BlueprintAiAction> actions = new List<BlueprintAiAction>();
            //Free Action Prebuffs
            //create seamantle
            var castPrebuffSeamantle = CreateCastSpell(
                "RisiaCastPrebuffSeamantle", OtherUtils.GetMd5("Risia.Brain.CastPrebuffSeamantle_1001"),
                seamantle_preBuff, 1001, 0, 0, 1, getConstant(0), 0, null
                );
            var castPrebuffAngelic = CreateCastSpell(
                "RisiaCastPrebuffAngelic", OtherUtils.GetMd5("Risia.Brain.CastPrebuffAngelic_1000"),
                angelicAspectGreater_preBuff, 1000, 0, 0, 1, getConstant(0), 0, null
                );
            actions.AddRange(new BlueprintAiCastSpell[] { castPrebuffSeamantle, castPrebuffAngelic });
            //Buffs
            //create cast swift shield
            var castSwiftShield = CreateCastSpell(
                "RisiaCastShieldSwift", OtherUtils.GetMd5("Risia.Brain.CastShieldSwift_51.5"),
                shield_swift, 51.5, 1, 2, 3, getConstant(4), 0, null, hasSwift, targetSelf, noShieldBuff);
            //create cast swift mirror
            var castMirror = CreateCastSpell(
                "RisiaCastMirrorSwift", OtherUtils.GetMd5("Risia.Brain.CastMirrorSwift_53.5"),
                mirror_swift, 53.5, 1, 2, 5, getConstant(2), 0, null, hasSwift, targetSelf, noMirrorBuff);
            //create cast swift invisibility greater
            var castInvisibilityGreater = CreateCastSpell(
                "RisiaCastIGSwift", OtherUtils.GetMd5("Risia.Brain.CastInvisibilityGreaterSwift_52.5"),
                invisiblity_greater_swift, 52.5, 1, 2, 2, getConstant(4), 0, null, hasSwift, targetSelf, noInvisibilityGreaterBuff);
            //create cast fiery body
            var castFieryBody = CreateCastSpell(
                "RisiaCastFieryBody", OtherUtils.GetMd5("Risia.Brain.CastFieryBody_afterShield_51.0"),
                fiery_body, 51.0, 1, 2, 1, getConstant(0), 0, null, hasStandard, targetSelf, noFieryBodyBuff);
            
            actions.AddRange(new BlueprintAiCastSpell[] { castSwiftShield, castMirror, castInvisibilityGreater, castFieryBody });

            // Control spells
            
            var castDMHolyAura = CreateCastSpell(
                "RisiaCastDMHolyAura", OtherUtils.GetMd5("Risia.Brain.CastDMHolyAuraOnly_40.0"),
                DMHolyAura, 40.0, 1, 1, 2, getConstant(3), 0, null,
                hasStandard, aroundHasHolyAuraBuff
                );
            var castDMUnholyAura = CreateCastSpell(
                "RisiaCastDMUnholyAura", OtherUtils.GetMd5("Risia.Brain.CastDMUnholyAuraOnly_40.0"),
                DMUnholyAura, 40.0, 1, 1, 2, getConstant(3), 0, null,
                hasStandard, aroundHasUnholyAuraBuff
                );
            var castArcaneReserviorBoostDC = CreateCastSpell(
                "RisiaCastArcaneReservoirBoostDC", OtherUtils.GetMd5("Risia.Brain.ArcaneReservoirBoostDC_19.9"),
                ArcaneReservoir.AR_AddDCAbl, 19.9, 2, 0, 10, getConstant(1), 0, null,
                hasStandard, selfHasNoARBoostDCBuff);
                
            var castOverwhelmingPresence = CreateCastSpell(
                "RisiaCastOverwhelming", OtherUtils.GetMd5("Risia.Brain.CastOverwhelming_19.0"),
                overwhelmingPresence, 19.0, 3, 1, 2, getConstant(4), 0, null,
                considerSpellSlot(9), hasStandard, selfHasARBoostDCBuff, attackTargetPriority
                );
            var castWeird = CreateCastSpell(
                "RisiaCastWeird", OtherUtils.GetMd5("Risia.Brain.CastWeird_20.0"),
                weird, 19.0, 3, 1, 2, getDice(2, DiceType.D3), 0, null,
                considerSpellSlot(9), hasStandard, selfHasARBoostDCBuff, attackTargetPriority
                );
            //var castPowerWordStun = CreateCastSpell(
                //)
            actions.AddRange(new BlueprintAiCastSpell[] { castDMHolyAura, castDMUnholyAura, castOverwhelmingPresence });
            
            // Summon spells
            var castSummon7 = CreateCastSpell(
                "RisiaCastSummonVII1d3", OtherUtils.GetMd5("Risia.Brain.CastSummonVII1d3_Consecute_25.0"),
                summon7Base, 25.0, 0, 0, 3, getDice(2, DiceType.D3), 2, summon7_1d3
                );
            var castSummon8 = CreateCastSpell(
                "RisiaCastSummonVIII", OtherUtils.GetMd5("Risia.Brain.CastSummonVIIISingle_Consecute_25.0"),
                summon8Base, 25.0, 0, 0, 3, getDice(1, DiceType.D6), 3, summon8Single
                );
            actions.AddRange(new BlueprintAiCastSpell[] { castSummon7, castSummon8 });

            // Attack
            actions.Add(library.Get<BlueprintAiAction>("866ffa6c34000cd4a86fb1671f86c7d8"));//simple attack

            brain.Actions = actions.ToArray();
            brain.name = "Risia.Brain";
            library.AddAsset(brain, OtherUtils.GetMd5("Risia.Boss.Brain"));
            risiaBoss.Brain = brain;
        }
    }
    public class HasSpellPerDayRemainedConsideration : Consideration {
        public override float Score([NotNull] DecisionContext context) {
            var spellbook = context.Unit.Descriptor.DemandSpellbook(spellbookBlue);
            if (spellbook == null) return 0f;
            int maxSpellLevel = spellbook.MaxSpellLevel;
            if (maxSpellLevel < level || level < 1) return 0f;
            return spellbook.GetSpontaneousSlots(level) > 0 ? 1f : 0f;
        }
        public BlueprintSpellbook spellbookBlue;
        public int level;
    }

    

}
