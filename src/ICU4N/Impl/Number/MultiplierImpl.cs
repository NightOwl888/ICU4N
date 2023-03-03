namespace ICU4N.Numerics
{
    internal class Multiplier : IMicroPropsGenerator// ICU4N TODO: API - this was public in ICU4J
    {
        internal readonly int magnitudeMultiplier;
        internal readonly BigMath.BigDecimal bigDecimalMultiplier;
        internal readonly IMicroPropsGenerator parent;

        public Multiplier(int magnitudeMultiplier)
        {
            this.magnitudeMultiplier = magnitudeMultiplier;
            this.bigDecimalMultiplier = null;
            parent = null;
        }

        public Multiplier(BigMath.BigDecimal bigDecimalMultiplier)
        {
            this.magnitudeMultiplier = 0;
            this.bigDecimalMultiplier = bigDecimalMultiplier;
            parent = null;
        }

        private Multiplier(Multiplier @base, IMicroPropsGenerator parent)
        {
            this.magnitudeMultiplier = @base.magnitudeMultiplier;
            this.bigDecimalMultiplier = @base.bigDecimalMultiplier;
            this.parent = parent;
        }

        public virtual IMicroPropsGenerator CopyAndChain(IMicroPropsGenerator parent)
        {
            return new Multiplier(this, parent);
        }

        public virtual MicroProps ProcessQuantity(IDecimalQuantity quantity)
        {
            MicroProps micros = parent.ProcessQuantity(quantity);
            quantity.AdjustMagnitude(magnitudeMultiplier);
            if (bigDecimalMultiplier != null)
            {
                quantity.MultiplyBy(bigDecimalMultiplier);
            }
            return micros;
        }
    }
}
