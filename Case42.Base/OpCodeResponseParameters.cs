﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Case42.Base
{
    public enum Case42OpCodeResponseParameter : byte
    {
        CommandResponse,
        OperationErrors,
        PropertyErrors,
        CommandId
    }
}
