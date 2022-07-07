#version 330 core

out vec4 FragColor;

struct Material {
    vec3 Ambient;
    vec3 Diffuse;
    vec3 Specular;
    
    int UseDiffuseMap;
    int UseNormalMap;
    int IsPBR;

    sampler2D DiffuseMap;
    sampler2D SpecularMap;
    sampler2D NormalMap;
    sampler2D DisplacementMap;
    sampler2D AlphaMap;
    sampler2D EmissiveMap;
    sampler2D MetalnessMap;
    sampler2D RoughnessMap;

    float Roughness;
    float Metalness;
    float Shininess;
    float Alpha;
    float DispScale;
}; 

struct Light {
    // 1 directional
    // 2 point
    // 3 spotlight
    int Type;

    vec3 Position;
    vec3 Direction;

    vec3 Ambient;
    vec3 Diffuse;
    vec3 Specular;
    
    float InnerCutOff;
    float OuterCutOff;

    float Constant;
    float Linear;
    float Quadratic;
    float FarPlane;
};

in VS_OUT {
    vec2 UVPos;
    vec3 NormalPos;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
    vec3 FragPos;
} FS_IN_OBJ;


uniform int GlobalLighting;

uniform Material ObjectMaterial;
uniform int LightCount;
uniform Light LightArray[30];

vec3 GetObjectAmbient(){
    if (ObjectMaterial.UseDiffuseMap == 1){
        return (texture(ObjectMaterial.DiffuseMap, FS_IN_OBJ.UVPos).rgb * ObjectMaterial.Ambient);
    }else{
        return (ObjectMaterial.Diffuse * ObjectMaterial.Ambient);
    }
}

vec3 GetObjectDiffuse(){

    if (ObjectMaterial.UseDiffuseMap == 1){
        return (texture(ObjectMaterial.DiffuseMap, FS_IN_OBJ.UVPos).rgb * ObjectMaterial.Diffuse);
    }else{
        return ObjectMaterial.Diffuse;
    }
}

vec3 GetObjectNormalPos(){
    vec3 NormalPos = vec3(0,0,0);
    if (ObjectMaterial.UseNormalMap == 1){
        NormalPos = normalize(vec3(texture(ObjectMaterial.NormalMap, FS_IN_OBJ.UVPos)));
        NormalPos = NormalPos * 2.0 - 1.0;
        return NormalPos;
    }
    return normalize(FS_IN_OBJ.NormalPos);
}

vec3 GetObjectSpecular(){
    if (ObjectMaterial.IsPBR == 1){
        return (texture(ObjectMaterial.MetalnessMap, FS_IN_OBJ.UVPos).rgb * ObjectMaterial.Metalness);
    }
    return (texture(ObjectMaterial.SpecularMap, FS_IN_OBJ.UVPos).rgb * ObjectMaterial.Specular);
}

float GetObjectShininess(){
    if (ObjectMaterial.IsPBR == 1){
        return 128.0 * (1.0 - (texture(ObjectMaterial.RoughnessMap, FS_IN_OBJ.UVPos).r * ObjectMaterial.Roughness));
    }
    return ObjectMaterial.Shininess;
}

float GetObjectDisplacement(vec2 UVPos){
    return (texture(ObjectMaterial.DisplacementMap, FS_IN_OBJ.UVPos).r);
}

float GetObjectAlpha(){
    return (texture(ObjectMaterial.DiffuseMap, FS_IN_OBJ.UVPos).a * ObjectMaterial.Alpha * texture(ObjectMaterial.AlphaMap, FS_IN_OBJ.UVPos).r);
}

vec3 GetObjectEmissive(){
    return (texture(ObjectMaterial.EmissiveMap, FS_IN_OBJ.UVPos).rgb);
}


vec3 CalculateDirLight(Light LightObject, vec3 NormalPos, vec3 FragPos, vec3 ViewDir, vec2 UVPos)
{
    // directional light only has direction
    vec3 LightDir = normalize(-LightObject.Direction);

    // calculate diffuse
    float DiffusePower = max(dot(NormalPos, LightDir), 0.0);

    // calculate specular
    vec3 HalfwayDir = normalize(LightDir + ViewDir);
    float SpecularPower = pow(max(dot(NormalPos, HalfwayDir), 0.00001), GetObjectShininess());

    // calculate components
    vec3 AmbientValue = LightObject.Ambient * GetObjectAmbient();
    vec3 DiffuseValue = LightObject.Diffuse * DiffusePower * GetObjectDiffuse();
    vec3 SpecularValue  = LightObject.Specular * SpecularPower * GetObjectSpecular();

    return (AmbientValue + DiffuseValue + SpecularValue);
}

