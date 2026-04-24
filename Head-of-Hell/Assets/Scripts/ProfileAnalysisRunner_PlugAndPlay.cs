using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ProfileAnalysisRunner_PlugAndPlay : MonoBehaviour
{
    [Header("Pipeline EXE")]
    public string exeName = "runtime_profile_pipeline.exe";

    [Header("UI")]
    public ProfileAnalysisPanelUI panelUI;
    public GameObject loadingScreenRoot;

    [Header("Safety")]
    public int processTimeoutMs = 120000; // 120 sec

    private bool isRunning;
    private RuntimeLoadingOverlay runtimeLoadingOverlay;

    public async void RunPipelineAndRefresh()
    {
        if (isRunning)
            return;

        isRunning = true;
        ShowLoadingScreen();

        try
        {
            UpdateLoadingProgress(0.05f, "Preparing analysis", "Collecting telemetry files");
            PlugAndPlayPipelineRequest request = BuildPipelineRequest();
            UpdateLoadingProgress(0.15f, "Running analysis pipeline", "Extracting combat behaviour");
            await Task.Yield();
            PlugAndPlayPipelineResult result = await Task.Run(() => RunPipelineInternal(request));

            UpdateLoadingProgress(0.85f, "Finalizing analysis", "Pipeline output received");

            if (panelUI != null)
            {
                UpdateLoadingProgress(0.95f, "Reloading profile data", "Applying refreshed charts and stats");
                panelUI.RefreshAnalysis();

                if (panelUI.profileAnalysisRoot != null)
                    panelUI.profileAnalysisRoot.SetActive(true);
            }
            else
            {
                UnityEngine.Debug.LogWarning("Pipeline finished but panelUI is not assigned.");
            }

            UnityEngine.Debug.Log("Profiles JSON path: " + request.ProfilesJsonPath);
            UnityEngine.Debug.Log($"[profile_pipeline.exe] STDOUT:\n{result.Stdout}");

            if (!string.IsNullOrWhiteSpace(result.Stderr))
                UnityEngine.Debug.LogWarning($"[profile_pipeline.exe] STDERR:\n{result.Stderr}");

            UnityEngine.Debug.Log("PIPELINE SUCCESS -> JSON READY");
            UnityEngine.Debug.Log("RUNTIME JSON PATH = " + request.UnityJsonPath);
            UnityEngine.Debug.Log("RUNTIME JSON LAST WRITE = " + File.GetLastWriteTime(request.UnityJsonPath));
            UpdateLoadingProgress(1f, "Analysis complete", "Profile data updated");
            await Task.Delay(180);
        }
        catch (Exception ex)
        {
            UpdateLoadingProgress(runtimeLoadingOverlay != null ? runtimeLoadingOverlay.CurrentProgress : 0f, "Analysis failed", "Check Unity Console for details");
            UnityEngine.Debug.LogError("Plug-and-play pipeline failed: " + ex.Message);

            if (panelUI != null && panelUI.profileAnalysisRoot != null)
                panelUI.profileAnalysisRoot.SetActive(true);
        }
        finally
        {
            HideLoadingScreen();
            isRunning = false;
        }
    }

    private PlugAndPlayPipelineRequest BuildPipelineRequest()
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

        string profilesJsonPath = Path.Combine(
            Application.persistentDataPath,
            "profiles.json"
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

        return new PlugAndPlayPipelineRequest
        {
            ExePath = exePath,
            TelemetryDir = telemetryDir,
            RuntimeOutDir = runtimeOutDir,
            RuntimeEloOutDir = runtimeEloOutDir,
            UnityJsonPath = unityJsonPath,
            ProfilesJsonPath = profilesJsonPath
        };
    }

    private PlugAndPlayPipelineResult RunPipelineInternal(PlugAndPlayPipelineRequest request)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = request.ExePath,
            Arguments =
                $"--telemetry-dir \"{request.TelemetryDir}\" " +
                $"--out-dir \"{request.RuntimeOutDir}\" " +
                $"--elo-out-dir \"{request.RuntimeEloOutDir}\" " +
                $"--unity-output \"{request.UnityJsonPath}\" " +
                $"--profiles-json \"{request.ProfilesJsonPath}\"",
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

            if (process.ExitCode != 0)
            {
                throw new Exception(
                    "Pipeline EXE failed with exit code " + process.ExitCode +
                    "\nSTDERR:\n" + stderr
                );
            }
        }

        if (!File.Exists(request.UnityJsonPath))
            throw new FileNotFoundException("profile_analysis.json was not generated: " + request.UnityJsonPath);

        return new PlugAndPlayPipelineResult
        {
            Stdout = stdout,
            Stderr = stderr
        };
    }

    private void ShowLoadingScreen()
    {
        if (panelUI != null && panelUI.profileAnalysisRoot != null)
            panelUI.profileAnalysisRoot.SetActive(false);

        if (loadingScreenRoot != null)
        {
            loadingScreenRoot.SetActive(true);
            return;
        }

        if (runtimeLoadingOverlay == null)
            runtimeLoadingOverlay = RuntimeLoadingOverlay.Create(GetReferenceFont());

        runtimeLoadingOverlay.Show();
    }

    private void HideLoadingScreen()
    {
        if (loadingScreenRoot != null)
        {
            loadingScreenRoot.SetActive(false);
            return;
        }

        if (runtimeLoadingOverlay != null)
            runtimeLoadingOverlay.Hide();
    }

    private void UpdateLoadingProgress(float progress, string message = null, string hint = null)
    {
        if (runtimeLoadingOverlay != null)
            runtimeLoadingOverlay.SetProgress(progress, message, hint);
    }

    private TMP_FontAsset GetReferenceFont()
    {
        if (panelUI != null)
        {
            if (panelUI.profileNameText != null && panelUI.profileNameText.font != null)
                return panelUI.profileNameText.font;

            if (panelUI.styleLabelText != null && panelUI.styleLabelText.font != null)
                return panelUI.styleLabelText.font;
        }

        return TMP_Settings.defaultFontAsset;
    }

    private class PlugAndPlayPipelineRequest
    {
        public string ExePath;
        public string TelemetryDir;
        public string RuntimeOutDir;
        public string RuntimeEloOutDir;
        public string UnityJsonPath;
        public string ProfilesJsonPath;
    }

    private class PlugAndPlayPipelineResult
    {
        public string Stdout;
        public string Stderr;
    }
}
