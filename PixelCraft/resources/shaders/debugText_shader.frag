#version 330 core

in vec2 texCoord;
in float texAlpha;

uniform sampler2D texture0;

out vec4 fragColor;

void main()
{
	fragColor = texture(texture0, texCoord) * vec4(1.0, 1.0, 1.0, texAlpha);
}