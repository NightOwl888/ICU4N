using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N.Globalization
{
    internal class ResourceCultureInfo : CultureInfo
    {
        private readonly string cultureName;
        public ResourceCultureInfo(string cultureName)
            // We need something here to prevent this from being considered an invariant culture, although, this isn't used.
            // Not all cultures being passed in this class are considered valid in .NET, so this turns off the validation.
            : base("en-US")
        {
            this.cultureName = cultureName ?? throw new ArgumentNullException(nameof(cultureName));
        }

        public override string Name => cultureName;

        //public override CultureInfo Parent
        //{
        //    get { return InvariantCulture; }
        //}

        public override string ToString()
        {
            return cultureName;
        }
    }
}
