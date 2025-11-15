#version 330

in vec3 fragWorldPos;
in vec3 fragNormal;
in vec3 fragViewDir;

out vec4 finalColor;

uniform float time;
uniform vec3 sunPosition;

//=============================================================================
// TWEAKABLE CONSTANTS
//=============================================================================

const float PI = 3.14159265359;
const float PLANET_RADIUS = 1.0;
const float ATMOSPHERE_RADIUS = 1.15;

// Terrain detail settings
const float TERRAIN_SCALE = 10.0;
const float CONTINENT_SCALE = 5.0;
const float LAND_DETAIL_SCALE = 50.0;
const float LAND_THRESHOLD = 0.52;
const float COASTLINE_SHARPNESS = 0.02;
const float HEIGHTMAP_STRENGTH = 0.05;
const float HEIGHTMAP_DETAIL_CONTRIBUTION = 0.1;
const vec3 CONTINENT_OFFSET = vec3(0.5, 0.2, 0.8);

// Ice cap settings
const float ICE_START = 0.83;
const float ICE_END = 0.85;
const float ICE_NOISE_SCALE = 32.0;
const float ICE_NOISE_STRENGTH = 0.3;

// Cloud settings
const float CLOUD_SCALE = 8.0;
const float CLOUD_SPEED = 0.0095;
const float CLOUD_SHADOW_STRENGTH = 1.0;
const float CLOUD_THRESHOLD_LOW = 0.35;
const float CLOUD_THRESHOLD_HIGH = 0.95;
const float CLOUD_POLE_START = 0.9;
const float CLOUD_POLE_END = 0.6;
const vec3 CLOUD_SHADOW_OFFSET = vec3(0.3, 0.0, 0.02);
const float CLOUD_BRIGHTNESS = 0.9;
const float CLOUD_MIN_LIGHTING = 0.1;

// Spiral distortion settings
const float SPIRAL_AMOUNT = 0.3;
const float SPIRAL_OFFSET_SCALE = 1.1;
const float SPIRAL_BLEND_POWER = 1.5;

// Atmosphere settings
const float ATMOSPHERE_RIM_POWER = 2.0;
const float ATMOSPHERE_SCATTER_MIN = 0.5;
const float ATMOSPHERE_SCATTER_MAX = 0.5;
const float ATMOSPHERE_STRENGTH = 0.3;

// Lighting settings
const float AMBIENT_LIGHT = 0.01;
const float OCEAN_SPECULAR_POWER = 32.0;
const float OCEAN_SPECULAR_STRENGTH = 0.5;
const float OCEAN_MASK_THRESHOLD = 0.2;

// Planet colors
const vec3 OCEAN_COLOR = vec3(0.02, 0.1, 0.3);
const vec3 LAND_COLOR1 = vec3(0.1, 0.3, 0.05);
const vec3 LAND_COLOR2 = vec3(0.3, 0.25, 0.1);
const vec3 ICE_COLOR = vec3(0.9, 0.95, 1.0);
const vec3 CLOUD_COLOR = vec3(1.0, 1.0, 1.0);
const vec3 ATMOSPHERE_COLOR = vec3(0.3, 0.5, 0.9);

//=============================================================================
// HASH FUNCTIONS
//=============================================================================

float hash(float n)
{
    return fract(sin(n) * 43758.5453123);
}

float hash2(vec2 p)
{
    return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453);
}

float hash3(vec3 p)
{
    p = fract(p * vec3(0.1031, 0.1030, 0.0973));
    p += dot(p, p.yzx + vec3(19.19));
    return fract((p.x + p.y) * p.z);
}

//=============================================================================
// NOISE FUNCTIONS
//=============================================================================

float noise(vec3 x)
{
    vec3 p = floor(x);
    vec3 f = fract(x);
    f = f * f * (3.0 - 2.0 * f);

    float n = p.x + p.y * 157.0 + 113.0 * p.z;

    return mix(
        mix(
            mix(hash(n + 0.0), hash(n + 1.0), f.x),
            mix(hash(n + 157.0), hash(n + 158.0), f.x),
            f.y
        ),
        mix(
            mix(hash(n + 113.0), hash(n + 114.0), f.x),
            mix(hash(n + 270.0), hash(n + 271.0), f.x),
            f.y
        ),
        f.z
    );
}

// Fractional Brownian Motion
float fbm(vec3 p)
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

vec2 Spiral(vec2 uv, float spiralAmount)
{
    vec2 center = vec2(0.5, 0.5);
    vec2 delta = uv - center;
    float dist = length(delta);

    // Offset perpendicular to radius
    vec2 offset = vec2(delta.y, -delta.x);

    // Blend based on distance from center
    float blend = pow(clamp((0.5 - dist) * 2.0, 0.0, 1.0), SPIRAL_BLEND_POWER);

    return uv + offset * SPIRAL_OFFSET_SCALE * spiralAmount * blend;
}

