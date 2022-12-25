namespace ICU4N.Numerics
{
    /// <author>sffc</author>
    internal interface IMicroPropsMutator<T>
    {
        void MutateMicros(MicroProps micros, T value);
    }
}
