#version 120 /*TODO MAKE LINE COLOURS UNIFORMS*/

uniform float u_zoom; // The current track zoom (needed to calculate the separation of neighbouring pixels)

varying vec2 world_position;
varying vec2 pixel_coord;

float sign(float val){
	return val/abs(val);
}

vec2 sign(vec2 val){
	return val/abs(val);
}

float grid_alph(vec2 pos, float res) // Returns (one minus) the alpha value of a given position based on its proximity to the grid
{
	vec2 grid_low = sign(pos) * vec2(pow(2.0, floor(log2(abs(pos.x)))), pow(2.0, floor(log2(abs(pos.y)))));
	vec2 grid_high = 2.0*grid_low;
	return step(res/2.0/u_zoom, abs(pos.x-grid_low.x)) * step(res/2.0/u_zoom, abs(grid_high.x-pos.x)) * step(res/2.0/u_zoom, abs(pos.y-grid_low.y)) * step(res/2.0/u_zoom, abs(grid_high.y-pos.y));
}

void main()
{
	vec4 color;

	float alpha = 1.0 - grid_alph(world_position, 1.0);
	color = vec4(0.0, 0.0, 1.0, alpha);
    gl_FragColor = color;
}