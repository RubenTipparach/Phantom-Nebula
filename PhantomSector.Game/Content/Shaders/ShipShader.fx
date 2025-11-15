#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0
    #define PS_SHADERMODEL ps_4_0
#endif

// Transformation matrices
float4x4 World;
float4x4 View;
float4x4 Projection;

// Lighting
float3 LightDirection = float3(1, -1, 1);
float3 LightColor = float3(0.7, 0.7, 0.7);
float3 AmbientColor = float3(0.3, 0.3, 0.3);

// Textures
texture DiffuseTexture;
sampler2D DiffuseSampler = sampler_state
{
    Texture = <DiffuseTexture>;
    MinFilter = Point;  // Pixelated look
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture EmissiveTexture;
sampler2D EmissiveSampler = sampler_state
{
    Texture = <EmissiveTexture>;
    MinFilter = Point;  // Pixelated look
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
};

// Vertex shader input
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

// Vertex shader output
struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float2 TexCoord : TEXCOORD1;
    float3 WorldPos : TEXCOORD2;
};

// Vertex Shader
VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.Normal = normalize(mul(input.Normal, (float3x3)World));
    output.TexCoord = input.TexCoord;
    output.WorldPos = worldPosition.xyz;

    return output;
}

// Pixel Shader
float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Sample textures with point filtering for pixelated look
    float4 diffuse = tex2D(DiffuseSampler, input.TexCoord);
    float4 emissive = tex2D(EmissiveSampler, input.TexCoord);

    // Normalize the interpolated normal
    float3 normal = normalize(input.Normal);

    // Directional lighting
    float3 lightDir = normalize(-LightDirection);
    float diffuseFactor = max(dot(normal, lightDir), 0.0);

    // Combine lighting
    float3 lighting = AmbientColor + (LightColor * diffuseFactor);

    // Apply lighting to diffuse texture
    float3 litColor = diffuse.rgb * lighting;

    // Add emissive (multiply brightness by grey pixel color)
    // Grey value acts as brightness multiplier
    float emissiveBrightness = (emissive.r + emissive.g + emissive.b) / 3.0;
    litColor += emissive.rgb * emissiveBrightness;

    return float4(litColor, diffuse.a);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
