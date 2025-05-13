Shader "URP/Cartoon/BillboardLeaf"
{
    Properties
    {
       
        _MainTex("MainTex",2D) = "white"{}
        
        [Space(20)]
        
        [Header(Diffuse)]
         _BaseColor("Base Color",Color) = (1.0,1.0,1.0,1.0)
    	_ShadowIntensity("Shadow Intensity",Range(0,1)) = 1
        _SmoothValue("Smooth Value",Range(0,0.1)) = 0
        
        [Space(20)]
        //Wind
        _WindDistortionMap("Wind Distortion Map",2D) = "black"{}
        _WindStrength("WindStrength",float) = 0
        _U_Speed("U_Seed",float) = 0
        _V_Speed("V_Seed",float) = 0
        _Bias("Wind Bias",Range(-1.0,1.0)) = 0
        
        [Space(20)]
        //FrameTexture
        [Toggle(_EnableFrameTexture)] _EnableFrameTexture("Enable Frame Texture",float) = 0
        _FrameTex("Sprite Frame Texture", 2D) = "white" {}
        _FrameNum("Frame Num",int) = 24
        _FrameRow("Frame Row",int) = 5
        _FrameColumn("Frame Column",int) = 5
        _FrameSpeed("Frame Speed",Range(0,10)) = 3
        
        [Space(20)]
        [Toggle(_EnableMultipleMeshMode)]_EnableMultipleMeshMode("Enable Multiple Mesh Mode",float) = 0
        [Toggle(_EnableHoudiniDecodeMode)]_EnableDecodeMode("Enable Houdini Decode Mode",float) = 0
        _DecodeValue("Decode Value",float) = 10000
        
    }
    
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        //UsePass "Universal Render Pipeline/Lit/SHADOWCASTER"
        
        HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
            
          //----------贴图声明开始-----------
            TEXTURE2D(_MainTex);//定义贴图
            SAMPLER(sampler_MainTex);//定义采样器
            TEXTURE2D(_WindDistortionMap);
            SAMPLER(sampler_WindDistortionMap);
            TEXTURE2D(_FrameTex);
            SAMPLER(sampler_FrameTex);
            //----------贴图声明结束-----------

            //GPU Instance
            UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
                UNITY_DEFINE_INSTANCED_PROP(half4, _BaseColor)
                UNITY_DEFINE_INSTANCED_PROP(float4,_MainTex_ST)
                UNITY_DEFINE_INSTANCED_PROP(float,_SmoothValue)
				UNITY_DEFINE_INSTANCED_PROP(float, _ShadowIntensity)
            
                UNITY_DEFINE_INSTANCED_PROP(float3,_normal)
            
                UNITY_DEFINE_INSTANCED_PROP(float, _WindStrength)
                UNITY_DEFINE_INSTANCED_PROP(float, _U_Speed)
                UNITY_DEFINE_INSTANCED_PROP(float, _V_Speed)
                UNITY_DEFINE_INSTANCED_PROP(float4,_WindDistortionMap_ST)
                UNITY_DEFINE_INSTANCED_PROP(float,_Bias)
            
                UNITY_DEFINE_INSTANCED_PROP(float4,_FrameTex_ST)
                UNITY_DEFINE_INSTANCED_PROP(int,_FrameNum)
                UNITY_DEFINE_INSTANCED_PROP(float,_FrameRow)
                UNITY_DEFINE_INSTANCED_PROP(float,_FrameColumn)
                UNITY_DEFINE_INSTANCED_PROP(float,_FrameSpeed)

                UNITY_DEFINE_INSTANCED_PROP(float,_DecodeValue)
            UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
            
            //AngleAxis3x3()接收一个角度（弧度制）并返回一个围绕提供轴旋转的矩阵
            float3x3 AngleAxis3x3(float angle, float3 axis)
            {
              float c, s;
              sincos(angle, s, c);

              float t = 1 - c;
              float x = axis.x;
              float y = axis.y;
              float z = axis.z;

              return float3x3(
	             t * x * x + c, t * x * y - s * z, t * x * z + s * y,
	             t * x * y + s * z, t * y * y + c, t * y * z - s * x,
	             t * x * z - s * y, t * y * z + s * x, t * z * z + c
	             );
            }
        ENDHLSL

        pass
        {
            Cull Off
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature _EnableFrameTexture
            #pragma shader_feature _EnableMultipleMeshMode
            #pragma shader_feature _EnableHoudiniDecodeMode
            
            //开启GPU Instance
            #pragma multi_compile_instancing

            #pragma multi_compile  _MAIN_LIGHT_SHADOWS
    		#pragma multi_compile  _MAIN_LIGHT_SHADOWS_CASCADE
    		#pragma multi_compile  _SHADOWS_SOFT
            
            struct vertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float2 uv3 : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct vertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 nDirWS : TEXCOORD1;
                float3 posWS : TEXCOORD2;
                float4 color : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            vertexOutput vert (vertexInput v)
            {
                vertexOutput o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                //Instance变量
                float windStrength = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_WindStrength);
                float2 windDistorationMap_FlowSpeed = float2(UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_U_Speed),UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_V_Speed));
                float4 windDistortionMap_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_WindDistortionMap_ST);
                float bias = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Bias);
                
                 float3 centerOffset = float3(0, 0, 0);
                
                #ifdef _EnableMultipleMeshMode
                    centerOffset = v.color;
                #endif

                #ifdef _EnableHoudiniDecodeMode
                    centerOffset = (float4(v.uv2,v.uv3)*2.0-1.0)*UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_DecodeValue);
                #endif
                v.vertex.xyz -= centerOffset;
                    
                //Billboard
                float3 camPosOS = TransformWorldToObject(_WorldSpaceCameraPos);//将摄像机的坐标转换到物体模型空间
                float3 newForwardDir = normalize(camPosOS - centerOffset); //计算新的forward轴
                float3 newRightDir = normalize(cross(float3(0, 1, 0), newForwardDir)); //计算新的right轴
                float3 newUpDir = normalize(cross(newForwardDir,newRightDir)); //计算新的up轴
                v.vertex.xyz = newRightDir * v.vertex.x + newUpDir * v.vertex.y + newForwardDir*v.vertex.z; //将原本的xyz坐标以在新轴上计算，相当于一个线性变换【原模型空间】->【新模型空间】
                
                //Wind
                float3 positionWS_0 = TransformObjectToWorld(float3(0,0,0)+centerOffset);
                windStrength = max(0.0001f,windStrength);
                float2 windUV = positionWS_0.xz*windDistortionMap_ST.xy + windDistortionMap_ST.zw + windDistorationMap_FlowSpeed*_Time.z;
                float2 windSample = ((SAMPLE_TEXTURE2D_LOD(_WindDistortionMap,sampler_WindDistortionMap, windUV,0).xy * 2 - 1)+bias) * windStrength;
                float3 wind = normalize(float3(windSample.x,0,windSample.y));
	            float3x3 windRotation = AngleAxis3x3(PI * windSample.x, newForwardDir);
                v.vertex.xyz = mul(windRotation,v.vertex.xyz);
                
                v.vertex.xyz += centerOffset;
                
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.nDirWS = TransformObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                o.posWS = TransformObjectToWorld(v.vertex.xyz);
                o.color = v.color;
                return o;
            }

            float CalculateDiffuseLightResult(float3 nDir, float3 lDir, float lightRadience, float shadow)
            {
                float nDotl = dot(nDir,lDir);
                float lambert = max(0,nDotl);
                float halfLambert = nDotl*0.5+0.5;
                half3 result = halfLambert*lightRadience*lerp(1-UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _ShadowIntensity),1,shadow);
                return result;
            }

            half4 frag (vertexOutput i) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(i);
                
                //Instance变量
                half3 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
                int frameNum = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _FrameNum);
                float frameRow = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _FrameRow);
                float frameColumn = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_FrameColumn);
                float frameSpeed = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_FrameSpeed);
                float4 frameTex_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_FrameTex_ST);
                float smoothValue = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_SmoothValue);
                
                float3 normal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_normal);

                #ifdef _EnableMultipleMeshMode
                    normal = i.nDirWS;
                #endif
                
                
                float2 mainTexUV;
                half4 mainTex;
                #ifdef _EnableFrameTexture
                    mainTexUV = i.uv * frameTex_ST.xy + frameTex_ST.zw;
                    float perX = 1.0f /frameRow;
                    float perY = 1.0f /frameColumn;
                    float currentIndex = fmod(_Time.z*frameSpeed,frameNum);
                    int rowIndex = currentIndex/frameRow;
                    int columnIndex = fmod(currentIndex,frameColumn);
                    float2 realMainTexUV = mainTexUV*float2(perX,perY)+float2(perX*columnIndex,perY*rowIndex);
                    mainTex =   SAMPLE_TEXTURE2D(_FrameTex, sampler_FrameTex, realMainTexUV);
                    
                #else
                    float4 mainTex_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
                    mainTexUV = i.uv*mainTex_ST.xy+mainTex_ST.zw;
                    mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainTexUV);
                #endif

                float4 shadowCoord = TransformWorldToShadowCoord(i.posWS);
                Light mainLight = GetMainLight(shadowCoord);
                //float3 nDir= i.nDirWS;
                float3 nDir= normal;
                float3 lDir = mainLight.direction;
                float3 vDir = normalize(_WorldSpaceCameraPos.xyz - i.posWS.xyz);
                float3 hDir = normalize(lDir+vDir);
                
                float3 nDirVS = TransformWorldToViewDir(normal);
                
                half4 albedo = mainTex;
                
                float mainLightRadiance = mainLight.distanceAttenuation;
                float mainDiffuse = CalculateDiffuseLightResult(nDir,lDir,mainLightRadiance,mainLight.shadowAttenuation);

                uint lightCount = GetAdditionalLightsCount();
            	float additionalDiffuse = half3(0,0,0);
            	float additionalSpecular = half3(0,0,0);
				for (uint lightIndex = 0; lightIndex < lightCount; lightIndex++)
				{
				    Light additionalLight = GetAdditionalLight(lightIndex, i.posWS, 1);
					half3 additionalLightColor = additionalLight.color;
					float3 additionalLightDir = additionalLight.direction;
					// 光照衰减和阴影系数
                    float additionalLightRadiance =  additionalLight.distanceAttenuation;
					float perDiffuse = CalculateDiffuseLightResult(nDir,additionalLightDir,additionalLightRadiance,additionalLight.shadowAttenuation);
				    additionalDiffuse += perDiffuse;
				}

                float diffuse = mainDiffuse+additionalDiffuse;
                float mask1 = 1-smoothstep(0.2-smoothValue,0.2,diffuse);
                float mask2 = 1-smoothstep(0.41-smoothValue,0.41,diffuse);
                float mask3 = 1-smoothstep(0.624-smoothValue,0.624,diffuse);
                float mask4 = 1-smoothstep(0.823-smoothValue,0.823,diffuse);
                float mask5 = 1-smoothstep(0.965-smoothValue,0.965,diffuse);
                float mask6 = 1 - mask5;
                mask2 = mask2 -mask1;
                mask3 = mask3 -mask2-mask1;
                mask4 = mask4 -mask3 - mask2 -mask1;
                mask5 = mask5 - mask4 -mask3 - mask2 -mask1;
                
                
                half3 diffuseColor1 = baseColor*albedo*mask1*half3(0.2829f,0.2976f,0.3584f);
                half3 diffuseColor2 = baseColor*albedo*mask2*half3(0.4024f,0.4244f,0.4811f);
                half3 diffuseColor3 = baseColor*albedo*mask3*half3(0.4396f,0.4660f,0.5377f);
                half3 diffuseColor4 = baseColor*albedo*mask4*half3(0.6031f,0.6451f,0.7641f);
                half3 diffuseColor5 = baseColor*albedo*mask5*half3(0.6776f,0.7272f,0.8584f);
                half3 diffuseColor6 = baseColor*albedo*mask6;
                half3 Diffuse = diffuseColor1+diffuseColor2+diffuseColor3+diffuseColor4+diffuseColor5+diffuseColor6;
                
                
                half3 finalRGB = Diffuse;

                clip(albedo.a-0.1);
                half4 result = half4(finalRGB,albedo.a);
                return result;
            }
            
            ENDHLSL
        }

        //DepthOnly Pass
        Pass
        {
        	Name "DepthOnly"

        	Tags{"LightMode" = "DepthOnly"}
        	Cull Off
        	
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature _EnableFrameTexture
            #pragma shader_feature _EnableMultipleMeshMode
            #pragma shader_feature _EnableHoudiniDecodeMode

            //开启GPU Instance
            #pragma multi_compile_instancing
            
            struct vertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float2 uv3 : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct vertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 nDirWS : TEXCOORD1;
                float3 posWS : TEXCOORD2;
                float4 color : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            vertexOutput vert (vertexInput v)
            {
                vertexOutput o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                //Instance变量
                float windStrength = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_WindStrength);
                float2 windDistorationMap_FlowSpeed = float2(UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_U_Speed),UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_V_Speed));
                float4 windDistortionMap_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_WindDistortionMap_ST);
                float bias = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Bias);
                
                 float3 centerOffset = float3(0, 0, 0);
                
                #ifdef _EnableMultipleMeshMode
                    centerOffset = v.color;
                #endif

                #ifdef _EnableHoudiniDecodeMode
                    centerOffset = (float4(v.uv2,v.uv3)*2.0-1.0)*UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_DecodeValue);
                #endif
                v.vertex.xyz -= centerOffset;
                    
                //Billboard
                float3 camPosOS = TransformWorldToObject(_WorldSpaceCameraPos);//将摄像机的坐标转换到物体模型空间
                float3 newForwardDir = normalize(camPosOS - centerOffset); //计算新的forward轴
                float3 newRightDir = normalize(cross(float3(0, 1, 0), newForwardDir)); //计算新的right轴
                float3 newUpDir = normalize(cross(newForwardDir,newRightDir)); //计算新的up轴
                v.vertex.xyz = newRightDir * v.vertex.x + newUpDir * v.vertex.y + newForwardDir*v.vertex.z; //将原本的xyz坐标以在新轴上计算，相当于一个线性变换【原模型空间】->【新模型空间】
                
                //Wind
                float3 positionWS_0 = TransformObjectToWorld(float3(0,0,0)+centerOffset);
                windStrength = max(0.0001f,windStrength);
                float2 windUV = positionWS_0.xz*windDistortionMap_ST.xy + windDistortionMap_ST.zw + windDistorationMap_FlowSpeed*_Time.z;
                float2 windSample = ((SAMPLE_TEXTURE2D_LOD(_WindDistortionMap,sampler_WindDistortionMap, windUV,0).xy * 2 - 1)+bias) * windStrength;
                float3 wind = normalize(float3(windSample.x,0,windSample.y));
	            float3x3 windRotation = AngleAxis3x3(PI * windSample.x, newForwardDir);
                v.vertex.xyz = mul(windRotation,v.vertex.xyz);
                
                v.vertex.xyz += centerOffset;
                
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.nDirWS = TransformObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                o.posWS = TransformObjectToWorld(v.vertex.xyz);
                o.color = v.color;
                return o;
            }

            float4 frag (vertexOutput i) : SV_TARGET
            {
            	UNITY_SETUP_INSTANCE_ID(i);
            	//Instance变量
                int frameNum = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _FrameNum);
                float frameRow = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _FrameRow);
                float frameColumn = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_FrameColumn);
                float frameSpeed = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_FrameSpeed);
                float4 frameTex_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_FrameTex_ST);
				
                float2 mainTexUV;
                half4 mainTex;
                #ifdef _EnableFrameTexture
                    mainTexUV = i.uv * frameTex_ST.xy + frameTex_ST.zw;
                    float perX = 1.0f /frameRow;
                    float perY = 1.0f /frameColumn;
                    float currentIndex = fmod(_Time.z*frameSpeed,frameNum);
                    int rowIndex = currentIndex/frameRow;
                    int columnIndex = fmod(currentIndex,frameColumn);
                    float2 realMainTexUV = mainTexUV*float2(perX,perY)+float2(perX*columnIndex,perY*rowIndex);
                    mainTex =   SAMPLE_TEXTURE2D(_FrameTex, sampler_FrameTex, realMainTexUV);
                    
                #else
                    float4 mainTex_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
                    mainTexUV = i.uv*mainTex_ST.xy+mainTex_ST.zw;
                    mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainTexUV);
                #endif

			    if (mainTex.a<0.5)
			    {
				    discard;
			    }
            	
            	float vertexRawDepth = i.pos.z;
            	return float4(vertexRawDepth,0,0,1);
            }
            
            ENDHLSL
        }

		//Normal Pass
        Pass
        {
        	Name "CustomNormalsPass"

        	Cull Off
        	Tags{"LightMode" = "DepthNormals"}
        	
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature _EnableFrameTexture
            #pragma shader_feature _EnableMultipleMeshMode
            #pragma shader_feature _EnableHoudiniDecodeMode

            //开启GPU Instance
            #pragma multi_compile_instancing
            
           struct vertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float2 uv3 : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct vertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 nDirWS : TEXCOORD1;
                float3 posWS : TEXCOORD2;
                float4 color : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            vertexOutput vert (vertexInput v)
            {
                vertexOutput o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                //Instance变量
                float windStrength = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_WindStrength);
                float2 windDistorationMap_FlowSpeed = float2(UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_U_Speed),UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_V_Speed));
                float4 windDistortionMap_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_WindDistortionMap_ST);
                float bias = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Bias);
                
                 float3 centerOffset = float3(0, 0, 0);
                
                #ifdef _EnableMultipleMeshMode
                    centerOffset = v.color;
                #endif

                #ifdef _EnableHoudiniDecodeMode
                    centerOffset = (float4(v.uv2,v.uv3)*2.0-1.0)*UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_DecodeValue);
                #endif
                v.vertex.xyz -= centerOffset;
                    
                //Billboard
                float3 camPosOS = TransformWorldToObject(_WorldSpaceCameraPos);//将摄像机的坐标转换到物体模型空间
                float3 newForwardDir = normalize(camPosOS - centerOffset); //计算新的forward轴
                float3 newRightDir = normalize(cross(float3(0, 1, 0), newForwardDir)); //计算新的right轴
                float3 newUpDir = normalize(cross(newForwardDir,newRightDir)); //计算新的up轴
                v.vertex.xyz = newRightDir * v.vertex.x + newUpDir * v.vertex.y + newForwardDir*v.vertex.z; //将原本的xyz坐标以在新轴上计算，相当于一个线性变换【原模型空间】->【新模型空间】
                
                //Wind
                float3 positionWS_0 = TransformObjectToWorld(float3(0,0,0)+centerOffset);
                windStrength = max(0.0001f,windStrength);
                float2 windUV = positionWS_0.xz*windDistortionMap_ST.xy + windDistortionMap_ST.zw + windDistorationMap_FlowSpeed*_Time.z;
                float2 windSample = ((SAMPLE_TEXTURE2D_LOD(_WindDistortionMap,sampler_WindDistortionMap, windUV,0).xy * 2 - 1)+bias) * windStrength;
                float3 wind = normalize(float3(windSample.x,0,windSample.y));
	            float3x3 windRotation = AngleAxis3x3(PI * windSample.x, newForwardDir);
                v.vertex.xyz = mul(windRotation,v.vertex.xyz);
                
                v.vertex.xyz += centerOffset;
                
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.nDirWS = TransformObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                o.posWS = TransformObjectToWorld(v.vertex.xyz);
                o.color = v.color;
                return o;
            }

            float4 frag (vertexOutput i) : SV_TARGET
            {
            	UNITY_SETUP_INSTANCE_ID(i);
            	//Instance变量
                int frameNum = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _FrameNum);
                float frameRow = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _FrameRow);
                float frameColumn = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_FrameColumn);
                float frameSpeed = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_FrameSpeed);
                float4 frameTex_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_FrameTex_ST);
				
                float2 mainTexUV;
                half4 mainTex;
                #ifdef _EnableFrameTexture
                    mainTexUV = i.uv * frameTex_ST.xy + frameTex_ST.zw;
                    float perX = 1.0f /frameRow;
                    float perY = 1.0f /frameColumn;
                    float currentIndex = fmod(_Time.z*frameSpeed,frameNum);
                    int rowIndex = currentIndex/frameRow;
                    int columnIndex = fmod(currentIndex,frameColumn);
                    float2 realMainTexUV = mainTexUV*float2(perX,perY)+float2(perX*columnIndex,perY*rowIndex);
                    mainTex =   SAMPLE_TEXTURE2D(_FrameTex, sampler_FrameTex, realMainTexUV);
                    
                #else
                    float4 mainTex_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
                    mainTexUV = i.uv*mainTex_ST.xy+mainTex_ST.zw;
                    mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainTexUV);
                #endif

			    if (mainTex.a<0.5)
			    {
				    discard;
			    }
                
            	float3 normalWS = NormalizeNormalPerPixel(i.nDirWS);
            	return float4(normalWS,1);
            }
            
            ENDHLSL
        }

		//ShadowCaster Pass
        Pass
        {
        	Name "CustomShadowCasterPass"

        	Tags{ "LightMode" = "ShadowCaster" }
        	
        	Cull Off
        	ZWrite On
			ZTest LEqual
        	
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma target 4.6

            #pragma shader_feature _EnableFrameTexture
            #pragma shader_feature _EnableMultipleMeshMode
            #pragma shader_feature _EnableHoudiniDecodeMode
            #pragma shader_feature _EnableCustomShadowColor

            //开启GPU Instance
            #pragma multi_compile_instancing
            
            struct vertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float2 uv3 : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct vertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 nDirWS : TEXCOORD1;
            	float4 screenPos : TEXCOORD2;
            	UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            vertexOutput vert (vertexInput v)
            {
                vertexOutput o;
            	UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

            	//Instance变量
                float windStrength = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_WindStrength);
                float2 windDistorationMap_FlowSpeed = float2(UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_U_Speed),UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_V_Speed));
                float4 windDistortionMap_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_WindDistortionMap_ST);
                float bias = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Bias);

            	float3 centerOffset = float3(0, 0, 0);
                
                #ifdef _EnableMultipleMeshMode
                    centerOffset = v.color;
                #endif

                #ifdef _EnableHoudiniDecodeMode
                    centerOffset = (float4(v.uv2,v.uv3)*2.0-1.0)*UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_DecodeValue);
                #endif
                v.vertex.xyz -= centerOffset;

                float4 originalPosCS = TransformObjectToHClip(float3(0,0,0)+centerOffset);
                float3 cameraPos;
                if (unity_OrthoParams.w>0.5)
                {
                    float4 screenPos = ComputeScreenPos(originalPosCS);
                    float2 ndcPos = screenPos.xy/screenPos.w*2-1;//map[0,1] -> [-1,1]
                    float3 viewPos = float3(unity_OrthoParams.xy * ndcPos.xy, 0);
                    cameraPos = mul(UNITY_MATRIX_I_V, float4(viewPos, 1)).xyz;
                }
                else
                {
                    cameraPos = _WorldSpaceCameraPos; //摄像机上的世界坐标
                }
                    
                //Billboard
                float3 camPosOS = TransformWorldToObject(cameraPos);//将摄像机的坐标转换到物体模型空间
                float3 newForwardDir = -normalize(camPosOS - centerOffset); //计算新的forward轴
                float3 newRightDir = normalize(cross(float3(0, 1, 0), newForwardDir)); //计算新的right轴
                float3 newUpDir = normalize(cross(newForwardDir,newRightDir)); //计算新的up轴
                v.vertex.xyz = newRightDir * v.vertex.x + newUpDir * v.vertex.y + newForwardDir*v.vertex.z; //将原本的xyz坐标以在新轴上计算，相当于一个线性变换【原模型空间】->【新模型空间】
                
                //Wind
                float3 positionWS = TransformObjectToWorld(v.vertex.xyz+centerOffset);
                float3 positionWS_0 = TransformObjectToWorld(float3(0,0,0)+centerOffset);
                windStrength = max(0.0001f,windStrength);
                float2 windUV = positionWS_0.xz*windDistortionMap_ST.xy + windDistortionMap_ST.zw + windDistorationMap_FlowSpeed*_Time.z;
                float2 windSample = ((SAMPLE_TEXTURE2D_LOD(_WindDistortionMap,sampler_WindDistortionMap, windUV,0).xy * 2 - 1)+bias) * windStrength;
                float3 wind = normalize(float3(windSample.x,0,windSample.y));
	            float3x3 windRotation = AngleAxis3x3(PI * windSample.x, newForwardDir);
                v.vertex.xyz = mul(windRotation,v.vertex.xyz);
                
                v.vertex.xyz += centerOffset;
            	
            	float4 posCS = TransformObjectToHClip(v.vertex.xyz);
                o.pos = posCS;
                o.nDirWS = normalize(TransformObjectToWorldNormal(v.normal));
                o.uv = v.uv;
            	o.screenPos = ComputeScreenPos(posCS);
                return o;
            }

            float4 frag (vertexOutput i) : SV_TARGET
            {
            	UNITY_SETUP_INSTANCE_ID(i);
            	//Instance变量
                int frameNum = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _FrameNum);
                float frameRow = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _FrameRow);
                float frameColumn = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_FrameColumn);
                float frameSpeed = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_FrameSpeed);
                float4 frameTex_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_FrameTex_ST);
				
                float2 mainTexUV;
                half4 mainTex;
                #ifdef _EnableFrameTexture
                    mainTexUV = i.uv * frameTex_ST.xy + frameTex_ST.zw;
                    float perX = 1.0f /frameRow;
                    float perY = 1.0f /frameColumn;
                    float currentIndex = fmod(_Time.z*frameSpeed,frameNum);
                    int rowIndex = currentIndex/frameRow;
                    int columnIndex = fmod(currentIndex,frameColumn);
                    float2 realMainTexUV = mainTexUV*float2(perX,perY)+float2(perX*columnIndex,perY*rowIndex);
                    mainTex =   SAMPLE_TEXTURE2D(_FrameTex, sampler_FrameTex, realMainTexUV);
                    
                #else
                    float4 mainTex_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
                    mainTexUV = i.uv*mainTex_ST.xy+mainTex_ST.zw;
                    mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainTexUV);
                #endif

			    if (mainTex.a<0.5)
			    {
				    discard;
			    }
            	
            	return 1;
            }
            
            ENDHLSL
        }
        
    }
}