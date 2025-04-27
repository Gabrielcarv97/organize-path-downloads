using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

[RunInstaller(true)]
public class ServiceInstallerClass : Installer
{
    public ServiceInstallerClass()
    {
        var processInstaller = new ServiceProcessInstaller
        {
            Account = ServiceAccount.LocalSystem
        };

        var serviceInstaller = new ServiceInstaller
        {
            ServiceName = "OrganizeDownloadService",
            StartType = ServiceStartMode.Automatic
        };

        Installers.Add(processInstaller);
        Installers.Add(serviceInstaller);
    }
}
