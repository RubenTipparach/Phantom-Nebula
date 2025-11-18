#version 330

// Input from vertex shader
in vec2 fragTexCoord;
in vec4 fragColor;

// Uniforms
uniform sampler2D texture0;
uniform float ditherPhase;
uniform float alpha;

// Output
out vec4 finalColor;

// Bayer matrix for ordered dithering (4x4)
const float bayerMatrix[16] = float[](
    0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
    12.0/16.0, 4.0/16.0, 14.0/16.0,  6.0/16.0,
    3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
    15.0/16.0, 7.0/16.0, 13.0/16.0,  5.0/16.0
);

void main()
{
    // Sample the explosion texture
    vec4 texColor = texture(texture0, fragTexCoord);

    // Use texture alpha as the base for transparency
    float texAlpha = texColor.a;

    // Combine texture color with fragment color (explosion tint)
    vec3 color = texColor.rgb * fragColor.rgb;

    // Apply dithering using Bayer matrix
    // Get the pixel coordinates in screen space
    ivec2 pixelCoord = ivec2(gl_FragCoord.xy);
    int ditherIndex = (pixelCoord.x % 4) + (pixelCoord.y % 4) * 4;
    float ditherValue = bayerMatrix[ditherIndex];

    // Apply dithering to alpha based on ditherPhase
    // This creates a stippled/dotted transparency effect
    float ditheredAlpha = texAlpha * alpha;

    // Use dither pattern to modulate the transparency
    // This creates an animated dithering effect when ditherPhase changes
    float ditherThreshold = mod(ditherPhase + ditherValue, 1.0);
    if (ditheredAlpha < ditherThreshold)
    {
        // Discard pixels below the dither threshold for animated dithering
        discard;
    }

    // Output the final color with the dithered alpha
    finalColor = vec4(color, ditheredAlpha);
}
