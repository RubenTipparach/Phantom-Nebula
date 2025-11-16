#version 330

// ============================================
// CONFIGURABLE PARAMETERS
// ============================================
#define PARALLAX_SCALE 0.01          // How much planet position affects dust position
#define DUST_DENSITY 0.4              // Scale of dust UV coordinates (lower = larger dust patches)
#define DUST_THRESHOLD_MIN 0.2        // Minimum noise value for dust particles
#define DUST_THRESHOLD_MAX 0.8        // Maximum noise value for smooth edge
#define DUST_ALPHA 0.2                // Overall dust opacity (0.0 - 1.0)
#define DUST_BRIGHTNESS_MIN 0.2       // Minimum brightness multiplier
#define DUST_BRIGHTNESS_MAX 0.8       // Maximum brightness multiplier
#define ANIMATION_SPEED 0.01          // Speed of dust animation over time
#define ANIMATION_BLEND 0.6           // How much animation affects static pattern
#define FBM_OCTAVES 6                 // Number of noise layers (more = more detail, slower)
#define DUST_COLOR vec3(0.9, 0.95, 1.0)  // Dust color tint (slightly blue-white)
#define SHIP_CLEARANCE_RADIUS 3.0    // Radius around ship where dust fades out
#define SHIP_CLEARANCE_FALLOFF 2.0   // Smoothness of the falloff edge
#define PLANE_FADE_RADIUS 100.0      // Distance from ship where plane starts to fade
#define PLANE_FADE_FALLOFF 400.0      // Smoothness of the radial fade

in vec2 fragTexCoord;
in vec3 fragWorldPos;

uniform vec3 planetPos;
uniform vec3 shipPos;
uniform float time;

out vec4 finalColor;

// Simple hash function for noise generation
float hash(vec2 p)
{
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

// 2D noise function
float noise(vec2 p)
{
    vec2 i = floor(p);
    vec2 f = fract(p);

    // Smooth interpolation
    f = f * f * (3.0 - 2.0 * f);

    // Four corners of the cell
    float a = hash(i);
    float b = hash(i + vec2(1.0, 0.0));
    float c = hash(i + vec2(0.0, 1.0));
    float d = hash(i + vec2(1.0, 1.0));

    // Bilinear interpolation
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

// Fractal Brownian Motion for layered noise
float fbm(vec2 p)
{
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;

    for (int i = 0; i < FBM_OCTAVES; i++)
    {
        value += amplitude * noise(p * frequency);
        frequency *= 2.0;
        amplitude *= 0.5;
    }

    return value;
}

void main()
{
    // Calculate UV offset based on planet position to create parallax effect
    // This makes dust appear stationary in world space
    vec2 parallaxOffset = planetPos.xz * PARALLAX_SCALE;

    // Scale UV coordinates for dust density
    vec2 dustUV = (fragWorldPos.xz + parallaxOffset) * DUST_DENSITY;

    // Generate noise pattern for dust
    float dustNoise = fbm(dustUV);

    // Add some animation with time
    float animatedNoise = fbm(dustUV + time * ANIMATION_SPEED);
    dustNoise = mix(dustNoise, animatedNoise, ANIMATION_BLEND);

    // Threshold to create sparse dust particles
    float dustMask = smoothstep(DUST_THRESHOLD_MIN, DUST_THRESHOLD_MAX, dustNoise);

    // Calculate distance from ship in XZ plane (ignore Y)
    vec2 shipPosXZ = shipPos.xz;
    vec2 fragPosXZ = fragWorldPos.xz;
    float distanceToShip = length(fragPosXZ - shipPosXZ);

    // Create clearance zone around ship with smooth falloff
    float clearanceMask = smoothstep(
        SHIP_CLEARANCE_RADIUS - SHIP_CLEARANCE_FALLOFF,
        SHIP_CLEARANCE_RADIUS + SHIP_CLEARANCE_FALLOFF,
        distanceToShip
    );

    // Create radial fade from ship center to hide plane edges
    float planeFadeMask = 1.0 - smoothstep(
        PLANE_FADE_RADIUS - PLANE_FADE_FALLOFF,
        PLANE_FADE_RADIUS + PLANE_FADE_FALLOFF,
        distanceToShip
    );

    // Apply both clearance mask and radial fade to dust
    dustMask *= clearanceMask * planeFadeMask;

    // Vary dust brightness
    float brightness = dustNoise * DUST_BRIGHTNESS_MAX + DUST_BRIGHTNESS_MIN;

    // Apply dust color tint with brightness
    vec3 dustColor = DUST_COLOR * brightness;

    // Output with alpha for transparency
    finalColor = vec4(dustColor, dustMask * DUST_ALPHA);
}
