using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Impl.Locale
{
    public class Extension
    {
        private char _key;
        protected string _value;

        protected Extension(char key)
        {
            _key = key;
        }

        internal Extension(char key, string value)
        {
            _key = key;
            _value = value;
        }

        public virtual char Key
        {
            get { return _key; }
        }

        public virtual string Value
        {
            get { return _value; }
        }

        public virtual string GetID() // ICU4N TODO: Make property ?
        {
            return _key + LanguageTag.SEP + _value;
        }

        public override string ToString()
        {
            return GetID();
        }
    }
}
