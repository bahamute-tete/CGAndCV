using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField, Min(0f)]
    float   maxXSpeed = 20f,
            maxStartXSpeed=2f,
            startXSpeed=8f,
            constantYSpeed = 10f,
            extents = 0.5f;//ballsize
    Vector2 position, velocity;

    public float Extents => extents;
    public Vector2 Position => position;
    public Vector2 Velocity => velocity;

    [SerializeField]
    ParticleSystem bounceParticleSystem;
    [SerializeField]
    int bouncePartileEmission = 20;

    void EmitBounceParticle(float x, float z, float rotation)
    {
        //调整发射体形状的位置和旋转角度
        ParticleSystem.ShapeModule shape = bounceParticleSystem.shape;
        shape.position = new Vector3(x, 0f, z);
        shape.rotation = new Vector3(0f, rotation, 0f);
        bounceParticleSystem.Emit(bouncePartileEmission);
    }

    public void UpdateVisualization()
    {
        transform.localPosition = new Vector3(position.x, 0f, position.y);
    }

    public void Move()
    {
        position += velocity * Time.deltaTime;
    }

    public void StartNewGame()
    {
        position = Vector2.zero;
        UpdateVisualization();

        ////玩家在下面，所以开始让小球往下移动
        //velocity = new Vector2(startXSpeed, -constantYSpeed);

        velocity.x = Random.Range(-maxStartXSpeed, maxStartXSpeed);
        velocity.y = -constantYSpeed;
        gameObject.SetActive(true);
    }


    public void SetXPositionAndSpeed(float start, float speedFactor, float deltaTime)
    {
        velocity.x = maxXSpeed * speedFactor;
        position.x = start + velocity.x * deltaTime;
    }

    public void BounceX(float boundary)
    {
        float durationAfterBounce = (position.x - boundary) / velocity.x;
        //假设反弹的后的位置距离中心点位置为x,离开边界距离为b
        //有方程 x+b= boundary x+2b= postionX
        //联系求解 2x+2b=2boudanry x+2b= posX 
        position.x = 2f * boundary - position.x;
        velocity.x = -velocity.x;


        EmitBounceParticle(boundary,//碰撞在边界上
                            position.y - velocity.y * durationAfterBounce,
                            boundary < 0f ? 90f : 270f);
    }

    public void BounceY(float boundary)
    {
        float durationAfterBounce = (position.y - boundary) / velocity.y;
        position.y = 2f * boundary - position.y;
        velocity.y = -velocity.y;

        EmitBounceParticle(boundary,
                            position.x - velocity.x * durationAfterBounce,
                            boundary < 0f ? 0f : 180f);
    }

    public void EndGame()
    {
        position.x = 0f;
        gameObject.SetActive(false);
    }

    private void Awake()
    {
        gameObject.SetActive(false);
    }

}
