float4x4 view_projection;

struct VS_OUTPUT
{
    float4 Position : POSITION;
    float2 UV       : TEXCOORD0;
    float4 Color    : TEXCOORD1;
};

struct VS_INPUT
{
    float4 Position : POSITION;
    float4 Color    : COLOR0;
    float2 UV       : TEXCOORD0;
};

VS_OUTPUT main(VS_INPUT IN)
{
    VS_OUTPUT OUT;
    OUT.Position = mul(view_projection, IN.Position);
    OUT.Color = IN.Color;
    OUT.UV = IN.UV;
    return OUT;
}
