#version 330 core

in vec3 texPos;

in vec3 fragPos;
in vec3 fragNormal;

in vec3 objColor;
in vec3 lightColor;
in vec3 lightPos;
in vec3 viewPos;

uniform sampler2D texture0;

out vec4 fragColor;
out vec4 texColor;

void main()
{
	// AMBIENT
	float ambientPow = 0.15;
	vec3 ambient = ambientPow * lightColor;
	
	// DIFFUSE
	vec3 norm = normalize(fragNormal);
	vec3 lightDir = normalize(lightPos - fragPos);
	float diff = max(dot(norm, lightDir), 0);
	vec3 diffuse = diff * lightColor;
	
	// SPECULAR
	float specStr = 0.5;
	vec3 viewDir = normalize(viewPos - fragPos);
	vec3 reflectDir = reflect(-lightDir, norm);
	float spec = pow(max(dot(viewDir, reflectDir), 0), 32);
	vec3 specular = specStr * spec * lightColor;
	
	// RESULT
	vec3 result = (ambient + diffuse + specular) * objColor;
	fragColor = vec4(result, 1);
	
	// TEXTURE
	texColor = texture(texture0, vec2(texPos));
}