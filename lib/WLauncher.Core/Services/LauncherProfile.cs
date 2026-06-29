namespace WLauncher.Core.Services
{
    public class LauncherProfile
    {
        public static LauncherProfile Default { get; } = new();

        public virtual string DisplayName => "WLauncher";
        public virtual string ApplicationId => "WLauncher";
        public virtual string Repository => "Spicywonda/WLauncher";
        public virtual string ExecutableName => "WLauncher";
        public virtual string DefaultInstallFolderName => "Apps";
        public virtual string UserAgent => "WLauncher/1.0";
        public virtual string CliUserAgent => "WLauncher-CLI";
        public virtual string UpdaterUserAgent => "WLauncher-Updater";
        public virtual string SteamTag => DisplayName;
        public virtual void ConfigureInstalledApp(string appPath)
        {
        }
    }
}
