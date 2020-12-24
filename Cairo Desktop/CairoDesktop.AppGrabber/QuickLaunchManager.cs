using ManagedShell.WindowsTasks;

namespace CairoDesktop.AppGrabber
{
    public class QuickLaunchManager
    {
        public ApplicationInfo GetQuickLaunchApplicationInfo(ApplicationWindow window)
        {
            // it would be nice to cache this, but need to handle case of user adding/removing app via various means after first access
            foreach (ApplicationInfo ai in AppGrabber.Instance.QuickLaunch)
            {
                if (ai.Target.ToLower() == window.WinFileName.ToLower() || (window.IsUWP && ai.Target == window.AppUserModelID))
                {
                    return ai;
                }
                
                if (window.Title.ToLower().Contains(ai.Name.ToLower()))
                {
                    return ai;
                }
            }

            return null;
        }

        public void PinToQuickLaunch(bool isUWP, string path)
        {
            if (isUWP)
            {
                // store app, do special stuff
                AppGrabber.Instance.AddStoreApp(path, AppCategoryType.QuickLaunch);
            }
            else
            {
                AppGrabber.Instance.AddByPath(new[] { path }, AppCategoryType.QuickLaunch);
            }
        }
    }
}
