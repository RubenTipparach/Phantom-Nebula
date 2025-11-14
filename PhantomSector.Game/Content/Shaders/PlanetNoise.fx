// Planet Shader with Atmosphere
// Based on https://www.shadertoy.com/view/XsjGRd
// Ported from GLSL to HLSL for MonoGame

//=============================================================================
// PARAMETERS
//=============================================================================

float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;
float Time;

// Sun position (orbiting light source)
float3 SunPosition = float3(100.0, 50.0, 0.0);

//=============================================================================
// TWEAKABLE CONSTANTS
//=============================================================================

static const float PI = 3.14159265359;
static const float PLANET_RADIUS = 1.0;
static const float ATMOSPHERE_RADIUS = 1.15;

// Terrain detail settings
static const float TERRAIN_SCALE = 5.0;          // Higher = more detail
static const float CONTINENT_SCALE = 2.0;        // Scale of land masses
static const float LAND_DETAIL_SCALE = 12.0;     // Fine terrain detail
static const float LAND_THRESHOLD = 0.52;        // Lower = more land
static const float3 CONTINENT_OFFSET = float3(0.5, 0.2, 0.8); // Seed for continent pattern

// Ice cap settings
static const float ICE_START = 0.7;              // Latitude where ice begins (0-1)
static const float ICE_END = 0.85;               // Latitude where ice is full (0-1)

// Cloud settings
static const float CLOUD_SCALE = 10.0;            // Cloud pattern scale
static const float CLOUD_SPEED = 0.15;          // Cloud movement speed
static const float CLOUD_SHADOW_STRENGTH = 0.8;  // How dark cloud shadows are
static const float CLOUD_THRESHOLD_LOW = 0.45;   // Cloud density threshold
static const float CLOUD_THRESHOLD_HIGH = 0.95;
static const float CLOUD_POLE_START = 0.9;       // Latitude where clouds start reducing
static const float CLOUD_POLE_END = 0.6;         // Latitude where clouds are minimal
static const float3 CLOUD_SHADOW_OFFSET = float3(0.3, 0.0, 0.02); // Offset for cloud shadows on ground
static const float CLOUD_BRIGHTNESS = 0.8;       // Cloud opacity/blend amount
static const float CLOUD_MIN_LIGHTING = 0.1;     // Minimum light on clouds (ambient)

// Spiral distortion settings
static const float SPIRAL_AMOUNT = 0.3;          // How much spiral distortion
static const float SPIRAL_OFFSET_SCALE = 1.1;    // Spiral offset multiplier
static const float SPIRAL_BLEND_POWER = 1.5;     // Spiral blend sharpness

// Atmosphere settings
static const float ATMOSPHERE_RIM_POWER = 2.0;   // Sharpness of atmosphere rim (higher = thinner)
static const float ATMOSPHERE_SCATTER_MIN = 0.5; // Minimum scatter brightness
static const float ATMOSPHERE_SCATTER_MAX = 0.5; // Additional scatter on sunlit side
static const float ATMOSPHERE_STRENGTH = 0.3;    // Overall atmosphere intensity

// Lighting settings
static const float AMBIENT_LIGHT = 0.1;         // Ambient light strength
static const float OCEAN_SPECULAR_POWER = 32.0;  // Specular highlight sharpness (higher = smaller)
static const float OCEAN_SPECULAR_STRENGTH = 0.5; // Specular highlight intensity
static const float OCEAN_MASK_THRESHOLD = 0.2;   // How close color must be to ocean for specular

// Planet colors
static const float3 OCEAN_COLOR = float3(0.02, 0.1, 0.3);
static const float3 LAND_COLOR1 = float3(0.1, 0.3, 0.05);
static const float3 LAND_COLOR2 = float3(0.3, 0.25, 0.1);
static const float3 ICE_COLOR = float3(0.9, 0.95, 1.0);
static const float3 CLOUD_COLOR = float3(1.0, 1.0, 1.0);
static const float3 ATMOSPHERE_COLOR = float3(0.3, 0.5, 0.9);

//=============================================================================
// HASH FUNCTIONS
//=============================================================================

float hash(float n)
{
    return frac(sin(n) * 43758.5453123);
}