//=============================================================================
// HEIGHTMAP
//=============================================================================

float GetHeight(vec3 normal)
{
    // Continental shapes for base elevation
    float continents = fbm(normal * CONTINENT_SCALE + CONTINENT_OFFSET);

    // Fine detail on top
    float detail = fbm(normal * LAND_DETAIL_SCALE);

    // Combine for final height
    float height = continents * HEIGHTMAP_STRENGTH;
    height += detail * HEIGHTMAP_STRENGTH * HEIGHTMAP_DETAIL_CONTRIBUTION;

    return height;
}

// Calculate normal from heightmap using finite differences
vec3 CalculateTerrainNormal(vec3 surfaceNormal, float landMask)
{
    // Only apply terrain normals to land areas
    if (landMask < 0.1)
    {
        return surfaceNormal;
    }

    // Small offset for sampling neighboring heights
    float epsilon = 0.001;

    // Create tangent and bitangent vectors
    vec3 tangent = normalize(cross(surfaceNormal, vec3(0.0, 1.0, 0.0)));
    if (length(tangent) < 0.1)
    {
        tangent = normalize(cross(surfaceNormal, vec3(1.0, 0.0, 0.0)));
    }
    vec3 bitangent = normalize(cross(surfaceNormal, tangent));

    // Sample heights at neighboring points
    float heightCenter = GetHeight(surfaceNormal);
    float heightRight = GetHeight(normalize(surfaceNormal + tangent * epsilon));
    float heightUp = GetHeight(normalize(surfaceNormal + bitangent * epsilon));

    // Calculate gradients
    float dx = (heightRight - heightCenter) / epsilon;
    float dy = (heightUp - heightCenter) / epsilon;

    // Perturb the surface normal
    vec3 perturbedNormal = normalize(surfaceNormal - tangent * dx - bitangent * dy);

    // Blend between smooth ocean and terrain normals
    return mix(surfaceNormal, perturbedNormal, landMask);
}

//=============================================================================
// PLANET SURFACE
//=============================================================================

vec3 GetPlanetColor(vec3 normal, vec3 worldPos, out float height, out float landMask)
{
    // Convert normal to spherical UV coordinates
    vec2 uv;
    uv.x = (atan(normal.x, normal.z) + PI) / (PI * 2.0);
    uv.y = acos(normal.y) / PI;

    // Get height for this position
    height = GetHeight(normal);

    // Base terrain noise
    vec3 noisePos = normal * TERRAIN_SCALE;
    float terrain = fbm(noisePos);

    // Continental shapes
    float continents = fbm(normal * CONTINENT_SCALE + CONTINENT_OFFSET);

    // Determine if land or ocean
    landMask = smoothstep(LAND_THRESHOLD - COASTLINE_SHARPNESS,
                          LAND_THRESHOLD + COASTLINE_SHARPNESS,
                          continents);

    // Land color variation
    float landDetail = fbm(normal * LAND_DETAIL_SCALE);
    vec3 landColor = mix(LAND_COLOR1, LAND_COLOR2, landDetail);

    // Ocean with depth variation
    float oceanDepth = (1.0 - continents) * 0.5;
    vec3 oceanColor = OCEAN_COLOR * (1.0 - oceanDepth * 0.5);

    // Mix land and ocean
    vec3 surfaceColor = mix(oceanColor, landColor, landMask);

    // Ice caps at poles
    float poleAmount = abs(normal.y);

    // Add noise to ice cap boundary
    float iceNoise = fbm(normal * ICE_NOISE_SCALE);
    float noisyPoleAmount = poleAmount + (iceNoise - 0.5) * ICE_NOISE_STRENGTH;

    float iceMask = smoothstep(ICE_START, ICE_END, noisyPoleAmount);
    surfaceColor = mix(surfaceColor, ICE_COLOR, iceMask);

    return surfaceColor;
}

//=============================================================================
// CLOUDS
//=============================================================================

