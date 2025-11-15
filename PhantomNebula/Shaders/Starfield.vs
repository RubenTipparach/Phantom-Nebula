#version 330

//=============================================================================
// VERTEX SHADER - Starfield (Sphere-based skybox)
//=============================================================================

// Input vertex attributes from the sphere mesh
in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec3 vertexNormal;
in vec4 vertexColor;

// Input uniform values
uniform mat4 mvp;

// Output to fragment shader
out vec3 fragPosition;
out vec3 fragNormal;

void main()
{
    // Transform vertex position to world space and pass to fragment shader
    fragPosition = vertexPosition;
    fragNormal = normalize(vertexNormal);

    // Transform vertex to projection space
    gl_Position = mvp * vec4(vertexPosition, 1.0);
}
