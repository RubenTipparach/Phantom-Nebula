using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Core;

/// <summary>
/// GIF recorder - captures frames and saves them as an animated GIF
/// Uses async tasks for non-blocking frame saving
/// </summary>
public class GifRecorder
{
    private bool isRecording = false;
    private List<Image> frames = new List<Image>();
    private int frameCount = 0;
    private const int MAX_FRAMES = 600; // 10 seconds at 60fps
    private const int FRAME_DELAY = 16; // milliseconds (60fps)
    private static int recordingIndex = 0;
    private Task? savingTask = null;
    private bool isSaving = false;

    public bool IsRecording => isRecording;
    public bool IsSaving => isSaving;
    public int FrameCount => frameCount;

    public void StartRecording()
    {
        if (isRecording)
        {
            Console.WriteLine("[GifRecorder] Already recording, ignoring start request");
            return;
        }

        frames.Clear();
        frameCount = 0;
        isRecording = true;
        Console.WriteLine("[GifRecorder] Started recording GIF");
    }

    public void StopRecording()
    {
        if (!isRecording)
        {
            Console.WriteLine("[GifRecorder] Not recording, ignoring stop request");
            return;
        }

        isRecording = false;
        Console.WriteLine($"[GifRecorder] Stopped recording - captured {frameCount} frames");

        if (frameCount > 0)
        {
            // Start async save task without blocking
            SaveGifAsync();
        }
    }

    public void CaptureFrame(int screenWidth, int screenHeight)
    {
        if (!isRecording)
            return;

        if (frameCount >= MAX_FRAMES)
        {
            Console.WriteLine("[GifRecorder] Maximum frame limit reached, stopping recording");
            StopRecording();
            return;
        }

        try
        {
            // Capture the current frame as an image using Raylib's screenshot function
            Image screenshot = LoadImageFromScreen();
            frames.Add(screenshot);
            frameCount++;

            if (frameCount % 60 == 0)
            {
                Console.WriteLine($"[GifRecorder] Captured {frameCount} frames...");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GifRecorder] Error capturing frame: {ex.Message}");
        }
    }

    private void SaveGifAsync()
    {
        if (isSaving)
        {
            Console.WriteLine("[GifRecorder] Save already in progress, ignoring request");
            return;
        }

        // Create a copy of frames to save asynchronously
        List<Image> framesToSave = new List<Image>(frames);
        int framesToSaveCount = frameCount;
        frames.Clear();
        frameCount = 0;

        isSaving = true;
        Console.WriteLine($"[GifRecorder] Starting async save of {framesToSaveCount} frames");

        // Fire and forget async task
        savingTask = Task.Run(() => SaveFramesAsPNGAsync(framesToSave, framesToSaveCount));
    }

    private async Task SaveFramesAsPNGAsync(List<Image> framesToSave, int totalFrames)
    {
        try
        {
            // Create output directory if it doesn't exist
            string outputDir = "Recordings";
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Generate filename with timestamp
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            int currentIndex = recordingIndex++;

            Console.WriteLine($"[GifRecorder] Async save starting: {totalFrames} frames to {outputDir}/recording_{timestamp}_{currentIndex}_*.png");

            // Save frames in parallel batches for better performance
            int batchSize = 4;
            for (int batch = 0; batch < framesToSave.Count; batch += batchSize)
            {
                List<Task> batchTasks = new List<Task>();
                int endBatch = Math.Min(batch + batchSize, framesToSave.Count);

                for (int i = batch; i < endBatch; i++)
                {
                    int frameIndex = i; // Capture for closure
                    Task saveTask = Task.Run(() =>
                    {
                        try
                        {
                            string framePath = Path.Combine(outputDir, $"recording_{timestamp}_{currentIndex}_{frameIndex:D4}.png");
                            ExportImage(framesToSave[frameIndex], framePath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[GifRecorder] Error saving frame {frameIndex}: {ex.Message}");
                        }
                    });
                    batchTasks.Add(saveTask);
                }

                // Wait for batch to complete before starting next batch
                await Task.WhenAll(batchTasks);
            }

            Console.WriteLine($"[GifRecorder] Async save completed: {outputDir}/recording_{timestamp}_{currentIndex}_*.png");

            // Unload all frames from memory
            for (int i = 0; i < framesToSave.Count; i++)
            {
                UnloadImage(framesToSave[i]);
            }
            framesToSave.Clear();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GifRecorder] Error during async save: {ex.Message}");
        }
        finally
        {
            isSaving = false;
        }
    }

    public void Dispose()
    {
        if (isRecording)
        {
            StopRecording();
        }

        // Wait for any pending save operations to complete (with timeout)
        if (isSaving && savingTask != null)
        {
            Console.WriteLine("[GifRecorder] Waiting for async save to complete...");
            try
            {
                if (!savingTask.Wait(TimeSpan.FromSeconds(30)))
                {
                    Console.WriteLine("[GifRecorder] Async save did not complete within timeout");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GifRecorder] Error waiting for async save: {ex.Message}");
            }
        }

        // Clean up any remaining frames
        for (int i = 0; i < frames.Count; i++)
        {
            UnloadImage(frames[i]);
        }
        frames.Clear();
    }
}
