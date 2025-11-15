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
static const float TERRAIN_SCALE = 10.0;          // Higher = more detail
static const float CONTINENT_SCALE = 5.0;         // Scale of land masses
static const float LAND_DETAIL_SCALE = 50.0;      // Fine terrain detail
static const float LAND_THRESHOLD = 0.52;         // Lower = more land (0-1)
static const float COASTLINE_SHARPNESS = 0.02;    // How sharp coastlines are (lower = sharper)
static const float HEIGHTMAP_STRENGTH = 0.05;     // Terrain height variation for normal mapping
static const float HEIGHTMAP_DETAIL_CONTRIBUTION = 0.1; // How much fine detail affects height (0-1)
static const float3 CONTINENT_OFFSET = float3(0.5, 0.2, 0.8); // Seed for continent pattern

// Ice cap settings
static const float ICE_START = 0.83;              // Latitude where ice begins (0-1)
static const float ICE_END = 0.85;                // Latitude where ice is full (0-1)
static const float ICE_NOISE_SCALE = 32.0;        // Scale of noise for ice cap edges (higher = more detail)
static const float ICE_NOISE_STRENGTH = 0.3;     // How much noise affects ice cap boundary

// Cloud settings
static const float CLOUD_SCALE = 8.0;            // Cloud pattern scale
static const float CLOUD_SPEED = 0.0095;          // Cloud movement speed
static const float CLOUD_SHADOW_STRENGTH = 1.0;  // How dark cloud shadows are
static const float CLOUD_THRESHOLD_LOW = 0.35;   // Cloud density threshold
static const float CLOUD_THRESHOLD_HIGH = 0.95;
static const float CLOUD_POLE_START = 0.9;       // Latitude where clouds start reducing
static const float CLOUD_POLE_END = 0.6;         // Latitude where clouds are minimal
static const float3 CLOUD_SHADOW_OFFSET = float3(0.3, 0.0, 0.02); // Offset for cloud shadows on ground
static const float CLOUD_BRIGHTNESS = 0.9;       // Cloud opacity/blend amount
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
static const float AMBIENT_LIGHT = 0.01;         // Ambient light strength
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
// TECHNIQUE (Starfield reference - use custom Raylib implementation)
//=============================================================================

technique PlanetShading
{
    pass P0
    {
        // Raylib will use custom rendering pipeline
    }
}
