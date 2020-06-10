//A direce port of https://www.shadertoy.com/view/3dBBWm

Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            //-----------------------------------------------------------------------------
            // Maths utils
            //-----------------------------------------------------------------------------
            float3x3 m = float3x3( 0.00,  0.80,  0.60,
                        -0.80,  0.36, -0.48,
                        -0.60, -0.48,  0.64 );
            float hash( float n )
            {
                return frac(sin(n)*43758.5453);
            }

            float noise( in float3 x )
            {
                float3 p = floor(x);
                float3 f = frac(x);

                f = f*f*(3.0-2.0*f);

                float n = p.x + p.y*57.0 + 113.0*p.z;

                float res = lerp(lerp(lerp( hash(n+  0.0), hash(n+  1.0),f.x),
                                    lerp( hash(n+ 57.0), hash(n+ 58.0),f.x),f.y),
                                lerp(lerp( hash(n+113.0), hash(n+114.0),f.x),
                                    lerp( hash(n+170.0), hash(n+171.0),f.x),f.y),f.z);
                return res - 0.5;
            }

            float fbm( float3 p )
            {
                float f = 0.5;
                f += 0.5000*noise( p ); 
                //p = m*p*2.0;
                p = mul(mul(m, p), 2);
                f += 0.2500*noise( p ); 
                //p = m*p*2.0;
                p = mul(mul(m, p), 2);
                f += 0.1250*noise( p ); 
                //p = m*p*2.0;
                p = mul(mul(m, p), 2);
                f += 0.0625*noise( p ); 
                //p = m*p*2.0;
                p = mul(mul(m, p), 2);
                return f;
            }


            //-----------------------------------------------------------------------------
            // Main functions
            //-----------------------------------------------------------------------------
            float scene(float3 p)
            {
                p.y *= 2.;
                //return 0.4-length(p)*0.03 + 0.5*(fbm(p*float3(.3,.15,.3))-0.5); // value perturbation
                return 0.4-length(p*(1.0+fbm(p*float3(.3,.15,.3))))*0.02; // position perturbation
            }

            fixed4 frag (v2f i) : SV_Target { 
                float2 q = i.uv;
                float2 v = -1.0 + 2.0*q;
                
                // float2 mo = 0;//2.0*iMouse.xy / iResolution.xy;
                float2 mo = (_Time.y / 2);

                // camera by iq
                float3 org = 25.0*normalize(float3(cos(2.75-3.0*mo.x), 0.7-1.0*(mo.y-1.0), sin(2.75-3.0*mo.x)));
                float3 ta = float3(0.0, 1.0, 0.0);
                float3 ww = normalize( ta - org);
                float3 uu = normalize(cross( float3(0.0,1.0,0.0), ww ));
                float3 vv = normalize(cross(ww,uu));
                float3 dir = normalize( v.x*uu + v.y*vv + 1.5*ww );
                float4 color = (.0);
                
                const int nbSample = 128;
                const int nbSampleLight = 8;
                
                float zMax         = 40.;
                float step         = zMax/float(nbSample);
                float zMaxl         = 20.;
                float stepl         = zMaxl/float(nbSampleLight);
                float3 p             = org;
                float T            = 2.2;
                float absorption   = 210.;
                float3 sun_direction = normalize( float3(1.,.0,.0) );
                
                for(int i=0; i<nbSample; i++)
                {
                    float density = scene(p);
                    if(density>0.)
                    {
                        float tmp = density / float(nbSample);
                        T *= 1. -tmp * absorption;
                        if( T <= 0.01)
                            break;
                            
                            
                        //Light scattering
                        float Tl = 1.0;
                        for(int j=0; j<nbSampleLight; j++)
                        {
                            float densityLight = scene( p + normalize(sun_direction)*float(j)*stepl);
                            if(densityLight>0.05)
                                Tl *= 1. - densityLight * absorption/float(nbSample);
                            if (Tl <= 0.01)
                                break;
                        }
                        
                        //Add ambiant + light scattering color
                        color += (1.)*50.*tmp*T +  float4(1.,.7,.4,1.)*80.*tmp*T*Tl;
                    }
                    p += dir*step;
                }    

                return color;

            }
            ENDCG
        }
    }
}
