using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager
{
    public class BlackListItem
    {
        public string Group;
        public string Value;
        public BlackListItem(string group, string value)
        {
            Group = group;
            Value = value;
        }
    }
}
