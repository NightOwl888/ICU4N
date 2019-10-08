namespace ICU4N.Impl.Locale
{
    public class Extension
    {
        private char key;
        protected string m_value;

        protected Extension(char key)
        {
            this.key = key;
        }

        internal Extension(char key, string value)
        {
            this.key = key;
            this.m_value = value;
        }

        public virtual char Key
        {
            get { return key; }
        }

        public virtual string Value
        {
            get { return m_value; }
        }

        public virtual string GetID() // ICU4N TODO: Make property ?
        {
            return key + LanguageTag.Separator + m_value;
        }

        public override string ToString()
        {
            return GetID();
        }
    }
}
