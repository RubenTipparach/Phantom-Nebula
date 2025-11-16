#version 330

//=============================================================================
// VERTEX SHADER - Starfield (Screen-space quad)
//=============================================================================

// Input vertex attributes from the quad mesh
in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec3 vertexNormal;
in vec4 vertexColor;

// Input uniform values
uniform mat4 mvp;
uniform mat4 matModel;
uniform vec3 cameraPosition;

// Output to fragment shader
out vec3 fragPosition;
out vec3 fragNormal;
out vec2 fragTexCoord;

void main()
{
    // Calculate world position
    vec4 worldPos = matModel * vec4(vertexPosition, 1.0);

    // Calculate view direction from camera to this point on the sphere
    fragPosition = normalize(worldPos.xyz - cameraPosition);
    fragNormal = normalize(vertexNormal);
    fragTexCoord = vertexTexCoord;

    // Transform vertex to projection space
    gl_Position = mvp * vec4(vertexPosition, 1.0);
}
