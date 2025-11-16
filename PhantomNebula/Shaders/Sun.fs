#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Output fragment color
out vec4 finalColor;

// Uniforms
uniform float time;

// Simple noise function
float hash(vec2 p)
{
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float noise(vec2 p)
{
    vec2 i = floor(p);
    vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);

    float a = hash(i);
    float b = hash(i + vec2(1.0, 0.0));
    float c = hash(i + vec2(0.0, 1.0));
    float d = hash(i + vec2(1.0, 1.0));

    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

void main()
{
    // Center coordinates (-0.5 to 0.5)
    vec2 uv = fragTexCoord - 0.5;

    // Distance from center
    float dist = length(uv) * 2.0;

    // Sun core (bright center)
    float core = 1.0 - smoothstep(0.0, 0.3, dist);

    // Sun glow (soft outer glow)
    float glow = 1.0 - smoothstep(0.0, 1.0, dist);
    glow = pow(glow, 2.0);

    // Add some animated noise for corona effect
    float angle = atan(uv.y, uv.x);
    float corona = noise(vec2(angle * 8.0, dist * 10.0 + time * 0.5)) * 0.3;
    corona *= smoothstep(0.2, 0.5, dist) * smoothstep(1.0, 0.6, dist);

    // Combine effects
    float brightness = core * 2.0 + glow + corona;

    // Sun color (yellow-orange)
    vec3 sunColor = mix(
        vec3(1.0, 0.9, 0.6),  // Outer corona (yellow)
        vec3(1.0, 1.0, 1.0),  // Core (white)
        core
    );

    // Apply brightness
    vec3 color = sunColor * brightness;

    // Alpha based on glow
    float alpha = clamp(brightness, 0.0, 1.0);

    finalColor = vec4(color, alpha);
}
