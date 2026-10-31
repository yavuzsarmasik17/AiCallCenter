using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiCallCenter.Domain.Enums;

public enum CallStatus
{
    Created = 1,
    Ringing = 2,
    InProgress = 3,
    Completed = 4,
    Failed = 5,
    Cancelled = 6,
    Transferred = 7
}
