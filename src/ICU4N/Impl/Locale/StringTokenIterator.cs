using System;

namespace ICU4N.Impl.Locale
{
    /// <summary>
    /// NOTE: This is equivalent to StringTokenIterator in ICU4J
    /// </summary>
    public class StringTokenEnumerator
    {
        private string _text;
        private string _dlms;

        private string _token;
        private int _start;
        private int _end;
        private bool _done;

        public StringTokenEnumerator(string text, string dlms)
        {
            _text = text;
            _dlms = dlms;
            _start = -1; // .NET semantics - start state is not on an element, so first call to MoveNext() sets us to the start.
        }

        public string First()
        {
            SetStart(0);
            return _token;
        }

        public string Current
        {
            get { return _token; }
        }

        public int CurrentStart
        {
            get { return _start; }
        }

        public int CurrentEnd
        {
            get { return _end; }
        }

        public bool IsDone
        {
            get { return _done; }
        }

        private string Next()
        {
            if (HasNext)
            {
                _start = _end + 1;
                _end = NextDelimiter(_start);
                _token = _text.Substring(_start, _end - _start); // ICU4N: Corrected 2nd parameter
            }
            else
            {
                _start = _end;
                _token = null;
                _done = true;
            }
            return _token;
        }

        // ICU4N specific enumerator method.
        public bool MoveNext()
        {
            // ICU4N: We initially set the start to -1 to indicate "before bounds" state
            // in .NET. So, we need to check here and move to the first position if that
            // is the case.
            if (_start < 0)
                SetStart(0);
            else
                Next();
            return !_done;
        }

        public bool HasNext
        {
            get { return (_end < _text.Length); }
        }

        public StringTokenEnumerator SetStart(int offset)
        {
            if (offset > _text.Length)
            {
                throw new IndexOutOfRangeException();
            }
            _start = offset;
            _end = NextDelimiter(_start);
            _token = _text.Substring(_start, _end - _start); // ICU4N: Corrected 2nd parameter
            _done = false;
            return this;
        }

        public StringTokenEnumerator SetText(string text)
        {
            _text = text;
            SetStart(0);
            return this;
        }

        private int NextDelimiter(int start)
        {
            int idx = start;
            //outer:
            while (idx < _text.Length)
            {
                char c = _text[idx];
                for (int i = 0; i < _dlms.Length; i++)
                {
                    if (c == _dlms[i])
                    {
                        goto outer_break;
                    }
                }
                idx++;
            }
            outer_break: { }
            return idx;
        }
    }
}
