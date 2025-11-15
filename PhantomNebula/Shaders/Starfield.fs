#version 330

//=============================================================================
// FRAGMENT SHADER - Voronoi Starfield (ported from PhantomSector)
//=============================================================================

in vec3 fragPosition;
in vec3 fragNormal;

out vec4 finalColor;

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
// MAIN
//=============================================================================

void main()
{
    // Use fragment position as direction vector (already normalized from vertex shader)
    vec3 dir = fragPosition;

    // Generate starfield with three density layers
    float stars = 0.0;
    stars += starfield3D(dir, 1.0, StarDensityLayer1) * Layer1Intensity;
    stars += starfield3D(dir, 2.0, StarDensityLayer2) * Layer2Intensity;
    stars += starfield3D(dir, 3.0, StarDensityLayer3) * Layer3Intensity;

    // Composite starfield with space background
    vec3 color = SpaceColor + stars * StarColor;
    finalColor = vec4(color, 1.0);
}
