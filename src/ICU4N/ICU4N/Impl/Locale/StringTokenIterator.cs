using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Impl.Locale
{
    public class StringTokenIterator
    {
        private string _text;
        private string _dlms;

        private string _token;
        private int _start;
        private int _end;
        private bool _done;

        public StringTokenIterator(string text, string dlms)
        {
            _text = text;
            _dlms = dlms;
            SetStart(0);
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

        public string Next()
        {
            if (HasNext())
            {
                _start = _end + 1;
                _end = NextDelimiter(_start);
                _token = _text.Substring(_start, _end - _start);
            }
            else
            {
                _start = _end;
                _token = null;
                _done = true;
            }
            return _token;
        }

        public bool HasNext()
        {
            return (_end < _text.Length);
        }

        public StringTokenIterator SetStart(int offset)
        {
            if (offset > _text.Length)
            {
                throw new IndexOutOfRangeException();
            }
            _start = offset;
            _end = NextDelimiter(_start);
            _token = _text.Substring(_start, _end - _start);
            _done = false;
            return this;
        }

        public StringTokenIterator SetText(string text)
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
