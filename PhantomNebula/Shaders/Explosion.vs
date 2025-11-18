#version 330

// Input vertex attributes
in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec4 vertexColor;

// Uniforms
uniform mat4 mvp;

// Output to fragment shader
out vec2 fragTexCoord;
out vec4 fragColor;

void main()
{
    // Apply MVP transformation (scaling is handled by the transformation matrix in C#)
    gl_Position = mvp * vec4(vertexPosition, 1.0);

    // Pass texture coordinates and color to fragment shader
    fragTexCoord = vertexTexCoord;
    fragColor = vertexColor;
}
