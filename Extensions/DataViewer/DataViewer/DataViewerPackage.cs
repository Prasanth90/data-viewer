﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Atmel.Studio.Services;
using Atmel.Studio.Services.Device;
using Company.DataViewer.Options;
using Company.DataViewer.Utils;
using EnvDTE;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace Company.DataViewer
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(DataViewerOptionsPage), "Extensions", "Data Viewer",0, 0, true)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(MyToolWindow))]
    [ProvideAutoLoad(UIContextGuids.NoSolution)]
    [Guid(GuidList.guidDataViewerPkgString)]
    public sealed class DataViewerPackage : Package
    {


        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public DataViewerPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }


        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(currentDomain_AssemblyResolve);

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                var optionsPage = GetOptionsPage();
                var graphSettings = new GraphSettings();
                Settings.GraphSettings = graphSettings;
                Settings.GraphSettings.ApplySettings(optionsPage);

                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidDataViewerCmdSet, (int)PkgCmdIDList.cmdidMyToolWindow);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand( menuToolWin );


                //Options
                var menuOptionsCmd = new CommandID(GuidList.guidDataViewerCmdSet,
                                                  (int)PkgCmdIDList.cmdidOptions);
                var menuOptionsCommand = new OleMenuCommand(OpenOptions, menuOptionsCmd) { Checked = false };
                mcs.AddCommand(menuOptionsCommand);

                //Help
                var menuHelpCmd = new CommandID(GuidList.guidDataViewerCmdSet,(int)PkgCmdIDList.cmdidHelp);
                var menuHelpCommand = new OleMenuCommand(RunHelp, menuHelpCmd) { Checked = false };
                mcs.AddCommand(menuHelpCommand);


                //delete all
                var menuDeleteAllCmd = new CommandID(GuidList.guidDataViewerCmdSet,(int)PkgCmdIDList.cmdidRemoveAll);
                var menuDeleteAllCommand = new OleMenuCommand(WatchDeleteAll, menuDeleteAllCmd) { Checked = false };
                mcs.AddCommand(menuDeleteAllCommand);
                
            }
        }

        System.Reflection.Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly myAssembly = null;
            string reflectionOnlyAssemblyName = "";

            if (args.Name.Contains("C1.WPF.C1Chart.4.dll"))
            {
                reflectionOnlyAssemblyName = "C1.WPF.C1Chart.4.dll";
            }

            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var reflectionOnlyAssemblypath = Path.Combine(dir, reflectionOnlyAssemblyName);
            if (File.Exists(reflectionOnlyAssemblypath))
            {
                //Load the assembly from the specified path. 					
                myAssembly = Assembly.LoadFrom(reflectionOnlyAssemblypath);
            }

            return myAssembly;
        }

        private void WatchDeleteAll(object sender, EventArgs e)
        {

        }

        private void RunHelp(object sender, EventArgs e)
        {
            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var helpFilePath = Path.Combine(dir, "Resources\\DataViewerHelp.pdf");
            if (File.Exists(helpFilePath))
            {

                System.Diagnostics.Process.Start(helpFilePath);
            }   
        }

        private void OpenOptions(object sender, EventArgs e)
        {
            DataViewerUtils.ShowDataViewerSettingsOptionsPage();
        }

        #endregion

        private DataViewerOptionsPage GetOptionsPage()
        {
            var dataviewerOptionsPage = (DataViewerOptionsPage)GetDialogPage(typeof(DataViewerOptionsPage));
            if (dataviewerOptionsPage == null) return null;
            return dataviewerOptionsPage;
        }

    }
}