vec3 CalculatePointLight(Light LightObject, vec3 NormalPos, vec3 FragPos, vec3 ViewDir, vec2 UVPos)
{
    // point light only has position, get direction from light position to vertex
    vec3 LightDir = normalize(LightObject.Position - FragPos);

    // calculate diffuse
    float DiffusePower = max(dot(NormalPos, LightDir), 0.0);

    // calculate specular
    vec3 HalfwayDir = normalize(LightDir + ViewDir);
    float SpecularPower = pow(max(dot(NormalPos, HalfwayDir), 0.00001), GetObjectShininess());

    // calculate light dropoff (attenuation)
    float Distance = length(LightObject.Position - FragPos);
    float Attenuation = 1.0 / (Distance * Distance);

    // calculate components
    vec3 AmbientValue = LightObject.Ambient * GetObjectAmbient();
    vec3 DiffuseValue = LightObject.Diffuse * DiffusePower * GetObjectDiffuse();
    vec3 SpecularValue = LightObject.Specular * SpecularPower * GetObjectSpecular();

    // apply light dropoff
    AmbientValue *= Attenuation;
    DiffuseValue *= Attenuation;
    SpecularValue *= Attenuation;

    return (AmbientValue + DiffuseValue + SpecularValue);
}

vec3 CalculateSpotlight(Light LightObject, vec3 NormalPos, vec3 FragPos, vec3 ViewDir, vec2 UVPos)
{
    // spot light has both position and direction, get current direction value and compare if its in range with spot light direction
    vec3 LightDir = normalize(LightObject.Position - FragPos);


    // calculate diffuse
    float DiffusePower = max(dot(NormalPos, LightDir), 0.0);

    // calculate specular
    vec3 HalfwayDir = normalize(LightDir + ViewDir);
    float SpecularPower = pow(max(dot(NormalPos, HalfwayDir), 0.00001), GetObjectShininess());

    // calculate light dropoff (attenuation)
    float Distance = length(LightObject.Position - FragPos);
    float Attenuation = 1.0 / (Distance * Distance);

    // check current direction target and apply intensity
    float Theta = dot(LightDir, normalize(-LightObject.Direction));
    float Epsilon = LightObject.InnerCutOff - LightObject.OuterCutOff;
    float Intensity = clamp((Theta - LightObject.OuterCutOff) / Epsilon, 0.0, 1.0);

    // calculate components
    vec3 AmbientValue = LightObject.Ambient * GetObjectAmbient();
    vec3 DiffuseValue = LightObject.Diffuse * DiffusePower * GetObjectDiffuse();
    vec3 SpecularValue = LightObject.Specular * SpecularPower * GetObjectSpecular();

    // apply light dropoff
    AmbientValue *= Attenuation;
    DiffuseValue *= Attenuation;
    SpecularValue *= Attenuation;

    return (AmbientValue + DiffuseValue + SpecularValue);
}

vec2 ParallaxMapping(vec2 UVPos, vec3 ViewDir)
{   
    // determining number of layers
    const float MinLayers = 64.0;
    const float MaxLayers = 128.0;
    float NumLayers = mix(MaxLayers, MinLayers, max(dot(vec3(0.0, 0.0, 1.0), ViewDir), 0.0));
    
    // calculate depth for each layer
    float LayerDepth = 1.0 / NumLayers;
    float CurrentLayerDepth = 0.0;

    // apply displacement until max value is reached
    vec2 P = ViewDir.xy * ObjectMaterial.DispScale; 
    vec2 DeltaUVPos = P / NumLayers;    
    vec2 CurrentUVPos = UVPos;
    float CurrentDepthMapValue = GetObjectDisplacement(CurrentUVPos);
  
    while(CurrentLayerDepth < CurrentDepthMapValue)
    {
        CurrentUVPos -= DeltaUVPos;
        CurrentDepthMapValue = GetObjectDisplacement(CurrentUVPos);  
        CurrentLayerDepth += LayerDepth;  
    }

    vec2 PreviousUVPos = CurrentUVPos + DeltaUVPos;

    float AfterDepth  = CurrentDepthMapValue - CurrentLayerDepth;
    float BeforeDepth = GetObjectDisplacement(PreviousUVPos) - CurrentLayerDepth + LayerDepth;
 
    float Weight = AfterDepth / (AfterDepth - BeforeDepth);
    vec2 DisplacedUVPos = PreviousUVPos * Weight + CurrentUVPos * (1.0 - Weight);
    
    return DisplacedUVPos;  
} 

void main()
{   
    // main renderer
    if(GetObjectAlpha() < 0.1)
        discard;
    // get current coordinates
    vec3 ViewDir = normalize(FS_IN_OBJ.TangentViewPos - FS_IN_OBJ.TangentFragPos);
    vec2 UVPos = ParallaxMapping(FS_IN_OBJ.UVPos,  ViewDir);
    vec3 NormalPos = GetObjectNormalPos();

    vec3 ResultColor = vec3(0, 0, 0);

    if(GlobalLighting == 0){
        for (int i = 0; i < LightCount; i++)
        {
            if (LightArray[i].Type == 1) 
            {
                ResultColor += CalculateDirLight(LightArray[i], NormalPos, FS_IN_OBJ.FragPos, ViewDir, UVPos);
            }
            else if (LightArray[i].Type == 2) 
            {
                ResultColor += CalculatePointLight(LightArray[i], NormalPos, FS_IN_OBJ.FragPos, ViewDir, UVPos);
            }
            else if (LightArray[i].Type == 3)
            {
                ResultColor += CalculateSpotlight(LightArray[i], NormalPos, FS_IN_OBJ.FragPos, ViewDir, UVPos);
            }
        }
    } else {
        ResultColor = GetObjectDiffuse();
    }
    FragColor = vec4(ResultColor + GetObjectEmissive(), GetObjectAlpha());

}
