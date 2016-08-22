
//------------------------------------------------------------------------------
// This code was generated by a tool.
//
//   Tool : Bond Compiler 0.4.1.0
//   File : ExceptionData_types.cs
//
// Changes to this file may cause incorrect behavior and will be lost when
// the code is regenerated.
// <auto-generated />
//------------------------------------------------------------------------------


// suppress "Missing XML comment for publicly visible type or member"
#pragma warning disable 1591


#region ReSharper warnings
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantNameQualifier
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameter.Local
// ReSharper disable RedundantUsingDirective
#endregion

namespace Microsoft.ApplicationInsights.Extensibility.Implementation.External
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    
    
    [System.CodeDom.Compiler.GeneratedCode("gbc", "0.4.1.0")]
    internal partial class ExceptionData
        : Domain
    {
        
        
        public int ver { get; set; }

        
        
        public IList<ExceptionDetails> exceptions { get; set; }

        
        
        public SeverityLevel? severityLevel { get; set; }

        
        
        
        public string problemId { get; set; }

        
        
        
        
        public IDictionary<string, string> properties { get; set; }

        
        
        
        public IDictionary<string, double> measurements { get; set; }

        public ExceptionData()
            : this("AI.ExceptionData", "ExceptionData")
        {}

        protected ExceptionData(string fullName, string name)
        {
            ver = 2;
            exceptions = new List<ExceptionDetails>();
            problemId = "";
            properties = new ConcurrentDictionary<string, string>();
            measurements = new ConcurrentDictionary<string, double>();
        }
    }
} // AI












