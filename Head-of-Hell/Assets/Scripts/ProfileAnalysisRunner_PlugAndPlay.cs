using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class ProfileAnalysisRunner_PlugAndPlay : MonoBehaviour
{
    [Header("Pipeline EXE")]
    public string exeName = "runtime_profile_pipeline.exe";

    [Header("UI")]
    public ProfileAnalysisPanelUI panelUI;

    [Header("Safety")]
    public int processTimeoutMs = 120000; // 120 sec

    public void RunPipelineAndRefresh()
    {
        try
        {
            string exePath = Path.Combine(
                Application.streamingAssetsPath,
                "Analysis",
                "Bin",
                exeName
            );

            if (!File.Exists(exePath))
                throw new FileNotFoundException("Pipeline EXE not found: " + exePath);

            string documentsRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "My Games",
                "Head of Hell"
            );

            string telemetryDir = Path.Combine(documentsRoot, "Telemetry", "Build");
            string runtimeOutDir = Path.Combine(documentsRoot, "PipelineOutputs", "runtime", "out");
            string runtimeEloOutDir = Path.Combine(documentsRoot, "PipelineOutputs", "runtime", "out_elo");

            string unityJsonPath = Path.Combine(
                Application.persistentDataPath,
                "ProfileAnalysis",
                "profile_analysis.json"
            );

            string unityJsonDir = Path.GetDirectoryName(unityJsonPath);
            if (!string.IsNullOrWhiteSpace(unityJsonDir))
                Directory.CreateDirectory(unityJsonDir);

            Directory.CreateDirectory(telemetryDir);
            Directory.CreateDirectory(runtimeOutDir);
            Directory.CreateDirectory(runtimeEloOutDir);

            string[] telemetryFiles = Directory.GetFiles(telemetryDir, "*.json", SearchOption.TopDirectoryOnly);
            if (telemetryFiles.Length == 0)
                throw new Exception("No telemetry JSON files found in Build folder: " + telemetryDir);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments =
                    $"--telemetry-dir \"{telemetryDir}\" " +
                    $"--out-dir \"{runtimeOutDir}\" " +
                    $"--elo-out-dir \"{runtimeEloOutDir}\" " +
                    $"--unity-output \"{unityJsonPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            string stdout;
            string stderr;

            using (Process process = new Process())
            {
                process.StartInfo = psi;
                process.Start();

                stdout = process.StandardOutput.ReadToEnd();
                stderr = process.StandardError.ReadToEnd();

                if (!process.WaitForExit(processTimeoutMs))
                {
                    try { process.Kill(); } catch { }
                    throw new Exception("Pipeline EXE timed out.");
                }

                UnityEngine.Debug.Log($"[profile_pipeline.exe] STDOUT:\n{stdout}");

                if (!string.IsNullOrWhiteSpace(stderr))
                    UnityEngine.Debug.LogWarning($"[profile_pipeline.exe] STDERR:\n{stderr}");

                if (process.ExitCode != 0)
                {
                    throw new Exception(
                        "Pipeline EXE failed with exit code " + process.ExitCode +
                        "\nSTDERR:\n" + stderr
                    );
                }
            }

            if (!File.Exists(unityJsonPath))
                throw new FileNotFoundException("profile_analysis.json was not generated: " + unityJsonPath);

            UnityEngine.Debug.Log("PIPELINE SUCCESS -> JSON READY");
            UnityEngine.Debug.Log("RUNTIME JSON PATH = " + unityJsonPath);
            UnityEngine.Debug.Log("RUNTIME JSON LAST WRITE = " + File.GetLastWriteTime(unityJsonPath));

            if (panelUI != null)
            {
                panelUI.RefreshAnalysis();
            }
            else
            {
                UnityEngine.Debug.LogWarning("Pipeline finished but panelUI is not assigned.");
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Plug-and-play pipeline failed: " + ex.Message);
        }
    }
}