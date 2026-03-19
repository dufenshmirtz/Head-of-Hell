using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class ProfileAnalysisRunner_PlugAndPlay : MonoBehaviour
{
    [Header("Python")]
    public string pythonExe = "python";

    [Header("Pipeline")]
    public string pipelineScript = "pipeline_runner.py";

    [Header("UI")]
    public ProfileAnalysisPanelUI panelUI;

    public void RunPipelineAndRefresh()
    {
        try
        {
            string workingDir = Path.Combine(Application.streamingAssetsPath, "Analysis");
            string scriptPath = Path.Combine(workingDir, pipelineScript);

            if (!File.Exists(scriptPath))
                throw new FileNotFoundException("Pipeline script not found: " + scriptPath);

            string telemetryDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "My Games",
                "Head of Hell",
                "Telemetry"
             );

            string unityJsonPath = Path.Combine(
                Application.persistentDataPath,
                "ProfileAnalysis",
                "profile_analysis.json"
            );

            string unityJsonDir = Path.GetDirectoryName(unityJsonPath);
            if (!string.IsNullOrWhiteSpace(unityJsonDir))
                Directory.CreateDirectory(unityJsonDir);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments =
                     $"\"{scriptPath}\" " +
                     $"--telemetry-dir \"{telemetryDir}\"",
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = psi;
                process.Start();

                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();

                process.WaitForExit();

                UnityEngine.Debug.Log($"[pipeline_runner] STDOUT:\n{stdout}");

                if (!string.IsNullOrWhiteSpace(stderr))
                    UnityEngine.Debug.LogWarning($"[pipeline_runner] STDERR:\n{stderr}");

                if (process.ExitCode != 0)
                    throw new Exception("Pipeline failed with exit code " + process.ExitCode);
            }

            // Run export_profile_analysis.py separately so Unity controls the final output path
            string exportScriptPath = Path.Combine(workingDir, "export_profile_analysis.py");
            if (!File.Exists(exportScriptPath))
                throw new FileNotFoundException("Export script not found: " + exportScriptPath);
            UnityEngine.Debug.Log("EXPORT unityJsonPath = " + unityJsonPath);
            UnityEngine.Debug.Log("EXPORT script = " + exportScriptPath);
            ProcessStartInfo exportPsi = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments =
                    $"\"{exportScriptPath}\" " +
                    $"--input \"{Path.Combine(workingDir, "out", "dataset_profile_level.csv")}\" " +
                    $"--elo-input \"{Path.Combine(workingDir, "out_elo", "elo_overall.csv")}\" " +
                    $"--output-dir \"{Path.Combine(workingDir, "out", "profile_analysis")}\" " +
                    $"--unity-output \"{unityJsonPath}\"",
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process exportProcess = new Process())
            {
                exportProcess.StartInfo = exportPsi;
                UnityEngine.Debug.Log("EXPORT args FULL = " + exportPsi.Arguments);
                exportProcess.Start();

                string stdout = exportProcess.StandardOutput.ReadToEnd();
                string stderr = exportProcess.StandardError.ReadToEnd();

                exportProcess.WaitForExit();
                UnityEngine.Debug.Log("UNITY JSON EXISTS AFTER EXPORT = " + File.Exists(unityJsonPath));
                UnityEngine.Debug.Log("UNITY JSON PATH = " + unityJsonPath);

                if (File.Exists(unityJsonPath))
                {
                    UnityEngine.Debug.Log("UNITY JSON LAST WRITE = " + File.GetLastWriteTime(unityJsonPath));
                }
                UnityEngine.Debug.Log($"[export_profile_analysis] STDOUT:\n{stdout}");

                if (!string.IsNullOrWhiteSpace(stderr))
                    UnityEngine.Debug.LogWarning($"[export_profile_analysis] STDERR:\n{stderr}");

                if (exportProcess.ExitCode != 0)
                    throw new Exception("export_profile_analysis failed with exit code " + exportProcess.ExitCode);
            }

            if (panelUI != null)
                panelUI.RefreshAnalysis();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Plug-and-play pipeline failed: " + ex.Message);
        }
    }
}