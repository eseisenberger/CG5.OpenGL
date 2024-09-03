#version 330 core

out vec4 FragColor;

in vec2 TexCoord;
in vec3 Normal;
in vec3 FragPos;

uniform vec3 objectColor;
uniform vec3 lightColor;
uniform vec3 lightPosition;
uniform vec3 viewPosition;
uniform int mode;
uniform float ambientStrength;
uniform float specularStrength;
uniform int specularCoefficient;

uniform sampler2D texture1;

void main() {
    if(mode == 0)
    {
        FragColor = texture(texture1, TexCoord);
        return;
    }
    else
    {
        /* diffuse */
        vec3 norm = normalize(Normal);
        vec3 lightDir = normalize(lightPosition - FragPos);

        float diff = max(dot(norm, lightDir), 0.0);
        vec3 diffuse = diff * lightColor;

        /* specular */
        vec3 viewDir = normalize(viewPosition - FragPos);
        vec3 reflectDir = reflect(-lightDir, norm);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), specularCoefficient);
        vec3 specular = specularStrength * spec * lightColor;

        /* ambient */
        vec3 ambient = ambientStrength * lightColor;

        vec3 color;
        
        if(mode == 1)
        {
            color = objectColor;
        }
        else
        {
            color = texture(texture1, TexCoord).xyz;    
        }
        
        vec3 result = (ambient + diffuse + specular) * color;

        FragColor = vec4(result, 1.0f);
    }
    
}