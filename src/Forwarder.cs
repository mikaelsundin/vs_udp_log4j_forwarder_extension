using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Log4JForwardExtension;



/// <summary>
/// Singleton class handling hooking into debug output window
/// </summary>
internal class Forwarder
{
    private AsyncPackage _package;

    Action<string> _debugLineHandler = null;
    private System.Timers.Timer _checkTimer = new();
    static SemaphoreSlim _checkSemaphore = new SemaphoreSlim(1, 1);
    private IWpfTextView _textView;





    /// <summary>
    /// Current instance
    /// </summary>
    public static Forwarder Instance { get; private set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="package"></param>
    private Forwarder(AsyncPackage package)
    {
        this._package = package;

        this._checkTimer.Interval = 1000;
        this._checkTimer.AutoReset = false;
        this._checkTimer.Elapsed += TryInstall;
    }

    /// <summary>
    /// Try to install the text listener.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void TryInstall(object sender, System.Timers.ElapsedEventArgs e)
    {
        bool sucess = false;

        //Try to install plugin.
        try
        {
            await _checkSemaphore.WaitAsync();
            
            //Ensure we are on UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            //Try to install the listener
            sucess = Install();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CheckTimer_Elapsed {}", ex);
        }
        finally
        {
            _checkSemaphore.Release();
            
        }

        //Restart timer on fail.
        if (!sucess)
        {
            _checkTimer.Start();
        }
    }
#pragma warning restore VSTHRD100 // Avoid async void methods


    /// <summary>
    /// Get the Debug Panel
    /// </summary>
    /// <returns></returns>
    IVsOutputWindowPane GetDebugPanel()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

        //windows GUID_BuildOutputWindowPane / GUID_OutWindowDebugPane
        Guid paneGuid = VSConstants.GUID_OutWindowDebugPane;
        IVsOutputWindowPane debugPane;
        outputWindow.GetPane(ref paneGuid, out debugPane);

        return debugPane;
    }

    /// <summary>
    /// Get the Debug output 
    /// </summary>
    /// <returns></returns>
    private IWpfTextView GetDebugOutputTextView()
    {
        var debugPanel = GetDebugPanel();

        //IVsUserData = Allows a caller to use a GUID to set or get user data (properties).
        IVsUserData userData = debugPanel as IVsUserData;
        if (userData == null)
        {
            return null;
        }

        //Get the TextView Host
        object viewHost;
        Guid guidViewHost = DefGuidList.guidIWpfTextViewHost;
        userData.GetData(ref guidViewHost, out viewHost);

        ///Get the TextView
        IWpfTextViewHost textViewHost = viewHost as IWpfTextViewHost;
        return textViewHost.TextView;
    }


    private bool Install()
    {
        try
        {
            IVsOutputWindow outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            var textView = GetDebugOutputTextView();
            if (textView == null)
            {
                return false;
            }
            _textView = textView;


            //Wire up eventhandler
            _textView.TextBuffer.Changed += new EventHandler<Microsoft.VisualStudio.Text.TextContentChangedEventArgs>(DebugOutput_Changed);

            //Success
            return true;
        }
        catch (Exception ex) 
        {
            Debug.WriteLine("Install ex: {}", ex);
            return false;
        }

    }

    private void DebugOutput_Changed(object sender, TextContentChangedEventArgs e)
    {
        foreach (var change in e.Changes)
        {
            string text = change.NewText.ToString();

            foreach(var line in text.Split('\n'))
            {
                if(_debugLineHandler != null)
                {
                    _debugLineHandler(line);
                }
            }
        }
    }


    public static void Initialize(AsyncPackage package, Action<string> debugLineHandler)
    {
        Instance = new Forwarder(package);
        Instance._checkTimer.Start();
        Instance._debugLineHandler = debugLineHandler;

    }


}
