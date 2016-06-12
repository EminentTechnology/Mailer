using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailer.Abstractions
{
    public enum MailPriority
    {
        //
        // Summary:
        //     The email has normal priority.
        Normal = 0,
        //
        // Summary:
        //     The email has low priority.
        Low = 1,
        //
        // Summary:
        //     The email has high priority.
        High = 2
    }
}
