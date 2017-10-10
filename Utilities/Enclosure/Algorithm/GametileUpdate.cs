namespace Plus.Utilities.Enclosure.Algorithm
{
    public class GametileUpdate
    {
        public GametileUpdate(int x, int y, byte value)
        {
            this.x = x;
            this.y = y;
            this.value = value;
        }

        public byte value { get; }
        public int y { get; }
        public int x { get; }
    }
}