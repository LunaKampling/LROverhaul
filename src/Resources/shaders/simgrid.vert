#version 120
varying vec2 world_position;
varying vec2 pixel_coord;

void main() 
{
	gl_Position = gl_ProjectionMatrix * gl_Vertex; //Vertex position is not translated/scaled as it's just used to cover the screen, so only use projection matrix
    vec4 position = gl_ModelViewMatrix * vec4(gl_Vertex.xy, 0, 1); //Position of the vertex in the track 'world'
	world_position = position.xy;
	pixel_coord = gl_Vertex.xy; //The pixel coordinate of the vertex (to be used in frag shader to determine line location)
}