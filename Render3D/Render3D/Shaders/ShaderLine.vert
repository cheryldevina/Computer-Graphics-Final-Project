﻿#version 330 core
layout (location = 0) in vec3 aPosition;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Projection;

void main()
{
    gl_Position = vec4(aPosition, 1.0) * Model * View * Projection;
}