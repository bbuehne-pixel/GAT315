using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public BoolData simulate;
    public FloatData gravity;
    public FloatData fixedFPS;
    public StringData fpsText;
    public FloatData gravitation;
    public BoolData collision;
    public BoolData wrap;

    Vector2 size;

    static World instance;
    static public World Instance { get { return instance; } }

    public Vector2 Gravity { get { return new Vector2(0, gravity.value); } }
    public List<Body> bodies { get; set; } = new List<Body>();
    public List<Spring> springs { get; set; } = new List<Spring>();

    float timeAccumulator = 0;
    float fixedDeltaTime { get { return 1.0f / fixedFPS.value; } }
    float fps = 0;
    float fpsAverage = 0;
    float smoothing = 0.975f;

    private void Awake()
    {
        instance = this;
        size = Camera.main.ViewportToWorldPoint(Vector2.one);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        fps = 1.0f / dt;
        fpsAverage = (fpsAverage * smoothing) + (fps * (1.0f - smoothing));
        fpsText.value = "FPS: " + fpsAverage.ToString("F1");

        if (!simulate.value) return;

        GravitationalForce.ApplyForce(bodies, gravitation.value);
        springs.ForEach(spring => spring.ApplyForce());

        timeAccumulator += dt;
        while(timeAccumulator > fixedDeltaTime)
        {
            bodies.ForEach(body => body.Step(fixedDeltaTime));
            bodies.ForEach(body => Integrator.ExplicitEuler(body, fixedDeltaTime));

            bodies.ForEach(body => body.shape.color = Color.white);

            if(collision == true)
            {
                Collision.CreateContacts(bodies, out List<Contact> contacts);
                contacts.ForEach(contact => { contact.bodyA.shape.color = Color.red; contact.bodyB.shape.color = Color.red; });
                ContactSolver.Resolve(contacts);
            }

            timeAccumulator = timeAccumulator - fixedDeltaTime;
        }

        if (wrap) 
        {
            bodies.ForEach(body => body.position = Utilities.Wrap(body.position, -size, size)); 
        }

        bodies.ForEach(body => body.Step(dt));
        bodies.ForEach(body => Integrator.SemiImplicitEuler(body, dt));

        bodies.ForEach(body => body.force = Vector2.zero);
        bodies.ForEach(body => body.acceleration = Vector2.zero);
    }
}
