using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Raylib_cs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using static Raylib_cs.Raylib;
using SharpImage = SixLabors.ImageSharp.Image;

namespace PhantomNebula.Core;

/// <summary>
/// GIF recorder - captures frames and creates animated GIFs asynchronously
/// Records at 24 FPS with 640x360 resolution (50% of screen)
/// Uses ImageSharp for lossless GIF encoding
/// </summary>
public class GifRecorder
{
    private bool isRecording = false;
    private List<Image<Rgb24>> frames = new List<Image<Rgb24>>();
    private int frameCount = 0;
    private int screenWidth = 1280;
    private int screenHeight = 720;
    private int captureWidth = 640;  // Half resolution
    private int captureHeight = 360; // Half resolution
    private const int MAX_FRAMES = 240; // 10 seconds at 24fps
    private static int recordingIndex = 0;
    private Task? savingTask = null;
    private bool isSaving = false;
    private int fps = 24;
    private Raylib_cs.Color clearColor = Raylib_cs.Color.Black;

    public bool IsRecording => isRecording;
    public bool IsSaving => isSaving;
    public int FrameCount => frameCount;

    public GifRecorder(int fps = 24, Raylib_cs.Color? clearScreenColor = null)
    {
        this.fps = fps;
        this.clearColor = clearScreenColor ?? Raylib_cs.Color.Black;
    }

    /// <summary>
    /// Convert a Raylib RGBA image to RGB24 byte array, stripping alpha completely
    /// </summary>
    private byte[] ConvertRaylibImageToRgb24(Raylib_cs.Image raylibImage, int width, int height)
    {
        byte[] rgbData = new byte[width * height * 3]; // 3 bytes per pixel (RGB only)

        unsafe
        {
            uint* sourcePixels = (uint*)raylibImage.Data;

            for (int i = 0; i < width * height; i++)
            {
                uint pixel = sourcePixels[i];

                // Extract RGB only - ignore alpha completely
                byte r = (byte)(pixel & 0xFF);
                byte g = (byte)((pixel >> 8) & 0xFF);
                byte b = (byte)((pixel >> 16) & 0xFF);
                // IGNORE: byte a = (byte)((pixel >> 24) & 0xFF);

                // Write RGB data directly (no compositing, no alpha)
                int byteIdx = i * 3;
                rgbData[byteIdx] = r;
                rgbData[byteIdx + 1] = g;
                rgbData[byteIdx + 2] = b;
            }
        }

        return rgbData;
    }

