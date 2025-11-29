using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT
{
    public class RepositoryInfo
    {
        public string OwnerName { get; set; } = "";

        public string RepositoryName { get; set; } = "";
        public RepositoryInfo()
        {

        }

        public RepositoryInfo(string ownerName, string repositoryName)
        {
            OwnerName = ownerName;
            RepositoryName = repositoryName;
        }

    }

    public class GithubUtils
    {

        public static RepositoryInfo GetCurrentRepositoryInfo(string PackageName)
        {

            RepositoryInfo repositoryInfo = new RepositoryInfo();
            repositoryInfo.RepositoryName = PackageName;

            if (PackageName == MigotoPackageName.GIMIPackage)
            {
                repositoryInfo.OwnerName = "SilentNightSound";
            }
            else if (PackageName == MigotoPackageName.SRMIPackage)
            {
                repositoryInfo.OwnerName = "SpectrumQT";
            }
            else if (PackageName == MigotoPackageName.ZZMIPackage)
            {
                repositoryInfo.OwnerName = "leotorrez";
            }
            else if (PackageName == MigotoPackageName.HIMIPackage)
            {
                repositoryInfo.OwnerName = "leotorrez";
            }
            else if (PackageName == MigotoPackageName.WWMIPackage)
            {
                repositoryInfo.OwnerName = "SpectrumQT";
            }
            else if (PackageName == MigotoPackageName.NBPPackage)
            {
                repositoryInfo.OwnerName = "StarBobis";
            }
            else if (PackageName == MigotoPackageName.MinBasePackage)
            {
                repositoryInfo.OwnerName = "StarBobis";
            }

            return repositoryInfo;
        }
    }
}
