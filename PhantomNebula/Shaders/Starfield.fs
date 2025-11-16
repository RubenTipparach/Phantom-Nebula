#version 330

//=============================================================================
// FRAGMENT SHADER - Voronoi Starfield (ported from PhantomSector)
//=============================================================================

in vec3 fragPosition;
in vec3 fragNormal;

out vec4 finalColor;

// Uniforms
uniform vec3 lightDirection;
uniform float time;

//=============================================================================
// TWEAKABLE PARAMETERS
//=============================================================================

const float StarDensityLayer1 = 100.0;
const float StarDensityLayer2 = 200.0;
const float StarDensityLayer3 = 300.0;

const float StarSharpness = 0.02;
const float StarFalloff = 0.0;

const float StarBrightnessThreshold = 0.7;
const float StarBrightnessMultiplier = 1.0;

const float Layer1Intensity = 1.0;
const float Layer2Intensity = 0.8;
const float Layer3Intensity = 0.6;

const vec3 SpaceColor = vec3(0.0, 0.0, 0.0);
const vec3 StarColor = vec3(1.0, 0.98, 0.95);

// Sun parameters
const float SunSize = 0.02;
const float SunGlowSize = 0.1;
const vec3 SunCoreColor = vec3(1.0, 1.0, 1.0);
const vec3 SunGlowColor = vec3(1.0, 0.9, 0.6);
const float CoronaBrightness = 0.2;
const float CoronaTransparency = 0.3;  // 0.0 = fully transparent, 1.0 = fully opaque

//=============================================================================
// HASH FUNCTIONS
//=============================================================================

float hash(float x)
{
    return fract(x + 1.3215 * 1.8152);
}

float hash3(vec3 a)
{
    return fract((hash(a.z * 42.8883) + hash(a.y * 36.9125) + hash(a.x * 65.4321)) * 291.1257);
}

vec3 rehash3(float x)
{
    return vec3(
        hash(((x + 0.5283) * 59.3829) * 274.3487),
        hash(((x + 0.8192) * 83.6621) * 345.3871),
        hash(((x + 0.2157) * 36.6521) * 458.3971)
    );
}

float sqr(float x)
{
    return x * x;
}

float fastdist(vec3 a, vec3 b)
{
    return sqr(b.x - a.x) + sqr(b.y - a.y) + sqr(b.z - a.z);
}

// Smooth noise function with interpolation
float smoothNoise(float x)
{
    float i = floor(x);
    float f = fract(x);

    // Smooth interpolation (smoothstep)
    float u = f * f * (3.0 - 2.0 * f);

    // Sample noise at integer positions and interpolate
    float a = hash(i);
    float b = hash(i + 1.0);

    return mix(a, b, u);
}

//=============================================================================
// VORONOI
//=============================================================================

void voronoi3D_float(vec3 pos, float density, out float Out, out float Cells)
{
    vec4 p[27];
    pos *= density;
    float x = pos.x;
    float y = pos.y;
    float z = pos.z;

    int idx = 0;
    for (int _x = -1; _x < 2; _x++)
    {
        for (int _y = -1; _y < 2; _y++)
        {
            for(int _z = -1; _z < 2; _z++)
            {
                vec3 _p = floor(pos) + vec3(float(_x), float(_y), float(_z));
                float h = hash3(_p);
                p[idx] = vec4((rehash3(h) + _p), h);
                idx++;
            }
        }
    }

    float m = 9999.9999;
    float w = 0.0;

    for (int i = 0; i < 27; i++)
    {
        float d = fastdist(vec3(x, y, z), p[i].xyz);
        if(d < m)
        {
            m = d;
            w = p[i].w;
        }
    }

    Out = m;
    Cells = w;
}

//=============================================================================
// STARFIELD
//=============================================================================

float starfield3D(vec3 dir, float layer, float density)
{
    float voronoiDist, cellId;
    voronoi3D_float(dir, density, voronoiDist, cellId);

    float star = smoothstep(StarSharpness, StarFalloff, voronoiDist);
    float brightness = fract(cellId * 123.456);
    star *= step(StarBrightnessThreshold, brightness);

    return star * brightness * StarBrightnessMultiplier;
}

//=============================================================================
// SUN
//=============================================================================

vec3 renderSun(vec3 dir, vec3 lightDir)
{
    // lightDir is the direction TO the sun (where sun is located)
    vec3 sunDir = vec3(lightDir.x, lightDir.y, lightDir.z);

    // Calculate angle between view direction and sun direction
    float sunDot = dot(dir, sunDir);

    // Sun core (bright center)
    float core = smoothstep(1.0 - SunSize, 1.0, sunDot);
    core = pow(core, 3.0);

    // Sun glow (soft outer glow)
    float glow = smoothstep(1.0 - SunGlowSize, 1.0, sunDot);
    glow = pow(glow, 2.0);

    // Add animated corona noise
    // Calculate perpendicular vector to sun direction for radial pattern
    vec3 perpendicular = normalize(cross(sunDir, vec3(0.0, 1.0, 0.0)));
    if (length(perpendicular) < 0.01) {
        perpendicular = normalize(cross(sunDir, vec3(1.0, 0.0, 0.0)));
    }
    vec3 tangent = normalize(cross(sunDir, perpendicular));

    // Project view direction onto the plane perpendicular to sun
    float radialX = dot(dir, perpendicular);
    float radialY = dot(dir, tangent);
    float angle = atan(radialY, radialX);

    // Create smooth corona variation using multiple octaves of interpolated noise
    float corona1 = smoothNoise(angle * 4.0);
    //float corona2 = smoothNoise(angle * 8.0);
    //float corona3 = smoothNoise(angle * 16.0);

    // Combine octaves for organic variation
    float coronaPattern = corona1 * 0.5;// + corona2 * 0.3 + corona3 * 0.2;

    float corona = coronaPattern * CoronaBrightness;
    corona *= smoothstep(1.0 - SunGlowSize, 1.0 - SunSize * 0.5, sunDot);
    corona *= smoothstep(1.0, 1.0 - SunSize * 2.0, sunDot);
    corona *= CoronaTransparency;

    // Combine effects
    vec3 sunColor = mix(SunGlowColor, SunCoreColor, core);
    float brightness = core * 3.0 + glow + corona;

    return sunColor * brightness;
}

//=============================================================================
// MAIN
//=============================================================================

void main()
{
    // Use fragment position as direction vector (already normalized from vertex shader)
    vec3 dir = normalize(fragPosition);

    // Generate starfield with three density layers
    float stars = 0.0;
    stars += starfield3D(dir, 1.0, StarDensityLayer1) * Layer1Intensity;
    stars += starfield3D(dir, 2.0, StarDensityLayer2) * Layer2Intensity;
    stars += starfield3D(dir, 3.0, StarDensityLayer3) * Layer3Intensity;

    // Render sun
    vec3 sunColor = renderSun(dir, normalize(lightDirection));

    // Composite starfield + sun with space background
    vec3 color = SpaceColor + stars * StarColor + sunColor;
    finalColor = vec4(color, 1.0);
}
