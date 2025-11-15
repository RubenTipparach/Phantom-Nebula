#version 330

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 vertexTexCoord;

uniform mat4 mvp;
uniform mat4 matModel;
uniform vec3 cameraPos;

out vec3 fragWorldPos;
out vec3 fragNormal;
out vec3 fragViewDir;

void main()
{
    vec4 worldPos = matModel * vec4(vertexPosition, 1.0);
    fragWorldPos = worldPos.xyz;
    fragNormal = normalize(mat3(matModel) * vertexNormal);
    fragViewDir = normalize(cameraPos - worldPos.xyz);
    gl_Position = mvp * vec4(vertexPosition, 1.0);
}
