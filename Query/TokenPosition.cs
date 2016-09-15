namespace ALinq.Dynamic
{
    internal struct TokenPosition
    {
        public int Line { get; set; }

        public int Column { get; set; }

        public int Sequence { get; set; }

        public static implicit operator int(TokenPosition pos)
        {
            return pos.Sequence;
        }
    }
}