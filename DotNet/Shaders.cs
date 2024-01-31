using Android;
using SkiaSharp;
using static Android.NativeMethods;

namespace hellonativeaot;

static class Shaders
{
    public static SKRuntimeShaderBuilder GetRandomShader()
    {
        string source =
            """
            uniform float3 iResolution;      // Viewport resolution (pixels)
            uniform float  iTime;            // Shader playback time (s)

            """;
        source += GetRandomShader(Random.Shared.Next(3));
        return SKRuntimeEffect.BuildShader(source);
    }

    static string GetRandomShader(int index)
    {
        LogPrint(LogPriority.Info, "Managed", $"GetRandomShader, index: {index}");

        return index switch
        {
            // https://shaders.skia.org/?id=de2a4d7d893a7251eb33129ddf9d76ea517901cec960db116a1bbd7832757c1f
            // Source: @notargs https://twitter.com/notargs/status/1250468645030858753
            0 => """
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
            
            // https://shaders.skia.org/?id=2bee4488820c3253cd8861e85ce3cab86f482cfddd79cfd240591bf64f7bcc38
            // Source: @XorDev https://twitter.com/XorDev/status/1475524322785640455
            1 => """
                vec4 main(vec2 FC) {
                    vec4 o = vec4(0);
                    vec2 p = vec2(0), c=p, u=FC.xy*2.-iResolution.xy;
                    float a;
                    for (float i=0; i<4e2; i++) {
                        a = i/2e2-1.;
                        p = cos(i*2.4+iTime+vec2(0,11))*sqrt(1.-a*a);
                        c = u/iResolution.y+vec2(p.x,a)/(p.y+2.);
                        o += (cos(i+vec4(0,2,4,0))+1.)/dot(c,c)*(1.-p.y)/3e4;
                    }
                    return o;
                }
                """,

            // https://shaders.skia.org/?id=23a360c975c3cb195c89ccdf65ec549e279ce8a959643b447e69cb70614a6eca
            // Source: @zozuar https://twitter.com/zozuar/status/1492217553103503363
            2 => """
                vec3 hsv(float h, float s, float v){
                    vec4 t = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                    vec3 p = abs(fract(vec3(h) + t.xyz) * 6.0 - vec3(t.w));
                    return v * mix(vec3(t.x), clamp(p - vec3(t.x), 0.0, 1.0), s);
                }

                vec4 main(vec2 FC) {
                    float e=0,R=0,t=iTime,s;
                    vec2 r = iResolution.xy;
                    vec3 q=vec3(0,0,-1), p, d=vec3((FC.xy-.5*r)/r.y,.7);
                    vec4 o=vec4(0);
                    for(float i=0;i<100;++i) {
                        o.rgb+=hsv(.1,e*.4,e/1e2)+.005;
                        p=q+=d*max(e,.02)*R*.3;
                        float py = (p.x == 0 && p.y == 0) ? 1 : p.y;
                        p=vec3(log(R=length(p))-t,e=asin(-p.z/R)-1.,atan(p.x,py)+t/3.);
                        s=1;
                        for(int z=1; z<=9; ++z) {
                        e+=cos(dot(sin(p*s),cos(p.zxy*s)))/s;
                        s+=s;
                        }
                        i>50.?d/=-d:d;
                    }
                    return o;
                }
                """,

            _ => throw new IndexOutOfRangeException($"No shader for index {index}"),
        };
    }
}