    /// <summary>
    /// Convert a Raylib RGBA image to RGB24, compositing over BLACK background
    /// Flips Y-axis vertically (render textures have inverted Y)
    /// Any semi-transparent pixels are composited on black to create opaque pixels
    /// </summary>
    private byte[] ConvertRaylibImageToRgb24WithComposite(Raylib_cs.Image raylibImage, int width, int height)
    {
        byte[] rgbData = new byte[width * height * 3]; // 3 bytes per pixel (RGB only)

        unsafe
        {
            uint* sourcePixels = (uint*)raylibImage.Data;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Flip Y: read from bottom row to top row
                    int sourceIdx = ((height - 1 - y) * width + x);
                    uint pixel = sourcePixels[sourceIdx];

                    // Extract RGBA
                    byte r = (byte)(pixel & 0xFF);
                    byte g = (byte)((pixel >> 8) & 0xFF);
                    byte b = (byte)((pixel >> 16) & 0xFF);
                    byte a = (byte)((pixel >> 24) & 0xFF);

                    // Composite over black background
                    float alpha = a / 255.0f;
                    byte composited_r = (byte)(r * alpha);
                    byte composited_g = (byte)(g * alpha);
                    byte composited_b = (byte)(b * alpha);

                    // Write composited RGB data
                    int destIdx = (y * width + x) * 3;
                    rgbData[destIdx] = composited_r;
                    rgbData[destIdx + 1] = composited_g;
                    rgbData[destIdx + 2] = composited_b;
                }
            }
        }

        return rgbData;
    }

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

    /// <summary>
    /// Capture a frame directly from a render texture
    /// </summary>
    public void CaptureFrameFromTexture(RenderTexture2D texture)
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
            // Load image from the render texture
            var textureImage = LoadImageFromTexture(texture.Texture);

            // Use actual image dimensions
            int actualWidth = textureImage.Width;
            int actualHeight = textureImage.Height;

            // Convert Raylib image to pixel data, compositing alpha on black background
            byte[] pixelData = ConvertRaylibImageToRgb24WithComposite(textureImage, actualWidth, actualHeight);

            // Load as ImageSharp image and resize to half resolution
            using (var fullSharpImage = SharpImage.LoadPixelData<Rgb24>(pixelData, actualWidth, actualHeight))
            {
                // Calculate half resolution from actual captured dimensions
                captureWidth = actualWidth / 2;
                captureHeight = actualHeight / 2;

                // Resize to half resolution using high-quality bicubic resampling
                fullSharpImage.Mutate(x => x.Resize(captureWidth, captureHeight, KnownResamplers.Bicubic));

                // Clone as RGB24 for GIF storage
                var rgb24Image = fullSharpImage.CloneAs<Rgb24>();
                frames.Add(rgb24Image);
            }

            UnloadImage(textureImage);
            frameCount++;

            if (frameCount % 60 == 0)
            {
                Console.WriteLine($"[GifRecorder] Captured {frameCount} frames at {captureWidth}x{captureHeight}...");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GifRecorder] Error capturing frame from texture: {ex.Message}");
        }
    }

    public void CaptureFrame(int screenWidthParam, int screenHeightParam)
    {
        if (!isRecording)
            return;

        screenWidth = screenWidthParam;
        screenHeight = screenHeightParam;

        if (frameCount >= MAX_FRAMES)
        {
            Console.WriteLine("[GifRecorder] Maximum frame limit reached, stopping recording");
            StopRecording();
            return;
        }

        try
        {
            // Capture full screenshot (entire screen including UI)
            var fullImage = LoadImageFromScreen();

            // Use actual captured image dimensions (not screen dimensions)
            int actualWidth = fullImage.Width;
            int actualHeight = fullImage.Height;

            // Convert Raylib image to pixel data using actual image dimensions
            byte[] pixelData = ConvertRaylibImageToRgb24(fullImage, actualWidth, actualHeight);

            // Load as ImageSharp image and resize to half resolution
            using (var fullSharpImage = SharpImage.LoadPixelData<Rgb24>(pixelData, actualWidth, actualHeight))
            {
                // Calculate half resolution from actual captured dimensions
                captureWidth = actualWidth / 2;
                captureHeight = actualHeight / 2;

                // Resize to half resolution using high-quality bicubic resampling
                fullSharpImage.Mutate(x => x.Resize(captureWidth, captureHeight, KnownResamplers.Bicubic));

                // Clone as RGB24 for GIF storage
                var rgb24Image = fullSharpImage.CloneAs<Rgb24>();
                frames.Add(rgb24Image);
            }

            UnloadImage(fullImage);
            frameCount++;

            if (frameCount % 60 == 0)
            {
                Console.WriteLine($"[GifRecorder] Captured {frameCount} frames at {captureWidth}x{captureHeight}...");
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
        List<Image<Rgb24>> framesToSave = new List<Image<Rgb24>>(frames);
        int framesToSaveCount = frameCount;
        int width = captureWidth;
        int height = captureHeight;
        frames.Clear();
        frameCount = 0;

        isSaving = true;
        Console.WriteLine($"[GifRecorder] Starting async GIF creation of {framesToSaveCount} frames ({width}x{height})");

        // Fire and forget async task
        savingTask = Task.Run(() => CreateGifAsync(framesToSave, framesToSaveCount, width, height));
    }

    private async Task CreateGifAsync(List<Image<Rgb24>> framesToSave, int totalFrames, int width, int height)
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
            string gifPath = Path.Combine(outputDir, $"recording_{timestamp}_{currentIndex}.gif");

            Console.WriteLine($"[GifRecorder] Creating GIF: {gifPath} ({totalFrames} frames @ {width}x{height} @ {fps}fps)");

            // Create GIF in background thread
            await Task.Run(() =>
            {
                try
                {
                    if (framesToSave.Count == 0)
                    {
                        Console.WriteLine("[GifRecorder] No frames to save");
                        return;
                    }

                    // Shift all black pixels to (1, 0, 0) to avoid black being treated as transparent
                    // This ensures pure black (0,0,0) is never used in the image
                    foreach (var frame in framesToSave)
                    {
                        frame.ProcessPixelRows(accessor =>
                        {
                            for (int y = 0; y < accessor.Height; y++)
                            {
                                var row = accessor.GetRowSpan(y);
                                for (int x = 0; x < row.Length; x++)
                                {
                                    // If pixel is pure black, shift to (1, 0, 0)
                                    if (row[x].R == 0 && row[x].G == 0 && row[x].B == 0)
                                    {
                                        row[x] = new Rgb24(1, 0, 0);
                                    }
                                }
                            }
                        });
                    }

                    // Clone first frame as base
                    using (var gifImage = framesToSave[0].CloneAs<Rgb24>())
                    {
                        // Set up GIF metadata - RepeatCount on the image
                        var gifMetadata = gifImage.Metadata.GetGifMetadata();
                        gifMetadata.RepeatCount = 0; // Infinite loop

                        // Calculate frame delay in centiseconds (GIF standard)
                        int frameDelayCs = (int)Math.Round(100.0 / fps);

                        // Set first frame delay through root frame metadata
                        var rootFrameMetadata = gifImage.Frames.RootFrame.Metadata.GetGifMetadata();
                        rootFrameMetadata.FrameDelay = frameDelayCs;
                        rootFrameMetadata.DisposalMethod = GifDisposalMethod.RestoreToBackground;
                        rootFrameMetadata.HasTransparency = false;

                        // Add remaining frames
                        for (int i = 1; i < framesToSave.Count; i++)
                        {
                            gifImage.Frames.AddFrame(framesToSave[i].Frames.RootFrame);

                            // Get the newly added frame and set its metadata
                            var addedFrame = gifImage.Frames[gifImage.Frames.Count - 1];
                            var frameMetadata = addedFrame.Metadata.GetGifMetadata();
                            frameMetadata.FrameDelay = frameDelayCs;
                            frameMetadata.DisposalMethod = GifDisposalMethod.RestoreToBackground;
                            frameMetadata.HasTransparency = false;
                        }

                        // Set HasTransparency = false on ALL frames
                        foreach (var frame in gifImage.Frames)
                        {
                            var frameGifMeta = frame.Metadata.GetGifMetadata();
                            frameGifMeta.HasTransparency = false;
                        }

                        // Create encoder with 64-color quantizer for smaller file sizes
                        var quantizerOptions = new QuantizerOptions
                        {
                            MaxColors = 64
                        };
                        var encoder = new GifEncoder
                        {
                            Quantizer = new OctreeQuantizer(quantizerOptions)
                        };

                        // Save to file
                        using (var outputStream = File.Create(gifPath))
                        {
                            gifImage.Save(outputStream, encoder);
                        }
                    }

                    Console.WriteLine($"[GifRecorder] GIF created successfully: {gifPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GifRecorder] Error creating GIF: {ex.Message}");
                    Console.WriteLine($"[GifRecorder] Stack trace: {ex.StackTrace}");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GifRecorder] Error during GIF creation: {ex.Message}");
            Console.WriteLine($"[GifRecorder] Stack trace: {ex.StackTrace}");
        }
        finally
        {
            // Clean up frames
            foreach (var frame in framesToSave)
            {
                frame?.Dispose();
            }
            framesToSave.Clear();
            isSaving = false;
        }
    }

    public void SetClearColor(Raylib_cs.Color color)
    {
        clearColor = color;
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
            Console.WriteLine("[GifRecorder] Waiting for GIF creation to complete...");
            try
            {
                if (!savingTask.Wait(TimeSpan.FromSeconds(120)))
                {
                    Console.WriteLine("[GifRecorder] GIF creation did not complete within timeout");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GifRecorder] Error waiting for GIF creation: {ex.Message}");
            }
        }

        // Clean up frames
        foreach (var frame in frames)
        {
            frame?.Dispose();
        }
        frames.Clear();
    }
}
