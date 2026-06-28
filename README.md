# 🚀 WLauncher

![WLauncher Banner](https://i.pinimg.com/originals/df/64/a0/df64a03a777dc9f9a060ef6b286773b3.gif)

**WLauncher** is an automated client designed for the installation, management, and updating of game decompilation projects (PC Ports/Recomps). 

## ⚙️ Key Features

*   **Automated Updates:** Both the client and the installed projects update autonomously in the background via the GitHub API, ensuring you always have the latest release without the need for manual patching.
*   **Modular Support (Custom Recomps):** An open and customizable architecture. Users can add, link, and manage their own decompilations by configuring local repositories.
*   **Legal Environment Management:** WLauncher *strictly* distributes engine binaries and patches. The system requires the user to provide their own legally obtained game assets or ROMs locally to function.
*   **Integrity Verification:** Scans local directories in real-time to ensure the executable binary and the extracted game assets are perfectly synchronized prior to execution.

## 📋 Installation & Usage

1. Download the latest release from the [Releases](../../releases) page.
2. Place the executable in a dedicated folder where you wish to manage your recomp projects.
3. Run `WLauncher.exe` (Running as Administrator is recommended on the first launch to prevent Windows permission issues during folder creation).
4. Provide your legal game assets/ROMs as instructed by the specific recomp project.
5. Allow the launcher to download the required base binaries and click 'Play'.

## ⚠️ Technical Note: Antivirus False Positives

Because WLauncher automatically downloads and executes binaries (`.exe`) directly from GitHub repositories, Windows Defender or your antivirus software may flag it as a potential threat. This is a standard **false positive** common among open-source launchers and auto-updaters. 

To ensure proper functionality, please add the WLauncher executable or its parent folder to your antivirus exclusions list.
