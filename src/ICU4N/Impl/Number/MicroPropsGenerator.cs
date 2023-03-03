namespace ICU4N.Numerics
{
    /// <summary>
    /// This interface is used when all number formatting settings, including the locale, are known, except for the quantity
    /// itself. The <see cref="ProcessQuantity(IDecimalQuantity)"/> method performs the final step in the number processing pipeline: it uses the
    /// quantity to generate a finalized <see cref="MicroProps"/>, which can be used to render the number to output.
    /// <para/>
    /// In other words, this interface is used for the parts of number processing that are <em>quantity-dependent</em>.
    /// <para/>
    /// In order to allow for multiple different objects to all mutate the same MicroProps, a "chain" of <see cref="IMicroPropsGenerator"/>s
    /// are linked together, and each one is responsible for manipulating a certain quantity-dependent part of the
    /// <see cref="MicroProps"/>. At the top of the linked list is a base instance of <see cref="MicroProps"/> with properties that are not
    /// quantity-dependent. Each element in the linked list calls <see cref="ProcessQuantity(IDecimalQuantity)"/> on its "parent", then does its
    /// work, and then returns the result.
    /// <para/>
    /// A class implementing <see cref="IMicroPropsGenerator"/> looks something like this:
    /// 
    /// <code>
    /// class Foo : IMicroPropsGenerator
    /// {
    ///     private readonly IMicroPropsGenerator parent;
    ///     
    ///     public Foo(IMicroPropsGenerator parent)
    ///     {
    ///         this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
    ///     }
    ///     
    ///     public MicroProps ProcessQuantity(IDecimalQuantity quantity)
    ///     {
    ///         MicroProps micros = this.parent.ProcessQuantity(quantity);
    ///         // Perform manipulations on micros and/or quantity
    ///         return micros;
    ///     }
    /// }
    /// </code>
    /// </summary>
    /// <author>sffc</author>
    internal interface IMicroPropsGenerator // ICU4N TODO: API - this was public in ICU4J
    {
        MicroProps ProcessQuantity(IDecimalQuantity quantity);
    }
}
