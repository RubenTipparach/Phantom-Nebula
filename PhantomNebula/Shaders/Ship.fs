#version 330

in vec3 fragWorldPos;
in vec3 fragNormal;
in vec3 fragViewDir;
in vec3 fragLightDir;
in vec2 fragTexCoord;

uniform sampler2D texture0;  // Albedo/diffuse map
uniform sampler2D texture1;  // Emissive map
uniform float time;

out vec4 finalColor;

const float PI = 3.14159265359;

void main()
{
    // Sample textures
    vec4 albedo = texture(texture0, fragTexCoord);
    vec4 emissive = texture(texture1, fragTexCoord);

    // Normalize vectors
    vec3 normal = normalize(fragNormal);
    vec3 viewDir = normalize(fragViewDir);
    vec3 lightDir = normalize(fragLightDir);

    // Diffuse lighting
    float ndotl = max(0.0, dot(normal, lightDir));
    vec3 diffuse = albedo.rgb * ndotl;

    // Ambient lighting
    vec3 ambient = albedo.rgb * 0.02;

    // Specular lighting (Blinn-Phong)
    vec3 halfVec = normalize(lightDir + viewDir);
    float spec = pow(max(0.0, dot(normal, halfVec)), 32.0);
    vec3 specular = vec3(1.0) * spec * 0.5;

    // Rim lighting for silhouette
    float rim = pow(1.0 - max(0.0, dot(viewDir, normal)), 2.0);
    vec3 rimLight = vec3(0.5, 0.7, 1.0) * rim * 0.01;

    // Combine lighting
    vec3 lit = ambient + diffuse  + specular + rimLight ;

    // Add emissive (glow from engine, etc)
    lit += albedo.rgb * emissive.rgb * 1.0;

    // Tone mapping and gamma correction
    lit = pow(lit, vec3(1.0 / 2.2));

    finalColor = vec4(lit, albedo.a);
}
