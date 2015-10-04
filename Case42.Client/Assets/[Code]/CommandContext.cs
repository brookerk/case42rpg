﻿using System.Collections.Generic;
using System.Linq;

namespace Assets.Code
{
	public class CommandContext
	{
        public IDictionary<string, IEnumerable<string> > PropertyErrors { get; private set;}
        public IEnumerable<string> OperationErrors { get; private set;}

        public bool IsValid { get; private set; }

        public CommandContext(IDictionary<string,IEnumerable<string>> propertyErrors, IEnumerable<string> operationErrors)
        {
            PropertyErrors = propertyErrors;
            OperationErrors = operationErrors;
            IsValid = !(operationErrors.Any() || propertyErrors.Any(t => t.Value.Any()));
        }

        public string ToErrorString()
        {
            //using toArray bcos Unity Net3.5 doesnt take in Ienumerable string
            return string.Format(
                "{0}\n{1}",
                string.Concat(OperationErrors.ToArray()),
                string.Join( "\n",
                    PropertyErrors.Select(t => string.Format(
                        "{0} - {1}", 
                        t.Key,
                        string.Join(", ",t.Value.ToArray()))).ToArray()
                )
                );
        }
	}

}
