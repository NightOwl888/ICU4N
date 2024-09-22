using ICU4N.Text;
using System;
using System.Threading;
#nullable enable

namespace ICU4N.Impl.Locale
{
    public class Extension
    {
        private readonly char key;
        private string? value;
        private string? id;

        protected Extension(char key)
        {
            this.key = key;
        }

        internal Extension(char key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public virtual char Key => key;

        public virtual string? Value
        {
            get => value;
            protected set
            {
                this.value = value;
                this.id = null;
            }
        }

        public virtual string ID => LazyInitializer.EnsureInitialized(ref id,
            () => StringHelper.Concat(stackalloc char[] { key, LanguageTag.Separator }, value.AsSpan()))!;

        public override string ToString()
        {
            return ID;
        }
    }
}
