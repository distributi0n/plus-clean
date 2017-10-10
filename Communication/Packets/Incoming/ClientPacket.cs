namespace Plus.Communication.Packets.Incoming
{
    using System;
    using Utilities;

    public class ClientPacket
    {
        private byte[] Body;
        private int Pointer;

        public ClientPacket(int messageID, byte[] body)
        {
            Init(messageID, body);
        }

        public int Id { get; private set; }

        public int RemainingLength => Body.Length - Pointer;

        public int Header => Id;

        public void Init(int messageID, byte[] body)
        {
            if (body == null)
            {
                body = new byte[0];
            }
            Id = messageID;
            Body = body;
            Pointer = 0;
        }

        public override string ToString() => "[" + Header + "] BODY: " +
                                             PlusEnvironment.GetDefaultEncoding().GetString(Body)
                                                 .Replace(Convert.ToChar(0).ToString(), "[0]");

        public void AdvancePointer(int i)
        {
            Pointer += i * 4;
        }

        public byte[] ReadBytes(int Bytes)
        {
            if (Bytes > RemainingLength)
            {
                Bytes = RemainingLength;
            }
            var data = new byte[Bytes];
            for (var i = 0; i < Bytes; i++)
            {
                data[i] = Body[Pointer++];
            }

            return data;
        }

        public byte[] PlainReadBytes(int Bytes)
        {
            if (Bytes > RemainingLength)
            {
                Bytes = RemainingLength;
            }
            var data = new byte[Bytes];
            for (int x = 0, y = Pointer; x < Bytes; x++, y++)
            {
                data[x] = Body[y];
            }

            return data;
        }

        public byte[] ReadFixedValue()
        {
            var len = HabboEncoding.DecodeInt16(ReadBytes(2));
            return ReadBytes(len);
        }

        public string PopString() => PlusEnvironment.GetDefaultEncoding().GetString(ReadFixedValue());

        public bool PopBoolean()
        {
            if (RemainingLength > 0 && Body[Pointer++] == Convert.ToChar(1))
            {
                return true;
            }

            return false;
        }

        public int PopInt()
        {
            if (RemainingLength < 1)
            {
                return 0;
            }

            var Data = PlainReadBytes(4);
            var i = HabboEncoding.DecodeInt32(Data);
            Pointer += 4;
            return i;
        }
    }
}