float hash2(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

float hash3(float3 p)
{
    p = frac(p * float3(0.1031, 0.1030, 0.0973));
    p += dot(p, p.yzx + 19.19);
    return frac((p.x + p.y) * p.z);
}

//=============================================================================
// NOISE FUNCTIONS
//=============================================================================

float noise(float3 x)
{
    float3 p = floor(x);
    float3 f = frac(x);
    f = f * f * (3.0 - 2.0 * f);

    float n = p.x + p.y * 157.0 + 113.0 * p.z;

    return lerp(
        lerp(
            lerp(hash(n + 0.0), hash(n + 1.0), f.x),
            lerp(hash(n + 157.0), hash(n + 158.0), f.x),
            f.y
        ),
        lerp(
            lerp(hash(n + 113.0), hash(n + 114.0), f.x),
            lerp(hash(n + 270.0), hash(n + 271.0), f.x),
            f.y
        ),
        f.z
    );
}

// Fractional Brownian Motion
float fbm(float3 p)
{
    float f = 0.0;
    f += 0.5000 * noise(p); p *= 2.02;
    f += 0.2500 * noise(p); p *= 2.03;
    f += 0.1250 * noise(p); p *= 2.01;
    f += 0.0625 * noise(p);
    return f / 0.9375;
}

//=============================================================================
// SPIRAL DISTORTION (for clouds)
//=============================================================================

float2 Spiral(float2 uv, float spiralAmount)
{
    float2 center = float2(0.5, 0.5);
    float2 delta = uv - center;
    float dist = length(delta);

    // Offset perpendicular to radius
    float2 offset = float2(delta.y, -delta.x);

    // Blend based on distance from center
    float blend = pow(saturate((0.5 - dist) * 2.0), SPIRAL_BLEND_POWER);

    return uv + offset * SPIRAL_OFFSET_SCALE * spiralAmount * blend;
}

//=============================================================================
// PLANET SURFACE
//=============================================================================

float3 GetPlanetColor(float3 normal, float3 worldPos)
{
    // Convert normal to spherical UV coordinates
    float2 uv;
    uv.x = (atan2(normal.x, normal.z) + PI) / (PI * 2.0);
    uv.y = acos(normal.y) / PI;

    // Base terrain noise - using tweakable scale
    float3 noisePos = normal * TERRAIN_SCALE;
    float terrain = fbm(noisePos);

    // Continental shapes (larger scale) with offset for variety
    float continents = fbm(normal * CONTINENT_SCALE + CONTINENT_OFFSET);

    // Determine if land or ocean
    float landMask = step(LAND_THRESHOLD, continents);

    // Land color variation with fine detail
    float landDetail = fbm(normal * LAND_DETAIL_SCALE);
    float3 landColor = lerp(LAND_COLOR1, LAND_COLOR2, landDetail);

    // Ocean
    float3 oceanColor = OCEAN_COLOR;

    // Mix land and ocean
    float3 surfaceColor = lerp(oceanColor, landColor, landMask);

    // Ice caps at poles (using tweakable parameters)
    float poleAmount = abs(normal.y);
    float iceMask = smoothstep(ICE_START, ICE_END, poleAmount);
    surfaceColor = lerp(surfaceColor, ICE_COLOR, iceMask);

    return surfaceColor;
}

//=============================================================================
// CLOUDS
//=============================================================================

float GetCloudDensity(float3 normal, float time, float3 offset)
{
    // Spiral distorted UV
    float2 uv;
    uv.x = (atan2(normal.x, normal.z) + PI) / (PI * 2.0);
    uv.y = acos(normal.y) / PI;

    // Apply spiral distortion
    float2 spiralUV = Spiral(uv, SPIRAL_AMOUNT);

    // Pan clouds slowly across the surface
    float3 cloudOffset = float3(time * CLOUD_SPEED, 0.0, time * CLOUD_SPEED * 0.5);

    // Multi-layer clouds with panning and optional offset (for shadows)
    float clouds = fbm(normal * CLOUD_SCALE + cloudOffset + offset);
    clouds = smoothstep(CLOUD_THRESHOLD_LOW, CLOUD_THRESHOLD_HIGH, clouds);

    // Reduce clouds at poles (using tweakable parameters)
    float poleAmount = abs(normal.y);
    clouds *= smoothstep(CLOUD_POLE_START, CLOUD_POLE_END, poleAmount);

    return clouds;
}

//=============================================================================
// ATMOSPHERE
//=============================================================================

float3 GetAtmosphere(float3 rayDir, float3 normal, float3 lightDir)
{
    // Fresnel-like rim lighting for atmosphere
    float rim = 1.0 - max(0.0, dot(rayDir, normal));
    rim = pow(rim, ATMOSPHERE_RIM_POWER);

    // Atmospheric scattering (brighter on sunlit side)
    float scatter = max(0.0, dot(normal, lightDir));

    return ATMOSPHERE_COLOR * rim * (ATMOSPHERE_SCATTER_MIN + ATMOSPHERE_SCATTER_MAX * scatter);
}

//=============================================================================
// LIGHTING
//=============================================================================

float3 CalculateLighting(float3 worldPos, float3 normal, float3 surfaceColor, float3 lightDir, float3 viewDir, float cloudDensity, float shadowCloudDensity)
{
    // Diffuse lighting
    float ndotl = max(0.0, dot(normal, lightDir));

    // Cloud shadows - use offset cloud density for shadow casting
    float cloudShadow = 1.0 - (shadowCloudDensity * CLOUD_SHADOW_STRENGTH);
    float3 diffuse = surfaceColor * ndotl * cloudShadow;

    // Ambient
    float3 ambient = surfaceColor * AMBIENT_LIGHT * cloudShadow;

    // Specular (only on ocean)
    float3 halfVec = normalize(lightDir + viewDir);
    float spec = pow(max(0.0, dot(normal, halfVec)), OCEAN_SPECULAR_POWER);

    // Only apply specular to ocean areas
    float oceanMask = step(length(surfaceColor - OCEAN_COLOR), OCEAN_MASK_THRESHOLD);
    float3 specular = float3(1.0, 1.0, 1.0) * spec * oceanMask * OCEAN_SPECULAR_STRENGTH;

    // Combine lighting
    float3 lit = ambient + diffuse + specular;

    // Apply clouds (clouds are lit by the sun)
    float3 cloudColor = CLOUD_COLOR * max(CLOUD_MIN_LIGHTING, ndotl);
    lit = lerp(lit, cloudColor, cloudDensity * CLOUD_BRIGHTNESS);

    return lit;
}

//=============================================================================
// VERTEX SHADER
//=============================================================================

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 WorldPos : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float3 ViewDir : TEXCOORD2;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPos = mul(input.Position, World);
    float4 viewPos = mul(worldPos, View);
    output.Position = mul(viewPos, Projection);

    output.WorldPos = worldPos.xyz;
    output.Normal = normalize(mul(input.Normal, (float3x3)World));
    output.ViewDir = normalize(CameraPosition - worldPos.xyz);

    return output;
}

