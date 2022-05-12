#version 330 core

// vertex
layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec3 vNormal;
layout (location = 2) in vec4 vColor;
layout (location = 3) in vec2 tCoord;

uniform mat4 obj_translate;

out vec2 texCoord;

void main()
{
	gl_Position = vec4(vPosition, 1f) * obj_translate;	
	texCoord = tCoord;	
}