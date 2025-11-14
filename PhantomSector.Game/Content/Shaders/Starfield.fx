// Voronoi Starfield Shader
// Creates a static starfield using Voronoi/Worley noise mapped to a sphere

float4x4 InverseViewProjection;
float3 CameraPosition;

// Hash function for pseudo-random numbers
float2 hash22(float2 p)
{
    p = float2(dot(p, float2(127.1, 311.7)),
               dot(p, float2(269.5, 183.3)));
    return frac(sin(p) * 43758.5453);
}

// Voronoi/Worley noise function (now takes 2D UV on sphere)
float voronoi(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);

    float minDist = 1.0;

    // Check 3x3 grid of cells
    for (int x = -1; x <= 1; x++)
    {
        for (int y = -1; y <= 1; y++)
        {
            float2 neighbor = float2(float(x), float(y));
            float2 cellPoint = hash22(i + neighbor);

            // Static points - no animation
            cellPoint = 0.5 + 0.5 * sin(6.2831 * cellPoint);

            float2 diff = neighbor + cellPoint - f;
            float dist = length(diff);

            minDist = min(minDist, dist);
        }
    }

    return minDist;
}

// Convert 3D direction to spherical coordinates (for mapping to sphere)
float2 directionToUV(float3 dir)
{
    float phi = atan2(dir.z, dir.x);
    float theta = acos(dir.y);
    return float2(phi / 6.28318530718, theta / 3.14159265359);
}

// Generate stars with multiple layers
float starfield(float2 uv, float layer)
{
    // Scale and offset each layer
    float2 offset = float2(layer * 123.45, layer * 678.90);
    float2 scaledUV = uv * (10.0 + layer * 5.0) + offset;

    // No scrolling - static stars
    float voronoiValue = voronoi(scaledUV);

    // Create sharp star points
    float star = smoothstep(0.1, 0.0, voronoiValue);

    // Add some variation in brightness
    float2 cellId = floor(scaledUV);
    float brightness = hash22(cellId).x;

    return star * brightness;
}

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 ScreenPosition : TEXCOORD0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = input.Position;
    output.ScreenPosition = input.Position;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Reconstruct world-space view direction
    float2 screenPos = input.ScreenPosition.xy / input.ScreenPosition.w;
    float4 worldPos = mul(float4(screenPos, 1.0, 1.0), InverseViewProjection);
    worldPos /= worldPos.w;

    // Direction from camera to point on far plane
    float3 viewDir = normalize(worldPos.xyz - CameraPosition);

    // Convert direction to spherical UV coordinates
    float2 uv = directionToUV(viewDir);

    // Create multiple layers of stars
    float stars = 0.0;
    stars += starfield(uv, 1.0) * 1.0;  // Bright layer
    stars += starfield(uv, 2.0) * 0.7;  // Medium layer
    stars += starfield(uv, 3.0) * 0.5;  // Dim layer

    // Add some blue/purple tint to space
    float3 spaceColor = float3(0.02, 0.01, 0.05);
    float3 starColor = float3(1.0, 0.95, 0.9);

    float3 finalColor = spaceColor + stars * starColor;

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
