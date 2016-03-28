using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Service
{
    // Provide the ProjectInstaller class which allows the service to be installed by the
    // Installutil.exe tool
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private ServiceProcessInstaller ProcessInst;
        private ServiceInstaller ServiceInst;

        public ProjectInstaller()
        {
            this.ProcessInst = new ServiceProcessInstaller();
            this.ProcessInst.Account = ServiceAccount.LocalSystem;

            this.ServiceInst = new ServiceInstaller();
            this.ServiceInst.ServiceName = Consts.WindowsServiceTitle;

            base.Installers.Add(ProcessInst);
            base.Installers.Add(ServiceInst);
        }
    }
}