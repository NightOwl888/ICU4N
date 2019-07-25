namespace ICU4N.Util
{
    /// <summary>
    /// Provides a flexible mechanism for controlling access, without requiring that
    /// a class be immutable.
    /// </summary>
    /// <remarks>
    /// Once frozen, an object can never be unfrozen, so it is
    /// thread-safe from that point onward. Once the object has been frozen, 
    /// it must guarantee that no changes can be made to it. Any attempt to alter 
    /// it must raise a <see cref="System.NotSupportedException"/> exception. This means that when 
    /// the object returns internal objects, or if anyone has references to those internal
    /// objects, that those internal objects must either be immutable, or must also
    /// raise exceptions if any attempt to modify them is made. Of course, the object
    /// can return clones of internal objects, since those are safe.
    /// <para/>
    /// <h2>Background</h2>
    /// <para/>
    /// There are often times when you need objects to be objects 'safe', so that
    /// they can't be modified. Examples are when objects need to be thread-safe, or
    /// in writing robust code, or in caches. If you are only creating your own
    /// objects, you can guarantee this, of course -- but only if you don't make a
    /// mistake. If you have objects handed into you, or are creating objects using
    /// others handed into you, it is a different story. It all comes down to whether
    /// you want to take the Blanche Dubois approach (&quot;depend on the kindness of
    /// strangers&quot;) or the Andy Grove approach (&quot;Only the Paranoid
    /// Survive&quot;).
    /// <para/>
    /// For example, suppose we have a simple class:
    /// <code>
    /// public class A
    /// {
    ///     public ICollection{string} B { get; protected set; }
    ///     public ICollection{string} C { get; protected set; }
    ///     
    ///     public A(ICollection{string} b, ICollection{string} c)
    ///     {
    ///         this.B = b;
    ///         this.C = c;
    ///     }
    /// }
    /// </code>
    /// <para/>
    /// Since the class doesn't have any public setters, someone might think that it is
    /// immutable. You know where this is leading, of course; this class is unsafe in
    /// a number of ways. The following illustrates that.
    /// <code>
    /// public Test1(SupposedlyImmutableClass x, SafeStorage y)
    /// {
    ///     // unsafe getter
    ///     A a = x.A;
    ///     ICollection{string} col = a.B;
    ///     col.Add(something); // a has now been changed, and x too
    ///     
    ///     // unsafe constructor
    ///     a = new A(col, col);
    ///     y.Store(a);
    ///     col.Add(something); // a has now been changed, and y too
    /// }
    /// </code>
    /// <para/>
    /// There are a few different techniques for having safe classes.
    /// <para/>
    /// <list type="number">
    ///     <item><term>Const objects.</term><description>In C++, you can declare parameters const.</description></item>
    ///     <item><term>Immutable wrappers.</term><description>For example, you can put a collection in an immutable wrapper.</description></item>
    ///     <item><term>Always-Immutable objects.</term><description>.NET uses this approach, with a few variations. Examples:
    ///         <list type="number">
    ///             <item><term>Simple.</term><description>Once a <see cref="System.Collections.Generic.KeyValuePair{TKey, TValue}"/> is created it is immutable.</description></item>
    ///             <item><term>Builder Class.</term><description>
    ///                 There is a separate 'builder' class. For example, modifiable Strings are created using 
    ///                 <see cref="System.Text.StringBuilder"/> (which doesn't have the
    ///                 full String API available). Once you want an immutable form, you create one
    ///                 with <see cref="System.Text.StringBuilder.ToString()"/>.</description></item>
    ///             <item><term>Primitives.</term><description>These are always safe, since they are copied on input/output from methods.</description></item>
    ///             <item><term></term><description></description></item>
    ///             <item><term></term><description></description></item>
    ///         </list>
    ///     </description></item>
    ///     <item><term>Cloning.</term><description>Where you need an object to be safe, you clone it.</description></item>
    /// </list>
    /// <para/>
    /// There are advantages and disadvantages of each of these.
    /// <list type="number">
    ///     <item><description>
    ///         Const provides a certain level of protection, but since const can be and
    ///         is often cast away, it only protects against most inadvertent mistakes. It
    ///         also offers no threading protection, since anyone who has a pointer to the
    ///         (unconst) object in another thread can mess you up.
    ///     </description></item>
    ///     <item><description>
    ///         Immutable wrappers are safer than const in that the constness can't be
    ///         cast away. But other than that they have all the same problems: not safe if
    ///         someone else keeps hold of the original object, or if any of the objects
    ///         returned by the class are mutable.
    ///     </description></item>
    ///     <item><description>
    ///         Always-Immutable Objects are safe, but usage can require excessive
    ///         object creation.
    ///     </description></item>
    ///     <item><description>
    ///         Cloning is only safe if the object truly has a 'safe' clone; defined as
    ///         one that <i>ensures that no change to the clone affects the original</i>.
    ///         Unfortunately, many objects don't have a 'safe' clone, and always cloning can
    ///         require excessive object creation.
    ///     </description></item>
    /// </list>
    /// <para/>
    /// <h2>Freezable Model</h2>
    /// <para/>
    /// The <see cref="IFreezable{T}"/> model supplements these choices by giving you
    /// the ability to build up an object by calling various methods, then when it is
    /// in a final state, you can <i>make</i> it immutable. Once immutable, an
    /// object cannot <i>ever </i>be modified, and is completely thread-safe: that
    /// is, multiple threads can have references to it without any synchronization.
    /// If someone needs a mutable version of an object, they can use
    /// <see cref="CloneAsThawed()"/>, and modify the copy. This provides a simple,
    /// effective mechanism for safe classes in circumstances where the alternatives
    /// are insufficient or clumsy. (If an object is shared before it is immutable,
    /// then it is the responsibility of each thread to mutex its usage (as with
    /// other objects).)
    /// <para/>
    /// Here is what needs to be done to implement this interface, depending on the
    /// type of the object.
    /// <para/>
    /// <h3><b>Immutable Objects</b></h3>
    /// <para/>
    /// These are the easiest. You just use the interface to reflect that, by adding
    /// the following:
    /// <code>
    /// public class A : IFreezable&lt;A&gt;
    /// {
    ///     public bool IsFrozen { get { return true; } }
    ///     public A Freeze() { return this; }
    ///     public A CloneAsThawed() { return this; }
    /// }
    /// </code>
    /// <para/>
    /// These can be sealed methods because subclasses of immutable objects must
    /// themselves be immutable. (Note: <see cref="Freeze()"/> is returning
    /// <c>this</c> for chaining.)
    /// <para/>
    /// <h3><b>Mutable Objects</b></h3>
    /// <para/>
    /// Add a protected 'flagging' field:
    /// <code>
    /// protected volatile bool frozen; // WARNING: must be volatile
    /// </code>
    /// <para/>
    /// Add the following methods:
    /// <code>
    /// public bool IsFrozen()
    /// {
    ///     return frozen;
    /// }
    /// 
    /// public A Freeze()
    /// {
    ///     frozen = true;  // WARNING: must be final statement before return
    ///     return this;
    /// }
    /// </code>
    /// <para/>
    /// Add a <see cref="CloneAsThawed()"/> method following the normal pattern for
    /// <c>ICloneable.Clone()</c>, except that <c>frozen=false</c> in the new
    /// clone.
    /// <para/>
    /// Then take the setters (that is, any member that can change the internal state
    /// of the object), and add the following as the first statement:
    /// <code>
    /// if (IsFrozen)
    /// {
    ///     throw new NotSupportedException(&quot;Attempt to modify frozen object&quot;);
    /// }
    /// </code>
    /// <para/>
    /// <h4><b>Subclassing</b></h4>
    /// <para/>
    /// Any subclass of a <see cref="IFreezable{T}"/> will just use its superclass's
    /// flagging field. It must override <see cref="Freeze()"/> and
    /// <see cref="CloneAsThawed()"/> to call the superclass, but normally does not
    /// override <see cref="IsFrozen"/>. It must then just pay attention to its
    /// own properties, methods and fields.
    /// <para/>
    /// <h4><b>Internal Caches</b></h4>
    /// <para/>
    /// Internal caches are cases where the object is logically unmodified, but
    /// internal state of the object changes. For example, there are const C++
    /// functions that cast away the const on the &quot;this&quot; pointer in order
    /// to modify an object cache. These cases are handled by mutexing the internal
    /// cache to ensure thread-safety. For example, suppose that <see cref="Text.UnicodeSet"/> had an
    /// internal marker to the last code point accessed. In this case, the field is
    /// not externally visible, so the only thing you need to do is to synchronize
    /// the field for thread safety.
    /// <para/>
    /// <h4>Unsafe Internal Access</h4>
    /// <para/>
    /// Internal fields are called <i>safe</i> if they are either
    /// <c>frozen</c> or immutable (such as string or primitives). If you've
    /// never allowed internal access to these, then you are all done. For example,
    /// converting <see cref="Text.UnicodeSet"/> to be <see cref="IFreezable{T}"/> is just accomplished
    /// with the above steps. But remember that you <i><b>have</b></i> allowed
    /// access to unsafe internals if you have any code like the following, in a
    /// getter, setter, or constructor:
    /// <code>
    /// ICollection{string} Stuff
    /// {
    ///     get { return stuff; } // caller could keep reference &amp; modify
    ///     set { stuff = value; } // caller could keep reference &amp; modify
    /// }
    /// 
    /// MyClass(ICollection{string} x) // caller could keep reference &amp; modify
    /// {
    ///     stuff = x;
    /// }
    /// </code>
    /// <para/>
    /// These also illustrated in the code sample in <b>Background</b> above.
    /// <para/>
    /// To deal with unsafe internals, the simplest course of action is to do the
    /// work in the <see cref="Freeze()"/> function. Just make all of your internal
    /// fields frozen, and set the frozen flag. Any subsequent getter/setter will
    /// work properly. Here is an example:
    /// <para/>
    /// <b>Warning!</b> The 'frozen' boolean MUST be volatile, and must be set as the last statement
    /// in the method.
    /// <para/>
    /// <code>
    /// public A Freeze()
    /// {
    ///     if (!frozen)
    ///     {
    ///         foo.Freeze();
    ///         frozen = true;
    ///     }
    ///     return this;
    /// }
    /// </code>
    /// If the field is a collection or dictionary, then to
    /// make it frozen you have two choices. If you have never allowed access to the
    /// collection from outside your object, then just wrap it to prevent future
    /// modification.
    /// <code>
    /// zone_to_country = zone_to_country.ToUnmodifiableDictionary();
    /// </code>
    /// <para/>
    /// If you have <i>ever</i> allowed access, then do a <c>Clone()</c>
    /// before wrapping it.
    /// <code>
    /// zone_to_country = ((IDictionary{string, string})zone_to_country.Clone()).ToUnmodifiableDictionary();
    /// </code>
    /// <para/>
    /// If a collection <i>(or any other container of objects)</i> itself can
    /// contain mutable objects, then for a safe clone you need to recurse through it
    /// to make the entire collection immutable. The recursing code should pick the
    /// most specific collection available, to avoid the necessity of later
    /// downcasing.
    /// <para/>
    /// <b>Note: </b>An annoying flaw in .NET is that the generic collections, like
    /// <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> or <see cref="System.Collections.Generic.HashSet{T}"/>, 
    /// don't have a <c>Clone()</c> operation. When you don't know the type of the collection, the simplest
    /// course is to just create a new collection:
    /// <code>
    /// zone_to_country = new Dictionary(zone_to_country).ToUnmodifiableDictionary();
    /// </code>
    /// </remarks>
    /// <typeparam name="T">Type of item to freeze.</typeparam>
    /// <stable>ICU 3.8</stable>
    public interface IFreezable<T>
#if FEATURE_CLONEABLE
        : ICloneable
#endif
    {
        /// <summary>
        /// Determines whether the object has been frozen or not.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        bool IsFrozen { get; }

        /// <summary>
        /// Freezes the object.
        /// </summary>
        /// <returns>The object itself.</returns>
        /// <stable>ICU 3.8</stable>
        T Freeze();

        /// <summary>
        /// Provides for the clone operation. Any clone is initially unfrozen.
        /// </summary>
        /// <returns>The cloned object.</returns>
        /// <stable>ICU 3.8</stable>
        T CloneAsThawed();
    }
}
