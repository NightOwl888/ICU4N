namespace ICU4N.Impl.Locale
{
    public class ParseStatus
    {
        private int _parseLength = 0;
        private int _errorIndex = -1;
        private string _errorMsg = null;

        public void Reset()
        {
            _parseLength = 0;
            _errorIndex = -1;
            _errorMsg = null;
        }

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