float GetCloudDensity(vec3 normal, float timeVal, vec3 offset)
{
    // Rotate clouds around the planet over time
    float rotationAngle = timeVal * CLOUD_SPEED;
    float cosAngle = cos(rotationAngle);
    float sinAngle = sin(rotationAngle);

    // Rotate normal around Y axis
    vec3 rotatedNormal;
    rotatedNormal.x = normal.x * cosAngle - normal.z * sinAngle;
    rotatedNormal.y = normal.y;
    rotatedNormal.z = normal.x * sinAngle + normal.z * cosAngle;

    // Spiral distorted UV
    vec2 uv;
    uv.x = (atan(rotatedNormal.x, rotatedNormal.z) + PI) / (PI * 2.0);
    uv.y = acos(rotatedNormal.y) / PI;

    // Apply spiral distortion
    vec2 spiralUV = Spiral(uv, SPIRAL_AMOUNT);

    // Add morphing over time
    vec3 morphOffset = vec3(0.0, 0.0, timeVal * CLOUD_SPEED * 0.3);

    // Multi-layer clouds
    float clouds = fbm(rotatedNormal * CLOUD_SCALE + morphOffset + offset);
    clouds = smoothstep(CLOUD_THRESHOLD_LOW, CLOUD_THRESHOLD_HIGH, clouds);

    // Reduce clouds at poles
    float poleAmount = abs(rotatedNormal.y);
    clouds *= smoothstep(CLOUD_POLE_START, CLOUD_POLE_END, poleAmount);

    return clouds;
}

//=============================================================================
// ATMOSPHERE
//=============================================================================

vec3 GetAtmosphere(vec3 rayDir, vec3 normal, vec3 lightDir)
{
    // Fresnel-like rim lighting
    float rim = 1.0 - max(0.0, dot(rayDir, normal));
    rim = pow(rim, ATMOSPHERE_RIM_POWER);

    // Atmospheric scattering
    float scatter = max(0.0, dot(normal, lightDir));

    // Only show atmosphere on sunlit side
    float atmosphereVisibility = smoothstep(-0.2, 0.2, dot(normal, lightDir));

    return ATMOSPHERE_COLOR * rim * (ATMOSPHERE_SCATTER_MIN + ATMOSPHERE_SCATTER_MAX * scatter) * atmosphereVisibility;
}

//=============================================================================
// LIGHTING
//=============================================================================

vec3 CalculateLighting(vec3 worldPos, vec3 normal, vec3 surfaceColor, vec3 lightDir, vec3 viewDir, float cloudDensity, float shadowCloudDensity)
{
    // Diffuse lighting
    float ndotl = max(0.0, dot(normal, lightDir));

    // Cloud shadows
    float cloudShadow = 1.0 - (shadowCloudDensity * CLOUD_SHADOW_STRENGTH);
    vec3 diffuse = surfaceColor * ndotl * cloudShadow;

    // Ambient
    vec3 ambient = surfaceColor * AMBIENT_LIGHT * cloudShadow;

    // Specular
    vec3 halfVec = normalize(lightDir + viewDir);
    float spec = pow(max(0.0, dot(normal, halfVec)), OCEAN_SPECULAR_POWER);

    // Only apply specular to ocean areas
    float oceanMask = step(length(surfaceColor - OCEAN_COLOR), OCEAN_MASK_THRESHOLD);
    vec3 specular = vec3(1.0, 1.0, 1.0) * spec * oceanMask * OCEAN_SPECULAR_STRENGTH;

    // Combine lighting
    vec3 lit = ambient + diffuse + specular;

    // Apply clouds
    vec3 cloudColor = CLOUD_COLOR * max(CLOUD_MIN_LIGHTING, ndotl);
    lit = mix(lit, cloudColor, cloudDensity * CLOUD_BRIGHTNESS);

    return lit;
}

//=============================================================================
// MAIN
//=============================================================================

void main()
{
    vec3 normal = normalize(fragNormal);
    vec3 viewDir = normalize(fragViewDir);

    // Calculate light direction
    vec3 lightDir = normalize(sunPosition - fragWorldPos);

    // Get base planet surface color
    float height;
    float landMask;
    vec3 surfaceColor = GetPlanetColor(normal, fragWorldPos, height, landMask);

    // Get cloud density
    float cloudDensity = GetCloudDensity(normal, time, vec3(0.0, 0.0, 0.0));

    // Calculate terrain normal from heightmap
    vec3 terrainNormal = CalculateTerrainNormal(normal, landMask);

    // Blend between smooth and terrain normal
    vec3 finalNormal = mix(terrainNormal, normal, cloudDensity);

    // Get offset cloud density for shadows
    float shadowCloudDensity = GetCloudDensity(normal, time, CLOUD_SHADOW_OFFSET);

    // Calculate lighting
    vec3 litColor = CalculateLighting(fragWorldPos, finalNormal, surfaceColor, lightDir, viewDir, cloudDensity, shadowCloudDensity);

    // Add atmosphere glow
    vec3 atmosphere = GetAtmosphere(viewDir, normal, lightDir);
    vec3 finalColorOut = litColor + atmosphere * ATMOSPHERE_STRENGTH;

    // Gamma correction
    finalColorOut = pow(abs(finalColorOut), vec3(1.0 / 2.2));

    finalColor = vec4(finalColorOut, 1.0);
}
