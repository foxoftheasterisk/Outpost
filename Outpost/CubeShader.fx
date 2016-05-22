
//okay let's do this

//Note to self:
//Although this is targeting OpenGL, the shader is written in HLSL due to Monogame quirks.

//probably more efficient to combine these CPU-side, as otherwise we'd need to do it per-vertex
//but we'll worry about that later
//for now, following the tutorial
//(and trying to break the code as little as possible)
float4x4 World;
float4x4 View;
float4x4 Projection;

//i see no reason for ambient color to not be white
//although, actually, I might kill ambient entirely
float4 ambientColor = float4(1, 1, 1, 1);
//float ambientIntensity = 1;
//just use the fourth as intensity?

//diffuse, uh, i dunno how to multiple light sources, which we'll want eventually...
//and even more than that, light occlusion is tough
//WELL, WE SHALL WORRY ABOUT THIS LATERS.
//for now, have a sun
float3 sunDir = float3(0.1, -1, 0.1);
float4 sunColor = float4(1, 1, 0.8, 1);

//so geometry shader-ing might be a terrible mistake, but we'll try it
//...except that Monogame (probably) doesn't support it.  Not worth it.
//float cubeSize = 0.2;

struct VertexData
{
	float4 position : POSITION0;
	float4 color : COLOR0;
	float4 specularColor : COLOR1;
	float3 normal : NORMAL0;
};

struct VertexToPixel
{
	float4 position : POSITION0;
	float4 litColor : COLOR0;
	float4 specularColor : COLOR1;
	float3 normal : NORMAL0;  //It refuses to recognize this 
};

VertexToPixel CubeVertexShader(VertexData input)
{
	VertexToPixel output;

	float4 worldPosition = mul(input.position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.position = mul(viewPosition, Projection);

	float4 lightColor = ambientColor * ambientColor.a;
	//normally we'd need to switch the normal to be in world-space
	//but, it conveniently already is
	//so we cool.
	//as long as we don't do any world space rotation.
	float diffuseIntensity = dot(input.normal, -sunDir) * sunColor.a;
	if (diffuseIntensity > 0)
		lightColor = lightColor + sunColor * diffuseIntensity;

	output.litColor = float4(0, 0, 0, 0);

	output.litColor.rgb = input.color.rgb * lightColor.rgb;
	//output.litColor.g = input.color.g * lightColor.g;
	//output.litColor.b = input.color.b * lightColor.b;
	output.litColor.a = input.color.a;

	output.litColor = saturate(output.litColor);

	output.specularColor = input.specularColor;
	output.normal = input.normal;

	return output;
}


//far as I know, MonoGame doesn't currently support Geometry shaders.  So, no doin' that.
//If it ever does become an option, I should have one point per face rather than one per voxel.
//void CubeGeometryShader(point VertexToPixel input[], inout TriangleStream<VertexToPixel> output)
//{
//
//}


//normally, you wouldn't be able to perform this reflection in the vertex shader
//but since I'm working with faces that are guaranteed to have a single normal across the whole face, it works.
//...unless it somehow breaks the compiler???


float4 CubePixelShader(VertexToPixel input) : COLOR0
{
	//specular time!!
	//yes, this ENTIRE fragment slash pixel shader is for specular. 

	float3 eyePosition = float3(0, 0, 1.0);
	//I have no idea if this is right...
	//It would be a consistent one though, I think, since we're talking view space here.
	//Or... do we have to go through projection to make it consistent?  No... no way, right?

	float3 worldNormal = mul(float4(input.normal, 0), World).xyz;
	float3 reflection = normalize(sunDir - (2 * worldNormal * dot(sunDir, worldNormal)));
	reflection = mul(float4(reflection, 0), View).xyz;

	//if (angle < 0)
	//return float4(premultSurfaceColor, input.litColor.a);

	float3 specularResult = input.specularColor.rgb * sunColor.rgb * pow(dot(reflection, eyePosition), input.specularColor.a);

	//Now, an experimental function to convert the "black" parts of the result to alpha
	//first guess as to how to do that: set alpha to the brightest color result, and treat it as premultiplied alpha
	float alphaFactor = max(max(specularResult.r, specularResult.g), specularResult.b);
	
	//this would convert it to non-premultiplied alpha
	//but compositing is easier if we DON'T do that
	//so
	//not doing it
	//float inverseAlphaFactor = 1 / alphaFactor;
	//float4 specularFinal = float4(specularResult * inverseAlphaFactor, alphaFactor);

	//now, alpha that over the lit color to get the final.
	//since alpha compositing is easier with premultiplied alpha, and we already have that for the specular, convert our surface value to that.
	float3 premultSurfaceColor = input.litColor.rgb * input.litColor.a;

	float4 finalColor = float4(0, 0, 0, 0);
	finalColor.rgb = specularResult.rgb + (premultSurfaceColor.rgb * (1 - alphaFactor));
	finalColor.a = alphaFactor + input.litColor.a * (1 - alphaFactor);

	//this does mean that our output color is premultiplied alpha
	//wouldn't be particularly hard to convert back tho.
	//although, it does make it slightly lossy I think?  but BFD
	return finalColor;
}

technique CubeShader
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 CubeVertexShader();
		PixelShader = compile ps_3_0 CubePixelShader();
	}
}