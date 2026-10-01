namespace lab1
{
    internal class AssociationRule
    {
        public string Antecedent { get; private set; }
        public string Consequent { get; private set; }

        public double Support { get; private set; }
        public double Confidence { get; private set; }
        public double Lift { get; private set; }

        public AssociationRule(
            string antecedent,
            string consequent,
            double support,
            double confidence,
            double lift)
        {
            Antecedent = antecedent;
            Consequent = consequent;
            Support = support;
            Confidence = confidence;
            Lift = lift;
        }

        public override string ToString()
        {
            return $"{Antecedent} -> {Consequent} " +
                   $"(поддержка: {Support:P2}, " +
                   $"достоверность: {Confidence:P2}, " +
                   $"лифт: {Lift:F2})";
        }
    }
}