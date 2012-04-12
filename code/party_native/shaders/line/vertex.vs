float4x4 view_projection;

struct VS_OUTPUT
{
    float4 Position : POSITION;
};

struct VS_INPUT
{
    float4 Position : POSITION;
};

VS_OUTPUT main(VS_INPUT IN)
{
    VS_OUTPUT OUT;
    OUT.Position = mul(view_projection, IN.Position);
    return OUT;
}
