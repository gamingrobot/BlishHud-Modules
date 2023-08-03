using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;

namespace CursorLock
{
    [Export(typeof(Module))]
    public class CursorLockModule : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<CursorLockModule>();

        internal static CursorLockModule CursorLockModuleInstance;

        [ImportingConstructor]
        public CursorLockModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            CursorLockModuleInstance = this;
        }

        // Define the settings you would like to use in your module.  Settings are persistent
        // between updates to both Blish HUD and your module.
        protected override void DefineSettings(SettingCollection settings)
        {
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
            if (GameService.GameIntegration.Gw2Instance.Gw2HasFocus)
            {
                var handle = GameService.GameIntegration.Gw2Instance.Gw2WindowHandle;
                if (handle == IntPtr.Zero)
                {
                    return;
                }

                //Clip/Restrict the cursor to the gw2 window
                NativeMethods.GetWindowRect(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle,
                    out var appBounds);
                NativeMethods.ClipCursor(ref appBounds);
            }
            else
            {
                //Unclip the cursor
                NativeMethods.ClipCursor(IntPtr.Zero);
            }
        }

        // For a good module experience, your module should clean up ANY and ALL entities
        // and controls that were created and added to either the World or SpriteScreen.
        // Be sure to remove any tabs added to the Director window, CornerIcons, etc.
        protected override void Unload()
        {

            //Unclip the Cursor
            NativeMethods.ClipCursor(IntPtr.Zero);

            // All static members must be manually unset
            // Static members are not automatically cleared and will keep a reference to your,
            // module unless manually unset.
            CursorLockModuleInstance = null;
        }
    }
}