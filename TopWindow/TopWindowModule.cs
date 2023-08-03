using System;
using System.ComponentModel.Composition;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;

namespace TopWindow
{
    [Export(typeof(Module))]
    public class TopWindowModule : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<TopWindowModule>();

        internal static TopWindowModule TopWindowModuleInstance;

        private SettingEntry<string> _windowName;
        private IntPtr _currentWindowHandle = IntPtr.Zero;
        private double _runningTime;

        [ImportingConstructor]
        public TopWindowModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            TopWindowModuleInstance = this;
        }

        // Define the settings you would like to use in your module.  Settings are persistent
        // between updates to both Blish HUD and your module.
        protected override void DefineSettings(SettingCollection settings)
        {
            _windowName = settings.DefineSetting(
                "window_name",
                "Picture-in-Picture",
                () => "Window Name",
                () => "Window name to apply on-top and unclickable to");
        }

        // Allows your module to perform any initialization it needs before starting to run.
        // Please note that Initialize is NOT asynchronous and will block Blish HUD's update
        // and render loop, so be sure to not do anything here that takes too long.
        protected override void Initialize()
        {
        }

        // Load content and more here. This call is asynchronous, so it is a good time to run
        // any long running steps for your module including loading resources from file or ref.
        protected override Task LoadAsync()
        {
            return Task.CompletedTask;
        }

        // Allows you to perform an action once your module has finished loading (once
        // <see cref="LoadAsync"/> has completed).  You must call "base.OnModuleLoaded(e)" at the
        // end for the <see cref="ExampleModule.ModuleLoaded"/> event to fire.
        //protected override void OnModuleLoaded(EventArgs e)
        //{
        //    // Base handler must be called
        //    base.OnModuleLoaded(e);
        //}

        // Allows your module to run logic such as updating UI elements,
        // checking for conditions, playing audio, calculating changes, etc.
        // This method will block the primary Blish HUD loop, so any long
        // running tasks should be executed on a separate thread to prevent
        // slowing down the overlay.
        protected override void Update(GameTime gameTime)
        {
            //Only run every 200ms
            _runningTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_runningTime < 200)
            {
                return;
            }
            _runningTime = 0;

            //Find the window
            var windowName = _windowName.Value;
            var windowHandle = _currentWindowHandle;

            //We dont have an active window, find it
            if (windowHandle == IntPtr.Zero || !NativeMethods.IsWindow(windowHandle))
            {
                NativeMethods.EnumWindows(delegate (IntPtr wnd, IntPtr param)
                {
                    var windowText = GetWindowText(wnd);
                    //Logger.Trace(() => $"WindowText: {windowText}");
                    if (windowText == windowName)
                    {
                        windowHandle = wnd;
                        return false;
                    }

                    // return true here so that we iterate all windows
                    return true;
                }, IntPtr.Zero);


                //Didn't find it
                if (windowHandle == IntPtr.Zero)
                {
                    _currentWindowHandle = IntPtr.Zero;
                    return;
                }
                _currentWindowHandle = windowHandle;
            }

            var extendedStyle = NativeMethods.GetWindowLong(windowHandle, NativeMethods.GWL_EXSTYLE);
            if (extendedStyle == 0)
            {
                Logger.Error($"Failed getting extendedStyle for {windowName}");
                return;
            }

            //Already set
            if ((extendedStyle & NativeMethods.WS_EX_TRANSPARENT) == NativeMethods.WS_EX_TRANSPARENT
                && (extendedStyle & NativeMethods.WS_EX_LAYERED) == NativeMethods.WS_EX_LAYERED)
            {
                //Disable if unfocused
                if (!GameService.GameIntegration.Gw2Instance.Gw2HasFocus)
                {
                    //Logger.Info($"Un-setting mouse transparency on {windowName}");
                    var result = NativeMethods.SetWindowLong(windowHandle, NativeMethods.GWL_EXSTYLE,
                        extendedStyle & ~NativeMethods.WS_EX_TRANSPARENT & ~NativeMethods.WS_EX_LAYERED);
                    if (result == 0)
                    {
                        Logger.Error($"Failed un-setting extendedStyle for {windowName}");
                    }
                }
            }
            else //Not set
            {
                //Enable if in focus
                if (GameService.GameIntegration.Gw2Instance.Gw2HasFocus)
                {
                    //Logger.Info($"Setting mouse transparency on {windowName}");
                    var result = NativeMethods.SetWindowLong(windowHandle, NativeMethods.GWL_EXSTYLE,
                        extendedStyle | NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_LAYERED);
                    if (result == 0)
                    {
                        Logger.Error($"Failed setting extendedStyle for {windowName}");
                    }
                }
            }

        }

        // For a good module experience, your module should clean up ANY and ALL entities
        // and controls that were created and added to either the World or SpriteScreen.
        // Be sure to remove any tabs added to the Director window, CornerIcons, etc.
        protected override void Unload()
        {
            //Reset window to old style
            if (NativeMethods.IsWindow(_currentWindowHandle))
            {
                var extendedStyle = NativeMethods.GetWindowLong(_currentWindowHandle, NativeMethods.GWL_EXSTYLE);
                if (extendedStyle != 0)
                {
                    NativeMethods.SetWindowLong(_currentWindowHandle, NativeMethods.GWL_EXSTYLE,
                        extendedStyle & ~NativeMethods.WS_EX_TRANSPARENT & ~NativeMethods.WS_EX_LAYERED);
                }
            }

            _currentWindowHandle = IntPtr.Zero;

            // All static members must be manually unset
            // Static members are not automatically cleared and will keep a reference to your,
            // module unless manually unset.
            TopWindowModuleInstance = null;
        }

        private static string GetWindowText(IntPtr hWnd)
        {
            var size = NativeMethods.GetWindowTextLength(hWnd);
            if (size <= 0)
            {
                return string.Empty;
            }
            var builder = new StringBuilder(size + 1);
            NativeMethods.GetWindowText(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }

    }
}