using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace VLCNP.Editor
{
    /** プロジェクト内のOFLフォントの著作権表示とライセンスを配布物に同梱する。 */
    public sealed class FontLicenseBuildProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            CopyLicenses(report.summary.platform, report.summary.outputPath);
        }

        private static void CopyLicenses(BuildTarget target, string outputPath)
        {
            string destinationRoot;
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    destinationRoot = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(outputPath)), "Licenses");
                    break;
                case BuildTarget.StandaloneOSX:
                    destinationRoot = Path.Combine(Path.GetFullPath(outputPath), "Contents", "Resources", "Licenses");
                    break;
                default:
                    return;
            }

            string fontsRoot = Path.Combine(Application.dataPath, "Game", "Fonts");
            foreach (string source in Directory.GetFiles(fontsRoot, "OFL.txt", SearchOption.AllDirectories))
            {
                string destination = Path.Combine(destinationRoot, source.Substring(fontsRoot.Length + 1));
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.Copy(source, destination, true);
            }
        }
    }
}
