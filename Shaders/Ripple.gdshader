shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

// Source: https://www.youtube.com/watch?v=hI4lii-P5dg
uniform sampler2D noise;
//uniform float speed : hint_range(0.05, 0.5) = 0.1;
uniform float strength : hint_range(0.05, 0.5) = 0.1;

//void fragment() {
	//// sample noise
	//float noise_value = texture(noise, (vec2((sin(UV + TIME * speed)) + 1.0) / 2.0)).r;
	//
	//COLOR = texture(SCREEN_TEXTURE, SCREEN_UV - (strength / 2.0) + vec2(noise_value) * strength);
//}

// Source: https://godotshaders.com/shader/ripple-shader/
uniform float wave_count : hint_range(1.0, 20.0, 1.0) = 20.0;
uniform float speed : hint_range(0.0, 10.0, 0.1) = 3.0;
uniform float height : hint_range(0.0, 0.1, 0.001) = 0.003;

void fragment() {
	vec2 cPos = -1.0 + 2.0 * UV / (1.0 / TEXTURE_PIXEL_SIZE);
	float cLength = length(cPos);
	vec2 uv = FRAGCOORD.xy / (1.0 / SCREEN_PIXEL_SIZE).xy + (cPos/cLength) * cos(cLength * wave_count - TIME * speed) * height;
    vec3 col = texture(SCREEN_TEXTURE,uv).xyz;
	COLOR = vec4(col,1.0);
}