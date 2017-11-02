using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Util
{
    public interface IFreezable<T>
#if FEATURE_CLONEABLE
        : ICloneable
#endif
    {
        /**
         * Determines whether the object has been frozen or not.
         * @stable ICU 3.8
         */
        bool IsFrozen { get; }

        /**
         * Freezes the object.
         * @return the object itself.
         * @stable ICU 3.8
         */
        T Freeze();

        /**
         * Provides for the clone operation. Any clone is initially unfrozen.
         * @stable ICU 3.8
         */
        T CloneAsThawed();
    }
}
