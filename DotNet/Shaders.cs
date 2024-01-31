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
            uniform float2 iMouse;           // Mouse drag pos=.xy

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
            
            // https://shaders.skia.org/?id=80c3854680c3e99d71fbe24d86185d5bb20cb047305242f9ecb5aff0f102cf73
            // Source: @kamoshika_vrc https://twitter.com/kamoshika_vrc/status/1495081980278751234
            1 => """
                const float PI2 = 6.28318530718;
                float F(vec2 c){
                    return fract(sin(dot(c, vec2(12.9898, 78.233))) * 43758.5453);
                }

                half4 main(float2 FC) {
                    vec4 o;
                    float t = iTime;
                    vec2 r = iResolution.xy * vec2(1, -1);
                    vec3 R=normalize(vec3((FC.xy*2.-r)/r.y,1));
                    for(float i=0; i<100; ++i) {
                        float I=floor(t/.1)+i;
                        float d=(I*.1-t)/R.z;
                        vec2 p=d*R.xy+vec2(sin(t+F(I.xx)*PI2)*.3+F(I.xx*.9),t+F(I.xx*.8));
                        if (F(I/100+ceil(p))<.2) {
                        o+=smoothstep(.1,0.,length(fract(p)-.5))*exp(-d*d*.04);
                        }
                    }
                    return o;
                }
                """,

            // https://shaders.skia.org/?id=e0ec9ef204763445036d8a157b1b5c8929829c3e1ee0a265ed984aeddc8929e2
            // Star Nest by Pablo Roman Andrioli
            // This content is under the MIT License.
            2 => """
                const int iterations = 17;
                const float formuparam = 0.53;

                const int volsteps = 10;
                const float stepsize = 0.1;

                const float zoom  = 0.800;
                const float tile  = 0.850;
                const float speed =0.010 ;

                const float brightness =0.0015;
                const float darkmatter =0.300;
                const float distfading =0.730;
                const float saturation =0.850;


                half4 main( in vec2 fragCoord )
                {
                    //get coords and direction
                    vec2 uv=fragCoord.xy/iResolution.xy-.5;
                    uv.y*=iResolution.y/iResolution.x;
                    vec3 dir=vec3(uv*zoom,1.);
                    float time=iTime*speed+.25;

                    //mouse rotation
                    float a1=.5+iMouse.x/iResolution.x*2.;
                    float a2=.8+iMouse.y/iResolution.y*2.;
                    mat2 rot1=mat2(cos(a1),sin(a1),-sin(a1),cos(a1));
                    mat2 rot2=mat2(cos(a2),sin(a2),-sin(a2),cos(a2));
                    dir.xz*=rot1;
                    dir.xy*=rot2;
                    vec3 from=vec3(1.,.5,0.5);
                    from+=vec3(time*2.,time,-2.);
                    from.xz*=rot1;
                    from.xy*=rot2;
                    
                    //volumetric rendering
                    float s=0.1,fade=1.;
                    vec3 v=vec3(0.);
                    for (int r=0; r<volsteps; r++) {
                        vec3 p=from+s*dir*.5;
                        p = abs(vec3(tile)-mod(p,vec3(tile*2.))); // tiling fold
                        float pa,a=pa=0.;
                        for (int i=0; i<iterations; i++) { 
                            p=abs(p)/dot(p,p)-formuparam; // the magic formula
                            a+=abs(length(p)-pa); // absolute sum of average change
                            pa=length(p);
                        }
                        float dm=max(0.,darkmatter-a*a*.001); //dark matter
                        a*=a*a; // add contrast
                        if (r>6) fade*=1.-dm; // dark matter, don't render near
                        //v+=vec3(dm,dm*.5,0.);
                        v+=fade;
                        v+=vec3(s,s*s,s*s*s*s)*a*brightness*fade; // coloring based on distance
                        fade*=distfading; // distance fading
                        s+=stepsize;
                    }
                    v=mix(vec3(length(v)),v,saturation); //color adjust
                    return vec4(v*.01,1.);	
                }
                """,

            _ => throw new IndexOutOfRangeException($"No shader for index {index}"),
        };
    }
}