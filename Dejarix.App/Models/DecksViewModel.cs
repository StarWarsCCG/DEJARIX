using System;
using System.Collections.Generic;

namespace Dejarix.App.Models
{
    public class DecksViewModel
    {
        public List<KeyValuePair<Guid, string>> Decks { get; } = new List<KeyValuePair<Guid, string>>();
    }
}