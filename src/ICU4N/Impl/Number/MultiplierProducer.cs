namespace ICU4N.Numerics
{
    /// <summary>
    /// An interface used by compact notation and scientific notation to choose a multiplier while rounding.
    /// </summary>
    internal interface IMultiplierProducer // ICU4N TODO: API - this was public in ICU4J
    {
        int GetMultiplier(int magnitude);
    }
}
