
//okay let's do this

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
	//float4 specularColor : COLOR1;
	float3 normal : NORMAL0;
};

struct VertexToPixel
{
	float4 position : POSITION0;
	float4 litColor : COLOR0;
	//float4 specularColor : COLOR1;
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
	float diffuseIntensity = dot(input.normal, -sunDir) * sunColor.a;
	if (diffuseIntensity > 0)
		lightColor = lightColor + sunColor * diffuseIntensity;

	output.litColor = float4(0, 0, 0, 0);

	output.litColor.r = input.color.r * lightColor.r;
	output.litColor.g = input.color.g * lightColor.g;
	output.litColor.b = input.color.b * lightColor.b;
	output.litColor.a = input.color.a;

	output.litColor = saturate(output.litColor);
	//output.specularColor = input.specularColor;

	return output;
}

/*
//far as I know, MonoGame doesn't currently support Geometry shaders.  So, no doin' that.
//If it ever does become an option, I should have one point per face rather than one per voxel.
void CubeGeometryShader(point VertexToPixel input[], inout TriangleStream<VertexToPixel> output)
{

}
//*/

//will be doin' specular here
//but not yet
float4 CubePixelShader(VertexToPixel input) : COLOR0
{
	return input.litColor;
}

technique CubeShader
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 CubeVertexShader();
		PixelShader = compile ps_3_0 CubePixelShader();
	}
}