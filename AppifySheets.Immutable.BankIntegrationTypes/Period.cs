using System;

namespace AppifySheets.Immutable.BankIntegrationTypes;

public class Period
{
    public Period(DateTime from, DateTime till)
    {
        if (till < from)
            throw new InvalidOperationException("Bad period");

        From = from;
        Till = till;
    }

    public DateTime From { get; }
    public DateTime Till { get; }
    public override string ToString() => $"Period: [{From:yyyy-MM-dd@HH-mm} - {Till:yyyy-MM-dd@HH-mm}]";
}