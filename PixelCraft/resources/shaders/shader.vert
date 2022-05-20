#version 330 core

// vertex
layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec3 vNormal;
layout (location = 2) in vec4 vColor;
layout (location = 3) in vec2 tCoord;

uniform mat4 obj_translate;
uniform mat4 obj_scale;
uniform mat4 obj_rotate;

uniform mat4 model;
uniform mat4 view_translate;
uniform mat4 view_rotate;
uniform mat4 projection;

uniform vec3 player_position;

out vec3 fragPos;
out vec3 fragNormal;

out vec2 texCoord;

out vec4 objColor;
out vec3 lightColor;
out vec3 lightPos;
out vec3 viewPos;

void main()
{
	vec4 obj = vec4(vPosition, 1) * obj_scale * obj_rotate * obj_translate;
	gl_Position = obj * model * view_translate * view_rotate * projection;

	
	fragPos = vec3(vec4(vPosition, 1) * model);
	fragNormal = vNormal * mat3(transpose(inverse(model)));
	
	texCoord = tCoord;
	
	objColor = vColor;
	lightColor = vec3(1, 1, 1);
	lightPos = vec3(-50, 10, -50);
	viewPos = player_position;
}