using System;
using System.ComponentModel.Composition;
using SimpleCalculator3;

namespace ExtendedOperations
{
    [Export(typeof(SimpleCalculator3.IOperation))]
    [ExportMetadata("Symbol", '%')]
    public class Mod : SimpleCalculator3.IOperation
    {
        private readonly ILogger _log;

        public Mod(ILogger log)
        {
            _log = log;
        }

        public int Operate(int left, int right)
        {
            _log.Log("Mod.Operate(). Entry...");
            try
            {
                return left % right;
            }
            finally
            {
                _log.Log("Mod.Operate(). Exit...");
            }
        }
    }
}
