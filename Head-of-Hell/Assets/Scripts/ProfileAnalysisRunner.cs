using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class ProfileAnalysisRunner : MonoBehaviour
{
    [Header("Python")]
    public string pythonExe = @"C:\Users\iroha\AppData\Local\Programs\Python\Python312\python.exe";
    public string workingDirectory = @"C:\Users\iroha\Desktop\analysis";

    [Header("Paths")]
    public string telemetryDirectory = @"C:\Users\iroha\Documents\My Games\Head of Hell\Telemetry";

    [Header("Scripts")]
    public string extractorScript = "extractor_upgraded.py";
    public string eloScript = "elo_from_telemetry.py";
    public string exportScript = "export_profile_analysis.py";

    [Header("UI")]
    public ProfileAnalysisPanelUI panelUI;

    public void RunPipelineAndRefresh()
    {
        try
        {
            RunPythonScript(
                extractorScript,
                $"--telemetry-dir \"{telemetryDirectory}\" --out-dir \"out\""
            );

            RunPythonScript(
                eloScript,
                $"--telemetry-dir \"{telemetryDirectory}\" --out-dir \"out_elo\""
            );

            RunPythonScript(exportScript);

            UnityEngine.Debug.Log("Full analysis pipeline completed.");

            LogIfExists(Path.Combine(workingDirectory, "out", "dataset_round_level.csv"));
            LogIfExists(Path.Combine(workingDirectory, "out", "dataset_profile_level.csv"));
            LogIfExists(Path.Combine(workingDirectory, "out_elo", "elo_overall.csv"));
            LogIfExists(Path.Combine(Application.streamingAssetsPath, "ProfileAnalysis", "profile_analysis.json"));

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
    private void RunPythonScript(string scriptName, string extraArgs = "")
    {
        string fullPath = Path.Combine(workingDirectory, scriptName);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Python script not found: " + fullPath);

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = pythonExe,
            Arguments = $"\"{fullPath}\" {extraArgs}",
            WorkingDirectory = workingDirectory,
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

            UnityEngine.Debug.Log($"[{scriptName}] STDOUT:\n{stdout}");

            if (!string.IsNullOrWhiteSpace(stderr))
                UnityEngine.Debug.LogWarning($"[{scriptName}] STDERR:\n{stderr}");

            if (process.ExitCode != 0)
                throw new Exception($"{scriptName} failed with exit code {process.ExitCode}");

            UnityEngine.Debug.Log($"{scriptName} finished successfully.");
        }
    }
}