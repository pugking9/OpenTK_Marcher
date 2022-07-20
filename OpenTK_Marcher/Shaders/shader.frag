#version 460 core

const int _maxSpheres = 64;

out vec4 fragColor;
in vec2 texCoord;

//uniform vec2 mouse;
uniform vec3 cam_pos;
uniform float time;
uniform float ratio;
uniform int sphereCount;
uniform float[_maxSpheres] sphereSizes;
uniform float[_maxSpheres * 3] spherePositions;
uniform float[_maxSpheres * 3] sphereColors;
float curDistance;
vec3 lightPos = vec3(sin(time) * 20,0,cos(time) * 20 + 24);

// Calculate distance from given sphere with ray origin, sphere origin, and sphere radius.
float distance_from_sphere(vec3 cam, vec3 pos, float rad)
{
    return length(cam - pos) - rad;
}

int get_closest_sphere(vec3 cam)
{
    int curIndex;
    curDistance = 1000;
    float tempDistance;

    for(int i = 0; i < sphereCount; i++)
    {
        tempDistance = distance_from_sphere(cam, vec3(spherePositions[i * 3], 
            spherePositions[i * 3 + 1], spherePositions[i * 3 + 2]), sphereSizes[i]);
        if(curDistance > tempDistance)
        {
            curDistance = tempDistance;
            curIndex = i;
        }
    }

    tempDistance = distance_from_sphere(cam, lightPos, 1);
    if(curDistance > tempDistance)
    {
        curDistance = tempDistance;
        curIndex = -1;
    }
    return curIndex;
}

vec3 calculate_normal(vec3 p, vec3 s, float size)
{
    const vec3 small_step = vec3(0.001, 0.0, 0.0);

    float gradient_x = distance_from_sphere(p + small_step.xyy, s, size) - distance_from_sphere(p - small_step.xyy, s, size);
    float gradient_y = distance_from_sphere(p + small_step.yxy, s, size) - distance_from_sphere(p - small_step.yxy, s, size);
    float gradient_z = distance_from_sphere(p + small_step.yyx, s, size) - distance_from_sphere(p - small_step.yyx, s, size);

    vec3 normal = vec3(gradient_x, gradient_y, gradient_z);

    return normalize(normal);
}
float calculateLambert(vec3 p, vec3 s, float size)
{
    vec3 lightDirection = normalize(lightPos - p);
    vec3 sphereNormal = normalize(p - s);
    return max(0.0f, dot(sphereNormal, lightDirection));
}
vec3 ray_march(vec3 ro, vec3 rd)
{
    float total_distance_traveled = 0.0;
    const int NUMBER_OF_STEPS = 128;
    const float MINIMUM_HIT_DISTANCE = 0.01;
    const float MAXIMUM_TRACE_DISTANCE = 1000.0;
    int closest;
    vec3 current_position;

    for (int i = 0; i < NUMBER_OF_STEPS; ++i)
    {
        float distance_to_closest;
        current_position = ro + total_distance_traveled * rd;

        closest = get_closest_sphere(current_position);
        vec3 spherePos = vec3(spherePositions[closest * 3], spherePositions[closest * 3 + 1], spherePositions[closest * 3 + 2]);
        if(closest < 0)
            distance_to_closest = distance_from_sphere(current_position, lightPos, 1);
        else
            distance_to_closest = distance_from_sphere(current_position, spherePos, sphereSizes[closest]);

        if (distance_to_closest < MINIMUM_HIT_DISTANCE)
        {
            if(closest < 0)
                return vec3(1);
            float dist = distance_from_sphere(ro, spherePos, sphereSizes[closest]);
            vec3 hitPos = ro + dist * rd;
            float lambert = calculateLambert(hitPos, spherePos, sphereSizes[closest]);
            return vec3(sphereColors[closest * 3], sphereColors[closest * 3 + 1], sphereColors[closest * 3 + 2]) * lambert;
        }

        if (total_distance_traveled > MAXIMUM_TRACE_DISTANCE)
        {
            break;
        }

        total_distance_traveled += distance_to_closest;
    }
    //return vec3(sin(texCoord.x + time),cos(texCoord.y + time),tan((texCoord.x + texCoord.y)/2));
    return vec3(0.05);
}

void main(void)
{
    vec2 uv = texCoord * 2.0 - 1;
    uv = vec2(uv.x * ratio, uv.y);
    vec3 rd = normalize(vec3(uv, 1));

    fragColor = vec4(ray_march(cam_pos, rd),1.0);
}