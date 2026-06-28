using WLauncher.Core.Services;

namespace WLauncher.Services
{
    public sealed class WLauncherProfile : LauncherProfile
    {
        public static WLauncherProfile Instance { get; } = new();

        public override string DisplayName => "WLauncher";
        public override string ApplicationId => "WLauncher";
        public override string Repository => "SirDiabo/WLauncher";
        public override string ExecutableName => "WLauncher";
        public override string DefaultInstallFolderName => "Apps";
        public override string UserAgent => "WLauncher/1.0";
        public override string CliUserAgent => "WLauncher-CLI";
        public override string UpdaterUserAgent => "WLauncher-Updater";
        public override string SteamTag => "WLauncher";
    }
}
