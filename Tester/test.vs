
layout(location = 0) in vec3 in_position;
layout(location = 1) in vec2 in_uv;
layout(location = 2) in vec3 in_normal;

uniform mat4 ModelMatrix;
uniform mat4 ViewMatrix;
uniform mat4 ProjectionMatrix;
uniform vec3 CameraPosition;
uniform float Time;
uniform vec3 input_Color;

out vec3 normal;
out vec4 positionWorldSpace;
out vec3 positionModelSpace;


float hash3(vec3 uv) {
	return fract(sin(uv.x * 15.78 + uv.y * 35.14 + uv.z * 26.1134) * 43758.23);
}


void main(){

    vec4 v = vec4(in_position,1);
    vec4 n = vec4(in_normal,0);
	mat4 mvp = ProjectionMatrix * ViewMatrix * ModelMatrix;
    gl_Position = mvp * v;
	positionWorldSpace = ModelMatrix * v;
	positionModelSpace = in_position;
	normal = (ProjectionMatrix * ModelMatrix * n).xyz;
}