#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBitangent;

out VS_OUT {
    vec2 UVPos;
    vec3 NormalPos;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
    vec3 FragPos;
} VS_OUT_OBJ;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Projection;

uniform vec3 ViewPos;

void main()
{
    mat3 ScalessModel = mat3(transpose(inverse(Model)));

    VS_OUT_OBJ.FragPos = vec3(vec4(aPosition, 1.0) * Model);
    VS_OUT_OBJ.UVPos = aTexCoord;

    vec3 Tangent = aTangent * ScalessModel;
    vec3 Bitangent = aBitangent * ScalessModel;
    vec3 Normal = aNormal * ScalessModel;

    VS_OUT_OBJ.NormalPos = Normal;

    mat3 TBN = mat3(Tangent, Bitangent, Normal);

    VS_OUT_OBJ.TangentViewPos = ViewPos * TBN;
    VS_OUT_OBJ.TangentFragPos = VS_OUT_OBJ.FragPos * TBN;

    gl_Position = vec4(aPosition, 1.0) * Model * View * Projection;
}