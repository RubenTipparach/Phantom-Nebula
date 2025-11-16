#version 330

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 vertexTexCoord;

uniform mat4 mvp;
uniform mat4 matModel;
uniform vec3 cameraPos;
uniform vec3 lightDir;

out vec3 fragColor;  // Interpolated color (Gouraud)
out vec2 fragTexCoord;

void main()
{
    vec4 worldPos = matModel * vec4(vertexPosition, 1.0);
    vec3 normal = normalize(mat3(matModel) * vertexNormal);
    vec3 viewDir = normalize(cameraPos - worldPos.xyz);
    vec3 lightDirection = normalize(lightDir);

    // Gouraud shading - calculate lighting per vertex
    // Diffuse
    float ndotl = max(0.0, dot(normal, lightDirection));
    vec3 diffuse = vec3(1.0) * ndotl;

    // Ambient
    vec3 ambient = vec3(0.1);

    // Specular (Blinn-Phong)
    vec3 halfVec = normalize(lightDirection + viewDir);
    float spec = pow(max(0.0, dot(normal, halfVec)), 32.0);
    vec3 specular = vec3(1.0) * spec * 0.5;

    // Combine lighting (to be interpolated)
    fragColor = ambient + diffuse + specular;

    fragTexCoord = vertexTexCoord;
    gl_Position = mvp * vec4(vertexPosition, 1.0);
}
