using RimWorld;
using System.Linq;
using Verse;

namespace ProjectOvermind
{
    public class CompProperties_UseEffect_LearnAbility : CompProperties_Usable
    {
        public AbilityDef abilityDef;
        public int levelRequired = 1;

        public CompProperties_UseEffect_LearnAbility()
        {
            compClass = typeof(CompUseEffect_LearnAbility);
        }
    }

    public class CompUseEffect_LearnAbility : CompUseEffect
    {
        public CompProperties_UseEffect_LearnAbility Props => (CompProperties_UseEffect_LearnAbility)props;

        public override void DoEffect(Pawn user)
        {
            base.DoEffect(user);

            if (Props.abilityDef == null)
            {
                Log.Error($"[ProjectOvermind] CompUseEffect_LearnAbility: abilityDef is null for {parent.def.defName}");
                return;
            }

            // Check if pawn has abilities comp
            Pawn_AbilityTracker abilities = user.abilities;
            if (abilities == null)
            {
                Messages.Message(
                    $"{user.LabelShort} cannot learn psycasts.",
                    user,
                    MessageTypeDefOf.RejectInput,
                    historical: false
                );
                return;
            }

            // Check if pawn has psylink and level requirement
            int psylinkLevel = user.GetPsylinkLevel();
            
            if (psylinkLevel == 0)
            {
                Messages.Message(
                    $"{user.LabelShort} must have a psylink to use this psytrainer.",
                    user,
                    MessageTypeDefOf.RejectInput,
                    historical: false
                );
                return;
            }
            
            if (psylinkLevel < Props.levelRequired)
            {
                Messages.Message(
                    $"{user.LabelShort} needs psylink level {Props.levelRequired} to learn this ability (currently level {psylinkLevel}).",
                    user,
                    MessageTypeDefOf.RejectInput,
                    historical: false
                );
                return;
            }

            // Check if already knows the ability
            if (abilities.GetAbility(Props.abilityDef) != null)
            {
                Messages.Message(
                    $"{user.LabelShort} already knows {Props.abilityDef.LabelCap}.",
                    user,
                    MessageTypeDefOf.RejectInput,
                    historical: false
                );
                return;
            }

            // Grant the ability
            abilities.GainAbility(Props.abilityDef);

            Messages.Message(
                $"{user.LabelShort} learned {Props.abilityDef.LabelCap}!",
                user,
                MessageTypeDefOf.PositiveEvent,
                historical: true
            );
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (Props.abilityDef == null)
            {
                return "Missing ability definition";
            }

            if (p.abilities == null)
            {
                return $"{p.LabelShort} cannot learn abilities.";
            }

            int psylinkLevel = p.GetPsylinkLevel();
            if (psylinkLevel == 0)
            {
                return $"{p.LabelShort} has no psylink.";
            }

            if (psylinkLevel < Props.levelRequired)
            {
                return $"Requires psylink level {Props.levelRequired} (currently {psylinkLevel}).";
            }

            if (p.abilities.GetAbility(Props.abilityDef) != null)
            {
                return $"{p.LabelShort} already knows this.";
            }

            return true;
        }
    }
}
