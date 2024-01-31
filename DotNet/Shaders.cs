using SkiaSharp;

namespace hellonativeaot;

static class Shaders
{
    public static SKRuntimeShaderBuilder GetRandomShader()
    {
        string source = GetRandomShader(Random.Shared.Next(2));
        return SKRuntimeEffect.BuildShader(source);
    }

    static string GetRandomShader(int index)
    {
        return index switch
        {
            // https://shaders.skia.org/?id=de2a4d7d893a7251eb33129ddf9d76ea517901cec960db116a1bbd7832757c1f
            // Source: @notargs https://twitter.com/notargs/status/1250468645030858753
            0 => """
                uniform float3 iResolution;      // Viewport resolution (pixels)
                uniform float  iTime;            // Shader playback time (s)

                float f(vec3 p) {
                    p.z -= iTime * 10.;
                    float a = p.z * .1;
                    p.xy *= mat2(cos(a), sin(a), -sin(a), cos(a));
                    return .1 - length(cos(p.xy) + sin(p.yz));
                }

                half4 main(vec2 fragcoord) { 
                    vec3 d = .5 - fragcoord.xy1 / iResolution.y;
                    vec3 p=vec3(0);
                    for (int i = 0; i < 32; i++) {
                        p += f(p) * d;
                    }
                    return ((sin(p) + vec3(2, 5, 12)) / length(p)).xyz1;
                }
                """,
            
            // https://shaders.skia.org/?id=ed72577c437c036447372e4c873462fc1bbfc0cb5e9fb0630ab1c07368a0db48
            // Source: @zozuar https://twitter.com/zozuar/status/1482754721450446850
            1 => """
                mat2 rotate2D(float r){
                    return mat2(cos(r), sin(r), -sin(r), cos(r));
                }

                mat3 rotate3D(float angle, vec3 axis){
                    vec3 a = normalize(axis);
                    float s = sin(angle);
                    float c = cos(angle);
                    float r = 1.0 - c;
                    return mat3(
                        a.x * a.x * r + c,
                        a.y * a.x * r + a.z * s,
                        a.z * a.x * r - a.y * s,
                        a.x * a.y * r - a.z * s,
                        a.y * a.y * r + c,
                        a.z * a.y * r + a.x * s,
                        a.x * a.z * r + a.y * s,
                        a.y * a.z * r - a.x * s,
                        a.z * a.z * r + c
                    );
                }

                half4 main(float2 FC) {
                    vec4 o = vec4(0);
                    vec2 r = iResolution.xy;
                    vec3 v = vec3(1,3,7), p = vec3(0);
                    float t=iTime, n=0, e=0, g=0, k=t*.2;
                    for (float i=0; i<100; ++i) {
                        p = vec3((FC.xy-r*.5)/r.y*g,g)*rotate3D(k,cos(k+v));
                        p.z += t;
                        p = asin(sin(p)) - 3.;
                        n = 0;
                        for (float j=0; j<9.; ++j) {
                        p.xz *= rotate2D(g/8.);
                        p = abs(p);
                        p = p.x<p.y ? n++, p.zxy : p.zyx;
                        p += p-v;
                        }
                        g += e = max(p.x,p.z) / 1e3 - .01;
                        o.rgb += .1/exp(cos(v*g*.1+n)+3.+1e4*e);
                    }
                    return o.xyz1;
                }
                """,

            _ => throw new IndexOutOfRangeException($"No shader for index {index}"),
        };
    }
}