//=============================================================================
// PIXEL SHADER
//=============================================================================

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 normal = normalize(input.Normal);
    float3 viewDir = normalize(input.ViewDir);

    // Calculate light direction (from sun position)
    float3 lightDir = normalize(SunPosition - input.WorldPos);

    // Get base planet surface color
    float3 surfaceColor = GetPlanetColor(normal, input.WorldPos);

    // Get cloud density (no offset for visible clouds)
    float cloudDensity = GetCloudDensity(normal, Time, float3(0.0, 0.0, 0.0));

    // Get offset cloud density for shadows (simulates clouds casting shadows on ground)
    float shadowCloudDensity = GetCloudDensity(normal, Time, CLOUD_SHADOW_OFFSET);

    // Calculate lighting
    float3 litColor = CalculateLighting(input.WorldPos, normal, surfaceColor, lightDir, viewDir, cloudDensity, shadowCloudDensity);

    // Add atmosphere glow
    float3 atmosphere = GetAtmosphere(viewDir, normal, lightDir);
    float3 finalColor = litColor + atmosphere * ATMOSPHERE_STRENGTH;

    // Gamma correction
    finalColor = pow(abs(finalColor), 1.0 / 2.2);

    return float4(finalColor, 1.0);
}

//=============================================================================
// TECHNIQUE
//=============================================================================

technique PlanetShading
{
    pass P0
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
    }
}
