using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleGravity : MonoBehaviour
{
    // Total strength multiplier (matches rbGravity by default)
    [SerializeField] private float gravityMagnitude = 60f;
    
    // A custom curve field for the particle gravity over its lifetime
    [SerializeField] private ParticleSystem.MinMaxCurve customGravityModifier;
    
    private ParticleSystem _particleSystem;
    private ParticleSystem.Particle[] _particles;
    private ParticleSystem.MainModule _mainModule;

    private int layerMask = 1 << 9;

    void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _mainModule = _particleSystem.main;
        
        // IMPORTANT: Ensure the native gravity modifier is 0 so Unity doesn't apply it.
        _mainModule.gravityModifier = 0;
    }

    void LateUpdate()
    {
        int maxParticles = _mainModule.maxParticles;
        if (_particles == null || _particles.Length < maxParticles)
        {
            _particles = new ParticleSystem.Particle[maxParticles];
        }

        int numParticlesAlive = _particleSystem.GetParticles(_particles);
        float deltaTime = Time.deltaTime;

        // Check if Simulation Space is Local or World
        bool isLocal = _mainModule.simulationSpace == ParticleSystemSimulationSpace.Local;
        
        for (int i = 0; i < numParticlesAlive; i++)
        {
            // 1. Get world position for the particle
            Vector3 worldPos = isLocal ? transform.TransformPoint(_particles[i].position) : _particles[i].position;
            
            // 2. Fetch gravity direction from custom system
            Vector3 gravityDir = GetGravityAtPosition(worldPos);
            
            // 3. Calculate curve-based multiplier (0.0 to 1.0)
            float ageFraction = 1.0f - (_particles[i].remainingLifetime / _particles[i].startLifetime);
            float modifier = customGravityModifier.Evaluate(ageFraction);

            // 4. Apply force: v = v + g * dt
            Vector3 acceleration = gravityDir * gravityMagnitude * modifier;
            Vector3 deltaV = acceleration * deltaTime;

            // 5. Convert back if simulation is local
            if (isLocal)
            {
                deltaV = transform.InverseTransformDirection(deltaV);
            }
            _particles[i].velocity += deltaV;
        }

        _particleSystem.SetParticles(_particles, numParticlesAlive);
    }

    private Vector3 GetGravityAtPosition(Vector3 worldPos)
    {
        return GravityField.GetGravityAtPosition(worldPos);
    }
}
