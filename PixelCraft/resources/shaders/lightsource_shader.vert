#version 330 core

// vertex
in vec3 vPosition;
in vec3 texPosition;
in vec3 vNormal;
in vec3 iLocation;
in vec3 iSize;
in vec3 iRotation;
in vec3 iColor;

uniform mat4 model;
uniform mat4 view_translate;
uniform mat4 view_rotate;
uniform mat4 projection;

uniform vec3 sunPosition;
uniform vec3 pPosition;

out vec3 texPos;

out vec3 fragPos;
out vec3 fragNormal;

out vec3 objColor;
out vec3 lightColor;
out vec3 lightPos;
out vec3 viewPos;

void main()
{
	mat4 scale = mat4(
		iSize.x, 0, 0, 0,
		0, iSize.y, 0, 0,
		0, 0, iSize.z, 0,
		0, 0, 0, 1
	);
		
	mat4 move = mat4(
		1, 0, 0, iLocation.x,
		0, 1, 0, iLocation.y,
		0, 0, 1, iLocation.z,
		0, 0, 0, 1
	);

	vec4 obj = vec4(vPosition, 1) * scale * move;	
	gl_Position = obj * model * view_translate * view_rotate * projection;

	texPos = vec3(0, 0, 0);
	
	fragPos = vec3(vec4(vPosition, 1) * model);
	fragNormal = vNormal * mat3(transpose(inverse(model)));	
	
	objColor = iColor;
	lightColor = vec3(1, 1, 1);
	lightPos = vec3(-50, 10, -50);
	viewPos = pPosition;
}