using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atmel.Studio.Services;
using Atmel.Studio.Services.Device;
using Atmel.VsIde.AvrStudio.Services.TargetService;
using Atmel.VsIde.AvrStudio.Services.TargetService.TCF.Internal.Services.Remote;
using Atmel.VsIde.AvrStudio.Services.TargetService.TCF.Services;

namespace Company.DataViewer.ExpressionEvaluator
{
    public class ExpressionEvaluationWrapper
    {
        private ITargetService _targetService;
        public string GetVaraibleValue(string variable)
        {
            var target = GetTarget();
            string value = string.Empty;
            if (target != null)
            {
                List<StackTraceInfo> stackFrames = target.GetCurrentStacks();
                ValueInfo adrValue;
                SymbolInfo symbolInfo;
                ExpressionInfo exprInfo;
                IStatus status;
                bool computed = target.ExpressionsProxy.Compute(stackFrames[0].ID, "C", variable, out exprInfo,
                                                                out adrValue, out symbolInfo, out status);
                if (computed && status == null)
                {
                    string type = string.Empty;
                    ExpressionsProxy.StringProcurorDelegate stringProcurorDelegate = StringProcurorDelegate;
                    value = ExpressionsProxy.GetExpressionValueAsString(target, stringProcurorDelegate, exprInfo,
                                                                        symbolInfo, adrValue, 10,
                                                                        ref exprInfo.CanAssign, ref type);
                }
            }
            return value;
        }

        public byte[] GetValueAtAddress(ulong startAddress,int byteCount)
        {
            var target2 = GetTarget2();
            byte[] value = null;
            var addressSpace = GetAddressSpace(target2);
            if (target2 != null)
            {
                MemoryErrorRange[] memoryErrorRange;
                value = target2.GetMemory(addressSpace, startAddress, 1, byteCount, 0,
                                         out memoryErrorRange);
            }
            return value;
        }

        private string GetAddressSpace(ITarget2 target2)
        {
           string addressSpace;
            if (target2.Device.Name.ToLower().Contains("sam"))
            {
                addressSpace = target2.GetAddressSpaceName(MemoryTypes.Base);
            }
            else
            {
                 addressSpace = target2.GetAddressSpaceName(MemoryTypes.Data);  
            }
           
            return addressSpace;
        }

        private Target GetTarget()
        {
            _targetService = ATServiceProvider.TargetService2 as ITargetService;
            if (_targetService != null)
            {
                return _targetService.GetLaunchedTarget();
            }
            return null;
        }

        private ITarget2 GetTarget2()
        {
            var targetService = ATServiceProvider.TargetService2;
            if (targetService != null)
            {
                return targetService.GetLaunchedTarget();
            }
            return null;
        }

        private string StringProcurorDelegate()
        {
            return string.Empty;
        }
    }
}
