#version 330

in vec3 fragColor;  // Interpolated lighting from vertex shader (Gouraud)
in vec2 fragTexCoord;

uniform sampler2D texture0;  // Albedo/diffuse map
uniform sampler2D texture1;  // Emissive map

out vec4 finalColor;

void main()
{
    // Sample textures
    vec4 albedo = texture(texture0, fragTexCoord);
    vec4 emissive = texture(texture1, fragTexCoord);

    // Apply interpolated lighting to albedo (Gouraud shading)
    vec3 lit = albedo.rgb * fragColor;

    // Add emissive (glow from engine, etc)
    lit += albedo.rgb * emissive.rgb * 1.0;

    finalColor = vec4(lit, albedo.a);
}
