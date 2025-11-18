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

// Bayer matrix for dithering (4x4)
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

    // Combine with fragment color (which contains the explosion color)
    vec4 color = texColor * fragColor;

    // Apply alpha
    color.a *= alpha;

    // Dither pattern for fizzle effect
    // Get pixel coordinates in screen/dither space
    vec2 ditherCoord = mod(gl_FragCoord.xy + ditherPhase * 8.0, 4.0);
    int ditherIndex = int(ditherCoord.y) * 4 + int(ditherCoord.x);
    ditherIndex = clamp(ditherIndex, 0, 15);

    // Apply dither threshold
    float ditherThreshold = bayerMatrix[ditherIndex];

    // Discard pixels based on dither pattern for fizzle effect
    // The dither pattern becomes more transparent over time
    if (color.a < ditherThreshold)
    {
        discard;
    }

    finalColor = color;
}
