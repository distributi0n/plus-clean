namespace Plus.HabboHotel.Rooms.AI
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Catalog.Utilities;
    using Speech;
    using Types;

    public class RoomBot
    {
        public BotAIType AiType;

        public bool AutomaticChat;
        public int BotId;

        public int DanceId;
        public string Gender;
        public int Id;

        public string Look;
        public int maxX;
        public int maxY;
        public int minX;
        public int minY;
        public bool MixSentences;
        public string Motto;
        public string Name;

        public int ownerID;
        public List<RandomSpeech> RandomSpeech;
        public int RoomId;

        public RoomUser RoomUser;
        public int Rot;
        public int SpeakingInterval;
        public int VirtualId;

        public string WalkingMode;

        public int X;
        public int Y;
        public double Z;

        public RoomBot(int BotId,
            int RoomId,
            string AiType,
            string WalkingMode,
            string Name,
            string Motto,
            string Look,
            int X,
            int Y,
            double Z,
            int Rot,
            int minX,
            int minY,
            int maxX,
            int maxY,
            ref List<RandomSpeech> Speeches,
            string Gender,
            int Dance,
            int ownerID,
            bool AutomaticChat,
            int SpeakingInterval,
            bool MixSentences,
            int ChatBubble)
        {
            Id = BotId;
            this.BotId = BotId;
            this.RoomId = RoomId;
            this.Name = Name;
            this.Motto = Motto;
            this.Look = Look;
            this.Gender = Gender.ToUpper();
            this.AiType = BotUtility.GetAIFromString(AiType);
            this.WalkingMode = WalkingMode;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.Rot = Rot;
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
            VirtualId = -1;
            RoomUser = null;
            DanceId = Dance;
            LoadRandomSpeech(Speeches);

            //this.LoadResponses(Responses);
            this.ownerID = ownerID;
            this.AutomaticChat = AutomaticChat;
            this.SpeakingInterval = SpeakingInterval;
            this.MixSentences = MixSentences;
            this.ChatBubble = ChatBubble;
            ForcedMovement = false;
            TargetCoordinate = new Point();
            TargetUser = 0;
        }

        public bool ForcedMovement { get; set; }
        public int ForcedUserTargetMovement { get; set; }
        public Point TargetCoordinate { get; set; }

        public int TargetUser { get; set; }

        public bool IsPet => AiType == BotAIType.PET;

        public int ChatBubble { get; set; }

        public void LoadRandomSpeech(List<RandomSpeech> Speeches)
        {
            RandomSpeech = new List<RandomSpeech>();
            foreach (var Speech in Speeches)
            {
                if (Speech.BotID == BotId)
                {
                    RandomSpeech.Add(Speech);
                }
            }
        }

        public RandomSpeech GetRandomSpeech()
        {
            var rand = new Random();
            if (RandomSpeech.Count < 1)
            {
                return new RandomSpeech("", 0);
            }

            return RandomSpeech[rand.Next(0, RandomSpeech.Count - 1)];
        }

        public BotAI GenerateBotAI(int VirtualId)
        {
            switch (AiType)
            {
                case BotAIType.PET:
                    return new PetBot(VirtualId);
                case BotAIType.GENERIC:
                    return new GenericBot(VirtualId);
                case BotAIType.BARTENDER:
                    return new BartenderBot(VirtualId);
                default:
                    return new GenericBot(VirtualId);
            }
        }
    }
}