using System;
using System.Collections.Generic;
using System.Text;

namespace Dejarix.App
{
    public class QueryBuilder
    {
        private readonly List<string> _select = new List<string>();
        private readonly List<string> _specify = new List<string>();
        private readonly List<string> _where = new List<string>();
        private readonly string _from;

        public QueryBuilder(string from)
        {
            _from = from;
        }

        public void Specify(params string[] items) => _specify.AddRange(items);
        public void Select(params string[] items) => _select.AddRange(items);
        public void Select<T>(Converter<T, string> converter, params T[] items)
        {
            foreach (var item in items)
                _select.Add(converter(item));
        }

        public void Where(params string[] items) => _where.AddRange(items);

        public override string ToString()
        {
            var builder = new StringBuilder("SELECT ");

            foreach (var specify in _specify)
                builder.Append(specify).Append(' ');
            
            if (_select.Count > 0)
            {
                builder.Append(_select[0]);

                for (int i = 1; i < _select.Count; ++i)
                    builder.Append(", ").Append(_select[i]);
            }
            else
            {
                builder.Append('*');
            }

            builder.Append(" FROM ").Append(_from);

            if (_where.Count > 0)
            {
                builder.Append(" WHERE (").Append(_where[0]);

                for (int i = 1; i < _where.Count; ++i)
                    builder.Append(") AND (").Append(_where[i]);
                
                builder.Append(')');
            }

            return builder.ToString();
        }
    }
}