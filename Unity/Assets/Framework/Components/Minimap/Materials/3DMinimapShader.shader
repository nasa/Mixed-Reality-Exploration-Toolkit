Shader "Custom/3DMinimapShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _DisplaceTex("Displacement Texture", 2D) = "black" {}
        _Amount("Displace Amount", Range(0, 1000)) = 1000
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows addshadow vertex:vert
        #include "UnityCG.cginc"
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _DisplaceTex;
        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _Amount;
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        
        void vert (inout appdata_full v) {
            fixed height = tex2Dlod(_DisplaceTex, float4(v.texcoord.xy, 0, 0)).r;
            v.vertex.xyz += v.normal * _Amount;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
        }
        ENDCG
    }
    FallBack "Diffuse"
}
