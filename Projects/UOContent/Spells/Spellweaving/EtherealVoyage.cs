using System;
using Server.Engines.BuffIcons;
using Server.Mobiles;

namespace Server.Spells.Spellweaving
{
    public class EtherealVoyageSpell : ArcaneForm
    {
        private static readonly SpellInfo _info = new(
            "Ethereal Voyage",
            "Orlavdra",
            -1
        );

        public EtherealVoyageSpell(Mobile caster, Item scroll = null) : base(caster, scroll, _info)
        {
        }

        public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(3.5);

        public override double RequiredSkill => 24.0;
        public override int RequiredMana => 32;

        public override int Body => 0x302;
        public override int Hue => 0x48F;

        public static void Initialize()
        {
            EventSink.AggressiveAction += RemoveTransformationOnAggressiveAction;
        }

        public static void RemoveTransformationOnAggressiveAction(AggressiveActionEventArgs e)
        {
            if (TransformationSpellHelper.UnderTransformation(e.Aggressor, typeof(EtherealVoyageSpell)))
            {
                TransformationSpellHelper.RemoveContext(e.Aggressor, true);
            }
        }

        public override bool CheckCast()
        {
            if (TransformationSpellHelper.UnderTransformation(Caster, typeof(EtherealVoyageSpell)))
            {
                Caster.SendLocalizedMessage(501775); // This spell is already in effect.
            }
            else if (!Caster.CanBeginAction<EtherealVoyageSpell>())
            {
                Caster.SendLocalizedMessage(1075124); // You must wait before casting that spell again.
            }
            else if (Caster.Combatant != null)
            {
                Caster.SendLocalizedMessage(1072586); // You cannot cast Ethereal Voyage while you are in combat.
            }
            else
            {
                return base.CheckCast();
            }

            return false;
        }

        public override void DoEffect(Mobile m)
        {
            m.PlaySound(0x5C8);
            m.SendLocalizedMessage(1074770); // You are now under the effects of Ethereal Voyage.

            var skill = Caster.Skills.Spellweaving.Value;

            var duration = TimeSpan.FromSeconds(12 + (int)(skill / 24) + FocusLevel * 2);

            Timer.StartTimer(duration, () => RemoveEffect(Caster));

            // Cannot cast this spell for another 5 minutes(300sec) after effect removed.
            Caster.BeginAction<EtherealVoyageSpell>();

            (Caster as PlayerMobile)?.AddBuff(new BuffInfo(BuffIcon.EtherealVoyage, 1031613, 1075805, duration));
        }

        public override void RemoveEffect(Mobile m)
        {
            m.SendLocalizedMessage(1074771); // You are no longer under the effects of Ethereal Voyage.

            TransformationSpellHelper.RemoveContext(m, true);

            Timer.StartTimer(TimeSpan.FromMinutes(5), m.EndAction<EtherealVoyageSpell>);

            (m as PlayerMobile)?.RemoveBuff(BuffIcon.EtherealVoyage);
        }
    }
}
