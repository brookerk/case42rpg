﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Case42.Base.Abstract
{
    public interface ICommand<TResponse> : ICommand where TResponse : ICommandResponse
    {
    }
}
