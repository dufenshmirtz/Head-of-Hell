using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class ProfileAnalysisRunner : MonoBehaviour
{
    [Header("Runtime Pipeline")]
    public string pipelineExeName = "runtime_profile_pipeline.exe";

    [Header("UI")]
    public ProfileAnalysisPanelUI panelUI;

    public void RunPipelineAndRefresh()
    {
        try
        {
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string telemetryDir = Path.Combine(documents, "My Games", "Head of Hell", "Telemetry");
            string telemetryBuildDir = Path.Combine(telemetryDir, "Build");

            // Αν υπάρχει Build subfolder, χρησιμοποίησέ το. Αλλιώς τον βασικό Telemetry.
            if (Directory.Exists(telemetryBuildDir))
                telemetryDir = telemetryBuildDir;

            string analysisRoot = Path.Combine(documents, "My Games", "Head of Hell", "Analysis");
            string outDir = Path.Combine(analysisRoot, "out");
            string eloOutDir = Path.Combine(analysisRoot, "out_elo");
            string unityOutput = Path.Combine(Application.streamingAssetsPath, "ProfileAnalysis", "profile_analysis.json");

            Directory.CreateDirectory(analysisRoot);
            Directory.CreateDirectory(outDir);
            Directory.CreateDirectory(eloOutDir);
            Directory.CreateDirectory(Path.GetDirectoryName(unityOutput));

            string exePath = Path.Combine(Application.streamingAssetsPath, "Analysis", pipelineExeName);

            if (!File.Exists(exePath))
                throw new FileNotFoundException("Pipeline exe not found: " + exePath);

            string args =
                $"--telemetry-dir \"{telemetryDir}\" " +
                $"--out-dir \"{outDir}\" " +
                $"--elo-out-dir \"{eloOutDir}\" " +
                $"--unity-output \"{unityOutput}\"";

            UnityEngine.Debug.Log("PIPELINE EXE: " + exePath);
            UnityEngine.Debug.Log("PIPELINE ARGS: " + args);
            UnityEngine.Debug.Log("PIPELINE WORKDIR: " + Path.GetDirectoryName(exePath));

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args,
                WorkingDirectory = Path.GetDirectoryName(exePath),
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

                UnityEngine.Debug.Log($"[runtime_profile_pipeline] STDOUT:\n{stdout}");

                if (!string.IsNullOrWhiteSpace(stderr))
                    UnityEngine.Debug.LogWarning($"[runtime_profile_pipeline] STDERR:\n{stderr}");

                if (process.ExitCode != 0)
                    throw new Exception($"runtime_profile_pipeline failed with exit code {process.ExitCode}");

                UnityEngine.Debug.Log("Full analysis pipeline completed.");
            }

            LogIfExists(Path.Combine(outDir, "dataset_round_level.csv"));
            LogIfExists(Path.Combine(outDir, "dataset_profile_level.csv"));
            LogIfExists(Path.Combine(eloOutDir, "elo_overall.csv"));
            LogIfExists(unityOutput);

            if (panelUI != null)
                panelUI.RefreshAnalysis();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Failed to run full pipeline: " + ex.Message);
        }
    }

    private void LogIfExists(string path)
    {
        if (File.Exists(path))
            UnityEngine.Debug.Log($"Updated file: {path} | LastWrite={File.GetLastWriteTime(path)}");
        else
            UnityEngine.Debug.LogWarning("Missing expected file: " + path);
    }
}