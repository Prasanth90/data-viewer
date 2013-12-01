using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Company.DataViewer.Utils
{
    public class DataViewerUtils
    {
        public static void ShowDataViewerSettingsOptionsPage()
        {
            try
            {
                object settingsPageGuid = "D2AE3A81-79E5-4D1D-B6DC-29CA310A8792";
                Guid tooldGroupGuid = VSConstants.GUID_VSStandardCommandSet97;

                var shell = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;

                if (shell != null)
                    shell.PostExecCommand(ref tooldGroupGuid, VSConstants.cmdidToolsOptions,
                                          0, ref settingsPageGuid);

            }
            catch (Exception) { }
        }
    }
}
