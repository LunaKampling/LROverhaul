#version 120

uniform float u_zoom; //The current track zoom (needed to calculate the separation of neighbouring pixels)
uniform float u_cellsize; //The size of individual grid cells

varying vec2 world_position;
varying vec2 pixel_coord;

float grid_alph(vec2 pos, float res) //Returns (one minus) the alpha value of a given position based on its proximity to the grid
{
	vec2 grid = fract(pos/u_cellsize);
	return step(res/2.0/u_cellsize/u_zoom, grid.x) * step(res/2.0/u_cellsize/u_zoom, 1.0-grid.x) * step(res/2.0/u_cellsize/u_zoom, grid.y) * step(res/2.0/u_cellsize/u_zoom, 1.0-grid.y);
}

void main()
{
	float alpha = 1.0 - grid_alph(world_position, 1.0);

    vec4 color = vec4(1.0, 0.0, 0.0, alpha);
    gl_FragColor = color;
}