namespace Plus.Communication.Packets.Incoming.Inventory.AvatarEffects
{
    using HabboHotel.GameClients;
    using Outgoing.Inventory.AvatarEffects;

    internal sealed class AvatarEffectActivatedEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var EffectId = Packet.PopInt();
            var Effect = Session.GetHabbo().Effects().GetEffectNullable(EffectId, false, true);
            if (Effect == null || Session.GetHabbo().Effects().HasEffect(EffectId, true))
            {
                return;
            }

            if (Effect.Activate())
            {
                Session.SendPacket(new AvatarEffectActivatedComposer(Effect));
            }
        }
    }
}