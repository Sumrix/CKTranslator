﻿namespace Core.Matching
{
    public class CalculationCell
    {
        public MaxFRoute Route0 = new();
        public MaxFRoute Route1 = new();

        public override string ToString()
        {
            FVector maxVector = FVector.Max(
                this.Route0?.Null,
                this.Route0?.Value,
                this.Route1?.Null,
                this.Route1?.Value
            );

            return maxVector == null
                ? "null"
                : $"{maxVector?.Letters0}; {maxVector?.Letters1}; {maxVector?.PathAverage}";
        }
    }
}