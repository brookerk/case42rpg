﻿using System.Collections.Generic;
using System;
using System.Linq;
using Case42.Base.Abstract;

namespace Case42.Server
{
    public class CommandContext
    {
        private readonly Dictionary<string, List<string>> _propertyErrors;
        private readonly List<string> _operationErrors;

        public ICommandResponse Response { get; private set; }
        public bool IsValid { get { return !(_operationErrors.Any() || _propertyErrors.Any(t => t.Value.Any())); } }

        public IEnumerable<string> OperationErrors { get { return _operationErrors; } }
        public IDictionary<string, List<string>> PropertyErrors { get { return _propertyErrors; } }


        public CommandContext()
        {
            _propertyErrors = new Dictionary<string, List<string>>(StringComparer.CurrentCultureIgnoreCase);
            _operationErrors = new List<string>();
        }

        public void RaisePropertyError(string name, string message)
        {
            GetPropertyErrors(name).Add(message);
        }

        private List<string> GetPropertyErrors(string property)
        {
            List<string> propertyErrors;
            return _propertyErrors.TryGetValue(property, out propertyErrors)
                ? propertyErrors
                : _propertyErrors[property] = new List<string>();
        }

        public void RaiseOperationError(string message)
        {
            _operationErrors.Add(message);
        }

        public void SetResponse(ICommandResponse response)
        {
            Response = response;
        }
    }
}