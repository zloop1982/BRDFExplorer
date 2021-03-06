
void main()
{
	vec3 normal = normalize(world_normal);
	vec3 view_vector = normalize(cameraPosition - world_position);
	vec3 light_vector = normalize(-light_direction);
	vec3 half_vector = normalize(light_vector + view_vector);

	float NdL = saturate(dot(normal, light_vector));
    float NdV = saturate(dot(normal, view_vector));
    float NdH = saturate(dot(normal, half_vector));
    float VdH = saturate(dot(view_vector, half_vector));
    float LdV = saturate(dot(light_vector, view_vector));

	float alpha = roughness_remap(roughness);

    vec3 real_albedo = albedo * (1.0 - metallic);
    vec3 real_specular = mix(vec3(specular), albedo, metallic);

	// Cook-Torrance: A Reflectance Model for Computer Graphics [Cook82]
    vec3 diffuse_comp = diffuse(real_albedo, NdL, NdV, VdH, roughness);
    vec3 specular_comp = distribution(alpha, NdH) * shadowing(alpha, NdV, NdL, NdH, VdH, LdV) * fresnel(real_specular, VdH) / (4.0 * NdL * NdV + 0.0001);

    vec3 reflect_vector = reflect(-view_vector, normal);
    reflect_vector.x *= -1.0;
    vec3 reflection = textureCube(environment, reflect_vector, alpha * 15.0).rgb;
    reflection = pow(reflection, vec3(GAMMA));

    vec3 env_fresnel = real_specular + (max(real_specular, 1.0 - alpha) - real_specular) * pow((1.0 - NdV), 10.0);

    vec3 color = (diffuse_comp * (1.0 - specular_comp) + specular_comp) * light_color * NdL * light_intensity;
    color += real_albedo * ambient_intensity;
    color += reflection * env_fresnel * reflectivity;
    gl_FragColor = pow(vec4(color, 1.0), vec4(1.0 / GAMMA));
}
