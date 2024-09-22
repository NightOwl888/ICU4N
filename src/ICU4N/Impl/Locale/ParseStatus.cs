namespace ICU4N.Impl.Locale
{
    public ref struct ParseStatus
    {
        private int _parseLength = 0;
        private int _errorIndex = -1;
        private string _errorMsg = null;

        public ParseStatus()
        {
        }

        // ICU4N: Eliminated Reset() because it is cheap to create a new instance
        public bool IsError => _errorIndex >= 0;

        public int ErrorIndex
        {
            get => _errorIndex;
            internal set => _errorIndex = value;
        }

        public int ParseLength
        {
            get => _parseLength;
            internal set => _parseLength = value;
        }

        public string ErrorMessage
        {
            get => _errorMsg;
            internal set => _errorMsg = value;
        }
    }
}
