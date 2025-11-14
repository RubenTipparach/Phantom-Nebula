// Voronoi Starfield Shader
// Creates a static starfield using proper 3D Voronoi
// 3D Voronoi referenced from https://www.shadertoy.com/view/flSGDK

//=============================================================================
// TWEAKABLE PARAMETERS - Adjust these to change star appearance
//=============================================================================

// Star Density (higher = more stars)
static const float StarDensityLayer1 = 8.0;    // Base layer density
static const float StarDensityLayer2 = 12.0;   // Medium layer density
static const float StarDensityLayer3 = 16.0;   // Far layer density

// Star Size (lower = smaller/sharper stars)
static const float StarSharpness = 0.02;       // Distance threshold for star points
static const float StarFalloff = 0.0;          // Smoothstep falloff (keep at 0 for sharp stars)

// Star Brightness
static const float StarBrightnessThreshold = 0.7;  // Only show stars brighter than this (0-1)
static const float StarBrightnessMultiplier = 1.0; // Overall brightness multiplier

// Layer Intensities (0-1)
static const float Layer1Intensity = 1.0;      // Brightest/closest stars
static const float Layer2Intensity = 0.8;      // Medium stars
static const float Layer3Intensity = 0.6;      // Dimmest/farthest stars

// Space Background Color (RGB, 0-1)
static const float3 SpaceColor = float3(0.02, 0.01, 0.05);  // Dark blue/purple

// Star Color (RGB, 0-1)
static const float3 StarColor = float3(1.0, 0.98, 0.95);    // Warm white

//=============================================================================

float4x4 View;
float4x4 Projection;

// Hash functions
float hash(float x)
{
    return frac(x + 1.3215 * 1.8152);
}

float hash3(float3 a)
{
    return frac((hash(a.z * 42.8883) + hash(a.y * 36.9125) + hash(a.x * 65.4321)) * 291.1257);
}

float3 rehash3(float x)
{
    return float3(
        hash(((x + 0.5283) * 59.3829) * 274.3487),
        hash(((x + 0.8192) * 83.6621) * 345.3871),
        hash(((x + 0.2157) * 36.6521) * 458.3971)
    );
}

float sqr(float x)
{
    return x * x;
}

float fastdist(float3 a, float3 b)
{
    return sqr(b.x - a.x) + sqr(b.y - a.y) + sqr(b.z - a.z);
}

// Proper 3D Voronoi
void voronoi3D_float(float3 pos, float density, out float Out, out float Cells)
{
    float4 p[27];
    pos *= density;
    float x = pos.x;
    float y = pos.y;
    float z = pos.z;

    for (int _x = -1; _x < 2; _x++)
    {
        for (int _y = -1; _y < 2; _y++)
        {
            for(int _z = -1; _z < 2; _z++)
            {
                float3 _p = float3(floor(x), floor(y), floor(z)) + float3(_x, _y, _z);
                float h = hash3(_p);
                p[(_x + 1) + ((_y + 1) * 3) + ((_z + 1) * 3 * 3)] = float4((rehash3(h) + _p).xyz, h);
            }
        }
    }

    float m = 9999.9999;
    float w = 0.0;

    for (int i = 0; i < 27; i++)
    {
        float d = fastdist(float3(x, y, z), p[i].xyz);
        if(d < m)
        {
            m = d;
            w = p[i].w;
        }
    }

    Out = m;
    Cells = w;
}

// Generate stars using proper 3D Voronoi
float starfield3D(float3 dir, float layer, float density)
{
    float voronoiDist, cellId;

    // Get voronoi value using proper 3D Voronoi
    voronoi3D_float(dir, density, voronoiDist, cellId);

    // Create sharp star points
    float star = smoothstep(StarSharpness, StarFalloff, voronoiDist);

    // Use cell ID for brightness variation
    float brightness = frac(cellId * 123.456);

    // Filter out dimmer stars (only show brightest)
    star *= step(StarBrightnessThreshold, brightness);

    // Apply brightness multiplier
    return star * brightness * StarBrightnessMultiplier;
}

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 ViewRay : TEXCOORD0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = input.Position;

    // Calculate view ray in view space
    float4 farPoint = float4(input.Position.xy, 1.0, 1.0);
    farPoint = mul(farPoint, Projection);
    farPoint /= farPoint.w;

    // Transform to world space direction
    output.ViewRay = mul(farPoint.xyz, (float3x3)View);

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Normalize the view direction
    float3 viewDir = normalize(input.ViewRay);

    // Create multiple layers of stars using 3D voronoi with configurable densities
    float stars = 0.0;
    stars += starfield3D(viewDir, 1.0, StarDensityLayer1) * Layer1Intensity;
    stars += starfield3D(viewDir, 2.0, StarDensityLayer2) * Layer2Intensity;
    stars += starfield3D(viewDir, 3.0, StarDensityLayer3) * Layer3Intensity;

    // Use configurable colors
    float3 finalColor = SpaceColor + stars * StarColor;

    return float4(finalColor, 1.0);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
    }
};
