// 3D Noise Planet Shader
// Generates procedural planet texture using 3D Perlin noise

float4x4 World;
float4x4 View;
float4x4 Projection;
float Time;
float3 CameraPosition;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 WorldPosition : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float2 TexCoord : TEXCOORD2;
};

// 3D Hash function
float3 hash33(float3 p)
{
    p = float3(dot(p, float3(127.1, 311.7, 74.7)),
               dot(p, float3(269.5, 183.3, 246.1)),
               dot(p, float3(113.5, 271.9, 124.6)));
    return frac(sin(p) * 43758.5453123);
}

// 3D Perlin-like noise
float noise3D(float3 p)
{
    float3 i = floor(p);
    float3 f = frac(p);
    f = f * f * (3.0 - 2.0 * f); // Smoothstep

    float n = 0.0;
    for (int z = 0; z <= 1; z++)
    {
        for (int y = 0; y <= 1; y++)
        {
            for (int x = 0; x <= 1; x++)
            {
                float3 g = float3(float(x), float(y), float(z));
                float3 o = hash33(i + g);
                float3 r = g - f + o;
                float d = dot(r, r);
                float w = exp(-d * 3.0);
                n += w * dot(hash33(i + g) - 0.5, f - g);
            }
        }
    }
    return n * 0.5 + 0.5;
}

// Fractional Brownian Motion (FBM) for layered noise
float fbm(float3 p, int octaves)
{
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;

    for (int i = 0; i < octaves; i++)
    {
        value += amplitude * noise3D(p * frequency);
        frequency *= 2.0;
        amplitude *= 0.5;
    }

    return value;
}

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.WorldPosition = worldPosition.xyz;
    output.Normal = normalize(mul(input.Normal, (float3x3)World));
    output.TexCoord = input.TexCoord;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Normalize the normal
    float3 normal = normalize(input.Normal);

    // Use world position as 3D texture coordinate
    float3 texCoord3D = input.WorldPosition * 0.2;

    // Add slow rotation to the noise over time
    float angle = Time * 0.1;
    float s = sin(angle);
    float c = cos(angle);
    float3x3 rotationMatrix = float3x3(
        c, 0, s,
        0, 1, 0,
        -s, 0, c
    );
    texCoord3D = mul(texCoord3D, rotationMatrix);

    // Generate multi-octave noise
    float noise1 = fbm(texCoord3D, 4);
    float noise2 = fbm(texCoord3D * 2.0 + float3(100.0, 100.0, 100.0), 4);

    // Combine noise values to create terrain-like features
    float heightMap = noise1 * 0.7 + noise2 * 0.3;

    // Define color palette for the planet
    float3 oceanColor = float3(0.1, 0.3, 0.6);   // Blue
    float3 landColor = float3(0.2, 0.6, 0.2);    // Green
    float3 mountainColor = float3(0.5, 0.4, 0.3); // Brown
    float3 snowColor = float3(0.9, 0.9, 0.95);   // White

    // Color based on height
    float3 baseColor;
    if (heightMap < 0.4)
    {
        // Ocean
        baseColor = oceanColor;
    }
    else if (heightMap < 0.5)
    {
        // Beach/Shore
        baseColor = lerp(oceanColor, landColor, (heightMap - 0.4) * 10.0);
    }
    else if (heightMap < 0.7)
    {
        // Land
        baseColor = landColor;
    }
    else if (heightMap < 0.85)
    {
        // Mountains
        baseColor = lerp(landColor, mountainColor, (heightMap - 0.7) / 0.15);
    }
    else
    {
        // Snow peaks
        baseColor = lerp(mountainColor, snowColor, (heightMap - 0.85) / 0.15);
    }

    // Add some detail variation
    float detail = noise3D(texCoord3D * 10.0) * 0.1;
    baseColor += detail;

    // Simple directional lighting
    float3 lightDir = normalize(float3(1.0, 1.0, 1.0));
    float diffuse = max(dot(normal, lightDir), 0.0);

    // Add ambient lighting
    float ambient = 0.3;
    float lighting = ambient + diffuse * 0.7;

    // Calculate view direction for specular
    float3 viewDir = normalize(CameraPosition - input.WorldPosition);
    float3 halfDir = normalize(lightDir + viewDir);
    float specular = pow(max(dot(normal, halfDir), 0.0), 32.0) * 0.3;

    // Ocean gets more specular
    if (heightMap < 0.4)
    {
        specular *= 2.0;
    }

    float3 finalColor = baseColor * lighting + specular;

    // Add atmospheric rim lighting
    float rim = 1.0 - max(dot(viewDir, normal), 0.0);
    rim = pow(rim, 3.0);
    float3 atmosphereColor = float3(0.4, 0.6, 1.0);
    finalColor += atmosphereColor * rim * 0.5;

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
