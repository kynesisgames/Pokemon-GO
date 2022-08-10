using DG.Tweening;
using PokemonGO.Code;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PokeBallVisual : MonoBehaviour
{
    [SerializeField] private PokeBall _pokeBall;
    
    [Header("Rotation")]
    [SerializeField] private ParticleSystem _rotationVFX;
    [SerializeField] private Transform _rotationBlur;
    [SerializeField] private float _rotationMultiplier;
    
    [Header("Trail")]
    [SerializeField] private TrailRenderer _trail;
    [SerializeField] private float _trailTime = 0.5f;

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (!Selection.Contains(gameObject))
            return;
#endif
        if (_pokeBall == null)
            _pokeBall = GetComponentInParent<PokeBall>();
    }

    private void Start()
    {
        _rotationVFX.transform.SetParent(null);
        _rotationVFX.Stop();
        
        _rotationBlur.SetParent(null);
        _rotationBlur.gameObject.SetActive(false);

        _trail.time = 0;
        _trail.gameObject.SetActive(false);
    }

    private void Update()
    {
        _rotationVFX.transform .position = _pokeBall.transform.position;
        _rotationBlur.position = _pokeBall.transform.position;
        _rotationBlur.Rotate(_pokeBall.AngularVelocity * _rotationMultiplier);
    }

    private void OnEnable()
    {
        _pokeBall.OnCharged += OnPokeBallCharged;
        _pokeBall.OnDischarged += OnPokeBallDischarged;
        _pokeBall.OnThrown += OnPokeBallThrown;
        _pokeBall.OnCollision += OnPokeBallCollision;
    }

    private void OnDisable()
    {
        _pokeBall.OnCharged -= OnPokeBallCharged;
        _pokeBall.OnDischarged -= OnPokeBallDischarged;
        _pokeBall.OnThrown -= OnPokeBallThrown;
        _pokeBall.OnCollision -= OnPokeBallCollision;
    }

    private void StopTrail()
    {
        DOVirtual.Float(_trail.time, 0, 0.2f, DecreaseTrailTime);
        void DecreaseTrailTime(float value) => _trail.time = value;
    }

    private void OnPokeBallCharged()
    {
        _rotationVFX.Play();
        _rotationBlur.gameObject.SetActive(true);
    }
    
    private void OnPokeBallDischarged()
    {
        _rotationVFX.Stop();
        
        _rotationBlur.gameObject.SetActive(false);
        _rotationBlur.DOKill();
    }
    
    private void OnPokeBallThrown()
    {
        _rotationBlur.gameObject.SetActive(false);
        _rotationBlur.DOKill();
        _trail.gameObject.SetActive(true);
        _trail.time = _trailTime;
    }
    
    private void OnPokeBallCollision(Collision collision)
    {
        _rotationVFX.Stop();
        StopTrail();
    }
}
